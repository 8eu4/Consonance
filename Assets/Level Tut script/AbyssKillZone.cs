using UnityEngine;
using UnityEngine.Rendering; // Jika pakai Volume nanti
using System.Collections;

public class AbyssKillZone : MonoBehaviour
{
    [Header("--- SETTINGAN RESPAWN ---")]
    public Transform respawnPoint;

    [Header("--- VISUAL DEATH (JURANG FEEL) ---")]
    public CanvasGroup blackScreen; 
    public float fadeDuration = 0.8f;   // Agak lama biar dramatis
    
    [Header("--- CAMERA EFFECTS ---")]
    public float fallFov = 90f;         // FOV melebar saat jatuh (Kesan ngebut)
    public float shakePower = 0.3f;     // Getaran saat jatuh
    public CameraShake cameraShake;     // Script CameraShake yang kamu punya

    [Header("--- AUDIO EFFECTS ---")]
    public AudioSource audioSource;     // Pasang AudioSource di object Trigger ini
    public AudioClip fallSound;         // Suara angin/jatuh ("Whoosh")
    public AudioClip respawnSound;      // Suara nafas/bangun ("Gasp")

    [Header("--- AUTO DETECT ---")]
    public MonoBehaviour movementScript; 
    public MonoBehaviour cameraScript;

    private bool isRespawning = false;
    private float originalFov;
    private Camera playerCam;

    private void Start()
    {
        if (blackScreen != null) 
        {
            blackScreen.alpha = 0;
            blackScreen.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isRespawning) return; 

        GameObject playerRoot = other.transform.root.gameObject;

        if (playerRoot.CompareTag("Player"))
        {
            // Auto Detect Script Jalan
            if (movementScript == null)
            {
                MonoBehaviour[] scripts = playerRoot.GetComponentsInChildren<MonoBehaviour>();
                foreach (var s in scripts) {
                    if (s.GetType().Name.Contains("Command") || s.GetType().Name.Contains("System") || s.GetType().Name.Contains("Controller")) 
                        movementScript = s;
                }
            }

            // Auto Detect Kamera & CameraShake
            playerCam = playerRoot.GetComponentInChildren<Camera>();
            if (playerCam != null)
            {
                originalFov = playerCam.fieldOfView; // Simpan FOV asli
                if (cameraShake == null) cameraShake = playerRoot.GetComponentInChildren<CameraShake>();
                
                // Cari script controller kamera
                if (cameraScript == null) cameraScript = playerCam.GetComponent<MonoBehaviour>();
            }

            StartCoroutine(TeleportSequence(playerRoot));
        }
    }

   IEnumerator TeleportSequence(GameObject player)
    {
        isRespawning = true;

        if (respawnPoint == null) { Debug.LogError("⚠️ LUPA ISI RESPAWN POINT!"); isRespawning = false; yield break; }

        // 1. EFEK JATUH (AUDIO & VISUAL)
        if (audioSource != null && fallSound != null) audioSource.PlayOneShot(fallSound);
        if (cameraShake != null) StartCoroutine(cameraShake.Shake(fadeDuration, shakePower));
        
        // FOV Melebar (Kesan Cepat)
        if (playerCam != null) StartCoroutine(ChangeFOV(playerCam, fallFov, 0.2f));

        // 2. MATIKAN KONTROL & FISIKA (FREEZE)
        ToggleControls(false);
        
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero; 
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true; 
        }

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        // 3. FADE OUT (GELAP)
        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(true);
            float t = 0;
            while(t < 1.0f)
            {
                t += Time.deltaTime / fadeDuration;
                blackScreen.alpha = t;
                yield return null;
            }
            blackScreen.alpha = 1;
        }

        yield return new WaitForSeconds(0.5f); // Jeda dalam kegelapan

        // 4. TELEPORT & RESET
        player.transform.position = respawnPoint.position;
        float targetY = respawnPoint.eulerAngles.y;
        player.transform.rotation = Quaternion.Euler(0f, targetY, 0f); // Reset Badan

        Physics.SyncTransforms(); 

        // ANTI-MIRING & RESET FOV
        if (playerCam != null)
        {
            Vector3 camEuler = playerCam.transform.localEulerAngles;
            playerCam.transform.localEulerAngles = new Vector3(0f, camEuler.y, 0f); // Reset Miring
            playerCam.fieldOfView = originalFov; // Balikin FOV Normal
        }

        // 5. AUDIO RESPAWN (Suara "Hosh" kaget)
        if (audioSource != null && respawnSound != null) audioSource.PlayOneShot(respawnSound);

        yield return new WaitForSeconds(0.2f); 

        // 6. FADE IN (TERANG)
        if (blackScreen != null)
        {
            float t = 0;
            while(t < 1.0f)
            {
                t += Time.deltaTime / fadeDuration;
                blackScreen.alpha = 1.0f - t;
                yield return null;
            }
            blackScreen.alpha = 0;
            blackScreen.gameObject.SetActive(false);
        }

        // 7. NYALAKAN LAGI
        if (cc != null) cc.enabled = true;
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            player.transform.rotation = Quaternion.Euler(0f, targetY, 0f); // Safety Reset
        }

        ToggleControls(true);
        isRespawning = false;
    }

    // Helper ganti FOV
    IEnumerator ChangeFOV(Camera cam, float targetFov, float duration)
    {
        float startFov = cam.fieldOfView;
        float t = 0;
        while(t < 1.0f)
        {
            t += Time.deltaTime / duration;
            cam.fieldOfView = Mathf.Lerp(startFov, targetFov, t);
            yield return null;
        }
    }

    void ToggleControls(bool state)
    {
        if (movementScript != null) movementScript.enabled = state;
        if (cameraScript != null && cameraScript.GetType().Name.Contains("Look")) cameraScript.enabled = state; 
    }
}