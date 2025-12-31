using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")] 
    public float moveSpeed = 6f;
    public float acceleration = 8f;
    public float iceAcceleration = 2f;
    public float airAcceleration = 5f;
    public float jumpForce = 6.5f;
    
    [Header("References")]
    public Transform cameraTransform;

    private Rigidbody rigidBody;
    private Animator animator;
    private PlayerGrounded groundCheck;
    private PlayerInput input;
    private Vector3 currentVelocity;
    private Vector2 moveInput;
    private bool isGrounded;
    private bool landingTriggered;
    private RaycastHit landingHit;
    private bool wasGrounded;
    private bool hasJumped;
    private int jumps;
    private Vector3 impactVelocity;
    public string currentSurface {set; get;}
    private float accelrationFactor;
    

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        input = GetComponent<PlayerInput>();
        groundCheck = GetComponent<PlayerGrounded>();
        
    }

    private void FixedUpdate()
    {
        moveInput = input.Move;
        isGrounded = groundCheck.IsGrounded;
        currentSurface = groundCheck.currentSurface;
        var cameraForward = cameraTransform.forward;
        var cameraRight = cameraTransform.right;
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        
        cameraForward.Normalize();
        cameraRight.Normalize();
        
        var targetDirection = cameraForward * moveInput.y + cameraRight * moveInput.x;
        var targetVelocity = targetDirection * moveSpeed;
        
        float currentAccel;
        if (!isGrounded) 
        {
            currentAccel = airAcceleration;
        } 
        else if (currentSurface == "Ice") 
        {
            currentAccel = iceAcceleration;
        } 
        else 
        {
            currentAccel = acceleration;
        }
        
        if (targetDirection.sqrMagnitude > 0.01f) 
        {
            targetDirection.y = 0f; 
            var targetRot = Quaternion.LookRotation(targetDirection, Vector3.up);
            
            rigidBody.MoveRotation(Quaternion.Slerp(rigidBody.rotation, targetRot, 10f * Time.fixedDeltaTime));
        }


        if (input.JumpPressed)
        {
            var jumpForceFactor = 1f;
            if (isGrounded && !hasJumped)
            {
                if (currentSurface == "Ice")
                {
                    jumpForceFactor = 0.8f;
                }
                rigidBody.linearVelocity = new Vector3(rigidBody.linearVelocity.x, 0f, rigidBody.linearVelocity.z);
                rigidBody.AddForce(Vector3.up * (jumpForce * jumpForceFactor), ForceMode.Impulse);
                
                animator.ResetTrigger("Jump"); 
                animator.SetTrigger("Jump");
        
                jumps++;
                hasJumped = true;
            }
            input.ConsumeJump();
        }

        if (!wasGrounded && isGrounded)
        {
            hasJumped = false;

            if (currentSurface != "Bouncy")
            { 
                animator.ResetTrigger("Jump"); 
            }
            
        }
        
        var finalVelocity = new Vector3(
            Mathf.Lerp(rigidBody.linearVelocity.x, targetVelocity.x, currentAccel * Time.fixedDeltaTime),
            rigidBody.linearVelocity.y,
            Mathf.Lerp(rigidBody.linearVelocity.z, targetVelocity.z, currentAccel * Time.fixedDeltaTime)
        );
        
        //impactVelocity = Vector3.Lerp(impactVelocity, Vector3.zero, 5f * Time.fixedDeltaTime);
        rigidBody.linearVelocity = finalVelocity;
        wasGrounded = isGrounded;
    }

    private void Update()
    {
        //bool landing = Physics.Raycast(transform.position, Vector3.down, out landingHit, groundCheck.groundCheckRadius, groundCheck.groundLayer);
        animator.SetFloat("VerticalVelocity", rigidBody.linearVelocity.y);
        animator.SetFloat("Speed", isGrounded ? input.Move.magnitude : 0f, 0.1f, Time.deltaTime);
        animator.SetBool("Grounded", isGrounded);
        
       
        
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Hazard"))
        {
            var pushDir = (transform.position - collision.contacts[0].point).normalized;
            var force = collision.relativeVelocity.magnitude;
            AddImpact(pushDir * force);
        }

        if (collision.gameObject.CompareTag("Bouncy"))
        {
            animator.SetTrigger("Jump");
        }
    }
    
    public bool IsFalling()
    {
        return !isGrounded && rigidBody.linearVelocity.y < -1f;
    }

    public int GetJumps()
    {
        return jumps;
    }
    
    public void AddImpact(Vector3 force) {
        if (force.y < 0) force.y = 0;
        impactVelocity += force;
    }
    
    
}
