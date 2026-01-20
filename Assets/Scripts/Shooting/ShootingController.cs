using System;
using System.Collections;
using UnityEngine;

public class ShootingController : MonoBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private float weaponRange = 20f;
    [SerializeField] private int weaponDamage = 1;
    [SerializeField] private LayerMask shootableLayers;     // Слои, в которые можно стрелять

    [Header("Visual Effects")]
    [SerializeField] private LineRenderer gunLine;          // Для отрисовки луча
    [SerializeField] private float lineDisplayTime = 0.1f;  // Время видимости луча

    [Header("Reference Settings")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private PlayerInputs playerInputs;
    [SerializeField] private Camera mainCamera;

    [Header("Debug Settings")]
    [SerializeField] private bool isViewDebug = false;

    // Для стрельбы
    private Vector2 _lookDirection;
    private float _nextFireTime; // Таймер для скорострельности
    private bool _isShootWasInputed = false;
    private Coroutine _displayShotCoroutine;

    private void OnEnable()
    {
        playerInputs.OnShootInput += HandleShootInput;
    }

    private void OnDisable()
    {
        playerInputs.OnShootInput -= HandleShootInput;
    }

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        // Инициализация LineRenderer
        if (gunLine != null)
        {
            gunLine.positionCount = 2;
            gunLine.enabled = false;
        }
    }

    private void Update()
    {
        DetermineLookDirection();

        if (_isShootWasInputed && Time.time >= _nextFireTime)
        {
            _nextFireTime = Time.time + fireRate;
            Shoot();
            _isShootWasInputed = false;
        }
    }

    private void HandleShootInput()
    {
        _isShootWasInputed = true;
    }

    private void DetermineLookDirection()
    {
        // Направление взгляда (к курсору мыши)
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        _lookDirection = (mouseWorldPos - transform.position).normalized;
    }

    private void Shoot()
    {
        // todo: при прицеливаннии учитывать разброс
        // todo: учитывать правильную колизию со стенами и тригеррами при стрельбе
        // Сейчас пули могут неверно работать с тригерамми (зоной атаки игрока врагом) и стенами 

        if (firePoint == null)
        {
            Debug.LogError("FirePoint не назначен!");
            return;
        }

        // Вычисляем конечную точку луча
        Vector2 firePointPosition = firePoint.position;
        Vector2 targetPoint = firePointPosition + _lookDirection * weaponRange;

        // Пускаем луч
        RaycastHit2D hit = Physics2D.Raycast(
            firePointPosition,
            _lookDirection,
            weaponRange,
            shootableLayers
        );

        // Обработка попадания
        if (hit.collider != null)
        {
            targetPoint = hit.point;

            // Наносим урон, если у объекта есть соответствующий компонент
            IDamageable damageable = hit.collider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(weaponDamage);
            }

            if (isViewDebug)
                Debug.Log($"Попадание в: {hit.collider.name}");
        }

        // Визуализация выстрела (можно создавать отдельный эффект для каждого выстрела например через пул)
        if (_displayShotCoroutine != null)
            StopCoroutine(_displayShotCoroutine);
        _displayShotCoroutine = StartCoroutine(DisplayShotCoroutine(firePointPosition, targetPoint));

        // Здесь можно добавить звук выстрела
        // AudioSource.PlayClipAtPoint(shotSound, firePoint.position);
    }

    private IEnumerator DisplayShotCoroutine(Vector2 startPos, Vector2 endPos)
    {
        if (gunLine != null)
        {
            gunLine.SetPosition(0, startPos);
            gunLine.SetPosition(1, endPos);
            gunLine.enabled = true;

            yield return new WaitForSeconds(lineDisplayTime);

            gunLine.enabled = false;
        }
    }

    // Вспомогательный метод для отладки (визуализация в редакторе)
    void OnDrawGizmosSelected()
    {
        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Vector2 direction = _lookDirection;
            if (Application.isPlaying == false)
            {
                direction = Vector2.right; // Направление по умолчанию в редакторе
            }
            Gizmos.DrawLine(firePoint.position, (Vector2)firePoint.position + direction * weaponRange);
        }
    }
}
