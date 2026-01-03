using UnityEngine;
using System.Collections;
using System.Linq; 

public class AbyssKillZone : MonoBehaviour
{
    [Header("--- DETECTION SETTINGS ---")]
    public string[] allowedTags = { "Player", "Muse", "Conductor", "Untagged" }; 

    [Header("--- SETTINGAN RESPAWN ---")]
    public Transform respawnPoint;
    public float spawnYOffset = 2.0f; // Tinggi aman

    [Header("--- VISUAL DEATH ---")]
    public CanvasGroup blackScreen; 
    public float fadeDuration = 0.5f;   
    
    [Header("--- AUTO DETECT ---")]
    private MonoBehaviour movementScript; 
    
    private bool isRespawning = false;

    // Tambahan Audio & Cam (Opsional)
    [Header("--- EFEK TAMBAHAN ---")]
    public AudioSource audioSource;     
    public AudioClip fallSound;         
    public AudioClip respawnSound;
    public CameraShake cameraShake;

    private void Start()
    {
        if (blackScreen != null) { blackScreen.alpha = 0; blackScreen.gameObject.SetActive(false); }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isRespawning) return; 

        GameObject playerRoot = other.transform.root.gameObject;
        
        // Cek apakah target valid
        bool isValidTarget = allowedTags.Contains(playerRoot.tag) || playerRoot.CompareTag("Player");

        if (isValidTarget)
        {
            Debug.Log("💀 JATUH: " + playerRoot.name);
            
            // Cari script gerak 'Move' atau 'Remi'
            movementScript = null;
            MonoBehaviour[] scripts = playerRoot.GetComponentsInChildren<MonoBehaviour>();
            foreach (var s in scripts) 
            {
                string sName = s.GetType().Name;
                if (sName.Contains("Move") || sName.Contains("Remi") || sName.Contains("Controller"))        
                {
                    if (s != this && !sName.Contains("Camera")) { movementScript = s; break; }
                }
            }

            StartCoroutine(TeleportSequence(playerRoot));
        }
    }

    IEnumerator TeleportSequence(GameObject player)
    {
        isRespawning = true;
        if (respawnPoint == null) { Debug.LogError("⛔ ERROR: Respawn Point Kosong!"); isRespawning = false; yield break; }

        // 1. MATIKAN SEMUA KONTROL
        if (movementScript != null) movementScript.enabled = false;

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Bekukan fisika
            #if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector3.zero;
            #else
            rb.velocity = Vector3.zero;
            #endif
            rb.angularVelocity = Vector3.zero;
            rb.interpolation = RigidbodyInterpolation.None; // Matikan smoothing visual sementara
        }

        // Efek Suara/Kamera
        if (audioSource != null && fallSound != null) audioSource.PlayOneShot(fallSound);
        if (cameraShake != null) StartCoroutine(cameraShake.Shake(fadeDuration, 0.3f));

        // 2. FADE OUT (GELAP)
        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(true);
            float t = 0;
            while(t < 1.0f) { t += Time.deltaTime / fadeDuration; blackScreen.alpha = t; yield return null; }
            blackScreen.alpha = 1;
        }

        yield return new WaitForSeconds(0.2f);

        // ==========================================================
        // 🔥 JURUS UTAMA: POSITION LOCKING (Gembok Posisi) 🔥
        // ==========================================================
        
        // Kita hitung posisi target
        Vector3 targetPos = respawnPoint.position + (Vector3.up * spawnYOffset);
        float targetRotY = respawnPoint.eulerAngles.y;

        // Kita PAKSA posisi dia di sana selama 10 frame berturut-turut
        // Ini biar script 'Move' atau Gravitasi gak bisa narik dia ke bawah
        for (int i = 0; i < 10; i++)
        {
            player.transform.position = targetPos;
            player.transform.rotation = Quaternion.Euler(0f, targetRotY, 0f);
            
            if (rb != null)
            {
                rb.position = targetPos;
                rb.rotation = Quaternion.Euler(0f, targetRotY, 0f);
                #if UNITY_6000_0_OR_NEWER
                rb.linearVelocity = Vector3.zero;
                #else
                rb.velocity = Vector3.zero;
                #endif
            }
            Physics.SyncTransforms(); 
            yield return new WaitForFixedUpdate(); // Tunggu frame fisika berikutnya
        }
        
        // ==========================================================

        yield return new WaitForSeconds(0.2f); // Jeda sebentar di posisi aman

        // 3. FADE IN (TERANG)
        if (blackScreen != null)
        {
            float t = 0;
            while(t < 1.0f) { t += Time.deltaTime / fadeDuration; blackScreen.alpha = 1.0f - t; yield return null; }
            blackScreen.alpha = 0;
            blackScreen.gameObject.SetActive(false);
        }

        // 4. HIDUPKAN KEMBALI
        if (rb != null)
        {
            rb.isKinematic = false; // Hidupkan fisika
            rb.interpolation = RigidbodyInterpolation.Interpolate; // Nyalakan lagi smoothing (kalau pakai)
            rb.WakeUp();
        }

        if (movementScript != null) movementScript.enabled = true;
        if (audioSource != null && respawnSound != null) audioSource.PlayOneShot(respawnSound);

        isRespawning = false;
    }
}