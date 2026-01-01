using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enhanced version of RandomPlatformSpawner with better "Only Up" mechanics
/// Features: Spiral pattern option, rest platforms, difficulty progression
/// </summary>
public class EnhancedPlatformSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public int numberOfPlatformsToSpawn = 40;
    public float heightDifferenceMin = 2f;
    public float heightDifferenceMax = 4f;
    public float horizontalSpread = 5f;
    
    [Header("Spiral Pattern (Optional)")]
    [Tooltip("Enable spiral upward pattern instead of random walk")]
    public bool useSpiralPattern = true;
    [Tooltip("How many degrees to rotate per platform")]
    public float spiralRotationPerPlatform = 30f;
    [Tooltip("How far from center (increases as you go up)")]
    public float spiralRadiusMin = 3f;
    public float spiralRadiusMax = 8f;
    
    [Header("Platform Settings")]
    public Vector3 platformScaleMin = new Vector3(2, 0.5f, 2);
    public Vector3 platformScaleMax = new Vector3(4, 1f, 4);
    public Material defaultMaterial;
    
    [Header("Rest Platforms")]
    [Tooltip("Spawn a larger, safer platform every X platforms")]
    public bool spawnRestPlatforms = true;
    public int restPlatformInterval = 10;
    public Vector3 restPlatformScale = new Vector3(6, 0.5f, 6);
    public Color restPlatformColor = Color.green;
    
    [Header("Chaos Settings")]
    public bool addChaosComponent = true;
    public PhysicsMaterial[] chaosMaterials;
    [Tooltip("Don't add chaos to rest platforms")]
    public bool restPlatformsStable = true;

    [Header("References")]
    public Transform startPoint;

    private Vector3 currentSpawnPos;
    private float currentSpiralAngle = 0f;
    private int platformCount = 0;

    [ContextMenu("Spawn Platforms")]
    public void SpawnPlatforms()
    {
        if (startPoint != null)
            currentSpawnPos = startPoint.position;
        else
            currentSpawnPos = transform.position;

        // Start slightly above the start point
        currentSpawnPos += Vector3.up * heightDifferenceMin;
        currentSpiralAngle = 0f;
        platformCount = 0;

        for (int i = 0; i < numberOfPlatformsToSpawn; i++)
        {
            platformCount = i;
            
            // Check if this should be a rest platform
            bool isRestPlatform = spawnRestPlatforms && (i + 1) % restPlatformInterval == 0;
            
            SpawnSinglePlatform(isRestPlatform);
        }
    }

    public void SpawnSinglePlatform(bool isRestPlatform = false)
    {
        Vector3 nextPos;

        if (useSpiralPattern)
        {
            // Spiral pattern - platforms circle upward
            float radius = Mathf.Lerp(spiralRadiusMin, spiralRadiusMax, platformCount / (float)numberOfPlatformsToSpawn);
            float angleRad = currentSpiralAngle * Mathf.Deg2Rad;
            
            Vector3 spiralOffset = new Vector3(
                Mathf.Cos(angleRad) * radius,
                0,
                Mathf.Sin(angleRad) * radius
            );
            
            nextPos = currentSpawnPos + spiralOffset;
            currentSpiralAngle += spiralRotationPerPlatform;
        }
        else
        {
            // Random walk pattern (original method)
            Vector3 randomOffset = Random.insideUnitCircle * horizontalSpread;
            nextPos = currentSpawnPos + new Vector3(randomOffset.x, 0, randomOffset.y);
        }
        
        // Add height
        float height = isRestPlatform ? heightDifferenceMin : Random.Range(heightDifferenceMin, heightDifferenceMax);
        nextPos.y = currentSpawnPos.y + height;

        // Create Platform
        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platform.name = isRestPlatform ? "RestPlatform_" + platformCount : "Platform_" + platformCount;
        platform.transform.position = nextPos;
        
        // Set Scale
        Vector3 scale;
        if (isRestPlatform)
        {
            scale = restPlatformScale;
        }
        else
        {
            scale = new Vector3(
                Random.Range(platformScaleMin.x, platformScaleMax.x),
                Random.Range(platformScaleMin.y, platformScaleMax.y),
                Random.Range(platformScaleMin.z, platformScaleMax.z)
            );
        }
        platform.transform.localScale = scale;

        // Material
        Renderer r = platform.GetComponent<Renderer>();
        if (isRestPlatform)
        {
            // Rest platforms get special color
            Material restMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (restMat.shader == null) restMat = new Material(Shader.Find("Standard"));
            restMat.color = restPlatformColor;
            r.material = restMat;
        }
        else if (defaultMaterial != null)
        {
            r.material = defaultMaterial;
        }
        else
        {
            // Random color
            Material m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (m.shader == null) m = new Material(Shader.Find("Standard"));
            m.color = Random.ColorHSV();
            r.material = m;
        }

        // Add ChaosObject (but not to rest platforms if stable)
        if (addChaosComponent && !(isRestPlatform && restPlatformsStable))
        {
            ChaosObject chaos = platform.AddComponent<ChaosObject>();
            if (chaosMaterials != null && chaosMaterials.Length > 0)
            {
                chaos.chaosMaterials = chaosMaterials;
            }
        }

        // Parent
        platform.transform.parent = this.transform;

        // Update position for next platform
        currentSpawnPos = platform.transform.position;
    }

    [ContextMenu("Clear Platforms")]
    public void ClearPlatforms()
    {
        int childCount = transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
        
        if (startPoint != null)
            currentSpawnPos = startPoint.position;
        else
            currentSpawnPos = transform.position;
            
        currentSpiralAngle = 0f;
        platformCount = 0;
    }

    // Visualize the spiral pattern in editor
    private void OnDrawGizmosSelected()
    {
        if (startPoint == null) return;
        if (!useSpiralPattern) return;

        Gizmos.color = Color.cyan;
        Vector3 lastPos = startPoint.position;
        float angle = 0f;

        for (int i = 0; i < Mathf.Min(numberOfPlatformsToSpawn, 50); i++)
        {
            float radius = Mathf.Lerp(spiralRadiusMin, spiralRadiusMax, i / (float)numberOfPlatformsToSpawn);
            float angleRad = angle * Mathf.Deg2Rad;
            
            Vector3 pos = lastPos + new Vector3(
                Mathf.Cos(angleRad) * radius,
                Random.Range(heightDifferenceMin, heightDifferenceMax),
                Mathf.Sin(angleRad) * radius
            );
            
            Gizmos.DrawLine(lastPos, pos);
            Gizmos.DrawWireCube(pos, Vector3.one);
            
            lastPos = pos;
            angle += spiralRotationPerPlatform;
        }
    }
}
