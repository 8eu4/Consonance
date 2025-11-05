using UnityEngine;
using UnityEngine.VFX;

public class VFX_Debugger : MonoBehaviour
{
    [Header("Button")]
    [SerializeField] private bool triggerVFX_Dots;
    [SerializeField] private bool triggerVFX_Notes;

    [Header("VFX")]
    [SerializeField] private VisualEffect VFX_SwitchCharacter;
    [SerializeField] private int VFX_Dots;
    [SerializeField] private int VFX_Notes;

    void Start()
    {
        triggerVFX_Dots = false;
        triggerVFX_Notes = false;
    }
    void Update()
    {
        if (triggerVFX_Dots)    { VFX_SwitchCharacter.SetInt("SpawnRate_Dots", VFX_Dots); }
        else                    { VFX_SwitchCharacter.SetInt("SpawnRate_Dots", 0); }

        if (triggerVFX_Notes)   { VFX_SwitchCharacter.SetInt("SpawnRate_Notes", VFX_Notes); }
        else                    { VFX_SwitchCharacter.SetInt("SpawnRate_Notes", 0); }
    }
}
