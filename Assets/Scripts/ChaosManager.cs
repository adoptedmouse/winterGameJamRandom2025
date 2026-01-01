using System.Collections;
using UnityEngine;

// central chaos controller that manages both player and object chaos events
// triggers them separately at different intervals for maximum chaos
public class ChaosManager : MonoBehaviour
{
    [Header("Chaos Frequency")]
    [Tooltip("how often player chaos triggers (seconds)")]
    public float playerChaosFrequency = 10f;
    [Tooltip("how often object chaos triggers (seconds)")]
    public float objectChaosFrequency = 8f;
    
    [Header("Chaos Toggles")]
    [Tooltip("enable/disable player chaos effects")]
    public bool enablePlayerChaos = true;
    [Tooltip("enable/disable object chaos effects")]
    public bool enableObjectChaos = true;
    
    [Header("References")]
    [Tooltip("drag your player GameObject here")]
    public PlayerChaos playerChaos;
    [Tooltip("HUD will be found automatically if not assigned")]
    public GameHUD gameHUD;
    
    [Header("Debug")]
    public bool debugMode = true;
    
    // cached references
    private ChaosObject[] chaosObjects;
    
    void Start()
    {
        // find GameHUD if not assigned
        if (gameHUD == null)
        {
            gameHUD = FindObjectOfType<GameHUD>();
        }
        
        // find player chaos if not assigned
        if (playerChaos == null)
        {
            playerChaos = FindObjectOfType<PlayerChaos>();
        }
        
        // find all chaos objects in scene
        RefreshChaosObjects();
        
        // start the chaos loops
        StartCoroutine(PlayerChaosLoop());
        StartCoroutine(ObjectChaosLoop());
        
        if (debugMode)
        {
            Debug.Log($"chaos manager: initialized with {chaosObjects.Length} chaos objects");
        }
    }
    
    /// find all ChaosObject components in the scene
    /// call this we spawn new objects during gameplay
    public void RefreshChaosObjects()
    {
        chaosObjects = FindObjectsOfType<ChaosObject>();
        if (debugMode)
        {
            Debug.Log($"chaos manager: found {chaosObjects.Length} chaos objects");
        }
    }
    
    // ===== PLAYER CHAOS LOOP =====
    
    IEnumerator PlayerChaosLoop()
    {
        // initial delay before first event
        yield return new WaitForSeconds(playerChaosFrequency);
        
        while (true)
        {
            // trigger player chaos only if enabled
            if (enablePlayerChaos)
            {
                if (playerChaos != null)
                {
                    playerChaos.TriggerRandomEffect();
                    
                    if (debugMode)
                    {
                        Debug.Log("chaos manager: player chaos triggered");
                    }
                }
                else
                {
                    Debug.LogWarning("chaos manager: playerChaos reference missing!");
                }
            }
            else if (debugMode)
            {
                Debug.Log("chaos manager: player chaos skipped (disabled)");
            }
            
            // wait for next player chaos event
            yield return new WaitForSeconds(playerChaosFrequency);
        }
    }
    
    // ===== OBJECT CHAOS LOOP =====
    
    IEnumerator ObjectChaosLoop()
    {
        // initial delay before first event (offset from player)
        yield return new WaitForSeconds(objectChaosFrequency * 0.5f);
        
        while (true)
        {
            // trigger chaos on all objects only if enabled
            if (enableObjectChaos)
            {
                TriggerObjectChaos();
            }
            else if (debugMode)
            {
                Debug.Log("chaos manager: object chaos skipped (disabled)");
            }
            
            // wait for next object chaos event
            yield return new WaitForSeconds(objectChaosFrequency);
        }
    }
    
    // trigger chaos on all ChaosObjects in the scene

    void TriggerObjectChaos()
    {
        if (chaosObjects == null || chaosObjects.Length == 0)
        {
            if (debugMode)
            {
                Debug.Log("chaos manager: no chaos objects found");
            }
            return;
        }
        
        int triggeredCount = 0;
        
        // tell each object to trigger (they'll pick their own effect)
        foreach (ChaosObject obj in chaosObjects)
        {
            if (obj != null)
            {
                obj.TriggerRandomEffect();
                triggeredCount++;
            }
        }
        
        // notify HUD
        if (gameHUD != null)
        {
            gameHUD.ShowChaosEffect("WORLD CHAOS!");
        }
        
        if (debugMode)
        {
            Debug.Log($"chaos manager: triggered {triggeredCount} chaos objects");
        }
    }
    
    // manually trigger player chaos
    public void ManualTriggerPlayerChaos()
    {
        if (playerChaos != null)
        {
            playerChaos.TriggerRandomEffect();
        }
    }
    
    // manually trigger object chaos
    public void ManualTriggerObjectChaos()
    {
        TriggerObjectChaos();
    }
    
    // stop all chaos effects
    public void StopAllChaos()
    {
        StopAllCoroutines();
        
        // force end player effect
        if (playerChaos != null)
        {
            playerChaos.ForceEndEffect();
        }
        
        // force end all object effects
        if (chaosObjects != null)
        {
            foreach (ChaosObject obj in chaosObjects)
            {
                if (obj != null)
                {
                    obj.ForceEndEffect();
                }
            }
        }
        
        if (debugMode)
        {
            Debug.Log("chaos manager: all chaos stopped");
        }
    }
    
    // restart chaos loops (after stopping)
    public void RestartChaos()
    {
        StopAllCoroutines();
        StartCoroutine(PlayerChaosLoop());
        StartCoroutine(ObjectChaosLoop());
        
        if (debugMode)
        {
            Debug.Log("chaos manager: chaos restarted");
        }
    }
}
