using UnityEngine;

public class SyrinxMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeedX = 2f;
    public float moveSpeedY = 2f;
    public float amplitudeX = 1f;
    public float amplitudeY = 0.5f;

    [Header("Chase Settings")]
    public Transform player;
    public float viewRange = 10f;          // mulai mengejar kalau terlalu jauh
    public float stopChaseDistance = 6f;   // berhenti mengejar kalau cukup dekat
    public float chaseSpeed = 3f;          // kecepatan dasar mengejar target

    [Header("Optional Settings")]
    public bool faceDirection = true;

    private Vector3 startPos;
    private float timeOffset;
    private bool isPaused = false;
    private float pauseStartTime;
    private float pauseDuration;
    private bool isChasing = false;

    private Vector3 chaseBasePos;   // posisi "dasar" yang bergerak mendekat ke target

    void Start()
    {
        startPos = transform.position;
        chaseBasePos = startPos;
        timeOffset = Random.Range(0f, 2f * Mathf.PI);
    }

    void Update()
    {
        if (isPaused) return;

        // --- CEK JARAK UNTUK CHASE ---
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            // mulai mengejar kalau target keluar dari viewRange
            if (!isChasing && distance > viewRange)
                isChasing = true;

            // berhenti mengejar kalau sudah cukup dekat
            else if (isChasing && distance <= stopChaseDistance)
                isChasing = false;
        }

        // --- UPDATE POSISI DASAR (CHASE) ---
        if (isChasing && player != null)
        {
            Vector3 dir = (player.position - chaseBasePos).normalized;
            dir.y = 0; // tidak ubah ketinggian
            chaseBasePos += dir * chaseSpeed * Time.deltaTime;
        }

        // --- GERAK SINUSOIDAL DI ATAS POSISI DASAR ---
        float t = Time.time + timeOffset - pauseDuration;
        float x = Mathf.Sin(t * moveSpeedX) * amplitudeX;
        float y = Mathf.Sin(t * moveSpeedY) * amplitudeY;

        Vector3 offset = new Vector3(x, y, 0);
        transform.position = chaseBasePos + offset;

        // --- ARAH FACING ---
        if (faceDirection)
        {
            Vector3 scale = transform.localScale;
            scale.x = (x >= 0) ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    // -----------------------------
    // ✅ Pause & Resume System
    // -----------------------------
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

    public bool IsChasing() => isChasing;
}
