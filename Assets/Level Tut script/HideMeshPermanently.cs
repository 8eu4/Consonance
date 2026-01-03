using UnityEngine;

public class HideMeshPermanently : MonoBehaviour
{
    [Header("Setting")]
    [Tooltip("Masukkan Tag yang digunakan saat karakter sedang dimainkan (FPP)")]
    public string playerTag = "Player";

    [Tooltip("Jika dicentang, bayangan tetap ada meskipun badannya hilang (Shadows Only)")]
    public bool keepShadows = true; 

    private Renderer myRenderer;

    void Start()
    {
        // Cari komponen Renderer (MeshRenderer atau SkinnedMeshRenderer) di object ini
        myRenderer = GetComponent<Renderer>();
    }

    void Update()
    {
        if (myRenderer == null) return;

        // Cek Tag milik Induk (Root) atau Object ini sendiri
        // Kita pakai 'root' biar aman kalau script ini ditaruh di anak (Child)
        bool isControlledByPlayer = transform.root.CompareTag(playerTag) || gameObject.CompareTag(playerTag);

        if (isControlledByPlayer)
        {
            // === MODHE FPP (LAGI DIMAINKAN) ===
            // Kita sembunyikan visualnya biar gak menghalangi kamera
            
            if (keepShadows)
            {
                // Cara Pro: Badan hilang, tapi BAYANGAN TETAP ADA
                myRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }
            else
            {
                // Cara Biasa: Hilang total
                myRenderer.enabled = false;
            }
        }
        else
        {
            // === MODE IDLE (LAGI JADI NPC/DIAM) ===
            // Munculkan badannya biar kelihatan sama karakter lain
            
            if (keepShadows)
            {
                myRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }
            else
            {
                myRenderer.enabled = true;
            }
        }
    }
}