using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class NPCController : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float minWaitTime = 2f;
    [SerializeField] private float maxWaitTime = 5f;
    [SerializeField] private bool startPatrollingOnStart = true;
    [SerializeField] private bool enableManualControl = false;
    
    [Header("Manual Control (Optional)")]
    [SerializeField] private InputActionReference attackAction;
    [SerializeField] private InputActionReference sprintAction;
    [SerializeField] private InputActionReference pointAction;
    
    private NavMeshAgent m_Agent;
    private RaycastHit m_HitInfo = new RaycastHit();
    private Camera m_Camera;
    
    // Patrol state
    private bool isPatrolling = false;
    private bool isWaiting = false;
    private List<int> availablePointIndices = new List<int>();
    private int currentTargetIndex = -1;
    private Coroutine patrolCoroutine;

    void Start()
    {
        m_Agent = GetComponent<NavMeshAgent>();
        m_Camera = Camera.main;
        
        // Initialize available patrol points
        RefreshAvailablePoints();
        
        // Subscribe to input events if manual control is enabled
        if (enableManualControl && attackAction != null)
        {
            attackAction.action.performed += OnAttackPerformed;
        }
        
        // Start patrolling if enabled
        if (startPatrollingOnStart && patrolPoints.Length > 0)
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
        // Unsubscribe from input events
        if (enableManualControl && attackAction != null)
        {
            attackAction.action.performed -= OnAttackPerformed;
        }
    }

    #region Patrol System
    
    public void StartPatrolling()
    {
        if (patrolPoints.Length == 0)
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
        while (isPatrolling)
        {
            // Choose a random patrol point
            int targetIndex = GetRandomPatrolPoint();
            if (targetIndex == -1)
            {
                Debug.LogWarning("No valid patrol points available!");
                yield break;
            }
            
            currentTargetIndex = targetIndex;
            Vector3 targetPosition = patrolPoints[targetIndex].position;
            
            // Move to the target position
            m_Agent.SetDestination(targetPosition);
            
            // Wait until we reach the destination
            yield return new WaitUntil(() => HasReachedDestination());
            
            // Wait for a random duration at the point
            isWaiting = true;
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            Debug.Log($"NPC reached patrol point {targetIndex}, waiting for {waitTime:F1} seconds");
            
            yield return new WaitForSeconds(waitTime);
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
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] != null)
            {
                availablePointIndices.Add(i);
            }
        }
    }
    
    private bool HasReachedDestination()
    {
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
    
    #endregion
    
    #region Manual Control (Optional)
    
    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        if (!enableManualControl) return;
        
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
    public Transform[] PatrolPoints => patrolPoints;
    
    #endregion
}
