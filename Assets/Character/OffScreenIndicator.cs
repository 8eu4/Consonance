using UnityEngine;
using UnityEngine.UI;

public class OffScreenIndicator : MonoBehaviour
{
    [Header("Targets to Track")]
    [SerializeField] private Transform[] targets;
    [SerializeField] private RectTransform arrowUI;
    [SerializeField] private Camera cam;

    private RectTransform[] arrows;


    void Start()
    {
        arrows = new RectTransform[targets.Length];
        for (int i = 0; i < targets.Length; i++)
        {
            arrows[i] = Instantiate(arrowUI, arrowUI.parent);
            arrows[i].gameObject.SetActive(true);
        }
        arrowUI.gameObject.SetActive(false);
    }

    void Update()
    {
        Transform currentPlayer = GameObject.FindGameObjectWithTag("Player").transform;
        float screenW = Screen.width;
        float screenH = Screen.height;
        Vector2 screenCenter = new Vector2(screenW / 2f, screenH / 2f);

        for (int i = 0; i < targets.Length; i++)
        {
            Transform target = targets[i];
            RectTransform arrow = arrows[i]; // Ambil panah yang spesifik

            if (target == null || target == currentPlayer)
            {
                arrow.gameObject.SetActive(false);
                continue;
            }

            Vector3 screenPos = cam.WorldToScreenPoint(target.position);
            bool isBehind = screenPos.z < 0;

            if (isBehind)
            {
                screenPos.x = screenW - screenPos.x;
                screenPos.y = screenH - screenPos.y;
            }

            float borderPaddingX = arrow.rect.width * arrow.localScale.x / 2f;
            float borderPaddingY = arrow.rect.height * arrow.localScale.y / 2f;

            float minX = borderPaddingX;
            float maxX = screenW - borderPaddingX;
            float minY = borderPaddingY;
            float maxY = screenH - borderPaddingY;

            bool isOffScreen =
                isBehind ||
                screenPos.x < minX || screenPos.x > maxX ||
                screenPos.y < minY || screenPos.y > maxY;

            if (isOffScreen)
            {
                arrow.gameObject.SetActive(true);

                Vector2 dir = ((Vector2)screenPos - screenCenter).normalized;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                arrow.rotation = Quaternion.Euler(0, 0, angle - 90);

                Vector2 v = (Vector2)screenPos - screenCenter;

                if (Mathf.Abs(v.x) < 0.01f && Mathf.Abs(v.y) < 0.01f)
                {
                    v = new Vector2(0f, -1f);
                }

                float halfW_border = (screenW / 2f) - borderPaddingX;
                float halfH_border = (screenH / 2f) - borderPaddingY;

                halfW_border = Mathf.Max(0.01f, halfW_border);
                halfH_border = Mathf.Max(0.01f, halfH_border);

                float screenAspect = halfW_border / halfH_border;
                float targetAspect;
                if (Mathf.Abs(v.y) < 0.01f)
                {
                    targetAspect = float.MaxValue;
                }
                else
                {
                    targetAspect = Mathf.Abs(v.x / v.y);
                }


                Vector2 newPos;

                if (targetAspect > screenAspect)
                {
                    float yVal;
                    if (Mathf.Abs(v.x) < 0.01f)
                    {
                        yVal = 0;
                    }
                    else
                    {
                        yVal = v.y * (halfW_border / Mathf.Abs(v.x));
                    }
                    newPos = new Vector2(Mathf.Sign(v.x) * halfW_border, yVal);
                }
                else
                {
                    float xVal;
                    if (Mathf.Abs(v.y) < 0.01f)
                    {
                        xVal = 0;
                    }
                    else
                    {
                        xVal = v.x * (halfH_border / Mathf.Abs(v.y));
                    }

                    newPos = new Vector2(xVal, Mathf.Sign(v.y) * halfH_border);
                }

                arrow.position = screenCenter + newPos;
            }
            else
            {
                arrow.gameObject.SetActive(false);
            }
        }
    }

}
