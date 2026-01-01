using UnityEngine;

public class SimpleIslandGenerator : MonoBehaviour
{
    [Header("Island Settings")]
    public float islandRadius = 10f;
    public float islandHeight = 5f;
    public Material islandMaterial;

    [Header("Water Settings")]
    public float waterSize = 1000f;
    public Color waterColor = new Color(0f, 0.4f, 0.8f, 0.5f);
    public Material waterMaterial;

    [ContextMenu("Generate Island")]
    public void GenerateIsland()
    {
        // 1. Create Island (Cylinder)
        GameObject island = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        island.name = "StartIsland";
        island.transform.position = Vector3.zero;
        island.transform.localScale = new Vector3(islandRadius, islandHeight, islandRadius);
        
        // Move it down so the top is at y=0 roughly (Cylinder height is 2 units by default)
        // Cylinder pivot is in the center. Height 1 scale = 2 units high.
        // If scale.y is 5, height is 10 units. Center is at 0. Top is at 5.
        // We want top at slightly above water? Or water at -1.
        
        // Let's put the top surface at Y=0.
        // Height is islandHeight * 2.
        float totalHeight = islandHeight * 2;
        island.transform.position = new Vector3(0, -islandHeight, 0); // Center at -Height means top is at 0.

        if (islandMaterial != null)
            island.GetComponent<Renderer>().material = islandMaterial;

        // Add MeshCollider (Cylinder comes with CapsuleCollider usually, lets replace or keep)
        // For a platformer, MeshCollider or BoxCollider is often better for the top surface flat walkability, 
        // but Capsule is fine for a cylinder.
        // Let's actually switch to a flattened cube if they want a "platform". 
        // But they asked for an island. Cylinder is good.

        // 2. Create Water (Plane)
        GameObject water = GameObject.CreatePrimitive(PrimitiveType.Plane);
        water.name = "Water";
        water.transform.position = new Vector3(0, -2f, 0); // Water slightly below 0
        water.transform.localScale = new Vector3(waterSize / 10f, 1, waterSize / 10f); // Plane is 10x10 default
        
        Renderer waterRenderer = water.GetComponent<Renderer>();
        if (waterMaterial != null)
        {
            waterRenderer.material = waterMaterial;
        }
        else
        {
            // Create a simple material if none provided (Note: In runtime this leaks material, editor ok)
            Material tempWaterMat = new Material(Shader.Find("Universal Render Pipeline/Lit")); 
            if (tempWaterMat.shader == null) tempWaterMat = new Material(Shader.Find("Standard"));
            
            tempWaterMat.color = waterColor;
            tempWaterMat.SetFloat("_Smoothness", 0.9f);
            waterRenderer.material = tempWaterMat;
        }

        // 3. Parent them to this object to keep hierarchy clean
        island.transform.parent = this.transform;
        water.transform.parent = this.transform;

        Debug.Log("Island and Water generated!");
    }
}
