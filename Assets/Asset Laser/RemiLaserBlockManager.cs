using UnityEngine;
using TMPro;
using System.Collections;

public class RemiLaserCutsceneManager : MonoBehaviour
{
    [Header("--- CHARACTERS & CAMERA ---")]
    public GameObject remi;
    public MonoBehaviour cameraRotationScript; 
    public Transform ventLookTarget; 

    [Header("--- UI SETTINGS ---")]
    public GameObject promptParent; 
    public TextMeshProUGUI promptText; 

    [Header("--- SCRIPTS TO DISABLE ---")]
    public MonoBehaviour remiMovement; 
    public MonoBehaviour switchSystem; 

    private bool isTriggered = false;
    private bool introDone = false;

    private void Start()
    {
        if (promptParent != null) promptParent.SetActive(false);
    }

    private void LateUpdate()
    {
        if (isTriggered && !introDone && ventLookTarget != null)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                Vector3 dir = ventLookTarget.position - cam.transform.position;
                cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 5f);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isTriggered && (other.CompareTag("Player") || other.transform.root.gameObject == remi))
        {
            isTriggered = true;
            StartCoroutine(SequenceLaserTutorial());
        }
    }

    IEnumerator SequenceLaserTutorial()
    {
        // --- FASE 1: INTRO (4 DETIK) ---
        if (remiMovement != null) remiMovement.enabled = false;
        if (switchSystem != null) switchSystem.enabled = false;
        if (cameraRotationScript != null) cameraRotationScript.enabled = false; 

        if (promptText != null) promptText.text = "GUNAKAN STRING LINE REMI UNTUK MENGGANGGU LASER.";
        if (promptParent != null) promptParent.SetActive(true);

        yield return new WaitForSeconds(4.0f); 

        // --- FASE 2: PLAYER MENEMBAK (10 DETIK) ---
        if (promptParent != null) promptParent.SetActive(false); 
        introDone = true; 
        if (remiMovement != null) remiMovement.enabled = true;
        if (cameraRotationScript != null) cameraRotationScript.enabled = true;

        // WAKTU DIPERPANJANG SESUAI TITAH EMPEROR
        yield return new WaitForSeconds(10.0f); 

        // --- FASE 3: SUKSES (PROMPT MAJU MANUAL) ---
        if (promptText != null) promptText.text = "LASER 1 TERBLOKIR — CONDUCTOR DAN DOMI SEKARANG DAPAT MAJU.";
        if (promptParent != null) promptParent.SetActive(true); 

        yield return new WaitForSeconds(4.0f); 
        if (promptParent != null) promptParent.SetActive(false);

        if (switchSystem != null) switchSystem.enabled = true; 
    }
}