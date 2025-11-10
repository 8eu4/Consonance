using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class CamRotation : MonoBehaviour
{
    public float sensX;
    public float sensY;

    Transform orientation;

    float xRotation;
    float yRotation;

    [SerializeField] private SwitchCharacter switchCharacterScript;

    GameObject Player;

    public bool isAttackLocked = false;

    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        orientation = Player.transform.Find("Orientation").transform;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (isAttackLocked) return;

        float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);

        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
        Player.transform.GetChild(0).rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public void UpdateOrientation()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        orientation = Player.transform.Find("Orientation").transform;
        yRotation = orientation.eulerAngles.y;

    }
    public void LockLookAt(Vector3 targetPoint)
    {
        // 1. Paksa kamera melihat ke target
        transform.LookAt(targetPoint);

        // 2. Ambil rotasi hasil 'LookAt'
        Vector3 currentEuler = transform.rotation.eulerAngles;

        // --- PERBAIKAN MASALAH JERK ---
        // Konversi sudut euler (0-360) ke format rotasi kita (-90 s/d 90)
        float newX = currentEuler.x;
        if (newX > 180f)
        {
            newX -= 360f; // Contoh: 350° (lihat bawah) menjadi -10°
        }

        // 3. Simpan rotasi yang sudah benar
        xRotation = Mathf.Clamp(newX, -90f, 90f);
        yRotation = currentEuler.y;

        // 4. Update rotasi player (orientation) agar WASD tetap sesuai
        if (orientation != null)
            orientation.rotation = Quaternion.Euler(0, yRotation, 0);
        if (Player != null)
            Player.transform.GetChild(0).rotation = Quaternion.Euler(0, yRotation, 0);
    }
}
