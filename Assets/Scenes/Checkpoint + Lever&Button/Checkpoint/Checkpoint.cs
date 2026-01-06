using UnityEngine;
using System.Text.RegularExpressions;

public class Checkpoint : MonoBehaviour
{
    [Header("Settings")]
    public int index;
    public Renderer meshRenderer;

    // KITA HAPUS PropertyBlock. Kita pakai cara langsung (Direct Access).

    void OnValidate()
    {
        // Auto-detect renderer di Editor
        if (meshRenderer == null) meshRenderer = GetComponentInChildren<Renderer>();

        // Auto-detect index dari nama (Checkpoint (1) -> 1)
        var match = Regex.Match(gameObject.name, @"\((\d+)\)$");
        if (match.Success) index = int.Parse(match.Groups[1].Value);
        else index = 0;
    }

    void Start()
    {
        // Pastikan Renderer ketemu
        if (meshRenderer == null) meshRenderer = GetComponentInChildren<Renderer>();

        if (RespawnManager.Instance != null)
            RespawnManager.Instance.RegisterCheckpoint(this);
    }

    void OnDestroy()
    {
        if (RespawnManager.Instance != null)
            RespawnManager.Instance.UnregisterCheckpoint(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Logic Trigger tetap sama
        if (other.TryGetComponent<PlayerIdentity>(out var identity))
        {
            if (identity.type == CharacterType.Conductor)
            {
                Debug.Log($"[CHECKPOINT {index}] Disentuh Player!");
                RespawnManager.Instance.SetCheckpoint(index, transform.position, transform.rotation);
            }
        }
    }

    // --- LOGIKA GANTI WARNA (VERSI PAKSA) ---
    public void UpdateState(int savedIndex)
    {
        if (meshRenderer == null) return;

        // Kita buat Instance Material baru (Clone) biar warnanya independen
        // Ini cara paling kasar tapi paling pasti jalan.

        if (index == savedIndex)
        {
            // HIJAU = AKTIF SEKARANG
            meshRenderer.material.color = Color.green;
        }
        else if (index < savedIndex)
        {
            // MERAH = SUDAH LEWAT
            meshRenderer.material.color = Color.red;
        }
        else
        {
            // ABU/PUTIH = BELUM DISENTUH
            meshRenderer.material.color = Color.white;
        }
    }

    // --- DEBUG MANUAL (Klik Kanan Script) ---
    [ContextMenu("TEST: Jadi HIJAU (Aktif)")]
    public void TestGreen()
    {
        if (meshRenderer) meshRenderer.material.color = Color.green;
    }

    [ContextMenu("TEST: Jadi MERAH (Lewat)")]
    public void TestRed()
    {
        if (meshRenderer) meshRenderer.material.color = Color.red;
    }
}