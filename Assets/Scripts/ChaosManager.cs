using System.Collections;
using UnityEngine;

public class ChaosManager : MonoBehaviour
{
    private PhysicsMaterial currentMaterial;
    private float nextChaosTime;
    private float chaosEndTime;

    [Header("Chaos Settings")] // FIXED: Added missing closing bracket ]
    // Drag the "CHAOS" object here from the inspector
    public Collider floorCollider;

    // Drag the materials to be used for randomization here from the inspector
    public PhysicsMaterial[] randomMaterials;

    [Header("Timing Settings")]
    [Tooltip("Frequency of chaos events in seconds")]
    public float chaosFrequency = 5f; // Frequency of chaos events in seconds
    public float chaosDuration = 2f;  // Duration of each chaos event in seconds

    private PhysicsMaterial originalMaterial; // Variable to store the normal floor material

    void Start()
    {
        nextChaosTime = Time.time + chaosFrequency;
        // 1. Safety Check: Make sure we have a collider
        if (floorCollider == null)
        {
            Debug.LogError("ChaosManager: No Floor Collider assigned!");
            return;
        }

        // 2. Save the original material so we can restore it later
        originalMaterial = floorCollider.sharedMaterial;

        // 3. Start the chaos loop
        StartCoroutine(ChaosLoop());
    }

    IEnumerator ChaosLoop()
    {
        while (true)
        {
            // Wait for the chaos frequency
            yield return new WaitForSeconds(chaosFrequency);

            // Safety Check: Ensure we have materials to pick from
            if (randomMaterials.Length > 0)
            {
                // Apply a random material to the floor
                PhysicsMaterial selectedMaterial = randomMaterials[Random.Range(0, randomMaterials.Length)];
                floorCollider.sharedMaterial = selectedMaterial;

                // Debug log the current material applied to the floor
                Debug.Log("Chaos material applied: " + selectedMaterial.name);
            }

            // Wait for the chaos duration
            yield return new WaitForSeconds(chaosDuration);

            // Reset the floor material to its ORIGINAL state (instead of null)
            floorCollider.sharedMaterial = originalMaterial;
            Debug.Log("Chaos ended. Material restored.");
        }
    }

    void Update()
{
    if (ChaosDebugHUD.Instance == null) return;

    bool chaosActive = currentMaterial != null;
    float t = Time.time;

    if (!chaosActive)
    {
        float remaining = Mathf.Max(0f, nextChaosTime - t);
        ChaosDebugHUD.Instance.SetLine($"Chaos: OFF\nNext in: {remaining:0.0}s\nMaterial: {originalMaterial?.name ?? "None"}");
    }
    else
    {
        float remaining = Mathf.Max(0f, chaosEndTime - t);
        ChaosDebugHUD.Instance.SetLine($"Chaos: ON\nEnds in: {remaining:0.0}s\nMaterial: {currentMaterial.name}");
    }
}

}