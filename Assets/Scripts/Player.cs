using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour {
    private Rigidbody rb;
    private Animator animator;

    [Header("Input Actions")]
    public InputActionReference moveAction; // expects Vector2
    public float moveSpeed = 2f;
    public float fallSpeed = 2f;
    
    [Header("Animation")]
    [SerializeField] private InputActionReference sprintAction; // optional, Button
    [SerializeField] private bool sprintEnabled = true; // Toggle sprint functionality
    [SerializeField, Range(0f, 1f)] private float sprintThreshold = 0.9f; // used if sprintAction is not assigned

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
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

    private void FixedUpdate() {
        Vector2 input = moveAction.action.ReadValue<Vector2>();
        Vector3 move = new Vector3(input.x, 0f, input.y);
        move = Vector3.ClampMagnitude(move, 1f);

        Vector3 cameraForward = Camera.main.transform.forward;
        Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, new Vector3(cameraForward.x, 0, cameraForward.z));

        Vector3 projectedMove = rotation * move;

        if (move != Vector3.zero) {
            transform.forward = projectedMove;
        }

        rb.linearVelocity = projectedMove * moveSpeed + Vector3.up * rb.linearVelocity.y;

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
