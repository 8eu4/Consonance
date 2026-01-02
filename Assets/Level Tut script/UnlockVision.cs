using UnityEngine;

public class UnlockScriptOnTrigger : MonoBehaviour
{
    [Header("Masukkan Script Vision Disini")]

    public VisionMode scriptToUnlock; // Ganti 'VisionMode' dengan nama script aslimu

    [Header("Setting")]
        public string playerTag = "Player";
    public bool destroyAfterEnter = true; // Hapus trigger setelah kena?

    // Matikan script target saat game baru mulai
    void Start()
    {
        if (scriptToUnlock != null)
        {
            scriptToUnlock.enabled = false;
        }
        else
        {
            Debug.LogWarning("Script target belum dimasukkan di Inspector!");
        }
    }

    // Dijalankan saat ada yang masuk ke area kotak ini
    void OnTriggerEnter(Collider other)
    {
        // Cek apakah yang masuk itu Player?
        if (other.CompareTag(playerTag))
        {
            if (scriptToUnlock != null)
            {
                scriptToUnlock.enabled = true; // NYALAKAN SCRIPTNYA!
                Debug.Log("Vision Mode Aktif!");
            }

            // Hapus object trigger ini biar gak kepanggil ulang (Opsional)
            if (destroyAfterEnter)
            {
                Destroy(gameObject); 
            }
        }
    }
}