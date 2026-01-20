using UnityEngine;

/// <summary>
/// Любой объект, который можно повредить, реализует этот интерфейс.
/// </summary>
public interface IDamageable
{
    void TakeDamage(float damage, Vector3 hitPoint, Vector3 hitNormal);
    void TakeDamage(float damage);
}