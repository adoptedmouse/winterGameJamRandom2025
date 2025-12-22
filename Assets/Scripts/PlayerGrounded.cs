using UnityEngine;

public class PlayerGrounded : MonoBehaviour
{
    public Transform groundCheck;
    public float checkRadius = 0.2f;
    public LayerMask groundLayer;

    public bool IsGrounded { get; private set; }

    void FixedUpdate()
    {
        IsGrounded = Physics.CheckSphere(
            groundCheck.position,
            checkRadius,
            groundLayer
        );
    }
}