using System;
using UnityEngine;

public class EnemyHealth : Health // Asumsi class Health sudah ada
{
    [Header("Visual Effects")]
    public GameObject deathEffectPrefab;
    public float destroyDelay = 0.1f;

    [Header("Loot Settings")]
    // Variabel ini akan diisi otomatis oleh WaveSpawner khusus untuk musuh terakhir
    [HideInInspector] public GameObject itemToDrop;

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
        // 1. Spawn Death Effect
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, transform.rotation);
        }

        // 2. LOGIKA DROP ITEM KUNCI
        // Jika Spawner memberikan item ke musuh ini, jatuhkan sekarang
        if (itemToDrop != null)
        {
            Instantiate(itemToDrop, transform.position, Quaternion.identity);
            Debug.Log("Key Item Dropped!");
        }

        // 3. Destroy Object
        Destroy(gameObject, destroyDelay);
    }
}