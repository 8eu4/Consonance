using UnityEngine; 

public class CamCollider : MonoBehaviour
{
    // Masukkan kepala/badan player ke sini di Inspector nanti
    public Transform target; 
    
    // Pilih layer "Default" atau layer tembok di Inspector
    public LayerMask wallLayer; 
    
    // Kecepatan kamera, biarkan 10 dulu
    public float smoothSpeed = 10f; 

    void LateUpdate() 
    {
        // Kalau lupa masukin target, biar gak error
        if (target == null) return;

        // 1. Hitung posisi ideal kamera (di belakang player)
        // Angka 3.0f = jarak ke belakang, 1.5f = tinggi kamera
        Vector3 desiredPos = target.position - (target.forward * 3.0f) + (Vector3.up * 1.5f); 
        
        RaycastHit hit;

        // 2. Cek apakah ada tembok menghalangi pandangan (Garis dari Player ke Kamera)
        if (Physics.Linecast(target.position, desiredPos, out hit, wallLayer))
        {
            // KENA TEMBOK: Pindahkan kamera ke titik tabrakan (dimajukan dikit biar ga nembus)
            transform.position = hit.point + (hit.normal * 0.2f);
        }
        else
        {
            // AMAN: Pakai posisi ideal
            transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * smoothSpeed);
        }

        // 3. Paksa kamera selalu nengok ke Player
        transform.LookAt(target.position + Vector3.up);
    }
}