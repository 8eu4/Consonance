using UnityEngine;

// UNTUK SETIAP LEVEL/SCENE, PASTIKAN ADA SCRIPT INI DI SCENE

public class LevelInitializer : MonoBehaviour
{
    [Tooltip("Drag objek posisi awal (Start Point) level ini di sini")]
    public Transform defaultStartPoint;

    void Start()
    {
        // Lapor ke Manager: "Hei Manager, ini titik start level ini ya!"
        if (RespawnManager.Instance != null && defaultStartPoint != null)
        {
            RespawnManager.Instance.InitLevel(defaultStartPoint.position, defaultStartPoint.rotation);
        }
    }
}