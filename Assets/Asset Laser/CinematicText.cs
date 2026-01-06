using UnityEngine;
using TMPro;
using System.Collections;

public class CinematicText : MonoBehaviour
{
    [Header("Component")]
    public TMP_Text uiText; 

    [Header("--- FREEZE CONTROLS ---")]
    [Tooltip("Masukkan Script Gerak Conductor di sini (misal: CharacterMovement/PlayerController)")]
    public MonoBehaviour movementScript; 
    
    [Tooltip("Masukkan Script Kamera/MouseLook di sini (biasanya ada di Main Camera atau Player)")]
    public CamRotation cameraLookScript;

    [Header("Settings")]
    public float typingSpeed = 0.05f; 
    public float floatSpeed = 0.5f;   // Saya turunkan dikit biar ga ngebut ke atas
    public float startDelay = 1.0f;   
    public float destroyTime = 6.0f;  

    [TextArea(3, 5)]
    public string content = "Tekan [F] untuk toggle antara Follow / Wait.\n" +
                            "Tahan [F] dan gerakan Mouse untuk memerintahkan Muse berhenti di titik tertentu.";

    private void Start()
    {
        // 1. MATIKAN KONTROL SAAT MULAI
        if (movementScript != null) movementScript.enabled = false;
        if (cameraLookScript != null) cameraLookScript.enabled = false;

        if (uiText != null)
        {
            uiText.text = ""; 
            StartCoroutine(ShowTextRoutine());
        }
    }

    private void Update()
    {
        if (uiText != null)
        {
            uiText.transform.position += Vector3.up * floatSpeed * Time.deltaTime;
        }
    }

    IEnumerator ShowTextRoutine()
    {
        yield return new WaitForSeconds(startDelay);

        // Ngetik...
        foreach (char letter in content.ToCharArray())
        {
            uiText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        // Teks sudah lengkap, tunggu player baca sebentar
        yield return new WaitForSeconds(destroyTime);

        // 2. NYALAKAN KEMBALI KONTROL (Player bisa gerak lagi)
        if (movementScript != null) movementScript.enabled = true;
        if (cameraLookScript != null) cameraLookScript.enabled = true;

        // Fade out teks
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
        
        uiText.gameObject.SetActive(false);
    }
}