using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VisionMode : MonoBehaviour
{
    public Volume visionVolume;
    public HighlightTarget[] highlights;
    public float normalEmission = 0f;
    public float visionEmission = 5f;
    private bool isVisionOn = false;
    public Camera highlightCamera;


    [System.Serializable]
    public class HighlightTarget
    {
        public Material material;
        public Color visionColor = Color.cyan; // color when vision mode is on
    }

    private void Start()
    {
        highlightCamera.gameObject.SetActive(false);
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

        // Toggle post-processing volume (greyscale, etc)
        if (visionVolume != null)
            visionVolume.enabled = isVisionOn;

        // Toggle highlight camera
        if (highlightCamera != null)
            highlightCamera.gameObject.SetActive(isVisionOn);

        // Toggle emission on highlighted objects
        foreach (HighlightTarget target in highlights)
        {
            if (isVisionOn)
            {
                target.material.EnableKeyword("_EMISSION");
                target.material.SetColor("_EmissionColor", target.visionColor * visionEmission);
            }
            else
            {
                target.material.SetColor("_EmissionColor", Color.black);
            }
        }

        DynamicGI.UpdateEnvironment();
    }

}
