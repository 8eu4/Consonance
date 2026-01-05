using UnityEngine;

public class XyrridLookAt : MonoBehaviour
{
    public Transform playerTarget; // Reference to the player's Transform
    public NearestTargetDetector targetDetector;
    public float rotationSpeed = 3.0f; // Speed of the rotation

    void Update()
    {
        if (targetDetector != null)
            playerTarget = targetDetector.currentTarget;

        if (playerTarget != null)
        {

            // 1. Calculate the direction from the character to the player
            Vector3 direction = playerTarget.position - transform.position;

            // Optional: Keep the rotation strictly on the Y-axis (horizontal) by zeroing the Y component of the direction
            direction.y = 0;

            // 2. Calculate the target rotation using Quaternion.LookRotation
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // 3. Smoothly rotate towards the target rotation
            // Use Quaternion.Slerp for spherical interpolation, which can feel smoother.
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Alternatively, use Quaternion.RotateTowards to ensure a consistent angular speed
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
