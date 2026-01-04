using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class SmartAIController : MonoBehaviour
{
    [Header("--- Setting Awal ---")]
    [Tooltip("Centang jika karakter ini adalah BOT.")]
    public bool isAIActive = true;
    [Tooltip("Jika dicentang, Bot langsung ikut saat spawn.")]
    public bool startFollowing = false;

    [Header("--- Referensi Komponen (WAJIB DIISI) ---")]
    public NavMeshAgent aiAgent;
    public Transform targetPlayer;   // Karakter Player (Conductor)
    public GameObject commandMarker; // Object Sphere Gepeng
    public TMP_Text statusText;      // Object UI TextMeshPro (Prompt)
    public LayerMask groundLayer;    // Layer Lantai (PENTING!)

    [Header("--- Setting Jarak ---")]
    public float interactDistance = 3.0f; // Jarak minimal untuk muncul teks interaksi

    // Internal Variables
    private bool isFollowing;
    private bool isCommandMode = false;
    private Vector3 commandPos;

    void Start()
    {
        // Set status awal
        isFollowing = startFollowing;

        // Pastikan Marker & Teks mati saat mulai game
        if (commandMarker != null) commandMarker.SetActive(false);
        if (statusText != null) statusText.gameObject.SetActive(false);
    }

    void Update()
    {
        // Cek jika script dimatikan atau bukan mode AI
        if (!isAIActive) return;

        // Pastikan NavMesh Agent aktif
        if (aiAgent != null && !aiAgent.enabled) aiAgent.enabled = true;

        CheckReferences(); // Cek error inspector
        HandleInput();     // Cek tombol player
        UpdateUI();        // Update teks
        MoveAI();          // Update gerakan
    }

    void HandleInput()
    {
        // ---------------------------------------------------------
        // FITUR 1: FOLLOW / WAIT (Tombol F)
        // ---------------------------------------------------------
        if (Input.GetKeyDown(KeyCode.F))
        {
            float distance = Vector3.Distance(transform.position, targetPlayer.position);

            // Cuma bisa tekan F kalau jarak dekat DAN tidak sedang mode command
            if (distance <= interactDistance && !isCommandMode)
            {
                ToggleFollow();
            }
        }

        // ---------------------------------------------------------
        // FITUR 2: COMMAND MODE (Tahan Klik Kanan Mouse)
        // ---------------------------------------------------------
        // Kita pakai Mouse Klik Kanan (1) biar enak seperti game RPG/MOBA
        if (Input.GetMouseButton(1)) 
        {
            isCommandMode = true;
            ShowMarkerPreview();

            // Saat marker muncul, Klik Kiri (0) untuk suruh jalan
            if (Input.GetMouseButtonDown(0))
            {
                GoToCommandPosition();
            }
        }
        else if (Input.GetMouseButtonUp(1)) // Saat Klik Kanan dilepas
        {
            isCommandMode = false;
            if (commandMarker != null) commandMarker.SetActive(false);
        }
    }

    // Mengatur Logika Gerak AI
    void MoveAI()
    {
        // Kalau statusnya FOLLOW, update tujuan ke posisi Player terus menerus
        if (isFollowing && targetPlayer != null)
        {
            aiAgent.SetDestination(targetPlayer.position);
        }
        // Kalau tidak follow (Command Mode), dia diam atau jalan ke titik command terakhir
        // NavMeshAgent akan otomatis jalan ke titik terakhir yang di-set di GoToCommandPosition
    }

    // Pindah mode Ikut <-> Diam
    void ToggleFollow()
    {
        isFollowing = !isFollowing;

        if (!isFollowing)
        {
            // Kalau disuruh berhenti, hapus semua path (REM MENDADAK)
            aiAgent.ResetPath();
        }
    }

    // Perintah jalan ke titik marker
    void GoToCommandPosition()
    {
        isFollowing = false; // Stop ngikutin player
        aiAgent.SetDestination(commandPos); // Jalan ke lokasi marker
        
        // Opsional: Matikan marker setelah diklik
        // isCommandMode = false; 
        // commandMarker.SetActive(false);
    }

    // Menampilkan Marker Sphere di lantai
    void ShowMarkerPreview()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Raycast hanya mendeteksi layer yang dipilih di Inspector (Ground Layer)
        if (Physics.Raycast(ray, out hit, 100f, groundLayer))
        {
            if (commandMarker != null)
            {
                commandMarker.SetActive(true);
                commandMarker.transform.position = hit.point;
                commandPos = hit.point;
            }
        }
    }

    // Mengatur Teks UI (Prompt)
    void UpdateUI()
    {
        if (targetPlayer == null || statusText == null) return;

        float distance = Vector3.Distance(transform.position, targetPlayer.position);

        // KONDISI A: Sedang Mode Command (Tahan Klik Kanan)
        if (isCommandMode)
        {
            statusText.gameObject.SetActive(true);
            statusText.text = "[Klik Kiri] Perintah Gerak";
            statusText.color = Color.yellow;
        }
        // KONDISI B: Jarak Dekat (Bisa Interaksi F)
        else if (distance <= interactDistance)
        {
            statusText.gameObject.SetActive(true);
            statusText.color = Color.white;

            if (isFollowing)
                statusText.text = "Tekan [F] Wait";
            else
                statusText.text = "Tekan [F] Follow";
        }
        // KONDISI C: Jauh (Sembunyikan Teks)
        else
        {
            statusText.gameObject.SetActive(false);
        }
    }

    void CheckReferences()
    {
        if (targetPlayer == null) Debug.LogWarning("Target Player belum diisi di Inspector!");
    }
}