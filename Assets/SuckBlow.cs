using UnityEngine;

public class SuckBlow : MonoBehaviour
{
    [Header("Settings")]
    public float forceStrength = 10f;
    public float maxDistance = 5f;
    public float coneAngle = 45f;
    public KeyCode blowKey = KeyCode.Mouse0;
    public KeyCode suckKey = KeyCode.Mouse1;

    [Header("Fog Particle System")]
    public ParticleSystem fogParticleSystem; // Assign your fog system in inspector
    public float particleForceMultiplier = 3f;
    public int maxParticlesToProcess = 1000; // Performance safeguard

    void Update()
    {
        if (Input.GetKey(blowKey) || Input.GetKey(suckKey))
        {
            bool isSucking = Input.GetKey(suckKey);
            ApplyForce(isSucking);
        }
    }

    void ApplyForce(bool isSucking)
    {
        Vector3 coneOrigin = transform.position;

        // Apply to rigidbodies (existing functionality)
        Collider[] hitColliders = Physics.OverlapSphere(coneOrigin, maxDistance);
        foreach (var hitCollider in hitColliders)
        {
            Rigidbody rb = hitCollider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                ApplyForceToRigidbody(rb, coneOrigin, isSucking);
            }
        }

        // Apply to fog particles (direct reference approach)
        if (fogParticleSystem != null)
        {
            ApplyForceToFogParticles(coneOrigin, isSucking);
        }
    }

    void ApplyForceToFogParticles(Vector3 coneOrigin, bool isSucking)
    {
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[maxParticlesToProcess];
        int numParticles = fogParticleSystem.GetParticles(particles);

        int particlesAffected = 0;

        for (int i = 0; i < numParticles && particlesAffected < maxParticlesToProcess; i++)
        {
            Vector3 particlePosition = particles[i].position;
            float distance = Vector3.Distance(coneOrigin, particlePosition);

            // Early exit if particle is too far
            if (distance > maxDistance) continue;

            Vector3 directionToParticle = (particlePosition - coneOrigin).normalized;
            float angle = Vector3.Angle(transform.forward, directionToParticle);

            if (angle <= coneAngle)
            {
                Vector3 forceDirection = isSucking ? -directionToParticle : directionToParticle;
                float distanceFactor = 1f - (distance / maxDistance);

                // Apply force to particle velocity
                Vector3 force = forceDirection * forceStrength * particleForceMultiplier * distanceFactor * Time.deltaTime;
                particles[i].velocity += force;

                // Enhanced sucking effect - particles disappear when very close
                if (isSucking && distance < maxDistance * 0.3f)
                {
                    particles[i].remainingLifetime = 0f; // Immediate disappearance
                }

                particlesAffected++;
            }
        }

        if (particlesAffected > 0)
        {
            fogParticleSystem.SetParticles(particles, numParticles);
        }
    }

    void ApplyForceToRigidbody(Rigidbody rb, Vector3 coneOrigin, bool isSucking)
    {
        Vector3 directionToTarget = (rb.transform.position - coneOrigin).normalized;
        float angle = Vector3.Angle(transform.forward, directionToTarget);

        if (angle <= coneAngle)
        {
            float distance = Vector3.Distance(coneOrigin, rb.transform.position);
            Vector3 forceDirection = isSucking ? -directionToTarget : directionToTarget;
            float distanceFactor = 1f - (distance / maxDistance);

            rb.AddForce(forceDirection * forceStrength * distanceFactor, ForceMode.Force);
        }
    }

    // ... (Keep the same Gizmos methods from previous version)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 coneOrigin = transform.position;
        Vector3 forward = transform.forward * maxDistance;

        Vector3 left = Quaternion.AngleAxis(-coneAngle, transform.up) * forward;
        Vector3 right = Quaternion.AngleAxis(coneAngle, transform.up) * forward;
        Vector3 up = Quaternion.AngleAxis(coneAngle, transform.right) * forward;
        Vector3 down = Quaternion.AngleAxis(-coneAngle, transform.right) * forward;

        Gizmos.DrawRay(coneOrigin, forward);
        Gizmos.DrawRay(coneOrigin, left);
        Gizmos.DrawRay(coneOrigin, right);
        Gizmos.DrawRay(coneOrigin, up);
        Gizmos.DrawRay(coneOrigin, down);

        DrawConeArc();
    }

    void DrawConeArc()
    {
        Vector3 forward = transform.forward;
        Vector3 startPoint = transform.position;

        Gizmos.color = Color.yellow;
        int segments = 20;
        float segmentAngle = coneAngle * 2 / segments;

        Vector3 previousPoint = startPoint + Quaternion.AngleAxis(-coneAngle, transform.up) * forward * maxDistance;
        for (int i = 0; i <= segments; i++)
        {
            float angle = -coneAngle + segmentAngle * i;
            Vector3 point = startPoint + Quaternion.AngleAxis(angle, transform.up) * forward * maxDistance;
            Gizmos.DrawLine(startPoint, point);
            if (i > 0) Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }

        previousPoint = startPoint + Quaternion.AngleAxis(-coneAngle, transform.right) * forward * maxDistance;
        for (int i = 0; i <= segments; i++)
        {
            float angle = -coneAngle + segmentAngle * i;
            Vector3 point = startPoint + Quaternion.AngleAxis(angle, transform.right) * forward * maxDistance;
            Gizmos.DrawLine(startPoint, point);
            if (i > 0) Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }
    }
}