using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EnemyHealthTracker : MonoBehaviour
{
    [Header("Targeting")]
    public string targetTag = "Enemy";

    [Header("Settings")]
    public float heightOffset = 0.5f;
    public int fallbackHealth = 5; // Dipakai kalau MaxHP di musuh 0/Error

    [Header("Dynamic Scale")]
    public float baseScale = 0.01f;
    public float minScale = 0.005f;
    public float maxScale = 0.03f;
    public float cullDistance = 50f;

    [Header("References")]
    public GameObject gridHealthPopUp;
    public GameObject heartPopUpIconPrefab;

    private Camera mainCam;

    // List musuh yang sedang kita tampilkan UI-nya
    private List<EnemyUIData> trackedEnemies = new List<EnemyUIData>();

    // Daftar ID musuh biar gak double create UI
    private HashSet<int> registeredEnemyIDs = new HashSet<int>();

    class EnemyUIData
    {
        public int instanceID; // ID unik game object
        public Transform target;
        public Collider targetCollider;
        public GameObject uiInstance;
        public RectTransform uiRect;
        public List<GameObject> heartIcons = new List<GameObject>();
    }

    void Start()
    {
        mainCam = Camera.main;

        // PENTING: Jalanin scan setiap 0.5 detik. 
        // Jadi kalau ada musuh baru di-spawn (Wave), UI otomatis muncul.
        InvokeRepeating(nameof(ScanForEnemies), 0f, 0.5f);
    }

    void ScanForEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(targetTag);

        foreach (GameObject enemy in enemies)
        {
            int id = enemy.GetInstanceID();

            // 1. Cek apakah musuh ini sudah punya UI? Kalau sudah, skip.
            if (registeredEnemyIDs.Contains(id)) continue;

            // 2. Cek apakah dia punya script EnemyHealth?
            EnemyHealth healthScript = enemy.GetComponent<EnemyHealth>();

            if (healthScript == null)
            {
                // Debugging biar kamu tau object mana yang error
                // Debug.LogWarning($"Object '{enemy.name}' punya tag Enemy tapi TIDAK ADA script EnemyHealth!");
                continue;
            }

            // --- BIKIN UI BARU ---
            RegisterNewEnemy(enemy, healthScript, id);
        }
    }

    void RegisterNewEnemy(GameObject enemy, EnemyHealth healthScript, int id)
    {
        GameObject ui = Instantiate(gridHealthPopUp);
        EnemyUIData data = new EnemyUIData();

        data.instanceID = id;
        data.target = enemy.transform;
        Collider col = enemy.GetComponent<Collider>();
        if (col == null) col = enemy.GetComponentInChildren<Collider>();
        data.targetCollider = col;
        data.uiInstance = ui;
        data.uiRect = ui.GetComponent<RectTransform>();

        // Cek Max HP
        int maxHP = healthScript.MaxHP;
        if (maxHP <= 0) maxHP = fallbackHealth; // Pake fallback kalau lupa setting

        int currentHP = healthScript.CurrentHP;
        if (currentHP <= 0 && maxHP > 0) currentHP = maxHP;

        // Spawn Hati
        for (int i = 0; i < maxHP; i++)
        {
            GameObject heart = Instantiate(heartPopUpIconPrefab, ui.transform);
            data.heartIcons.Add(heart);
        }

        UpdateHearts(data, currentHP);

        // Subscribe Event
        healthScript.OnHealthChanged += (newVal) => UpdateHearts(data, newVal);

        // Masukkan ke daftar tracking
        trackedEnemies.Add(data);
        registeredEnemyIDs.Add(id);
    }

    void UpdateHearts(EnemyUIData data, int currentHP)
    {
        if (data.uiInstance == null) return;

        for (int i = 0; i < data.heartIcons.Count; i++)
        {
            if (i < currentHP)
                data.heartIcons[i].SetActive(true);
            else
                data.heartIcons[i].SetActive(false);
        }
    }

    void LateUpdate()
    {
        if (mainCam == null) return;

        for (int i = trackedEnemies.Count - 1; i >= 0; i--)
        {
            EnemyUIData data = trackedEnemies[i];

            // Kalau musuh mati/destroy, hapus UI-nya
            if (data.target == null)
            {
                if (data.uiInstance != null) Destroy(data.uiInstance);
                registeredEnemyIDs.Remove(data.instanceID); // Hapus dari daftar ID biar bersih
                trackedEnemies.RemoveAt(i);
                continue;
            }

            Vector3 topPos = data.target.position;
            if (data.targetCollider != null)
                topPos = new Vector3(data.target.position.x, data.targetCollider.bounds.max.y, data.target.position.z);

            Vector3 finalPos = topPos + Vector3.up * heightOffset;
            float dist = Vector3.Distance(finalPos, mainCam.transform.position);

            if (dist > cullDistance)
            {
                if (data.uiInstance.activeSelf) data.uiInstance.SetActive(false);
                continue;
            }

            if (!data.uiInstance.activeSelf) data.uiInstance.SetActive(true);
            data.uiInstance.transform.position = finalPos;
            data.uiInstance.transform.LookAt(data.uiInstance.transform.position + mainCam.transform.rotation * Vector3.forward,
                                             mainCam.transform.rotation * Vector3.up);

            float targetScale = baseScale * (dist * 0.1f);
            targetScale = Mathf.Clamp(targetScale, minScale, maxScale);
            data.uiRect.localScale = new Vector3(targetScale, targetScale, 1f);
        }
    }
}