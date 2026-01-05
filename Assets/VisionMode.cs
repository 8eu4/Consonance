using UnityEngine;
using UnityEngine.Rendering;

public class VisionMode : MonoBehaviour
{
    [Header("References")]
    public Volume visionVolume;
    public Camera highlightCamera;
    public SwitchCharacter switchCharacterScript;
    public Camera mainCamera;

    [Header("Settings")]
    public Color baseColor = Color.cyan;
    public float normalEmission = 0.5f; // Redup (Enemy Belum Scan)
    public float visionEmission = 5f;   // Terang (Enemy Scan / Non-Enemy)

    private bool isVisionOn = false;
    private VisionHeart[] allHearts;

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        // Pastikan mati di awal
        if (highlightCamera != null) highlightCamera.gameObject.SetActive(false);
        if (visionVolume != null) visionVolume.enabled = false;
    }

    void Update()
    {
        // SYARAT 1: Harus Karakter 0 (Conductor)
        if (switchCharacterScript.GetActiveCharacterIndex() != 0)
        {
            if (isVisionOn) TurnVisionOff();
            return;
        }

        // SYARAT 2: Input V
        if (Input.GetKeyDown(KeyCode.V))
        {
            ToggleVision();
        }

        // SYARAT 3: Klik Kiri (Saat Vision ON)
        if (isVisionOn && Input.GetMouseButtonDown(0))
        {
            PerformPenetratingRaycast();
        }
    }

    void ToggleVision()
    {
        isVisionOn = !isVisionOn;
        UpdateSystemState();
    }

    void TurnVisionOff()
    {
        isVisionOn = false;
        UpdateSystemState();
    }

    void UpdateSystemState()
    {
        if (visionVolume != null) visionVolume.enabled = isVisionOn;
        if (highlightCamera != null) highlightCamera.gameObject.SetActive(isVisionOn);

        // Update visual semua heart
        allHearts = FindObjectsByType<VisionHeart>(FindObjectsSortMode.None);
        foreach (VisionHeart heart in allHearts)
        {
            if (heart != null)
            {
                heart.UpdateVisualState(isVisionOn, baseColor, normalEmission, visionEmission);
            }
        }
    }

    // --- INI KUNCI AGAR BISA KLIK YANG KETUTUPAN ---
    void PerformPenetratingRaycast()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // Gunakan RaycastAll: Dia akan mengambil SEMUA objek yang tertembus garis klik
        // (Mulai dari kulit luar, tulang, sampai organ dalam/heart)
        RaycastHit[] hits = Physics.RaycastAll(ray);

        foreach (RaycastHit hit in hits)
        {
            VisionHeart heart = hit.collider.GetComponent<VisionHeart>();

            // Jika yang tertembus adalah Heart (bukan kulit luarnya)
            if (heart != null)
            {
                // Klik heart tersebut
                heart.Interact(baseColor, visionEmission);
                return; // Stop setelah nemu satu heart, biar gak dobel klik
            }
        }
    }
}