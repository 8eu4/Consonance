using UnityEngine;
using UnityEngine.UI;

public class UI_SwitchCharacter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SwitchCharacter SwitchCharacterScript;
    [SerializeField] private Image UI_ConductorHP;
    [SerializeField] private Image UI_DomiHP;
    [SerializeField] private Image UI_RemiHP;

    [Header("UI Elements")]
    [SerializeField] private Sprite _2ActiveHP;
    [SerializeField] private Sprite _2UnactiveHP;
    [SerializeField] private Sprite _4ActiveHP;
    [SerializeField] private Sprite _4UnactiveHP;

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
}
