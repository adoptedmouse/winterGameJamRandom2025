using System.Collections;
using UnityEngine;

// makes an object chaotic - add this to platforms, obstacles, walls, etc.
// each object independently chooses a random effect when triggered

[RequireComponent(typeof(Collider))]
public class ChaosObject : MonoBehaviour
{
    [Header("Allowed Effects")]
    [Tooltip("check which effects this object can use")]
    public bool allowMaterialChange = true;
    public bool allowScaleChange = true;
    public bool allowQuickSpin = true;
    public bool allowFlip180 = true;
    public bool allowColorChange = true;
    public bool allowDisappear = true;
    
    [Header("Effect Settings")]
    public float effectDuration = 5f;
    public PhysicsMaterial[] chaosMaterials; // assign physics materials in inspector
    
    [Header("Scale Settings")]
    public float scaleMin = 0.7f;
    public float scaleMax = 1.4f;
    
    [Header("Rotation Settings")]
    public float spinDuration = 1f; // how long the spin takes
    public float flipDuration = 0.5f; // how long the flip takes
    
    [Header("Disappear Settings")]
    public float disappearWarningTime = 1f; // blink time before disappearing
    
    [Header("Visual Settings")]
    public Color chaosColorTint = new Color(1f, 0.5f, 0f, 1f); // orange tint
    public float glowIntensity = 1.5f;
    
    // component references
    private Collider objectCollider;
    private Renderer objectRenderer;
    private MaterialPropertyBlock propBlock;
    
    // original values
    private PhysicsMaterial originalMaterial;
    private Vector3 originalScale;
    private Quaternion originalRotation;
    private Color originalColor;
    private bool hasEmission;
    
    // tracking active effects
    private bool hasActiveEffect = false;
    private Coroutine currentEffectCoroutine;
    
    void Awake()
    {
        objectCollider = GetComponent<Collider>();
        objectRenderer = GetComponent<Renderer>();
        propBlock = new MaterialPropertyBlock();
        
        // store originals
        originalMaterial = objectCollider.sharedMaterial;
        originalScale = transform.localScale;
        originalRotation = transform.rotation;
        
        if (objectRenderer != null)
        {
            objectRenderer.GetPropertyBlock(propBlock);
            if (propBlock.isEmpty)
            {
                originalColor = objectRenderer.sharedMaterial.color;
            }
            else
            {
                originalColor = propBlock.GetColor("_Color");
            }
            
            hasEmission = objectRenderer.sharedMaterial.HasProperty("_EmissionColor");
        }
    }
    
    // trigger a random chaos effect on this object
    // called by ChaosManager
    public void TriggerRandomEffect()
    {
        // if already has an effect, don't stack
        if (hasActiveEffect)
        {
            return;
        }
        
        // build list of available effects
        System.Collections.Generic.List<int> availableEffects = new System.Collections.Generic.List<int>();
        
        if (allowMaterialChange && chaosMaterials != null && chaosMaterials.Length > 0)
            availableEffects.Add(0);
        if (allowScaleChange)
            availableEffects.Add(1);
        if (allowQuickSpin)
            availableEffects.Add(2);
        if (allowFlip180)
            availableEffects.Add(3);
        if (allowColorChange && objectRenderer != null)
            availableEffects.Add(4);
        if (allowDisappear)
            availableEffects.Add(5);
        
        // if no effects enabled, bail out
        if (availableEffects.Count == 0)
        {
            Debug.LogWarning($"chaos object {gameObject.name}: no effects enabled");
            return;
        }
        
        // pick random effect from available ones
        int effectIndex = availableEffects[Random.Range(0, availableEffects.Count)];
        
        switch (effectIndex)
        {
            case 0:
                currentEffectCoroutine = StartCoroutine(MaterialChangeEffect());
                break;
            case 1:
                currentEffectCoroutine = StartCoroutine(ScaleChangeEffect());
                break;
            case 2:
                currentEffectCoroutine = StartCoroutine(QuickSpinEffect());
                break;
            case 3:
                currentEffectCoroutine = StartCoroutine(Flip180Effect());
                break;
            case 4:
                currentEffectCoroutine = StartCoroutine(ColorChangeEffect());
                break;
            case 5:
                currentEffectCoroutine = StartCoroutine(DisappearEffect());
                break;
        }
    }
    
    // ===== MATERIAL CHANGE EFFECT =====
    
