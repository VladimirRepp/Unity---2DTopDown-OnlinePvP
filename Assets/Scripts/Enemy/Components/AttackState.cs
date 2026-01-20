using System.Collections;
using UnityEngine;

public class AttackState : MonoBehaviour, IEnemyState
{
    [Header("Attack Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private int damage = 10;

    private Animator _animator;
    private HardEnemyController _controller;
    private Coroutine _attackCoroutine;

    public float AttackRange => attackRange;
    public string PlayerTag => playerTag;

    public void EnterState(HardEnemyController enemy, Animator animator)
    {
        _animator = animator;
        _controller = enemy;

        if (_animator != null)
            animator.SetFloat("Speed", -1f);

        if (_attackCoroutine != null)
            StopCoroutine(_attackCoroutine);

        _attackCoroutine = StartCoroutine(AttackRoutine(_controller.Target));
    }

    public void ExitState(HardEnemyController enemy = null)
    {
        if (_attackCoroutine != null)
            StopCoroutine(_attackCoroutine);
    }

    public void UpdateState(HardEnemyController enemy = null)
    {
        throw new System.NotImplementedException();
    }

    private IEnumerator AttackRoutine(GameObject target)
    {
        _controller.LookAt2D(transform, target.transform);

        while (true)
        {
            // todo: нанести урон цели
            if (target.TryGetComponent<DamageableObject>(out DamageableObject damageable))
                damageable.TakeDamage(damage);

            // Атака с передышкой
            if (_animator != null)
                _animator.SetTrigger("Attack");
            else
                Debug.Log($"{gameObject.name} - is attacking!");
            yield return new WaitForSeconds(attackCooldown);
        }
    }
}
