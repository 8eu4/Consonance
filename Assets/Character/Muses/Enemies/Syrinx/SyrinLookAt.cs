using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    public Transform playerTarget;
    public float rotationSpeed = 3.0f;

    private bool isPaused = false;

    void Update()
    {
        if (isPaused || playerTarget == null)
            return;

        Vector3 direction = playerTarget.position - transform.position;
        direction.y = 0;

        if (direction.sqrMagnitude < 0.0001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // ❗ Pakai SATU metode rotasi saja (lebih stabil)
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    // -----------------------------
    // Pause / Resume Rotation
    // -----------------------------
    public void PauseLookAt()
    {
        isPaused = true;
    }

    public void ResumeLookAt()
    {
        isPaused = false;
    }
}
