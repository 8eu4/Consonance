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

    // Reference ke Ground Check (biar lompatnya enak)
    // Kita pakai logic sederhana bawaan CharacterController.isGrounded

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Mengunci kursor mouse di tengah layar dan menyembunyikannya
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // --- 1. LOGIKA KAMERA (MOUSE LOOK) ---
        
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotasi Vertikal (Atas/Bawah) - Kamera saja yang nunduk/dongak
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Batasi biar ga muter 360 derajat

        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotasi Horizontal (Kiri/Kanan) - Seluruh badan Player ikut putar
        transform.Rotate(Vector3.up * mouseX);


        // --- 2. LOGIKA GRAVITASI & GROUND CHECK ---
        
        // Reset kecepatan jatuh jika sudah napak tanah
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Angka kecil biar tetap nempel tanah
        }


        // --- 3. LOGIKA BERGERAK (WASD) ---
        
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Arah gerakan relatif terhadap arah hadap player
        Vector3 move = transform.right * x + transform.forward * z;

        // Jalankan controller
        controller.Move(move * moveSpeed * Time.deltaTime);


        // --- 4. LOGIKA LOMPAT ---
        
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            // Rumus fisika lompat: v = akar(h * -2 * g)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }


        // --- 5. APLIKASI GRAVITASI ---
        
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}