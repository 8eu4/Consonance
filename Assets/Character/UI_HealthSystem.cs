using NUnit;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_HealthSystem : MonoBehaviour
{
    [Header("Scripts")]
    [SerializeField] private ConductorHP ConductorHP_Script;
    [SerializeField] private DomiHP DomiHP_Script;
    [SerializeField] private RemiHP RemiHP_Script;
    [Header("UI HP")]
    [SerializeField] private Image UI_ConductorHP;
    [SerializeField] private Image UI_DomiHP;
    [SerializeField] private Image UI_RemiHP;

    private bool Conductor_onHit;
    private bool Domi_onHit;
    private bool Remi_onHit;

    private Image C_HP1;
    private Image C_HP2;
    private Image C_HP3;
    private Image C_HP4;

    private Image D_HP1;
    private Image D_HP2;

    private Image R_HP1;
    private Image R_HP2;

    private Color start;
    private Color end;
    void Start()
    {
        Conductor_onHit = ConductorHP_Script.onHit;
        Domi_onHit = DomiHP_Script.onHit;
        Remi_onHit = RemiHP_Script.onHit;

        C_HP1 = UI_ConductorHP.transform.GetChild(0).GetComponent<Image>();
        C_HP2 = UI_ConductorHP.transform.GetChild(1).GetComponent<Image>();
        C_HP3 = UI_ConductorHP.transform.GetChild(2).GetComponent<Image>();
        C_HP4 = UI_ConductorHP.transform.GetChild(3).GetComponent<Image>();

        D_HP1 = UI_DomiHP.transform.GetChild(0).GetComponent<Image>();
        D_HP2 = UI_DomiHP.transform.GetChild(1).GetComponent<Image>();

        R_HP1 = UI_RemiHP.transform.GetChild(0).GetComponent<Image>();
        R_HP2 = UI_RemiHP.transform.GetChild(1).GetComponent<Image>();


        start = new Color(1f, 0f, 0f);
        end = new Color(75f / 255f, 0f, 0f);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) Conductor_onHit = true;
        if (Input.GetKeyDown(KeyCode.R)) Domi_onHit = true;
        if (Input.GetKeyDown(KeyCode.T)) Remi_onHit = true;

        if (Conductor_onHit)
        {

            Conductor_onHit = false;

            if (ConductorHP_Script.HP == 4)
                StartCoroutine(FadeHP(C_HP4, start, end, 1.5f, () => ConductorHP_Script.HP--));
            else if (ConductorHP_Script.HP == 3)
                StartCoroutine(FadeHP(C_HP3, start, end, 1.5f, () => ConductorHP_Script.HP--));
            else if (ConductorHP_Script.HP == 2)
                StartCoroutine(FadeHP(C_HP2, start, end, 1.5f, () => ConductorHP_Script.HP--));
            else if (ConductorHP_Script.HP == 1)
                StartCoroutine(FadeHP(C_HP1, start, end, 1.5f, () => ConductorHP_Script.HP--));
        }
        else if (Domi_onHit)
        {
            Domi_onHit = false;

            if (DomiHP_Script.HP == 2)
                StartCoroutine(FadeHP(D_HP2, start, end, 1.5f, () => DomiHP_Script.HP--));
            else if (DomiHP_Script.HP == 1)
                StartCoroutine(FadeHP(D_HP1, start, end, 1.5f, () => DomiHP_Script.HP--));
        }
        else if (Remi_onHit)
        {
            Remi_onHit = false;

            if (RemiHP_Script.HP == 2)
                StartCoroutine(FadeHP(R_HP2, start, end, 1.5f, () => RemiHP_Script.HP--));
            else if (RemiHP_Script.HP == 1)
                StartCoroutine(FadeHP(R_HP1, start, end, 1.5f, () => RemiHP_Script.HP--));
        }

    }

    IEnumerator FadeHP(Image targetImage, Color startColor, Color endColor, float duration, System.Action onComplete = null)
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            targetImage.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }

        targetImage.color = endColor;

        onComplete?.Invoke(); // jalanin callback setelah selesai
    }

}
