using UnityEngine;
using UnityEngine.UI;

public enum CheckpointState
{
    Fresh,   // putih
    Used,    // merah
    Current  // hijau
}

[RequireComponent(typeof(Transform))]
public class Checkpoint : MonoBehaviour
{
    [Header("Activation")]
    public float activationRadius = 2.0f;
    public bool autoActivate = true; // jika true: cukup masuk radius -> auto aktif
    public KeyCode interactKey = KeyCode.E; // jika autoActivate == false, tekan E untuk activate

    [Header("Checkpoint ordering (important for 1-way behaviour)")]
    [Tooltip("Urutan checkpoint dalam alur cerita. Checkpoint dengan index lebih kecil dianggap 'sebelumnya' dan akan menjadi Used setelah melewati checkpoint berindex lebih tinggi.")]
    public int index = 0;

    [Header("Optional")]
    public string checkpointName = "";

    [Header("Story Quest")]
    [TextArea(2, 4)]
    public string mainObjective;


    // Visuals: pilih salah satu atau kedua-duanya
    [Header("Visuals (optional)")]
    public Renderer indicatorRenderer;       // world renderer (misalnya mesh)
    public Image uiIndicatorImage;           // UI image jika Anda punya UI untuk checkpoint

    // internal state
    Transform playerT;
    CheckpointState state = CheckpointState.Fresh;

    void Start()
    {
        playerT = GameObject.FindGameObjectWithTag("Player")?.transform;
        UpdateVisuals();
    }

    void Update()
    {
        if (playerT == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerT = p.transform;
            else return;
        }

        float dist = Vector3.Distance(playerT.position, transform.position);

        // hanya bisa diaktifkan jika masih Fresh (belum Used / Current)
        if (state == CheckpointState.Fresh && dist <= activationRadius)
        {
            if (autoActivate)
            {
                ActivateCheckpoint();
            }
            else
            {
                if (Input.GetKeyDown(interactKey))
                    ActivateCheckpoint();
            }
        }
    }

    public void ActivateCheckpoint()
    {
        if (state != CheckpointState.Fresh) return;

        if (RespawnManager.Instance != null)
        {
            RespawnManager.Instance.RegisterCheckpoint(this);
        }

        // UPDATE QUEST UI
        if (QuestUIController.Instance != null && !string.IsNullOrEmpty(mainObjective))
        {
            QuestUIController.Instance.SetQuest(mainObjective);
        }

        Debug.Log($"[Checkpoint] Activated: {(string.IsNullOrEmpty(checkpointName) ? name : checkpointName)} (index {index})");
    }


    // dipanggil oleh RespawnManager untuk menandai state
    public void MarkAsUsed()
    {
        state = CheckpointState.Used;
        UpdateVisuals();
    }

    public void MarkAsCurrent()
    {
        state = CheckpointState.Current;
        UpdateVisuals();
    }

    public void ResetCheckpoint()
    {
        state = CheckpointState.Fresh;
        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        // warna untuk world renderer
        if (indicatorRenderer != null)
        {
            // pastikan material instance agar tidak mengubah material shared
            if (Application.isPlaying)
            {
                if (indicatorRenderer.material == null) return;
            }

            Color col = Color.white;
            switch (state)
            {
                case CheckpointState.Fresh: col = Color.white; break;
                case CheckpointState.Used: col = Color.red; break;
                case CheckpointState.Current: col = Color.green; break;
            }

            // jika material array, ubah color pada material utama
            if (indicatorRenderer.material != null)
                indicatorRenderer.material.color = col;
        }

        // warna untuk UI image
        if (uiIndicatorImage != null)
        {
            Color col = Color.white;
            switch (state)
            {
                case CheckpointState.Fresh: col = Color.white; break;
                case CheckpointState.Used: col = Color.red; break;
                case CheckpointState.Current: col = Color.green; break;
            }
            uiIndicatorImage.color = col;
        }
    }

    // debug sphere
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationRadius);
    }
}