    IEnumerator MaterialChangeEffect()
    {
        hasActiveEffect = true;
        
        // pick random material
        PhysicsMaterial randomMat = chaosMaterials[Random.Range(0, chaosMaterials.Length)];
        objectCollider.sharedMaterial = randomMat;
        
        // visual feedback - add glow
        if (objectRenderer != null)
        {
            ApplyGlow(true);
        }
        
        Debug.Log($"chaos object {gameObject.name}: material changed to {randomMat.name}");
        yield return new WaitForSeconds(effectDuration);
        
        // restore
        objectCollider.sharedMaterial = originalMaterial;
        if (objectRenderer != null)
        {
            ApplyGlow(false);
        }
        
        hasActiveEffect = false;
        Debug.Log($"chaos object {gameObject.name}: material restored");
    }
    
    // ===== SCALE CHANGE EFFECT =====
    
    IEnumerator ScaleChangeEffect()
    {
        hasActiveEffect = true;
        
        // random scale multiplier
        float scaleMultiplier = Random.Range(scaleMin, scaleMax);
        Vector3 targetScale = originalScale * scaleMultiplier;
        
        // smooth scale transition
        float elapsed = 0f;
        float transitionTime = 0.5f;
        
        while (elapsed < transitionTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionTime;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }
        
        transform.localScale = targetScale;
        
        // visual feedback
        if (objectRenderer != null)
        {
            ApplyGlow(true);
        }
        
        Debug.Log($"chaos object {gameObject.name}: scaled to {scaleMultiplier:F2}x");
        
        // wait for duration
        yield return new WaitForSeconds(effectDuration - transitionTime * 2);
        
        // smooth scale back
        elapsed = 0f;
        while (elapsed < transitionTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionTime;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }
        
        transform.localScale = originalScale;
        
        if (objectRenderer != null)
        {
            ApplyGlow(false);
        }
        
        hasActiveEffect = false;
        Debug.Log($"chaos object {gameObject.name}: scale restored");
    }
    
    // ===== QUICK SPIN EFFECT (360 degree spin) =====
    
    IEnumerator QuickSpinEffect()
    {
        hasActiveEffect = true;
        
        // choose random axis (mostly Y for platforms)
        Vector3 spinAxis = Random.value > 0.7f ? Vector3.right : Vector3.up;
        if (Random.value > 0.85f) spinAxis = Vector3.forward;
        
        Quaternion startRot = transform.rotation;
        Quaternion targetRot = startRot * Quaternion.AngleAxis(360f, spinAxis);
        
        // visual feedback
        if (objectRenderer != null)
        {
            ApplyGlow(true);
        }
        
        Debug.Log($"chaos object {gameObject.name}: quick spin on {spinAxis}");
        
        // spin animation
        float elapsed = 0f;
        while (elapsed < spinDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / spinDuration;
            // ease in-out for smooth spin
            float smoothT = t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
            transform.rotation = Quaternion.Slerp(startRot, targetRot, smoothT);
            yield return null;
        }
        
        transform.rotation = startRot; // return to original rotation
        
        if (objectRenderer != null)
        {
            ApplyGlow(false);
        }
        
        hasActiveEffect = false;
        Debug.Log($"chaos object {gameObject.name}: spin complete");
    }
    
    // ===== FLIP 180 EFFECT =====
    
    IEnumerator Flip180Effect()
    {
        hasActiveEffect = true;
        
        // choose axis to flip on (usually forward or right for platforms)
        Vector3 flipAxis = Random.value > 0.5f ? Vector3.forward : Vector3.right;
        
        Quaternion startRot = transform.rotation;
        Quaternion targetRot = startRot * Quaternion.AngleAxis(180f, flipAxis);
        
        // visual feedback
        if (objectRenderer != null)
        {
            ApplyGlow(true);
        }
        
        Debug.Log($"chaos object {gameObject.name}: flipping 180 on {flipAxis}");
        
        // flip animation
        float elapsed = 0f;
        while (elapsed < flipDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flipDuration;
            // ease in-out
            float smoothT = t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
            transform.rotation = Quaternion.Slerp(startRot, targetRot, smoothT);
            yield return null;
        }
        
        transform.rotation = targetRot;
        
        // wait while flipped
        yield return new WaitForSeconds(effectDuration - flipDuration * 2);
        
        // flip back
        elapsed = 0f;
        Quaternion flippedRot = transform.rotation;
        while (elapsed < flipDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flipDuration;
            float smoothT = t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
            transform.rotation = Quaternion.Slerp(flippedRot, startRot, smoothT);
            yield return null;
        }
        
        transform.rotation = startRot;
        
        if (objectRenderer != null)
        {
            ApplyGlow(false);
        }
        
        hasActiveEffect = false;
        Debug.Log($"chaos object {gameObject.name}: flip complete");
    }
    
