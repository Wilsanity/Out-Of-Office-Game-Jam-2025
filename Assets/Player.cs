using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour {
    private Rigidbody rb;

    [Header("Input Actions")]
    public InputActionReference moveAction; // expects Vector2
    public float moveSpeed = 2f;
    public float fallSpeed = 2f;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
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
    }
}
