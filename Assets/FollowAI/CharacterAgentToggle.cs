using UnityEngine;
using UnityEngine.AI;

public class CharacterAgentToggle : MonoBehaviour
{
    [Header("Settings")]
    public string playerTag = "Player"; // Tag saat dikendalikan

    private NavMeshAgent agent;
    private bool wasPlayer = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (agent == null) return;

        // Cek apakah sekarang saya adalah Player?
        bool isPlayerNow = gameObject.CompareTag(playerTag);

        // KASUS 1: Baru berubah jadi Player (Kita matikan rem tangan)
        if (isPlayerNow && !wasPlayer)
        {
            agent.enabled = false; // MATIKAN NAVMESH BIAR GAK BERAT
            wasPlayer = true;
            // Debug.Log(gameObject.name + ": Mode Player Aktif (Agent OFF)");
        }
        // KASUS 2: Baru berubah jadi NPC (Kita nyalakan rem tangan)
        else if (!isPlayerNow && wasPlayer)
        {
            agent.enabled = true; // NYALAKAN NAVMESH BIAR BISA DIPERINTAH
            wasPlayer = false;
            // Debug.Log(gameObject.name + ": Mode NPC Aktif (Agent ON)");
        }
    }
}