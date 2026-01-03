using UnityEngine;
using UnityEngine.Rendering;

public class HideMeshPermanently : MonoBehaviour
{
    [Header("Setting")]
    [Tooltip("Tag saat karakter dimainkan")]
    public string playerTag = "Player";

    [Tooltip("True = Invisible tapi ada bayangan (ShadowsOnly)")]
    public bool keepShadows = true; 

    private Renderer myRenderer;

    void Start()
    {
        myRenderer = GetComponent<Renderer>();
    }

    void Update()
    {
        if (myRenderer == null) return;

        // --- PERBAIKAN LOGIC DETEKSI ---
        bool isControlledByPlayer = false;

        // 1. Cek Object Mesh ini sendiri
        if (gameObject.CompareTag(playerTag)) isControlledByPlayer = true;
        
        // 2. Cek Bapaknya (Parent) langsung <-- INI YANG SERING KELEWAT
        else if (transform.parent != null && transform.parent.CompareTag(playerTag)) isControlledByPlayer = true;
        
        // 3. Cek Root paling atas (Jaga-jaga)
        else if (transform.root.CompareTag(playerTag)) isControlledByPlayer = true;


        // --- EKSEKUSI ---
        if (isControlledByPlayer)
        {
            // === MODE FPP (LAGI DIMAINKAN) ===
            if (keepShadows)
            {
                if (myRenderer.shadowCastingMode != ShadowCastingMode.ShadowsOnly)
                    myRenderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            }
            else
            {
                if (myRenderer.enabled) myRenderer.enabled = false;
            }
        }
        else
        {
            // === MODE IDLE (JADI NPC) ===
            if (keepShadows)
            {
                if (myRenderer.shadowCastingMode != ShadowCastingMode.On)
                    myRenderer.shadowCastingMode = ShadowCastingMode.On;
            }
            else
            {
                if (!myRenderer.enabled) myRenderer.enabled = true;
            }
        }
    }
}