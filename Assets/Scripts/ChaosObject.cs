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
    public bool allowRotation = true;
    public bool allowColorChange = true;
    
    [Header("Effect Settings")]
    public float effectDuration = 5f;
    public PhysicsMaterial[] chaosMaterials; // assign physics materials in inspector
    
    [Header("Scale Settings")]
    public float scaleMin = 0.7f;
    public float scaleMax = 1.4f;
    
    [Header("Rotation Settings")]
    public float rotationSpeed = 45f; // degrees per second
    
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
        if (allowRotation)
            availableEffects.Add(2);
        if (allowColorChange && objectRenderer != null)
            availableEffects.Add(3);
        
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
                currentEffectCoroutine = StartCoroutine(RotationEffect());
                break;
            case 3:
                currentEffectCoroutine = StartCoroutine(ColorChangeEffect());
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
    
    // ===== ROTATION EFFECT =====
    
    IEnumerator RotationEffect()
    {
        hasActiveEffect = true;
        
        // random rotation direction
        Vector3 rotationAxis = Random.insideUnitSphere.normalized;
        float randomSpeed = rotationSpeed * Random.Range(0.5f, 1.5f);
        
        // visual feedback
        if (objectRenderer != null)
        {
            ApplyGlow(true);
        }
        
        Debug.Log($"chaos object {gameObject.name}: rotating at {randomSpeed:F1} deg/s");
        
        float elapsed = 0f;
        while (elapsed < effectDuration)
        {
            transform.Rotate(rotationAxis, randomSpeed * Time.deltaTime, Space.World);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (objectRenderer != null)
        {
            ApplyGlow(false);
        }
        
        hasActiveEffect = false;
        Debug.Log($"chaos object {gameObject.name}: rotation stopped");
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
        
        if (objectRenderer != null)
        {
            objectRenderer.GetPropertyBlock(propBlock);
            propBlock.SetColor("_Color", originalColor);
            propBlock.SetColor("_EmissionColor", Color.black);
            objectRenderer.SetPropertyBlock(propBlock);
        }
        
        hasActiveEffect = false;
    }
}
