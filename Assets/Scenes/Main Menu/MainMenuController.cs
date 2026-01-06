using UnityEngine;
using UnityEngine.SceneManagement; // Wajib ada

public class MainMenuController : MonoBehaviour
{
    [Header("Setup")]
    public string newGameSceneName = "Level Tut"; // Pastikan nama scene benar

    [Header("UI")]
    public GameObject continueButton;

    void Start()
    {
        // Cek apakah ada save data untuk memunculkan tombol Continue
        string savedScene = "";
        if (RespawnManager.Instance != null) savedScene = RespawnManager.Instance.GetSavedSceneName();
        else savedScene = PlayerPrefs.GetString("SaveScene", "");

        if (continueButton != null)
        {
            continueButton.SetActive(!string.IsNullOrEmpty(savedScene));
        }
    }

    // --- NEW GAME ---
    public void OnButtonNewGame()
    {
        // 1. Hapus Save Data
        if (RespawnManager.Instance != null) RespawnManager.Instance.ClearSaveData();
        else PlayerPrefs.DeleteKey("SaveScene");

        Debug.Log("[Menu] Starting New Game (Direct Load)...");

        // 2. Langsung Pindah Scene (Tanpa Loading Screen)
        SceneManager.LoadScene(newGameSceneName);
    }

    // --- CONTINUE ---
    public void OnButtonLoadGame()
    {
        string savedScene = "";
        if (RespawnManager.Instance != null) savedScene = RespawnManager.Instance.GetSavedSceneName();
        else savedScene = PlayerPrefs.GetString("SaveScene", "");

        if (!string.IsNullOrEmpty(savedScene))
        {
            Debug.Log($"[Menu] Continuing to: {savedScene}");

            // Langsung Pindah Scene
            SceneManager.LoadScene(savedScene);
            // Sesampainya di sana, RespawnManager akan otomatis teleport player ke Checkpoint
        }
    }

    public void OnButtonExit() { Application.Quit(); }
}