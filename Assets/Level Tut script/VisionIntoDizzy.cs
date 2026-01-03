using UnityEngine;
using TMPro;
using System.Collections;

public class VisionIntoDizzy : MonoBehaviour
{
    [Header("--- KONTROL PEMAIN (AUTO DETECT) ---")]
    public GameObject playerObject;      
    public GameObject mainCamera;        
    private MonoBehaviour playerMovementScript; 
    private MonoBehaviour cameraLookScript;
    private Rigidbody playerRb;

    [Header("--- SETTING VISION (FASE 1) ---")]
    public KeyCode visionKey = KeyCode.V;
    public Transform museFocusPoint;     // Titik cahaya/Core yang dilihat
    public GameObject targetHighlight;   // (Opsional) Penanda target
    public float cameraZoomDuration = 1.5f;

    [Header("--- SETTING DIZZY (FASE 2) ---")]
    public float dizzyLookDownAngle = 45.0f; // Seberapa nunduk pas pusing
    public float dizzyDuration = 4.0f;

    [Header("--- UI & DIALOG ---")]
    public GameObject subtitlePanel;     
    public TextMeshProUGUI subtitleText; 
    public CanvasGroup blackScreen;      // Layar Hitam (CanvasGroup)
    
    [TextArea] public string dialogVision = "Ohh aku tahu cahaya itu… Resonant Core.";
    [TextArea] public string dialogDizzy = "Agh... kepalaku... berat sekali...";

    // Internal State
    private bool playerInside = false;
    private bool eventStarted = false;
    private Transform originalCamParent;
    private Vector3 originalCamLocalPos;

    void Start()
    {
        if (subtitlePanel) subtitlePanel.SetActive(false);
        if (targetHighlight) targetHighlight.SetActive(false);
        if (blackScreen) { blackScreen.alpha = 0; blackScreen.gameObject.SetActive(false); }
    }

    void Update()
    {
        // Menunggu input V saat di dalam area
        if (playerInside && !eventStarted)
        {
            // Auto look sedikit ke arah target sebelum tekan V (Opsional, biar halus)
            if (museFocusPoint && mainCamera)
            {
                Vector3 dir = museFocusPoint.position - mainCamera.transform.position;
                Quaternion targetRot = Quaternion.LookRotation(dir);
                mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, targetRot, Time.deltaTime * 2f);
            }

            if (Input.GetKeyDown(visionKey))
            {
                StartCoroutine(FullCinematicSequence());
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (eventStarted) return;
        
        if (other.CompareTag("Player") || other.transform.root.CompareTag("Player"))
        {
            playerInside = true;
            // Cari komponen player otomatis
            if (playerObject == null) playerObject = other.transform.root.gameObject;
            playerRb = playerObject.GetComponent<Rigidbody>();
            
            // Cari script movement (Move & MouseLook)
            MonoBehaviour[] scripts = playerObject.GetComponentsInChildren<MonoBehaviour>();
            foreach (var s in scripts) {
                string name = s.GetType().Name;
                if (name.Contains("Move") || name.Contains("Controller") || name.Contains("Input")) playerMovementScript = s;
                if (name.Contains("Look") || name.Contains("Mouse") || name.Contains("Cam")) cameraLookScript = s;
            }

            ToggleControls(false); // Bekukan Player
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!eventStarted && (other.CompareTag("Player") || other.transform.root.CompareTag("Player")))
        {
            playerInside = false;
            ToggleControls(true);
        }
    }

    IEnumerator FullCinematicSequence()
    {
        eventStarted = true;
        
        GetComponent<Collider>().enabled = false;
        if (targetHighlight) targetHighlight.SetActive(true);

        // ==============================================================
        // 🎬 FASE 1: VISION (ZOOM KE MUSE)
        // ==============================================================
        
        // Detach Kamera
        originalCamParent = mainCamera.transform.parent;
        originalCamLocalPos = mainCamera.transform.localPosition;
        Quaternion startRot = mainCamera.transform.rotation;
        Vector3 startPos = mainCamera.transform.position;

        mainCamera.transform.parent = null; 

        // 1. Zoom In
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime / cameraZoomDuration;
            float smooth = Mathf.SmoothStep(0, 1, t);
            mainCamera.transform.position = Vector3.Lerp(startPos, museFocusPoint.position, smooth);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, museFocusPoint.rotation, smooth);
            yield return null;
        }

        // 2. Dialog
        ShowSubtitle(dialogVision);
        yield return new WaitForSeconds(3.0f);
        HideSubtitle();

