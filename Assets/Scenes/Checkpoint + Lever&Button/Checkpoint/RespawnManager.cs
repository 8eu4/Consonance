using UnityEngine;
using UnityEngine.SceneManagement;
using System;

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

    // PlayerPrefs keys
    const string PREF_SCENE = "RESPAWN_SCENE";
    const string PREF_POS_X = "RESPAWN_POS_X";
    const string PREF_POS_Y = "RESPAWN_POS_Y";
    const string PREF_POS_Z = "RESPAWN_POS_Z";
    const string PREF_ROT_X = "RESPAWN_ROT_X";
    const string PREF_ROT_Y = "RESPAWN_ROT_Y";
    const string PREF_ROT_Z = "RESPAWN_ROT_Z";
    const string PREF_ROT_W = "RESPAWN_ROT_W";
    const string PREF_CHECKPOINT_INDEX = "RESPAWN_CHECKPOINT_INDEX";

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

    // jika Anda ingin mendaftarkan default spawn lewat PlayerRespawner.Start()
    public void RegisterDefaultSpawn(Transform t)
    {
        if (defaultSpawn == null)
            defaultSpawn = t;
    }

    // Register dari objek Checkpoint (lebih baik daripada hanya Transform)
    public void RegisterCheckpoint(Checkpoint checkpoint)
    {
        if (checkpoint == null) return;

        currentPos = checkpoint.transform.position;
        currentRot = checkpoint.transform.rotation;
        hasCheckpoint = true;
        currentCheckpointIndex = checkpoint.index;

        // Tandai checkpoint lain berdasarkan index � implementasi 1-way:
        // - checkpoints dengan index < currentCheckpointIndex -> Used
        // - checkpoint dengan index == currentCheckpointIndex -> Current
        // - checkpoints dengan index > currentCheckpointIndex -> Fresh (reset)
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

        SaveCheckpointToPrefs(currentPos, currentRot, currentCheckpointIndex);
        Debug.Log("[RespawnManager] Checkpoint registered at " + currentPos + " index " + currentCheckpointIndex);
    }

    // Register by position/rotation (manual)
    public void RegisterCheckpoint(Vector3 pos, Quaternion rot, int checkpointIndex = -1)
    {
        currentPos = pos;
        currentRot = rot;
        hasCheckpoint = true;
        currentCheckpointIndex = checkpointIndex;
        SaveCheckpointToPrefs(pos, rot, checkpointIndex);
        Debug.Log("[RespawnManager] Manual Checkpoint registered at " + pos + " index " + checkpointIndex);
    }

    #region Save / Load
    private void SaveCheckpointToPrefs(Vector3 pos, Quaternion rot, int checkpointIndex)
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

        PlayerPrefs.SetInt(PREF_CHECKPOINT_INDEX, checkpointIndex);
        PlayerPrefs.Save();
    }

    // Returns true if a valid saved checkpoint exists for this active scene
    public bool LoadSavedCheckpointForCurrentScene(out Vector3 pos, out Quaternion rot, out int checkpointIndex)
    {
        pos = Vector3.zero;
        rot = Quaternion.identity;
        checkpointIndex = -1;

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

        checkpointIndex = PlayerPrefs.GetInt(PREF_CHECKPOINT_INDEX, -1);

        currentPos = pos;
        currentRot = rot;
        currentCheckpointIndex = checkpointIndex;
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
        PlayerPrefs.DeleteKey(PREF_CHECKPOINT_INDEX);
        PlayerPrefs.Save();

        hasCheckpoint = false;
        currentCheckpointIndex = -1;
        Debug.Log("[RespawnManager] Saved checkpoint cleared.");

        // RESET semua checkpoint di scene ke Fresh (NewGame harus membuat checkpoint "fresh")
        ResetAllCheckpointsToFresh();
    }

    private void ResetAllCheckpointsToFresh()
    {
        var all = FindObjectsOfType<Checkpoint>();
        foreach (var cp in all)
            cp.ResetCheckpoint();

        Debug.Log("[RespawnManager] All checkpoints reset to Fresh.");
    }
    #endregion

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

        MoveTransformSafely(playerT, targetPos, targetRot);

        var playerReset = player.GetComponent<IRespawnReset>();
        if (playerReset != null) playerReset.OnRespawn();

        Debug.Log("[RespawnManager] Player respawned to " + targetPos);
    }

    // ContinueGame: jika saved checkpoint untuk current scene ada, gunakan pos & index tersebut,
    // lalu set visual checkpoint sesuai saved checkpoint index.
    public bool ContinueGameRespawn(GameObject player)
    {
        Vector3 pos; Quaternion rot; int savedIndex;
        bool ok = LoadSavedCheckpointForCurrentScene(out pos, out rot, out savedIndex);
        if (!ok)
        {
            // tidak ada saved checkpoint untuk scene ini => fallback ke default spawn (tidak menandai checkpoint apa pun)
            hasCheckpoint = false;
            currentCheckpointIndex = -1;
            RespawnPlayer(player);
            return false;
        }

        // Jika savedIndex valid, update visual states pada semua checkpoint
        if (savedIndex >= 0)
        {
            var all = FindObjectsOfType<Checkpoint>();
            foreach (var cp in all)
            {
                if (cp.index < savedIndex) cp.MarkAsUsed();
                else if (cp.index == savedIndex) cp.MarkAsCurrent();
                else cp.ResetCheckpoint();
            }
        }

        RespawnPlayer(player);
        return true;
    }
    #endregion

    #region Helpers
    private void MoveTransformSafely(Transform t, Vector3 pos, Quaternion rot)
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
    #endregion
}

// Optional interface for player components that need to reset on respawn
public interface IRespawnReset
{
    void OnRespawn();
}
