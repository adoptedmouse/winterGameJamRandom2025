using UnityEngine;

/// <summary>
/// Makes water deadly - respawns player when they touch it
/// Attach this to your Water plane and make sure it has a trigger collider
/// </summary>
public class WaterKillZone : MonoBehaviour
{
    [Header("Respawn Settings")]
    [Tooltip("Where to respawn the player (usually the starting island)")]
    public Transform respawnPoint;
    
    [Tooltip("Height offset above respawn point")]
    public float respawnHeightOffset = 2f;

    private void Start()
    {
        // Ensure this object has a collider set to trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        else
        {
            Debug.LogWarning("WaterKillZone: No collider found! Add a Box Collider and set it to Trigger.");
        }

        // If no respawn point set, try to find the starting island
        if (respawnPoint == null)
        {
            GameObject island = GameObject.Find("StartIsland");
            if (island != null)
            {
                respawnPoint = island.transform;
                Debug.Log("WaterKillZone: Auto-found StartIsland as respawn point.");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if player touched the water
        if (other.CompareTag("Player") || other.GetComponent<PlayerMovement>() != null)
        {
            RespawnPlayer(other.gameObject);
        }
    }

    private void RespawnPlayer(GameObject player)
    {
        if (respawnPoint != null)
        {
            // Respawn at the respawn point
            Vector3 respawnPosition = respawnPoint.position + Vector3.up * respawnHeightOffset;
            player.transform.position = respawnPosition;

            // Reset velocity if player has a rigidbody
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            Debug.Log("Player fell in water! Respawned at: " + respawnPosition);
        }
        else
        {
            Debug.LogWarning("WaterKillZone: No respawn point set! Assign a respawn point in the inspector.");
        }
    }
}
