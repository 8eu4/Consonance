using UnityEngine;

public class XyrridMovement : MonoBehaviour
{
    private Animator anim;
    private Vector3 lastPosition;

    [Header("Movement Settings")]
    public float moveSpeedX = 1f;
    public float moveSpeedZ = 2f;
    public float amplitudeX = 3f;
    public float amplitudeZ = 2f;

    [Header("Chase Settings")]
    public Transform player;
    public float viewRange = 12f;
    public float stopChaseDistance = 10f;
    public float chaseSpeed = 5f;

    [Header("Optional Settings")]
    public bool faceDirection = true;

    private Vector3 startPos;
    private float timeOffset;
    private bool isPaused = false;
    private float pauseStartTime;
    private float pauseDuration;

    private bool isChasing = false;

    [Header("Circle Reposition Settings")]
    public float strafeRadius = 7f;          // jarak ideal dari player
    public float strafeSpeed = 4f;           // kecepatan melingkar
    public float repositionDuration = 1.2f;

    private bool isRepositioning = false;
    private float repositionTimer;
    private float strafeDirection;           // +1 / -1

    private Vector3 chaseBasePos;

    void Start()
    {
        startPos = transform.position;
        chaseBasePos = startPos;
        timeOffset = Random.Range(0f, 2f * Mathf.PI);
        strafeDirection = Random.value > 0.5f ? 1f : -1f;

        anim = GetComponent<Animator>(); // Ambil referensi Animator
        lastPosition = transform.position;
    }

    void Update()
    {
        if (isPaused) return;

        // --------------------------------
        // CHASE LOGIC (SELALU AKTIF)
        // --------------------------------
        if (player != null)
        {
            float distance = Vector3.Distance(chaseBasePos, player.position);

            if (distance > viewRange)
                isChasing = true;
            else if (distance <= stopChaseDistance)
                isChasing = false;
        }

        // --------------------------------
        // MOVEMENT CORE
        // --------------------------------
        if (isRepositioning && player != null)
        {
            CircleStrafe();
            repositionTimer -= Time.deltaTime;

            if (repositionTimer <= 0f)
                isRepositioning = false;
        }
        else if (isChasing && player != null)
        {
            // CHASE NORMAL
            Vector3 dir = (player.position - chaseBasePos).normalized;
            dir.y = 0;
            chaseBasePos += dir * chaseSpeed * Time.deltaTime;
        }

        // --------------------------------
        // SINUSOIDAL FLOAT
        // --------------------------------
        float t = Time.time + timeOffset - pauseDuration;
        float x = Mathf.Sin(t * moveSpeedX) * amplitudeX;
        float z = Mathf.Sin(t * moveSpeedZ) * amplitudeZ;

        transform.position = chaseBasePos + new Vector3(x, 0, z);

        // --------------------------------
        // FACE TARGET
        // --------------------------------
        if (faceDirection)
            FaceTarget();

        UpdateAnimator(); // Panggil fungsi animasi
    }

    void UpdateAnimator()
    {
        if (anim == null) return;

        // 1. Hitung kecepatan global
        Vector3 velocity = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;

        // 2. Ubah kecepatan global menjadi lokal (relatif terhadap arah hadap karakter)
        //    Agar saat dia menghadap player, gerak ke kiri terbaca sebagai "Kiri" bukan "Barat/Timur"
        Vector3 localVelocity = transform.InverseTransformDirection(velocity);

        // 3. Normalisasi nilai agar berada di range -1 sampai 1 (untuk Blend Tree)
        //    Nilai pembagi (5f) disesuaikan dengan rata-rata moveSpeed kamu
        float inputX = Mathf.Clamp(localVelocity.x / chaseSpeed, -1f, 1f);
        float inputZ = Mathf.Clamp(localVelocity.z / chaseSpeed, -1f, 1f);

        // 4. Kirim ke Animator (Damped agar transisi halus)
        anim.SetFloat("InputX", inputX, 0.1f, Time.deltaTime);
        anim.SetFloat("InputZ", inputZ, 0.1f, Time.deltaTime);
    }

    // -----------------------------
    // CIRCLE STRAFE (INTI SISTEM)
    // -----------------------------
    private void CircleStrafe()
    {
        Vector3 toPlayer = chaseBasePos - player.position;
        toPlayer.y = 0;

        float currentDist = toPlayer.magnitude;

        // jaga radius agar tetap dalam chase range
        // gunakan clamp agar radius tetap masuk akal (tidak melewati viewRange)
        float targetRadius = Mathf.Clamp(strafeRadius, 0.1f, viewRange - 0.5f);

        // koreksi jarak (jika terlalu dekat / jauh)
        Vector3 radialCorrection = Vector3.zero;
        if (currentDist > 0.01f)
            radialCorrection = toPlayer.normalized * (currentDist - targetRadius);

        // arah melingkar (tangent)
        Vector3 tangent = Vector3.Cross(Vector3.up, toPlayer.normalized) * strafeDirection;

        // gabungkan koreksi radial + tangent untuk gerakan melingkar yang menjaga radius
        Vector3 moveDir = (-radialCorrection + tangent).normalized;

        chaseBasePos += moveDir * strafeSpeed * Time.deltaTime;
    }

    // -----------------------------
    // FACE TARGET
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
    // TRIGGER REPOSITION (DIPANGGIL SENJATA)
    // versi baru: menerima durasi opsional -> reposition akan berlangsung selama duration jika > 0
    // -----------------------------
    public void Reposition(float duration = -1f)
    {
        if (player == null) return;

        isRepositioning = true;
        // jika duration valid gunakan itu, jika tidak gunakan default repositionDuration
        repositionTimer = (duration > 0f) ? duration : repositionDuration;

        // random ganti arah strafe tiap reposition
        strafeDirection = Random.value > 0.5f ? 1f : -1f;
    }

    // -----------------------------
    // PAUSE SYSTEM (TIDAK DIUBAH)
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
