using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Фасад для работы с системой ввода.
/// </summary>
public class PlayerInputs : MonoBehaviour
{
    private InputSystem_Actions _inputs;

    public Action<Vector2> OnMoveInput;
    public Action OnShootInput;

    private void OnEnable()
    {
        _inputs = new();
        _inputs.Enable();

        _inputs.Player.Move.performed += HandleMovePerformed;
        _inputs.Player.Move.canceled += HandleMoveCanceled;

        _inputs.Player.Shoot.performed += HandleShootPerformed;
    }

    private void OnDisable()
    {
        _inputs.Player.Move.performed -= HandleMovePerformed;
        _inputs.Player.Move.canceled -= HandleMoveCanceled;

        _inputs.Player.Shoot.performed -= HandleShootPerformed;

        _inputs.Disable();
    }

    private void HandleMovePerformed(InputAction.CallbackContext context)
    {
        OnMoveInput?.Invoke(context.ReadValue<Vector2>());
    }

    private void HandleMoveCanceled(InputAction.CallbackContext context)
    {
        OnMoveInput?.Invoke(Vector2.zero);
    }

    private void HandleShootPerformed(InputAction.CallbackContext context)
    {
        OnShootInput?.Invoke();
    }
}
