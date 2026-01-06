using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public bool isThisBubble = true;
    public int damage = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (isThisBubble)
            {
                Debug.Log("Player terkena BUBBLE!");
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("Player terkena LASER!");
            }
        }
    }
}
