using UnityEngine;
using TMPro;
using System.Collections;

public class TutorialDomi_Interactive : MonoBehaviour
{
    [Header("Referensi Object")]
    public GameObject tutorialPanel; 
    public TextMeshProUGUI tutorialText; 
    
    public Transform playerCamera;   // Kamera Player (buat nunduk)
    public Transform handObject;     // Object Tangan/Senjata/Badan yang mau diangkat
    
    [Header("Referensi Script (Untuk Lock Gerak)")]
    public MonoBehaviour[] scriptsToDisable; // Masukkan Move & CamRotation biar player gabisa lari pas cutscene

    [Header("Settings Animasi Tangan")]
    public Vector3 handInspectOffset = new Vector3(-0.2f, -0.3f, 0.4f); // Posisi tangan pas diangkat (relatif ke kamera)
    public float lookDownAngle = 15f; // Seberapa nunduk kameranya
    public float animSpeed = 2.0f;    // Kecepatan animasi

    [Header("Settings Tutorial")]
    public string playerTag = "Player";
    
    private Vector3 initialHandPos;
    private Quaternion initialHandRot;
    private Quaternion initialCamRot;
    private bool hasTriggered = false;

    void Start()
    {
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        GameObject hitObject = other.gameObject;
        if (other.attachedRigidbody != null) hitObject = other.attachedRigidbody.gameObject;

        if (hitObject.CompareTag(playerTag) || hitObject.name.Contains("Conductor"))
        {
            hasTriggered = true;
            StartCoroutine(PlaySequence());
        }
    }

    IEnumerator PlaySequence()
    {
        // 1. Kunci Pergerakan Player
        foreach (var script in scriptsToDisable) if (script != null) script.enabled = false;
        
        // Simpan posisi awal biar bisa balikin nanti
        if (handObject != null)
        {
            initialHandPos = handObject.localPosition;
            initialHandRot = handObject.localRotation;
        }
        if (playerCamera != null) initialCamRot = playerCamera.localRotation;

        // --- FASE 1: ANIMASI TANGAN (INSPECT) ---
        
        float t = 0;
        
        // A. Angkat Tangan & Nunduk
        while (t < 1f)
        {
            t += Time.deltaTime * animSpeed;
            float smooth = Mathf.SmoothStep(0, 1, t);

            // Kamera Nunduk Dikit
            if (playerCamera != null)
                playerCamera.localRotation = Quaternion.Slerp(initialCamRot, initialCamRot * Quaternion.Euler(lookDownAngle, 0, 0), smooth);

            // Tangan Naik ke Depan Muka
            if (handObject != null)
            {
                // Kita pindahkan tangan ke posisi depan kamera
                // Note: Ini asumsi handObject adalah anak dari kamera/player yg ikut rotasi
                handObject.localPosition = Vector3.Lerp(initialHandPos, initialHandPos + handInspectOffset, smooth);
                
                // Putar tangan dikit biar kayak ngelihat telapak
                handObject.localRotation = Quaternion.Slerp(initialHandRot, initialHandRot * Quaternion.Euler(0, 0, 45f), smooth);
            }
            yield return null;
        }

        // B. Tahan Sebentar (Lihat Tangan)
        if (tutorialPanel != null) tutorialPanel.SetActive(true);
        tutorialText.text = "<i>Tangannya terangkat perlahan...</i>";
        yield return new WaitForSeconds(2.0f);

        // C. Balik-balik Tangan (Animasi Muter)
        t = 0;
        Quaternion inspectRotStart = handObject.localRotation;
        Quaternion inspectRotEnd = inspectRotStart * Quaternion.Euler(0, 180, 0); // Putar 180 derajat
        
        while (t < 1f)
        {
            t += Time.deltaTime * (animSpeed * 0.5f); // Pelan dikit
            if (handObject != null)
                handObject.localRotation = Quaternion.Slerp(inspectRotStart, inspectRotEnd, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }
        
        tutorialText.text = "<i>...seolah merasakan getaran musik.</i>";
        yield return new WaitForSeconds(1.5f);

        // D. Kembalikan Tangan & Kamera (Balik FPP Normal)
        t = 0;
        Vector3 currentHandPos = handObject.localPosition;
        Quaternion currentHandRot = handObject.localRotation;
        Quaternion currentCamRot = playerCamera.localRotation;

        while (t < 1f)
        {
            t += Time.deltaTime * animSpeed;
            float smooth = Mathf.SmoothStep(0, 1, t);

            if (playerCamera != null) playerCamera.localRotation = Quaternion.Slerp(currentCamRot, initialCamRot, smooth);
            if (handObject != null)
            {
                handObject.localPosition = Vector3.Lerp(currentHandPos, initialHandPos, smooth);
                handObject.localRotation = Quaternion.Slerp(currentHandRot, initialHandRot, smooth);
            }
            yield return null;
        }

        // Pastikan posisi presisi balik ke awal (biar ga bug aim)
        if (handObject != null) { handObject.localPosition = initialHandPos; handObject.localRotation = initialHandRot; }
        if (playerCamera != null) playerCamera.localRotation = initialCamRot;

        // Buka Kunci Gerak
        foreach (var script in scriptsToDisable) if (script != null) script.enabled = true;
        // Khusus CamRotation kita sync ulang
        foreach (var script in scriptsToDisable) if (script is CamRotation c) c.UpdateOrientation();


        // --- FASE 2: TUTORIAL INPUT (SAMA KAYAK TADI) ---
        
        tutorialText.text = "Tekan <color=yellow>[2]</color> \nAmbil kendali Muse";
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Alpha2));

        tutorialText.text = "Klik <color=yellow>[LMB]</color> \nNembak String line";
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

        tutorialText.text = "Tekan <color=yellow>[R]</color> \nReload";
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.R));

        tutorialText.text = "Gunakan String Line \nSambungkan line ke sebuah permukaan.";
        yield return new WaitForSeconds(4.0f);

        // Selesai
        tutorialText.text = "";
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        Destroy(gameObject);
    }
}