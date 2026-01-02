using UnityEngine;
using UnityEngine.Rendering; 
using TMPro;
using System.Collections; 

public class VisionTutorialTrigger : MonoBehaviour
{
    [Header("PENTING: Masukkan Script Jalan Disini")]
    public MonoBehaviour playerMovementScript; 
    public MonoBehaviour cameraLookScript;

    [Header("UI Settings")]
    public GameObject tutorialPromptUI; 
    
    [Header("Vision Effects")]
    public Volume visionVolume;         
    public GameObject targetHighlight;  
    public float effectSpeed = 2.0f;    

    [Header("Camera Cutscene")]
    public GameObject mainCamera;       
    public Transform museFocusPoint;    
    public GameObject playerObject;     
    public float cameraMoveDuration = 2.0f; 
    public float delayBeforeZoom = 2.0f; 

    [Header("Blink Effect")]
    public CanvasGroup blinkCanvasGroup; 
    public float blinkDuration = 0.5f;   

    [Header("Monologue Settings")]
    public GameObject subtitlePanel;       
    public TextMeshProUGUI subtitleText;   
    [TextArea] public string monologueLine = "Ohh aku tahu cahaya itu… Resonant Core";
    public float textDuration = 4.0f;      

    [Header("Settings")]
    public KeyCode visionKey = KeyCode.V;
    public bool destroyAfterTrigger = true;

    private bool playerInside = false;
    private bool tutorialCompleted = false;
    private bool isVisionActive = false; 
    
    private bool isFrozen = false; 
    private Vector3 lockedPosition; 

    private Transform originalCameraParent; 

    void Start()
    {
        // Setup awal UI & Volume
        if (tutorialPromptUI != null) tutorialPromptUI.SetActive(false);
        if (visionVolume != null) visionVolume.weight = 0; 
        if (targetHighlight != null) targetHighlight.SetActive(false);
        if (subtitlePanel != null) subtitlePanel.SetActive(false);

        // Auto-detect script player
        if (playerMovementScript == null && playerObject != null)
        {
            MonoBehaviour[] scripts = playerObject.GetComponentsInChildren<MonoBehaviour>();
            foreach (var s in scripts) {
                if (s.GetType().Name.Contains("Command") || s.GetType().Name.Contains("System")) 
                    playerMovementScript = s;
            }
        }
    }

    void Update()
    {
        // --- JURUS PAKU BUMI (FORCE POSITION) ---
        if (isFrozen && playerObject != null)
        {
            playerObject.transform.position = lockedPosition;
            Rigidbody rb = playerObject.GetComponent<Rigidbody>();
            if(rb) 
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero; // Matikan putaran fisika
            }
        }

