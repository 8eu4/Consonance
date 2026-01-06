using UnityEngine;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("Optional")]
    public GameObject playerObject; // boleh kosong, akan dicari otomatis

    public void NewGame()
    {
        // 1. Reset save
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.NewGame();
        }

        // 2. Reset Quest UI
        if (QuestUIController.Instance != null)
        {
            QuestUIController.Instance.ResetQuest();
        }

        // 3. Ensure player reference
        EnsurePlayerReference();

        // 4. Respawn player to default spawn (in current scene). If you want to load a starter scene, do that here.
        if (RespawnManager.Instance != null && playerObject != null)
        {
            RespawnManager.Instance.RespawnPlayer(playerObject);
        }

        Debug.Log("[MainMenu] New Game started");
    }

    public void ContinueGame()
    {
        EnsurePlayerReference();

        if (SaveManager.Instance != null)
        {
            // Start coroutine to load saved scene & respawn player
            StartCoroutine(SaveManager.Instance.ContinueAndRespawn(playerObject));
        }
        else
        {
            Debug.LogWarning("[MainMenu] SaveManager missing. Can't continue.");
        }
    }

    void EnsurePlayerReference()
    {
        if (playerObject != null) return;

        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            playerObject = p;
    }

    public void Option() { /* implement options */ }

    public void ExitGame() { Application.Quit(); }
}
