using UnityEngine;

public class FogWindController : MonoBehaviour
{
    public ParticleSystem fogParticles;
    public float windSmoothness = 2f;  // how quickly fog reacts
    private Vector3 targetWind;
    private ParticleSystem.VelocityOverLifetimeModule velocityModule;

    void Start()
    {
        if (fogParticles == null)
            fogParticles = GetComponent<ParticleSystem>();

        velocityModule = fogParticles.velocityOverLifetime;
    }

    void Update()
    {
        // Smoothly interpolate the velocity towards the target wind direction
        Vector3 current = new Vector3(velocityModule.x.constant, velocityModule.y.constant, velocityModule.z.constant);
        Vector3 newVel = Vector3.Lerp(current, targetWind, Time.deltaTime * windSmoothness);

        velocityModule.x = newVel.x;
        velocityModule.y = newVel.y;
        velocityModule.z = newVel.z;
    }

    public void ApplyWind(Vector3 direction, float strength)
    {
        targetWind = direction.normalized * strength;
    }

    public void StopWind()
    {
        targetWind = Vector3.zero;
    }
}
