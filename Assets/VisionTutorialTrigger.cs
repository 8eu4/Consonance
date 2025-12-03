using UnityEngine;
using UnityEngine.Rendering; // Wajib ada untuk Post Processing
using TMPro;
using System.Collections; // Perlu ini untuk Coroutine (Durasi teks)

public class VisionTutorialTrigger : MonoBehaviour
{
    [Header("UI Settings")]
    public GameObject tutorialPromptUI; // UI Text "Tekan [V]..."
    
    [Header("Vision Effects")]
    public Volume visionVolume;         // Drag "VisionVolume" ke sini
    public GameObject targetHighlight;  // Drag efek cahaya di Muse ke sini
    public float effectSpeed = 2.0f;    // Kecepatan transisi layar

    [Header("Camera Cutscene")]
    public GameObject mainCamera;       // Kamera FPS Player (yang nempel di player)
    public Transform museFocusPoint;    // Object KOSONG di seberang jurang, tempat kamera akan bergerak (bukan kamera lagi)
    public GameObject playerObject;     // Drag Player (untuk mematikan gerak sementara, opsional)
    public float cameraMoveDuration = 2.0f; // Durasi pergerakan kamera ke Muse

    [Header("Blink Effect")]
    public CanvasGroup blinkCanvasGroup; // Canvas Group (Panel Hitam full layar) untuk efek kedip
    public float blinkDuration = 0.5f;   // Durasi kedip

    [Header("Monologue Settings")]
    public GameObject subtitlePanel;       // Drag Subtitle Panel (Background Hitam)
    public TextMeshProUGUI subtitleText;   // Drag Text Subtitle
    [TextArea]
    public string monologueLine = "Ohh aku tahu cahaya itu… Resonant Core";
    public float textDuration = 4.0f;      // Berapa lama teks & kamera sorot muncul

    [Header("Settings")]
    public KeyCode visionKey = KeyCode.V;
    public bool destroyAfterTrigger = true;

    private bool playerInside = false;
    private bool tutorialCompleted = false;
    private bool isVisionActive = false;
    private Transform originalCameraParent; // Untuk menyimpan parent asli kamera

    void Start()
    {
        if (tutorialPromptUI != null) tutorialPromptUI.SetActive(false);
        if (visionVolume != null) visionVolume.weight = 0; 
        if (targetHighlight != null) targetHighlight.SetActive(false);
        if (subtitlePanel != null) subtitlePanel.SetActive(false);
        
        // [FIX] Jangan matikan blinkCanvasGroup di sini!
        // Karena panel ini dipakai bersamaan dengan PrologueDirector saat start.
        // Biarkan PrologueDirector yang mengontrol kondisi awalnya.
        /* if (blinkCanvasGroup != null) {
            blinkCanvasGroup.alpha = 0f; 
            blinkCanvasGroup.gameObject.SetActive(false); 
        } 
        */

        // Pastikan kita punya referensi yang benar
        if (mainCamera == null) Debug.LogError("LUPA MASUKIN 'Main Camera' DI INSPECTOR VISION TRIGGER!");
        if (museFocusPoint == null) Debug.LogError("LUPA MASUKIN 'Muse Focus Point' DI INSPECTOR VISION TRIGGER! Ganti MuseCam jadi object kosong biasa.");
    }

    void Update()
    {
        // Logika Trigger
        if (playerInside && !tutorialCompleted)
        {
            if (Input.GetKeyDown(visionKey))
            {
                ActivateVisionMode();
            }
        }

        // Animasi Smooth Perubahan Layar
        if (visionVolume != null)
        {
            float targetWeight = isVisionActive ? 1.0f : 0.0f;
            visionVolume.weight = Mathf.Lerp(visionVolume.weight, targetWeight, Time.deltaTime * effectSpeed);
        }
    }

    void ActivateVisionMode()
    {
        tutorialCompleted = true;
        isVisionActive = true; 

        // 1. Sembunyikan UI Prompt "Tekan V"
        if (tutorialPromptUI != null) tutorialPromptUI.SetActive(false);

        // 2. Nyalakan Highlight Muse
        if (targetHighlight != null) targetHighlight.SetActive(true);

        // 3. Matikan Trigger
        if (destroyAfterTrigger) GetComponent<Collider>().enabled = false;

        // 4. Mulai Cutscene (Kamera Gerak & Dialog & Blink)
        StartCoroutine(PlayCutsceneSequence());

        Debug.Log("VISION MODE ON: Layar berubah, kamera bergerak ke Muse!");
    }

