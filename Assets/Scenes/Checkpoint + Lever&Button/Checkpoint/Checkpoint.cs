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
    public bool autoActivate = true;
    public KeyCode interactKey = KeyCode.E;

    [Header("Checkpoint ordering (important for 1-way behaviour)")]
    public int index = 0;

    [Header("Optional")]
    public string checkpointName = "";

    [Header("Story Quest")]
    [TextArea(2, 4)]
    public string mainObjective;

    [Header("Visuals (optional)")]
    public Renderer indicatorRenderer;
    public Image uiIndicatorImage;

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

        if (state == CheckpointState.Fresh && dist <= activationRadius)
        {
            if (autoActivate) ActivateCheckpoint();
            else if (Input.GetKeyDown(interactKey)) ActivateCheckpoint();
        }
    }

    public void ActivateCheckpoint()
    {
        if (state != CheckpointState.Fresh) return;

        if (RespawnManager.Instance != null)
        {
            RespawnManager.Instance.RegisterCheckpoint(this);
        }

        // Update Quest UI
        if (QuestUIController.Instance != null && !string.IsNullOrEmpty(mainObjective))
        {
            QuestUIController.Instance.SetQuest(mainObjective);
        }

        Debug.Log($"[Checkpoint] Activated: {(string.IsNullOrEmpty(checkpointName) ? name : checkpointName)} (index {index})");
    }

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
        if (indicatorRenderer != null)
        {
            if (Application.isPlaying && indicatorRenderer.material == null) return;
            Color col = Color.white;
            switch (state)
            {
                case CheckpointState.Fresh: col = Color.white; break;
                case CheckpointState.Used: col = Color.red; break;
                case CheckpointState.Current: col = Color.green; break;
            }

            if (indicatorRenderer.material != null)
                indicatorRenderer.material.color = col;
        }

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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationRadius);
    }
}
