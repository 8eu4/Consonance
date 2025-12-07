using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public GameObject playerObject; // assign in inspector if needed

    // Called by "New Game" button
    public void NewGame()
    {
        if (RespawnManager.Instance != null)
        {
            RespawnManager.Instance.ClearSavedCheckpoint();
            // Immediately respawn to default
            if (playerObject != null) RespawnManager.Instance.RespawnPlayer(playerObject);
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
}
