using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform player;

    [Header("Follow Settings")]
    public float smoothSpeed = 0.125f;
    public Vector2 offset = new Vector2(0f, 2f);

    [Header("Boundary Settings")]
    public float minY = -5f;
    public float maxY = 10f;
    public float minX = -10f;
    public float maxX = 10f;
    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (player == null) return;

        // Вычисляем целевую позицию с учетом смещения
        Vector3 targetPosition = player.position + new Vector3(offset.x, offset.y, 0);

        // Фиксируем позицию камеры по Z (важно для 2D)
        targetPosition.z = transform.position.z;

        // Ограничиваем позицию по X и Y
        float clampedX = Mathf.Clamp(targetPosition.x, minX, maxX);
        float clampedY = Mathf.Clamp(targetPosition.y, minY, maxY);

        Vector3 clampedPosition = new Vector3(clampedX, clampedY, targetPosition.z);

        // Плавное перемещение с использованием SmoothDamp
        transform.position = Vector3.SmoothDamp(
            transform.position,
            clampedPosition,
            ref velocity,
            smoothSpeed
        );
    }

    // Визуализация границ в редакторе
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3(
            (minX + maxX) * 0.5f,
            (minY + maxY) * 0.5f,
            transform.position.z
        );
        Vector3 size = new Vector3(maxX - minX, maxY - minY, 0.1f);
        Gizmos.DrawWireCube(center, size);
    }
}