using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

public class PuzzleTutorialManager : MonoBehaviour
{
    [Header("--- CHARACTERS ---")]
    public GameObject conductor;
    public GameObject remi;
    public GameObject domi;

    [Header("--- TELEPORT SETTINGS ---")]
    public Transform conductorTargetPos;
    public Transform remiTargetPos;
    public Transform domiTargetPos;
    
    [Header("--- FIX TENGGELAM (Height Offset) ---")]
    public float domiOffsetY = 0.8f; 
    public float remiOffsetY = 0.1f;

    [Header("--- UI & AUDIO ---")]
    public CanvasGroup blackScreen;
    public GameObject promptTekan3;
    public GameObject uiFollowingText;
    public AudioSource audioSource;
    public AudioClip voClip1, voClip2, voClip3;

    [Header("--- SYSTEM CONTROL ---")]
    public MonoBehaviour mainSwitchSystem; // Script CharacterSwitchManager
    public MonoBehaviour conductorMovement; // Script gerak (WASD)
    
    // PERUBAHAN DI SINI:
    // Diganti dari MonoBehaviour menjadi PlayerCommandSystem agar Unity otomatis mencari script yang benar
    public PlayerCommandSystem playerCommandSystem; 
    
    public GameObject conductorCameraHolder; 

    [Header("--- LOOK TARGETS ---")]
    public Transform ventTarget;

    // --- INTERNAL STATE ---
    private bool isCutsceneActive = false;
    private bool isWaitingForInput3 = false; 
    private Transform remiCurrentLookTarget;
    private Camera conductorCamComponent;
    private HashSet<GameObject> charsInZone = new HashSet<GameObject>();

    private void Start()
    {
        if (blackScreen != null) 
        { 
            blackScreen.alpha = 0; 
            blackScreen.gameObject.SetActive(true); 
            blackScreen.blocksRaycasts = false; 
        }
        
        if (promptTekan3 != null) promptTekan3.SetActive(false);
        if (conductor != null) conductorCamComponent = conductor.GetComponentInChildren<Camera>();
    }

    private void Update()
    {
        // 1. LOGIC REMI MENOLEH
        if (remiCurrentLookTarget != null && remi != null)
        {
            Vector3 dir = remiCurrentLookTarget.position - remi.transform.position;
            dir.y = 0;
            if (dir != Vector3.zero) 
                remi.transform.rotation = Quaternion.Slerp(remi.transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 3.0f);
        }

        // 2. LOGIC INPUT TEKAN 3 (UNLOCK PHASE)
        if (isWaitingForInput3 && Input.GetKeyDown(KeyCode.Alpha3))
        {
            // --- FASE 3: FULL UNLOCK SETELAH TEKAN 3 ---
            
            if (promptTekan3 != null) promptTekan3.SetActive(false); 
            
            // Nyalakan kembali Move & Command
            if (conductorMovement != null) conductorMovement.enabled = true;
            if (playerCommandSystem != null) playerCommandSystem.enabled = true; // Nyalakan lagi logic command
            if (uiFollowingText != null) uiFollowingText.SetActive(true);

            isWaitingForInput3 = false; 
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCutsceneActive) return;

        GameObject rootObj = other.transform.root.gameObject;
        if (rootObj == conductor || rootObj == remi || rootObj == domi)
        {
            if (!charsInZone.Contains(rootObj)) charsInZone.Add(rootObj);
            if (charsInZone.Count >= 3) StartCoroutine(SequenceTutorial());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isCutsceneActive) return;
        GameObject rootObj = other.transform.root.gameObject;
        if (charsInZone.Contains(rootObj)) charsInZone.Remove(rootObj);
    }

    IEnumerator SequenceTutorial()
    {
        isCutsceneActive = true; 

        // --- FASE 1: MATIKAN SEMUA (Cutscene Mode) ---
        if (mainSwitchSystem != null) mainSwitchSystem.enabled = false;
        if (conductorMovement != null) conductorMovement.enabled = false;
        
        // Matikan sistem command
        if (playerCommandSystem != null) playerCommandSystem.enabled = false; 
        if (uiFollowingText != null) uiFollowingText.SetActive(false);

        // Ambil komponen fisik
        CharacterController ccConductor = conductor.GetComponent<CharacterController>();
        NavMeshAgent agentRemi = remi.GetComponent<NavMeshAgent>();
        NavMeshAgent agentDomi = domi.GetComponent<NavMeshAgent>();

        // Matikan Physics
        if (agentRemi != null) agentRemi.enabled = false;
        if (agentDomi != null) agentDomi.enabled = false;
        if (ccConductor != null) ccConductor.enabled = false;

        // --- FADE OUT ---
        if (blackScreen != null)
        {
            blackScreen.blocksRaycasts = true;
            yield return StartCoroutine(FadeCanvas(blackScreen, 1f, 1.0f));
        }

        yield return new WaitForFixedUpdate();

        // --- TELEPORT ---
        if (conductor && conductorTargetPos)
        {
            conductor.transform.position = conductorTargetPos.position;
            conductor.transform.rotation = conductorTargetPos.rotation;
        }

        if (remi && remiTargetPos)
        {
            Vector3 finalPos = remiTargetPos.position + (Vector3.up * remiOffsetY);
            if (agentRemi != null) {
                agentRemi.enabled = true; 
                if(!agentRemi.Warp(finalPos)) remi.transform.position = finalPos;
                agentRemi.enabled = false;
            } else {
                remi.transform.position = finalPos;
            }
            remi.transform.rotation = remiTargetPos.rotation;
        }

        if (domi && domiTargetPos)
        {
            Vector3 finalPos = domiTargetPos.position + (Vector3.up * domiOffsetY);
            if (agentDomi != null) {
                agentDomi.enabled = true;
                if(!agentDomi.Warp(finalPos)) domi.transform.position = finalPos;
                agentDomi.enabled = false;
            } else {
                domi.transform.position = finalPos;
            }
            domi.transform.rotation = domiTargetPos.rotation;
        }

        Physics.SyncTransforms(); 
        yield return new WaitForFixedUpdate();

        // --- RESET KAMERA ---
        if (conductorCameraHolder != null) 
        {
            conductorCameraHolder.SetActive(true);
            if (conductorCamComponent != null) conductorCamComponent.transform.localRotation = Quaternion.identity;
        }

        // --- FADE IN ---
        if (blackScreen != null) yield return StartCoroutine(FadeCanvas(blackScreen, 0f, 1.5f));
        if (blackScreen != null) blackScreen.blocksRaycasts = false;

        // --- DIALOG ---
        remiCurrentLookTarget = ventTarget; 
        PlayVO(voClip1);
        yield return new WaitForSeconds(8f); 

        remiCurrentLookTarget = conductor.transform;
        PlayVO(voClip2);
        yield return new WaitForSeconds(7f);

        PlayVO(voClip3);
        yield return new WaitForSeconds(5f);

        // --- KEMBALIKAN FISIKA ---
        if (ccConductor != null) ccConductor.enabled = true;
        if (agentRemi != null) agentRemi.enabled = true;     
        if (agentDomi != null) agentDomi.enabled = true;     
        
        remiCurrentLookTarget = null;

        // --- FASE 2: UNLOCK SWITCH ONLY ---
        if (mainSwitchSystem != null) mainSwitchSystem.enabled = true; 
        
        if (conductorMovement != null) conductorMovement.enabled = false; 
        if (playerCommandSystem != null) playerCommandSystem.enabled = false;

        if (promptTekan3 != null)
        {
            promptTekan3.SetActive(true);
            isWaitingForInput3 = true; 
        }
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