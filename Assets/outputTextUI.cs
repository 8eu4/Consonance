using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class outputTextUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI SpeedText;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private GameObject MainMenuButton;
    [SerializeField] private GameObject ExitButton;
    [SerializeField] private GameObject CrossHair;
    [SerializeField] private GameObject BackgroundMenu;

    [SerializeField] private CamRotation CamRotationScript;

    private LookAt lookAtScript;
    private bool Pause;

    private float frameCount;
    private float timer;

    void Start()
    {
        Pause = false;
        lookAtScript = GameObject.FindGameObjectWithTag("CameraHolder").transform.GetChild(0).GetComponent<LookAt>();
        
    }

    void Update()
    {
        frameCount++;
        timer += Time.unscaledDeltaTime;


        if (Input.GetKeyDown(KeyCode.Q))
        {
            Pause = !Pause;
        }

        if (Pause)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 0f;
            CamRotationScript.enabled = false;
         
            MainMenuButton.SetActive(true);
            ExitButton.SetActive(true);
            BackgroundMenu.SetActive(true);
            CrossHair.SetActive(false);
            SpeedText.text = null;

        }

        else
        {
            int FPS = Mathf.RoundToInt(frameCount / timer);
            if (timer > 1)
            {
                FPS = Mathf.RoundToInt(frameCount / timer);
                frameCount = 0;
                timer -= 1;
            }

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            Time.timeScale = 1f;
            CamRotationScript.enabled = true;

            MainMenuButton.SetActive(false);
            ExitButton.SetActive(false);
            BackgroundMenu.SetActive(false);
            CrossHair.SetActive(true);
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            SpeedText.text = "speed: " + Mathf.Round(flatVel.magnitude).ToString()
                            + "\nHit: " + lookAtScript.getIsHit()
                            + "\nFPS: " + FPS;


        }


    }
}
