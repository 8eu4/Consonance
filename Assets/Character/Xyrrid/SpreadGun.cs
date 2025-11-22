using UnityEngine;
using System.Collections;

public class XyrridSpreadGun : MonoBehaviour
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
    public bool useArc = false;
    public float arcHeight = 1.5f;

    [Header("Shotgun Settings")]
    public int pelletCount = 5;          // jumlah tembakan
    public float pelletInterval = 0.1f;  // delay antar tembakan
    public float spreadAngle = 20f;      // derajat penyebaran

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

            if (distance > attackRange)
            {
                yield return null;
                continue;
            }

            float reloadTime = Random.Range(reloadTimeRange.x, reloadTimeRange.y);
            yield return new WaitForSeconds(reloadTime);

            if (chargeAudio != null)
                chargeAudio.Play();

            float elapsed = 0f;
            bool hasLockedTarget = false;

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

            // SHOTGUN MODE — tembak bubble satu-satu tapi menyebar
            yield return StartCoroutine(FireShotgun(lockedTargetPos));
        }
    }

    private IEnumerator FireShotgun(Vector3 targetPos)
    {
        if (stopMovementWhileFiring && movementScript != null)
            movementScript.PauseMovement();

        for (int i = 0; i < pelletCount; i++)
        {
            ShootWithSpread(targetPos);
            yield return new WaitForSeconds(pelletInterval);
        }

        if (stopMovementWhileFiring && movementScript != null)
            movementScript.ResumeMovement();
    }

    private void ShootWithSpread(Vector3 targetPos)
    {
        if (firePoint == null || bubblePrefab == null) return;

        GameObject bubble = Instantiate(bubblePrefab, firePoint.position, Quaternion.identity);
        Rigidbody rb = bubble.GetComponent<Rigidbody>();

        if (rb == null)
        {
            rb = bubble.AddComponent<Rigidbody>();
            rb.useGravity = false;
        }

        // arah dasar ke target
        Vector3 dir = (targetPos - firePoint.position).normalized;

        // apply spread random angle (horizontal)
        float angle = Random.Range(-spreadAngle, spreadAngle);
        Quaternion rot = Quaternion.Euler(0, angle, 0);
        dir = rot * dir;

        // straight shot
        if (!useArc)
        {
            rb.linearVelocity = dir * bubbleSpeed;
        }
        else
        {
            // arc/parabola shot
            Vector3 dirXZ = new Vector3(dir.x, 0, dir.z).normalized;
            Vector3 arcVelocity = dirXZ * bubbleSpeed;
            arcVelocity.y = arcHeight;
            rb.linearVelocity = arcVelocity;
        }

        Destroy(bubble, bubbleLifetime);
    }
}
