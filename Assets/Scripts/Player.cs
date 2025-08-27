using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour {
    private Rigidbody rb;
    private Animator animator;

    private Interactable highlighted = null;
    private float stunDurationSeconds = 0f; // blocks all input

    [Header("Input Actions")]
    public InputActionReference moveAction; // expects Vector2
    public InputActionReference interactAction;
    public float moveSpeed = 2f;
    public float interactRadius = .75f;
    
    [Header("Animation")]
    [SerializeField] private InputActionReference sprintAction; // optional, Button
    [SerializeField] private bool sprintEnabled = true; // Toggle sprint functionality
    [SerializeField, Range(0f, 1f)] private float sprintThreshold = 0.9f; // used if sprintAction is not assigned

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        interactAction.action.started += context => Interact();
    }
    
    /// <summary>
    /// Enable or disable sprint functionality
    /// </summary>
    public void SetSprintEnabled(bool enabled) {
        sprintEnabled = enabled;
    }
    
    /// <summary>
    /// Check if sprint is currently enabled
    /// </summary>
    public bool IsSprintEnabled() {
        return sprintEnabled;
    }

    public void Stun (float durationSeconds) {
        stunDurationSeconds = Mathf.Max(stunDurationSeconds, durationSeconds);
    }

    private void Update() {
        // Highlight the nearest interactable in range
        Collider[] overlapping = Physics.OverlapSphere(transform.position, interactRadius, LayerMask.GetMask("Interactable"));
        Interactable nearest = null;
        float nearestSqDist = float.PositiveInfinity;
        foreach(Collider c in overlapping) {
            Interactable interactable = c.GetComponent<Interactable>();
            if (interactable == null) continue;

            float sqDist = (transform.position - c.transform.position).sqrMagnitude;
            if (sqDist < nearestSqDist) {
                nearest = interactable;
                nearestSqDist = sqDist;
            }
        }
        if (nearest != highlighted) {
            if (highlighted != null) {
                highlighted.SetHighlight(false);
            }
            if (nearest != null) {
                nearest.SetHighlight(true);
            }
            highlighted = nearest;
        }
    }

    private void Interact() {
        if (stunDurationSeconds > 0) {
            return;
        }
        if (highlighted != null) {
            highlighted.Interact(this);
        }
    }

    private float CalculateSpeed() {
        float currentSpeed = moveSpeed;
        foreach (Slowdown slowdown in FindObjectsByType<Slowdown>(FindObjectsSortMode.None)) {
            currentSpeed *= slowdown.GetSlowdownAtPoint(transform.position);
        }
        return currentSpeed;
    }

    private void FixedUpdate() {
        Vector2 input = moveAction.action.ReadValue<Vector2>();
        if (stunDurationSeconds > 0) {
            input = Vector3.zero;
            stunDurationSeconds -= Time.fixedDeltaTime;
        }

        Vector3 move = new Vector3(input.x, 0f, input.y);
        move = Vector3.ClampMagnitude(move, 1f);

        Vector3 cameraForward = Camera.main.transform.forward;
        Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, new Vector3(cameraForward.x, 0, cameraForward.z));

        Vector3 projectedMove = rotation * move;

        if (move != Vector3.zero) {
            transform.forward = projectedMove;
        }

        rb.linearVelocity = projectedMove * CalculateSpeed() + Vector3.up * rb.linearVelocity.y;

        // Animation driving
        float inputMagnitude = input.magnitude;                // 0..1
        bool isMoving = inputMagnitude > 0f;
        bool isSprinting = sprintEnabled && (sprintAction != null
            ? sprintAction.action.IsPressed()
            : inputMagnitude >= sprintThreshold);

        if (animator != null) {
            animator.SetBool("IsMoving", isMoving);
            animator.SetBool("IsSprinting", isSprinting && isMoving);
        }
    }
}
