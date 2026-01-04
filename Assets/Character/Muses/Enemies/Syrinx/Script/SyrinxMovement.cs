using UnityEngine;
using System.Collections;

public class SyrinxMovement : MonoBehaviour
{
    [Header("Animation")]
    public Animator animator;

    [Header("Movement Settings")]
    public float moveSpeedX = 1f;
    public float moveSpeedY = 1f;
    public float amplitudeX = 1f;
    public float amplitudeY = 1f;

    [Header("Chase Settings")]
    public Transform player;
    public float viewRange = 12f;
    public float stopChaseDistance = 10f;
    public float chaseSpeed = 6f;

    [Header("Circle Reposition Settings (Strafe)")] // [BARU]
    public float strafeRadius = 7f;          // Jarak ideal saat strafe
    public float strafeSpeed = 4f;           // Kecepatan melingkar
    public float defaultRepositionDuration = 1.2f; // Durasi default strafe

    private bool isRepositioning = false;
    private float repositionTimer;
    private float strafeDirection;           // +1 (Kanan) atau -1 (Kiri)

    [Header("Optional Settings")]
    public bool faceDirection = true;

    [Header("Knockback Settings")]
    public float knockbackStrength = 0.5f;
    public float knockbackDuration = 0.5f;

    private Vector3 chaseBasePos;
    private float timeOffset;
    private bool isPaused = false;
    private float pauseStartTime;
    private float pauseDuration;
    private bool isChasing = false;

    // last offset used when pausing
    private Vector3 lastOffset = Vector3.zero;

    // knockback handling
    private Vector3 knockbackOffset = Vector3.zero;
    private Coroutine knockbackRoutine;

    void Start()
    {
        chaseBasePos = transform.position;
        timeOffset = Random.Range(0f, 2f * Mathf.PI);
        lastOffset = Vector3.zero;

        // Acak arah putaran awal
        strafeDirection = Random.value > 0.5f ? 1f : -1f;
    }

    void Update()
    {
        // ---------- CHASE CHECK ----------
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            if (!isChasing && distance > viewRange)
                isChasing = true;
            else if (isChasing && distance <= stopChaseDistance)
                isChasing = false;
        }

        Vector3 currentMoveDir = Vector3.zero;

        // ---------- MOVEMENT LOGIC (Chase vs Strafe) ----------
        if (!isPaused && isChasing && player != null)
        {
            // LOGIKA REPOSITION / STRAFE [BARU]
            if (isRepositioning)
            {
                repositionTimer -= Time.deltaTime;
                if (repositionTimer <= 0f)
                    isRepositioning = false;

                // Hitung gerakan melingkar & simpan arahnya ke currentMoveDir
                currentMoveDir = CalculateCircleStrafe();

                // Terapkan gerakan ke base position
                chaseBasePos += currentMoveDir * strafeSpeed * Time.deltaTime;
            }
            // LOGIKA CHASE BIASA (LURUS)
            else
            {
                Vector3 dir = (player.position - chaseBasePos);
                dir.y = 0;
                if (dir.sqrMagnitude > 0.0001f)
                {
                    Vector3 moveDir = dir.normalized;
                    currentMoveDir = moveDir;
                    chaseBasePos += moveDir * chaseSpeed * Time.deltaTime;
                }
            }
        }

        // ---------- UPDATE ANIMATOR ----------
        UpdateAnimation(currentMoveDir);

