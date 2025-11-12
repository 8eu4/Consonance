using UnityEngine;

public class ParticleForceFieldController : MonoBehaviour
{
    [Header("Force Field Settings")]
    public float forceStrength = 10f;
    public float maxDistance = 8f;
    public float coneAngle = 45f;
    public KeyCode blowKey = KeyCode.Mouse0;
    public KeyCode suckKey = KeyCode.Mouse1;

    [Header("References")]
    public ParticleSystemForceField forceField;
    public Transform forceOrigin;

    private bool isActive = false;

    void Start()
    {
        // Create force field if not assigned
        if (forceField == null)
        {
            GameObject forceFieldObj = new GameObject("WindForceField");
            forceField = forceFieldObj.AddComponent<ParticleSystemForceField>();

            // Configure the force field
            forceField.shape = ParticleSystemForceFieldShape.Sphere;
            forceField.startRange = 0f;
            forceField.endRange = maxDistance;
            forceField.length = maxDistance;
        }

        if (forceOrigin == null)
            forceOrigin = transform;

        // Position force field
        forceField.transform.position = forceOrigin.position;
        forceField.transform.rotation = forceOrigin.rotation;

        // Initially disable
        forceField.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKey(blowKey))
        {
            ApplyForceField(false); // Blow
        }
        else if (Input.GetKey(suckKey))
        {
            ApplyForceField(true); // Suck
        }
        else if (isActive)
        {
            // Disable when no keys pressed
            forceField.gameObject.SetActive(false);
            isActive = false;
        }
    }

    void ApplyForceField(bool isSucking)
    {
        if (!isActive)
        {
            forceField.gameObject.SetActive(true);
            isActive = true;
        }

        // Update position and rotation to follow player
        forceField.transform.position = forceOrigin.position;
        forceField.transform.rotation = forceOrigin.rotation;

        // Configure force field parameters
        forceField.directionX = 0f;
        forceField.directionY = 0f;
        forceField.directionZ = isSucking ? -1f : 1f; // Forward or backward

        // Set strength (negative for attraction/sucking, positive for repulsion/blowing)
        float strength = isSucking ? -forceStrength : forceStrength;
        forceField.gravity = strength;

        // Configure cone shape
        forceField.shape = ParticleSystemForceFieldShape.Sphere;
        forceField.rotationRandomness = Vector3.zero;

        // You can also animate these values for more dynamic effect
        forceField.directionX = Mathf.Sin(Time.time * 5f) * 0.1f; // Slight wobble
    }

    void OnDrawGizmosSelected()
    {
        if (forceOrigin == null) return;

        Gizmos.color = Color.magenta;
        Vector3 coneOrigin = forceOrigin.position;
        Vector3 forward = forceOrigin.forward * maxDistance;

        Vector3 left = Quaternion.AngleAxis(-coneAngle, forceOrigin.up) * forward;
        Vector3 right = Quaternion.AngleAxis(coneAngle, forceOrigin.up) * forward;
        Vector3 up = Quaternion.AngleAxis(coneAngle, forceOrigin.right) * forward;
        Vector3 down = Quaternion.AngleAxis(-coneAngle, forceOrigin.right) * forward;

        Gizmos.DrawRay(coneOrigin, forward);
        Gizmos.DrawRay(coneOrigin, left);
        Gizmos.DrawRay(coneOrigin, right);
        Gizmos.DrawRay(coneOrigin, up);
        Gizmos.DrawRay(coneOrigin, down);
    }
}