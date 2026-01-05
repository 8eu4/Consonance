using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EnemyHealthTracker : MonoBehaviour
{
    [Header("Targeting")]
    public string targetTag = "Enemy";

    [Header("Settings")]
    public float heightOffset = 0.5f; // Jarak dari titik tertinggi Mesh (Visual)
    public Vector2 heartIconSize = new Vector2(0.5f, 0.5f);

    [Header("References")]
    public GameObject gridHealthPopUpPrefab;
    public GameObject heartIconPrefab;

    private Camera mainCam;
    private Dictionary<int, EnemyUIData> trackedEnemies = new Dictionary<int, EnemyUIData>();

    class EnemyUIData
    {
        public Transform enemyTransform;
        public GameObject uiInstance;
        public List<GameObject> hearts = new List<GameObject>();
        // Kita simpan list renderer biar tidak GetComponent ulang terus menerus (berat)
        public Renderer[] cachedRenderers;
    }

    void Start()
    {
        mainCam = Camera.main;
        InvokeRepeating(nameof(ScanForEnemies), 0f, 0.5f);
    }

    void ScanForEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(targetTag);

        foreach (GameObject enemy in enemies)
        {
            int id = enemy.GetInstanceID();

            if (trackedEnemies.ContainsKey(id)) continue;

            EnemyHealth healthScript = enemy.GetComponent<EnemyHealth>();
            if (healthScript != null && healthScript.CurrentHP > 0)
            {
                CreateUI(enemy, healthScript, id);
            }
        }
    }

    void CreateUI(GameObject enemy, EnemyHealth healthScript, int id)
    {
        EnemyUIData data = new EnemyUIData();
        data.enemyTransform = enemy.transform;

        // --- CARI SEMUA MESH (Visual) ---
        // Kita ambil semua renderer di anak-anak object juga (misal: pedang, kepala, armor)
        List<Renderer> validRenderers = new List<Renderer>();
        Renderer[] allRenderers = enemy.GetComponentsInChildren<Renderer>();

        foreach (var r in allRenderers)
        {
            // PENTING: Jangan hitung Particle System (efek asap/api) atau Trail
            // Kita cuma mau MeshRenderer (bangunan/prop) atau SkinnedMeshRenderer (karakter animasi)
            if (r is ParticleSystemRenderer || r is TrailRenderer || r is LineRenderer)
                continue;

            validRenderers.Add(r);
        }

        data.cachedRenderers = validRenderers.ToArray();
        // --------------------------------

        GameObject ui = Instantiate(gridHealthPopUpPrefab);
        data.uiInstance = ui;

        GridLayoutGroup grid = ui.GetComponent<GridLayoutGroup>();
        if (grid != null) grid.cellSize = heartIconSize;

        for (int i = 0; i < healthScript.MaxHP; i++)
        {
            GameObject heart = Instantiate(heartIconPrefab, ui.transform);
            data.hearts.Add(heart);
        }

        UpdateHeartVisuals(data, healthScript.CurrentHP);

        healthScript.OnHealthChanged += (currentHP, maxHP) =>
        {
            if (trackedEnemies.ContainsKey(id))
                UpdateHeartVisuals(data, currentHP);
        };

        trackedEnemies.Add(id, data);
    }

    void UpdateHeartVisuals(EnemyUIData data, int currentHP)
    {
        for (int i = 0; i < data.hearts.Count; i++)
        {
            data.hearts[i].SetActive(i < currentHP);
        }
    }

    void LateUpdate()
    {
        List<int> toRemove = new List<int>();

        foreach (var kvp in trackedEnemies)
        {
            EnemyUIData data = kvp.Value;

            if (data.enemyTransform == null)
            {
                if (data.uiInstance != null) Destroy(data.uiInstance);
                toRemove.Add(kvp.Key);
                continue;
            }

            // --- HITUNG POSISI BERDASARKAN VISUAL MESH ---

            Vector3 topPosition = data.enemyTransform.position;
            Vector3 centerPosition = data.enemyTransform.position;

            if (data.cachedRenderers != null && data.cachedRenderers.Length > 0)
            {
                // Inisialisasi bounds dengan renderer pertama
                Bounds combinedBounds = data.cachedRenderers[0].bounds;

                // Gabungkan sisa renderer lain (misal: gabungkan bounds badan + bounds kepala)
                for (int i = 1; i < data.cachedRenderers.Length; i++)
                {
                    combinedBounds.Encapsulate(data.cachedRenderers[i].bounds);
                }

                // Ambil titik teratas visual & titik tengah visual
                topPosition = new Vector3(combinedBounds.center.x, combinedBounds.max.y, combinedBounds.center.z);
            }
            else
            {
                // Fallback kalau musuh invisible (tak punya mesh)
                topPosition += Vector3.up * 2.0f;
            }

            // Posisi UI = Di atas titik tertinggi visual + Offset
            data.uiInstance.transform.position = new Vector3(topPosition.x, topPosition.y + heightOffset, topPosition.z);

            // Rotasi Billboard
            data.uiInstance.transform.rotation = mainCam.transform.rotation;
        }

        foreach (int id in toRemove)
        {
            trackedEnemies.Remove(id);
        }
    }
}