using UnityEngine;

public class SteamPipe : MonoBehaviour
{
    [Header("Wind Settings")]
    public Vector3 pushDirection = Vector3.right; // Arah semburan (Ganti ke Kiri/Kanan sesuai posisi pipa)
    public float pushForce = 20f;                // Kekuatan dorongan pipa

    private void OnTriggerStay(Collider other)
    {
        // Mengecek apakah yang lewat adalah Conductor atau Muse yang punya Rigidbody
        Rigidbody rb = other.GetComponent<Rigidbody>();
        
        if (rb != null)
        {
            // Memberikan gaya dorong terus menerus selama di dalam area steam
            rb.AddForce(pushDirection.normalized * pushForce, ForceMode.Force);
        }
    }

    // Visualisasi arah angin di Scene View agar Emperor tidak bingung
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, pushDirection.normalized * 2f);
    }
}