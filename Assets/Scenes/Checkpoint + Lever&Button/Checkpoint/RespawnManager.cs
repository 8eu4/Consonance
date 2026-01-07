using UnityEngine;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// RespawnManager tetap menangani logic respawn/visual marking checkpoint.
/// Penyimpanan persistent dilakukan oleh SaveManager (JSON).
/// </summary>
public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }

    [Header("Default (NewGame) spawn")]
    public Transform defaultSpawn; // assign in inspector

    // in-memory current checkpoint
    private Vector3 currentPos;
    private Quaternion currentRot;
    private bool hasCheckpoint = false;
    private int currentCheckpointIndex = -1;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RegisterDefaultSpawn(Transform t)
    {
        if (defaultSpawn == null)
            defaultSpawn = t;
    }

    // Register from Checkpoint object
    public void RegisterCheckpoint(Checkpoint checkpoint)
    {
        if (checkpoint == null) return;

        currentPos = checkpoint.transform.position;
        currentRot = checkpoint.transform.rotation;
        hasCheckpoint = true;
        currentCheckpointIndex = checkpoint.index;

        // Update visuals (1-way marking)
        var all = FindObjectsOfType<Checkpoint>();
        foreach (var cp in all)
        {
            if (cp.index < currentCheckpointIndex)
                cp.MarkAsUsed();
            else if (cp.index == currentCheckpointIndex)
                cp.MarkAsCurrent();
            else
                cp.ResetCheckpoint();
        }

        // Persist via SaveManager (scene name + quest message)
        string scene = SceneManager.GetActiveScene().name;
        string quest = checkpoint.mainObjective;
        if (SaveManager.Instance != null)
            SaveManager.Instance.SaveCheckpoint(scene, currentPos, currentRot, currentCheckpointIndex, quest);

        Debug.Log("[RespawnManager] Checkpoint registered at " + currentPos + " index " + currentCheckpointIndex);
    }

    // Manual register
    public void RegisterCheckpoint(Vector3 pos, Quaternion rot, int checkpointIndex = -1, string questMessage = "")
    {
        currentPos = pos;
        currentRot = rot;
        hasCheckpoint = true;
        currentCheckpointIndex = checkpointIndex;

        var all = FindObjectsOfType<Checkpoint>();
        foreach (var cp in all)
        {
            if (cp.index < currentCheckpointIndex) cp.MarkAsUsed();
            else if (cp.index == currentCheckpointIndex) cp.MarkAsCurrent();
            else cp.ResetCheckpoint();
        }

        string scene = SceneManager.GetActiveScene().name;
        if (SaveManager.Instance != null)
            SaveManager.Instance.SaveCheckpoint(scene, pos, rot, checkpointIndex, questMessage);

        Debug.Log("[RespawnManager] Manual Checkpoint registered at " + pos + " index " + checkpointIndex);
    }

    #region Respawn actions

    public void RespawnPlayer(GameObject player)
    {
        if (player == null) return;
        Transform playerT = player.transform;

        Vector3 targetPos;
        Quaternion targetRot;

        if (hasCheckpoint)
        {
            targetPos = currentPos;
            targetRot = currentRot;
        }
        else if (defaultSpawn != null)
        {
            targetPos = defaultSpawn.position;
            targetRot = defaultSpawn.rotation;
        }
        else
        {
            Debug.LogWarning("[RespawnManager] No spawn available. Using current position.");
            return;
        }

        MoveTransformSafelyPublic(playerT, targetPos, targetRot);

        //var playerReset = player.GetComponent<IRespawnReset>();
        //if (playerReset != null) playerReset.OnRespawn();

        Debug.Log("[RespawnManager] Player respawned to " + targetPos);
    }

    public bool ContinueGameRespawn(GameObject player)
    {
        // NOTE: prefer using SaveManager. This method kept for compatibility:
        if (SaveManager.Instance == null)
        {
            RespawnPlayer(player);
            return false;
        }

        // If saved file belongs to a different scene, SaveManager will handle scene load
        // call SaveManager.ContinueAndRespawn coroutine from caller (e.g. MainMenuController)
        RespawnPlayer(player);
        return true;
    }

    #endregion

    #region Helpers (made public for SaveManager usage)

    public void MoveTransformSafelyPublic(Transform t, Vector3 pos, Quaternion rot)
    {
        var cc = t.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            t.SetPositionAndRotation(pos, rot);
            cc.enabled = true;
            return;
        }

        var rb = t.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = pos;
            rb.rotation = rot;
            return;
        }

        t.SetPositionAndRotation(pos, rot);
    }

    /// <summary>
    /// Dipanggil oleh SaveManager.NewGame agar in-memory checkpoint dan visual di scene direset.
    /// </summary>
    public void ClearInMemoryCheckpoint()
    {
        hasCheckpoint = false;
        currentCheckpointIndex = -1;

        var all = FindObjectsOfType<Checkpoint>();
        foreach (var cp in all)
            cp.ResetCheckpoint();

        Debug.Log("[RespawnManager] In-memory checkpoint cleared and visuals reset.");
    }

    #endregion
}
