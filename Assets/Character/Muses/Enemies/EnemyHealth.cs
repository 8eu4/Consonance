using System;
using UnityEngine;

public class EnemyHealth : Health
{
    public void TakeDamage(int amount)
    {
        CurrentHP -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage, HP is now {CurrentHP}");
    }
    public void Heal(int amount)
    {
        CurrentHP += amount;
        Debug.Log($"{gameObject.name} healed {amount}, HP is now {CurrentHP}");
    }
}
