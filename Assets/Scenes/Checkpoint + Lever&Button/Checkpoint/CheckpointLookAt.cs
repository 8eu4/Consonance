using UnityEngine;

public class LookAtPlayerY : MonoBehaviour
{
    private Transform playerTransform;

    void Start()
    {
        // Find the player object by tag at the start of the game
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Player object with tag 'Player' not found!");
        }
    }

    void Update()
    {
        if (playerTransform != null)
        {
            // Get the direction to the player, ignoring the Y difference
            Vector3 directionToPlayer = playerTransform.position - transform.position;
            directionToPlayer.y = 0; // Lock the vertical component

            // If the object is far enough away from the player to form a direction
            if (directionToPlayer != Vector3.zero)
            {
                // Create a rotation that looks along the new direction (Quaternion.LookRotation)
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);

                // Apply the rotation to the object's transform
                transform.rotation = targetRotation;
            }
        }
    }
}
