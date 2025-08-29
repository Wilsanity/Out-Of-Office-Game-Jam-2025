using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class NPCController : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private Transform patrolPoints;
    [SerializeField] private float minWaitTime = 2f;
    [SerializeField] private float maxWaitTime = 5f;
    [SerializeField] private bool startPatrollingOnStart = true;
    [SerializeField] private bool enableManualControl = false;
    
    [Header("Animation Settings")]
    [SerializeField] private string[] idleAnimations;
    [SerializeField] private float minAnimationDuration = 1f;
    [SerializeField] private float maxAnimationDuration = 3f;
    [SerializeField] private bool enableRandomAnimations = true;
    
    [Header("Manual Control (Optional)")]
    [SerializeField] private InputActionReference attackAction;
    [SerializeField] private InputActionReference sprintAction;
    [SerializeField] private InputActionReference pointAction;
    
    private NavMeshAgent m_Agent;
    private RaycastHit m_HitInfo = new RaycastHit();
    private Camera m_Camera;
    private Animator m_Animator;
    
    // Patrol state
    private bool isPatrolling = false;
    private bool isWaiting = false;
    private List<int> availablePointIndices = new List<int>();
    private int currentTargetIndex = -1;
    private Coroutine patrolCoroutine;

    private bool isDestroyed = false;

    void Start()
    {
        m_Agent = GetComponent<NavMeshAgent>();
        m_Camera = Camera.main;
        m_Animator = GetComponent<Animator>();
        
        // Initialize available patrol points
        RefreshAvailablePoints();
        
        // Subscribe to input events if manual control is enabled
        if (enableManualControl && attackAction != null)
        {
            attackAction.action.performed += OnAttackPerformed;
        }

        // Start patrolling if enabled
        if (startPatrollingOnStart && patrolPoints != null)
        {
            StartPatrolling();
        }
    }

    void OnEnable()
    {
        if (enableManualControl)
        {
            if (attackAction != null)
                attackAction.action.Enable();
            if (sprintAction != null)
                sprintAction.action.Enable();
            if (pointAction != null)
                pointAction.action.Enable();
        }
    }

    void OnDisable()
    {
        if (enableManualControl)
        {
            if (attackAction != null)
                attackAction.action.Disable();
            if (sprintAction != null)
                sprintAction.action.Disable();
            if (pointAction != null)
                pointAction.action.Disable();
        }
    }

    void OnDestroy()
    {
        isDestroyed = true;
        
        // Unsubscribe from input events
        if (enableManualControl && attackAction != null)
        {
            attackAction.action.performed -= OnAttackPerformed;
        }
        
        // Stop all coroutines
        if (patrolCoroutine != null)
        {
            StopCoroutine(patrolCoroutine);
            patrolCoroutine = null;
        }
    }

    #region Patrol System
    
    public void StartPatrolling()
    {
        if (isDestroyed || patrolPoints == null)
        {
            Debug.LogWarning("No patrol points assigned to NPCController!");
            return;
        }
        
        if (patrolCoroutine != null)
        {
            StopCoroutine(patrolCoroutine);
        }
        
        isPatrolling = true;
        patrolCoroutine = StartCoroutine(PatrolRoutine());
    }
    
    public void StopPatrolling()
    {
        isPatrolling = false;
        if (patrolCoroutine != null)
        {
            StopCoroutine(patrolCoroutine);
            patrolCoroutine = null;
        }
        m_Agent.SetDestination(transform.position); // Stop moving
    }
    
    private IEnumerator PatrolRoutine()
    {
        while (isPatrolling && m_Agent != null)
        {
            // Choose a random patrol point
            int targetIndex = GetRandomPatrolPoint();
            if (targetIndex == -1)
            {
                Debug.LogWarning("No valid patrol points available!");
                yield break;
            }
            
            currentTargetIndex = targetIndex;
            Vector3 targetPosition = patrolPoints.GetChild(targetIndex).position;
            
            // Move to the target position
            m_Agent.SetDestination(targetPosition);
            
            // Wait until we reach the destination
            yield return new WaitUntil(() => HasReachedDestination());
            
            // Wait for a random duration at the point with random animations
            isWaiting = true;
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            //Debug.Log($"NPC reached patrol point {targetIndex}, waiting for {waitTime:F1} seconds");
            
            // Play random animations during the wait period
            if (enableRandomAnimations && m_Animator != null)
            {
                yield return StartCoroutine(PlayRandomAnimationsDuringWait(waitTime));
            }
            else
            {
                yield return new WaitForSeconds(waitTime);
            }
            
            isWaiting = false;
            
            // Remove this point from available points temporarily
            availablePointIndices.Remove(targetIndex);
            
            // If we've visited all points, refresh the list
            if (availablePointIndices.Count == 0)
            {
                RefreshAvailablePoints();
            }
        }
    }
    
    private int GetRandomPatrolPoint()
    {
        if (availablePointIndices.Count == 0)
            return -1;
            
        int randomIndex = Random.Range(0, availablePointIndices.Count);
        return availablePointIndices[randomIndex];
    }
    
    private void RefreshAvailablePoints()
    {
        availablePointIndices.Clear();
        for (int i = 0; i < patrolPoints.childCount; i++)
        {
            availablePointIndices.Add(i);
        }
    }
    
    private bool HasReachedDestination()
    {
        if (m_Agent == null) return false;
        
        if (!m_Agent.pathPending)
        {
            if (m_Agent.remainingDistance < 0.5f)
            {
                if (!m_Agent.hasPath || m_Agent.velocity.sqrMagnitude < 0.1f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private IEnumerator PlayRandomAnimationsDuringWait(float totalWaitTime)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < totalWaitTime && isWaiting && !isDestroyed)
        {
            // Choose a random animation
            string randomAnimation = GetRandomAnimation();
            
            // Calculate how long to play this animation
            float animationDuration = Mathf.Min(
                Random.Range(minAnimationDuration, maxAnimationDuration),
                totalWaitTime - elapsedTime
            );
            
            // Play the random animation
            PlayAnimation(randomAnimation);
            
            // Wait for the animation duration or until interrupted
            yield return new WaitForSeconds(animationDuration);
            
            elapsedTime += animationDuration;
            
            // Small pause between animations
            if (elapsedTime < totalWaitTime)
            {
                yield return new WaitForSeconds(Random.Range(0.2f, 0.8f));
            }
        }
        
        // Return to idle state
        if (m_Animator != null && !isDestroyed)
        {
            m_Animator.SetBool("IsWalking", false);
            m_Animator.SetBool("IsSprinting", false);
        }
    }

    private string GetRandomAnimation()
    {
        if (idleAnimations == null || idleAnimations.Length == 0)
        {
            return "Idle"; // Fallback to default idle
        }
        
        return idleAnimations[Random.Range(0, idleAnimations.Length)];
    }

    private void PlayAnimation(string animationName)
    {
        if (isDestroyed || m_Animator == null) return;
        
        // Reset all animation parameters first
        m_Animator.SetBool("IsWalking", false);
        m_Animator.SetBool("IsSprinting", false);
        
        // Play the random animation
        // Note: This assumes your animations are set up as triggers or direct Play() calls
        // Adjust this method based on your animator controller setup
        
        // Method 1: If using triggers
        // m_Animator.SetTrigger(animationName);
        
        // Method 2: If using direct Play (uncomment if needed)
        m_Animator.CrossFade(animationName, 0.2f);
        
        //Debug.Log($"Playing random animation: {animationName}");
    }

    private void Update()
    {
        if (isDestroyed || m_Agent == null || m_Animator == null) return;
        
        // Only update movement animations if not waiting (to avoid conflicts with random animations)
        if (!isWaiting)
        {
            if (m_Agent.velocity.magnitude > 0f)
            {
                // if (m_Agent.velocity.magnitude > 2f)
                // {
                //     m_Animator.SetBool("IsWalking", false);                    
                //     m_Animator.SetBool("IsSprinting", true);
                // }
                // else
                // {
                //     m_Animator.SetBool("IsSprinting", false);
                //     m_Animator.SetBool("IsWalking", true);
                // }

                //Keeping it simple for now because of not being able to trigger right animation at thresold speed e.g. m_Agent.velocity.magnitude == 2f
                m_Animator.SetBool("IsWalking", true);
                m_Animator.SetBool("IsSprinting", false);
            }
            else
            {
                m_Animator.SetBool("IsWalking", false);
                m_Animator.SetBool("IsSprinting", false);
            }
        }
    }

    #endregion
    
    #region Manual Control (Optional)
    
    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        if (!enableManualControl || m_Agent == null || m_Camera == null) return;
        
        // Stop patrolling when manual control is used
        if (isPatrolling)
        {
            StopPatrolling();
        }
        
        // Check if sprint is not being held (same logic as original LeftShift check)
        bool isSprintHeld = sprintAction != null && sprintAction.action.IsPressed();
        
        if (!isSprintHeld)
        {
            // Get mouse position using Input System
            Vector2 mousePosition = Vector2.zero;
            if (pointAction != null)
            {
                mousePosition = pointAction.action.ReadValue<Vector2>();
            }
            else
            {
                // Fallback to Mouse.current if pointAction is not assigned
                mousePosition = Mouse.current?.position.ReadValue() ?? Vector2.zero;
            }

            var ray = m_Camera.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray.origin, ray.direction, out m_HitInfo))
            {
                m_Agent.destination = m_HitInfo.point;
            }
        }
    }
    
    #endregion
    
    #region Public Properties
    
    public bool IsPatrolling => isPatrolling;
    public bool IsWaiting => isWaiting;
    public int CurrentTargetIndex => currentTargetIndex;
    public Transform PatrolPoints => patrolPoints;
    public string[] IdleAnimations => idleAnimations;
    public bool EnableRandomAnimations => enableRandomAnimations;
    
    #endregion
}
