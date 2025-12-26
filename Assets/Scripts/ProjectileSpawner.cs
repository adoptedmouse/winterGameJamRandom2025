using UnityEngine;
using System.Collections.Generic;

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

    //tracking
    private float timer;
    private List<GameObject> activeObjects = new List<GameObject>();
    void Start()

    {

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

    //Visualize Gizmo --  draws line to player in editor to see targeting
    void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
}



