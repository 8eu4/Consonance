using UnityEngine;
using System.Collections;

public class LightFlicker : MonoBehaviour
{
    private Light myLight;

    [Header("--- Setting Cahaya ---")]
    [Tooltip("Terang minimal (bisa 0 biar mati total)")]
    public float minIntensity = 0f;
    
    [Tooltip("Terang maksimal (sesuaikan dengan angka Intensity kamu tadi, misal 1000)")]
    public float maxIntensity = 2000f;

    [Header("--- Setting Kecepatan ---")]
    [Tooltip("Waktu kedip tercepat")]
    public float minWaitTime = 0.05f;
    
    [Tooltip("Waktu kedip terlama")]
    public float maxWaitTime = 0.2f;

    void Start()
    {
        myLight = GetComponent<Light>();
        if (myLight == null)
        {
            // Kalau lupa pasang di lampu, dia cari di anak-anaknya
            myLight = GetComponentInChildren<Light>();
        }
        
        StartCoroutine(FlickeringLoop());
    }

    IEnumerator FlickeringLoop()
    {
        while (true)
        {
            // 1. Acak terang redupnya
            if (myLight != null)
            {
                myLight.intensity = Random.Range(minIntensity, maxIntensity);
            }

            // 2. Tunggu waktu acak sebelum kedip lagi
            yield return new WaitForSeconds(Random.Range(minWaitTime, maxWaitTime));
        }
    }
}