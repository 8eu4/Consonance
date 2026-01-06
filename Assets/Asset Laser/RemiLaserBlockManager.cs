using UnityEngine;
using TMPro;
using System.Collections;

public class RemiLaserCutsceneManager : MonoBehaviour
{
    // ... (Bagian Header Variable TETAP SAMA seperti sebelumnya) ...
    [Header("--- CHARACTERS & PHYSICS ---")]
    public GameObject remi;
    public Rigidbody remiRb; 
    
    [Header("--- CAMERA CONTROL ---")]
    public Transform cameraToRotate; 
    public MonoBehaviour cameraRotationScript; 
    public Transform ventLookTarget; 

    [Header("--- UI SETTINGS ---")]
    public GameObject promptParent; 
    public TextMeshProUGUI promptText; 

    [Header("--- SCRIPTS TO DISABLE ---")]
    public MonoBehaviour remiMovement; 
    public MonoBehaviour switchSystem; 

    // Internal States
    private bool isTriggered = false;
    private bool isLaserBlocked = false; 
    
    // --- TAMBAHAN FIX: Flag khusus buat kamera ---
    private bool isLookingAtVent = false; 

    // ... (Bagian Start & OnTriggerEnter TETAP SAMA) ...
    private void Start()
    {
        if (promptParent != null) promptParent.SetActive(false);
        if (remiRb == null && remi != null) remiRb = remi.GetComponent<Rigidbody>();
        if (cameraToRotate == null && Camera.main != null) cameraToRotate = Camera.main.transform;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isTriggered && (other.CompareTag("Player") || (remi != null && other.transform.root.gameObject == remi)))
        {
            isTriggered = true;
            StartCoroutine(SequenceLaserTutorial());
        }
    }

    // --- BAGIAN INI YANG DIUBAH (LateUpdate) ---
    private void LateUpdate()
    {
        // LOGIC FIX: Cuma paksa liat vent kalau variable 'isLookingAtVent' nyala
        if (isLookingAtVent && ventLookTarget != null && cameraToRotate != null)
        {
            // Matikan input mouse kamera manual
            if (cameraRotationScript != null && cameraRotationScript.enabled) 
                cameraRotationScript.enabled = false;

            // Putar kamera halus ke arah target
            Vector3 dir = ventLookTarget.position - cameraToRotate.position;
            if (dir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                cameraToRotate.rotation = Quaternion.Slerp(cameraToRotate.rotation, targetRot, Time.deltaTime * 5f);
            }
        }
    }

    // ... (Fungsi OnWallHitByString TETAP SAMA) ...
    public void OnWallHitByString()
    {
        if (!isLaserBlocked)
        {
            isLaserBlocked = true; 
            Debug.Log("Manager: Sinyal diterima! String sudah memblokir laser.");
        }
    }

    // --- BAGIAN INI YANG DIUBAH (Coroutine) ---
    IEnumerator SequenceLaserTutorial()
    {
        // --- STEP 1: PREPARATION ---
        StopCharacterPhysics();
        DisableControlsImmediately();

        // FIX: Mulai paksa kamera liat vent
        isLookingAtVent = true; 

        // --- STEP 2: INTRO (4 Detik) ---
        if (promptText != null) promptText.text = "GUNAKAN STRING LINE REMI UNTUK MENGGANGGU LASER.";
        if (promptParent != null) promptParent.SetActive(true);

        yield return new WaitForSeconds(4.0f); 

        // --- STEP 3: ACTION (Player disuruh nembak) ---
        // FIX: Intro selesai, STOP paksa kamera liat vent
        isLookingAtVent = false; 

        if (promptParent != null) promptParent.SetActive(false); 
        
        // Unlock Kontrol
        if (remiMovement != null) remiMovement.enabled = true;
        
        // Nyalakan lagi script kamera player supaya bisa aim
        if (cameraRotationScript != null) cameraRotationScript.enabled = true; 
        
        if (remiRb != null) remiRb.isKinematic = false; 

        Debug.Log("Intro selesai. Kamera kembali ke player.");

        // TUNGGU SAMPAI SENSOR MENGIRIM SINYAL
        yield return new WaitUntil(() => isLaserBlocked);

        // --- STEP 4: SUKSES ---
        if (promptText != null) promptText.text = "LASER 1 TERBLOKIR — CONDUCTOR DAN DOMI SEKARANG DAPAT MAJU.";
        if (promptParent != null) promptParent.SetActive(true); 

        yield return new WaitForSeconds(4.0f); 
        
        // --- STEP 5: SELESAI ---
        if (promptParent != null) promptParent.SetActive(false);
        if (switchSystem != null) switchSystem.enabled = true; 
    }

    // ... (Sisanya StopCharacterPhysics & DisableControlsImmediately TETAP SAMA) ...
    void StopCharacterPhysics()
    {
        if (remiRb != null)
        {
            #if UNITY_6000_0_OR_NEWER
            remiRb.linearVelocity = Vector3.zero; 
            remiRb.angularVelocity = Vector3.zero;
            #else
            remiRb.velocity = Vector3.zero;
            remiRb.angularVelocity = Vector3.zero;
            #endif
            remiRb.isKinematic = true;                 
        }
    }

    void DisableControlsImmediately()
    {
        if (remiMovement != null) remiMovement.enabled = false;
        if (switchSystem != null) switchSystem.enabled = false;
        if (cameraRotationScript != null) cameraRotationScript.enabled = false;
    }
}