        // 1. Fase Sebelum Vision (Nunggu Input V)
        if (playerInside && !isVisionActive && !tutorialCompleted)
        {
            // Kamera nengok NPC pelan-pelan
            if (museFocusPoint != null && mainCamera != null)
            {
                Vector3 directionToNPC = museFocusPoint.position - mainCamera.transform.position;
                if (directionToNPC != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToNPC);
                    mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, targetRotation, Time.deltaTime * 5f);
                }
            }

            if (Input.GetKeyDown(visionKey)) ActivateVisionMode();
        }

        // 2. Animasi Volume Vision
        if (visionVolume != null)
        {
            float targetWeight = isVisionActive ? 1.0f : 0.0f;
            visionVolume.weight = Mathf.Lerp(visionVolume.weight, targetWeight, Time.deltaTime * effectSpeed);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((other.CompareTag("Player") || other.transform.root.CompareTag("Player")) && !tutorialCompleted)
        {
            playerInside = true;
            if (tutorialPromptUI != null) tutorialPromptUI.SetActive(true);

            // Set Player Object kalau belum ada
            if (playerObject == null) playerObject = other.transform.root.gameObject;

            lockedPosition = playerObject.transform.position;
            isFrozen = true;
            TogglePlayerControl(false); 
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((other.CompareTag("Player") || other.transform.root.CompareTag("Player")) && !tutorialCompleted)
        {
            playerInside = false;
            if (tutorialPromptUI != null) tutorialPromptUI.SetActive(false);
            isFrozen = false;
            TogglePlayerControl(true); 
        }
    }

    void ActivateVisionMode()
    {
        tutorialCompleted = true; 
        isVisionActive = true;    

        if (tutorialPromptUI != null) tutorialPromptUI.SetActive(false);
        if (targetHighlight != null) targetHighlight.SetActive(true);
        if (destroyAfterTrigger) GetComponent<Collider>().enabled = false;

        StartCoroutine(PlayCutsceneSequence());
    }

    IEnumerator PlayCutsceneSequence()
    {
        yield return new WaitForSeconds(delayBeforeZoom);

        // --- MULAI CUTSCENE ---
        // Simpan posisi & rotasi awal
        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;
        Vector3 startLocalPos = mainCamera.transform.localPosition;
        
        // Simpan rotasi lokal awal buat dikembalikan nanti
        Vector3 startLocalEuler = mainCamera.transform.localEulerAngles;
        
        originalCameraParent = mainCamera.transform.parent;
        mainCamera.transform.parent = null; // Lepas kamera dari player

        // ZOOM IN
        float time = 0;
        while (time < cameraMoveDuration)
        {
            time += Time.deltaTime;
            float smoothT = Mathf.SmoothStep(0, 1, time / cameraMoveDuration); 

            mainCamera.transform.position = Vector3.Lerp(startPos, museFocusPoint.position, smoothT);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, museFocusPoint.rotation, smoothT);
            yield return null;
        }
        mainCamera.transform.position = museFocusPoint.position;
        mainCamera.transform.rotation = museFocusPoint.rotation;

        // DIALOG
        if (subtitlePanel != null && subtitleText != null)
        {
            subtitleText.text = "<i>" + monologueLine + "</i>"; 
            subtitlePanel.SetActive(true);
        }

        yield return new WaitForSeconds(textDuration);

        // BLINK MEREM
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

        // RESET UI & STATE
        if (subtitlePanel != null) subtitlePanel.SetActive(false);
        if (subtitleText != null) subtitleText.text = "";
        
        // --- KEMBALIKAN KAMERA KE PEMAIN ---
        mainCamera.transform.parent = originalCameraParent;
        mainCamera.transform.localPosition = startLocalPos;
        
        // BALIKKAN ROTASI KAMERA (TAPI PAKSA TEGAK)
        // Kita pakai rotasi awal, tapi Z-nya kita matikan total (0)
        mainCamera.transform.localRotation = Quaternion.Euler(startLocalEuler.x, startLocalEuler.y, 0f);

        // --- [FIX MIRING PALING PENTING] LURUSKAN BADAN PLAYER ---
        if (playerObject != null)
        {
            // Ambil rotasi badan saat ini
            Vector3 currentBodyRot = playerObject.transform.eulerAngles;
            // Paksa X=0 dan Z=0 (Tegak Lurus Lantai), pertahankan Y (Arah Hadap)
            playerObject.transform.rotation = Quaternion.Euler(0f, currentBodyRot.y, 0f);
            
            // Matikan sisa gaya putar di Rigidbody
            Rigidbody rb = playerObject.GetComponent<Rigidbody>();
            if(rb != null)
            {
                rb.angularVelocity = Vector3.zero;
                // KUNCI ROTASI AGAR TIDAK TERGULING LAGI
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY; 
                // Note: FreezePositionY opsional, bisa dihapus kalau mau loncat
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            }
        }

        yield return new WaitForSeconds(0.1f); // Tunggu fisika stabil

        // BLINK MELEK
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

        // SELESAI
        isFrozen = false; 
        TogglePlayerControl(true); 
    }

    void TogglePlayerControl(bool enable)
    {
        if (playerMovementScript != null) playerMovementScript.enabled = enable;
        if (cameraLookScript != null) cameraLookScript.enabled = enable;

        if (playerObject != null)
        {
            Rigidbody rb = playerObject.GetComponent<Rigidbody>();
            if (rb == null) rb = playerObject.GetComponentInChildren<Rigidbody>();

            if (rb != null) 
            {
                if (!enable) 
                {
                    // MATIKAN FISIKA SAAT FREEZE
                    rb.linearVelocity = Vector3.zero; 
                    rb.angularVelocity = Vector3.zero; 
                    rb.isKinematic = true; 
                }
                else 
                {
                    // NYALAKAN LAGI
                    rb.isKinematic = false; 
                    rb.WakeUp();
                    
                    // --- SAFETY NET TERAKHIR ---
                    // Paksa tegak sekali lagi saat kontrol dikembalikan
                    rb.angularVelocity = Vector3.zero;
                    Vector3 upright = playerObject.transform.eulerAngles;
                    playerObject.transform.rotation = Quaternion.Euler(0f, upright.y, 0f);
                }
            }
        }
        
        // Stop Animasi
        Animator anim = playerObject.GetComponent<Animator>();
        if (anim == null) anim = playerObject.GetComponentInChildren<Animator>();
        if (anim != null && !enable) {
             anim.SetFloat("Speed", 0f); 
             anim.Play("Idle"); 
        }
    }
}