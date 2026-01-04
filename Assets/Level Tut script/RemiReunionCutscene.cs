using UnityEngine;
using TMPro;
using UnityEngine.UI; 
using UnityEngine.SceneManagement; 
using System.Collections;

public class RemiReunionCutscene : MonoBehaviour
{
    [Header("--- UI SETTINGS ---")]
    public CanvasGroup blackScreen;   
    public GameObject subPanel;      // ✅ KEMBALIKAN UI PANEL (Drag SubPanel disini)
    public TextMeshProUGUI subText;  // ✅ KEMBALIKAN UI TEXT (Drag SubText disini)
    
    [Header("--- ACTORS (KARAKTER) ---")]
    public Transform conductor;
    public Transform domi;
    public Transform remi; 

    [Header("--- POSISI REUNION (TITIK KUMPUL) ---")]
    public Transform conductorSpot; 
    public Transform domiSpot;
    public Transform remiSpot;

    [Header("--- KAMERA ---")]
    public Transform mainCamera;      
    public Transform cameraCloseUpSpot; 

    [Header("--- SCRIPT PENGGANGGU (WAJIB ISI) ---")]
    [Tooltip("Masukkan script 'Move', 'MouseLook', 'SwitchCharacter' disini biar mati pas cutscene")]
    public MonoBehaviour[] scriptsToDisable; 

    [Header("--- AUDIO ---")]
    public AudioSource audioSource;
    public AudioClip harpSound;

    [Header("--- NEXT SCENE ---")]
    public string nextSceneName; 

    private bool sequenceStarted = false;

    void Start()
    {
        if (blackScreen != null) { blackScreen.alpha = 0; blackScreen.gameObject.SetActive(false); }
        if (subPanel != null) subPanel.SetActive(false); // Sembunyikan panel di awal
    }

    void OnTriggerEnter(Collider other)
    {
        if (sequenceStarted) return;

        if (other.transform.root.name.Contains("Remi"))
        {
            StartCoroutine(StartSequence());
        }
    }

    IEnumerator StartSequence()
    {
        sequenceStarted = true;

        // --- STEP 1: REMI SAMPAI (PROMPT [1] MUNCUL) ---
        if (subPanel != null) subPanel.SetActive(true); // ✅ NYALAKAN PANEL
        subText.text = "Remi sampai.\nTekan <color=yellow>[1]</color> Kembali ke Conductor"; // ✅ TAMPILKAN INSTRUKSI

        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Alpha1));

        // --- MULAI CUTSCENE ---
        DisableAllMovement(true);

        // --- STEP 2: FADE OUT (GELAP) ---
        subText.text = ""; 
        if (subPanel != null) subPanel.SetActive(false); // ❌ SEMBUNYIKAN PANEL SAAT TRANSISI
        
        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(true);
            float t = 0;
            while (t < 1f) { t += Time.deltaTime * 2; blackScreen.alpha = t; yield return null; }
            blackScreen.alpha = 1;
        }

        // --- STEP 3: PINDAHKAN POSISI ---
        TeleportActor(conductor, conductorSpot);
        TeleportActor(domi, domiSpot);
        TeleportActor(remi, remiSpot);
        
        if (mainCamera != null) 
        { 
            mainCamera.position = cameraCloseUpSpot.position; 
            mainCamera.rotation = cameraCloseUpSpot.rotation; 
        }

        yield return new WaitForSeconds(1.0f); 

        // --- STEP 4: FADE IN ---
        if (blackScreen != null)
        {
            float t = 0;
            while (t < 1f) { t += Time.deltaTime * 1; blackScreen.alpha = 1 - t; yield return null; }
            blackScreen.alpha = 0;
        }

        // --- STEP 5: DIALOG REMI (HANYA SUARA, TANPA TEKS) ---
        if (audioSource != null && harpSound != null) audioSource.PlayOneShot(harpSound);

        // ❌ JANGAN TAMPILKAN SUBTITLE CERITA, BIARKAN LAYAR BERSIH
        // subText.text = "Bla bla bla..."; (INI DIHAPUS)

        yield return new WaitForSeconds(4.0f); 

        // --- STEP 6: PROMPT LANJUT (PROMPT [E] MUNCUL) ---
        if (subPanel != null) subPanel.SetActive(true); // ✅ NYALAKAN PANEL LAGI
        
        subText.fontSize = subText.fontSize * 0.8f; 
        subText.fontStyle = FontStyles.Italic;
        subText.text = "Ikuti <color=yellow>(E)</color>"; // ✅ TAMPILKAN INSTRUKSI

        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.E));

        // --- STEP 7: AKHIR ---
        if (blackScreen != null)
        {
            float t = 0;
            while (t < 1f) { t += Time.deltaTime * 1; blackScreen.alpha = t; yield return null; }
            blackScreen.alpha = 1;
        }

        Debug.Log("🚀 Pindah Scene...");
        SceneManager.LoadScene(nextSceneName);
    }

    // --- FUNGSI PEMBANTU ---
    void TeleportActor(Transform actor, Transform spot)
    {
        if (actor == null || spot == null) return;
        CharacterController cc = actor.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        Rigidbody rb = actor.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true; 

        actor.position = spot.position;
        actor.rotation = spot.rotation;
    }

    void DisableAllMovement(bool disable)
    {
        foreach (var script in scriptsToDisable)
        {
            if (script != null) script.enabled = !disable;
        }
    }
}