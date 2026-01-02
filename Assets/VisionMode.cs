using UnityEngine;
using UnityEngine.Rendering;

public class VisionMode : MonoBehaviour
{
    public Volume visionVolume;
    public HighlightTarget[] highlights;
    public float normalEmission = 0f;
    public float visionEmission = 5f;
    private bool isVisionOn = false;
    public Camera HighlightCamera;

    [System.Serializable]
    public class HighlightTarget
    {
        public Material material;
        public Color visionColor = Color.cyan; // color when vision mode is on
    }

    private void Start()
    {
        HighlightCamera.transform.GetComponent<Camera>().enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            ToggleVision();
        }
    }

    void ToggleVision()
    {
        isVisionOn = !isVisionOn;

        if (visionVolume != null)
            visionVolume.enabled = isVisionOn;

        foreach (HighlightTarget target in highlights)
        {
            if (isVisionOn)
            {
                // Use bright vibrant emission color
                HighlightCamera.transform.GetComponent<Camera>().enabled = true;
                target.material.SetColor("_EmissionColor", target.visionColor * visionEmission);
            }
            else
            {
                // Turn emission off
                HighlightCamera.transform.GetComponent<Camera>().enabled = false;
                target.material.SetColor("_EmissionColor", target.visionColor * normalEmission);
            }
        }

        DynamicGI.UpdateEnvironment();
    }
}