        // ---------- SINUSOIDAL OFFSET (LOCAL SPACE) ----------
        if (!isPaused)
        {
            float t = Time.time + timeOffset - pauseDuration;
            float x = Mathf.Sin(t * moveSpeedX) * amplitudeX;
            float y = Mathf.Sin(t * moveSpeedY) * amplitudeY;

            // movement follows current rotation
            Vector3 offset = transform.right * x + transform.up * y;
            lastOffset = offset;

            // final position
            transform.position = chaseBasePos + offset + knockbackOffset;

            // facing based on sinusoidal wave
            if (faceDirection)
            {
                Vector3 scale = transform.localScale;
                scale.x = (x >= 0) ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
                transform.localScale = scale;
            }
        }
        else
        {
            // paused -> freeze sinusoidal
            transform.position = chaseBasePos + lastOffset + knockbackOffset;

            if (faceDirection)
            {
                float localX = Vector3.Dot(lastOffset, transform.right);
                Vector3 scale = transform.localScale;
                scale.x = (localX >= 0) ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
                transform.localScale = scale;
            }
        }
    }

    // [BARU] Logika matematika untuk gerakan melingkar
    private Vector3 CalculateCircleStrafe()
    {
        if (player == null) return Vector3.zero;

        Vector3 toPlayer = chaseBasePos - player.position;
        toPlayer.y = 0;

        float currentDist = toPlayer.magnitude;
        // Jaga agar radius tidak melebihi jarak pandang
        float targetRadius = Mathf.Clamp(strafeRadius, 0.1f, viewRange - 0.5f);

        // 1. Koreksi Radial (Maju/Mundur menyesuaikan jari-jari lingkaran)
        Vector3 radialCorrection = Vector3.zero;
        if (currentDist > 0.01f)
            radialCorrection = toPlayer.normalized * (currentDist - targetRadius);

        // 2. Gerak Tangent (Melingkar ke samping)
        // Cross Product Vector.up dengan arah ke player menghasilkan vektor tegak lurus (kiri/kanan)
        Vector3 tangent = Vector3.Cross(Vector3.up, toPlayer.normalized) * strafeDirection;

        // Gabungkan: (Mundur/Maju ke Radius Ideal) + (Geser ke Samping)
        return (-radialCorrection + tangent).normalized;
    }

    // [BARU] Fungsi Public untuk memicu Strafe (Dipanggil script senjata/AI)
    public void Reposition(float duration = -1f)
    {
        if (player == null) return;

        isRepositioning = true;
        repositionTimer = (duration > 0f) ? duration : defaultRepositionDuration;

        // Acak arah lagi setiap kali reposition (biar tidak monoton)
        strafeDirection = Random.value > 0.5f ? 1f : -1f;
    }

    void UpdateAnimation(Vector3 worldMoveDir)
    {
        if (animator == null) return;
        Vector3 localVelocity = transform.InverseTransformDirection(worldMoveDir);
        animator.SetFloat("InputX", localVelocity.x, 0.1f, Time.deltaTime);
        animator.SetFloat("InputY", localVelocity.z, 0.1f, Time.deltaTime);
    }

    // -----------------------------
    // Pause / Resume
    // -----------------------------
    public void PauseMovement()
    {
        if (isPaused) return;

        float t = Time.time + timeOffset - pauseDuration;
        float x = Mathf.Sin(t * moveSpeedX) * amplitudeX;
        float y = Mathf.Sin(t * moveSpeedY) * amplitudeY;
        lastOffset = transform.right * x + transform.up * y;

        isPaused = true;
        pauseStartTime = Time.time;

        if (animator != null)
        {
            animator.SetFloat("InputX", 0);
            animator.SetFloat("InputY", 0);
        }
    }

    public void ResumeMovement()
    {
        if (!isPaused) return;
        isPaused = false;
        pauseDuration += Time.time - pauseStartTime;
    }

    // -----------------------------
    // Knockback
    // -----------------------------
    public void ApplyKnockback(Vector3 fromPosition)
    {
        Vector3 dir = (chaseBasePos - fromPosition);
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f)
        {
            dir = -transform.forward;
            dir.y = 0f;
        }
        dir = dir.normalized;

        if (knockbackRoutine != null)
            StopCoroutine(knockbackRoutine);
        knockbackRoutine = StartCoroutine(KnockbackRoutine(dir));
    }

    private IEnumerator KnockbackRoutine(Vector3 direction)
    {
        float elapsed = 0f;
        Vector3 startOffset = direction * knockbackStrength;

        while (elapsed < knockbackDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / knockbackDuration;
            knockbackOffset = Vector3.Lerp(startOffset, Vector3.zero, t);
            yield return null;
        }

        knockbackOffset = Vector3.zero;
        knockbackRoutine = null;
    }

    public bool IsChasing() => isChasing;
}