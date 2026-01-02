using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class StringVisualizer : MonoBehaviour
{
    private LineRenderer lr;

    [Header("Settings")]
    [SerializeField] private LineRenderer targetLineRenderer;
    [SerializeField] private int points = 100;      // Semakin tinggi semakin halus
    [SerializeField] private float width = 0.5f;    // Ketebalan garis
    [SerializeField] private float amplitude = 1.0f; // Tinggi gelombang (Puncak/Lembah)
    [SerializeField] private float frequency = 1.0f; // Jumlah gelombang
    [SerializeField] private float speed = 2.0f;     // Kecepatan gerak (Kiri ke Kanan)
    [SerializeField] private float length = 10.0f;   // Panjang garis di layar

    void Start()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = points;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.useWorldSpace = false; // Agar ikut rotasi object

        if (targetLineRenderer != null)
        {
            targetLineRenderer.positionCount = points;
            targetLineRenderer.startWidth = width;
            targetLineRenderer.endWidth = width;
        }
    }

    void Update()
    {
        DrawTravelingSineWave();
    }

    void DrawTravelingSineWave()
    {
        if (targetLineRenderer == null) return;
        for (int i = 0; i < points; i++)
        {
            float progress = (float)i / (points - 1);
            float x = progress * length;

            float angle = (progress * Mathf.PI * 2 * frequency) + (Time.time * speed);

            float y = Mathf.Sin(x * frequency - Time.time * speed) * amplitude;

            targetLineRenderer.SetPosition(i, new Vector3(x, y, 0));
        }
    }
}