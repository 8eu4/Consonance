using UnityEngine;
using System.Collections;

// Enum untuk arah, bisa diakses dari skrip lain
public enum ArrowDirection { Up, Down, Left, Right }

public class ConductorAttack : MonoBehaviour
{
    [Header("References")]
    private LockToAttack lockToAttackScript;

    [Header("Animation")]
    [SerializeField] private Animator handAnimator;

    [Header("Attack Settings")]
    [SerializeField] private float swipeThreshold = 50f;

    private EnemyHealth currentTargetHealth;
    public bool isAttacking = false;

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

    public void ManualSwipe(Vector2 swipeDirection)
    {
        // Jika tidak ada panah aktif, abaikan
        if (activeArrow == null) return;

        CheckSwipe(swipeDirection);
    }

    /// <summary>
    /// Mengecek gestur swipe.
    /// Animasi dijalankan BERDASARKAN INPUT MOUSE (bukan panah).
    /// Benar/Salah dicek setelahnya.
    /// </summary>
    void CheckSwipe(Vector2 delta)
    {
        if (activeArrow == null) return;

        // 1. TENTUKAN ARAH SWIPE PLAYER (Murni dari input mouse)
        // Kita gunakan nullable enum atau flag untuk menandai jika swipe valid
        bool validSwipe = false;
        ArrowDirection playerInputDirection = ArrowDirection.Up; // Default placeholder
        float animX = 0f;
        float animY = 0f;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            // --- Swipe Horizontal ---
            if (delta.x > swipeThreshold)
            {
                playerInputDirection = ArrowDirection.Right;
                animX = 1f;
                validSwipe = true;
            }
            else if (delta.x < -swipeThreshold)
            {
                playerInputDirection = ArrowDirection.Left;
                animX = -1f;
                validSwipe = true;
            }
        }
        else
        {
            // --- Swipe Vertikal ---
            if (delta.y > swipeThreshold)
            {
                playerInputDirection = ArrowDirection.Up;
                animY = 1f;
                validSwipe = true;
            }
            else if (delta.y < -swipeThreshold)
            {
                playerInputDirection = ArrowDirection.Down;
                animY = -1f;
                validSwipe = true;
            }
        }

        // Jika swipe terlalu pendek (tidak valid), keluar
        if (!validSwipe) return;

        // 2. JALANKAN ANIMASI (Visual Feedback)
        // Karakter akan tetap memukul sesuai arah mouse, meskipun itu salah.
        TriggerAnimation(animX, animY);

        // 3. CEK LOGIKA GAME (Apakah Input Player == Permintaan Arrow?)
        if (playerInputDirection == activeArrow.Direction)
        {
            HitSuccess();
        }
        else
        {
            HitFail(); // Arah salah, tapi animasi tetap jalan
        }
    }

    /// <summary>
    /// Helper untuk memicu animasi dengan parameter yang sesuai
    /// </summary>
    void TriggerAnimation(float posX, float posY)
    {
        if (handAnimator != null)
        {
            // Reset dulu biar bersih
            handAnimator.SetFloat("AttackX", 0);
            handAnimator.SetFloat("AttackY", 0);

            // Set parameter arah sesuai input player
            handAnimator.SetFloat("AttackX", posX);
            handAnimator.SetFloat("AttackY", posY);

            // Pukul!
            handAnimator.SetTrigger("TriggerAttack");
        }
    }

    /// <summary>
    /// Dipanggil oleh Arrow saat panah ini siap menerima input
    /// </summary>
    public void SetActiveArrow(Arrow arrow)
    {
        activeArrow = arrow;
    }

    void HitSuccess()
    {
        Debug.Log("HIT SUCCESS!");

        Arrow arrowToResolve = activeArrow;
        activeArrow = null;

        if (currentTargetHealth != null)
        {
            currentTargetHealth.TakeDamage(1);
        }

        if (arrowToResolve != null)
        {
            arrowToResolve.ResolveHit();
        }
    }

    void HitFail()
    {
        Debug.Log("HIT FAIL! (Wrong Direction). But animation played.");

        // Jika salah arah, panah dianggap miss/fail
        activeArrow?.ResolveMiss_WrongSwipe();
        activeArrow = null;
    }

    public void HitMissed()
    {
        Debug.Log("HIT MISSED! (Arrow Escaped)");
        if (currentTargetHealth != null)
        {
            currentTargetHealth.Heal(1);
        }

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