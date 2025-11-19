using UnityEngine;

public class XyrridMovement : MonoBehaviour
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

    private Vector3 startPos;
    private float timeOffset;
    private bool isPaused = false;
    private float pauseStartTime;
    private float pauseDuration;
    private bool isChasing = false;

    private Vector3 chaseBasePos;

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

            if (!isChasing && distance > viewRange)
                isChasing = true;
            else if (isChasing && distance <= stopChaseDistance)
                isChasing = false;
        }

        // --- UPDATE POSISI DASAR (CHASE) ---
        if (isChasing && player != null)
        {
            Vector3 dir = (player.position - chaseBasePos).normalized;
            dir.y = 0;
            chaseBasePos += dir * chaseSpeed * Time.deltaTime;
        }

        // --- GERAK SINUSOIDAL ---
        float t = Time.time + timeOffset - pauseDuration;
        float x = Mathf.Sin(t * moveSpeedX) * amplitudeX;
        float y = Mathf.Sin(t * moveSpeedY) * amplitudeY;

        Vector3 offset = new Vector3(x, y, 0);
        transform.position = chaseBasePos + offset;

        // --- ARAH FACING KE TARGET ---
        if (faceDirection)
            FaceTarget();
    }

    // -----------------------------
    //  FACE TARGET FUNCTION
    // -----------------------------
    private void FaceTarget()
    {
        if (player == null) return;

        Vector3 dir = player.position - transform.position;

        Vector3 scale = transform.localScale;
        scale.x = (dir.x >= 0) ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    // -----------------------------
    //  PAUSE SYSTEM
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
