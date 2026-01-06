using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

public class UI_SwitchCharacter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SwitchCharacter SwitchCharacterScript;
    [SerializeField] private Image UI_ConductorHP;
    [SerializeField] private Image UI_DomiHP;
    [SerializeField] private Image UI_RemiHP;
    [SerializeField] private VisualEffect VFX_SwitchCharacter;

    [Header("UI Elements")]
    [SerializeField] private Sprite _2ActiveHP;
    [SerializeField] private Sprite _2UnactiveHP;
    [SerializeField] private Sprite _4ActiveHP;
    [SerializeField] private Sprite _4UnactiveHP;

    [Header("Color Settings")]
    public Color normalColor = Color.white;
    public Color lockedColor = Color.red;

    public void change4HP(bool isActive)
    {
        Image HP1 = UI_ConductorHP.transform.GetChild(0).GetComponent<Image>();
        Image HP2 = UI_ConductorHP.transform.GetChild(1).GetComponent<Image>();
        Image HP3 = UI_ConductorHP.transform.GetChild(2).GetComponent<Image>();
        Image HP4 = UI_ConductorHP.transform.GetChild(3).GetComponent<Image>();

        if (isActive)
        {
            HP1.sprite = _4ActiveHP;
            HP2.sprite = _4ActiveHP;
            HP3.sprite = _4ActiveHP;
            HP4.sprite = _4ActiveHP;
        }
        else
        {
            HP1.sprite = _4UnactiveHP;
            HP2.sprite = _4UnactiveHP;
            HP3.sprite = _4UnactiveHP;
            HP4.sprite = _4UnactiveHP;
        }
    }

    public void change2HP(int Muse, bool isActive)
    {
        Image HP1 = null;
        Image HP2 = null;

        if (Muse == 1)
        {
            HP1 = UI_DomiHP.transform.GetChild(0).GetComponent<Image>();
            HP2 = UI_DomiHP.transform.GetChild(1).GetComponent<Image>();
        }
        else if (Muse == 2)
        {
            HP1 = UI_RemiHP.transform.GetChild(0).GetComponent<Image>();
            HP2 = UI_RemiHP.transform.GetChild(1).GetComponent<Image>();
        }

        if (isActive)
        {
            HP1.sprite = _2ActiveHP;
            HP2.sprite = _2ActiveHP;
        }
        else
        {
            HP1.sprite = _2UnactiveHP;
            HP2.sprite = _2UnactiveHP;
        }
    }

    // --- REVISI: Mengubah warna Induk BESERTA Anak-anaknya (HP Bars) ---
    public void SetLockedVisuals(bool isLocked, int activeCharIndex)
    {
        Color targetColor = isLocked ? lockedColor : normalColor;

        // Helper function kecil biar kodenya rapi
        void ApplyColorToHierarchy(Image parentImage, Color color)
        {
            // Ubah warna parent (Icon Karakter)
            parentImage.color = color;

            // Ubah warna semua anak (HP Bars: C_HP1, C_HP2, dst)
            foreach (Transform child in parentImage.transform)
            {
                Image childImg = child.GetComponent<Image>();
                if (childImg != null)
                {
                    childImg.color = color;
                }
            }
        }

        // Logic Utama: Hanya ubah karakter yang TIDAK AKTIF
        // Karakter aktif tetap warna normal (putih)

        // 1. CONDUCTOR
        if (activeCharIndex != 0)
            ApplyColorToHierarchy(UI_ConductorHP, targetColor);
        else
            ApplyColorToHierarchy(UI_ConductorHP, normalColor);

        // 2. DOMI
        if (activeCharIndex != 1)
            ApplyColorToHierarchy(UI_DomiHP, targetColor);
        else
            ApplyColorToHierarchy(UI_DomiHP, normalColor);

        // 3. REMI
        if (activeCharIndex != 2)
            ApplyColorToHierarchy(UI_RemiHP, targetColor);
        else
            ApplyColorToHierarchy(UI_RemiHP, normalColor);
    }

    public void PlayVFX_SwitchCharacter()
    {
        VFX_SwitchCharacter.SendEvent("in-bottom");
        Invoke(nameof(StopVFX_SwitchCharacter), 0.2f);
    }

    public void StopVFX_SwitchCharacter()
    {

    }
}