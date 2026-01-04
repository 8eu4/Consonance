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

    [Space]
    [Header("Shake Settings")]
    [SerializeField] private float shakeDuration = 0.5f; // Lama getaran
    [SerializeField] private float shakeMagnitude = 20f; // Kekuatan getaran

    // Referensi ini di-set oleh LockToAttack
    private ConductorAttack conductorAttackScript;
    private Health currentTargetHealth;

    private bool isSpawning = false;
    private Coroutine spawnLoopCoroutine;
    private Arrow currentActiveArrow; // Menyimpan referensi ke panah yang sedang aktif

    public void StartSpawning(Health targetHealth, ConductorAttack attackScript)
    {
        if (isSpawning) return;

        isSpawning = true;
        currentTargetHealth = targetHealth;
        conductorAttackScript = attackScript;

        spawnLoopCoroutine = StartCoroutine(SpawnLoop());
    }

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
            arrow.OnArrowResolved -= OnArrowFinished;
            Destroy(arrow.gameObject);
        }
        currentActiveArrow = null;
    }

    private IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(0.3f); // Jeda awal

        while (isSpawning && currentTargetHealth != null && currentTargetHealth.CurrentHP > 0)
        {
            // 1. Pilih arah panah secara acak
            ArrowDirection direction = (ArrowDirection)UnityEngine.Random.Range(0, 4);

            RectTransform startPoint = null;
            RectTransform targetPoint = null;
            RectTransform exitPoint = null;

            // 2. Tentukan spawn, target, dan exit point
            switch (direction)
            {
                case ArrowDirection.Up:
                    startPoint = upSpawnPoint; targetPoint = upTargetPoint; exitPoint = upExitPoint; break;
                case ArrowDirection.Down:
                    startPoint = downSpawnPoint; targetPoint = downTargetPoint; exitPoint = downExitPoint; break;
                case ArrowDirection.Left:
                    startPoint = leftSpawnPoint; targetPoint = leftTargetPoint; exitPoint = leftExitPoint; break;
                case ArrowDirection.Right:
                    startPoint = rightSpawnPoint; targetPoint = rightTargetPoint; exitPoint = rightExitPoint; break;
            }

            if (startPoint == null || targetPoint == null || exitPoint == null)
            {
                Debug.LogError($"Missing points for {direction} direction in ArrowSpawner!");
                StopSpawning();
                yield break;
            }

            // 3. Buat instance prefab panah
            GameObject arrowInstance = Instantiate(arrowPrefab, canvasParentPanel);
            arrowInstance.transform.rotation = GetRotationForDirection(direction);

            // 4. Inisialisasi Arrow
            currentActiveArrow = arrowInstance.GetComponent<Arrow>();
            if (currentActiveArrow == null)
            {
                Debug.LogError("Arrow Prefab tidak memiliki komponen 'Arrow.cs'!");
                StopSpawning();
                yield break;
            }

            currentActiveArrow.OnArrowResolved += OnArrowFinished;

            // --- UPDATE: PASSING SHAKE SETTINGS ---
            currentActiveArrow.Initialize(
                direction,
                conductorAttackScript,
                startPoint.anchoredPosition,
                targetPoint.anchoredPosition,
                exitPoint.anchoredPosition,
                shakeDuration,  // Kirim setting durasi
                shakeMagnitude  // Kirim setting kekuatan
            );

            conductorAttackScript.SetActiveArrow(currentActiveArrow);

            // 5. Tunggu sampai panah selesai
            yield return new WaitUntil(() => currentActiveArrow == null);

            // 6. Jeda antar panah
            yield return new WaitForSeconds(0.2f);
        }

        StopSpawning();
    }

    private void OnArrowFinished()
    {
        if (currentActiveArrow != null)
        {
            currentActiveArrow.OnArrowResolved -= OnArrowFinished;
            currentActiveArrow = null;
        }
    }

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