    // ===== COLOR CHANGE EFFECT =====
    
    IEnumerator ColorChangeEffect()
    {
        hasActiveEffect = true;
        
        // apply chaos color tint
        objectRenderer.GetPropertyBlock(propBlock);
        propBlock.SetColor("_Color", chaosColorTint);
        objectRenderer.SetPropertyBlock(propBlock);
        
        // add glow
        ApplyGlow(true);
        
        Debug.Log($"chaos object {gameObject.name}: color changed");
        yield return new WaitForSeconds(effectDuration);
        
        // restore original color
        propBlock.SetColor("_Color", originalColor);
        objectRenderer.SetPropertyBlock(propBlock);
        ApplyGlow(false);
        
        hasActiveEffect = false;
        Debug.Log($"chaos object {gameObject.name}: color restored");
    }
    
    // ===== DISAPPEAR EFFECT (blinks then disappears temporarily) =====
    
    IEnumerator DisappearEffect()
    {
        hasActiveEffect = true;
        
        Debug.Log($"chaos object {gameObject.name}: disappearing soon");
        
        // blink warning
        float blinkInterval = 0.2f;
        float warningElapsed = 0f;
        bool isVisible = true;
        
        while (warningElapsed < disappearWarningTime)
        {
            warningElapsed += blinkInterval;
            isVisible = !isVisible;
            objectRenderer.enabled = isVisible;
            objectCollider.enabled = isVisible;
            yield return new WaitForSeconds(blinkInterval);
        }
        
        // disappear completely
        objectRenderer.enabled = false;
        objectCollider.enabled = false;
        
        Debug.Log($"chaos object {gameObject.name}: disappeared");
        
        // stay disappeared
        yield return new WaitForSeconds(effectDuration - disappearWarningTime);
        
        // reappear with blink
        warningElapsed = 0f;
        isVisible = false;
        while (warningElapsed < disappearWarningTime)
        {
            warningElapsed += blinkInterval;
            isVisible = !isVisible;
            objectRenderer.enabled = isVisible;
            objectCollider.enabled = isVisible;
            yield return new WaitForSeconds(blinkInterval);
        }
        
        // ensure fully visible
        objectRenderer.enabled = true;
        objectCollider.enabled = true;
        
        hasActiveEffect = false;
        Debug.Log($"chaos object {gameObject.name}: reappeared");
    }
    
    // ===== VISUAL FEEDBACK HELPERS =====
    
    void ApplyGlow(bool enable)
    {
        if (!hasEmission) return;
        
        objectRenderer.GetPropertyBlock(propBlock);
        
        if (enable)
        {
            // enable emission with chaos color
            Color emissionColor = chaosColorTint * glowIntensity;
            propBlock.SetColor("_EmissionColor", emissionColor);
        }
        else
        {
            // disable emission
            propBlock.SetColor("_EmissionColor", Color.black);
        }
        
        objectRenderer.SetPropertyBlock(propBlock);
    }
    
    // check if this object has an active effect
    public bool HasActiveEffect()
    {
        return hasActiveEffect;
    }
    
    // force end current effect (for cleanup/respawn)
    public void ForceEndEffect()
    {
        if (currentEffectCoroutine != null)
        {
            StopCoroutine(currentEffectCoroutine);
            currentEffectCoroutine = null;
        }
        
        // restore everything
        objectCollider.sharedMaterial = originalMaterial;
        transform.localScale = originalScale;
        transform.rotation = originalRotation;
        
        if (objectRenderer != null)
        {
            objectRenderer.enabled = true;
            objectRenderer.GetPropertyBlock(propBlock);
            propBlock.SetColor("_Color", originalColor);
            propBlock.SetColor("_EmissionColor", Color.black);
            objectRenderer.SetPropertyBlock(propBlock);
        }
        
        if (objectCollider != null)
        {
            objectCollider.enabled = true;
        }
        
        hasActiveEffect = false;
    }
}
