using System.Collections;
using UnityEngine;

/// <summary>
/// Все состояния врага реализованы в разных компонентах.
/// Этот контроллер управляет переходами между состояниями и общими параметрами.
/// </summary>
public class HardEnemyController : MonoBehaviour
{
    [Header("State Settings")]
    [SerializeField] private AttackState _attackState;
    [SerializeField] private PatrolState _patrolState;
    [SerializeField] private DeadState _deadState;
    [SerializeField] private TakeDamageState _takeDamageState;

    [Header("Animation Settings")]
    [SerializeField] private Animator animator;

    [Header("Debug views-only")]
    [SerializeField] private EStateEnemy _currentState = EStateEnemy.Startup;

    private DamageableObject _myDamageable;
    private CircleCollider2D _detectionCollider;

    private bool _isLockChangeState = false;

    private GameObject _currentTarget;
    private string _playerTag;

    public GameObject Target => _currentTarget;
    public bool IsLockChangeState { get => _isLockChangeState; set => _isLockChangeState = value; }

    private void Awake()
    {
        _myDamageable = GetComponent<DamageableObject>();
        _detectionCollider = GetComponent<CircleCollider2D>();
        _detectionCollider.radius = _attackState.AttackRange;

        _playerTag = _attackState.PlayerTag;

        ResetAllStates();
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
        if (other.CompareTag(_playerTag))
        {
            _currentTarget = other.gameObject;
            StateChanges(EStateEnemy.Attack);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(_playerTag))
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

    private void ResetAllStates()
    {
        _attackState.enabled = false;
        _patrolState.enabled = false;
        _deadState.enabled = false;
        _takeDamageState.enabled = false;
    }

    public void StateChanges(EStateEnemy newState)
    {
        if (_currentState == EStateEnemy.Dead)
            return;

        if (_isLockChangeState && newState != EStateEnemy.Dead)
            return;

        if (_currentState == newState)
            return;

        _currentState = newState;

        if (_currentState != EStateEnemy.Attack && _attackState.enabled)
        {
            _attackState.ExitState();
        }

        ResetAllStates();

        if (_currentState == EStateEnemy.Attack)
        {
            _attackState.enabled = true;
            _attackState.EnterState(this, animator != null ? animator : null);
            return;
        }

        if (_currentState == EStateEnemy.TakeDamage)
        {
            _takeDamageState.enabled = true;
            _takeDamageState.EnterState(this, animator != null ? animator : null);
            return;
        }

        if (_currentState == EStateEnemy.Patrol)
        {
            _patrolState.enabled = true;
            _patrolState.EnterState(this, animator != null ? animator : null);
            return;
        }

        if (_currentState == EStateEnemy.Dead)
        {
            _deadState.enabled = true;
            _deadState.EnterState(this, animator != null ? animator : null);
            return;
        }
    }

    public void LookAt2D(Transform source, Transform target)
    {
        Vector2 direction = target.position - source.transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        source.transform.rotation = Quaternion.Euler(0, 0, angle - 90); // -90 если спрайт смотрит вверх
    }
}
