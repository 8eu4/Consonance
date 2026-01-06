using System;
using UnityEngine;

public class EnemyHealth : Health
{
    public GameObject deathEffectPrefab;
    public float destroyDelay = 0.1f;

    protected override void Die()
    {
        base.Die();

        if (WaveSpawner.instance != null)
        {
            WaveSpawner.instance.OnEnemyKilled(transform.position);
        }

        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, transform.rotation);
        }

        Destroy(gameObject, destroyDelay);
    }
}