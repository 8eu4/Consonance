using UnityEngine;
using UnityEngine.UI;
using TMPro; // Jika ada teks di dalam bukunya
using System.Collections;

public class GuidebookTrigger : MonoBehaviour
{
    [Header("Guidebook UI")]
    public GameObject guidePanel;       // Panel Gambar Lembar Musik (floating UI)
    public CanvasGroup guideCanvasGroup; // Untuk efek Fade In/Out halus
    public float fadeDuration = 1.0f;

    [Header("Audio Guide")]
    public AudioSource audioSource;
    public AudioClip guideVoiceClip;    // Audio: "Aktifkan Resonant Core..."
    [Range(0f, 1f)] public float volume = 1.0f;

    [Header("Configuration")]
    public bool freezePlayer = false;   // Apakah player berhenti saat baca panduan?
    public GameObject playerObject;     // Drag Player jika freezePlayer = true
    public float extraReadingTime = 2.0f; // Waktu tambahan setelah suara selesai sebelum nutup

    [Header("References (Optional)")]
    // Hanya untuk referensi visual di scene view, tidak wajib diisi script
    public Transform domiPosition; // Posisi Domi (Sisi Kita)
    public Transform remiPosition; // Posisi Remi (Seberang)

    private bool hasTriggered = false;

    void Start()
    {
        // Pastikan UI mati & transparan di awal
        if (guidePanel != null) 
        {
            guidePanel.SetActive(false);
            if (guideCanvasGroup != null) guideCanvasGroup.alpha = 0f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Cek Player (atau parentnya jika collider ada di child)
        if (!hasTriggered && (other.CompareTag("Player") || other.transform.root.CompareTag("Player")))
        {
            StartCoroutine(ShowGuideSequence());
        }
    }

    IEnumerator ShowGuideSequence()
    {
        hasTriggered = true;

        // 1. Matikan Gerakan Player (Opsional, biar fokus dengar)
        MonoBehaviour[] scripts = null;
        if (freezePlayer && playerObject != null)
        {
            scripts = playerObject.GetComponentsInChildren<MonoBehaviour>();
            foreach(var script in scripts) 
            {
                // Matikan controller, movement, camera look
                if (script.GetType().Name.Contains("Controller") || script.GetType().Name.Contains("Move") || script.GetType().Name.Contains("Look"))
                    script.enabled = false; 
            }
        }

        // 2. Munculkan UI (Fade In)
        if (guidePanel != null)
        {
            guidePanel.SetActive(true);
            
            // Animasi Fade In
            if (guideCanvasGroup != null)
            {
                float timer = 0;
                while (timer < fadeDuration)
                {
                    timer += Time.deltaTime;
                    guideCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
                    yield return null;
                }
                guideCanvasGroup.alpha = 1f;
            }
        }

        // 3. Mainkan Audio Guide
        float waitDuration = 3.0f; // Default kalau gak ada audio
        if (audioSource != null && guideVoiceClip != null)
        {
            audioSource.PlayOneShot(guideVoiceClip, volume);
            waitDuration = guideVoiceClip.length;
        }

        // Tunggu suara selesai + waktu baca ekstra
        yield return new WaitForSeconds(waitDuration + extraReadingTime);

        // 4. Sembunyikan UI (Fade Out)
        if (guideCanvasGroup != null)
        {
            float timer = 0;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                guideCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
                yield return null;
            }
            guideCanvasGroup.alpha = 0f;
        }
        if (guidePanel != null) guidePanel.SetActive(false);

        // 5. Kembalikan Gerakan Player
        if (freezePlayer && playerObject != null && scripts != null)
        {
            foreach(var script in scripts) 
            {
                if (script.GetType().Name.Contains("Controller") || script.GetType().Name.Contains("Move") || script.GetType().Name.Contains("Look"))
                    script.enabled = true;
            }
        }

        // Matikan Trigger ini
        GetComponent<Collider>().enabled = false;
    }
}