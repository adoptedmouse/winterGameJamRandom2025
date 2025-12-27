using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float jumpForce = 7f;
    public float rotationSpeed = 10f;

    [Header("Drag")]
    public float groundDrag = 6f;
    public float airDrag = 0.5f;

    [Header("Air Control")]
    public float airAcceleration = 3f;
    public float airMaxSpeed = 2.5f;

    Rigidbody rb;
    PlayerInput input;
    PlayerGrounded grounded;
    Transform cam;
    Animator animator;

    Vector3 moveDir;
    Vector3 airMomentum;
    bool wasGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        input = GetComponent<PlayerInput>();
        grounded = GetComponent<PlayerGrounded>();
        cam = Camera.main.transform;
        animator = GetComponentInChildren<Animator>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        moveDir = GetMoveDirection();
        float animSpeed = input.Move.magnitude;
        animator.SetFloat("Speed", animSpeed, 0.1f, Time.fixedDeltaTime);
        animator.SetBool("Grounded", grounded.IsGrounded);
    }

    void FixedUpdate()
    {
        rb.linearDamping = grounded.IsGrounded ? groundDrag : airDrag;

        Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (!grounded.IsGrounded && wasGrounded)
        {
            airMomentum = horizontalVel;
        }

        if (grounded.IsGrounded)
        {
            Vector3 desiredVelocity = moveDir * moveSpeed;
            Vector3 velocityDelta = desiredVelocity - horizontalVel;

            rb.AddForce(velocityDelta, ForceMode.VelocityChange);
        }
        else 
        {

            horizontalVel = GetHorizontalVelocity();
            if (moveDir.sqrMagnitude > 0.001f && horizontalVel.magnitude < airMaxSpeed)
            {
                Vector3 airAccel = moveDir * (airAcceleration * Time.fixedDeltaTime);
                Vector3 newVel = horizontalVel + airAccel;
                newVel = Vector3.ClampMagnitude(newVel, airMaxSpeed);

                rb.linearVelocity = new Vector3(
                    newVel.x,
                    rb.linearVelocity.y,
                    newVel.z
                );
            }
        }
        
        if (moveDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            rb.MoveRotation(Quaternion.Slerp(
                rb.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            ));
        }

        if (input.JumpPressed && grounded.IsGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            input.ConsumeJump();
        }

        wasGrounded = grounded.IsGrounded;
    }

    Vector3 GetMoveDirection()
    {
        Vector3 forward = cam.forward;
        Vector3 right = cam.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        return (forward * input.Move.y + right * input.Move.x).normalized;
    }

    Vector3 GetHorizontalVelocity()
    {
        return new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
    }
}