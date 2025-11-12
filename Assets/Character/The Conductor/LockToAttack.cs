using UnityEngine;

[RequireComponent(typeof(ConductorHP))]
public class LockToAttack : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private CamRotation camRotationScript;

    [Header("Lock Settings")]
    [SerializeField] private float maxLockOnDistance = 30f;
    [SerializeField] private float maxLockOnAngle = 15f;

    private Transform lockedTarget = null;
    private Transform Player;

    private float mouseX_whileLocked;
    private float mouseY_whileLocked;

    private Transform camTransform;

    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player").transform;

        if (camRotationScript != null)
        {
            camTransform = camRotationScript.transform;
        }
        else
        {
            Debug.LogError("CamRotation script belum di-assign di LockToAttack!");
        }
    }

    void Update()
    {
        // 1. Selalu deteksi input mouse
        mouseX_whileLocked = Input.GetAxis("Mouse X");
        mouseY_whileLocked = Input.GetAxis("Mouse Y");

        // 2. Logika untuk memulai dan mengakhiri lock
        if (Input.GetMouseButtonDown(1)) // Saat tombol kanan mouse DITEKAN
        {
            // Cek apakah kita sedang lock atau tidak
            if (lockedTarget == null)
            {
                FindAndLockTargetAtCenter();
            }
            else
            {
                UnlockTarget();
            }
        }

        
        // 3. Update Tampilan jika sedang locked
        if (lockedTarget != null)
        {

            // Cek jika target terlalu jauh atau mati
            if (Vector3.Distance(Player.position, lockedTarget.position) > maxLockOnDistance)
            {
                UnlockTarget();
            }
            
            
            
            // TODO: Tambahkan cek jika musuh mati








        }
    }

    void FindAndLockTargetAtCenter()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform bestTarget = null;
        float smallestAngle = maxLockOnAngle; // Mulai dengan sudut toleransi maksimal

        foreach (GameObject enemy in enemies)
        {
            Transform enemyTransform = enemy.transform;

            // 1. Cek Jarak
            float distance = Vector3.Distance(Player.position, enemyTransform.position);
            if (distance > maxLockOnDistance)
            {
                continue; // Terlalu jauh, skip
            }

            // 2. Cek Angle
            Vector3 dirToEnemy = (enemyTransform.position - camTransform.position).normalized;
            float angle = Vector3.Angle(camTransform.forward, dirToEnemy);

            if (angle > maxLockOnAngle)
            {
                continue;
            }

            // 3. Cek Tembok (Linecast/Raycast)
            RaycastHit hit;
            if (Physics.Linecast(camTransform.position, enemyTransform.position, out hit))
            {
                if (hit.transform != enemyTransform)
                {
                    continue; // Musuh terhalang, skip
                }
            }

            // 4. Jika lolos semua cek, bandingkan sudutnya
            if (angle < smallestAngle)
            {
                smallestAngle = angle;
                bestTarget = enemyTransform;
            }
        }

        // 5. Kunci target terbaik yang ditemukan
        if (bestTarget != null)
        {
            lockedTarget = bestTarget;
            camRotationScript.IsAttackLocked = true;
            camRotationScript.SetLockOnTarget(bestTarget);
        }
        else
        {
            // Tidak menemukan target yang valid
            UnlockTarget();
        }
    }

    void UnlockTarget()
    {
        lockedTarget = null;
        camRotationScript.IsAttackLocked = false;
    }

    public Transform LockedTarget
    {
        get { return lockedTarget; }
        set { Transform lockedTarget = value; }
    }
}