        // 3. Zoom Out (Balik ke Kepala)
        t = 0;
        Vector3 currentCamPos = mainCamera.transform.position;
        Quaternion currentCamRot = mainCamera.transform.rotation;
        Transform headTarget = originalCamParent != null ? originalCamParent : playerObject.transform;

        while (t < 1f)
        {
            t += Time.deltaTime / (cameraZoomDuration * 0.8f);
            float smooth = Mathf.SmoothStep(0, 1, t);
            
            mainCamera.transform.position = Vector3.Lerp(currentCamPos, headTarget.position + originalCamLocalPos, smooth);
            Quaternion targetLook = Quaternion.LookRotation(playerObject.transform.forward);
            mainCamera.transform.rotation = Quaternion.Slerp(currentCamRot, targetLook, smooth);
            
            yield return null;
        }

        // Attach Ulang Kamera
        mainCamera.transform.parent = originalCamParent;
        mainCamera.transform.localPosition = originalCamLocalPos;
        mainCamera.transform.localRotation = Quaternion.identity;

        // ==============================================================
        // 😵 FASE 2: DIZZY (GOYANG MANUAL)
        // ==============================================================

        // Dialog Pusing
        ShowSubtitle("<i>" + dialogDizzy + "</i>");
        
        float dizzyTimer = 0;
        
        // Kita simulasikan pusing dengan menggoyangkan rotasi kamera via script (Tanpa Animator)
        while (dizzyTimer < dizzyDuration)
        {
            dizzyTimer += Time.deltaTime;

            // Target nunduk bertahap
            float currentX = Mathf.Lerp(0, dizzyLookDownAngle, Mathf.PingPong(dizzyTimer * 0.5f, 1)); 
            
            // Efek Mabuk (Geleng-geleng & Miring dikit) - Manual Math
            float wobbleY = Mathf.Sin(Time.time * 2.0f) * 5.0f; 
            float wobbleZ = Mathf.Cos(Time.time * 1.5f) * 5.0f; 

            mainCamera.transform.localRotation = Quaternion.Euler(currentX, wobbleY, wobbleZ);
            
            if (playerRb) { 
                playerRb.linearVelocity = Vector3.zero; 
                playerRb.angularVelocity = Vector3.zero; 
            }

            yield return null;
        }
        HideSubtitle();

        // Kedip Mata
        yield return StartCoroutine(BlinkEffect());

        // ==============================================================
        // 🛠️ FASE 3: RECOVERY & ANTI-MIRING
        // ==============================================================

        // STEP 1: Matikan Fisika Putaran Total
        if (playerRb) {
            playerRb.angularVelocity = Vector3.zero;
            playerRb.constraints = RigidbodyConstraints.FreezeRotation; 
        }

        // STEP 2: Luruskan Badan & Kamera (Hard Reset)
        Vector3 currentBodyEuler = playerObject.transform.eulerAngles;
        playerObject.transform.rotation = Quaternion.Euler(0f, currentBodyEuler.y, 0f);
        mainCamera.transform.localRotation = Quaternion.Euler(0f, 0f, 0f); 

        yield return new WaitForSeconds(0.2f); 

        // Kembalikan Constraint Fisika
        if (playerRb) {
            playerRb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        ToggleControls(true);
        if (targetHighlight) targetHighlight.SetActive(false); 
        Destroy(gameObject, 1.0f); 
    }

    // --- FUNGSI PENDUKUNG ---

    void ToggleControls(bool state)
    {
        if (playerMovementScript) playerMovementScript.enabled = state;
        if (cameraLookScript) cameraLookScript.enabled = state;
        
        if (!state && playerRb)
        {
            playerRb.linearVelocity = Vector3.zero;
            playerRb.isKinematic = true; 
        }
        else if (state && playerRb)
        {
            playerRb.isKinematic = false; 
            playerRb.WakeUp();
        }
    }

    IEnumerator BlinkEffect()
    {
        if (!blackScreen) yield break;
        blackScreen.gameObject.SetActive(true);
        float t = 0;
        while(t < 1f) { t += Time.deltaTime * 3f; blackScreen.alpha = t; yield return null; }
        blackScreen.alpha = 1;
        yield return new WaitForSeconds(0.5f);
        t = 0;
        while(t < 1f) { t += Time.deltaTime * 2f; blackScreen.alpha = 1 - t; yield return null; }
        blackScreen.alpha = 0;
        blackScreen.gameObject.SetActive(false);
    }

    void ShowSubtitle(string text)
    {
        if (subtitlePanel && subtitleText)
        {
            subtitlePanel.SetActive(true);
            subtitleText.text = text;
        }
    }

    void HideSubtitle()
    {
        if (subtitlePanel) subtitlePanel.SetActive(false);
    }
}