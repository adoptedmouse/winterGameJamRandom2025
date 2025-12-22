using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public Vector2 Move { get; private set; }
    public bool JumpPressed { get; private set; }

    PlayerControls controls;

    void Awake()
    {
        controls = new PlayerControls();

        controls.Player.Move.performed += ctx => Move = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => Move = Vector2.zero;

        controls.Player.Jump.performed += ctx => JumpPressed = true;
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    public void ConsumeJump() => JumpPressed = false;
}