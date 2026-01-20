using System.Collections;
using UnityEngine;

/// <summary>
/// Только визуальная реализация состояния получения урона
/// Логика получения урона реализована в классе DamageableObject
/// Наносит урон тот, кто вызывает метод TakeDamage у DamageableObject
/// </summary>
public class TakeDamageState : MonoBehaviour, IEnemyState
{
    private Coroutine _waitAndChangeStateCoroutine = null;
    private HardEnemyController _controller;
    private Transform _currentTarget;

    public void EnterState(HardEnemyController enemy, Animator animator = null)
    {
        _controller = enemy;
        _currentTarget = enemy.Target?.transform;
        _controller.IsLockChangeState = true;

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
        _waitAndChangeStateCoroutine = StartCoroutine(WaitAndChangeStateCoroutine(1f,
        _currentTarget != null ? EStateEnemy.Attack : EStateEnemy.Patrol));
    }

    private IEnumerator WaitAndChangeStateCoroutine(float waitTime, EStateEnemy newState)
    {
        yield return new WaitForSeconds(waitTime);
        _controller.IsLockChangeState = false;
        _controller.StateChanges(newState);
    }

    public void ExitState(HardEnemyController enemy = null)
    {
        // Можно добавить логику выхода из состояния, если необходимо
        throw new System.NotImplementedException();
    }

    public void UpdateState(HardEnemyController enemy = null)
    {
        // Логика обновления состояния при получении урона, если необходимо
        throw new System.NotImplementedException();
    }
}

