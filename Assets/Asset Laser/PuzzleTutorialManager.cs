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

    [Header("--- TELEPORT POINTS ---")]
    public Transform conductorTargetPos;
    public Transform remiTargetPos;
    public Transform domiTargetPos;

    [Header("--- CAMERA SETTINGS ---")]
    public Camera mainCamera; 
    public Transform cutsceneCameraPos; 
    public Transform cameraLookTarget; 

    [Header("--- UI ELEMENTS ---")]
    public CanvasGroup blackScreen; 
    public TextMeshProUGUI dialogText;
    public GameObject promptTekan3;

    [Header("--- PLAYER SCRIPTS (CONTROL) ---")]
    public MonoBehaviour conductorMovement;
    public MonoBehaviour conductorCameraLook;
    public MonoBehaviour remiMovement; 
    public MonoBehaviour remiCameraLook;

    [Header("--- EXTERNAL SYSTEM LOCK ---")]
    public MonoBehaviour mainSwitchSystem;

    [Header("--- LOOK TARGETS (REMI) ---")]
    public Transform ventTarget;      
    public Transform conductorCameraPos; 

    private Transform originalCameraParent;
    private Vector3 originalCameraPos;
    private Quaternion originalCameraRot;
    private bool isTriggered = false;
    private bool canSwitchWithThree = false;

    private void Start()
    {
        Debug.Log("<color=cyan>[Emperor Debug]</color> Script Started. Checking UI...");
        if (blackScreen != null) blackScreen.alpha = 0;
        if (promptTekan3 != null) promptTekan3.SetActive(false);
        if (dialogText != null) dialogText.text = "";
    }

    private void Update()
    {
        if (canSwitchWithThree && Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("<color=green>[Emperor Debug]</color> Key 3 Pressed! Switching control...");
            SwitchToRemiByCommand();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        bool isPlayer = other.CompareTag("Player") || (other.transform.parent != null && other.transform.parent.CompareTag("Player"));

        if (isPlayer && !isTriggered)
        {
            Debug.Log("<color=yellow>[Emperor Debug]</color> Trigger Activated by: " + other.name);
            isTriggered = true;
            StartCoroutine(SequenceTutorial());
        }
    }

    IEnumerator SequenceTutorial()
    {
        Debug.Log("<color=yellow>[Emperor Debug]</color> Step 1: 10s Countdown started.");
        if (mainSwitchSystem != null) mainSwitchSystem.enabled = false;

        yield return new WaitForSeconds(10f);

        Debug.Log("<color=yellow>[Emperor Debug]</color> Step 2: Fading screen to black.");
        if (blackScreen != null) yield return StartCoroutine(FadeCanvas(blackScreen, 1, 1.5f));

        Debug.Log("<color=yellow>[Emperor Debug]</color> Step 3: Teleporting & Locking Characters.");
        LockControl(conductor, conductorMovement, conductorCameraLook, true);
        LockControl(remi, remiMovement, remiCameraLook, true); 
        SetFixedCamera(true); 
        ToggleNavMesh(false);

        yield return new WaitForSeconds(0.5f);
        Teleport();
        yield return new WaitForSeconds(1f);

        Debug.Log("<color=yellow>[Emperor Debug]</color> Step 4: Fading screen back in.");
        if (blackScreen != null) yield return StartCoroutine(FadeCanvas(blackScreen, 0, 1.5f));

        // --- DIALOG SEQUENCE ---
        Debug.Log("<color=yellow>[Emperor Debug]</color> Step 5: Dialog 1 - Remi looks at Vent.");
        UpdateRemiLook(ventTarget);
        SetDialogText("Remi: Lubang ini… aku bisa masuk. Tubuhku lebih kecil dari kalian. Biarkan aku yang coba.");
        yield return new WaitForSeconds(8f);

        Debug.Log("<color=yellow>[Emperor Debug]</color> Step 6: Dialog 2 - Remi looks at Conductor.");
        UpdateRemiLook(conductorCameraPos);
        SetDialogText("Remi: Kalau kau percaya padaku, arahkan aku. Tekan kendaliku… aku siap.");
        yield return new WaitForSeconds(7f);

        Debug.Log("<color=yellow>[Emperor Debug]</color> Step 7: Dialog 3 - Remi finale.");
        UpdateRemiLook(conductorCameraPos);
        SetDialogText("Remi: Aku tidak bisa melompat sejauh Domi, tapi jalur sempit ini… aku yang urus.");
        yield return new WaitForSeconds(5f);

        Debug.Log("<color=yellow>[Emperor Debug]</color> Step 8: Showing Prompt 3.");
        SetDialogText(""); 
        if (promptTekan3 != null) promptTekan3.SetActive(true);
        canSwitchWithThree = true; 
    }

    void SetDialogText(string text)
    {
        if (dialogText != null)
        {
            dialogText.text = text;
            dialogText.gameObject.SetActive(true); // Memastikan teks tidak tersembunyi
        }
        else Debug.LogError("<color=red>[Emperor Debug]</color> DialogText slot is EMPTY!");
    }

    void SwitchToRemiByCommand()
    {
        canSwitchWithThree = false;
        if (promptTekan3 != null) promptTekan3.SetActive(false);
        SetFixedCamera(false);

        LockControl(conductor, conductorMovement, conductorCameraLook, true);
        LockControl(remi, remiMovement, remiCameraLook, false);

        if (mainSwitchSystem != null) mainSwitchSystem.enabled = true;
        ToggleNavMesh(true);
        Debug.Log("<color=green>[Emperor Debug]</color> Control successfully restored to Remi.");
    }

    void SetFixedCamera(bool useFixed)
    {
        if (mainCamera == null || cutsceneCameraPos == null) return;
        if (useFixed)
        {
            originalCameraParent = mainCamera.transform.parent;
            originalCameraPos = mainCamera.transform.localPosition;
            originalCameraRot = mainCamera.transform.localRotation;

            mainCamera.transform.SetParent(null);
            mainCamera.transform.position = cutsceneCameraPos.position;
            
            // PAKSA arah pandang kamera ke target agar tidak "ga jelas"
            if (cameraLookTarget != null) 
            {
                mainCamera.transform.LookAt(cameraLookTarget.position);
                Debug.Log("<color=yellow>[Emperor Debug]</color> Camera locked onto: " + cameraLookTarget.name);
            }
        }
        else
        {
            mainCamera.transform.SetParent(originalCameraParent);
            mainCamera.transform.localPosition = originalCameraPos;
            mainCamera.transform.localRotation = originalCameraRot;
            Debug.Log("<color=yellow>[Emperor Debug]</color> Camera returned to Player.");
        }
    }

    void LockControl(GameObject character, MonoBehaviour move, MonoBehaviour look, bool lockIt)
    {
        if (move != null) move.enabled = !lockIt;
        if (look != null) look.enabled = !lockIt;
        CharacterController cc = character.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = !lockIt;
    }

    void ToggleNavMesh(bool state)
    {
        NavMeshAgent ar = remi.GetComponent<NavMeshAgent>();
        if (ar != null) ar.enabled = state;
        NavMeshAgent ad = domi.GetComponent<NavMeshAgent>();
        if (ad != null) ad.enabled = state;
    }

    void Teleport()
    {
        // Memberikan sedikit offset Y (0.2f) agar kaki tidak menembus tanah
        conductor.transform.position = conductorTargetPos.position + Vector3.up * 0.2f;
        conductor.transform.rotation = conductorTargetPos.rotation;
        remi.transform.position = remiTargetPos.position + Vector3.up * 0.2f;
        remi.transform.rotation = remiTargetPos.rotation;
        domi.transform.position = domiTargetPos.position + Vector3.up * 0.2f;
        domi.transform.rotation = domiTargetPos.rotation;
    }

    void UpdateRemiLook(Transform target)
    {
        if (remi != null && target != null)
        {
            Vector3 dir = target.position - remi.transform.position;
            dir.y = 0; 
            remi.transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    IEnumerator FadeCanvas(CanvasGroup cg, float target, float duration)
    {
        float start = cg.alpha;
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, target, time / duration);
            yield return null;
        }
        cg.alpha = target;
    }
}