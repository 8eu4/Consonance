using System.Collections;
using UnityEngine;

public class SwitchCharacter : MonoBehaviour
{
    [Header("Characters")]
    [SerializeField] private GameObject Conductor;
    [SerializeField] private GameObject Domi;
    [SerializeField] private GameObject Remi;

    [Header("Mesh (stringed / woodwind)")]
    [SerializeField] private GameObject stringed_domi_object;
    [SerializeField] private GameObject woodwind_domi_object;
    [Space]
    [SerializeField] private GameObject stringed_remi_object;
    [SerializeField] private GameObject woodwind_remi_object;

    [Header("Which Area? (stringed / woodwind)")]
    [SerializeField] private bool isStringedArea = true;

    private int activeCharacterIndex;

    [Header("Camera Script")]
    [SerializeField] private CamRotation camRotationScript;
    [SerializeField] private Cam camScript;

    [Header("UI Script")]
    [SerializeField] private UI_SwitchCharacter uiSwitchCharacterScript;
    [SerializeField] private OffScreenIndicator offScreenIndicatorScript;
    
    [Header("Other Scripts Reference")]
    [SerializeField] private VisionMode visionModeScript;
    [SerializeField] private ConductorAttack conductorAttackScript;

    [Header("Level Rules")]
    [SerializeField] private bool conductorLockedInThisScene = false;

    [Header("HandObject")]
    [SerializeField] private GameObject HandObject;

    // Variabel ini akan otomatis berubah isinya sesuai karakter yang aktif
    [Header("Tidak usah diisi v")]
    public Move currentMoveScript;

    private bool _isSwitching = false;
    private Transform _CurrentPlayer;

    void Start()
    {
        activeCharacterIndex = -1;

        if (conductorLockedInThisScene)
        {
            activeCharacterIndex = 2;
            ChangeCharacter(2);
        }
        else
        {
            activeCharacterIndex = 0;
            ChangeCharacter(0);
        }
    }

