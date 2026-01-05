using UnityEngine;

public class DomiHP : Health
{
    private void OnCollisionEnter(Collision collision)
    {
        EnemyAttack bubble = collision.gameObject.GetComponent<EnemyAttack>();

        void TakeDamage(int amount)
        {
            CurrentHP -= amount;

            Debug.Log($"{gameObject.name} took {amount} damage, HP is now {CurrentHP}");

            if (CurrentHP == 0)
            {
                Die();
            }
        }

        if (bubble != null)
        {
            Debug.Log("Conductor terkena Bubble!");

            // kalau nanti mau pakai damage:
            TakeDamage(bubble.damage);

            // optional: hancurkan bubble
            Destroy(bubble.gameObject);
        }

        void Die()
        {
            Debug.Log("Domi Die");
            Destroy(gameObject);
        }
    }
}
