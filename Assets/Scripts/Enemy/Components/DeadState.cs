using UnityEngine;

public class DeadState : MonoBehaviour, IEnemyState
{
    public void EnterState(HardEnemyController enemy, Animator animator = null)
    {
        if (animator != null)
        {
            // Сбросить все параметры перед смертью
            animator.ResetTrigger("TakeDamage");
            animator.ResetTrigger("Attack");
            animator.SetFloat("Speed", 0f);

            animator.SetTrigger("Dead");
        }
        else
            Debug.Log($"{enemy.name} is dead!");
    }

    public void UpdateState(HardEnemyController enemy = null)
    {
        throw new System.NotImplementedException();
    }

    public void ExitState(HardEnemyController enemy = null)
    {
        // Do nothing - dead state is final
    }
}