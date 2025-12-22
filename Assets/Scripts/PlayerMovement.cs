using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float jumpForce = 7f;
    public float airControlMultiplier = 0.5f;

    Rigidbody rb;
    PlayerInput input;
    PlayerGrounded grounded;
    Transform cam;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        input = GetComponent<PlayerInput>();
        grounded = GetComponent<PlayerGrounded>();
        cam = Camera.main.transform;
    }

    void FixedUpdate()
    {
        Vector3 moveDir = GetMoveDirection();
        Vector3 targetVelocity = moveDir * moveSpeed;

        Vector3 velocity = rb.linearVelocity;
        Vector3 velocityChange = targetVelocity - new Vector3(velocity.x, 0, velocity.z);

        float control = grounded.IsGrounded ? 1f : airControlMultiplier;
        rb.AddForce(velocityChange * control, ForceMode.VelocityChange);

        if (input.JumpPressed && grounded.IsGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            input.ConsumeJump();
        }
    }

    Vector3 GetMoveDirection()
    {
        Vector3 forward = cam.forward;
        Vector3 right = cam.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        return (forward * input.Move.y + right * input.Move.x).normalized;
    }
}
