using UnityEngine;
using TMPro;
using UnityEngine.UI; 
using UnityEngine.SceneManagement; 
using System.Collections;

public class RemiReunionCutscene : MonoBehaviour
{
    [Header("--- UI SETTINGS ---")]
    public CanvasGroup blackScreen;   
    public GameObject subPanel;       
    public TextMeshProUGUI subText;   
    
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
    public MonoBehaviour[] scriptsToDisable; // <--- INI SOLUSI BIAR GAK GERAK

    [Header("--- AUDIO ---")]
    public AudioSource audioSource;
    public AudioClip harpSound;

    [Header("--- NEXT SCENE ---")]
    public string nextSceneName; 

    private bool sequenceStarted = false;

    void Start()
    {
        if (blackScreen != null) { blackScreen.alpha = 0; blackScreen.gameObject.SetActive(false); }
        if (subPanel != null) subPanel.SetActive(false);
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

        // --- STEP 1: REMI SAMPAI ---
        if (subPanel != null) subPanel.SetActive(true);
        subText.text = "Remi sampai.\nTekan <color=yellow>[1]</color> Kembali ke Conductor";

        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Alpha1));

        // --- MULAI CUTSCENE: MATIKAN SEMUA GERAKAN ---
        DisableAllMovement(true); // <--- KUNCI GERAK DISINI

        // --- STEP 2: FADE OUT (GELAP) ---
        subText.text = ""; 
        subPanel.SetActive(false);
        
        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(true);
            float t = 0;
            while (t < 1f) { t += Time.deltaTime * 2; blackScreen.alpha = t; yield return null; }
            blackScreen.alpha = 1;
        }

        // --- STEP 3: PINDAHKAN POSISI & BEKUKAN FISIKA ---
        
        // Pindahkan Conductor
        TeleportActor(conductor, conductorSpot);
        
        // Pindahkan Domi
        TeleportActor(domi, domiSpot);
        
        // Pindahkan Remi
        TeleportActor(remi, remiSpot);
        
        // Pindahkan Kamera
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

        // --- STEP 5: DIALOG REMI ---
        if (subPanel != null) subPanel.SetActive(true);
        if (audioSource != null && harpSound != null) audioSource.PlayOneShot(harpSound);

        subText.text = "<color=#FFAAAA>Remi:</color>\n\"Kau… kau menyelamatkanku. \nAku akan ikut bersamamu.\"";

        yield return new WaitForSeconds(4.0f); 

        // --- STEP 6: PROMPT ---
        subText.text = ""; 
        yield return new WaitForSeconds(0.5f);
        
        subText.fontSize = subText.fontSize * 0.8f; 
        subText.fontStyle = FontStyles.Italic;
        subText.text = "Ikuti <color=yellow>(E)</color>";

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

    // --- FUNGSI PEMBANTU (BIAR GAK TENGGELAM) ---
    void TeleportActor(Transform actor, Transform spot)
    {
        if (actor == null || spot == null) return;

        // 1. Matikan CharacterController (Penyebab utama nyangkut)
        CharacterController cc = actor.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        // 2. Matikan Rigidbody Physic (Biar gak jatuh ke bumi)
        Rigidbody rb = actor.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true; 

        // 3. Pindahkan
        actor.position = spot.position;
        actor.rotation = spot.rotation;
    }

    // --- FUNGSI PEMBANTU (BIAR GAK GERAK) ---
    void DisableAllMovement(bool disable)
    {
        // Matikan script yang didaftarkan di inspector
        foreach (var script in scriptsToDisable)
        {
            if (script != null) script.enabled = !disable;
        }
    }
}