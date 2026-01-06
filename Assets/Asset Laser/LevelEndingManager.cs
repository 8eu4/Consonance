using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.AI; 

public class LevelEndingManager : MonoBehaviour
{
    [Header("--- CHARACTERS ---")]
    public GameObject conductor;
    public GameObject domi;
    public GameObject remi;

    [Header("--- MOVEMENT SCRIPTS ---")]
    public MonoBehaviour conductorMoveScript; 
    public MonoBehaviour switchSystem;        
    public PlayerCommandSystem playerCommandSystem; 

    [Header("--- POSITIONS (CUTSCENE) ---")]
    public Transform posConductor; 
    public Transform posDomi; 
    public Transform posRemi; 
    
    [Header("--- CAMERA ---")]
    public GameObject cameraHolder; 

    [Header("--- UI & AUDIO ---")]
    public CanvasGroup blackScreenGroup; 
    public GameObject uiPromptText; 
    public AudioSource voiceRemi; 
    public AudioSource sfxHarpa; 

    [Header("--- SETTINGS ---")]
    public string nextSceneName; 
    public float groundOffset = 0.1f; // Jarak dikit dari lantai biar ga nyangkut

    private List<GameObject> playersInArea = new List<GameObject>();
    private bool isEnding = false;

    private void Start()
    {
        if (blackScreenGroup != null) 
        {
            blackScreenGroup.alpha = 0; 
            blackScreenGroup.blocksRaycasts = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isEnding) return;

        GameObject rootObj = other.transform.root.gameObject;
        if (rootObj == conductor || rootObj == domi || rootObj == remi)
        {
            if (!playersInArea.Contains(rootObj)) playersInArea.Add(rootObj);

            if (playersInArea.Count >= 3)
            {
                isEnding = true;
                StartCoroutine(PlayEndingCutscene());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isEnding) return;
        GameObject rootObj = other.transform.root.gameObject;
        if (playersInArea.Contains(rootObj)) playersInArea.Remove(rootObj);
    }

    IEnumerator PlayEndingCutscene()
    {
        // 1. MATIKAN KONTROL & UI
        if (conductorMoveScript != null) conductorMoveScript.enabled = false;
        if (switchSystem != null) switchSystem.enabled = false;
        if (playerCommandSystem != null)
        {
            playerCommandSystem.ForceResetState();
            playerCommandSystem.enabled = false;
        }
        if (uiPromptText != null) uiPromptText.SetActive(false);

        // 2. FADE OUT (LAYAR GELAP)
        if (blackScreenGroup != null)
        {
            blackScreenGroup.blocksRaycasts = true;
            float t = 0;
            while (t < 1) { t += Time.deltaTime * 2f; blackScreenGroup.alpha = t; yield return null; }
            blackScreenGroup.alpha = 1;
        }
        else yield return new WaitForSeconds(1.0f);

        // 3. --- PROSES TELEPORT CONDUCTOR (YANG SERING BUG) ---
        CharacterController ccConductor = conductor.GetComponent<CharacterController>();
        
        // Matikan CC dan tunggu 1 frame (PENTING!)
        if(ccConductor != null) ccConductor.enabled = false;
        yield return null; 

        // Pindahkan Conductor
        conductor.transform.position = posConductor.position;
        conductor.transform.rotation = posConductor.rotation;

        // Reset Fisika biar ga slide
        Rigidbody rbConductor = conductor.GetComponent<Rigidbody>();
        if (rbConductor != null) { rbConductor.linearVelocity = Vector3.zero; rbConductor.angularVelocity = Vector3.zero; }

        // 4. --- PROSES TELEPORT REMI & DOMI ---
        TeleportNavMesh(remi, posRemi);
        TeleportNavMesh(domi, posDomi);

        // Tunggu physics sync lagi
        Physics.SyncTransforms();
        yield return new WaitForFixedUpdate();

        // Nyalakan CC Conductor lagi
        if(ccConductor != null) ccConductor.enabled = true;

        // 5. --- ATUR ROTASI WAJAH (FIX LOOK) ---
        // Kita paksa rotasi manual biar pasti bener
        
        // Conductor liat Remi
        LookAtTarget(conductor, remi.transform);
        
        // Domi liat Conductor (PASTI BENER KARENA CONDUCTOR UDAH PINDAH)
        LookAtTarget(domi, conductor.transform);
        
        // Remi liat Domi (Sesuai request)
        LookAtTarget(remi, domi.transform);

        // Camera liat Remi
        if (cameraHolder != null && remi != null)
            cameraHolder.transform.LookAt(remi.transform.position + Vector3.up * 1.5f);

        yield return new WaitForSeconds(0.5f); // Jeda dikit biar posisi stabil

        // 6. FADE IN (TERANG)
        if (blackScreenGroup != null)
        {
            float t = 1;
            while (t > 0) { t -= Time.deltaTime * 1.5f; blackScreenGroup.alpha = t; yield return null; }
            blackScreenGroup.alpha = 0;
        }

        // 7. DRAMA PLAY
        if (sfxHarpa != null) sfxHarpa.Play();
        
        // Remi liat Domi 1 detik...
        yield return new WaitForSeconds(1.0f);

        // Remi noleh ke Conductor
        LookAtTarget(remi, conductor.transform);

        if (voiceRemi != null) voiceRemi.Play();
        Debug.Log("Remi: 'Kurasa... aku benar-benar berguna ya...'");

        yield return new WaitForSeconds(5.0f); 

        // 8. SELESAI
        if (blackScreenGroup != null)
        {
            float t = 0;
            while (t < 1) { t += Time.deltaTime * 1.5f; blackScreenGroup.alpha = t; yield return null; }
            blackScreenGroup.alpha = 1;
        }
        
        yield return new WaitForSeconds(1.0f);
        SceneManager.LoadScene(nextSceneName);
    }

    // Fungsi Teleport Khusus NavMesh (Anti Slide)
    void TeleportNavMesh(GameObject obj, Transform target)
    {
        if (obj == null || target == null) return;
        NavMeshAgent agent = obj.GetComponent<NavMeshAgent>();
        
        Vector3 finalPos = target.position + (Vector3.up * groundOffset);

        if (agent != null)
        {
            agent.enabled = true;
            agent.Warp(finalPos);
            agent.velocity = Vector3.zero;
            agent.isStopped = true;
            agent.updateRotation = false; // Matikan rotasi otomatis biar kita bisa atur manual
        }
        else
        {
            obj.transform.position = finalPos;
        }
    }

    // Fungsi Rotasi yang Stabil (Cuma putar Y axis biar ga dangak/nunduk aneh)
    void LookAtTarget(GameObject viewer, Transform target)
    {
        if (viewer == null || target == null) return;
        
        Vector3 direction = target.position - viewer.transform.position;
        direction.y = 0; // Kunci sumbu Y biar ga miring
        
        if (direction != Vector3.zero)
        {
            viewer.transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}