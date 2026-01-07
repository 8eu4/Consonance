using UnityEngine;
using System.Collections;

public class LeverFadeOutObject : MonoBehaviour, ILeverAction
{
    public GameObject targetObject;
    public float fadeDuration = 1.5f;

    private Renderer[] renderers;
    private bool hasFaded = false;

    void Awake()
    {
        if (targetObject != null)
            renderers = targetObject.GetComponentsInChildren<Renderer>();
    }

    public void OnLeverToggle(bool isOn)
    {
        if (targetObject == null || hasFaded) return;

        StartCoroutine(FadeOut());
        hasFaded = true;
    }

    IEnumerator FadeOut()
    {
        float elapsed = 0f;

        // Cache original colors
        Color[][] originalColors = new Color[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = new Color[renderers[i].materials.Length];
            for (int j = 0; j < renderers[i].materials.Length; j++)
            {
                originalColors[i][j] = renderers[i].materials[j].color;
            }
        }

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);

            for (int i = 0; i < renderers.Length; i++)
            {
                for (int j = 0; j < renderers[i].materials.Length; j++)
                {
                    Color c = originalColors[i][j];
                    c.a = alpha;
                    renderers[i].materials[j].color = c;
                }
            }

            yield return null;
        }

        targetObject.SetActive(false);
    }
}
