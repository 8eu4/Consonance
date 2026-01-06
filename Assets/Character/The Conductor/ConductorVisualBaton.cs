using UnityEngine;

public class ConductorVisualBaton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator handAnimator;
    [SerializeField] private Move conductorMoveScript;
    [SerializeField] private Rigidbody conductorRb;

    void Update()
    {
        // 1. CEK REFERENCE
        if (handAnimator == null)
        {
            Debug.LogError("LUPA ASSIGN: Hand Animator kosong!");
            return;
        }
        if (conductorMoveScript == null)
        {
                Debug.LogError("LUPA ASSIGN: Move Script kosong!");
            return;
        }

        // 2. LOGIKA GERAK (WALK/IDLE)
        // Deadzone agar animasi idle cepat masuk
        Vector3 flatVel = new Vector3(conductorRb.linearVelocity.x, 0, conductorRb.linearVelocity.z);
        float currentSpeed = flatVel.magnitude < 0.1f ? 0f : flatVel.magnitude;

        handAnimator.SetFloat("Speed", currentSpeed);
        handAnimator.SetBool("IsGrounded", conductorMoveScript.grounded);
    }
}