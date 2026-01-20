using UnityEngine;

public class PatrolState : MonoBehaviour, IEnemyState
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private Transform[] patrolPoints;

    private Rigidbody2D _rb; // todo: использовать при необходимости движения через физику
    private HardEnemyController _controller;

    private bool _canPatrol = false;
    private int _currentPatrolIndex = 0;

    public void EnterState(HardEnemyController enemy, Animator animator)
    {
        _controller = enemy;
        _canPatrol = true;
        if (animator != null)
        {
            animator.SetFloat("Speed", 1f);
            animator.ResetTrigger("Attack");
        }

        // _rb = enemy.GetComponent<Rigidbody2D>(); - или получить как то иначе, если нужно
    }

    public void ExitState(HardEnemyController enemy = null)
    {
        _canPatrol = false;
    }

    public void UpdateState(HardEnemyController enemy = null)
    {
        throw new System.NotImplementedException();
    }

    private void Update()
    {
        if (_canPatrol)
            Patrolling();
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
        _controller.LookAt2D(transform, targetPoint);
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
}
