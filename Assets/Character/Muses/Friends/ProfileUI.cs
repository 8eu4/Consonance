using UnityEngine;
using UnityEngine.UI;

public class ProfileUI : MonoBehaviour
{
    [Header("Main References")]
    [Tooltip("Drag script SwitchCharacter di sini")]
    [SerializeField] private SwitchCharacter switchCharacterScript;

    [Header("Remi Images")]
    [SerializeField] private Image remi_profile_stringed;
    [SerializeField] private Image remi_profile_woodwind;

    [Header("Domi Images")]
    [SerializeField] private Image domi_profile_stringed;
    [SerializeField] private Image domi_profile_woodwind;

    // Variable untuk menyimpan status terakhir agar tidak spam SetActive tiap frame
    private bool lastStringedState;
    private bool isInitialized = false;

    void Start()
    {
        // Cari otomatis jika lupa drag di inspector
        if (switchCharacterScript == null)
        {
            switchCharacterScript = FindAnyObjectByType<SwitchCharacter>();
        }

        // Force update di frame pertama
        if (switchCharacterScript != null)
        {
            UpdateIcons(switchCharacterScript.isStringedArea);
        }
    }

    void Update()
    {
    }

    public void UpdateIcons(bool isStringed)
    {
        // LOGIKA:
        // Jika isStringedArea == TRUE  -> Stringed AKTIF, Woodwind MATI
        // Jika isStringedArea == FALSE -> Stringed MATI, Woodwind AKTIF

        // 1. Set Profile Stringed (Sesuai nilai isStringed)
        if (remi_profile_stringed) remi_profile_stringed.enabled = isStringed;
        if (domi_profile_stringed) domi_profile_stringed.enabled = isStringed;

        // 2. Set Profile Woodwind (Kebalikan nilai isStringed)
        if (remi_profile_woodwind) remi_profile_woodwind.enabled = !isStringed;
        if (domi_profile_woodwind) domi_profile_woodwind.enabled = !isStringed;
    }
}