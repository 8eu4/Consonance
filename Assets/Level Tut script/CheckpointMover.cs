using UnityEngine;

public class CheckpointMover : MonoBehaviour
{
    [Header("--- SETTING TARGET ---")]
    [Tooltip("Tulis nama karakter yang boleh mengaktifkan checkpoint ini (misal: Remi)")]
    public string allowedCharacterName = "Remi";

    [Tooltip("Drag Object Titik Respawn ASLI yang terdaftar di KillZone")]
    public Transform respawnPointToMove;

    [Header("--- OPSI ---")]
    public bool destroyAfterTrigger = false; // Centang kalau checkpoint cuma sekali pakai

    private void OnTriggerEnter(Collider other)
    {
        // Cek apakah yang lewat adalah karakter yang dimaksud (misal: Remi)
        // Kita cek root-nya supaya aman
        if (other.transform.root.name.Contains(allowedCharacterName))
        {
            if (respawnPointToMove != null)
            {
                Debug.Log("🚩 CHECKPOINT REACHED: Memindahkan titik respawn " + allowedCharacterName);
                
                // PINDAHKAN Object Respawn ke posisi Trigger ini berdiri
                respawnPointToMove.position = transform.position;
                respawnPointToMove.rotation = transform.rotation;
                
                // (Opsional) Matikan trigger biar gak kepanggil terus
                if (destroyAfterTrigger) 
                {
                    Destroy(gameObject);
                }
                else
                {
                    // Atau matikan colldier-nya saja
                    Collider col = GetComponent<Collider>();
                    if (col != null) col.enabled = false;
                }
            }
            else
            {
                Debug.LogError("❌ LUPA DRAG: Masukkan object Titik Respawn ke script CheckpointMover!");
            }
        }
    }
}