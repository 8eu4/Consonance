using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GlobalLoader : MonoBehaviour
{
    public static GlobalLoader Instance;

    public float minLoadTime = 3f;
    public LoadingScreenManager loadingScreenVisual;

    private Canvas myCanvas;
    private string targetScene;
    private float loadStartTime;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        myCanvas = GetComponent<Canvas>();
    }

    void Start()
    {
        myCanvas.enabled = false;

        loadingScreenVisual.onRevealFinished.AddListener(OnRevealFinished);
        loadingScreenVisual.onHideFinished.AddListener(OnHideFinished);
    }

    public void LoadScene(string sceneName)
    {
        targetScene = sceneName;
        loadStartTime = Time.unscaledTime;

        myCanvas.enabled = true;
        loadingScreenVisual.RevealLoadingScreen();
    }

    private void OnRevealFinished()
    {
        StartCoroutine(LoadAsync());
    }

    IEnumerator LoadAsync()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(targetScene);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
            yield return null;

        while (Time.unscaledTime - loadStartTime < minLoadTime)
            yield return null;

        op.allowSceneActivation = true;

        while (!op.isDone)
            yield return null;

        loadingScreenVisual.HideLoadingScreen();
    }

    private void OnHideFinished()
    {
        myCanvas.enabled = false;
    }
}
