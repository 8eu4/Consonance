using UnityEngine;
using TMPro;
using System.Collections;

public class CinematicText : MonoBehaviour
{
    [Header("Component")]
    public TMP_Text uiText; // Masukkan objek TextMeshPro di sini

    [Header("Settings")]
    public float typingSpeed = 0.05f; // Kecepatan ngetik (makin kecil makin cepat)
    public float floatSpeed = 2.0f;   // Kecepatan teks melayang naik
    public float startDelay = 1.5f;   // Waktu tunggu sebelum teks muncul
    public float destroyTime = 8.0f;  // Berapa lama teks tampil sebelum hilang

    [TextArea(3, 5)]
    public string content = "Tekan [F] untuk toggle antara Follow / Wait.\n" +
                            "Tahan [F] dan gerakan Mouse untuk memerintahkan Remi berhenti di titik tertentu.";

    private void Start()
    {
        if (uiText != null)
        {
            uiText.text = ""; // Kosongkan teks di awal
            StartCoroutine(ShowTextRoutine());
        }
    }

    private void Update()
    {
        // Efek Melayang Perlahan ke Atas
        if (uiText != null)
        {
            uiText.transform.position += Vector3.up * floatSpeed * Time.deltaTime;
        }
    }

    IEnumerator ShowTextRoutine()
    {
        // Tunggu sebentar pas awal scene (biar player nafas dulu)
        yield return new WaitForSeconds(startDelay);

        // Efek Ngetik (Typewriter)
        foreach (char letter in content.ToCharArray())
        {
            uiText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        // Tunggu beberapa detik, lalu hilangkan pelan-pelan (Fade Out)
        yield return new WaitForSeconds(destroyTime);
        yield return StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        float startAlpha = uiText.alpha;
        float duration = 2.0f;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            uiText.alpha = Mathf.Lerp(startAlpha, 0, time / duration);
            yield return null;
        }
        
        // Hapus objek/matikan teks setelah hilang biar bersih
        uiText.gameObject.SetActive(false);
    }
}