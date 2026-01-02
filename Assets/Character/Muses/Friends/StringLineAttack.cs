using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringLineAttack : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Transform fireOrigin;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private CamRotation camRotation;
    [SerializeField] private SwitchCharacter SwitchCharacterScript;

    [Header("Attack Settings")]
    [SerializeField] private float lineSpeed = 15f;
    [SerializeField] private float maxLineLength = 100f;
    [SerializeField] private float attachDelay = 2f;
    [SerializeField] private LayerMask hittableLayers;

    // --- VISUAL SETTINGS ---
    [Header("Visual Settings")]
    [SerializeField] private float textureScrollSpeed = 2f;
    [SerializeField] private Material lineMaterial;
    [SerializeField] private float lineThickness = 0.02f;
    [SerializeField] private Gradient lineColor;

    [Header("Multi-Line Settings")]
    [Range(1, 10)]
    [SerializeField] private int numberOfLines = 5;
    [SerializeField] private float lineSpacing = 0.1f;

    [Header("Magical Wave Settings")]
    [SerializeField] private int points = 100;
    [SerializeField] private float amplitude = 0.25f;
    [SerializeField] private float frequency = 3.0f;
    [SerializeField] private float waveSpeed = 6.0f;

    [Header("Collider")]
    [SerializeField] private GameObject lineObj;
    [SerializeField] private CapsuleCollider lineCol;
    [SerializeField] private float colliderRadius = 0.5f;
    [SerializeField] private float colliderEndOffset = 0.1f;
    [SerializeField] private float safetyMargin = 0.05f;
    [SerializeField] private CapsuleCollider playerCol;

    private List<LineRenderer> lineRenderers = new List<LineRenderer>();
    private Texture2D generatedTexture;

    private Coroutine fireRoutine;
    private bool isAttached;
    private bool?[] isAttacking;
    private Vector3 targetPoint;
    private Vector3 currentTipPosition;
    private float startOffset;
    private Collider ignoredCollider;

    private int museIdx
    {
        get
        {
            if (CompareTag("Domi")) return 0;
            else if (CompareTag("Remi")) return 1;
            else return 2;
        }
    }

    void Start()
    {
        // Safety check material
        if (lineMaterial == null)
        {
            lineMaterial = new Material(Shader.Find("Sprites/Default"));
        }

        // Generate texture warna sekali di awal
        BakeGradientToTexture();

        // Siapkan garis (tapi dalam kondisi mati/enabled=false)
        SetupLines();

        isAttacking = new bool?[3] { false, false, null };
        startOffset = playerCol ? playerCol.radius + safetyMargin : safetyMargin;

        if (lineObj) lineObj.SetActive(false);
        if (playerCol && lineCol) Physics.IgnoreCollision(playerCol, lineCol, true);
    }

    // --- FUNGSI GENERATE WARNA (DIPERBAIKI AGAR BLEND/HALUS) ---
    void BakeGradientToTexture()
    {
        if (generatedTexture != null) Destroy(generatedTexture);

        // Perbaikan: Gunakan TextureFormat.RGBA32 agar warna tidak pecah/dikompresi
        generatedTexture = new Texture2D(256, 1, TextureFormat.RGBA32, false);
        generatedTexture.wrapMode = TextureWrapMode.Repeat;
        generatedTexture.filterMode = FilterMode.Bilinear; // Kunci agar gradasi halus (Blend)

        for (int x = 0; x < 256; x++)
        {
            Color color = lineColor.Evaluate((float)x / 255f);
            generatedTexture.SetPixel(x, 0, color);
        }
        generatedTexture.Apply();
    }

    void SetupLines()
    {
        // Bersihkan garis lama
        foreach (var lr in lineRenderers)
        {
            if (lr != null && lr.gameObject != this.gameObject) Destroy(lr.gameObject);
        }
        lineRenderers.Clear();

        for (int i = 0; i < numberOfLines; i++)
        {
            GameObject subLine = new GameObject($"MagicalLine_{i}");
            subLine.transform.SetParent(this.transform);
            subLine.transform.localPosition = Vector3.zero;

            LineRenderer lr = subLine.AddComponent<LineRenderer>();

            // Setup Material dengan Texture baru
            if (lineMaterial != null)
                lr.material = new Material(lineMaterial);

            lr.material.mainTexture = generatedTexture;

            // Set Warna dasar putih agar texture terlihat asli
            lr.startColor = Color.white;
            lr.endColor = Color.white;

            lr.positionCount = points;
            lr.useWorldSpace = true;
            lr.widthMultiplier = lineThickness;

            // Default MATI saat start
            lr.enabled = false;

            lr.textureMode = LineTextureMode.Tile;

            lineRenderers.Add(lr);
        }
    }

    void Update()
    {
        // --- LOGIKA UPDATE LIVE DI INSPECTOR ---
        // Hanya update properti visual, TIDAK menyalakan garis sembarangan
        if (Application.isEditor)
        {
            // Jika jumlah garis diubah, setup ulang
            if (lineRenderers.Count != numberOfLines) SetupLines();

            // Re-bake texture agar perubahan warna di inspector langsung terlihat
            BakeGradientToTexture();

            foreach (var lr in lineRenderers)
            {
                if (lr == null) continue;
                lr.widthMultiplier = lineThickness;
                // Update texture jika gradient berubah
                if (lr.material != null) lr.material.mainTexture = generatedTexture;
            }
        }

        // Animasi Scroll Warna (Hanya jalan jika garis nyala)
        float offset = Time.time * textureScrollSpeed;
        foreach (var lr in lineRenderers)
        {
            if (lr != null && lr.enabled && lr.material != null)
                lr.material.mainTextureOffset = new Vector2(-offset, 0f);
        }

        if (CompareTag("Player")) HandleInput();
    }

    void LateUpdate()
    {
        bool isActive = false;
        Vector3 endPos = Vector3.zero;

        // Cek apakah sedang menyerang atau nempel
        if (isAttacking[museIdx] == true || isAttached)
        {
            isActive = true;
            endPos = isAttached ? targetPoint : currentTipPosition;

            // Kamera Lock
            if (isAttacking[museIdx] == true) camRotation.LockLookAt(targetPoint, gameObject);
        }
        else
        {
            camRotation.CancelLineAttack(gameObject);
        }

        // --- RENDER CONTROL ---
        if (isActive && lineRenderers.Count > 0)
        {
            // Nyalakan renderer jika belum nyala
            if (!lineRenderers[0].enabled)
                foreach (var lr in lineRenderers) lr.enabled = true;

            DrawMagicalWaveMulti(fireOrigin.position, endPos);
        }
        else
        {
            // Matikan renderer jika tidak aktif
            if (lineRenderers.Count > 0 && lineRenderers[0].enabled)
                foreach (var lr in lineRenderers) lr.enabled = false;
        }

        if (lineObj.activeInHierarchy) UpdateCollider();
    }

    void DrawMagicalWaveMulti(Vector3 start, Vector3 end)
    {
        float distance = Vector3.Distance(start, end);

        // --- FIX INVALID AABB (PENTING) ---
        // Jika jarak sangat dekat (hampir 0), jangan hitung matematika arah karena akan error (NaN).
        // Sebagai gantinya, set semua titik ke posisi 'start' agar garis tidak meledak.
        if (distance < 0.01f) 
        {
            for (int lineIndex = 0; lineIndex < lineRenderers.Count; lineIndex++)
            {
                if(lineRenderers[lineIndex] == null) continue;
                LineRenderer lr = lineRenderers[lineIndex];
                lr.positionCount = points;
                for(int i=0; i<points; i++) lr.SetPosition(i, start);
            }
            return; // Stop di sini, jangan lanjut ke bawah
        }
        // ----------------------------------

        Vector3 direction = (end - start).normalized;
        
        // Safety check lagi: jika direction NaN (error), paksa ke depan
        if (float.IsNaN(direction.x)) direction = Vector3.forward;

        Vector3 waveUp = Vector3.up;
        if (Mathf.Abs(Vector3.Dot(direction, Vector3.up)) > 0.99f) waveUp = Vector3.forward;

        Vector3 side = Vector3.Cross(direction, waveUp).normalized;
        Vector3 finalUp = Vector3.Cross(side, direction).normalized;

        for (int lineIndex = 0; lineIndex < lineRenderers.Count; lineIndex++)
        {
            LineRenderer lr = lineRenderers[lineIndex];
            lr.positionCount = points;

            float offsetMultiplier = lineIndex - (numberOfLines - 1) / 2.0f;
            Vector3 verticalOffset = finalUp * offsetMultiplier * lineSpacing;

            for (int i = 0; i < points; i++)
            {
                float t = (float)i / (points - 1);
                Vector3 basePosition = Vector3.Lerp(start, end, t);
                float phaseShift = lineIndex * 0.2f;
                float waveHeight = Mathf.Sin(t * distance * frequency - Time.time * waveSpeed + phaseShift) * amplitude;
                lr.SetPosition(i, basePosition + verticalOffset + (finalUp * waveHeight));
            }
        }
    }

    // --- INPUT & LOGIC ---
    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (camRotation.IsAttackLocked && isAttacking[museIdx] == false) return;
            if (isAttached) CancelAttack(museIdx);
            else if (fireRoutine == null) StartAttack(museIdx);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (fireRoutine != null) CancelAttack(museIdx);
        }
    }

    private void StartAttack(int who)
    {
        isAttacking[who] = true;
        camRotation.IsAttackLocked = true;
        ResetIgnored();

        RaycastHit hit;
        Vector3 origin = playerCamera.transform.position;
        Vector3 dir = playerCamera.transform.forward;

        if (Physics.Raycast(origin, dir, out hit, maxLineLength, hittableLayers))
        {
            targetPoint = hit.point;
            Ignore(hit.collider);
        }
        else targetPoint = origin + dir * maxLineLength;

        lineObj.SetActive(true);
        currentTipPosition = fireOrigin.position;
        fireRoutine = StartCoroutine(FireLine());
    }

    private void CancelAttack(int who)
    {
        camRotation.CancelLineAttack(gameObject);
        isAttacking[who] = false;
        isAttached = false;
        if (fireRoutine != null) { StopCoroutine(fireRoutine); fireRoutine = null; }

        // lineRenderers akan dimatikan otomatis di LateUpdate karena isActive jadi false
        lineObj.SetActive(false);
        ResetIgnored();
        camRotation.IsAttackLocked = false;
    }

    private IEnumerator FireLine()
    {
        Vector3 start = fireOrigin.position;
        float dist = Vector3.Distance(start, targetPoint);
        float len = 0f;
        while (len < dist)
        {
            len = Mathf.Min(len + lineSpeed * Time.deltaTime, dist);
            Vector3 dir = (targetPoint - start).normalized;
            currentTipPosition = start + dir * len;
            yield return null;
        }
        currentTipPosition = targetPoint;
        if (ignoredCollider) { yield return new WaitForSeconds(attachDelay); isAttached = true; }
        else CancelAttack(museIdx);
        fireRoutine = null;
    }

    private void UpdateCollider()
    {
        Vector3 a = fireOrigin.position;
        Vector3 b = isAttached ? targetPoint : currentTipPosition;
        float dist = Vector3.Distance(a, b);
        float colLen = dist - startOffset - colliderEndOffset;
        if (colLen <= 0) { lineCol.height = 0; return; }
        lineCol.direction = 2; lineCol.radius = colliderRadius; lineCol.height = colLen; lineCol.center = Vector3.zero;
        Transform t = lineCol.transform;
        t.position = a + (b - a).normalized * (startOffset + colLen / 2f);
        t.LookAt(b);
    }

    private void Ignore(Collider col) { if (!lineCol || !col) return; Physics.IgnoreCollision(lineCol, col, true); ignoredCollider = col; }
    private void ResetIgnored() { if (ignoredCollider && lineCol) Physics.IgnoreCollision(lineCol, ignoredCollider, false); ignoredCollider = null; }
}