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
    [SerializeField] private float exitDuration = 0.3f; // Durasi keluar normal (untuk Hit atau Telat)

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

    public ArrowDirection Direction { get; private set; }
    public bool IsResolved { get; private set; } = false;
    public event Action OnArrowResolved;

    private Color originalColor;
    private Coroutine mainLifecycleCoroutine;

    public void Initialize(ArrowDirection dir, ConductorAttack conductor, Vector2 currentStartPos, Vector2 currentTargetPos, Vector2 currentExitPos)
    {
        Direction = dir;
        attackScript = conductor;
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        arrowImage = GetComponent<Image>();

        startPos = currentStartPos;
        targetPos = currentTargetPos;
        exitPos = currentExitPos;

        originalColor = arrowImage.color;
        canvasGroup.alpha = 0f;
        rectTransform.anchoredPosition = startPos;
        ResetStretch();

        mainLifecycleCoroutine = StartCoroutine(MoveLifecycle());
    }

    /// <summary>
    /// Coroutine yang HANYA mengelola pergerakan dan mendeteksi "miss" jika lolos
    /// </summary>
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
            t = -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f;
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }

        // --- 3. FINAL CHECK (DIPINDAHKAN KE SINI) ---
        if (IsResolved) yield break; // Cek lagi jika di-hit di frame terakhir
        rectTransform.anchoredPosition = targetPos; // Pastikan di posisi target

        // Jika kita sampai di sini (sudah di targetPos) dan BELUM di-hit,
        // itu adalah 'miss' karena telat.
        if (!IsResolved)
        {
            // Panggil 'miss' yang normal/lambat
            // ResolveMiss_Timeout() akan otomatis memanggil ExitAndCleanup()
            // yang akan menggerakkannya ke exitPos sambil berubah merah.
            ResolveMiss_Timeout();
        }

        // --- 4. FASE "MOVE TO EXIT" YANG LAMA DIHAPUS ---
        // Logika "Move to Exit" (baris 99-108 di file Anda) dan "Final Check" (baris 111-115)
        // tidak lagi diperlukan di sini, karena sudah ditangani oleh ResolveMiss_Timeout().
    }

    /// <summary>
    /// Coroutine yang mengelola fade-out, stretch-out, dan cleanup.
    /// </summary>
    private IEnumerator ExitAndCleanup(float duration)
    {
        // --- 1. EXIT ANIMATION (Fade-out & Stretch-out) ---
        float timer = 0f;
        Vector2 currentPosition = rectTransform.anchoredPosition;
        float startAlpha = canvasGroup.alpha;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            float stretchFactor = 1f + t * 0.5f;
            ApplyStretch(stretchFactor);
            rectTransform.anchoredPosition = Vector2.Lerp(currentPosition, exitPos, t);

            yield return null;
        }

        // --- 2. FINAL CLEANUP ---
        canvasGroup.alpha = 0f;
        OnArrowResolved?.Invoke();
        Destroy(gameObject);
    }

    /// <summary>
    /// Dipanggil oleh ConductorAttack jika player BERHASIL
    /// </summary>
    public void ResolveHit()
    {
        if (IsResolved) return;
        IsResolved = true;

        if (mainLifecycleCoroutine != null)
            StopCoroutine(mainLifecycleCoroutine);

        // Visual feedback (berubah jadi hijau lalu kembali)
        StartCoroutine(FadeColor(originalColor, hitColor, colorFadeDuration, () => {
            StartCoroutine(FadeColor(hitColor, originalColor, colorFadeDuration));
        }));

        Debug.Log("Arrow Hit!");

        // Mulai cleanup dengan durasi normal
        StartCoroutine(ExitAndCleanup(exitDuration));
    }

    /// <summary>
    /// Dipanggil secara internal jika waktu habis (player TELAT)
    /// </summary>
    private void ResolveMiss_Timeout()
    {
        if (IsResolved) return;
        IsResolved = true;

        attackScript.HitMissed();

        // Visual feedback (berubah jadi merah DAN TETAP MERAH)
        StartCoroutine(FadeColor(originalColor, missColor, colorFadeDuration));

        Debug.Log("Arrow Missed! (Timeout)");

        // Mulai cleanup dengan durasi normal
        StartCoroutine(ExitAndCleanup(exitDuration));
    }

    /// <summary>
    /// Dipanggil oleh ConductorAttack jika player SWIPE SALAH
    /// </summary>
    public void ResolveMiss_WrongSwipe()
    {
        if (IsResolved) return;
        IsResolved = true;

        if (mainLifecycleCoroutine != null)
            StopCoroutine(mainLifecycleCoroutine);

        attackScript.HitMissed();

        // Visual feedback (berubah jadi merah DAN TETAP MERAH)
        StartCoroutine(FadeColor(originalColor, missColor, colorFadeDuration));

        Debug.Log("Arrow Missed! (Wrong Swipe)");

        // Mulai cleanup dengan durasi CEPAT
        // Anda lupa mengganti ini di file Anda, saya perbaiki:
        StartCoroutine(ExitAndCleanup(exitDuration));
    }


    // --- (Sisa skrip: FadeColor, ApplyStretch, ResetStretch tetap sama) ---
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