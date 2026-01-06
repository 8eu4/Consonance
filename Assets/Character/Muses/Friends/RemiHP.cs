using Unity.VisualScripting;
using UnityEngine;

public class RemiHP : Health
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<EnemyAttack>(out var attack))
        {
            TakeDamage(attack.damage);
            Destroy(collision.gameObject);
        }
    }

    protected override void Die()
    {
        base.Die();
        var identity = GetComponent<PlayerIdentity>();
        if (identity != null)
        {
            identity.Die();
        }
    }
}
