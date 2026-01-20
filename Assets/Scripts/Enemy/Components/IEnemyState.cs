using UnityEngine;

public interface IEnemyState
{
    void EnterState(HardEnemyController enemy, Animator animator = null);
    void UpdateState(HardEnemyController enemy = null);   // todo: можно реализовать через события
    void ExitState(HardEnemyController enemy = null);     // todo: можно реализовать через события
}
