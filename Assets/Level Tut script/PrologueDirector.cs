using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Rendering; 
using System.Collections;

public class PrologueDirector : MonoBehaviour
{
    [Header("UI Settings")]
    public CanvasGroup introCanvasGroup;
    public TextMeshProUGUI textNarator;
    public TextMeshProUGUI skipTooltipText; 
    
    [Header("Dialogue & Subtitle")]
    public GameObject subtitlePanel;     
    public TextMeshProUGUI subtitleText; 

    [Header("Text Speed")]
    public float typingSpeed = 0.08f; 
    public float readingTime = 3.0f; 
    public float textFadeSpeed = 3.0f; 

    [Header("Story Content 📜")]
    [TextArea(3, 10)]
    public string[] narrationLines; 

    [Header("Audio")]
    public AudioSource audioSource;     
    public AudioSource museAudioSource;
    public AudioSource musicSource;
    public AudioClip voiceClip; 
    public AudioClip sfxRuntuhan;
    public AudioClip sfxConductorGasp; 
    public AudioClip voiceMuseHelp;
    public AudioClip diManaAku;
    public AudioClip actualDiManaAku;
    public AudioClip tolongAku;

    [Header("Final World Movement")]
    public Transform worldObjectToMove;
    public Vector3 moveOffsetUp = new Vector3(0, 3f, 0);
    public float worldMoveDuration = 2.0f;



    [Header("Visual FX")]
    public Volume dizzyVolume; 

    [Header("Scene References")]
    public GameObject introCamera;
    public GameObject playerObject;

    // 🔥 INI BARIS BARU: Tempat menaruh script Move & CamRotation temanmu
    public MonoBehaviour[] scriptsToDisable; 
    
    // [UNITY 6 SUPPORT] Menggunakan Rigidbody
    public Rigidbody playerRigid; 
    
    public Transform playerCameraTransform;      

    [Header("Settings")]
    public float startLookAngle = -70f; 

    [Header("Humanize Details")]
    public float sitOvershoot = 10f;       
    public float headShakeAmount = 3f;     
    public float standLookDownAngle = 25f; 
    public float wobbleIntensity = 2.0f;  

    [Header("Animation Timing")]
    public float lieDownDuration = 1.0f; 
    public float sitUpDuration = 3.5f;   
    public float lookAroundDuration = 3.0f; 
    public float prepareDuration = 0.5f;
    public float standUpDuration = 2.5f; 

    private Vector3 standPos;
    private Quaternion standRot;
    private bool isSkipping = false; 

    void Start()
    {
        if (playerCameraTransform != null)
        {
            standPos = playerCameraTransform.localPosition;
            standRot = playerCameraTransform.localRotation;
        }
        if (subtitlePanel != null) subtitlePanel.SetActive(false);
        if (dizzyVolume != null) dizzyVolume.weight = 0;

        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        // ============================================================
        // FASE 1: NARASI KERTAS
        // ============================================================
        
        // Setup Awal
        playerObject.SetActive(false);
        introCamera.SetActive(true);
        introCanvasGroup.alpha = 1; 
        textNarator.text = ""; 
        textNarator.alpha = 1; 

        if (skipTooltipText != null) { skipTooltipText.alpha = 1; skipTooltipText.gameObject.SetActive(true); }
        if (voiceClip != null) { audioSource.clip = voiceClip; audioSource.Play(); }

        foreach (string sentence in narrationLines)
        {
            textNarator.text = "";
            textNarator.alpha = 1;
            isSkipping = false;

            // Muncul per huruf
            char[] characters = sentence.ToCharArray(); 

            foreach (char letter in characters)
            {
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) isSkipping = true;
                
                if (isSkipping) { 
                    textNarator.text = sentence; 
                    break; 
                }

                textNarator.text += letter;
                
                float waitTimer = 0;
                while (waitTimer < typingSpeed) {
                    waitTimer += Time.deltaTime;
                    if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) { isSkipping = true; break; }
                    yield return null;
                }
            }
            if (isSkipping) yield return null; 

