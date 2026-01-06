using UnityEngine;
using UnityEngine.UI;

public class LockToAttack : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private CamRotation camRotationScript;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private SwitchCharacter switchCharacterScript;
    [SerializeField] private ArrowSpawner arrowSpawnerScript;
    [SerializeField] private VisionMode visionModeScript;
    private ConductorAttack conductorAttackScript;

    [Header("Lock Settings")]
    [Tooltip("Jarak maksimal untuk BISA MEMULAI lock")]
    [SerializeField] private float maxLockOnDistance = 30f;

    [Tooltip("Jarak otomatis CANCEL jika musuh menjauh melebihi angka ini")]
    [SerializeField] private float breakLockDistance = 35f;

    [SerializeField] private float maxLockOnRadius = 2f;

    // Pindahkan referensi ini ke atas agar bisa diakses
    private Transform lockedTarget = null;
    private EnemyHealth lockedTargetHealth = null;
    private Transform camTransform;

    void Start()
    {
        camTransform = camRotationScript.transform;
        conductorAttackScript = GetComponent<ConductorAttack>();
        if (visionModeScript == null)
            visionModeScript = FindAnyObjectByType<VisionMode>();
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

        // 2. CEK JARAK OTOMATIS (AUTO CANCEL)
        if (lockedTarget != null)
        {
            float currentDist = Vector3.Distance(transform.position, lockedTarget.position);

            // Jika jarak melebihi batas 'Break Distance', batalkan serangan otomatis
            if (currentDist > breakLockDistance)
            {
                // Opsional: Debug.Log("Target too far! Auto cancelling...");
                UnlockTarget();
            }
        }
    }

    void FindAndLockTargetAtCenter()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform bestTarget = null;

        float smallestDistanceToRay = maxLockOnRadius;
        Ray aimRay = new Ray(camTransform.position, camTransform.forward);

        // Ambil status Vision
        bool isVisionActive = (visionModeScript != null && visionModeScript.IsVisionActive);

        foreach (GameObject enemy in enemies)
        {
            Transform enemyTransform = enemy.transform;

            // SYARAT 1: JARAK
            float distance = Vector3.Distance(transform.position, enemyTransform.position);
            if (distance > maxLockOnDistance) continue;

            // SYARAT 2: DI DEPAN KAMERA
            Vector3 dirToEnemy = (enemyTransform.position - camTransform.position);
            if (Vector3.Dot(camTransform.forward, dirToEnemy) <= 0) continue;

            // --- SYARAT 3: LOGIKA LOCK (REVISI FINAL) ---

            // KASUS A: SEDANG DI DUNIA HITAM PUTIH (VISION ON)
            if (isVisionActive)
            {
                // Request: "harusnya itu lock hanya bisa saat di dunia nyata (bukan hitam putih)"
                // Jadi kalau sedang Vision Mode, kita DILARANG lock siapapun.
                continue;
            }

            // KASUS B: SEDANG DI DUNIA NYATA (COLOR)
            else
            {
                // Request: "tetap harus jadi visionEmission untuk lock"
                // Artinya kita harus cek VisionHeart musuh.
                VisionHeart enemyHeart = enemy.GetComponentInChildren<VisionHeart>();

                // Kalau musuh tidak punya heart, ATAU heart-nya belum di-scan (belum revealed/belum terang)
                // Maka TIDAK BISA di-lock.
                if (enemyHeart == null || !enemyHeart.IsLockable())
                {
                    continue;
                }

                // Jika lolos (Heart ada DAN Heart sudah Revealed), lanjut ke bawah (Boleh Lock).
            }
            // ---------------------------------------------

            // SYARAT 4: PRESISI AIM
            float distanceToRay = Vector3.Cross(aimRay.direction, enemyTransform.position - aimRay.origin).magnitude;
            if (distanceToRay > maxLockOnRadius) continue;

            // SYARAT 5: TIDAK TERHALANG TEMBOK
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

        if (bestTarget != null)
        {
            lockedTarget = bestTarget;
            lockedTargetHealth = lockedTarget.GetComponent<EnemyHealth>();

            if (lockedTargetHealth != null)
            {
                lockedTargetHealth.OnDied += HandleTargetDeath;
                conductorAttackScript.StartAttacking(lockedTargetHealth);
                arrowSpawnerScript.StartSpawning(lockedTargetHealth, conductorAttackScript);

                camRotationScript.IsAttackLocked = true;
                camRotationScript.SetLockOnTarget(bestTarget);
            }
            else
            {
                UnlockTarget();
            }
        }
        else
        {
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