using UnityEngine;
using System.Collections;

public class LootItemMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float followSpeed = 5f;
    public float rotationSpeed = 180f; // Degrees per second
    public float startFollowDelay = 1f; // Delay before moving to player

    [Header("Pop-In (Scale Up)")]
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public float scaleDuration = 0.25f; // seconds
    public bool playPopOnStart = true; // can disable if you don't want pop-in

    private Transform target;
    private bool isFollowing = false;
    private Vector3 initialScale;

    void Awake()
    {
        // Cache the prefab's original scale so we always animate back to that, whatever it is.
        initialScale = transform.localScale;
    }

    void Start()
    {
        // If requested, start from zero and play the Pop-In animation
        if (playPopOnStart)
        {
            transform.localScale = Vector3.zero;
            StartCoroutine(ScaleUpRoutine());
        }

        // Find the player object or a specific 'loot magnet' point on the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // You can set the target to the player's transform or a child object (e.g., a "LootTarget" point)
            target = player.transform;
            StartCoroutine(StartFollowingRoutine());
        }
        // Optional: Apply an initial upwards force for a small "bounce" effect using Rigidbody.AddForce()
    }

    void Update()
    {
        // Continuous rotation in the Update loop (runs while pop-in animates)
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        if (isFollowing && target != null)
        {
            // Smoothly move towards the target's position
            transform.position = Vector3.Lerp(transform.position, target.position, followSpeed * Time.deltaTime);

            // Optional: Destroy the item when it's very close to the player to simulate collection
            if (Vector3.Distance(transform.position, target.position) < 0.5f)
            {
                // Add collection logic here (e.g., add to inventory)
                Destroy(gameObject);
            }
        }
    }

    IEnumerator StartFollowingRoutine()
    {
        yield return new WaitForSeconds(startFollowDelay);
        isFollowing = true;
    }

    IEnumerator ScaleUpRoutine()
    {
        if (scaleDuration <= 0f)
        {
            transform.localScale = initialScale;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < scaleDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / scaleDuration);
            float curveVal = scaleCurve.Evaluate(t);
            transform.localScale = initialScale * curveVal;
            yield return null;
        }

        // Ensure exact final scale
        transform.localScale = initialScale;
    }

    // Optional: Use OnTriggerEnter to detect player collision for collection
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Add collection logic here
            Destroy(gameObject);
        }
    }
}
