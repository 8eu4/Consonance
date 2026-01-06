using UnityEngine;
using UnityEngine.Events; // WAJIB ADA

public class LoadingScreenManager : MonoBehaviour
{
    private Animator _animatorComponent;

    // Event untuk lapor ke GlobalLoader
    public UnityEvent onRevealFinished;
    public UnityEvent onHideFinished;

    private void Start()
    {
        _animatorComponent = GetComponent<Animator>();
    }

    public void RevealLoadingScreen()
    {
        if (_animatorComponent) _animatorComponent.SetTrigger("Reveal");
    }

    public void HideLoadingScreen()
    {
        if (_animatorComponent) _animatorComponent.SetTrigger("Hide");
    }

    // --- BAGIAN INI YANG SEBELUMNYA ERROR ---
    // Sekarang kita pakai Event, bukan cari DemoSceneManager lagi.

    public void OnFinishedReveal()
    {
        // Panggil event (Lapor ke GlobalLoader)
        onRevealFinished?.Invoke();
    }

    public void OnFinishedHide()
    {
        onHideFinished?.Invoke();
    }
}