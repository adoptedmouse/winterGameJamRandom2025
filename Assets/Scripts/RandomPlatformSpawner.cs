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
    public bool addChaosComponent = true;
    public PhysicsMaterial[] chaosMaterials; // Assign these to pass to ChaosObject

    [Header("References")]
    public Transform startPoint; // Where to start spawning from (e.g., the island)

    private Vector3 currentSpawnPos;

    [ContextMenu("Spawn Platforms")]
    public void SpawnPlatforms()
    {
        if (startPoint != null)
            currentSpawnPos = startPoint.position;
        else
            currentSpawnPos = transform.position;

        // Ensure we start slightly above the start point
        currentSpawnPos += Vector3.up * heightDifferenceMin;

        for (int i = 0; i < numberOfPlatformsToSpawn; i++)
        {
            SpawnSinglePlatform();
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

        // Add ChaosObject
        if (addChaosComponent)
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

    [ContextMenu("Clear Platforms")]
    public void ClearPlatforms()
    {
        // Destroy all children
        int childCount = transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
        
        // Reset spawn pos
        if (startPoint != null)
            currentSpawnPos = startPoint.position;
        else
            currentSpawnPos = transform.position;
    }
}
