using UnityEngine;

[DisallowMultipleComponent]
public class LeverBaseInteraction : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public GameObject interactionUI;
    public Transform leverHandle;

    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.F;
    public float triggerDistance = 3f;

    [Header("Lever Rotation")]
    public float leverOnAngle = -30f;
    public float leverOffAngle = 30f;
    public float rotateSpeed = 5f;

    [Header("Custom Behavior")]
    [Tooltip("Script apa saja yang akan bereaksi terhadap ON / OFF lever")]
    public MonoBehaviour[] leverActions;

    private bool isPlayerNear = false;
    private bool isLeverOn = false;
    private bool isRotating = false;

    private void Start()
    {
        if (interactionUI != null)
            interactionUI.SetActive(false);

        // Set lever ke posisi OFF
        if (leverHandle != null)
        {
            Vector3 euler = leverHandle.localEulerAngles;
            euler.z = leverOffAngle;
            leverHandle.localEulerAngles = euler;
        }
    }

    private void Update()
    {
        if (player == null) return;

        // Cek jarak interaksi
        isPlayerNear = Vector3.Distance(player.position, transform.position) <= triggerDistance;

        if (interactionUI != null)
            interactionUI.SetActive(isPlayerNear);

        if (!isPlayerNear || isRotating)
            return;

        // Input interaksi
        if (Input.GetKeyDown(interactKey))
        {
            ToggleLever();
        }
    }

    private void ToggleLever()
    {
        isLeverOn = !isLeverOn;

        StartCoroutine(RotateLever());

        // Panggil semua script yang implement ILeverAction
        foreach (var mb in leverActions)
        {
            if (mb is ILeverAction action)
                action.OnLeverToggle(isLeverOn);
        }
    }

    System.Collections.IEnumerator RotateLever()
    {
        isRotating = true;

        float targetAngle = isLeverOn ? leverOnAngle : leverOffAngle;
        float currentAngle = leverHandle.localEulerAngles.z;

        // Normalize angle (0–360 → -180–180)
        if (currentAngle > 180) currentAngle -= 360;

        while (Mathf.Abs(currentAngle - targetAngle) > 0.1f)
        {
            currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * rotateSpeed);

            Vector3 euler = leverHandle.localEulerAngles;
            euler.z = currentAngle;
            leverHandle.localEulerAngles = euler;

            yield return null;
        }

        // Snap to final rotation
        Vector3 finalE = leverHandle.localEulerAngles;
        finalE.z = targetAngle;
        leverHandle.localEulerAngles = finalE;

        isRotating = false;
    }

    // Gizmo untuk menunjukkan jarak interaksi
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
    }
}
