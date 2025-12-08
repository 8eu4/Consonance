using UnityEngine;

public class PlayerMovementWithRotation : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f; // Speed at which the character rotates
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody not found on this GameObject. Please add one.");
            enabled = false; // Disable the script if no Rigidbody is present
        }
    }

    void FixedUpdate() // FixedUpdate for physics-related operations
    {
        // Get input for movement
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Calculate movement direction
        Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        // Apply movement
        if (movement != Vector3.zero)
        {
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);

            // Calculate target rotation
            Quaternion targetRotation = Quaternion.LookRotation(movement);

            // Smoothly rotate the character towards the movement direction
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
        }
    }
}