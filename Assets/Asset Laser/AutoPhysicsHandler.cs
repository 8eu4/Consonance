using UnityEngine;
using UnityEngine.AI;

public class AutoPhysicsHandler : MonoBehaviour
{
    private Rigidbody rb;
    private NavMeshAgent agent;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // Cek apakah object ini sedang menjadi "Player"?
        bool amIPlayer = gameObject.CompareTag("Player");

        if (amIPlayer)
        {
            // --- MODE PLAYER (KITA KENDALIKAN) ---
            // Kinematic MATI supaya bisa kena fisika/input keyboard
            if (rb.isKinematic == true) rb.isKinematic = false;
            
            // Matikan Agent biar gak nge-lock posisi (bentrok sama Rigidbody)
            if (agent != null && agent.enabled) agent.enabled = false;
        }
        else
        {
            // --- MODE AI (FOLLOWER) ---
            // Kinematic NYALA supaya gerakan full diatur NavMesh (gak geter)
            if (rb.isKinematic == false) rb.isKinematic = true;

            // Nyalakan Agent buat follow
            if (agent != null && !agent.enabled) agent.enabled = true;
        }
    }
}