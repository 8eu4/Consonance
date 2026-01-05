using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LevelEndingManager : MonoBehaviour
{
    [Header("--- CHARACTERS ---")]
    public GameObject conductor;
    public GameObject domi;
    public GameObject remi;

    [Header("--- MOVEMENT SCRIPTS ---")]
    public MonoBehaviour conductorMoveScript;
    public MonoBehaviour domiMoveScript;
    public MonoBehaviour remiMoveScript;
    public MonoBehaviour switchSystem; 

    [Header("--- CAMERA & POSITIONS ---")]
    public GameObject cameraHolder; 
    public Transform posConductor; 
    public Transform posDomi; 
    public Transform posRemi; 

    [Header("--- UI & AUDIO ---")]
    public CanvasGroup blackScreenGroup; // Gunakan CanvasGroup agar bisa fade
    public AudioSource voiceRemi; 
    public AudioSource sfxHarpa; 

    [Header("--- SETTINGS ---")]
    public string nextSceneName; 

    private List<GameObject> playersInArea = new List<GameObject>();
    private bool isEnding = false;

    private void Start()
    {
        if (blackScreenGroup != null) blackScreenGroup.alpha = 0; // Transparan di awal
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject rootObj = other.transform.root.gameObject;
        if (rootObj == conductor || rootObj == domi || rootObj == remi)
        {
            if (!playersInArea.Contains(rootObj))
            {
                playersInArea.Add(rootObj);
                Debug.Log("<color=yellow>[Ending]</color> Karakter Masuk: " + rootObj.name);
            }

            if (playersInArea.Count >= 3 && !isEnding)
            {
                isEnding = true;
                StartCoroutine(PlayEndingCutscene());
            }
        }
    }

    IEnumerator PlayEndingCutscene()
    {
        // 1. FREEZE PLAYER
        if (conductorMoveScript != null) conductorMoveScript.enabled = false;
        if (domiMoveScript != null) domiMoveScript.enabled = false;
        if (remiMoveScript != null) remiMoveScript.enabled = false;
        if (switchSystem != null) switchSystem.enabled = false;

        // 2. FADE TO BLACK (Layar jadi gelap)
        if (blackScreenGroup != null)
        {
            while (blackScreenGroup.alpha < 1)
            {
                blackScreenGroup.alpha += Time.deltaTime * 2f;
                yield return null;
            }
        }

        yield return new WaitForSeconds(1.0f);

        // 3. TELEPORT SAAT GELAP
        conductor.transform.position = posConductor.position;
        conductor.transform.rotation = posConductor.rotation;
        domi.transform.position = posDomi.position;
        domi.transform.rotation = posDomi.rotation;
        remi.transform.position = posRemi.position;
        remi.transform.rotation = posRemi.rotation;

        if (cameraHolder != null)
        {
            Vector3 targetDir = remi.transform.position - cameraHolder.transform.position;
            cameraHolder.transform.rotation = Quaternion.LookRotation(targetDir);
        }

        // 4. FADE OUT BLACK (Layar terbuka lagi biar kelihatan Remi-nya)
        if (blackScreenGroup != null)
        {
            while (blackScreenGroup.alpha > 0)
            {
                blackScreenGroup.alpha -= Time.deltaTime * 1.5f;
                yield return null;
            }
        }

        // 5. AUDIO & DIALOG
        if (sfxHarpa != null) sfxHarpa.Play();
        if (voiceRemi != null) voiceRemi.Play();
        Debug.Log("<color=cyan>Remi: “Kurasa… aku benar-benar berguna ya…”</color>");

        yield return new WaitForSeconds(8.0f); // Tunggu bicara selesai

        // 6. FADE TO BLACK LAGI SEBELUM PINDAH SCENE
        if (blackScreenGroup != null)
        {
            while (blackScreenGroup.alpha < 1)
            {
                blackScreenGroup.alpha += Time.deltaTime * 2f;
                yield return null;
            }
        }
        
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene(nextSceneName);
    }
}