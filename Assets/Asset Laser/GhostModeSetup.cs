using UnityEngine;

public class GhostModeSetup : MonoBehaviour
{
    // Script ini akan jalan otomatis sebelum game mulai (Awake)
    void Awake()
    {
        // PERINTAH SAKTI: "Woi Unity, Mata Laser (Raycast) JANGAN LIHAT Trigger!"
        Physics.queriesHitTriggers = false;
        
        Debug.Log("👻 GHOST MODE ON: Raycast sekarang tembus pandang Trigger!");
    }
}