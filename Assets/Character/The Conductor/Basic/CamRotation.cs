using UnityEngine;

public class CamRotation : MonoBehaviour
{
    public float sensX;
    public float sensY;

    Transform orientation;

    float xRotation;
    float yRotation;

    GameObject Player; // Objek Utama (Root Character)
    Transform playerModel; // Objek Visual (PlayerObject) yang akan diputar

    private bool isAttackLocked = false;

    [Header("Lock-On Settings")]
    [SerializeField] private float smoothLockOnSpeed = 1f;
    private Transform lockOnTarget = null;

    [Header("References")]
    [SerializeField] private SwitchCharacter switchCharacterScript;
    // [SerializeField] private StringLineAttack[] StringLineAttackScript; // (Tidak dipakai di snippet ini)
    [SerializeField] private Transform Conductor;
    [SerializeField] private Transform Domi;
    [SerializeField] private Transform Remi;

    private bool DomiLineIsAttached = false;
    private bool RemiLineIsAttached = false;
    private Quaternion DomiRotation;
    private Quaternion RemiRotation;

    private bool doLerp = false;
    private Vector3 dirToTarget;
    private Quaternion targetRotation;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        targetRotation = transform.rotation;

        // Inisialisasi awal akan dipanggil oleh SwitchCharacter saat Start
    }

    void LateUpdate()
    {
        if (Player == null || orientation == null || playerModel == null) return;

        // --- LOGIKA LOCK ON ---
        if (isAttackLocked && lockOnTarget != null && switchCharacterScript.CurrentPlayer == Conductor)
        {
            dirToTarget = (lockOnTarget.position - transform.position).normalized;
            targetRotation = Quaternion.LookRotation(dirToTarget);

            if (doLerp)
            {
                smoothLockOnSpeed += Time.deltaTime * 100f;
                float angleDifference = Quaternion.Angle(transform.rotation, targetRotation);
                if (angleDifference < 0.5f) doLerp = false;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * smoothLockOnSpeed);
            }
            else
            {
                transform.rotation = targetRotation;
            }

            Vector3 currentEuler = transform.rotation.eulerAngles;
            yRotation = currentEuler.y;
            xRotation = currentEuler.x;
            if (xRotation > 180f) xRotation -= 360f;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        }
        // --- LOGIKA DOMI LINE ---
        else if (switchCharacterScript.CurrentPlayer == Domi && DomiLineIsAttached)
        {
            transform.rotation = DomiRotation;
            UpdateRotationVars();
        }
        // --- LOGIKA REMI LINE ---
        else if (switchCharacterScript.CurrentPlayer == Remi && RemiLineIsAttached)
        {
            transform.rotation = RemiRotation;
            UpdateRotationVars();
        }
        // --- FREE LOOK ---
        else
        {
            doLerp = true;
            smoothLockOnSpeed = 1f;

            float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * sensX;
            float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * sensY;

            yRotation += mouseX;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        }

        // --- UPDATE ROTASI KARAKTER ---
        // Putar object Orientation (untuk arah jalan)
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);

        // Putar Model Visual (PlayerObject)
        // Kita pakai variabel playerModel yang sudah diset di SetCharacter
        playerModel.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    // Helper untuk update variabel rotasi saat dikunci
    private void UpdateRotationVars()
    {
        Vector3 currentEuler = transform.rotation.eulerAngles;
        yRotation = currentEuler.y;
        xRotation = currentEuler.x;
        if (xRotation > 180f) xRotation -= 360f;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
    }

    public void SetLockOnTarget(Transform target)
    {
        lockOnTarget = target;
    }

    /// <summary>
    /// FUNGSI BARU: Dipanggil langsung oleh SwitchCharacter.
    /// Tidak lagi mencari berdasarkan Tag, tapi menerima Transform langsung.
    /// </summary>
    public void SetCharacter(Transform newCharacter)
    {
        Player = newCharacter.gameObject;

        // Cari Orientation di dalam player baru
        Transform findOrientation = Player.transform.Find("Orientation");
        if (findOrientation != null)
        {
            orientation = findOrientation;
        }
        else
        {
            Debug.LogError($"Object 'Orientation' tidak ditemukan di dalam {Player.name}!");
        }

        // Ambil Child ke-0 sebagai model yang akan diputar (PlayerObject)
        if (Player.transform.childCount > 0)
        {
            playerModel = Player.transform.GetChild(0);
        }
        else
        {
            Debug.LogError($"{Player.name} tidak memiliki child untuk diputar!");
        }

        // Sinkronisasi rotasi awal kamera dengan orientasi karakter baru agar tidak snapping aneh
        if (orientation != null)
            yRotation = orientation.eulerAngles.y;
    }

    public void LockLookAt(Vector3 targetPoint, GameObject character)
    {
        transform.LookAt(targetPoint);
        Vector3 currentEuler = transform.rotation.eulerAngles;

        float newX = currentEuler.x;
        if (newX > 180f) newX -= 360f;

        xRotation = Mathf.Clamp(newX, -90f, 90f);
        yRotation = currentEuler.y;

        if (Domi.gameObject == character && !DomiLineIsAttached) // Domi
        {
            DomiLineIsAttached = true;
            DomiRotation = transform.rotation;
        }
        else if (Remi.gameObject == character && !RemiLineIsAttached) // Remi
        {
            RemiLineIsAttached = true;
            RemiRotation = transform.rotation;
        }

        if (orientation != null)
            orientation.rotation = Quaternion.Euler(0, yRotation, 0);

        if (playerModel != null)
            playerModel.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public void CancelLineAttack(GameObject character)
    {
        if (Domi.gameObject == character) DomiLineIsAttached = false;
        else if (Remi.gameObject == character) RemiLineIsAttached = false;
    }

    public bool IsAttackLocked
    {
        get { return isAttackLocked; }
        set { isAttackLocked = value; }
    }

    public void UpdateOrientation()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        orientation = Player.transform.Find("Orientation").transform;
        yRotation = orientation.eulerAngles.y;

    }
}