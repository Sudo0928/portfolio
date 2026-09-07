using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class InputManager
{
    public PlayerInputActions InputActions { get; private set; }
    public PlayerInputActions.PlayerActions PlayerActions { get; private set; }

    [field: SerializeField] public Vector2 CurrentMovementInput { get; private set; }
    [field: SerializeField] public bool CurrentOnWalkToggle { get; private set; }
    [field: SerializeField] public bool CurrentDash { get; private set; }
    [field: SerializeField] public bool CurrentSprint { get; private set; }
    [field: SerializeField] public bool CurrentJump { get; private set; }
    [field: SerializeField] public float CurrentYawInput { get; private set; }

    [field: SerializeField] public StickInputTracker MovementInput { get; private set; } = new StickInputTracker();
    [field: SerializeField] public ButtonInputTracker WalkToggleInput { get; private set; } = new ButtonInputTracker();
    [field: SerializeField] public ButtonInputTracker DashInput { get; private set; } = new ButtonInputTracker();
    [field: SerializeField] public ButtonInputTracker SprintInput { get; private set; } = new ButtonInputTracker();
    [field: SerializeField] public ButtonInputTracker JumpInput { get; private set; } = new ButtonInputTracker();
    [field: SerializeField] public AxisInputTracker YawInput { get; private set; } = new AxisInputTracker();

    public void Init()
    {
        InputActions = new PlayerInputActions();

        PlayerActions = InputActions.Player;
    }

    public void OnEnable()
    {
        InputActions.Enable();
        Debug.Log("InputActions Enabled");

        MovementInput.Enable();
        WalkToggleInput.Enable();
        DashInput.Enable();
        SprintInput.Enable();
        JumpInput.Enable();
        YawInput.Enable();

        InputActions.Player.Sprint.performed += OnSprint;
        InputActions.Player.Sprint.canceled += OnSprint;
    }

    public void OnDisable()
    {
        InputActions.Disable();

        MovementInput.Disable();
        WalkToggleInput.Disable();
        DashInput.Disable();
        SprintInput.Disable();
        JumpInput.Disable();
        YawInput.Disable();

        // InputActions.Player.Sprint.performed -= OnSprint;
        // InputActions.Player.Sprint.canceled -= OnSprint;
    }

    public void Update()
    {
        CurrentMovementInput = PlayerActions.Movement.ReadValue<Vector2>();
        CurrentOnWalkToggle = PlayerActions.WalkToggle.IsPressed();
        CurrentDash = PlayerActions.Dash.IsPressed();
        CurrentSprint = PlayerActions.Sprint.phase == InputActionPhase.Performed;
        CurrentJump = PlayerActions.Jump.IsPressed();
        CurrentYawInput = PlayerActions.Look.ReadValue<Vector2>().x;
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed)
        {
            CurrentSprint = true;
        }
        else if(context.phase == InputActionPhase.Canceled)
        {
            CurrentSprint = false;
        }
    }
}
