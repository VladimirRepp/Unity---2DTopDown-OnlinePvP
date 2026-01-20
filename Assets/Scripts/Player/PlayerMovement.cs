using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Animation Settings")]
    [SerializeField] private float directionThreshold = 0.1f; // Порог для определения направления

    [Header("Reference Settings")]
    // Можно перенести эти ссылки в Awake или Start, если они всегда будут на том же объекте или у дочерних объектов
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator bodyAnimator;
    [SerializeField] private PlayerInputs playerInputs;
    [SerializeField] private SpriteRenderer bodySpriteRenderer;

    private Vector2 _movementInput;
    private bool _isMoving;
    private Vector2 _lastNonZeroMovement = Vector2.down; // По умолчанию смотрим вниз

    private void OnEnable()
    {
        playerInputs.OnMoveInput += HandleMoveInput;
    }

    private void OnDisable()
    {
        playerInputs.OnMoveInput -= HandleMoveInput;
    }

    void Update()
    {
        UpdateMovement();
        UpdateAnimation();
    }

    private void HandleMoveInput(Vector2 vector)
    {
        _movementInput = vector;

        // Сохраняем последнее ненулевое направление для анимации Idle
        if (vector.magnitude > directionThreshold)
        {
            _lastNonZeroMovement = vector.normalized;
        }
    }

    private void UpdateMovement()
    {
        Vector2 movement = _movementInput.normalized * moveSpeed;
        rb.linearVelocity = movement;

        _isMoving = movement.magnitude > 0;
    }

    private void UpdateAnimation()
    {
        if (bodyAnimator == null)
            return;

        // Определяем, движется ли персонаж
        bool isMoving = _movementInput.magnitude > directionThreshold;
        bodyAnimator.SetBool("isMoving", isMoving);

        // Если персонаж движется, используем текущее направление
        // Если стоит, используем последнее направление движения
        Vector2 directionForAnimation = isMoving ? _movementInput.normalized : _lastNonZeroMovement;

        // Определяем основное направление (вверх/вниз/в сторону)
        float absX = Mathf.Abs(directionForAnimation.x);
        float absY = Mathf.Abs(directionForAnimation.y);

        if (absX > absY)
        {
            // Движение в сторону (влево/вправо)
            bodyAnimator.SetFloat("moveX", Mathf.Sign(directionForAnimation.x));
            bodyAnimator.SetFloat("moveY", 0);

            // Отражение спрайта в зависимости от направления
            if (directionForAnimation.x < 0)
                bodySpriteRenderer.flipX = false;
            else if (directionForAnimation.x > 0)
                bodySpriteRenderer.flipX = true;
        }
        else
        {
            // Движение вверх/вниз
            bodyAnimator.SetFloat("moveX", 0);
            bodyAnimator.SetFloat("moveY", Mathf.Sign(directionForAnimation.y));
        }

        // Сохраняем последнее направление для аниматора
        bodyAnimator.SetFloat("lastMoveX", directionForAnimation.x);
        bodyAnimator.SetFloat("lastMoveY", directionForAnimation.y);
    }
}
