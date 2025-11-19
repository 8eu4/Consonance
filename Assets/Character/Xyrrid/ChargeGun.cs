using UnityEngine;
using System.Collections;

public class XyrridBubbleGun : MonoBehaviour
{
    [Header("References")]
    public Transform firePoint;
    public Transform target;
    public GameObject bubblePrefab;
    public AudioSource chargeAudio;
    public XyrridMovement movementScript;

    [Header("Timings")]
    public Vector2 reloadTimeRange = new Vector2(1f, 4f);
    public float chargeTime = 1.2f;
    public float fixedShootOffset = 0.5f;

    [Header("Bubble Settings")]
    public float bubbleSpeed = 6f;
    public float bubbleLifetime = 4f;
    public bool useArc = false;         // true = tembakan melengkung (parabola)
    public float arcHeight = 1.5f;      // tinggi lengkung

    [Header("Attack Range")]
    public float attackRange = 8f;

    [Header("Behavior")]
    public bool stopMovementWhileFiring = true;

    private Vector3 lockedTargetPos;

    private void Start()
    {
        StartCoroutine(FireRoutine());
    }

    private IEnumerator FireRoutine()
    {
        while (true)
        {
            if (target == null)
            {
                yield return null;
                continue;
            }

            float distance = Vector3.Distance(transform.position, target.position);

            // skip kalau player terlalu jauh
            if (distance > attackRange)
            {
                yield return null;
                continue;
            }

            // random delay reload
            float reloadTime = Random.Range(reloadTimeRange.x, reloadTimeRange.y);
            yield return new WaitForSeconds(reloadTime);

            // mulai charge
            if (chargeAudio != null)
                chargeAudio.Play();

            float elapsed = 0f;
            bool hasLockedTarget = false;

            // PHASE: Charging
            while (elapsed < chargeTime)
            {
                elapsed += Time.deltaTime;

                if (!hasLockedTarget && elapsed >= (chargeTime - fixedShootOffset))
                {
                    lockedTargetPos = target.position;
                    hasLockedTarget = true;
                }

                yield return null;
            }

            // SHOOT BUBBLE
            ShootBubble(lockedTargetPos);

            yield return null;
        }
    }

    private void ShootBubble(Vector3 targetPos)
    {
        if (firePoint == null || bubblePrefab == null) return;

        if (stopMovementWhileFiring && movementScript != null)
            movementScript.PauseMovement();

        GameObject bubble = Instantiate(bubblePrefab, firePoint.position, Quaternion.identity);
        Rigidbody rb = bubble.GetComponent<Rigidbody>();

        if (rb == null)
        {
            rb = bubble.AddComponent<Rigidbody>();
            rb.useGravity = false;
        }

        // normal tembak lurus
        if (!useArc)
        {
            Vector3 dir = (targetPos - firePoint.position).normalized;
            rb.linearVelocity = dir * bubbleSpeed;
        }
        else
        {
            // parabola
            rb.useGravity = false;

            Vector3 dirXZ = (new Vector3(targetPos.x, 0, targetPos.z) -
                             new Vector3(firePoint.position.x, 0, firePoint.position.z)).normalized;

            Vector3 arcVelocity = dirXZ * bubbleSpeed;
            arcVelocity.y = arcHeight;      // angkat sedikit biar melengkung
            rb.linearVelocity = arcVelocity;
        }

        // bubble lifetime
        Destroy(bubble, bubbleLifetime);

        if (stopMovementWhileFiring && movementScript != null)
            StartCoroutine(ResumeAfterDelay(0.2f)); // delay kecil biar nembak stabil
    }

    private IEnumerator ResumeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (movementScript != null)
            movementScript.ResumeMovement();
    }
}
