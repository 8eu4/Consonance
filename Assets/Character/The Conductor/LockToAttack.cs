using UnityEngine;
using UnityEngine.UI;

public class LockToAttack : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private CamRotation camRotationScript;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private SwitchCharacter switchCharacterScript;
    [SerializeField] private ArrowSpawner arrowSpawnerScript;
    private ConductorAttack conductorAttackScript;

    [Header("Lock Settings")]
    [SerializeField] private float maxLockOnDistance = 30f;
    [SerializeField] private float maxLockOnRadius = 2f;

    // Pindahkan referensi ini ke atas agar bisa diakses
    private Transform lockedTarget = null;
    private EnemyHealth lockedTargetHealth = null;
    private Transform camTransform;

    void Start()
    {
        camTransform = camRotationScript.transform;
        conductorAttackScript = GetComponent<ConductorAttack>();
    }

    void Update()
    {
        // 1. Logika untuk lock/unlock
        if (Input.GetKeyDown(KeyCode.Mouse1) && switchCharacterScript.CurrentPlayer == transform)
        {
            if (lockedTarget == null)
            {
                FindAndLockTargetAtCenter();
            }
            else
            {
                UnlockTarget();
            }
        }

        // 2. Di dalam Update(), HANYA cek jarak jika kita SUDAH lock
        if (lockedTarget != null)
        {
            // Cek jika target terlalu jauh
            if (Vector3.Distance(transform.position, lockedTarget.position) > maxLockOnDistance)
            {
                UnlockTarget();
            }
        }
    }

    void FindAndLockTargetAtCenter()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform bestTarget = null; // Gunakan variabel lokal

        float smallestDistanceToRay = maxLockOnRadius;
        Ray aimRay = new Ray(camTransform.position, camTransform.forward);

        foreach (GameObject enemy in enemies)
        {
            Transform enemyTransform = enemy.transform;

            float distance = Vector3.Distance(transform.position, enemyTransform.position);
            if (distance > maxLockOnDistance) continue;

            Vector3 dirToEnemy = (enemyTransform.position - camTransform.position);
            if (Vector3.Dot(camTransform.forward, dirToEnemy) <= 0) continue;

            float distanceToRay = Vector3.Cross(aimRay.direction, enemyTransform.position - aimRay.origin).magnitude;
            if (distanceToRay > maxLockOnRadius) continue;

            RaycastHit hit;
            if (Physics.Linecast(camTransform.position, enemyTransform.position, out hit))
            {
                if (hit.transform != enemyTransform) continue;
            }

            if (distanceToRay < smallestDistanceToRay)
            {
                smallestDistanceToRay = distanceToRay;
                bestTarget = enemyTransform;
            }
        }

        // --- INI ADALAH LOGIKA "START ATTACK" YANG DIPINDAHKAN ---
        if (bestTarget != null)
        {
            lockedTarget = bestTarget;

            // Ambil health
            lockedTargetHealth = lockedTarget.GetComponent<EnemyHealth>();

            if (lockedTargetHealth != null)
            {
                // 1. Daftar event OnDied SATU KALI
                lockedTargetHealth.OnDied += HandleTargetDeath;

                // 2. Mulai skrip lain SATU KALI
                conductorAttackScript.StartAttacking(lockedTargetHealth);
                arrowSpawnerScript.StartSpawning(lockedTargetHealth, conductorAttackScript);

                // 3. Set kamera SATU KALI
                camRotationScript.IsAttackLocked = true;
                camRotationScript.SetLockOnTarget(bestTarget);
            }
            else
            {
                // Jika target tidak punya health, batalkan lock
                Debug.LogWarning("Locked target has no EnemyHealth component.");
                UnlockTarget();
            }
        }
        else
        {
            // Tidak menemukan target, pastikan unlock
            UnlockTarget();
        }
    }

    /// <summary>
    /// Dipanggil oleh event OnDied dari musuh
    /// </summary>
    void HandleTargetDeath()
    {
        Debug.Log("Target has died. Unlocking...");
        UnlockTarget();
    }

    void UnlockTarget()
    {
        // Cek ini penting untuk mencegah error jika UnlockTarget dipanggil berkali-kali
        if (lockedTarget == null) return;

        if (lockedTargetHealth != null)
        {
            // Berhenti mendaftar event
            lockedTargetHealth.OnDied -= HandleTargetDeath;
        }

        // Hentikan skrip lain
        conductorAttackScript.StopAttacking();
        arrowSpawnerScript.StopSpawning();

        // Reset kamera
        camRotationScript.IsAttackLocked = false;

        // Reset variabel
        lockedTarget = null;
        lockedTargetHealth = null;
    }

    public Transform LockedTarget
    {
        get { return lockedTarget; }
        set { lockedTarget = value; }
    }
}