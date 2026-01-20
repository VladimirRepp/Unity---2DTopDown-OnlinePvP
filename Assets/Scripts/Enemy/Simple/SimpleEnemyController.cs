using System.Collections;
using UnityEngine;

/// <summary>
/// Все состояния врага реализованы в этом классе в отдельных методах
/// </summary>
public class SimpleEnemyController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private Transform[] patrolPoints;

    [Header("Attack Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private int damage = 10;

    [Header("References Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D _rb;

    [Header("Debug views-only")]
    [SerializeField] private EStateEnemy _currentState = EStateEnemy.Startup;

    private DamageableObject _myDamageable;
    private CircleCollider2D _detectionCollider;

    private int _currentPatrolIndex = 0;
    private GameObject _currentTarget = null;
    private Coroutine _attackCoroutine = null;

    private Coroutine _waitAndChangeStateCoroutine = null;
    private bool _isLockChangeState = false;

    private void Awake()
    {
        _myDamageable = GetComponent<DamageableObject>();
        _detectionCollider = GetComponent<CircleCollider2D>();
    }

    private void OnEnable()
    {
        if (_myDamageable != null)
        {
            _myDamageable.OnDeath += HandleDeath;
            _myDamageable.OnHealthChanged += HandleHealthChanged;
        }
    }

    private void OnDisable()
    {
        if (_myDamageable != null)
        {
            _myDamageable.OnDeath -= HandleDeath;
            _myDamageable.OnHealthChanged -= HandleHealthChanged;
        }
    }

    private void Start()
    {
        StateChanges(EStateEnemy.Patrol);
        _detectionCollider.radius = attackRange;
    }

    private void Update()
    {
        if (_currentState == EStateEnemy.Patrol)
            Patrolling();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Детектирования игрока
        // I через подсчет дистанции 
        // - мало объектов
        // - разные радиусы обнаружения (не нужно создавать коллайдеры)
        // - переодическая проверка (не каждый кадр)
        // - простой расчет быстрее и меньше накладных расходов
        // II через коллайдеры 
        // - много объектов
        // - одинаковый радиус обнаружения
        // - мгновенная реакция на появление цели в зоне
        // III через рейкасты
        // - дальняя дистанция обнаружения
        // - сложная геометрия окружения
        // - высокая точность обнаружения цели
        // - требует больше ресурсов на вычисления
        // - может быть менее надежным из-за возможных помех в линии видимости
        // IV гибридный подход 
        // - комбинирование методов для оптимизации производительности и точности
        if (other.CompareTag(playerTag))
        {
            _currentTarget = other.gameObject;
            StateChanges(EStateEnemy.Attack);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            StateChanges(EStateEnemy.Patrol);
        }
    }

    private void HandleDeath()
    {
        StateChanges(EStateEnemy.Dead);
    }

    private void HandleHealthChanged(float health_value)
    {
        StateChanges(EStateEnemy.TakeDamage);
    }

    private void StateChanges(EStateEnemy newState)
    {
        if (_currentState == EStateEnemy.Dead)
            return;

        if (_isLockChangeState && newState != EStateEnemy.Dead)
            return;

        if (_currentState == newState)
            return;

        if (_currentState == EStateEnemy.Attack &&
            newState != EStateEnemy.Attack)
        {
            if (_attackCoroutine != null)
                StopCoroutine(_attackCoroutine);

            _attackCoroutine = null;
            _currentTarget = null;
        }

        _currentState = newState;

        if (_currentState == EStateEnemy.TakeDamage)
        {
            TakeDamageState();
            return;
        }

        if (_currentState == EStateEnemy.Attack)
        {
            AttackState(_currentTarget);
            return;
        }

        if (_currentState == EStateEnemy.Patrol)
        {
            PatrolState();
            // Patrolling(); - called in Update
            return;
        }

        if (_currentState == EStateEnemy.Dead)
        {
            DeadState();
            return;
        }
    }

    private void LookAt2D(Transform target)
    {
        Vector2 direction = target.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90); // -90 если спрайт смотрит вверх
    }

    #region States Implementations

    private void AttackState(GameObject target)
    {
        if (animator != null)
            animator.SetFloat("Speed", -1f);

        if (_attackCoroutine != null)
            StopCoroutine(_attackCoroutine);

        _attackCoroutine = StartCoroutine(AttackRoutine(target));
    }

    private IEnumerator AttackRoutine(GameObject target)
    {
        LookAt2D(target.transform);

        while (true)
        {
            // todo: нанести урон цели
            if (target.TryGetComponent<DamageableObject>(out DamageableObject damageable))
                damageable.TakeDamage(damage);

            // Атака с передышкой
            if (animator != null)
                animator.SetTrigger("Attack");
            else
                Debug.Log($"{gameObject.name} - is attacking!");
            yield return new WaitForSeconds(attackCooldown);
        }
    }

    private void PatrolState()
    {
        if (animator != null)
            animator.SetFloat("Speed", 1f);
    }

    private void Patrolling()
    {
        if (patrolPoints.Length == 0)
            return;

        Transform targetPoint = patrolPoints[_currentPatrolIndex];

        // Направление к целевой точке
        Vector2 direction = (targetPoint.position - transform.position).normalized;

        // Движение через Rigidbody2D (рекомендуется для 2D)
        if (_rb != null)
        {
            _rb.linearVelocity = direction * moveSpeed;
        }
        else // Если нет Rigidbody2D
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                targetPoint.position,
                moveSpeed * Time.deltaTime
            );
        }

        // Поворот спрайта/объекта в сторону движения
        // if (direction.x != 0)
        // {
        //     // Отражение спрайта по X для смены направления взгляда
        //     transform.localScale = new Vector3(
        //         Mathf.Sign(direction.x) * Mathf.Abs(transform.localScale.x),
        //         transform.localScale.y,
        //         transform.localScale.z
        //     );
        // }

        // Альтернатива: вращение объекта в сторону движения
        LookAt2D(targetPoint);
        // float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        // transform.rotation = Quaternion.Euler(0, 0, angle - 90); // -90 если спрайт смотрит вверх

        // Проверка достижения точки (используем Vector2.Distance для 2D)
        if (Vector2.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            int prevIndex = _currentPatrolIndex;
            _currentPatrolIndex = Random.Range(0, patrolPoints.Length);

            // Если случайный индекс совпал с предыдущим, берем следующий
            if (prevIndex == _currentPatrolIndex)
            {
                _currentPatrolIndex = (prevIndex + 1) % patrolPoints.Length;
            }
        }
    }

    private void DeadState()
    {
        _isLockChangeState = true;

        if (animator != null)
        {
            // Сбросить все параметры перед смертью
            animator.ResetTrigger("TakeDamage");
            animator.SetFloat("Speed", 0f);

            // Запустить смерть через триггер
            animator.SetTrigger("Die");
        }
        else
        {
            Debug.Log($"{gameObject.name} - is dead!");
        }

        this.enabled = false;
    }

    /// <summary>
    /// Только визуальная реализация состояния получения урона
    /// Логика получения урона реализована в классе DamageableObject
    /// Наносит урон тот, кто вызывает метод TakeDamage у DamageableObject
    /// </summary>
    private void TakeDamageState()
    {
        _isLockChangeState = true;

        if (animator != null)
        {
            animator.SetFloat("Speed", -1f);
            animator.SetTrigger("TakeDamage");
        }
        else
        {
            Debug.Log($"{gameObject.name} - took damage!");
        }

        if (_waitAndChangeStateCoroutine != null)
        {
            StopCoroutine(_waitAndChangeStateCoroutine);
            _waitAndChangeStateCoroutine = null;
        }

        // TODO: подправить время ожидания в зависимости от анимации урона
        _waitAndChangeStateCoroutine = StartCoroutine(WaitAndChangeStateCoroutine(1f, _currentTarget != null ? EStateEnemy.Attack : EStateEnemy.Patrol));
    }

    private IEnumerator WaitAndChangeStateCoroutine(float waitTime, EStateEnemy newState)
    {
        yield return new WaitForSeconds(waitTime);
        _isLockChangeState = false;
        StateChanges(newState);
    }
    #endregion
}
