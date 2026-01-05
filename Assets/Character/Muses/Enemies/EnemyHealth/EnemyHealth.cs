using System;
using UnityEngine;

public class EnemyHealth : Health
{
    public GameObject deathEffectPrefab;
    public float destroyDelay = 0.1f;

    public void TakeDamage(int amount)
    {
        CurrentHP -= amount;

        // Debug.Log($"{gameObject.name} took {amount} damage, HP is now {CurrentHP}");

        OnHealthChanged?.Invoke(CurrentHP, _MaxHP);

        if (CurrentHP <= 0) // Gunakan <= untuk keamanan
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        CurrentHP += amount;
        OnHealthChanged?.Invoke(CurrentHP, _MaxHP);
    }

    void Die()
    {
        // === TAMBAHAN BARU ===
        // Lapor ke WaveSpawner bahwa musuh ini mati di posisi ini
        if (WaveSpawner.instance != null)
        {
            WaveSpawner.instance.OnEnemyKilled(transform.position);
        }
        // =====================

        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, transform.rotation);
        }

        Destroy(gameObject, destroyDelay);
    }
}