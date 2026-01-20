using System;
using UnityEngine;

// Пример реализации для тестовых целей
public class DamageableObject : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private GameObject hitEffect;

    [Header("Effect Settings")]
    [SerializeField] private PerlinShaker perlinShaker;

    [Header("Destroy Settings")]
    [SerializeField] private bool isDestroyAfterDeath = false;
    [SerializeField] private float destroyDelay = 2f;

    [Header("Debug Settings")]
    [SerializeField] private bool isViewDebug = false;

    private float _currentHealth;

    public float CurrentHealth => _currentHealth;
    public float MaxHealth => maxHealth;

    public Action<float> OnHealthChanged;
    public Action OnDeath;

    private void Start()
    {
        _currentHealth = maxHealth;
    }

    public void TakeDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        _currentHealth -= damage;
        OnHealthChanged?.Invoke(_currentHealth);

        if (isViewDebug)
            Debug.Log($"{gameObject.name} получил {damage} урона. Здоровье: {_currentHealth}/{maxHealth}");

        // Спавним эффект попадания
        if (hitEffect != null)
        {
            // TODO: пул или спавн? 
            GameObject effect = Instantiate(hitEffect, hitPoint, Quaternion.LookRotation(hitNormal));
            effect.transform.SetParent(transform);
            //Destroy(effect, 2f);
        }

        if (perlinShaker != null)
        {
            perlinShaker.Shake();
        }

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    public void TakeDamage(float damage)
    {
        TakeDamage(damage, transform.position, Vector3.up);
    }

    private void Die()
    {
        if (isViewDebug)
            Debug.Log($"{gameObject.name} погиб/уничтожен!");
        OnDeath?.Invoke();

        if (isDestroyAfterDeath)
        {
            Destroy(gameObject, destroyDelay);
        }
    }
}