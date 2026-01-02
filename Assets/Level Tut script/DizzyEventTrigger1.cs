using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DizzyEventTrigger : MonoBehaviour
{
    [Header("--- KONTROL PEMAIN (WAJIB DIISI) ---")]
    public Transform playerCamera;          
    public MonoBehaviour mouseLookScript;   
    public MonoBehaviour movementScript;    
    public Rigidbody playerRigidbody;       

    [Header("--- UI KEDIP ---")]
    public CanvasGroup blackScreen;         
    public Canvas blackScreenCanvas;        
    public float blinkSpeed = 4.0f;         

    [Header("--- VISUAL DIZZY ---")]
    public Volume dizzyVolume;              
    public CameraShake cameraShake;         
    public Animator playerAnimator;
    
    [Header("--- GERAKAN KEPALA ---")]
    public float lookDownAngle = 60.0f;     
    public float headShakeAmount = 25.0f;   
    public float headShakeSpeed = 3.0f;     
    public float cameraShakePower = 0.1f;   

    [Header("--- DIALOG ---")]
    public GameObject subtitlePanel;
    public TextMeshProUGUI subtitleText;
    [TextArea] public string monologue = "…kepalaku… berat sekali…";

    private bool hasTriggered = false;
    private Quaternion originalRotation;

    void Start()
    {
        if (dizzyVolume != null) dizzyVolume.weight = 0;
        // Tidak mematikan UI di Start (Aman buat Intro)
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasTriggered && (other.CompareTag("Player") || other.transform.root.CompareTag("Player")))
        {
            hasTriggered = true;
            StartCoroutine(RealDizzySequence());
        }
    }

    IEnumerator RealDizzySequence()
    {
        // 1. MATIKAN INPUT
        if (mouseLookScript != null) mouseLookScript.enabled = false;
        if (movementScript != null) movementScript.enabled = false; 

        if (playerCamera != null) originalRotation = playerCamera.localRotation;

        // 2. MULAI EFEK
        if (playerAnimator != null) playerAnimator.SetTrigger("Headache");
        if (cameraShake != null) StartCoroutine(cameraShake.Shake(6.0f, cameraShakePower));
        StartCoroutine(PulseVolumeEffect(6.0f));

        // --- FASE 1: NUNDUK ---
        float timer = 0;
        float duration = 0.8f;
        float startY = originalRotation.eulerAngles.y; 

        while (timer < duration)
        {
            timer += Time.deltaTime;
            
            // REM PAKSA (Biar gak jalan)
            if (playerRigidbody != null) 
            {
                playerRigidbody.linearVelocity = Vector3.zero;
                playerRigidbody.angularVelocity = Vector3.zero;
            }

            if (playerCamera != null)
            {
                Quaternion target = Quaternion.Euler(lookDownAngle, startY, 0);
                playerCamera.localRotation = Quaternion.Slerp(originalRotation, target, timer / duration);
            }
            yield return null;
        }

        // --- FASE 2: KEDIP (MEREM) ---
        int oldOrder = 0;
        if (blackScreenCanvas != null) 
        {
            oldOrder = blackScreenCanvas.sortingOrder;
            blackScreenCanvas.sortingOrder = 999; 
        }

        yield return StartCoroutine(DoFullBlinkCycle());

        // --- FASE 3: GELENG (VISIBLE) ---
        if (subtitlePanel != null)
        {
            subtitleText.text = "<i>" + monologue + "</i>";
            subtitlePanel.SetActive(true);
        }

        timer = 0;
        float dizzyDuration = 3.5f; 

        while (timer < dizzyDuration)
        {
            timer += Time.deltaTime;
            // Rem Paksa
            if (playerRigidbody != null) playerRigidbody.linearVelocity = Vector3.zero;

            if (playerCamera != null)
            {
                float gelengY = Mathf.Sin(Time.time * headShakeSpeed) * headShakeAmount; 
                float miringZ = Mathf.Cos(Time.time * headShakeSpeed) * (headShakeAmount * 0.5f); 
                
                playerCamera.localRotation = Quaternion.Euler(lookDownAngle, startY + gelengY, miringZ);
            }
            yield return null;
        }

        // --- FASE 4: KEDIP LAGI (MELEK) ---
        if (subtitlePanel != null) subtitlePanel.SetActive(false);
        yield return StartCoroutine(DoFullBlinkCycle());

        // --- FASE 5: PULIH (RECOVERY) ---
        timer = 0;
        duration = 1.5f;
        Quaternion currentRot = playerCamera.localRotation;
        Quaternion finalRot = Quaternion.Euler(0, startY, 0); 

        while (timer < duration)
        {
            timer += Time.deltaTime;
            if (playerCamera != null)
            {
                playerCamera.localRotation = Quaternion.Slerp(currentRot, finalRot, timer / duration);
            }
            yield return null;
        }

        // ============================================================
        // 🔥 ANTI-MIRING / HARD RESET (Z-AXIS KILLER) 🔥
        // ============================================================
        
        // 1. Reset Rotasi Kamera (Paksa Tegak)
        if (playerCamera != null)
        {
            // Ambil arah hadap (Y) terakhir, tapi paksa X=0 dan Z=0
            Vector3 camEuler = playerCamera.localEulerAngles;
            playerCamera.localEulerAngles = new Vector3(0f, camEuler.y, 0f);
        }

        // 2. Reset Rotasi Badan Player (PENTING JIKA BADAN MIRING)
        // Kita cari transform induk (Player Body) dari movement script atau rigidbody
        Transform playerBody = null;
        if (movementScript != null) playerBody = movementScript.transform;
        else if (playerRigidbody != null) playerBody = playerRigidbody.transform;

        if (playerBody != null)
        {
            // Matikan putaran fisika sisa
            if (playerRigidbody != null) playerRigidbody.angularVelocity = Vector3.zero;

            // Paksa badan tegak lurus lantai
            Vector3 bodyEuler = playerBody.eulerAngles;
            playerBody.rotation = Quaternion.Euler(0f, bodyEuler.y, 0f);
        }
        // ============================================================

        // Kembalikan Kontrol
        if (mouseLookScript != null) mouseLookScript.enabled = true;
        if (movementScript != null) movementScript.enabled = true;
        if (blackScreenCanvas != null) blackScreenCanvas.sortingOrder = oldOrder;

        Destroy(gameObject);
    }

    IEnumerator DoFullBlinkCycle()
    {
        if (blackScreen == null) yield break;

        blackScreen.gameObject.SetActive(true);

        float t = 0;
        while (t < 1.0f) { t += Time.deltaTime * blinkSpeed; blackScreen.alpha = t; yield return null; }
        blackScreen.alpha = 1;

        yield return new WaitForSeconds(0.2f);

        t = 0;
        while (t < 1.0f) { t += Time.deltaTime * blinkSpeed; blackScreen.alpha = 1.0f - t; yield return null; }
        blackScreen.alpha = 0;
        blackScreen.gameObject.SetActive(false);
    }

    IEnumerator PulseVolumeEffect(float duration)
    {
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            if (dizzyVolume != null)
                dizzyVolume.weight = Mathf.PingPong(Time.time * 2.5f, 0.5f) + 0.2f; 
            yield return null;
        }
        if (dizzyVolume != null) { while (dizzyVolume.weight > 0) { dizzyVolume.weight -= Time.deltaTime; yield return null; } }
    }
}