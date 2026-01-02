using UnityEngine;
using System.Collections; 

public class UnlockSwitch_FIX : MonoBehaviour
{
    [Header("Masukkan Script SwitchCharacter Disini")]
    public MonoBehaviour switchCharScript;

    [Header("Setting")]
    public string playerTag = "Player";
    public bool destroyAfterEnter = true; 

    private bool hasTriggered = false;

    void Start()
    {
        // 1. Matikan Script SwitchCharacter saat game baru mulai
        if (switchCharScript != null)
        {
            switchCharScript.enabled = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        // Logika Deteksi Player (Cari Induk/Root)
        GameObject hitObject = other.gameObject;
        if (other.attachedRigidbody != null)
        {
            hitObject = other.attachedRigidbody.gameObject;
        }

        // Cek apakah ini Player
        if (hitObject.CompareTag(playerTag) || hitObject.name.Contains("Conductor"))
        {
            // 2. Nyalakan Script Temanmu
            if (switchCharScript != null)
            {
                switchCharScript.enabled = true;
                Debug.Log("✅ Switch Character Aktif.");
            }

            // 3. 🔥 FIX UTAMA: PAKSA TAG JADI PLAYER 🔥
            hitObject.tag = "Player";

            // Jalankan "Double Check" 
            StartCoroutine(ForceTagPlayer(hitObject));

            // 4. Paksa Unfreeze Fisika
            Rigidbody playerRb = hitObject.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.isKinematic = false;
                playerRb.constraints = RigidbodyConstraints.FreezeRotation; 
            }

            hasTriggered = true;
            
            // Hapus object trigger setelah selesai
            if (destroyAfterEnter)
            {
                GetComponent<BoxCollider>().enabled = false; 
                Destroy(gameObject, 1f); 
            }
        }
    }

    // Fungsi Pengaman
    IEnumerator ForceTagPlayer(GameObject target)
    {
        for (int i = 0; i < 10; i++)
        {
            target.tag = "Player";
            yield return null; 
        }
        Debug.Log("🔒 Tag dipaksa kunci di 'Player'. Aman.");
    }
}