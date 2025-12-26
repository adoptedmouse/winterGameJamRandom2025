using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float jumpForce = 7f;
    public float rotationSpeed = 10f;

    Rigidbody rb;
    PlayerInput input;
    PlayerGrounded grounded;
    Transform cam;
    Animator animator;
    Vector3 moveDir;
    

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        input = GetComponent<PlayerInput>();
        grounded = GetComponent<PlayerGrounded>();
        cam = Camera.main.transform;
        animator = GetComponentInChildren<Animator>();
    }

    void FixedUpdate()
    {
        rb.linearDamping = grounded.IsGrounded ? 6f : 0f;
        moveDir = GetMoveDirection();
        
        
        if (moveDir.sqrMagnitude > 0.001f)
        {
            Vector3 desiredVelocity = moveDir * moveSpeed;
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

            Vector3 velocityDelta = desiredVelocity - horizontalVelocity;
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            Quaternion newRotation = Quaternion.Slerp(
                rb.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            );
            rb.MoveRotation(newRotation);
            rb.AddForce(velocityDelta, ForceMode.VelocityChange);

        }

        if (input.JumpPressed && grounded.IsGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            animator.SetTrigger("Jump");
            input.ConsumeJump();
        }
    }

    public void Update()
    {
        float animSpeed = input.Move.magnitude;
        animator.SetFloat("Speed", animSpeed, 0.1f, Time.fixedDeltaTime);
        animator.SetBool("Grounded", grounded.IsGrounded);
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
