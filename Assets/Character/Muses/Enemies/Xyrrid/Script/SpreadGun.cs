using UnityEngine;
using System.Collections;

public class XyrridSpreadGun : MonoBehaviour
{
    private Animator anim;

    [Header("References")]
    public Transform firePoint;
    public Transform target;
    public NearestTargetDetector targetDetector;
    public GameObject bubblePrefab;
    public AudioSource chargeAudio;
    public XyrridMovement movementScript;

    [Header("Timings")]
    public Vector2 reloadTimeRange = new Vector2(1f, 4f);
    public float chargeTime = 1.0f;
    public float shootAnimDelay = 0.1f;

    [Header("Bubble Settings")]
    public float bubbleSpeed = 6f;
    public float bubbleLifetime = 4f;

    [Header("Shotgun Settings")]
    public int pelletCount = 5;
    public float pelletInterval = 0.1f;
    public float spreadAngle = 20f;

    [Header("Attack Settings")]
    public float attackRange = 8f;

    [Tooltip("Layer yang dianggap sebagai penghalang tembakan (Wall / Environment)")]
    public LayerMask obstacleMask;

    [Header("Behavior")]
    public bool stopMovementWhileFiring = true;

    private Vector3 lockedTargetPos;

    // cached squared range (optimization)
    private float attackRangeSqr;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        attackRangeSqr = attackRange * attackRange;
    }

    private void Start()
    {
        StartCoroutine(FireRoutine());
    }

    // ======================================================
    // MAIN FIRE LOOP
    // ======================================================
    private IEnumerator FireRoutine()
    {
        float reloadTime = Random.Range(reloadTimeRange.x, reloadTimeRange.y);

        while (true)
        {
            yield return new WaitForSeconds(reloadTime);

            if (targetDetector != null)
                target = targetDetector.currentTarget;

            if (target == null)
                continue;

            // ---------- DISTANCE CHECK (CHEAP) ----------
            Vector3 toTarget = target.position - transform.position;
            toTarget.y = 0f;

            if (toTarget.sqrMagnitude > attackRangeSqr)
                continue;

            // ---------- LINE OF SIGHT CHECK (RAYCAST) ----------
            if (!HasLineOfSight(target))
                continue;

            // ---------- STOP MOVEMENT ----------
            if (stopMovementWhileFiring && movementScript != null)
                movementScript.PauseMovement();

            yield return new WaitForSeconds(chargeTime);

            if (anim != null)
                anim.SetTrigger("Shoot");

            if (chargeAudio != null)
                chargeAudio.Play();

            if (shootAnimDelay > 0f)
                yield return new WaitForSeconds(shootAnimDelay);

            lockedTargetPos = target.position;
            yield return StartCoroutine(FireShotgun(lockedTargetPos));

            if (stopMovementWhileFiring && movementScript != null)
                movementScript.ResumeMovement();

            float nextReload = Random.Range(reloadTimeRange.x, reloadTimeRange.y);

            if (movementScript != null && !movementScript.IsChasing())
                movementScript.Reposition(nextReload);

            reloadTime = nextReload;
        }
    }

    // ======================================================
    // LINE OF SIGHT CHECK (OPTIMIZED)
    // ======================================================
    private bool HasLineOfSight(Transform target)
    {
        Vector3 origin = firePoint != null ? firePoint.position : transform.position;
        Vector3 targetPos = target.position;

        Vector3 dir = targetPos - origin;
        float dist = dir.magnitude;

        // normalize once
        dir /= dist;

        // Raycast hanya ke obstacle layer
        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist, obstacleMask))
        {
            // Terhalang tembok / environment
            return false;
        }

        // Tidak ada penghalang → LOS clear
        return true;
    }

    // ======================================================
    // SHOOTING
    // ======================================================
    private IEnumerator FireShotgun(Vector3 targetPos)
    {
        for (int i = 0; i < pelletCount; i++)
        {
            ShootWithSpread(targetPos);
            yield return new WaitForSeconds(pelletInterval);
        }
    }

    private void ShootWithSpread(Vector3 targetPos)
    {
        if (firePoint == null || bubblePrefab == null)
            return;

        GameObject bubble = Instantiate(bubblePrefab, firePoint.position, Quaternion.identity);
        Rigidbody rb = bubble.GetComponent<Rigidbody>();

        if (rb == null)
            rb = bubble.AddComponent<Rigidbody>();

        rb.useGravity = false;

        Vector3 dir = (targetPos - firePoint.position).normalized;
        float angle = Random.Range(-spreadAngle, spreadAngle);
        dir = Quaternion.Euler(0f, angle, 0f) * dir;

        rb.linearVelocity = dir * bubbleSpeed;

        Destroy(bubble, bubbleLifetime);
    }
}
