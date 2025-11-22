// Salin dan Timpa seluruh skrip ConductorAttack.cs Anda dengan ini
using UnityEngine;
using System.Collections;

// Enum untuk arah, bisa diakses dari skrip lain
public enum ArrowDirection { Up, Down, Left, Right }

public class ConductorAttack : MonoBehaviour
{
    [Header("References")]
    private LockToAttack lockToAttackScript;

    [Header("Attack Settings")]
    [SerializeField] private float swipeThreshold = 50f;

    private EnemyHealth currentTargetHealth;
    private bool isAttacking = false;

    private Vector2 mouseStartPos;
    private bool isSwiping = false;

    private Arrow activeArrow;

    void Start()
    {
        lockToAttackScript = GetComponent<LockToAttack>();
    }

    void Update()
    {
        if (!isAttacking || activeArrow == null)
        {
            isSwiping = false;
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            mouseStartPos = Input.mousePosition;
            isSwiping = true;
        }

        if (Input.GetMouseButtonUp(0) && isSwiping)
        {
            isSwiping = false;
            Vector2 mouseEndPos = Input.mousePosition;
            Vector2 swipeDelta = mouseEndPos - mouseStartPos;

            CheckSwipe(swipeDelta);
        }
    }

    /// <summary>
    /// Mengecek apakah gestur swipe valid dan sesuai dengan panah
    /// </summary>
    void CheckSwipe(Vector2 delta)
    {
        if (activeArrow == null) return;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            // Swipe Horizontal
            if (delta.x > swipeThreshold && activeArrow.Direction == ArrowDirection.Right)
                HitSuccess();
            else if (delta.x < -swipeThreshold && activeArrow.Direction == ArrowDirection.Left)
                HitSuccess();
            else
                HitFail(); // Arah salah
        }
        else
        {
            // Swipe Vertikal
            if (delta.y > swipeThreshold && activeArrow.Direction == ArrowDirection.Up)
                HitSuccess();
            else if (delta.y < -swipeThreshold && activeArrow.Direction == ArrowDirection.Down)
                HitSuccess();
            else
                HitFail(); // Arah salah
        }
    }

    /// <summary>
    /// Dipanggil oleh Arrow saat panah ini siap menerima input
    /// </summary>
    public void SetActiveArrow(Arrow arrow)
    {
        activeArrow = arrow;
    }

    /// <summary>
    /// Dipanggil jika gestur swipe BERHASIL
    /// </summary>
    /// <summary>
    /// Dipanggil jika gestur swipe BERHASIL
    /// </summary>
    void HitSuccess()
    {
        Debug.Log("HIT SUCCESS!");

        // 1. Ambil referensi lokal ke panah SEKARANG.
        //    Ini aman dari race condition.
        Arrow arrowToResolve = activeArrow;

        // 2. Set referensi global ke null SEKARANG.
        //    Sekarang jika StopAttacking() dipanggil, 
        //    activeArrow memang sudah null, jadi tidak ada masalah.
        activeArrow = null;

        // 3. Lakukan aksi yang bisa memicu event (TakeDamage)
        if (currentTargetHealth != null)
        {
            currentTargetHealth.TakeDamage(1);
        }

        // 4. Gunakan referensi lokal yang aman untuk menyelesaikan panah
        //    Kita cek null di sini untuk keamanan ekstra,
        //    meskipun seharusnya tidak pernah null.
        if (arrowToResolve != null)
        {
            arrowToResolve.ResolveHit();
        }
    }

    /// <summary>
    /// Dipanggil jika gestur swipe GAGAL (salah arah)
    /// </summary>
    void HitFail()
    {
        Debug.Log("HIT FAIL! (Wrong Direction). Try again!");

        activeArrow?.ResolveMiss_WrongSwipe();
        activeArrow = null;
        // KOSONGKAN.
        // Kita tidak memberi penalti untuk salah arah,
        // player bisa langsung coba swipe lagi.
    }

    /// <summary>
    /// Dipanggil oleh Arrow jika player TELAT (panah lolos)
    /// </summary>
    public void HitMissed()
    {
        Debug.Log("HIT MISSED! (Arrow Escaped)");
        if (currentTargetHealth != null)
        {
            currentTargetHealth.Heal(1); // Tambah HP musuh
        }

        // Set null di sini untuk membersihkan referensi ke panah yang sudah lolos
        activeArrow = null;
    }

    // --- Metode Start/Stop ---

    public void StartAttacking(EnemyHealth targetHealth)
    {
        currentTargetHealth = targetHealth;
        isAttacking = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void StopAttacking()
    {
        currentTargetHealth = null;
        isAttacking = false;
        activeArrow = null;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}