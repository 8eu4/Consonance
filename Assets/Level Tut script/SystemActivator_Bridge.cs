using UnityEngine;
using System.Collections.Generic; // Wajib buat pakai List

public class SystemActivator_Bridge : MonoBehaviour
{
    [Header("--- DRAG OBJECT UI NYA KESINI ---")]
    [Tooltip("Drag GameObject yang menampung script-script error tadi")]
    public GameObject uiSystemObject; 

    [Header("--- KETIK NAMA SCRIPT YANG MAU DIBLOCK ---")]
    [Tooltip("Ketik nama script persis sesuai nama filenya (Besar kecil huruf berpengaruh!)")]
    public string[] scriptNames = { "OffScreenIndicator", "ArrowSpawner", "VFX_Debugger" }; 

    [Header("--- SETTING TRIGGER ---")]
    public string targetTag = "Player";
    
    private bool hasTriggered = false;
    
    // List buat nyimpen script yang berhasil ditemukan
    private List<MonoBehaviour> foundScripts = new List<MonoBehaviour>();

    void Start()
    {
        if (uiSystemObject != null)
        {
            // 1. LOOPING CARI SEMUA SCRIPT BERDASARKAN NAMA
            foreach (string name in scriptNames)
            {
                // Cari komponen (script) berdasarkan nama string
                MonoBehaviour script = uiSystemObject.GetComponent(name) as MonoBehaviour;

                if (script != null)
                {
                    // Simpan ke list biar gampang dinyalain nanti
                    foundScripts.Add(script);
                    
                    // Matikan scriptnya
                    script.enabled = false;
                    Debug.Log("🔒 Script Dimatikan: " + name);
                }
                else
                {
                    Debug.LogWarning("⚠️ Script tidak ditemukan: " + name + " (Cek typo?)");
                }
            }
        }
        else
        {
            Debug.LogError("❌ LUPA DRAG: Masukkan object UI ke slot 'Ui System Object'!");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        GameObject hitObject = other.transform.root.gameObject;

        if (hitObject.CompareTag(targetTag) || hitObject.name.Contains("Conductor") || hitObject.name.Contains("Remi"))
        {
            Debug.Log("✅ Player sampai! Menyalakan " + foundScripts.Count + " script...");

            // 2. NYALAKAN SEMUA SCRIPT YANG TADI DISIMPAN
            foreach (var script in foundScripts)
            {
                if (script != null) script.enabled = true;
            }

            hasTriggered = true;
            // Destroy(gameObject); // Aktifkan kalau mau trigger hilang setelah kena
        }
    }
}