using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(RectTransform), typeof(CanvasGroup), typeof(Image))]
public class Arrow : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float appearDuration = 0.2f;
    [SerializeField] private float reachTargetDuration = 0.8f;
    [SerializeField] private float exitDuration = 0.3f; // Durasi gerak ke exit (dipakai juga saat shake)

    [Header("Visual Feedback Settings")]
    [SerializeField] private Color hitColor = Color.green;
    [SerializeField] private Color missColor = Color.red;
    [SerializeField] private float colorFadeDuration = 0.1f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Image arrowImage;
    private ConductorAttack attackScript;
    private Vector2 startPos;
    private Vector2 targetPos;
    private Vector2 exitPos;

    // Setting Shake (diterima dari Spawner)
    private float shakeDuration;
    private float shakeMagnitude;

    public ArrowDirection Direction { get; private set; }
    public bool IsResolved { get; private set; } = false;
    public event Action OnArrowResolved;

    private Color originalColor;
    private Coroutine mainLifecycleCoroutine;

    public void Initialize(ArrowDirection dir, ConductorAttack conductor, Vector2 currentStartPos, Vector2 currentTargetPos, Vector2 currentExitPos, float shakeDur, float shakeMag)
    {
        Direction = dir;
        attackScript = conductor;
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        arrowImage = GetComponent<Image>();

        startPos = currentStartPos;
        targetPos = currentTargetPos;
        exitPos = currentExitPos;

        // Simpan setting shake
        this.shakeDuration = shakeDur; // Opsional: Bisa dipakai jika ingin durasi getar beda dengan exitDuration
        this.shakeMagnitude = shakeMag;

        originalColor = arrowImage.color;
        canvasGroup.alpha = 0f;
        rectTransform.anchoredPosition = startPos;
        ResetStretch();

        mainLifecycleCoroutine = StartCoroutine(MoveLifecycle());
    }

    private IEnumerator MoveLifecycle()
    {
        // --- 1. APPEAR ---
        float timer = 0f;
        while (timer < appearDuration)
        {
            if (IsResolved) yield break;
            timer += Time.deltaTime;
            float t = timer / appearDuration;
            canvasGroup.alpha = t;
            float stretchFactor = 1f + (1f - t) * 0.5f;
            ApplyStretch(stretchFactor);
            yield return null;
        }
        canvasGroup.alpha = 1f;
        ResetStretch();

        // --- 2. MOVE TO TARGET ---
        timer = 0f;
        while (timer < reachTargetDuration)
        {
            if (IsResolved) yield break;
            timer += Time.deltaTime;
            float t = timer / reachTargetDuration;
            // Easing: Ease In Out Sine
            t = -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f;
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }

        // --- 3. FINAL CHECK ---
        if (IsResolved) yield break;
        rectTransform.anchoredPosition = targetPos;

        // Jika sampai sini belum di-hit, berarti TELAT (Timeout)
        if (!IsResolved)
        {
            // Panggil logika Miss yang sama dengan Wrong Swipe
            TriggerMissBehavior("Timeout");
        }
    }

    /// <summary>
    /// Logika NORMAL saat berhasil (Hit) - Tidak bergetar, warna Hijau
    /// </summary>
    public void ResolveHit()
    {
        if (IsResolved) return;
        IsResolved = true;

        if (mainLifecycleCoroutine != null) StopCoroutine(mainLifecycleCoroutine);

        // Ubah warna Hijau
        StartCoroutine(FadeColor(originalColor, hitColor, colorFadeDuration, () => {
            StartCoroutine(FadeColor(hitColor, originalColor, colorFadeDuration));
        }));

        Debug.Log("Arrow Hit!");

        // Keluar secara halus (tanpa shake)
        StartCoroutine(ExitAndCleanup(exitDuration));
    }

    /// <summary>
    /// Logika saat SALAH SWIPE (Dipanggil dari luar/Conductor)
    /// </summary>
    public void ResolveMiss_WrongSwipe()
    {
        if (IsResolved) return;
        // Panggil logika Miss yang sama
        TriggerMissBehavior("Wrong Swipe");
    }

    /// <summary>
    /// Fungsi Pusat untuk menangani "MISS" (Baik karena Telat maupun Salah Swipe)
    /// </summary>
    private void TriggerMissBehavior(string reason)
    {
        IsResolved = true;

        if (mainLifecycleCoroutine != null) StopCoroutine(mainLifecycleCoroutine);

        attackScript.HitMissed();

        // 1. Ubah warna jadi MERAH
        StartCoroutine(FadeColor(originalColor, missColor, colorFadeDuration));

        Debug.Log($"Arrow Missed! ({reason}) - SHAKE & EXIT");

        // 2. Bergerak ke Exit SAMBIL Bergetar
        StartCoroutine(ShakeAndMoveToExit());
    }

    /// <summary>
    /// Coroutine Keluar Normal (Untuk Hit)
    /// </summary>
    private IEnumerator ExitAndCleanup(float duration)
    {
        float timer = 0f;
        Vector2 currentPosition = rectTransform.anchoredPosition;
        float startAlpha = canvasGroup.alpha;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);

            // Efek Stretch saat keluar
            float stretchFactor = 1f + t * 0.5f;
            ApplyStretch(stretchFactor);

            // Gerak lurus ke exitPos
            rectTransform.anchoredPosition = Vector2.Lerp(currentPosition, exitPos, t);

            yield return null;
        }

        FinalizeArrow();
    }

    /// <summary>
    /// Coroutine Keluar + Getar (Untuk Miss/Timeout)
    /// </summary>
    private IEnumerator ShakeAndMoveToExit()
    {
        float timer = 0f;
        Vector2 startShakePos = rectTransform.anchoredPosition;
        float startAlpha = canvasGroup.alpha;

        // Kita gunakan exitDuration agar konsisten waktunya dengan kecepatan keluar
        while (timer < exitDuration)
        {
            timer += Time.deltaTime;
            float t = timer / exitDuration;

            // 1. Hitung posisi dasar (sedang bergerak ke Exit)
            Vector2 basePosition = Vector2.Lerp(startShakePos, exitPos, t);

            // 2. Hitung Offset Getaran
            float xShake = UnityEngine.Random.Range(-1f, 1f) * shakeMagnitude;
            float yShake = UnityEngine.Random.Range(-1f, 1f) * shakeMagnitude;

            // 3. Gabungkan: Bergerak + Getar
            rectTransform.anchoredPosition = basePosition + new Vector2(xShake, yShake);

            // 4. Fade Out & Stretch
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            float stretchFactor = 1f + t * 0.5f;
            ApplyStretch(stretchFactor);

            yield return null;
        }

        FinalizeArrow();
    }

    private void FinalizeArrow()
    {
        canvasGroup.alpha = 0f;
        OnArrowResolved?.Invoke();
        Destroy(gameObject);
    }

    // --- Helper Functions ---

    private IEnumerator FadeColor(Color startColor, Color endColor, float duration, Action onComplete = null)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            arrowImage.color = Color.Lerp(startColor, endColor, timer / duration);
            yield return null;
        }
        arrowImage.color = endColor;
        onComplete?.Invoke();
    }

    private void ApplyStretch(float factor)
    {
        Vector3 newScale = Vector3.one;
        if (Direction == ArrowDirection.Up || Direction == ArrowDirection.Down)
        {
            newScale.y = factor;
        }
        else
        {
            newScale.x = factor;
        }
        rectTransform.localScale = newScale;
    }

    private void ResetStretch()
    {
        rectTransform.localScale = Vector3.one;
    }
}