    IEnumerator PlayCutsceneSequence()
    {
        // --- 0. PERSIAPAN CUTSCENE ---
        // Tunggu sebentar untuk transisi efek layar
        yield return new WaitForSeconds(0.5f);

        // Matikan Gerakan Player
        MonoBehaviour[] scripts = null;
        if (playerObject != null)
        {
            scripts = playerObject.GetComponentsInChildren<MonoBehaviour>();
            foreach(var script in scripts) 
            {
                // Matikan script controller/movement/kamera rotation
                if (script.GetType().Name.Contains("Controller") || script.GetType().Name.Contains("Move") || script.GetType().Name.Contains("Rotation") || script.GetType().Name.Contains("Look"))
                    script.enabled = false; 
            }
        }

        // Simpan posisi & parent asli kamera
        Vector3 startPos = mainCamera.transform.position; // Posisi Dunia (untuk Lerp)
        Quaternion startRot = mainCamera.transform.rotation;
        
        // [PERBAIKAN] Simpan posisi LOKAL juga. Ini kuncinya!
        // Supaya nanti pas balik, kita reset ke 0,0,0 relatif terhadap CameraHolder
        Vector3 startLocalPos = mainCamera.transform.localPosition;
        Quaternion startLocalRot = mainCamera.transform.localRotation;
        
        originalCameraParent = mainCamera.transform.parent;

        // Lepaskan kamera dari parentnya (biar bisa gerak bebas)
        mainCamera.transform.parent = null;


        // --- 1. KAMERA BERGERAK KE MUSE (LINEAR) ---
        float time = 0;
        while (time < cameraMoveDuration)
        {
            time += Time.deltaTime;
            float t = time / cameraMoveDuration;
            // Pakai SmoothStep biar gerakannya lebih halus (ada ease-in ease-out)
            float smoothT = Mathf.SmoothStep(0, 1, t); 

            mainCamera.transform.position = Vector3.Lerp(startPos, museFocusPoint.position, smoothT);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, museFocusPoint.rotation, smoothT);

            yield return null;
        }
        // Pastikan posisi akhir pas di target
        mainCamera.transform.position = museFocusPoint.position;
        mainCamera.transform.rotation = museFocusPoint.rotation;


        // --- 2. TAMPILKAN DIALOG DI MUSE ---
        if (subtitlePanel != null && subtitleText != null)
        {
            subtitleText.text = "<i>" + monologueLine + "</i>"; 
            subtitlePanel.SetActive(true);
        }

        // --- TUNGGU DURASI ---
        yield return new WaitForSeconds(textDuration);


        // --- 3. EFEK KEDIP (BLINK) UNTUK KEMBALI ---
        // A. Tutup Mata (Fade Out ke Hitam)
        if (blinkCanvasGroup != null)
        {
            blinkCanvasGroup.gameObject.SetActive(true);
            float blinkTime = 0;
            while(blinkTime < blinkDuration)
            {
                blinkTime += Time.deltaTime;
                blinkCanvasGroup.alpha = Mathf.Lerp(0f, 1f, blinkTime / blinkDuration);
                yield return null;
            }
            blinkCanvasGroup.alpha = 1f;
        }

        // B. Kembalikan Posisi Kamera (Saat Gelap)
        // Sembunyikan dialog dulu
        if (subtitlePanel != null) subtitlePanel.SetActive(false);
        if (subtitleText != null) subtitleText.text = "";
        
        // [PERBAIKAN BUG KAMERA MUNDUR/NEMBUS]
        // 1. Tempelkan dulu ke parent aslinya (CameraHolder)
        mainCamera.transform.parent = originalCameraParent;
        
        // 2. Reset posisi LOKAL-nya ke awal (biasanya 0,0,0)
        // Ini menjamin kamera pas lagi di leher/kepala, gak peduli rotasi badannya gimana
        mainCamera.transform.localPosition = startLocalPos;
        mainCamera.transform.localRotation = startLocalRot;

        // Tunggu sebentar saat gelap (opsional, biar kerasa kedipnya)
        yield return new WaitForSeconds(0.1f);


        // C. Buka Mata (Fade In dari Hitam)
        if (blinkCanvasGroup != null)
        {
            float blinkTime = 0;
            while(blinkTime < blinkDuration)
            {
                blinkTime += Time.deltaTime;
                blinkCanvasGroup.alpha = Mathf.Lerp(1f, 0f, blinkTime / blinkDuration);
                yield return null;
            }
            blinkCanvasGroup.alpha = 0f;
            blinkCanvasGroup.gameObject.SetActive(false);
        }


        // --- 4. SELESAI CUTSCENE ---
        // Kembalikan Gerakan Player
        if (playerObject != null && scripts != null)
        {
            foreach(var script in scripts) 
            {
                if (script.GetType().Name.Contains("Controller") || script.GetType().Name.Contains("Move") || script.GetType().Name.Contains("Rotation") || script.GetType().Name.Contains("Look"))
                    script.enabled = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((other.CompareTag("Player") || other.transform.root.CompareTag("Player")) && !tutorialCompleted)
        {
            playerInside = true;
            if (tutorialPromptUI != null) tutorialPromptUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((other.CompareTag("Player") || other.transform.root.CompareTag("Player")) && !tutorialCompleted)
        {
            playerInside = false;
            if (tutorialPromptUI != null) tutorialPromptUI.SetActive(false);
        }
    }
}