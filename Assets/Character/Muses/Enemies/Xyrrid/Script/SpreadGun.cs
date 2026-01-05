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

    [Tooltip("Waktu tunggu (aiming) sebelum animasi dimainkan")]
    public float chargeTime = 1.0f;

    [Tooltip("Delay mikro setelah animasi mulai agar peluru pas keluar saat tangan lurus (misal 0.1 atau 0.2)")]
    public float shootAnimDelay = 0.1f;

    [Header("Bubble Settings")]
    public float bubbleSpeed = 6f;
    public float bubbleLifetime = 4f;
    public bool useArc = false;
    public float arcHeight = 1.5f;

    [Header("Shotgun Settings")]
    public int pelletCount = 5;
    public float pelletInterval = 0.1f;
    public float spreadAngle = 20f;

    [Header("Attack Range")]
    public float attackRange = 8f;

    [Header("Behavior")]
    public bool stopMovementWhileFiring = true;

    private Vector3 lockedTargetPos;

    private void Start()
    {
        anim = GetComponent<Animator>();
        StartCoroutine(FireRoutine());
    }

    private IEnumerator FireRoutine()
    {
        float reloadTime = Random.Range(reloadTimeRange.x, reloadTimeRange.y);

        while (true)
        {
            // 1. TUNGGU RELOAD (Cooldown antar serangan)
            yield return new WaitForSeconds(reloadTime);

            if (targetDetector != null)
                target = targetDetector.currentTarget;


            // Cek Target & Jarak
            if (target == null) { yield return null; continue; }
            float distance = Vector3.Distance(transform.position, target.position);
            if (distance > attackRange) { yield return null; continue; }

            // 2. STOP MOVEMENT & MULAI CHARGE (AIMING)
            // Musuh diam dulu sejenak (seolah-olah membidik)
            if (stopMovementWhileFiring && movementScript != null)
                movementScript.PauseMovement();

            // Tunggu durasi ChargeTime (Waktu persiapan sebelum animasi)
            yield return new WaitForSeconds(chargeTime);

            // 3. PLAY ANIMASI (EKSEKUSI)
            // Setelah charge selesai, baru animasi dimainkan
            if (anim != null) anim.SetTrigger("Shoot");

            // 4. AUDIO & MICRO DELAY
            if (chargeAudio != null) chargeAudio.Play();

            // Delay sangat singkat (misal 0.1s) biar peluru tidak keluar 
            // saat tangan masih blending dari posisi Idle.
            if (shootAnimDelay > 0) yield return new WaitForSeconds(shootAnimDelay);

            // 5. KUNCI TARGET & TEMBAK
            lockedTargetPos = target.position;
            yield return StartCoroutine(FireShotgun(lockedTargetPos));

            // 6. RESUME MOVEMENT
            if (stopMovementWhileFiring && movementScript != null)
                movementScript.ResumeMovement();

            // 7. SIAPKAN RELOAD BERIKUTNYA
            float nextReload = Random.Range(reloadTimeRange.x, reloadTimeRange.y);
            if (movementScript != null && !movementScript.IsChasing())
            {
                movementScript.Reposition(nextReload);
            }
            reloadTime = nextReload;
        }
    }

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
        if (firePoint == null || bubblePrefab == null) return;

        GameObject bubble = Instantiate(bubblePrefab, firePoint.position, Quaternion.identity);
        Rigidbody rb = bubble.GetComponent<Rigidbody>();

        if (rb == null) rb = bubble.AddComponent<Rigidbody>();
        rb.useGravity = false;

        Vector3 dir = (targetPos - firePoint.position).normalized;

        float angle = Random.Range(-spreadAngle, spreadAngle);
        Quaternion rot = Quaternion.Euler(0, angle, 0);
        dir = rot * dir;

        if (!useArc)
        {
            rb.linearVelocity = dir * bubbleSpeed;
        }
        else
        {
            Vector3 dirXZ = new Vector3(dir.x, 0, dir.z).normalized;
            Vector3 arcVelocity = dirXZ * bubbleSpeed;
            arcVelocity.y = arcHeight;
            rb.linearVelocity = arcVelocity;
        }

        Destroy(bubble, bubbleLifetime);
    }
}