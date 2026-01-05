using UnityEngine;

public class WindPipeHazard : MonoBehaviour
{
    [Header("Wind Settings")]
    public float windForce = 15f;
    public Vector3 windDirection = Vector3.right;
    public bool isActive = true;

    [Header("Rhythm Settings")]
    public bool useRhythm = false;
    public float activeDuration = 3f;
    public float inactiveDuration = 2f;
    private float rhythmTimer;

    [Header("Visuals")]
    public ParticleSystem gasEffect;

    void Update()
    {
        // Logika Ritme untuk Pipa Kedua & Ketiga
        if (useRhythm)
        {
            rhythmTimer += Time.deltaTime;
            if (isActive && rhythmTimer >= activeDuration)
            {
                SetWind(false);
            }
            else if (!isActive && rhythmTimer >= inactiveDuration)
            {
                SetWind(true);
            }
        }
    }

    void SetWind(bool state)
    {
        isActive = state;
        rhythmTimer = 0;
        if (gasEffect != null)
        {
            if (isActive) gasEffect.Play(); else gasEffect.Stop();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isActive) return;

        // Cek apakah yang kena semburan adalah Conductor
        if (other.CompareTag("Player") || other.name.Contains("Conductor"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Berikan gaya dorong pipa
                rb.AddForce(windDirection.normalized * windForce, ForceMode.Acceleration);
            }
        }
    }
}