using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputHandler : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction turnAction;
    private InputAction jumpAction;
    private InputAction slideAction;
    private InputAction primaryContactAction;
    private InputAction primaryPositionAction;

    public Action<InputAction.CallbackContext> OnPlayerTurnPerformedAction;
    public Action<InputAction.CallbackContext> OnPlayerJumpPerformedAction;
    public Action<InputAction.CallbackContext> OnPlayerSlidePerformedAction;
    public Action<Vector2> OnPlayerSwipeAction;

    private Vector2 initialPos;
    private Vector2 currentPos => primaryPositionAction.ReadValue<Vector2>();

    [SerializeField] float swipeResistance = 50f;


    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        turnAction = playerInput.actions["Turn"];
        jumpAction = playerInput.actions["Jump"];
        slideAction = playerInput.actions["Slide"];
        primaryContactAction = playerInput.actions["PrimaryContact"];
        primaryPositionAction = playerInput.actions["PrimaryPosition"];
    }

    private void OnEnable()
    {
        turnAction.performed += OnPlayerTurnPerformed;
        jumpAction.performed += OnPlayerJumpPerformed;
        slideAction.performed += OnPlayerSlidePerformed;
        primaryContactAction.performed += OnPlayerPrimaryContactPerformed;
        primaryContactAction.canceled += OnPlayerPrimaryContactCanceled;
    }


    private void OnDisable()
    {
        turnAction.performed -= OnPlayerTurnPerformed;
        jumpAction.performed -= OnPlayerJumpPerformed;
        slideAction.performed -= OnPlayerSlidePerformed;
        primaryContactAction.performed -= OnPlayerPrimaryContactPerformed;
        primaryContactAction.canceled -= OnPlayerPrimaryContactCanceled;
    }

    private void OnPlayerSlidePerformed(InputAction.CallbackContext context) => OnPlayerSlidePerformedAction?.Invoke(context);
    private void OnPlayerJumpPerformed(InputAction.CallbackContext context) => OnPlayerJumpPerformedAction?.Invoke(context);
    private void OnPlayerTurnPerformed(InputAction.CallbackContext context) => OnPlayerTurnPerformedAction?.Invoke(context);
    private void OnPlayerPrimaryContactPerformed(InputAction.CallbackContext context) => initialPos = currentPos;

    private void OnPlayerPrimaryContactCanceled(InputAction.CallbackContext context)
    {
        Vector2 delta = currentPos - initialPos;
        Vector2 direction = Vector2.zero;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            if (Mathf.Abs(delta.x) > swipeResistance)
            {
                direction.x = Mathf.Clamp(delta.x, -1, 1);
            }

        }
        else
        {
            if (Mathf.Abs(delta.y) > swipeResistance)
            {
                direction.y = Mathf.Clamp(delta.y, -1, 1);
            }
        }

        if (direction != Vector2.zero)
        {
            OnPlayerSwipeAction?.Invoke(direction);
        }
    }
}
