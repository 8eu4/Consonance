using System.IO;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    string saveFileName = "savegame.json";
    string SaveFilePath => Path.Combine(Application.persistentDataPath, saveFileName);

    public SaveData CurrentSave { get; private set; } = null;
    public MainMenuController mainMenuController;

    void Awake()
    {
        void Awake()
        {
            // Cek apakah sudah ada instance lain
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Object ini tidak akan hancur saat load scene
            }
            else
            {
                // Jika sudah ada instance lain (misal saat kembali ke Main Menu),
                // hancurkan object baru ini agar tidak terjadi duplikat.
                Destroy(gameObject);
            }
        }

        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (mainMenuController.cont == true)
        {
            Debug.Log("BOTAK");
            mainMenuController.ContinueGame();
        }
    }

    #region Save / Load / NewGame

    public void SaveCheckpoint(string sceneName, Vector3 pos, Quaternion rot, int checkpointIndex, string questMessage)
    {
        CurrentSave = new SaveData(sceneName, pos, rot, questMessage, checkpointIndex);
        SaveToFile();
        Debug.Log($"[SaveManager] Checkpoint saved: scene={sceneName} pos={pos} idx={checkpointIndex} quest='{questMessage}' -> {SaveFilePath}");
    }

    void SaveToFile()
    {
        try
        {
            string json = JsonUtility.ToJson(CurrentSave, true);
            File.WriteAllText(SaveFilePath, json);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[SaveManager] Failed to save file: " + ex);
        }
    }

    public bool LoadFromFile()
    {
        if (!File.Exists(SaveFilePath)) return false;

        try
        {
            string json = File.ReadAllText(SaveFilePath);
            CurrentSave = JsonUtility.FromJson<SaveData>(json);
            return CurrentSave != null;
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[SaveManager] Failed to load file: " + ex);
            return false;
        }
    }

    public void NewGame(string startingScene = null)
    {
        // Remove file and reset in-memory
        if (File.Exists(SaveFilePath)) File.Delete(SaveFilePath);
        CurrentSave = null;

        // Also tell RespawnManager to clear in-memory checkpoint states / visuals
        if (RespawnManager.Instance != null)
            RespawnManager.Instance.ClearInMemoryCheckpoint();

        // Optionally load startingScene - leave to MainMenuController to handle scene/respawn
        Debug.Log("[SaveManager] New Game started. Save file cleared.");
    }

    #endregion

    #region Continue (Load saved scene and respawn player)

    /// <summary>
    /// Continue flow: load saved file, load saved scene (if needed), then place player to saved position and update Quest UI.
    /// Usage: StartCoroutine(SaveManager.Instance.ContinueAndRespawn(playerGameObject));
    /// </summary>
    public IEnumerator ContinueAndRespawn(GameObject player)
    {
        bool ok = LoadFromFile();
        if (!ok || CurrentSave == null)
        {
            Debug.LogWarning("[SaveManager] No saved data to continue.");
            yield break;
        }

        // If saved scene different, load it
        string targetScene = CurrentSave.sceneName;
        if (SceneManager.GetActiveScene().name != targetScene)
        {
            var loadOp = SceneManager.LoadSceneAsync(targetScene);
            while (!loadOp.isDone) yield return null;
            // Allow one frame for scene init
            yield return null;
        }
        else
        {
            // allow scene objects to finish Awake/Start
            yield return null;
        }

        // Find player (by tag)
        GameObject p = player;
        if (p == null)
            p = GameObject.FindGameObjectWithTag("Player");

        // If still null: wait a few frames for player to spawn (some projects spawn player later)
        int tries = 0;
        while (p == null && tries < 10)
        {
            yield return null;
            p = GameObject.FindGameObjectWithTag("Player");
            tries++;
        }

        if (p == null)
        {
            Debug.LogError("[SaveManager] Could not find Player object to respawn.");
            yield break;
        }

        // Move player safely (use RespawnManager helper if available)
        if (RespawnManager.Instance != null)
        {
            RespawnManager.Instance.MoveTransformSafelyPublic(p.transform, CurrentSave.position, CurrentSave.rotation);
            // Mark checkpoint visuals according to saved checkpoint index
            if (CurrentSave.checkpointIndex >= 0)
            {
                var all = GameObject.FindObjectsOfType<Checkpoint>();
                foreach (var cp in all)
                {
                    if (cp.index < CurrentSave.checkpointIndex) cp.MarkAsUsed();
                    else if (cp.index == CurrentSave.checkpointIndex) cp.MarkAsCurrent();
                    else cp.ResetCheckpoint();
                }
            }
        }
        else
        {
            // fallback direct set
            p.transform.SetPositionAndRotation(CurrentSave.position, CurrentSave.rotation);
        }

        // Update Quest UI
        if (!string.IsNullOrEmpty(CurrentSave.questMessage) && QuestUIController.Instance != null)
        {
            QuestUIController.Instance.SetQuest(CurrentSave.questMessage);
        }

        // Notify any respawn reset interface
        //var playerReset = p.GetComponent<IRespawnReset>();
        //if (playerReset != null) playerReset.OnRespawn();

        Debug.Log("[SaveManager] Continue applied: " + CurrentSave.sceneName);
        yield break;
    }

    #endregion
}
