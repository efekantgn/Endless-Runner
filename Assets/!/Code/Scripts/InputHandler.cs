using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputHandler : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction turnAction;
    private InputAction jumpAction;
    private InputAction slideAction;

    public Action<InputAction.CallbackContext> OnPlayerTurnPerformedAction;
    public Action<InputAction.CallbackContext> OnPlayerJumpPerformedAction;
    public Action<InputAction.CallbackContext> OnPlayerSlidePerformedAction;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        turnAction = playerInput.actions["Turn"];
        jumpAction = playerInput.actions["Jump"];
        slideAction = playerInput.actions["Slide"];
    }

    private void OnEnable()
    {
        turnAction.performed += OnPlayerTurnPerformed;
        jumpAction.performed += OnPlayerJumpPerformed;
        slideAction.performed += OnPlayerSlidePerformed;
    }
    private void OnDisable()
    {
        turnAction.performed -= OnPlayerTurnPerformed;
        jumpAction.performed -= OnPlayerJumpPerformed;
        slideAction.performed -= OnPlayerSlidePerformed;
    }

    private void OnPlayerSlidePerformed(InputAction.CallbackContext context) => OnPlayerSlidePerformedAction?.Invoke(context);
    private void OnPlayerJumpPerformed(InputAction.CallbackContext context) => OnPlayerJumpPerformedAction?.Invoke(context);
    private void OnPlayerTurnPerformed(InputAction.CallbackContext context) => OnPlayerTurnPerformedAction?.Invoke(context);

}
