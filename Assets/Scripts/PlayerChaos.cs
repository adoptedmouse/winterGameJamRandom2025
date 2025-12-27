using System.Collections;
using UnityEngine;

// handles chaos effects that affect the player's movement and physics
// attach to the player
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerChaos : MonoBehaviour
{
    [Header("Effect Durations")]
    [Tooltip("how long each effect type lasts")]
    public float speedEffectDuration = 6f;
    public float jumpEffectDuration = 5f;
    public float gravityEffectDuration = 8f;
    public float dragEffectDuration = 5f;
    
    [Header("Effect Multipliers")]
    [Tooltip("how strong each effect is")]
    public float speedBoostMultiplier = 1.8f;
    public float speedReductionMultiplier = 0.5f;
    public float jumpBoostMultiplier = 1.6f;
    public float jumpReductionMultiplier = 0.6f;
    public float lowGravityMultiplier = 0.4f;
    public float highGravityMultiplier = 2.0f;
    public float icyDragMultiplier = 0.3f;
    
    // component references
    private PlayerMovement playerMovement;
    private Rigidbody rb;
    
    // original values to restore after effects
    private float originalMoveSpeed;
    private float originalJumpForce;
    private float originalGravity;
    private float originalGroundDrag;
    
    // tracking active effects
    private bool hasActiveEffect = false;
    private Coroutine currentEffectCoroutine;
    
    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody>();
        
        // store original values
        originalMoveSpeed = playerMovement.moveSpeed;
        originalJumpForce = playerMovement.jumpForce;
        originalGravity = Physics.gravity.y;
        originalGroundDrag = playerMovement.groundDrag;
    }
    
    // apply a random chaos effect to the player
    // called by ChaosManager
    public void TriggerRandomEffect()
    {
        // if already has an effect active, don't stack them
        if (hasActiveEffect)
        {
            Debug.Log("player chaos: effect already active, skipping");
            return;
        }
        
        // pick a random effect
        int randomEffect = Random.Range(0, 7);
        
        switch (randomEffect)
        {
            case 0:
                currentEffectCoroutine = StartCoroutine(SpeedBoostEffect());
                break;
            case 1:
                currentEffectCoroutine = StartCoroutine(SpeedReductionEffect());
                break;
            case 2:
                currentEffectCoroutine = StartCoroutine(SuperJumpEffect());
                break;
            case 3:
                currentEffectCoroutine = StartCoroutine(WeakJumpEffect());
                break;
            case 4:
                currentEffectCoroutine = StartCoroutine(LowGravityEffect());
                break;
            case 5:
                currentEffectCoroutine = StartCoroutine(HighGravityEffect());
                break;
            case 6:
                currentEffectCoroutine = StartCoroutine(IcyMovementEffect());
                break;
        }
    }
    
    // ===== SPEED EFFECTS =====
    
    IEnumerator SpeedBoostEffect()
    {
        hasActiveEffect = true;
        playerMovement.moveSpeed = originalMoveSpeed * speedBoostMultiplier;
        
        // notify HUD
        GameHUD hud = FindObjectOfType<GameHUD>();
        if (hud != null) hud.ShowChaosEffect("SPEED BOOST!");
        
        Debug.Log("player chaos: speed boost active");
        yield return new WaitForSeconds(speedEffectDuration);
        
        playerMovement.moveSpeed = originalMoveSpeed;
        hasActiveEffect = false;
        Debug.Log("player chaos: speed boost ended");
    }
    
    IEnumerator SpeedReductionEffect()
    {
        hasActiveEffect = true;
        playerMovement.moveSpeed = originalMoveSpeed * speedReductionMultiplier;
        
        GameHUD hud = FindObjectOfType<GameHUD>();
        if (hud != null) hud.ShowChaosEffect("SLOW MOTION!");
        
        Debug.Log("player chaos: speed reduction active");
        yield return new WaitForSeconds(speedEffectDuration);
        
        playerMovement.moveSpeed = originalMoveSpeed;
        hasActiveEffect = false;
        Debug.Log("player chaos: speed reduction ended");
    }
    
    // ===== JUMP EFFECTS =====
    
    IEnumerator SuperJumpEffect()
    {
        hasActiveEffect = true;
        playerMovement.jumpForce = originalJumpForce * jumpBoostMultiplier;
        
        GameHUD hud = FindObjectOfType<GameHUD>();
        if (hud != null) hud.ShowChaosEffect("SUPER JUMP!");
        
        Debug.Log("player chaos: super jump active");
        yield return new WaitForSeconds(jumpEffectDuration);
        
        playerMovement.jumpForce = originalJumpForce;
        hasActiveEffect = false;
        Debug.Log("player chaos: super jump ended");
    }
    
    IEnumerator WeakJumpEffect()
    {
        hasActiveEffect = true;
        playerMovement.jumpForce = originalJumpForce * jumpReductionMultiplier;
        
        GameHUD hud = FindObjectOfType<GameHUD>();
        if (hud != null) hud.ShowChaosEffect("WEAK JUMP!");
        
        Debug.Log("player chaos: weak jump active");
        yield return new WaitForSeconds(jumpEffectDuration);
        
        playerMovement.jumpForce = originalJumpForce;
        hasActiveEffect = false;
        Debug.Log("player chaos: weak jump ended");
    }
    
    // ===== GRAVITY EFFECTS =====
    
    IEnumerator LowGravityEffect()
    {
        hasActiveEffect = true;
        Physics.gravity = new Vector3(0, originalGravity * lowGravityMultiplier, 0);
        
        GameHUD hud = FindObjectOfType<GameHUD>();
        if (hud != null) hud.ShowChaosEffect("MOON GRAVITY!");
        
        Debug.Log("player chaos: low gravity active");
        yield return new WaitForSeconds(gravityEffectDuration);
        
        Physics.gravity = new Vector3(0, originalGravity, 0);
        hasActiveEffect = false;
        Debug.Log("player chaos: low gravity ended");
    }
    
    IEnumerator HighGravityEffect()
    {
        hasActiveEffect = true;
        Physics.gravity = new Vector3(0, originalGravity * highGravityMultiplier, 0);
        
        GameHUD hud = FindObjectOfType<GameHUD>();
        if (hud != null) hud.ShowChaosEffect("HEAVY GRAVITY!");
        
        Debug.Log("player chaos: high gravity active");
        yield return new WaitForSeconds(gravityEffectDuration);
        
        Physics.gravity = new Vector3(0, originalGravity, 0);
        hasActiveEffect = false;
        Debug.Log("player chaos: high gravity ended");
    }
    
    // ===== DRAG EFFECTS =====
    
    IEnumerator IcyMovementEffect()
    {
        hasActiveEffect = true;
        playerMovement.groundDrag = originalGroundDrag * icyDragMultiplier;
        
        GameHUD hud = FindObjectOfType<GameHUD>();
        if (hud != null) hud.ShowChaosEffect("ICY CONTROLS!");
        
        Debug.Log("player chaos: icy movement active");
        yield return new WaitForSeconds(dragEffectDuration);
        
        playerMovement.groundDrag = originalGroundDrag;
        hasActiveEffect = false;
        Debug.Log("player chaos: icy movement ended");
    }
    
    // check if player currently has an active chaos effect
    public bool HasActiveEffect()
    {
        return hasActiveEffect;
    }
    
    // force end current effect (if needed for a transition/respawn)
    public void ForceEndEffect()
    {
        if (currentEffectCoroutine != null)
        {
            StopCoroutine(currentEffectCoroutine);
            currentEffectCoroutine = null;
        }
        
        // restore all values
        playerMovement.moveSpeed = originalMoveSpeed;
        playerMovement.jumpForce = originalJumpForce;
        playerMovement.groundDrag = originalGroundDrag;
        Physics.gravity = new Vector3(0, originalGravity, 0);
        
        hasActiveEffect = false;
    }
}
