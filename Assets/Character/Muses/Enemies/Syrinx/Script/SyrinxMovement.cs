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

    [Header("Circle Reposition Settings (Strafe)")]
    public float strafeRadius = 7f;
    public float strafeSpeed = 4f;
    public float defaultRepositionDuration = 1.2f;

    private bool isRepositioning = false;
    private float repositionTimer;
    private float strafeDirection;

    [Header("Optional Settings")]
    public bool faceDirection = false;

    [Header("Knockback Settings")]
    public float knockbackStrength = 0.5f;
    public float knockbackDuration = 0.5f;

    private Vector3 chaseBasePos;
    private float timeOffset;
    private bool isPaused = false;
    private float pauseStartTime;
    private float pauseDuration;
    private bool isChasing = false;

    private Vector3 lastOffset = Vector3.zero;
    private Vector3 knockbackOffset = Vector3.zero;
    private Coroutine knockbackRoutine;

    void Start()
    {
        chaseBasePos = transform.position;
        timeOffset = Random.Range(0f, 2f * Mathf.PI);
        strafeDirection = Random.value > 0.5f ? 1f : -1f;
    }

    void Update()
    {
        // ---------- CHASE CHECK ----------
        if (player != null && !isRepositioning)
        {
            float distance = Vector3.Distance(chaseBasePos, player.position);

            if (distance > viewRange)
                isChasing = true;
            else if (distance <= stopChaseDistance)
                isChasing = false;
        }

        Vector3 currentMoveDir = Vector3.zero;

        // ==================================================
        // 1️⃣ REPOSITION (PRIORITAS TERTINGGI)
        // ==================================================
        if (!isPaused && isRepositioning && player != null)
        {
            repositionTimer -= Time.deltaTime;
            if (repositionTimer <= 0f)
                isRepositioning = false;

            currentMoveDir = CalculateCircleStrafe();
            chaseBasePos += currentMoveDir * strafeSpeed * Time.deltaTime;
        }
        // ==================================================
        // 2️⃣ CHASE NORMAL
        // ==================================================
        else if (!isPaused && isChasing && player != null)
        {
            Vector3 dir = (player.position - chaseBasePos);
            dir.y = 0;

            if (dir.sqrMagnitude > 0.0001f)
            {
                currentMoveDir = dir.normalized;
                chaseBasePos += currentMoveDir * chaseSpeed * Time.deltaTime;
            }
        }

        UpdateAnimation(currentMoveDir);

        // ---------- SINUSOIDAL OFFSET ----------
        if (!isPaused)
        {
            float t = Time.time + timeOffset - pauseDuration;
            float x = Mathf.Sin(t * moveSpeedX) * amplitudeX;
            float y = Mathf.Sin(t * moveSpeedY) * amplitudeY;

            Vector3 offset = transform.right * x + transform.up * y;
            lastOffset = offset;

            transform.position = chaseBasePos + offset + knockbackOffset;
        }
        else
        {
            transform.position = chaseBasePos + lastOffset + knockbackOffset;
        }
    }

    // ==================================================
    // CIRCLE STRAFE
    // ==================================================
    private Vector3 CalculateCircleStrafe()
    {
        Vector3 toPlayer = chaseBasePos - player.position;
        toPlayer.y = 0;

        float targetRadius = Mathf.Clamp(strafeRadius, 0.1f, viewRange - 0.5f);
        Vector3 radial = toPlayer.normalized * (toPlayer.magnitude - targetRadius);
        Vector3 tangent = Vector3.Cross(Vector3.up, toPlayer.normalized) * strafeDirection;

        return (-radial + tangent).normalized;
    }

    // ==================================================
    // DIPANGGIL SETELAH MENEMBAK
    // ==================================================
    public void Reposition(float duration = -1f)
    {
        if (player == null) return;

        isRepositioning = true;
        repositionTimer = (duration > 0f) ? duration : defaultRepositionDuration;
        strafeDirection = Random.value > 0.5f ? 1f : -1f;
    }

    void UpdateAnimation(Vector3 worldMoveDir)
    {
        if (animator == null) return;

        Vector3 local = transform.InverseTransformDirection(worldMoveDir);
        animator.SetFloat("InputX", local.x, 0.1f, Time.deltaTime);
        animator.SetFloat("InputY", local.z, 0.1f, Time.deltaTime);
    }

    public void PauseMovement()
    {
        if (isPaused) return;
        isPaused = true;
        pauseStartTime = Time.time;
    }

    public void ResumeMovement()
    {
        if (!isPaused) return;
        isPaused = false;
        pauseDuration += Time.time - pauseStartTime;
    }

    public void ApplyKnockback(Vector3 fromPosition)
    {
        Vector3 dir = (chaseBasePos - fromPosition).normalized;
        if (knockbackRoutine != null)
            StopCoroutine(knockbackRoutine);
        knockbackRoutine = StartCoroutine(KnockbackRoutine(dir));
    }

    private IEnumerator KnockbackRoutine(Vector3 direction)
    {
        float elapsed = 0f;
        Vector3 start = direction * knockbackStrength;

        while (elapsed < knockbackDuration)
        {
            elapsed += Time.deltaTime;
            knockbackOffset = Vector3.Lerp(start, Vector3.zero, elapsed / knockbackDuration);
            yield return null;
        }

        knockbackOffset = Vector3.zero;
    }

    public bool IsChasing() => isChasing;
}
