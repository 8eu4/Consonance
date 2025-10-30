using UnityEngine;

public class Pause : MonoBehaviour
{
    GameObject PauseMenu;
    GameObject Crosshair;
    void Start()
    {
        PauseMenu = GameObject.Find("Pause");
        Crosshair = PauseMenu.transform.GetChild(3).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(Time.timeScale == 1)
            {
                Time.timeScale = 0;
                // show pause menu
                // Cursor.visible = true;
                // Cursor.lockState = CursorLockMode.None;
                // disable player movement
                // disable player look
                // disable player actions

                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                PauseMenu.SetActive(true);
                Crosshair.SetActive(false);
            }
            else
            {
                Time.timeScale = 1;
                Crosshair.SetActive(false);
            }
        }
    }
}
