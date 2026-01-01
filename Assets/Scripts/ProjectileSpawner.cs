using UnityEngine;
using System.Collections.Generic;

public enum ChaosFrequency
{
    None,           // No chaos objects
    All,            // Every spawned object is a chaos object
    EveryOther,     // Alternating pattern
    Random50,       // 50% chance
    Random25,       // 25% chance
    Random75        // 75% chance
}

public class ProjectileSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Drag the prefabs you want to spawn here")]
    public List<GameObject> objectsToSpawn;

    [Tooltip("How many seconds between spawns")]
    public float spawnInterval = 2f;

    [Tooltip("How fast the object flies at the player")]
    public float flingForce = 20f;

    [Header("Life Management")]
    [Tooltip("How long before the spawned object is destroyed")]
    public float objectLifeTime = 5f;

    [Tooltip("Maximum number of objects allowed from THIS spawner")]
    public int maxConcurrentObjects = 10;

    [Header("Targeting")]
    [Tooltip("The target the projectiles will aim for")]
    public Transform target;

    [Header("Chaos Settings")]
    [Tooltip("How often spawned objects should be chaos objects")]
    public ChaosFrequency chaosFrequency = ChaosFrequency.None;
    
    [Tooltip("Chaos materials to assign to chaos objects")]
    public PhysicsMaterial[] chaosMaterials;
    
    [Tooltip("Reference to chaos manager (will find automatically if not set)")]
    public ChaosManager chaosManager;

    //tracking
    private float timer;
    private List<GameObject> activeObjects = new List<GameObject>();
    private int spawnCounter = 0;
    void Start()
    {
        // Find chaos manager if not assigned
        if (chaosManager == null)
        {
            chaosManager = FindObjectOfType<ChaosManager>();
        }
    }

    void Update()
    {

        //list cleanup, remove objects from list that have been destroyed
        activeObjects.RemoveAll(item => item == null);

        //cooldown timer
        timer += Time.deltaTime;

        // spawn logic, if timer exceeds interval and under max objects
        if (timer >= spawnInterval && activeObjects.Count < maxConcurrentObjects)
        {
            SpawnAndFling();
            timer = 0f; //reset timer
        }
        
    }

    void SpawnAndFling()
    {
        if (objectsToSpawn.Count == 0 || target == null)
            return;

        //select random object to spawn
        int index = Random.Range(0, objectsToSpawn.Count);
        GameObject objToSpawn = objectsToSpawn[index];

        //spawn object at spawner's position and rotation
        GameObject spawnedObj = Instantiate(objToSpawn, transform.position, transform.rotation);
        activeObjects.Add(spawnedObj);

        // increment spawn counter for every other tracking
        spawnCounter++;

        // CHAOS LOGIC - Add ChaosObject component based on frequency
        bool shouldAddChaos = ShouldSpawnAsChaos();
        
        if (shouldAddChaos)
        {
            // Check if it already has ChaosObject (from prefab)
            ChaosObject chaosObj = spawnedObj.GetComponent<ChaosObject>();
            
            if (chaosObj == null)
            {
                // Add ChaosObject component if not present
                chaosObj = spawnedObj.AddComponent<ChaosObject>();
            }
            
            // Assign chaos materials if available
            if (chaosMaterials != null && chaosMaterials.Length > 0)
            {
                chaosObj.chaosMaterials = chaosMaterials;
            }
            
            // Notify chaos manager to refresh its object list
            if (chaosManager != null)
            {
                chaosManager.RefreshChaosObjects();
            }
        }

        //calculate direction to target
        Vector3 direction = (target.position - transform.position).normalized;

        //apply force to fling towards target
        Rigidbody rb = spawnedObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(direction * flingForce, ForceMode.VelocityChange);
        }

        //schedule destruction after lifetime
        Destroy(spawnedObj, objectLifeTime);
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
                return spawnCounter % 2 == 0;
            
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

    //Visualize Gizmo --  draws line to player in editor to see targeting
    void OnDrawGizmos()
    {
        // Check if debug HUD is enabled and spawner gizmos should show
        if (ChaosDebugHUD.Instance != null && ChaosDebugHUD.Instance.showSpawnerGizmos && target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, target.position);
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}
