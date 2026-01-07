using UnityEngine;
using UnityEngine.Events;

public class ProjectileSensor : MonoBehaviour
{
    [Header("Detection Settings")]
    public string targetTag = "PlayerAttack"; 

    [Header("Events")]
    public UnityEvent onHit; 

    private bool hasTriggered = false;

    // Kita pakai OnTriggerEnter lagi biar log-nya gak spamming parah
    private void OnTriggerEnter(Collider other)
    {
        // --- DEBUG AREA ---
        // Ini akan muncul di Console kalau ADA APAPUN yang nyentuh sensor ini
        Debug.Log($"⚠️ [SENSOR DETECTED] Benda masuk: '{other.name}' | Tag: '{other.tag}' | Layer: {other.gameObject.layer}");
        // ------------------

        if (!hasTriggered)
        {
            // Cek Tag
            if (other.CompareTag(targetTag))
            {
                Debug.Log("✅ [SUCCESS] Tag Sesuai! Mengirim sinyal...");
                hasTriggered = true; 
                onHit.Invoke();
            }
            else
            {
                Debug.Log($"❌ [FAIL] Tag tidak cocok. Target: '{targetTag}', Yang masuk: '{other.tag}'");
            }
        }
    }
    
    // Cadangan kalau string-nya spawn di dalam
    private void OnTriggerStay(Collider other)
    {
        if(!hasTriggered && other.CompareTag(targetTag))
        {
             Debug.Log("✅ [SUCCESS-STAY] Deteksi via Stay!");
             hasTriggered = true; 
             onHit.Invoke();
        }
    }

    public void ResetSensor()
    {
        hasTriggered = false;
    }
}