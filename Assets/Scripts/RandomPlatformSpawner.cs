using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomPlatformSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public int numberOfPlatformsToSpawn = 20;
    public float heightDifferenceMin = 2f;
    public float heightDifferenceMax = 4f;
    public float horizontalSpread = 5f;
    
    [Header("Platform Settings")]
    public Vector3 platformScaleMin = new Vector3(2, 0.5f, 2);
    public Vector3 platformScaleMax = new Vector3(4, 1f, 4);
    public Material defaultMaterial;
    
    [Header("Chaos Settings")]
    [Tooltip("How often spawned platforms should be chaos objects")]
    public ChaosFrequency chaosFrequency = ChaosFrequency.None;
    
    [Tooltip("Chaos materials to assign to chaos objects")]
    public PhysicsMaterial[] chaosMaterials;
    
    [Tooltip("Reference to chaos manager (will find automatically if not set)")]
    public ChaosManager chaosManager;

    [Header("References")]
    public Transform startPoint; // Where to start spawning from (e.g., the island)

    private Vector3 currentSpawnPos;
    private int platformCounter = 0;

    [ContextMenu("Spawn Platforms")]
    public void SpawnPlatforms()
    {
        // Find chaos manager if not assigned
        if (chaosManager == null)
        {
            chaosManager = FindObjectOfType<ChaosManager>();
        }
        
        if (startPoint != null)
            currentSpawnPos = startPoint.position;
        else
            currentSpawnPos = transform.position;

        // Ensure we start slightly above the start point
        currentSpawnPos += Vector3.up * heightDifferenceMin;

        // Reset counter for consistent pattern
        platformCounter = 0;

        for (int i = 0; i < numberOfPlatformsToSpawn; i++)
        {
            SpawnSinglePlatform();
        }
        
        // Refresh chaos manager after spawning all platforms
        if (chaosManager != null && chaosFrequency != ChaosFrequency.None)
        {
            chaosManager.RefreshChaosObjects();
        }
    }

    public void SpawnSinglePlatform()
    {
        // Calculate new position
        // Random angle
        float angle = Random.Range(0f, 360f);
        // Random distance from center (spiral effect or just random walk)
        // Let's do random walk relative to last position, but keep it within some bounds if needed.
        // For "Only Up", usually it winds up.
        
        Vector3 randomOffset = Random.insideUnitCircle * horizontalSpread;
        Vector3 nextPos = currentSpawnPos + new Vector3(randomOffset.x, 0, randomOffset.y);
        
        // Add height
        float height = Random.Range(heightDifferenceMin, heightDifferenceMax);
        nextPos.y += height; // Absolute height increase

        // Create Platform
        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platform.name = "Platform_" + Random.Range(1000, 9999);
        platform.transform.position = new Vector3(nextPos.x, nextPos.y, nextPos.z);
        platform.layer = LayerMask.NameToLayer("Ground");
        
        // Random Scale
        Vector3 scale = new Vector3(
            Random.Range(platformScaleMin.x, platformScaleMax.x),
            Random.Range(platformScaleMin.y, platformScaleMax.y),
            Random.Range(platformScaleMin.z, platformScaleMax.z)
        );
        platform.transform.localScale = scale;

        // Material
        if (defaultMaterial != null)
        {
            platform.GetComponent<Renderer>().material = defaultMaterial;
        }
        else
        {
             // Assign a random color if no material
            Renderer r = platform.GetComponent<Renderer>();
            Material m = new Material(Shader.Find("Standard"));
            m.color = Random.ColorHSV();
            r.material = m;
        }

        // Increment platform counter
        platformCounter++;

        // CHAOS LOGIC - Add ChaosObject component based on frequency
        bool shouldAddChaos = ShouldSpawnAsChaos();
        
        if (shouldAddChaos)
        {
            ChaosObject chaos = platform.AddComponent<ChaosObject>();
            
            // Assign chaos materials if available
            if (chaosMaterials != null && chaosMaterials.Length > 0)
            {
                chaos.chaosMaterials = chaosMaterials;
            }
        }

        // Parent to this
        platform.transform.parent = this.transform;

        // Update currentSpawnPos for next iteration
        // BUT, we want the next platform to be reachable from this one.
        // So next spawn position base should be THIS platform's position.
        currentSpawnPos = platform.transform.position;
    }

    // Determine if this spawn should be a chaos object based on frequency setting
    bool ShouldSpawnAsChaos()
    {
        switch (chaosFrequency)
        {
            case ChaosFrequency.None:
                return false;
            
            case ChaosFrequency.All:
                return true;
            
            case ChaosFrequency.EveryOther:
                return platformCounter % 2 == 0;
            
            case ChaosFrequency.Random25:
                return Random.value < 0.25f;
            
            case ChaosFrequency.Random50:
                return Random.value < 0.5f;
            
            case ChaosFrequency.Random75:
                return Random.value < 0.75f;
            
            default:
                return false;
        }
    }

    [ContextMenu("Clear Platforms")]
    public void ClearPlatforms()
    {
        // Destroy all children
        int childCount = transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
        
        // Reset spawn pos and counter
        if (startPoint != null)
            currentSpawnPos = startPoint.position;
        else
            currentSpawnPos = transform.position;
            
        platformCounter = 0;
        
        // Refresh chaos manager after clearing
        if (chaosManager != null)
        {
            chaosManager.RefreshChaosObjects();
        }
    }
}
