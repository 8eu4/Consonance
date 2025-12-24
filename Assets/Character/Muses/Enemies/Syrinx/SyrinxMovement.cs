using UnityEngine;
using System.Collections;

public class SyrinxMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeedX = 2f;
    public float moveSpeedY = 2f;
    public float amplitudeX = 1f;
    public float amplitudeY = 0.5f;

    [Header("Chase Settings")]
    public Transform player;
    public float viewRange = 10f;
    public float stopChaseDistance = 6f;
    public float chaseSpeed = 3f;

    [Header("Optional Settings")]
    public bool faceDirection = true;

    [Header("Knockback Settings")]
    public float knockbackStrength = 1.5f;   // how strong the recoil is
    public float knockbackDuration = 0.15f;  // time for knockback to damp out

    private Vector3 chaseBasePos;
    private float timeOffset;
    private bool isPaused = false;
    private float pauseStartTime;
    private float pauseDuration;
    private bool isChasing = false;

    // last offset used when pausing (so we can keep the exact visual position while paused)
    private Vector3 lastOffset = Vector3.zero;

    // knockback handling
    private Vector3 knockbackOffset = Vector3.zero;
    private Coroutine knockbackRoutine;

    void Start()
    {
        chaseBasePos = transform.position;
        timeOffset = Random.Range(0f, 2f * Mathf.PI);
        lastOffset = Vector3.zero;
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

        // ---------- CHASE BASE POSITION ----------
        if (!isPaused && isChasing && player != null)
        {
            Vector3 dir = (player.position - chaseBasePos);
            dir.y = 0;
            if (dir.sqrMagnitude > 0.0001f)
                chaseBasePos += dir.normalized * chaseSpeed * Time.deltaTime;
        }

        // ---------- SINUSOIDAL OFFSET (LOCAL SPACE) ----------
        if (!isPaused)
        {
            float t = Time.time + timeOffset - pauseDuration;
            float x = Mathf.Sin(t * moveSpeedX) * amplitudeX;
            float y = Mathf.Sin(t * moveSpeedY) * amplitudeY;

            // movement follows current rotation (so it rotates with LookAt)
            Vector3 offset = transform.right * x + transform.up * y;
            lastOffset = offset;

            // final position includes knockback offset
            transform.position = chaseBasePos + offset + knockbackOffset;

            // facing based on local x component
            if (faceDirection)
            {
                // determine sign using the local x value (x above)
                Vector3 scale = transform.localScale;
                scale.x = (x >= 0) ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
                transform.localScale = scale;
            }
        }
        else
        {
            // paused -> freeze sinusoidal and chaseBasePos, but still apply knockback visually
            transform.position = chaseBasePos + lastOffset + knockbackOffset;

            // maintain facing using lastOffset projected to local right
            if (faceDirection)
            {
                float localX = Vector3.Dot(lastOffset, transform.right);
                Vector3 scale = transform.localScale;
                scale.x = (localX >= 0) ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
                transform.localScale = scale;
            }
        }
    }

    // -----------------------------
    // Pause / Resume
    // -----------------------------
    public void PauseMovement()
    {
        if (isPaused) return;

        // store current offset so visual doesn't jump
        float t = Time.time + timeOffset - pauseDuration;
        float x = Mathf.Sin(t * moveSpeedX) * amplitudeX;
        float y = Mathf.Sin(t * moveSpeedY) * amplitudeY;
        lastOffset = transform.right * x + transform.up * y;

        isPaused = true;
        pauseStartTime = Time.time;
    }

    public void ResumeMovement()
    {
        if (!isPaused) return;
        isPaused = false;
        pauseDuration += Time.time - pauseStartTime;
    }

    // -----------------------------
    // Knockback (Recoil)
    // -----------------------------
    // fromPosition: the position of the "shot target" (we will push away from it)
    public void ApplyKnockback(Vector3 fromPosition)
    {
        // compute direction from shot towards our base pos (we want to push opposite the shot direction)
        Vector3 dir = (chaseBasePos - fromPosition);
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f)
        {
            // fallback direction (backwards relative to forward)
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

            // smooth damping (ease out)
            knockbackOffset = Vector3.Lerp(startOffset, Vector3.zero, t);
            yield return null;
        }

        knockbackOffset = Vector3.zero;
        knockbackRoutine = null;
    }

    public bool IsChasing() => isChasing;
}
