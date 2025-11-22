using UnityEngine;
using System.Collections;
using System;

public class ArrowSpawner : MonoBehaviour
{
    [Header("UI Arrows")]
    [SerializeField] private GameObject arrowPrefab; // Prefab ini HARUS punya skrip "Arrow.cs"
    [SerializeField] private RectTransform canvasParentPanel;
    [Space]
    // Posisi Spawn: Di luar layar
    [SerializeField] private RectTransform upSpawnPoint;
    [SerializeField] private RectTransform downSpawnPoint;
    [SerializeField] private RectTransform leftSpawnPoint;
    [SerializeField] private RectTransform rightSpawnPoint;
    [Space]
    // Posisi Target: Di tengah layar, tempat player harus swipe
    [SerializeField] private RectTransform upTargetPoint;
    [SerializeField] private RectTransform downTargetPoint;
    [SerializeField] private RectTransform leftTargetPoint;
    [SerializeField] private RectTransform rightTargetPoint;
    [Space]
    // Posisi Exit: Di luar layar, setelah melewati target
    [SerializeField] private RectTransform upExitPoint;
    [SerializeField] private RectTransform downExitPoint;
    [SerializeField] private RectTransform leftExitPoint;
    [SerializeField] private RectTransform rightExitPoint;

    // Referensi ini di-set oleh LockToAttack
    private ConductorAttack conductorAttackScript;
    private Health currentTargetHealth;

    private bool isSpawning = false;
    private Coroutine spawnLoopCoroutine;
    private Arrow currentActiveArrow; // Menyimpan referensi ke panah yang sedang aktif

    /// <summary>
    /// Dipanggil oleh LockToAttack untuk memulai spawn
    /// </summary>
    public void StartSpawning(Health targetHealth, ConductorAttack attackScript)
    {
        if (isSpawning) return;

        isSpawning = true;
        currentTargetHealth = targetHealth;
        conductorAttackScript = attackScript;

        spawnLoopCoroutine = StartCoroutine(SpawnLoop());
    }

    /// <summary>
    /// Dipanggil oleh LockToAttack untuk menghentikan spawn
    /// </summary>
    public void StopSpawning()
    {
        if (!isSpawning) return;

        isSpawning = false;
        if (spawnLoopCoroutine != null)
        {
            StopCoroutine(spawnLoopCoroutine);
        }

        // Hancurkan semua panah yang mungkin masih ada di layar
        foreach (Arrow arrow in canvasParentPanel.GetComponentsInChildren<Arrow>())
        {
            // Unsubscribe dari event sebelum menghancurkan
            arrow.OnArrowResolved -= OnArrowFinished;
            Destroy(arrow.gameObject);
        }
        currentActiveArrow = null; // Reset
    }

    /// <summary>
    /// Coroutine yang terus menerus memunculkan panah
    /// </summary>
    private IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(0.3f); // Jeda awal

        while (isSpawning && currentTargetHealth != null && currentTargetHealth.CurrentHP > 0)
        {
            // 1. Pilih arah panah secara acak
            ArrowDirection direction = (ArrowDirection) UnityEngine.Random.Range(0, 4);

            RectTransform startPoint = null;
            RectTransform targetPoint = null;
            RectTransform exitPoint = null;

            // 2. Tentukan spawn, target, dan exit point berdasarkan arah
            switch (direction)
            {
                case ArrowDirection.Up:
                    startPoint = upSpawnPoint;
                    targetPoint = upTargetPoint;
                    exitPoint = upExitPoint;
                    break;
                case ArrowDirection.Down:
                    startPoint = downSpawnPoint;
                    targetPoint = downTargetPoint;
                    exitPoint = downExitPoint;
                    break;
                case ArrowDirection.Left:
                    startPoint = leftSpawnPoint;
                    targetPoint = leftTargetPoint;
                    exitPoint = leftExitPoint;
                    break;
                case ArrowDirection.Right:
                    startPoint = rightSpawnPoint;
                    targetPoint = rightTargetPoint;
                    exitPoint = rightExitPoint;
                    break;
            }

            // Pastikan semua poin telah di-set di Inspector
            if (startPoint == null || targetPoint == null || exitPoint == null)
            {
                Debug.LogError($"Missing spawn/target/exit point for {direction} direction in ArrowSpawner!");
                StopSpawning();
                yield break;
            }

            // 3. Buat instance prefab panah
            GameObject arrowInstance = Instantiate(arrowPrefab, canvasParentPanel); // Instantiate langsung di parent
            arrowInstance.transform.rotation = GetRotationForDirection(direction); // Set rotasi

            // 4. Dapatkan skrip Arrow dan inisialisasi
            currentActiveArrow = arrowInstance.GetComponent<Arrow>();
            if (currentActiveArrow == null)
            {
                Debug.LogError("Arrow Prefab tidak memiliki komponen 'Arrow.cs'!");
                StopSpawning();
                yield break;
            }

            // Berlangganan event agar tahu kapan panah ini selesai
            currentActiveArrow.OnArrowResolved += OnArrowFinished;
            currentActiveArrow.Initialize(direction, conductorAttackScript, startPoint.anchoredPosition, targetPoint.anchoredPosition, exitPoint.anchoredPosition);

            conductorAttackScript.SetActiveArrow(currentActiveArrow);

            // 5. Tunggu sampai panah ini "selesai" (dihit atau mis) dan sudah dihancurkan
            yield return new WaitUntil(() => currentActiveArrow == null);

            // 6. Beri jeda antar panah
            yield return new WaitForSeconds(0.2f);
        }

        // Loop berakhir (mungkin karena musuh mati atau player membatalkan lock)
        StopSpawning(); // Panggil StopSpawning untuk cleanup
    }

    /// <summary>
    /// Dipanggil oleh event OnArrowResolved dari objek Arrow
    /// </summary>
    private void OnArrowFinished()
    {
        if (currentActiveArrow != null)
        {
            currentActiveArrow.OnArrowResolved -= OnArrowFinished; // Berhenti berlangganan
            currentActiveArrow = null; // Reset referensi
        }
    }

    /// <summary>
    /// Mendapatkan Quaternion rotasi untuk arah panah
    /// (Asumsi prefab 'Up Arrow' memiliki rotasi 0)
    /// </summary>
    private Quaternion GetRotationForDirection(ArrowDirection direction)
    {
        switch (direction)
        {
            case ArrowDirection.Up: return Quaternion.Euler(0, 0, 0);
            case ArrowDirection.Down: return Quaternion.Euler(0, 0, 180);
            case ArrowDirection.Left: return Quaternion.Euler(0, 0, 90);
            case ArrowDirection.Right: return Quaternion.Euler(0, 0, -90);
            default: return Quaternion.identity;
        }
    }
}