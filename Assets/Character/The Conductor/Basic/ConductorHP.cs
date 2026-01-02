using UnityEngine;

public class ConductorHP : Health
{
    private void OnCollisionEnter(Collision collision)
    {
        // Cek apakah object yang menabrak memiliki BubbleDamage
        BubbleDamage bubble = collision.gameObject.GetComponent<BubbleDamage>();

        if (bubble != null)
        {
            Debug.Log("Conductor terkena Bubble!");

            // kalau nanti mau pakai damage:
            // TakeDamage(bubble.damage);

            // optional: hancurkan bubble
            Destroy(bubble.gameObject);
        }
    }
}
