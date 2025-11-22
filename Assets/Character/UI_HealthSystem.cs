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

    [Space]
    [SerializeField] private Image C_HP1;
    [SerializeField] private Image C_HP2;
    [SerializeField] private Image C_HP3;
    [SerializeField] private Image C_HP4;

    [SerializeField] private Image D_HP1;
    [SerializeField] private Image D_HP2;

    [SerializeField] private Image R_HP1;
    [SerializeField] private Image R_HP2;

    private bool Conductor_onHit;
    private bool Domi_onHit;
    private bool Remi_onHit;

    private Color start;
    private Color end;
    void Start()
    {
        Conductor_onHit = false;
        Domi_onHit = false;
        Remi_onHit = false;

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

            if (ConductorHP_Script.CurrentHP == 4)
                StartCoroutine(FadeHP(C_HP4, start, end, 1.5f, () => ConductorHP_Script.CurrentHP = 3));
            else if (ConductorHP_Script.CurrentHP == 3)
                StartCoroutine(FadeHP(C_HP3, start, end, 1.5f, () => ConductorHP_Script.CurrentHP = 2));
            else if (ConductorHP_Script.CurrentHP == 2)
                StartCoroutine(FadeHP(C_HP2, start, end, 1.5f, () => ConductorHP_Script.CurrentHP = 1));
            else if (ConductorHP_Script.CurrentHP == 1)
                StartCoroutine(FadeHP(C_HP1, start, end, 1.5f, () => ConductorHP_Script.CurrentHP = 0));
        }
        if (Domi_onHit)
        {
            Domi_onHit = false;

            if (DomiHP_Script.CurrentHP == 2)
                StartCoroutine(FadeHP(D_HP2, start, end, 1.5f, () => DomiHP_Script.CurrentHP = 1));
            else if (DomiHP_Script.CurrentHP == 1)
                StartCoroutine(FadeHP(D_HP1, start, end, 1.5f, () => DomiHP_Script.CurrentHP = 0));
        }
        if (Remi_onHit)
        {
            Remi_onHit = false;

            if (RemiHP_Script.CurrentHP == 2)
                StartCoroutine(FadeHP(R_HP2, start, end, 1.5f, () => RemiHP_Script.CurrentHP = 1));
            else if (RemiHP_Script.CurrentHP == 1)
                StartCoroutine(FadeHP(R_HP1, start, end, 1.5f, () => RemiHP_Script.CurrentHP = 0));
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
