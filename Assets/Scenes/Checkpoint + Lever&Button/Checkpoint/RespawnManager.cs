using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }

    private const string KEY_SCENE = "SaveScene";
    private const string KEY_POS_X = "CP_X";
    private const string KEY_POS_Y = "CP_Y";
    private const string KEY_POS_Z = "CP_Z";
    private const string KEY_ROT_Y = "CP_ROTY";
    private const string KEY_IDX = "CP_IDX";

    private List<PlayerIdentity> activePlayers = new List<PlayerIdentity>();
    private List<Checkpoint> sceneCheckpoints = new List<Checkpoint>();

    private Transform activeConductor;
    private Vector3 levelStartPos;
    private Quaternion levelStartRot;

    private Vector3 lastCheckpointPos;
    private Quaternion lastCheckpointRot;
    private bool hasCheckpointData = false;
    private int lastCheckpointIndex = -1;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void InitLevel(Vector3 startPos, Quaternion startRot)
    {
        levelStartPos = startPos;
        levelStartRot = startRot;
        Debug.Log($"[RespawnManager] Level Start Point: {startPos}");
        LoadCheckpointData();
    }

    public void RegisterCheckpoint(Checkpoint cp)
    {
        if (!sceneCheckpoints.Contains(cp))
        {
            sceneCheckpoints.Add(cp);
            cp.UpdateState(lastCheckpointIndex);
        }
    }

    public void UnregisterCheckpoint(Checkpoint cp)
    {
        if (sceneCheckpoints.Contains(cp)) sceneCheckpoints.Remove(cp);
    }

    public void RegisterPlayer(PlayerIdentity player)
    {
        if (!activePlayers.Contains(player))
        {
            activePlayers.Add(player);
            if (player.type == CharacterType.Conductor) activeConductor = player.transform;
        }
    }

    public void UnregisterPlayer(PlayerIdentity player)
    {
        if (activePlayers.Contains(player)) activePlayers.Remove(player);
    }

    public void RespawnTeam()
    {
        Vector3 targetPos = hasCheckpointData ? lastCheckpointPos : levelStartPos;
        Quaternion targetRot = hasCheckpointData ? lastCheckpointRot : levelStartRot;

        Debug.Log($"[RespawnManager] Respawning Team di: {targetPos}");

        foreach (var p in activePlayers)
        {
            Vector3 offset = (p.type != CharacterType.Conductor) ? (UnityEngine.Random.insideUnitSphere * 2.0f) : Vector3.zero;
            offset.y = 0;

            Teleport(p.transform, targetPos + offset, targetRot);
            p.OnRespawn();
        }
    }

    public void RespawnSupport(Transform supportChar)
    {
        if (activeConductor == null) { RespawnTeam(); return; }

        Vector3 forwardPos = activeConductor.position + (activeConductor.forward * 2.5f);
        Vector3 finalPos = activeConductor.position;

        RaycastHit hit;
        Vector3 rayStart = forwardPos + Vector3.up * 2f;

        if (Physics.Raycast(rayStart, Vector3.down, out hit, 10f))
        {
            if (!Physics.CheckSphere(hit.point + Vector3.up * 0.5f, 0.4f))
            {
                finalPos = hit.point;
            }
        }

        Teleport(supportChar, finalPos, activeConductor.rotation);
        supportChar.GetComponent<PlayerIdentity>()?.OnRespawn();
    }

    public void SetCheckpoint(int index, Vector3 pos, Quaternion rot)
    {
        // Logika pencegahan save checkpoint mundur
        if (hasCheckpointData && index <= lastCheckpointIndex && SceneManager.GetActiveScene().name == PlayerPrefs.GetString(KEY_SCENE))
            return;

        Debug.Log($"[RespawnManager] Checkpoint {index} Disimpan");
        lastCheckpointIndex = index;
        lastCheckpointPos = pos;
        lastCheckpointRot = rot;
        hasCheckpointData = true;

        SaveToDisk();
        UpdateVisualsInScene();
    }

    public void ClearSaveData()
    {
        PlayerPrefs.DeleteKey(KEY_SCENE);
        PlayerPrefs.DeleteKey(KEY_POS_X);
        PlayerPrefs.DeleteKey(KEY_POS_Y);
        PlayerPrefs.DeleteKey(KEY_POS_Z);
        PlayerPrefs.DeleteKey(KEY_ROT_Y);
        PlayerPrefs.DeleteKey(KEY_IDX);
        PlayerPrefs.Save();

        hasCheckpointData = false;
        lastCheckpointIndex = -1;
        UpdateVisualsInScene();
    }

    public string GetSavedSceneName() => PlayerPrefs.GetString(KEY_SCENE, "");

    private void SaveToDisk()
    {
        PlayerPrefs.SetString(KEY_SCENE, SceneManager.GetActiveScene().name);
        PlayerPrefs.SetFloat(KEY_POS_X, lastCheckpointPos.x);
        PlayerPrefs.SetFloat(KEY_POS_Y, lastCheckpointPos.y);
        PlayerPrefs.SetFloat(KEY_POS_Z, lastCheckpointPos.z);
        PlayerPrefs.SetFloat(KEY_ROT_Y, lastCheckpointRot.eulerAngles.y);
        PlayerPrefs.SetInt(KEY_IDX, lastCheckpointIndex);
        PlayerPrefs.Save();
    }

    private void LoadCheckpointData()
    {
        if (SceneManager.GetActiveScene().name == GetSavedSceneName())
        {
            hasCheckpointData = true;
            lastCheckpointIndex = PlayerPrefs.GetInt(KEY_IDX);
            lastCheckpointPos = new Vector3(PlayerPrefs.GetFloat(KEY_POS_X), PlayerPrefs.GetFloat(KEY_POS_Y), PlayerPrefs.GetFloat(KEY_POS_Z));
            lastCheckpointRot = Quaternion.Euler(0, PlayerPrefs.GetFloat(KEY_ROT_Y), 0);

            Debug.Log($"[RespawnManager] Data Loaded. CP Index: {lastCheckpointIndex}");

            RespawnTeam();
            UpdateVisualsInScene();
        }
        else
        {
            hasCheckpointData = false;
            lastCheckpointIndex = -1;
        }
    }

    private void UpdateVisualsInScene()
    {
        foreach (var cp in sceneCheckpoints)
            if (cp != null) cp.UpdateState(lastCheckpointIndex);
    }

    private void Teleport(Transform t, Vector3 pos, Quaternion rot)
    {
        var cc = t.GetComponent<CharacterController>();
        var rb = t.GetComponent<Rigidbody>();

        // Matikan komponen penggerak
        if (cc) cc.enabled = false;
        if (rb) rb.isKinematic = true;

        // Pindahkan posisi
        t.position = pos;
        t.rotation = rot;

        // Paksa sinkronisasi physics engine
        Physics.SyncTransforms();

        // Nyalakan kembali
        if (cc) cc.enabled = true;
        if (rb) rb.isKinematic = false;
    }
}