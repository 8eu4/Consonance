using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }

    [Header("Default (NewGame) spawn)")]
    public Transform defaultSpawn; // assign in inspector

    // In-memory current checkpoint (if any). If null => use defaultSpawn
    private Vector3 currentPos;
    private Quaternion currentRot;
    private bool hasCheckpoint = false;

    // PlayerPrefs keys
    const string PREF_SCENE = "RESPAWN_SCENE";
    const string PREF_POS_X = "RESPAWN_POS_X";
    const string PREF_POS_Y = "RESPAWN_POS_Y";
    const string PREF_POS_Z = "RESPAWN_POS_Z";
    const string PREF_ROT_X = "RESPAWN_ROT_X";
    const string PREF_ROT_Y = "RESPAWN_ROT_Y";
    const string PREF_ROT_Z = "RESPAWN_ROT_Z";
    const string PREF_ROT_W = "RESPAWN_ROT_W";

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

    void Start()
    {
        // Optionally, load checkpoint at start (but we prefer explicit ContinueGame call)
        // LoadSavedCheckpoint();
    }

    #region API for checkpoints
    // Called by PlayerRespawner on Start to register the default spawn automatically (optional)
    public void RegisterDefaultSpawn(Transform t)
    {
        if (defaultSpawn == null)
            defaultSpawn = t;
    }

    // Register a checkpoint (in-memory and save to PlayerPrefs)
    public void RegisterCheckpoint(Transform checkpointTransform)
    {
        if (checkpointTransform == null) return;

        currentPos = checkpointTransform.position;
        currentRot = checkpointTransform.rotation;
        hasCheckpoint = true;
        SaveCheckpointToPrefs(checkpointTransform);
        Debug.Log("[RespawnManager] Checkpoint registered at " + currentPos);
    }

    // Force-register by position/rotation (no transform)
    public void RegisterCheckpoint(Vector3 pos, Quaternion rot)
    {
        currentPos = pos;
        currentRot = rot;
        hasCheckpoint = true;
        SaveCheckpointToPrefs(pos, rot);
        Debug.Log("[RespawnManager] Checkpoint registered (manual) at " + pos);
    }
    #endregion

    #region Save / Load
    private void SaveCheckpointToPrefs(Transform t)
    {
        SaveCheckpointToPrefs(t.position, t.rotation);
    }

    private void SaveCheckpointToPrefs(Vector3 pos, Quaternion rot)
    {
        string scene = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString(PREF_SCENE, scene);
        PlayerPrefs.SetFloat(PREF_POS_X, pos.x);
        PlayerPrefs.SetFloat(PREF_POS_Y, pos.y);
        PlayerPrefs.SetFloat(PREF_POS_Z, pos.z);
        PlayerPrefs.SetFloat(PREF_ROT_X, rot.x);
        PlayerPrefs.SetFloat(PREF_ROT_Y, rot.y);
        PlayerPrefs.SetFloat(PREF_ROT_Z, rot.z);
        PlayerPrefs.SetFloat(PREF_ROT_W, rot.w);
        PlayerPrefs.Save();
    }

    // Returns true if a valid saved checkpoint exists for this active scene
    public bool LoadSavedCheckpointForCurrentScene(out Vector3 pos, out Quaternion rot)
    {
        pos = Vector3.zero;
        rot = Quaternion.identity;

        if (!PlayerPrefs.HasKey(PREF_SCENE)) return false;

        string savedScene = PlayerPrefs.GetString(PREF_SCENE);
        if (savedScene != SceneManager.GetActiveScene().name) return false;

        pos.x = PlayerPrefs.GetFloat(PREF_POS_X);
        pos.y = PlayerPrefs.GetFloat(PREF_POS_Y);
        pos.z = PlayerPrefs.GetFloat(PREF_POS_Z);
        rot.x = PlayerPrefs.GetFloat(PREF_ROT_X);
        rot.y = PlayerPrefs.GetFloat(PREF_ROT_Y);
        rot.z = PlayerPrefs.GetFloat(PREF_ROT_Z);
        rot.w = PlayerPrefs.GetFloat(PREF_ROT_W);

        // update in-memory
        currentPos = pos;
        currentRot = rot;
        hasCheckpoint = true;
        return true;
    }

    // Remove saved checkpoint entirely (for NewGame)
    public void ClearSavedCheckpoint()
    {
        PlayerPrefs.DeleteKey(PREF_SCENE);
        PlayerPrefs.DeleteKey(PREF_POS_X);
        PlayerPrefs.DeleteKey(PREF_POS_Y);
        PlayerPrefs.DeleteKey(PREF_POS_Z);
        PlayerPrefs.DeleteKey(PREF_ROT_X);
        PlayerPrefs.DeleteKey(PREF_ROT_Y);
        PlayerPrefs.DeleteKey(PREF_ROT_Z);
        PlayerPrefs.DeleteKey(PREF_ROT_W);
        PlayerPrefs.Save();

        hasCheckpoint = false;
        Debug.Log("[RespawnManager] Saved checkpoint cleared.");
    }
    #endregion

    #region Respawn actions
    // Respawn a player GameObject (moves transform and resets velocities if rigidbody/charactercontroller)
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

        MoveTransformSafely(playerT, targetPos, targetRot);
        // If player has health/respawn state, you can call its reset here
        var playerReset = player.GetComponent<IRespawnReset>();
        if (playerReset != null) playerReset.OnRespawn();

        Debug.Log("[RespawnManager] Player respawned to " + targetPos);
    }

    // Same but returns the chosen pos/rot for caller
    public (Vector3 pos, Quaternion rot) GetActiveSpawn()
    {
        if (hasCheckpoint) return (currentPos, currentRot);
        if (defaultSpawn != null) return (defaultSpawn.position, defaultSpawn.rotation);
        return (Vector3.zero, Quaternion.identity);
    }

    // Attempt to load saved checkpoint and respawn player (for ContinueGame)
    // returns true if used saved checkpoint
    public bool ContinueGameRespawn(GameObject player)
    {
        Vector3 pos; Quaternion rot;
        bool ok = LoadSavedCheckpointForCurrentScene(out pos, out rot);
        if (!ok)
        {
            // fallback to default
            if (defaultSpawn != null)
            {
                RegisterCheckpoint(defaultSpawn); // sets in-memory as well (but not saved)
            }
        }

        RespawnPlayer(player);
        return ok;
    }
    #endregion

    #region Helpers
    private void MoveTransformSafely(Transform t, Vector3 pos, Quaternion rot)
    {
        // If player has CharacterController, disable it while moving to avoid collisions issues
        var cc = t.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            t.SetPositionAndRotation(pos, rot);
            cc.enabled = true;
            // reset velocity if character has controller-derived movement - user handles if needed
            return;
        }

        // RigidBody kinematic vs non-kinematic
        var rb = t.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // If non-kinematic, reset velocity and move
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = pos;
            rb.rotation = rot;
            return;
        }

        // default
        t.SetPositionAndRotation(pos, rot);
    }
    #endregion
}

// Optional interface for player components that need to reset on respawn
public interface IRespawnReset
{
    void OnRespawn();
}
