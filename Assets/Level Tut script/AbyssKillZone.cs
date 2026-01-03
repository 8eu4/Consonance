using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Butuh ini buat List
using System.Linq; 

public class AbyssKillZone : MonoBehaviour
{
    [Header("--- DETECTION SETTINGS ---")]
    public string[] allowedTags = { "Player", "Muse", "Conductor", "Untagged" }; 

    [Header("--- SETTINGAN RESPAWN (DEFAULT) ---")]
    [Tooltip("Titik respawn umum (misal buat Conductor)")]
    public Transform defaultRespawnPoint; 
    public float spawnYOffset = 2.0f; 

    // --- TAMBAHAN BARU: DAFTAR RESPAWN KHUSUS ---
    [System.Serializable] // Biar muncul di Inspector
    public struct SpecialRespawn
    {
        public string characterName; // Contoh: "Remi"
        public Transform targetPoint; // Drag object titik respawn seberang
    }

    [Header("--- RESPAWN KHUSUS (REMI/DOMI) ---")]
    public List<SpecialRespawn> specialRespawns; // Isi di Inspector!
    // ---------------------------------------------

    [Header("--- VISUAL DEATH ---")]
    public CanvasGroup blackScreen; 
    public float fadeDuration = 0.5f;    
    
    [Header("--- AUTO DETECT ---")]
    private MonoBehaviour movementScript; 
    
    private bool isRespawning = false;

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
        
        bool isValidTarget = allowedTags.Contains(playerRoot.tag) || playerRoot.CompareTag("Player");

        if (isValidTarget)
        {
            Debug.Log("💀 JATUH: " + playerRoot.name);
            
            // Cari script gerak
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

            // --- LOGIKA PEMILIHAN TITIK RESPAWN ---
            Transform targetPoint = defaultRespawnPoint; // Defaultnya ke Conductor/Awal

            // Cek apakah yang jatuh adalah Remi (atau nama lain di list khusus)
            foreach (var special in specialRespawns)
            {
                // Cek apakah nama object mengandung kata kunci (misal "Remi")
                if (playerRoot.name.Contains(special.characterName))
                {
                    targetPoint = special.targetPoint;
                    Debug.Log("🎯 Respawn Khusus Terdeteksi untuk: " + special.characterName);
                    break;
                }
            }

            // Kirim titik tujuan ke Coroutine
            StartCoroutine(TeleportSequence(playerRoot, targetPoint));
        }
    }

    // Update: Nambah parameter 'destination'
    IEnumerator TeleportSequence(GameObject player, Transform destination)
    {
        isRespawning = true;
        
        // Safety check kalau lupa ngisi
        if (destination == null) destination = defaultRespawnPoint;
        if (destination == null) { Debug.LogError("⛔ ERROR: Tidak ada Respawn Point!"); isRespawning = false; yield break; }

        // 1. MATIKAN KONTROL
        if (movementScript != null) movementScript.enabled = false;

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            #if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector3.zero;
            #else
            rb.velocity = Vector3.zero;
            #endif
            rb.angularVelocity = Vector3.zero;
            rb.interpolation = RigidbodyInterpolation.None;
        }

        if (audioSource != null && fallSound != null) audioSource.PlayOneShot(fallSound);
        if (cameraShake != null) StartCoroutine(cameraShake.Shake(fadeDuration, 0.3f));

        // 2. FADE OUT
        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(true);
            float t = 0;
            while(t < 1.0f) { t += Time.deltaTime / fadeDuration; blackScreen.alpha = t; yield return null; }
            blackScreen.alpha = 1;
        }

        yield return new WaitForSeconds(0.2f);

        // ==========================================================
        // 🔥 JURUS UTAMA: POSITION LOCKING 🔥
        // ==========================================================
        
        // Gunakan 'destination' yang sudah dipilih tadi
        Vector3 targetPos = destination.position + (Vector3.up * spawnYOffset);
        float targetRotY = destination.eulerAngles.y;

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
            yield return new WaitForFixedUpdate();
        }
        
        // ==========================================================

        yield return new WaitForSeconds(0.2f);

        // 3. FADE IN
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
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.WakeUp();
        }

        if (movementScript != null) movementScript.enabled = true;
        if (audioSource != null && respawnSound != null) audioSource.PlayOneShot(respawnSound);

        isRespawning = false;
    }
}