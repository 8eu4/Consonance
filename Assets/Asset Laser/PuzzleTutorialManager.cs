using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.AI;

public class PuzzleTutorialManager : MonoBehaviour
{
    [Header("--- CHARACTERS ---")]
    public GameObject conductor;
    public GameObject remi;
    public GameObject domi;

    [Header("--- TELEPORT POSITIONS ---")]
    public Transform conductorTargetPos;
    public Transform remiTargetPos;
    public Transform domiTargetPos;

    [Header("--- UI & AUDIO ---")]
    public CanvasGroup blackScreen; 
    public GameObject promptTekan3;
    public GameObject uiFollowingText; 
    public AudioSource audioSource;
    public AudioClip voClip1, voClip2, voClip3;

    [Header("--- CONTROL SCRIPTS ---")]
    public MonoBehaviour conductorMovement;
    public GameObject cameraHolder; 
    public MonoBehaviour mainSwitchSystem; // Tarik script SwitchCharacter ke sini

    [Header("--- LOOK TARGETS ---")]
    public Transform ventTarget;      

    private bool isTriggered = false;
    private Transform remiCurrentLookTarget;
    private Transform mainCamera;
    private Quaternion originalCamLocalRot;

    private void Start()
    {
        if (blackScreen != null) { blackScreen.alpha = 0; blackScreen.gameObject.SetActive(true); }
        if (promptTekan3 != null) promptTekan3.SetActive(false); 
        if (conductor != null) mainCamera = conductor.GetComponentInChildren<Camera>().transform;
    }

    private void Update()
    {
        // Smooth Rotation Remi menoleh
        if (remiCurrentLookTarget != null)
        {
            Vector3 dir = remiCurrentLookTarget.position - remi.transform.position;
            dir.y = 0;
            if (dir != Vector3.zero) remi.transform.rotation = Quaternion.Slerp(remi.transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 3.0f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((other.CompareTag("Player") || other.transform.root.CompareTag("Player")) && !isTriggered)
        {
            isTriggered = true;
            StartCoroutine(SequenceTutorial());
        }
    }

    IEnumerator SequenceTutorial()
    {
        yield return new WaitForSeconds(10f);
        if (blackScreen != null) yield return StartCoroutine(FadeCanvas(blackScreen, 1, 1.0f));

        // Matikan Kontrol selama cutscene
        if (conductorMovement != null) conductorMovement.enabled = false;
        if (mainSwitchSystem != null) mainSwitchSystem.enabled = false;
        if (uiFollowingText != null) uiFollowingText.SetActive(false); 
        if (cameraHolder != null) cameraHolder.SetActive(false); 

        CharacterController cc = conductor.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false; 

        ToggleMusesNavMesh(false); 
        yield return new WaitForFixedUpdate(); 
        
        TeleportCharacters();
        
        if (mainCamera != null)
        {
            originalCamLocalRot = mainCamera.localRotation;
            mainCamera.localRotation = Quaternion.identity; 
        }

        yield return new WaitForSeconds(0.5f);
        if (blackScreen != null) yield return StartCoroutine(FadeCanvas(blackScreen, 0, 1.5f));

        // --- DIALOG SEQUENCE ---
        remiCurrentLookTarget = ventTarget; 
        PlayVO(voClip1);
        yield return new WaitForSeconds(8f);

        remiCurrentLookTarget = conductor.transform;
        PlayVO(voClip2);
        yield return new WaitForSeconds(7f);

        PlayVO(voClip3);
        yield return new WaitForSeconds(5f);

        // 🔥 TAHAP SIMPLE DARI EMPEROR:
        // Begitu dialog selesai, aktifkan semua script kembali secara otomatis
        // Jadi saat Emperor pencet 3, script SwitchCharacter sudah 'ON' dan langsung respon.
        
        if (promptTekan3 != null) promptTekan3.SetActive(true);
        
        if (mainSwitchSystem != null) mainSwitchSystem.enabled = true; // Aktifkan SwitchCharacter
        if (cameraHolder != null) cameraHolder.SetActive(true);       // Aktifkan Kamera Mouse
        if (conductorMovement != null) conductorMovement.enabled = true; // Aktifkan Gerak
        if (cc != null) cc.enabled = true;

        // Berhenti memaksa Remi menoleh agar dia bisa gerak bebas lagi
        remiCurrentLookTarget = null;
    }

    void TeleportCharacters()
    {
        conductor.transform.position = conductorTargetPos.position;
        conductor.transform.rotation = conductorTargetPos.rotation;
        remi.transform.position = remiTargetPos.position + Vector3.up * 0.1f;
        remi.transform.rotation = remiTargetPos.rotation;
        domi.transform.position = domiTargetPos.position + Vector3.up * 0.8f; 
        domi.transform.rotation = domiTargetPos.rotation;
    }

    void ToggleMusesNavMesh(bool state)
    {
        NavMeshAgent ar = remi.GetComponent<NavMeshAgent>();
        if (ar != null) ar.enabled = state;
        NavMeshAgent ad = domi.GetComponent<NavMeshAgent>();
        if (ad != null) ad.enabled = state;
    }

    IEnumerator FadeCanvas(CanvasGroup cg, float target, float dur)
    {
        float start = cg.alpha;
        float time = 0;
        while (time < dur)
        {
            time += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, target, time / dur);
            yield return null;
        }
        cg.alpha = target;
    }

    void PlayVO(AudioClip clip) { if (audioSource != null && clip != null) audioSource.PlayOneShot(clip); }
}