            float readTimer = 0;
            while (readTimer < readingTime) {
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) break;
                readTimer += Time.deltaTime;
                yield return null;
            }

            float fadeT = 0;
            while (fadeT < 1f) {
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) fadeT += Time.deltaTime * 10f;
                else fadeT += Time.deltaTime * textFadeSpeed;
                textNarator.alpha = Mathf.Lerp(1, 0, fadeT);
                yield return null;
            }
            yield return new WaitForSeconds(0.5f);
        }

        if (skipTooltipText != null) skipTooltipText.gameObject.SetActive(false);
        textNarator.text = ""; 

        // ============================================================
        // 👁️ FASE 2: THE AWAKENING
        // ============================================================
        
        playerObject.SetActive(true);
        
        // 🔥 Script Move & CamRotation dimatikan dulu
        foreach (var script in scriptsToDisable) 
        {
            if (script != null) script.enabled = false;
        }

        // Matikan Rigidbody (Jadi Patung)
        if (playerRigid != null)
        {
            playerRigid.isKinematic = true; 
            #if UNITY_6000_0_OR_NEWER
            playerRigid.linearVelocity = Vector3.zero; 
            #else
            playerRigid.velocity = Vector3.zero;
            #endif
        }
        
        Vector3 liePos = new Vector3(0, 0.6f, 0); 
        Quaternion lieRot = Quaternion.Euler(startLookAngle, 0, 0); 

        if (playerCameraTransform != null) {
            playerCameraTransform.localPosition = liePos;
            playerCameraTransform.localRotation = lieRot;
        }
        yield return null;
        musicSource.Play();
        introCamera.SetActive(false);

        // --- 1. BUKA MATA DIKIT (SAMAR) ---
        float blinkT = 0;
        while(blinkT < 1f)
        { 
            PlayDialogue(diManaAku);
            blinkT += Time.deltaTime * 0.5f; 
            introCanvasGroup.alpha = Mathf.Lerp(1f, 0.4f, blinkT);
           
            if (playerCameraTransform != null) { playerCameraTransform.localPosition = liePos; playerCameraTransform.localRotation = lieRot; }
            yield return null;
        }

        // --- 2. MONOLOG BISIKAN (SUDAH DIHAPUS SUBTITLENYA) ---
        // Kita matikan UI-nya, tapi Audio Gasp tetap jalan biar kerasa kagetnya
        if (sfxConductorGasp != null) audioSource.PlayOneShot(sfxConductorGasp);
        
        /* ❌ SUBTITLE DIHAPUS SESUAI REQUEST
        if (subtitlePanel != null)
        {
            subtitlePanel.SetActive(true);
            subtitleText.text = "<i>...di mana aku...?</i>";
        }
        */
        
        yield return new WaitForSeconds(2.0f);

        // --- 3. TUTUP MATA (PUSING) ---
        blinkT = 0;
        while(blinkT < 1f)
        {
            blinkT += Time.deltaTime * 1.5f; 
            introCanvasGroup.alpha = Mathf.Lerp(0.4f, 1f, blinkT); 
            if (playerCameraTransform != null) { playerCameraTransform.localPosition = liePos; playerCameraTransform.localRotation = lieRot; }
            yield return null;
        }
        
        // subtitleText.text = ""; // Tidak perlu reset karena tidak muncul

        yield return new WaitForSeconds(1.0f);

        // --- 4. TERIAKAN MUSE (TRIGGER) (SUDAH DIHAPUS SUBTITLENYA) ---
        if (museAudioSource != null) 
        {
            if (voiceMuseHelp != null) museAudioSource.PlayOneShot(voiceMuseHelp);
            else museAudioSource.Play(); 
        }

        /* ❌ SUBTITLE DIHAPUS SESUAI REQUEST
        if (subtitlePanel != null)
        {
            subtitlePanel.SetActive(true);
            subtitleText.text = "<color=#FFD700>Muse:</color> Tolong aku! Di seberang jurang!";
        }
        */

        // --- 4.5 KEDIP REFLEKS ---
        blinkT = 0;
        while(blinkT < 1f)
        {
            blinkT += Time.deltaTime * 2.5f; 
            introCanvasGroup.alpha = Mathf.Lerp(1f, 0.3f, blinkT);
            if (playerCameraTransform != null) { playerCameraTransform.localPosition = liePos; playerCameraTransform.localRotation = lieRot; }
            yield return null;
        }
        yield return new WaitForSeconds(0.15f); 
        blinkT = 0;
        while(blinkT < 1f)
        {
            blinkT += Time.deltaTime * 2.5f; 
            introCanvasGroup.alpha = Mathf.Lerp(0.3f, 1f, blinkT);
            if (playerCameraTransform != null) { playerCameraTransform.localPosition = liePos; playerCameraTransform.localRotation = lieRot; }
            yield return null;
        }

        // --- 5. BUKA MATA TOTAL ---
        blinkT = 0;
        while(blinkT < 1f)
        {
            blinkT += Time.deltaTime * 0.6f; 
            float smoothBlink = Mathf.SmoothStep(0, 1, blinkT);
            introCanvasGroup.alpha = Mathf.Lerp(1f, 0f, smoothBlink);
            if (playerCameraTransform != null) { playerCameraTransform.localPosition = liePos; playerCameraTransform.localRotation = lieRot; }
            yield return null;
        }
        introCanvasGroup.gameObject.SetActive(false);

        yield return new WaitForSeconds(1.0f); 
        // if (subtitlePanel != null) subtitlePanel.SetActive(false); // Tidak perlu karena tidak pernah aktif

        // ============================================================
        // 😵‍💫 FASE 3: BANGUN DUDUK
        // ============================================================
        Vector3 sitPos = new Vector3(0, 0.8f, 0); 
        Quaternion sitRot = Quaternion.identity; 

        float time = 0;
        while (time < sitUpDuration)
        {
            time += Time.deltaTime;
            float t = time / sitUpDuration;
            float smoothT = Mathf.SmoothStep(0, 1, t); 

            playerCameraTransform.localPosition = Vector3.Lerp(liePos, sitPos, smoothT);

            float currentOvershoot = 0;
            if (t > 0.5f) currentOvershoot = (t - 0.5f) * 2f * sitOvershoot * (1f - t); 
            
            Quaternion baseRot = Quaternion.Slerp(lieRot, sitRot, smoothT);
            playerCameraTransform.localRotation = baseRot * Quaternion.Euler(currentOvershoot, 0, 0);

            if (dizzyVolume != null)
            {
                float dizzyWeight = Mathf.Sin(t * Mathf.PI) * 1f; 
                dizzyVolume.weight = dizzyWeight;
            }
            yield return null;
        }
        if (dizzyVolume != null) dizzyVolume.weight = 0;

        // ============================================================
        // 🔍 FASE 4: CELINGUKAN
        // ============================================================
        Quaternion startLookingRot = playerCameraTransform.localRotation;
        time = 0;
        while (time < lookAroundDuration)
        {
            time += Time.deltaTime;
            float t = time / lookAroundDuration;
            float yaw = Mathf.Sin(time * 2f) * 40f; 
            float pitchNoise = Mathf.PerlinNoise(time * 1.5f, 0) * headShakeAmount - (headShakeAmount/2);
            float rollNoise = Mathf.PerlinNoise(0, time * 1.5f) * headShakeAmount - (headShakeAmount/2);
            Quaternion targetLookRot = Quaternion.Euler(pitchNoise, yaw, rollNoise);
            float blendFactor = Mathf.Clamp01(time / 1.0f);
            PlayDialogue(actualDiManaAku);
            playerCameraTransform.localRotation = Quaternion.Slerp(startLookingRot, targetLookRot, blendFactor);
            float breathY = Mathf.Sin(time * 4f) * 0.02f;
            playerCameraTransform.localPosition = sitPos + new Vector3(0, breathY, 0);
            yield return null;
        }

        // ============================================================
        // 🦵 FASE 5: BERDIRI
        // ============================================================
        time = 0;
        Quaternion currentRot = playerCameraTransform.localRotation;
        Quaternion readyRot = Quaternion.Euler(15f, 0, 0); 
        while (time < prepareDuration)
        {
            time += Time.deltaTime;
            float t = time / prepareDuration;
            float smoothT = t * t * (3f - 2f * t);
            playerCameraTransform.localRotation = Quaternion.Slerp(currentRot, readyRot, smoothT);
            playerCameraTransform.localPosition = sitPos;
            yield return null;
        }

        time = 0;

        while (time < standUpDuration)
        {
            time += Time.deltaTime;
            float t = time / standUpDuration;
            float smoothT = Mathf.SmoothStep(0, 1, t); 

            playerCameraTransform.localPosition = Vector3.Lerp(sitPos, standPos, smoothT);

            float dipCurve = Mathf.Sin(t * Mathf.PI); 
            float activeLookDown = dipCurve * standLookDownAngle; 
            float startBias = Mathf.Lerp(15f, 0, t * 4f); 
            float stability = 1f - t; 
            float wobbleZ = Mathf.Sin(time * 12f) * wobbleIntensity * stability; 
            float wobbleY = Mathf.Cos(time * 8f) * (wobbleIntensity * 0.5f) * stability; 

            playerCameraTransform.localRotation = Quaternion.Euler(startBias + activeLookDown, wobbleY, wobbleZ);
            yield return null;
        }

        // FINAL
        if (playerCameraTransform != null) { playerCameraTransform.localPosition = standPos; playerCameraTransform.localRotation = standRot; }
        StartCoroutine(MoveWorldObjectUp());

        // Nyalakan Rigidbody (Aktifkan Fisika)
        if (playerRigid != null) playerRigid.isKinematic = false;

        // 🔥🔥🔥 FIX UTAMA: PAKSA TAG JADI "PLAYER" SEBELUM NYALAKAN SCRIPT 🔥🔥🔥
        if (playerObject != null)
        {
            playerObject.tag = "Player";
        }
        
        // 🔥 Menghidupkan kembali script temanmu agar Player bisa gerak
        foreach (var script in scriptsToDisable) {
            if (script != null) {
                script.enabled = true; 

                // Khusus CamRotation kita update orientasinya biar kamera lurus
                if (script is CamRotation camScript) 
                {
                    camScript.UpdateOrientation(); 
                }
            }
        }
        PlayDialogue(tolongAku);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (sfxRuntuhan != null) audioSource.PlayOneShot(sfxRuntuhan);
    }

    void PlayDialogue(AudioClip clip)
    {
        if (clip == null) return;

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }

    IEnumerator MoveWorldObjectUp()
    {
        if (worldObjectToMove == null)
            yield break;

        Vector3 startPos = worldObjectToMove.position;
        Vector3 endPos = startPos + moveOffsetUp;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / worldMoveDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            worldObjectToMove.position = Vector3.Lerp(startPos, endPos, smoothT);
            yield return null;
        }

        worldObjectToMove.position = endPos;
    }

}
