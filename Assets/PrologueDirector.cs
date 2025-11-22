using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class PrologueDirector : MonoBehaviour
{
    [Header("UI & Audio")]
    public CanvasGroup introCanvasGroup;
    public TextMeshProUGUI textNarator;
    public AudioSource audioSource;
    public AudioClip voiceClip;
    public AudioClip sfxRuntuhan;

    [Header("Scene References")]
    public GameObject introCamera;
    public GameObject playerObject;
    
    // Pastikan tipe datanya sesuai script kamu
    public SimpleFPSController playerControllerScript; 
    public Animator playerCameraAnimator;

    void Start()
    {
        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        // --- SETUP AWAL ---
        playerObject.SetActive(false); 
        introCamera.SetActive(true);   
        introCanvasGroup.alpha = 1;    
        textNarator.text = "Echo Lantern dahulu adalah sebuah kota harmoni...";

        // --- NARASI ---
        if (voiceClip != null)
        {
            audioSource.clip = voiceClip;
            audioSource.Play();
            yield return new WaitForSeconds(voiceClip.length + 1f);
        }
        else
        {
            yield return new WaitForSeconds(4f);
        }

        // --- THE SWITCH (CRITICAL PART) ---
        
        // 1. Matikan Kamera Intro
        introCamera.SetActive(false); 

        // 2. Matikan Script FPS DULUAN (Lewat referensi, walau objeknya mati)
        if (playerControllerScript != null) playerControllerScript.enabled = false;

        // 3. Baru Nyalakan Player
        playerObject.SetActive(true); 

        // 4. PAKSA Animasi Ulang (Supaya override posisi kamera script FPS)
        if (playerCameraAnimator != null)
        {
            playerCameraAnimator.enabled = true;
            playerCameraAnimator.Play("WakeUp", 0, 0f); // Paksa mulai dari detik 0
        }
        
        // --- FADE OUT ---
        float t = 0;
        while (t < 2f)
        {
            t += Time.deltaTime;
            introCanvasGroup.alpha = Mathf.Lerp(1, 0, t / 2f);
            yield return null;
        }
        introCanvasGroup.gameObject.SetActive(false);

        // --- FINAL CLEANUP (Masalah Tubuh Bawah Gerak) ---
        
        // Tunggu sebentar biar aman
        yield return new WaitForSeconds(0.5f); 

        // 1. MATIKAN ANIMATOR SECARA PAKSA (Solusi Leher Kaku)
        if (playerCameraAnimator != null) 
        {
            playerCameraAnimator.enabled = false; // Matikan komponennya
            // Opsional: Reset rotasi biar lurus pas mouse ambil alih
            // playerCameraAnimator.transform.localRotation = Quaternion.identity; 
        }

        // 2. HIDUPKAN Script FPS (Solusi bisa jalan lagi)
        if (playerControllerScript != null) playerControllerScript.enabled = true;

        // SFX
        if (sfxRuntuhan != null) audioSource.PlayOneShot(sfxRuntuhan);
    }
}