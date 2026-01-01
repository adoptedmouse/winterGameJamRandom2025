using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerController))]
public class ChaosDebugHUD : MonoBehaviour
{
    // Singleton Instance
    public static ChaosDebugHUD Instance { get; private set; }

    [Header("Chaos HUD Settings")]
    public bool showDebugUI = true;
    public bool showSpawnerGizmos = false;
    public KeyCode toggleKey = KeyCode.BackQuote; // The ` key
    public Color uiBackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.7f);
    public Color textColor = new Color (1f, 0.8f, 0f); // Gold color

    // References
    private PlayerController player;
    private Rigidbody rb;
    private PlayerGrounded grounded;
    private PlayerInput input;

    // calculated values
    private string chaosLine = ""; // For chaos manager updates
    private string groundMaterialName = "Air";
    private float currentSpeed;

    void Awake()
    {
        // Set singleton instance
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        player = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody>();
        grounded = GetComponent<PlayerGrounded>();
        input = GetComponent<PlayerInput>();
    }

    void Update()
    {
        // Check for toggle key press using new Input System
        if (Keyboard.current != null && Keyboard.current.backquoteKey.wasPressedThisFrame)
        {
            showDebugUI = !showDebugUI;
        }

        if (!showDebugUI) return;

        CalculatedMaterialData();
        currentSpeed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;
    }

    void OnGUI()
    {
        if (!showDebugUI) return;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = textColor;
        style.fontSize = 14;
        style.fontStyle = FontStyle.Bold;

        //Draw the backgrounnd box
        GUILayout.BeginArea(new Rect(20, 20, 350, 300));
        GUI.backgroundColor = uiBackgroundColor;
        GUILayout.BeginVertical("Box");

        // HEADER
        GUILayout.Label($"--- CHAOS DEBUGGER ({toggleKey} ---)");

        // MOVEMENT STATES
        GUILayout.Space(5);
        GUILayout.Label($"Grounded: {grounded.IsGrounded}", style);
        GUILayout.Label($"Ground Material: {groundMaterialName}", style);

        //PHYSICS DATA
        GUILayout.Space(5);
        GUILayout.Label($"Speed: {currentSpeed:F2} units/s", style);
        GUILayout.Label($"Vertical Velocity: {rb.linearVelocity.y:F2} units/s", style);
        GUILayout.Label($"Linear Drag: {rb.linearDamping:F2}", style);

        // INPUT DATA
        GUILayout.Space(5);
        GUILayout.Label($"Input Move Vector: {input.Move}", style);
        GUILayout.Label($"Jump Pressed: {input.JumpPressed}", style);

        GUILayout.EndVertical();
        GUILayout.EndArea();

    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying || rb == null) return;

        // Visualizing the physics velocity in scene view
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + rb.linearVelocity);
        Gizmos.DrawSphere(transform.position + rb.linearVelocity, 0.1f);
    }

    // Public method for ChaosManager to update the chaos line
    public void SetLine(string line)
    {
        chaosLine = line;
    }

    void CalculatedMaterialData()
    {
        if (!grounded.IsGrounded)
        {
            groundMaterialName = "Air";
            return;
        }

        // Use the current surface from PlayerController (updated in PlayerGrounded)
        groundMaterialName = player.currentSurface;
        
        if (string.IsNullOrEmpty(groundMaterialName))
        {
            groundMaterialName = "Unknown";
        }
    }
}
