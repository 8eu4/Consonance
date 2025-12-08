using UnityEngine;

public class PlayerRespawner : MonoBehaviour, IRespawnReset
{
    [Header("Optional")]
    public bool registerDefaultOnStart = true; // register this transform as default spawn at Start()

    void Start()
    {
        if (registerDefaultOnStart && RespawnManager.Instance != null)
        {
            RespawnManager.Instance.RegisterDefaultSpawn(transform);
        }
    }

    // Call this when player "dies"
    public void Die()
    {
        // Here you can play death effect / disable input then respawn
        RespawnManager.Instance.RespawnPlayer(gameObject);
    }

    // Called by RespawnManager after moving player
    public void OnRespawn()
    {
        // Reset health, states, animations, etc. Example:
        //var hp = GetComponent<Health>();
        //if (hp != null) hp.ResetHealthToFull();

        // Re-enable input, reset flags, etc.
    }
}
