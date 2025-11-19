using UnityEngine;

public class BubbleDamage : MonoBehaviour
{
    public float damage = 5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // contoh placeholder
            Debug.Log("Player kena bubble!");

            Destroy(gameObject); // bubble pecah
        }
    }
}