    void Update()
    {
        // Pengecekan Grounded sekarang jadi sangat simpel
        if (!IsCurrentCharacterGrounded()) return;

        bool canSwitch = CanSwitchCharacter();

        // Update UI Visual (Merah jika gak bisa switch, Putih jika bisa)
        if (uiSwitchCharacterScript != null)
        {
            // Kirim kebalikan dari canSwitch (isLocked = !canSwitch)
            uiSwitchCharacterScript.SetLockedVisuals(!canSwitch, activeCharacterIndex);
        }

        if (!canSwitch) return;

        if (Input.GetKeyDown(KeyCode.Alpha1) && activeCharacterIndex != 0)
        {
            if (conductorLockedInThisScene) return;
            DelayAndSwitchTo(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && activeCharacterIndex != 1)
        {
            DelayAndSwitchTo(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && activeCharacterIndex != 2)
        {
            DelayAndSwitchTo(2);
        }
    }

    private bool CanSwitchCharacter()
    {
        // 1. Cek Grounded (Harus di tanah)
        if (!IsCurrentCharacterGrounded()) return false;

        // 2. Cek Vision Mode (Harus dunia nyata / berwarna)
        if (visionModeScript != null && visionModeScript.IsVisionActive) return false;

        // 3. Cek Attack (Harus tidak sedang menyerang)
        // Pastikan di ConductorAttack ada bool IsAttacking atau semacamnya
        if (conductorAttackScript != null && conductorAttackScript.isAttacking) return false;

        // Jika semua aman:
        return true;
    }

    private bool IsCurrentCharacterGrounded()
    {
        // Karena currentMoveScript sudah di-update saat ChangeCharacter,
        // kita tinggal pakai saja, tidak perlu cari-cari lagi.
        if (currentMoveScript != null)
        {
            return currentMoveScript.grounded;
        }
        return true; // Default true agar tidak macet jika error
    }

    public void DelayAndSwitchTo(int characterIndex)
    {
        uiSwitchCharacterScript.PlayVFX_SwitchCharacter();
        Invoke(() => ChangeCharacter(characterIndex), 0.6f);
    }

    void Invoke(System.Action action, float delay)
    {
        StartCoroutine(InvokeRoutine(action, delay));
    }

    IEnumerator InvokeRoutine(System.Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action();
    }

    void ChangeCharacter(int characterIndex)
    {
        if (conductorLockedInThisScene && characterIndex == 0) return;

        if (conductorLockedInThisScene) Conductor.tag = "Conductor";

        // --- RESET KARAKTER LAMA ---
        if (activeCharacterIndex == 0)
        {
            Conductor.tag = "Conductor";
            Conductor.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
            uiSwitchCharacterScript.change4HP(false);
        }
        else if (activeCharacterIndex == 1)
        {
            Domi.tag = "Domi";
            stringed_domi_object.SetActive(isStringedArea);
            woodwind_domi_object.SetActive(!isStringedArea);
            uiSwitchCharacterScript.change2HP(1, false);
        }
        else if (activeCharacterIndex == 2)
        {
            Remi.tag = "Remi";
            stringed_remi_object.SetActive(isStringedArea);
            woodwind_remi_object.SetActive(!isStringedArea);
            uiSwitchCharacterScript.change2HP(2, false);
        }

        // --- SET KARAKTER BARU ---
        if (characterIndex == 0)
        {
            Conductor.tag = "Player";
            activeCharacterIndex = 0;
            CurrentPlayer = Conductor.transform;

            uiSwitchCharacterScript.change4HP(true);
            noFreeze(Conductor);

            Domi.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            Remi.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

            HandObject.SetActive(true);
        }
        else if (characterIndex == 1)
        {
            Domi.tag = "Player";
            activeCharacterIndex = 1;
            CurrentPlayer = Domi.transform;

            uiSwitchCharacterScript.change2HP(1, true);
            noFreeze(Domi);

            Conductor.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            Remi.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

            HandObject.SetActive(false);
        }
        else if (characterIndex == 2)
        {
            Remi.tag = "Player";
            activeCharacterIndex = 2;
            CurrentPlayer = Remi.transform;

            uiSwitchCharacterScript.change2HP(2, true);
            noFreeze(Remi);

            Conductor.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            Domi.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

            HandObject.SetActive(false);
        }

        // --- LOGIKA BARU: Update Move Script ---
        // Kita ambil script Move dari karakter yang BARU saja dipilih
        if (CurrentPlayer != null)
        {
            currentMoveScript = CurrentPlayer.GetComponent<Move>();
        }

        // --- UPDATE KAMERA ---
        camScript.setCameraPosition();
        camRotationScript.SetCharacter(CurrentPlayer); // Ini memanggil script CamRotation yang saya kasih sebelumnya

        // FINAL OVERRIDE for rail conductor
        if (conductorLockedInThisScene)
        {
            Rigidbody rb = Conductor.GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        }
    }

    public bool isSwitching
    {
        get { return _isSwitching; }
        set { _isSwitching = value; }
    }

    public void noFreeze(GameObject gObject)
    {
        gObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        gObject.GetComponent<Rigidbody>().constraints = ~RigidbodyConstraints.FreezePosition;

        if (activeCharacterIndex == 0)
        {
            gObject.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
        }
        else if (activeCharacterIndex == 1)
        {
            stringed_domi_object.SetActive(false);
            woodwind_domi_object.SetActive(false);
        }
        else if (activeCharacterIndex == 2)
        {
            stringed_remi_object.SetActive(false);
            woodwind_remi_object.SetActive(false);
        }
    }

    public int GetActiveCharacterIndex()
    {
        return activeCharacterIndex;
    }
    public Transform CurrentPlayer
    {
        get { return _CurrentPlayer; }
        set { _CurrentPlayer = value; }
    }
}