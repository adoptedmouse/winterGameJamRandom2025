using UnityEngine;
using TMPro;

/// main game HUD that displays timer and will eventually show chaos effects
public class GameHUD : MonoBehaviour
{
    [Header("Timer Display")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private bool showMilliseconds = true;
    
    [Header("Future Chaos Display")]
    [SerializeField] private TextMeshProUGUI chaosEffectText;
    [SerializeField] private float chaosTextDisplayDuration = 3f;
    
    // Timer tracking
    private float gameStartTime;
    private float chaosTextTimer;
    
    void Start()
    {
        // Record when the game started
        gameStartTime = Time.time;
        
        // Hide chaos text initially (for future use)
        if (chaosEffectText != null)
        {
            chaosEffectText.gameObject.SetActive(false);
        }
        
        // Validate references
        if (timerText == null)
        {
            Debug.LogError("GameHUD: Timer Text reference is missing! Please assign it in the Inspector.");
        }
    }
    
    void Update()
    {
        UpdateTimer();
        UpdateChaosText();
    }
    
    /// updates the timer display showing elapsed time since game start
    void UpdateTimer()
    {
        if (timerText == null) return;
        
        float elapsedTime = Time.time - gameStartTime;
        
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        int milliseconds = Mathf.FloorToInt((elapsedTime * 100f) % 100f);
        
        if (showMilliseconds)
        {
            timerText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
        }
        else
        {
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
    
    /// Handles the chaos effect text display
    void UpdateChaosText()
    {
        if (chaosEffectText == null || !chaosEffectText.gameObject.activeSelf) return;
        
        chaosTextTimer -= Time.deltaTime;
        if (chaosTextTimer <= 0f)
        {
            chaosEffectText.gameObject.SetActive(false);
        }
    }
    
    // call this method when a chaos effect is triggered (for future use)
    public void ShowChaosEffect(string effectName)
    {
        if (chaosEffectText == null) return;
        
        chaosEffectText.text = effectName;
        chaosEffectText.gameObject.SetActive(true);
        chaosTextTimer = chaosTextDisplayDuration;
    }
    
    // reset the timer
    public void ResetTimer()
    {
        gameStartTime = Time.time;
    }
}
