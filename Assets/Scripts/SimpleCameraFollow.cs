using UnityEngine;

/// <summary>
/// Simple smooth camera follow for third-person view
/// Attach this to your Main Camera
/// </summary>
public class SimpleCameraFollow : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The player or object to follow")]
    public Transform target;

    [Header("Follow Settings")]
    [Tooltip("Offset from the target position")]
    public Vector3 offset = new Vector3(0, 5, -10);
    
    [Tooltip("How smoothly the camera follows (lower = smoother, higher = more responsive)")]
    [Range(0.01f, 1f)]
    public float smoothSpeed = 0.125f;

    [Header("Rotation Settings")]
    [Tooltip("Should the camera rotate to look at the target?")]
    public bool lookAtTarget = true;
    
    [Tooltip("Point to look at relative to target (e.g., look slightly above the player)")]
    public Vector3 lookAtOffset = new Vector3(0, 1, 0);

    private void Start()
    {
        // Auto-find player if no target set
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                Debug.Log("SimpleCameraFollow: Auto-found player as target.");
            }
            else
            {
                Debug.LogWarning("SimpleCameraFollow: No target set! Assign a target in the inspector or tag your player with 'Player' tag.");
            }
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Calculate desired position
        Vector3 desiredPosition = target.position + offset;

        // Smoothly move camera towards desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // Look at target
        if (lookAtTarget)
        {
            Vector3 lookTarget = target.position + lookAtOffset;
            transform.LookAt(lookTarget);
        }
    }

    // Gizmo to visualize camera offset in editor
    private void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, target.position + lookAtOffset);
            Gizmos.DrawWireSphere(target.position + offset, 0.5f);
        }
    }
}
