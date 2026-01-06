using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Wajib untuk Slider
using System.Collections;

public class LoaderManager : MonoBehaviour
{
    public static LoaderManager Instance { get; private set; }

    [Header("Settings")]
    public float minLoadTime = 3.0f; // Minimal nunggu 3 detik

    [Header("UI References")]
    [Tooltip("Canvas atau Panel utama Loading Screen")]
    public GameObject loadingScreenObject;
    [Tooltip("Slider untuk Progress Bar")]
    public Slider progressBar;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Sembunyikan saat awal game
        if (loadingScreenObject != null) loadingScreenObject.SetActive(false);
    }

    // PANGGIL FUNGSI INI DARI SCRIPT LAIN UNTUK PINDAH SCENE
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // 1. Munculkan Loading Screen
        if (loadingScreenObject != null)
        {
            loadingScreenObject.SetActive(true);
            if (progressBar != null) progressBar.value = 0;
        }

        // 2. Mulai Load Scene di Background
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        // PENTING: Cegah scene langsung muncul begitu selesai load
        operation.allowSceneActivation = false;

        float timer = 0f;

        // 3. Loop selama proses load berjalan ATAU waktu belum 3 detik
        while (!operation.isDone)
        {
            timer += Time.deltaTime;

            // Hitung progress "Asli" (Unity mentok di 0.9 saat allowSceneActivation = false)
            float realProgress = Mathf.Clamp01(operation.progress / 0.9f);

            // Hitung progress "Waktu" (Agar minimal 3 detik)
            float timeProgress = Mathf.Clamp01(timer / minLoadTime);

            // Kita ambil nilai TERKECIL agar bar tidak penuh sebelum 3 detik
            float displayProgress = Mathf.Min(realProgress, timeProgress);

            // Update UI Slider
            if (progressBar != null) progressBar.value = displayProgress;

            // Syarat selesai:
            // A. Load aset selesai (progress >= 0.9)
            // B. Waktu minimal sudah lewat (timer >= minLoadTime)
            if (operation.progress >= 0.9f && timer >= minLoadTime)
            {
                // Set bar ke penuh biar enak dilihat
                if (progressBar != null) progressBar.value = 1f;

                // Izinkan pindah scene
                operation.allowSceneActivation = true;
            }

            yield return null;
        }

        // 4. Sembunyikan Loading Screen setelah scene baru aktif
        if (loadingScreenObject != null) loadingScreenObject.SetActive(false);
    }
}