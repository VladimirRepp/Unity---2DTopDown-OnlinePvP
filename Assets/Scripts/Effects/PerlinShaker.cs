using UnityEngine;
using System.Collections;

public class PerlinShaker : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeIntensity = 0.2f;
    [SerializeField] private float frequency = 25f; // Частота шума

    private Vector3 originalPosition;
    private float seed;

    private void Start()
    {
        originalPosition = transform.localPosition;
        seed = Random.Range(0f, 100f);
    }

    public void Shake()
    {
        StartCoroutine(ShakeCoroutine());
    }

    private IEnumerator ShakeCoroutine()
    {
        float elapsed = 0f;
        originalPosition = transform.localPosition;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            // Используем шум Перлина для плавной тряски
            float noiseX = Mathf.PerlinNoise(seed, elapsed * frequency) * 2f - 1f;
            float noiseY = Mathf.PerlinNoise(seed + 1f, elapsed * frequency) * 2f - 1f;

            // Уменьшаем интенсивность со временем
            float damping = 1f - (elapsed / shakeDuration);

            // Применяем смещение
            Vector3 offset = new Vector3(noiseX, noiseY, 0) * shakeIntensity * damping;
            transform.localPosition = originalPosition + offset;

            yield return null;
        }

        // Возвращаем на место
        transform.localPosition = originalPosition;
    }
}