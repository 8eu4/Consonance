using UnityEngine;
using TMPro; 
using System.Collections; 

public class TutorialDomi_Story : MonoBehaviour
{
    [Header("--- UI SETTINGS ---")]
    public GameObject subPanel;     
    public TextMeshProUGUI subText; 
    
    [Header("--- CONTROLLER ---")]
    public Transform playerCamera;   
    public Transform targetLook;     
    
    [Header("--- SCRIPT KAMERA (PENTING) ---")]
    [Tooltip("Masukkan script 'CamRotation' atau 'MouseLook' dari CameraHolder ke sini")]
    public MonoBehaviour cameraScript; // <--- SLOT KHUSUS KAMERA

    [Header("--- SCRIPTS TO LOCK (GERAKAN) ---")]
    [Tooltip("Masukkan Script MOVE Conductor saja")]
    public MonoBehaviour[] conductorMoveScripts; 

    [Tooltip("Masukkan Script MOVE Domi saja")]
    public MonoBehaviour[] domiMoveScripts; 

    [Header("--- SETTING ---")]
    public string playerTag = "Player"; 
    public float rotateSpeed = 3.0f;

    void Start()
    {
        if (subPanel != null) subPanel.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        GameObject hitObject = other.gameObject;
        if (other.attachedRigidbody != null) hitObject = other.attachedRigidbody.gameObject;

        if (hitObject.CompareTag(playerTag) || hitObject.name.Contains("Conductor"))
        {
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;
            StartCoroutine(RunTutorialSequence());
        }
    }

    IEnumerator RunTutorialSequence()
    {
        // 1. MATIKAN KAMERA & GERAK CONDUCTOR (Biar fokus ke Domi)
        if (cameraScript != null) cameraScript.enabled = false;
        foreach (var s in conductorMoveScripts) if (s != null) s.enabled = false;

        // 2. PUTAR KAMERA OTOMATIS
        if (playerCamera != null && targetLook != null)
        {
            Quaternion startRot = playerCamera.rotation;
            Vector3 dir = (targetLook.position - playerCamera.position).normalized;
            Quaternion endRot = Quaternion.LookRotation(dir);

            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * rotateSpeed;
                playerCamera.rotation = Quaternion.Slerp(startRot, endRot, t);
                yield return null;
            }
        }

        // 3. TUTORIAL SWITCH
        if (subPanel != null) subPanel.SetActive(true);
        if (subText != null) subText.text = "Tekan <color=yellow>[2]</color> \nAmbil kendali Muse";
        
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Alpha2));
        
        // --- 🔥 PERBAIKAN DISINI 🔥 ---
        yield return new WaitForSeconds(0.1f); 

        // NYALAKAN LAGI KAMERA (Supaya Domi bisa ngebidik)
        if (cameraScript != null) cameraScript.enabled = true;

        // TAPI KUNCI KAKINYA DOMI (Supaya diam di tempat)
        foreach (var s in domiMoveScripts) if (s != null) s.enabled = false;


        if (subText != null) subText.text = ""; 
        yield return new WaitForSeconds(0.5f);

        // --- STEP 2: SHOOT ---
        if (subText != null) subText.text = "Klik <color=yellow>[LMB]</color> \nNembak String Line";
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

    
        // SELESAI
       
        yield return new WaitForSeconds(2.0f);

        // RESET SEMUA (Nyalakan Semua)
        if (subPanel != null) subPanel.SetActive(false);
        
        if (cameraScript != null) cameraScript.enabled = true; // Pastikan nyala
        foreach (var s in conductorMoveScripts) if (s != null) s.enabled = true;
        foreach (var s in domiMoveScripts) if (s != null) s.enabled = true;
        
        Destroy(gameObject); 
    }
}