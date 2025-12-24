using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [Header("Optional")]
    public GameObject playerObject; // boleh kosong, akan dicari otomatis

    // =========================
    // NEW GAME
    // =========================
    public void NewGame()
    {
        // 1. Reset checkpoint system
        if (RespawnManager.Instance != null)
        {
            RespawnManager.Instance.ClearSavedCheckpoint();
        }

        // 2. Reset Quest UI (Story Objective)
        if (QuestUIController.Instance != null)
        {
            QuestUIController.Instance.ResetQuest();
        }

        // 3. Pastikan player reference ada
        EnsurePlayerReference();

        // 4. Respawn player ke default spawn
        if (RespawnManager.Instance != null && playerObject != null)
        {
            RespawnManager.Instance.RespawnPlayer(playerObject);
        }

        Debug.Log("[MainMenu] New Game started");
    }

    // =========================
    // CONTINUE GAME
    // =========================
    public void ContinueGame()
    {
        EnsurePlayerReference();

        if (RespawnManager.Instance != null && playerObject != null)
        {
            bool loaded = RespawnManager.Instance.ContinueGameRespawn(playerObject);
            Debug.Log("[MainMenu] Continue Game used checkpoint: " + loaded);
        }
    }

    // =========================
    // UTILITIES
    // =========================
    void EnsurePlayerReference()
    {
        if (playerObject != null) return;

        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            playerObject = p;
    }

    // =========================
    // OTHER MENU
    // =========================
    public void Option()
    {
        // implementasi menu option (audio, graphics, dll)
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
