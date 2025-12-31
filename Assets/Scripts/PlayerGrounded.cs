using UnityEngine;
using UnityEngine.Serialization;

public class PlayerGrounded : MonoBehaviour
{
    public Transform groundCheck;
    [FormerlySerializedAs("checkRadius")] 
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    
    public string currentSurface {get; private set;} = "Untagged";
    public bool IsGrounded { get; private set; }

    void Update()
    {
        if (Physics.Raycast(transform.position + (Vector3.up * 0.10f), Vector3.down, out RaycastHit hit, groundCheckRadius, groundLayer))
        {
            IsGrounded = true;
            currentSurface = hit.collider.tag;
        }
        else
        {
            IsGrounded = false;
        }
    }
}