using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimpleFPSController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("Camera Settings")]
    public Camera playerCamera;
    public float mouseSensitivity = 2f;
    
    // Private Variables
    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;
    private bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        // KITA HAPUS BAGIAN KURSOR DISINI
        // Biarkan PrologueDirector yang mengatur kursor di awal game
        // Cursor.lockState = CursorLockMode.Locked; 
        // Cursor.visible = false;
    }

    // --- [BARU] FUNGSI SINKRONISASI ---
    // Panggil fungsi ini TEPAT SEBELUM script ini dinyalakan lagi
    public void SyncRotation()
    {
        if (playerCamera != null)
        {
            // Ambil sudut kamera saat ini (dari Cutscene)
            float currentX = playerCamera.transform.localEulerAngles.x;

            // Konversi sudut Unity (0-360) ke sudut Script (-90 sampai 90)
            if (currentX > 180) xRotation = currentX - 360;
            else xRotation = currentX;
        }
    }

    void Update()
    {
        // --- 1. LOGIKA KAMERA (MOUSE LOOK) ---
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        // --- 2. GROUND CHECK ---
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // --- 3. GERAK (WASD) ---
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // --- 4. LOMPAT ---
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // --- 5. GRAVITASI ---
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}