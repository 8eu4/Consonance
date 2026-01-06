using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class VisionHeart : MonoBehaviour
{
    [Header("Auto-Detect Settings")]
    public Renderer heartRenderer;

    [Header("Visual Settings")]
    // Atur warna gradien di sini (Merah -> Pink)
    public Gradient targetGradient;

    [Header("Size & Animation")]
    [Range(0.1f, 10f)]
    public float baseRadius = 1.5f;      // <--- Defaultnya saya besarin
    public float lineWidth = 0.1f;       // <--- Ketebalan garis
    public float pulseSpeed = 5f;        // Kecepatan denyut
    public float pulseAmount = 0.2f;     // Seberapa "lebay" denyutnya
    public int segments = 50;            // Kehalusan lingkaran

    [Header("Debug Info (Read Only)")]
    public bool isEnemy = false;
    public bool isRevealed = false;

    private GameObject enemyRootObject;
    private Material heartMat;
    private EnemyHealthTracker trackerScript;
    private LineRenderer lineRenderer;
    private bool isVisionActive = false;
    private Camera mainCam;

    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    void Start()
    {
        if (heartRenderer == null) heartRenderer = GetComponent<Renderer>();
        mainCam = Camera.main;

        // --- SETUP LINE RENDERER ---
        lineRenderer = GetComponent<LineRenderer>();

        // PENTING: Kita pakai World Space biar bisa muter bebas menghadap player
        // tanpa terpengaruh rotasi objek Heart aslinya.
        lineRenderer.useWorldSpace = true;

        lineRenderer.loop = true;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = segments;

        // Alignment View membantu garisnya gepeng ke kamera
        lineRenderer.alignment = LineAlignment.View;

        // Fallback Material (Biar X-Ray Opaque tetap jalan kalau lupa set)
        if (lineRenderer.sharedMaterial == null)
        {
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }

        lineRenderer.enabled = false;

        // --- SETUP MATERIAL HEART ---
        heartMat = heartRenderer.material;
        heartMat.EnableKeyword("_EMISSION");
        SetEmissionColor(false, Color.black, 0);

        trackerScript = FindAnyObjectByType<EnemyHealthTracker>();

        // Logika Deteksi Enemy (Sama seperti sebelumnya)
        if (CompareTag("Enemy"))
        {
            isEnemy = true;
            enemyRootObject = gameObject;
        }
        else
        {
            Transform parentCheck = transform.parent;
            while (parentCheck != null)
            {
                if (parentCheck.CompareTag("Enemy"))
                {
                    isEnemy = true;
                    enemyRootObject = parentCheck.gameObject;
                    break;
                }
                parentCheck = parentCheck.parent;
            }
        }
    }

    void Update()
    {
        // Pastikan update lebar garis kalau diubah di inspector saat play
        if (lineRenderer.enabled)
        {
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            AnimateCircleBillboarding();
        }
    }

    // Fungsi baru: Membuat lingkaran selalu menghadap kamera (Billboard)
    void AnimateCircleBillboarding()
    {
        if (mainCam == null) return;

        float currentRadius = baseRadius + (Mathf.Sin(Time.time * pulseSpeed) * pulseAmount);
        float deltaTheta = (2f * Mathf.PI) / segments;
        float theta = 0f;

        // 1. Cari arah dari Heart ke Kamera
        Vector3 directionToCamera = transform.position - mainCam.transform.position;

        // 2. Bikin rotasi agar lingkaran tegak lurus menghadap kamera
        // Kita pakai LookRotation. Ini membuat sumbu Z lingkaran menunjuk ke kamera.
        Quaternion facingRotation = Quaternion.LookRotation(directionToCamera);

        for (int i = 0; i < segments; i++)
        {
            // Hitung posisi lingkaran dasar (X dan Y)
            float x = currentRadius * Mathf.Cos(theta);
            float y = currentRadius * Mathf.Sin(theta);

            // Posisi titik lokal (Z = 0 karena lingkaran gepeng)
            Vector3 localPoint = new Vector3(x, y, 0f);

            // 3. Gabungkan: Posisi Heart + (Rotasi ke Kamera * Titik Lokal)
            Vector3 worldPoint = transform.position + (facingRotation * localPoint);

            lineRenderer.SetPosition(i, worldPoint);

            theta += deltaTheta;
        }
    }

    public void UpdateVisualState(bool isVisionOn, Color baseColor, float normalIntensity, float visionIntensity)
    {
        isVisionActive = isVisionOn;

        if (!isVisionOn)
        {
            SetEmissionColor(false, baseColor, 0);
            lineRenderer.enabled = false;
            return;
        }

        if (!isEnemy)
        {
            SetEmissionColor(true, baseColor, visionIntensity);
            lineRenderer.enabled = false;
        }
        else
        {
            if (isRevealed)
            {
                SetEmissionColor(true, baseColor, visionIntensity);
                lineRenderer.enabled = false;
            }
            else
            {
                SetEmissionColor(true, baseColor, normalIntensity);
                lineRenderer.enabled = true;
                lineRenderer.colorGradient = targetGradient;
            }
        }
    }

    public void Interact(Color visionColor, float highIntensity)
    {
        if (!isEnemy) return;
        if (isRevealed) return;

        isRevealed = true;
        lineRenderer.enabled = false;
        SetEmissionColor(true, visionColor, highIntensity);

        if (trackerScript != null && enemyRootObject != null)
        {
            trackerScript.ShowHealthForEnemy(enemyRootObject);
        }
    }
    public bool IsLockable()
    {
        // Hanya bisa di-lock jika MUSUH dan SUDAH REVEALED (Scan sukses)
        return isEnemy && isRevealed;
    }
    private void SetEmissionColor(bool enable, Color color, float intensity)
    {
        if (enable)
        {
            Color finalColor = color * intensity;
            heartMat.EnableKeyword("_EMISSION");
            heartMat.SetColor(EmissionColorID, finalColor);
            heartMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
        else
        {
            heartMat.SetColor(EmissionColorID, Color.black);
        }
    }
}