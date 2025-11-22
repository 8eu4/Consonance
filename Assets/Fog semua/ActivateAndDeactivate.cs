using UnityEngine;
using System.Collections;

public class ActivateAndDeactivate : MonoBehaviour
{
    [Header("Object Settings")]
    public GameObject objectToActivateAndDeactivate; // parent berisi ParticleSystem
    public ParticleSystem myParticleSystem;

    [Header("Fade Settings")]
    public float fadeOutTime = 1.5f;
    public float defaultEmissionRate = 50f; // rate normal, sesuaikan dengan particle kamu

    private bool isFading = false;

    void Start()
    {
        if (myParticleSystem == null)
        {
            Debug.LogError("Particle System belum di-assign!");
            return;
        }

        // Pastikan emission aktif dan particle nyala di awal
        var emission = myParticleSystem.emission;
        emission.rateOverTime = new ParticleSystem.MinMaxCurve(defaultEmissionRate);

        // Jika objek diaktifkan dari awal, play langsung
        if (objectToActivateAndDeactivate.activeSelf)
        {
            myParticleSystem.Clear();
            myParticleSystem.Play();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!isFading && myParticleSystem.isPlaying)
                StartCoroutine(FadeOutAndDeactivate());
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!objectToActivateAndDeactivate.activeSelf)
            {
                ActivateParticleSystem();
            }
        }
    }

    IEnumerator FadeOutAndDeactivate()
    {
        isFading = true;

        var emission = myParticleSystem.emission;
        float startRate = emission.rateOverTime.constant;
        float t = 0f;

        // Kurangi emission pelan-pelan
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            float newRate = Mathf.Lerp(startRate, 0f, t / fadeOutTime);
            emission.rateOverTime = newRate;
            yield return null;
        }

        // Stop mengeluarkan partikel baru, biarkan yang hidup fade alami
        myParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        // Tunggu sampai semua partikel mati
        yield return new WaitWhile(() => myParticleSystem.IsAlive(true));

        // Matikan object
        objectToActivateAndDeactivate.SetActive(false);
        isFading = false;
    }

    void ActivateParticleSystem()
    {
        // Aktifkan object
        objectToActivateAndDeactivate.SetActive(true);

        // Reset emission ke nilai normal
        var emission = myParticleSystem.emission;
        emission.rateOverTime = new ParticleSystem.MinMaxCurve(defaultEmissionRate);

        // Restart partikel dari nol
        myParticleSystem.Clear();
        myParticleSystem.Play();
    }
}
