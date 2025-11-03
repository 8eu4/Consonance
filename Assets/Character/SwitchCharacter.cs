using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class SwitchCharacter : MonoBehaviour
{
    [Header ("Characters")]
    [SerializeField] private GameObject Conductor;
    [SerializeField] private GameObject Domi;
    [SerializeField] private GameObject Remi;

    private int activeCharacterIndex;

    [Header ("Camera Script")]
    [SerializeField] private CamRotation camRotationScript;
    [SerializeField] private Cam camScript;

    [Header ("UI Script")]
    [SerializeField] private UI_SwitchCharacter uiSwitchCharacterScript;


    void Start()
    {
        activeCharacterIndex = 0;
        Domi.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        Remi.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        Conductor.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && activeCharacterIndex != 0) { ChangeCharacter(0); }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && activeCharacterIndex != 1) { ChangeCharacter(1); }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && activeCharacterIndex != 2) { ChangeCharacter(2); }
    }

    void ChangeCharacter(int characterIndex)
    {


        // Set kembali tag Karakter menjadi asli nya
        if (activeCharacterIndex == 0) 
        { 
            Conductor.tag = "Conductor";
            Conductor.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
            uiSwitchCharacterScript.change4HP(false);
        }
        else if (activeCharacterIndex == 1) 
        { 
            Domi.tag = "Domi";
            Domi.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
            uiSwitchCharacterScript.change2HP(1, false);
        }
        else if (activeCharacterIndex == 2) 
        { 
            Remi.tag = "Remi";
            Remi.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
            uiSwitchCharacterScript.change2HP(2, false);
        }



        // Set sekarang Player control siapa?
        if (characterIndex == 0) 
        { 
            Conductor.tag = "Player"; 
            activeCharacterIndex = 0;

            uiSwitchCharacterScript.change4HP(true);
            noFreeze(Conductor);

            Domi.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            Remi.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

        }
        else if (characterIndex == 1) 
        { 
            Domi.tag = "Player"; 
            activeCharacterIndex = 1;

            uiSwitchCharacterScript.change2HP(1, true);
            noFreeze(Domi);

            Conductor.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            Remi.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

        }
        else if (characterIndex == 2) 
        { 
            Remi.tag = "Player"; 
            activeCharacterIndex = 2;

            uiSwitchCharacterScript.change2HP(2, true);
            noFreeze(Remi);

            Conductor.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            Domi.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

        }

        // Ubah posisi kamera sesuai dengan posisi Player
        camScript.setCameraPosition();

        // Ganti orientation di sini
        camRotationScript.UpdateOrientation();
    }

    public void noFreeze(GameObject gObject)
    {
        gObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        gObject.GetComponent<Rigidbody>().constraints = ~RigidbodyConstraints.FreezePosition;
        gObject.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
    }
    public int getActiveCharacterIndex()
    {
        return activeCharacterIndex;
    }
}
