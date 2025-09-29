using UnityEngine;

public class NewGameButton : MonoBehaviour
{
    public void NewGame()
    {
        // load the scene named "Mechanic"
        UnityEngine.SceneManagement.SceneManager.LoadScene("Mechanic");
    }
}
