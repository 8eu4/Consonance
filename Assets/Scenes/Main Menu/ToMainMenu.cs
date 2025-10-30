using UnityEngine;

public class ToMainMenu : MonoBehaviour
{
    public void GoToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

}
