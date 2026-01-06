using UnityEngine;
using TMPro;
using System.Collections;

public class RemiLaserCutsceneManager : MonoBehaviour
{
    [Header("--- CHARACTERS & PHYSICS ---")]
    public GameObject remi;
    public Rigidbody remiRb; 
    
    [Header("--- CAMERA CONTROL ---")]
    [Tooltip("Drag Main Camera ke sini!")]
    public Transform cameraToRotate; 
    public CamRotation cameraRotationScript; // Pastikan nama script ini sesuai script kameramu
    public Transform ventLookTarget; 

    [Header("--- UI SETTINGS ---")]
    public GameObject promptParent; 
    public TextMeshProUGUI promptText; 

    [Header("--- SCRIPTS TO DISABLE ---")]
    public MonoBehaviour remiMovement; 
    public MonoBehaviour switchSystem; 

    // Internal States
    private bool isTriggered = false;
    private bool introDone = false;
    private bool isLaserBlocked = false; // Flag penanda sukses

    private void Start()
    {
        if (promptParent != null) promptParent.SetActive(false);
        if (remiRb == null && remi != null) remiRb = remi.GetComponent<Rigidbody>();
        
        if (cameraToRotate == null && Camera.main != null) 
            cameraToRotate = Camera.main.transform;
    }

    private void LateUpdate()
    {
        // LOGIC FORCE LOOK (Kamera maksa liat vent)
        if (isTriggered && !introDone && ventLookTarget != null && cameraToRotate != null)
        {
            if (cameraRotationScript != null && cameraRotationScript.enabled) 
                cameraRotationScript.enabled = false;

            Vector3 dir = ventLookTarget.position - cameraToRotate.position;
            if (dir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                cameraToRotate.rotation = Quaternion.Slerp(cameraToRotate.rotation, targetRot, Time.deltaTime * 8f);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isTriggered && (other.CompareTag("Player") || other.transform.root.gameObject == remi))
        {
            isTriggered = true;
            StopCharacterPhysics();
            DisableControlsImmediately();
            StartCoroutine(SequenceLaserTutorial());
        }
    }

    // --- FUNGSI INI DIPANGGIL OLEH SCRIPT TEMBOK ---
    public void OnWallHitByString()
    {
        if (introDone && !isLaserBlocked)
        {
            isLaserBlocked = true; // Sinyal diterima, lanjut ke fase sukses!
            Debug.Log("Manager: String nempel tembok, story lanjut!");
        }
    }

    void StopCharacterPhysics()
    {
        if (remiRb != null)
        {
            remiRb.linearVelocity = Vector3.zero;        
            remiRb.angularVelocity = Vector3.zero; 
            remiRb.isKinematic = true;             
        }
    }

    void DisableControlsImmediately()
    {
        if (remiMovement != null) remiMovement.enabled = false;
        if (switchSystem != null) switchSystem.enabled = false;
        if (cameraRotationScript != null) cameraRotationScript.enabled = false;
    }

    IEnumerator SequenceLaserTutorial()
    {
        // FASE 1: INTRO (4 Detik)
        if (promptText != null) promptText.text = "GUNAKAN STRING LINE REMI UNTUK MENGGANGGU LASER.";
        if (promptParent != null) promptParent.SetActive(true);

        yield return new WaitForSeconds(4.0f); 

        // FASE 2: ACTION (Tunggu Player)
        if (promptParent != null) promptParent.SetActive(false); 
        introDone = true; 
        
        // Unlock Kontrol
        if (remiMovement != null) remiMovement.enabled = true;
        if (cameraRotationScript != null) cameraRotationScript.enabled = true;
        if (remiRb != null) remiRb.isKinematic = false; 

        // TUNGGU SAMPAI TEMBOK KENA HIT
        Debug.Log("Menunggu player menembak tembok...");
        yield return new WaitUntil(() => isLaserBlocked);

        // FASE 3: SUKSES
        if (promptText != null) promptText.text = "LASER 1 TERBLOKIR — CONDUCTOR DAN DOMI SEKARANG DAPAT MAJU.";
        if (promptParent != null) promptParent.SetActive(true); 

        yield return new WaitForSeconds(4.0f); 
        if (promptParent != null) promptParent.SetActive(false);

        if (switchSystem != null) switchSystem.enabled = true; 
    }
}