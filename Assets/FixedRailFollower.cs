using UnityEngine;

public class FixedRailFollower : MonoBehaviour
{
    private Animator animator;

    [Header("Rail Settings")]
    public Vector3 railDirection = Vector3.forward; // World-space rail
    public float followMultiplier = 1f;

    private Rigidbody rb;

    void Start()

    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        railDirection = railDirection.normalized;
    }

    void FixedUpdate()
    {
        // Safety: don't move if THIS object somehow becomes the player
        if (gameObject.CompareTag("Player"))
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        // Find current player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb == null) return;

        // 1?? Check relative position along the rail
        Vector3 toPlayer = player.transform.position - transform.position;
        float playerAheadAmount = Vector3.Dot(toPlayer, railDirection);

        // 2?? Check player forward movement
        float playerForwardSpeed =
            Vector3.Dot(playerRb.linearVelocity, railDirection);

        // 3?? Only move if player is AHEAD and moving FORWARD
        float forwardAmount = 0f;

        if (playerAheadAmount > 0f && playerForwardSpeed > 0f)
        {
            forwardAmount = playerForwardSpeed;
        }

        // 4?? Apply rail-only velocity
        Vector3 railVelocity = railDirection * forwardAmount * followMultiplier;

        rb.linearVelocity = new Vector3(
            railVelocity.x,
            rb.linearVelocity.y,
            railVelocity.z
        );

        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(forwardAmount));
        }
    }

}
