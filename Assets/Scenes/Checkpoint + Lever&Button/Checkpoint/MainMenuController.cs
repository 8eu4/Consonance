using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public GameObject playerObject; // assign in inspector if needed

    // Called by "New Game" button
    public void NewGame()
    {
        if (RespawnManager.Instance != null)
        {
            // Clear saved checkpoint AND reset semua checkpoint di scene
            RespawnManager.Instance.ClearSavedCheckpoint();

            // Find player jika belum di-assign
            if (playerObject == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) playerObject = p;
            }

            // Respawn ke default langsung
            if (playerObject != null)
                RespawnManager.Instance.RespawnPlayer(playerObject);
        }
    }

    // Called by "Continue" button
    public void ContinueGame()
    {
        if (RespawnManager.Instance != null)
        {
            if (playerObject == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) playerObject = p;
            }
            if (playerObject != null)
            {
                bool loaded = RespawnManager.Instance.ContinueGameRespawn(playerObject);
                Debug.Log("Continue used saved checkpoint: " + loaded);
            }
        }
    }

    public void Option()
    {
        // implementasi opsi
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
