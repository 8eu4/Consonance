using System;
using UnityEngine;



public class EnemyHealth : Health
{
    public GameObject deathEffectPrefab; // Drag your death animation/particle prefab here in the Inspector
    public float destroyDelay = 0.1f; // Adjust to match your death animation/effect duration

    public void TakeDamage(int amount)
    {
        CurrentHP -= amount;

        Debug.Log($"{gameObject.name} took {amount} damage, HP is now {CurrentHP}");

        if( CurrentHP == 0)
        {
            Die();
        }
    }
    public void Heal(int amount)
    {
        CurrentHP += amount;
        Debug.Log($"{gameObject.name} healed {amount}, HP is now {CurrentHP}");
    }

    void Die()
    {
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, transform.rotation);
        }

        //Debug.Log(gameObject.name + " died!"); // Log message for debugging
        //Destroy(gameObject); // Destroy the entire enemy GameObject
        Destroy(gameObject, destroyDelay);

    }
}
