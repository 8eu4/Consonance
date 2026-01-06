using UnityEngine;

// TARUH DI SETIAP CONDUCTOR/REMI/DOMI PREFAB

public enum CharacterType
{
    Conductor, // Ketua (Kalau mati, reset ulang)
    Remi,      // Support (Kalau mati, spawn di dekat Conductor)
    Domi       // Support (Kalau mati, spawn di dekat Conductor)
}

public class PlayerIdentity : MonoBehaviour
{
    [Header("Identity")]
    public CharacterType type; // Pilih di Inspector: Conductor/Remi/Domi

    void Start()
    {
        // 1. Lapor diri ke Manager saat lahir (Auto-Register)
        if (RespawnManager.Instance != null)
            RespawnManager.Instance.RegisterPlayer(this);
    }

    void OnDestroy()
    {
        // 2. Cabut berkas saat hancur/pindah scene (Memory Safety)
        if (RespawnManager.Instance != null)
            RespawnManager.Instance.UnregisterPlayer(this);
    }

    // Panggil fungsi ini saat darah 0
    public void Die()
    {
        Debug.Log($"[PlayerIdentity] {name} ({type}) Died.");

        if (type == CharacterType.Conductor)
        {
            // KETUA MATI -> Reset satu tim ke Checkpoint terakhir
            RespawnManager.Instance.RespawnTeam();
        }
        else
        {
            // KROCO MATI -> Cuma dia yang pindah ke dekat Ketua
            RespawnManager.Instance.RespawnSupport(transform);
        }
    }

    public void OnRespawn()
    {
        // Reset Logic (Darah Penuh, Animasi Idle, dll)
        gameObject.SetActive(true);

        // Contoh reset HP (sesuaikan script HP kamu):
        // GetComponent<Health>()?.ResetFull(); 

        Debug.Log($"{name} Respawned/Reset.");
    }
}