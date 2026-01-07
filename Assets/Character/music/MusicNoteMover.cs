using UnityEngine;

public class MusicNoteMover : MonoBehaviour
{
    private Vector3 targetPos;
    private float speed;
    private Camera mainCam;
    private bool isInitialized = false;

    // Variabel Visual
    private Vector3 currentBasePos;
    private float waveFrequency;
    private float waveAmplitude;
    private float randomPhase;

    // Spread visual (Agar tidak lurus kaku garis)
    private Vector3 randomDriftDir;

    public void Initialize(Vector3 target, float moveSpeed)
    {
        targetPos = target;
        speed = moveSpeed;
        mainCam = Camera.main;

        currentBasePos = transform.position;

        // Randomize Gelombang agar terlihat "Chaotic" natural
        randomPhase = Random.Range(0f, 10f);
        waveAmplitude = Random.Range(0.2f, 0.6f); // Amplitudo lebih besar biar menyebar
        waveFrequency = Random.Range(2f, 10f);    // Frekuensi random

        // Arah drift acak (biar ada note yang melengkung ke kiri, ada yang ke kanan)
        randomDriftDir = Random.onUnitSphere;

        isInitialized = true;
        Destroy(gameObject, 4f);
    }

    void Update()
    {
        if (!isInitialized) return;

        // 1. UPDATE BASE POSITION (Gerak ke Target)
        // Lerp factor berdasarkan jarak agar makin dekat makin cepat "nempel" ke target
        // atau pakai MoveTowards biasa
        currentBasePos = Vector3.MoveTowards(currentBasePos, targetPos, speed * Time.deltaTime);

        // 2. HITUNG OFFSETS (Wave + Drift)
        float distFactor = Vector3.Distance(currentBasePos, targetPos);

        // Wave Sinus (Naik Turun / Goyang)
        Vector3 waveUpDir = mainCam != null ? mainCam.transform.up : Vector3.up;
        float waveOffset = Mathf.Sin(Time.time * waveFrequency + randomPhase) * waveAmplitude;

        // Drift (Menyebar keluar lalu mengecil saat dekat target)
        // Kita kurangi drift saat < 2 meter dari target
        float spreadMultiplier = Mathf.Clamp01(distFactor / 2f);
        Vector3 driftOffset = randomDriftDir * (0.5f * spreadMultiplier); // 0.5f adalah lebar spread tambahan

        // 3. APPLY FINAL POSITION
        transform.position = currentBasePos + (waveUpDir * waveOffset) + driftOffset;

        // 4. ROTATION & BILLBOARD
        if (mainCam)
        {
            // A. Hadapkan ke kamera
            transform.LookAt(transform.position + mainCam.transform.rotation * Vector3.forward,
                             mainCam.transform.rotation * Vector3.up);

            // B. MANUAL ROTATION FIX (Z = 180)
            // Putar 180 derajat pada sumbu Z lokal-nya sendiri
            transform.Rotate(0, 0, 180, Space.Self);
        }

        // 5. DESTROY
        if (distFactor < 0.1f)
        {
            Destroy(gameObject);
        }
    }
}