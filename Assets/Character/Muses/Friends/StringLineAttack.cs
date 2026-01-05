using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringLineAttack : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Transform fireOrigin;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private CamRotation camRotation;

    [Header("Animation")]
    public Animator animator; // Assign di Inspector

    [Header("Attack Settings")]
    [SerializeField] private float lineSpeed = 25f;
    [SerializeField] private float maxLineLength = 5f;
    [SerializeField] private float breakDistance = 17f;
    [SerializeField] private float attachDelay = 0.5f;
    [SerializeField] private LayerMask hittableLayers;

    [Header("Smoothness Settings")]
    [Tooltip("Waktu slide dari titik tembak (hit) ke titik kunci (center).")]
    [SerializeField] private float slideDuration = 0.3f;

    // --- VISUAL SETTINGS ---
    [Header("Visual Settings")]
    [SerializeField] private float textureScrollSpeed = 2f;
    [SerializeField] private Material lineMaterial;
    [SerializeField] private float lineThickness = 0.02f;
    [SerializeField] private Gradient lineColor;

    [Header("Special Target Settings (Stun & Lock)")]
    [SerializeField] private List<string> specialTags = new List<string> { "Remi", "Domi", "Enemy" };
    [SerializeField] private Gradient specialLineColor;
    [SerializeField] private float stunCoilHeight = 2.0f;
    [SerializeField] private float stunCoilSpeed = 10f;

    [Header("Multi-Line Settings")]
    [Range(1, 10)]
    [SerializeField] private int numberOfLines = 5;
    [SerializeField] private float lineSpacing = 0.1f;

    [Header("Magical Wave Settings")]
    [SerializeField] private int points = 100;
    [SerializeField] private float amplitude = 0.25f;
    [SerializeField] private float frequency = 3.0f;
    [SerializeField] private float waveSpeed = 6.0f;

    [Header("Enemy Ring Settings")]
    [SerializeField] private float ringRadius = 0.8f;
    [SerializeField] private float ringWidth = 0.1f;
    [SerializeField] private int ringSegments = 50;

    [Header("Collider")]
    [SerializeField] private GameObject lineObj;
    [SerializeField] private BoxCollider lineCol;
    [SerializeField] private float colliderRadius = 0.5f;
    [SerializeField] private float colliderEndOffset = 0.1f;
    [SerializeField] private float safetyMargin = 0.05f;
    [SerializeField] private CapsuleCollider playerCol;

    private List<LineRenderer> lineRenderers = new List<LineRenderer>();
    private LineRenderer ringRenderer;
    private bool showEnemyRing = false;

    private Texture2D normalTexture;
    private Texture2D specialTexture;

    private Coroutine fireRoutine;
    private bool isAttached;
    private bool?[] isAttacking;

    private Vector3 targetPoint;       // Posisi tujuan (Center Musuh)
    private Vector3 currentTipPosition; // Ujung tali visual
    private Vector3 initialHitPoint;    // Titik awal kena (misal Kepala)

    private Vector3 snapVelocity;

    private float startOffset;
    private Collider ignoredCollider;

    private bool isTargetSpecial = false;

    // --- VARIABEL LOCKING ---
    private Transform lockedTargetTransform;
    private Vector3 lockedPosition;
    private Quaternion lockedRotation;
    private Rigidbody cachedRb;
    private bool wasKinematic;

    private int museIdx
    {
        get
        {
            if (gameObject.transform.parent.parent.CompareTag("Domi")) return 0;
            else if (gameObject.transform.parent.parent.CompareTag("Remi")) return 1;
            else return 2;
        }
    }

    void Start()
    {
        if (lineMaterial == null) lineMaterial = new Material(Shader.Find("Sprites/Default"));

        BakeGradients();
        SetupLines();
        SetupRing();

        isAttacking = new bool?[3] { false, false, null };
        startOffset = playerCol ? playerCol.radius + safetyMargin : safetyMargin;

        if (lineObj) lineObj.SetActive(false);
        if (playerCol && lineCol) Physics.IgnoreCollision(playerCol, lineCol, true);
    }

    void SetupRing()
    {
        if (ringRenderer != null) Destroy(ringRenderer.gameObject);
        GameObject ringObj = new GameObject("StunEffectRing");
        ringObj.transform.SetParent(this.transform);
        ringObj.transform.localPosition = Vector3.zero;

        ringRenderer = ringObj.AddComponent<LineRenderer>();
        ringRenderer.material = new Material(Shader.Find("Sprites/Default"));
        ringRenderer.loop = false;
        ringRenderer.useWorldSpace = true;
        ringRenderer.widthMultiplier = ringWidth;
        ringRenderer.positionCount = ringSegments;
        ringRenderer.enabled = false;
    }

    void SetupLines()
    {
        foreach (var lr in lineRenderers) { if (lr && lr.gameObject != gameObject) Destroy(lr.gameObject); }
        lineRenderers.Clear();

        for (int i = 0; i < numberOfLines; i++)
        {
            GameObject subLine = new GameObject($"MagicalLine_{i}");
            subLine.transform.SetParent(transform);
            subLine.transform.localPosition = Vector3.zero;
            LineRenderer lr = subLine.AddComponent<LineRenderer>();

            if (lineMaterial) lr.material = new Material(lineMaterial);
            lr.material.mainTexture = normalTexture;
            lr.textureMode = LineTextureMode.Tile;
            lr.startColor = Color.white;
            lr.endColor = Color.white;
            lr.useWorldSpace = true;
            lr.widthMultiplier = lineThickness;
            lr.enabled = false;
            lineRenderers.Add(lr);
        }
    }

    void BakeGradients()
    {
        if (normalTexture) Destroy(normalTexture);
        normalTexture = BakeTextureFromGradient(lineColor);
        if (specialTexture) Destroy(specialTexture);
        specialTexture = BakeTextureFromGradient(specialLineColor);
    }

    Texture2D BakeTextureFromGradient(Gradient grad)
    {
        Texture2D tex = new Texture2D(256, 1, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.filterMode = FilterMode.Bilinear;
        for (int x = 0; x < 256; x++) tex.SetPixel(x, 0, grad.Evaluate(x / 255f));
        tex.Apply();
        return tex;
    }

    void Update()
    {
        if (Application.isEditor)
        {
            if (lineRenderers.Count != numberOfLines) SetupLines();
            if (ringRenderer) ringRenderer.widthMultiplier = ringWidth;
        }

        float offset = Time.time * textureScrollSpeed;
        Texture2D currentTex = (isAttached && isTargetSpecial) ? specialTexture : normalTexture;

        foreach (var lr in lineRenderers)
        {
            if (lr && lr.enabled)
            {
                lr.material.mainTexture = currentTex;
                lr.material.mainTextureOffset = new Vector2(-offset, 0);
            }
        }

        if (gameObject.transform.parent.parent.CompareTag("Player")) HandleInput();
    }

    void LateUpdate()
    {
        bool isActive = false;
        Vector3 endPos = Vector3.zero;


        if (isAttacking[museIdx] == true || isAttached)
        {
            
            if (animator != null && gameObject.transform.parent.parent.CompareTag("Player")) animator.SetBool("Shoot", true);
            isActive = true;

            // 1. CEK JARAK
            float currentDistance = Vector3.Distance(fireOrigin.position, targetPoint);
            if (currentDistance > breakDistance)
            {
                CancelAttack(museIdx);
                return;
            }

            // 2. LOGIKA LOCK (PHYSICS)
            if (isAttached && ignoredCollider != null)
            {
                if (isTargetSpecial && lockedTargetTransform != null)
                {
                    lockedTargetTransform.position = lockedPosition;
                    lockedTargetTransform.rotation = lockedRotation;
                    targetPoint = lockedPosition;
                }
            }

            // 3. LOGIKA POSISI VISUAL & KAMERA
            if (isAttached)
            {
                // -- FASE 2: SUDAH NEMPEL (SLIDE) --
                // Tali bergerak halus dari Titik Kena Awal -> Titik Center
                currentTipPosition = Vector3.SmoothDamp(currentTipPosition, targetPoint, ref snapVelocity, slideDuration);
                endPos = currentTipPosition;

                // Kamera mengikuti pergerakan halus tali (Slide effect)
                if (isAttacking[museIdx] == true) camRotation.LockLookAt(endPos, gameObject);
            }
            else
            {
                // -- FASE 1: SEDANG TERBANG (FLYING) --
                // Tali mengikuti peluru/ujung visual
                endPos = currentTipPosition;

                // PERBAIKAN:
                // Saat terbang, Kamera fokus ke TARGET UTAMA (Crosshair), BUKAN ke ujung tali yang baru keluar dari tangan.
                // Ini mencegah kamera "nunduk" melihat tangan.
                if (isAttacking[museIdx] == true) camRotation.LockLookAt(targetPoint, gameObject);
            }
        }
        else
        {
            if (animator != null && gameObject.transform.parent.parent.CompareTag("Player")) animator.SetBool("Shoot", false);
            camRotation.CancelLineAttack(gameObject);
        }

        // RENDER LINES
        if (isActive && lineRenderers.Count > 0)
        {
            if (!lineRenderers[0].enabled) foreach (var lr in lineRenderers) lr.enabled = true;
            DrawMagicalWaveMulti(fireOrigin.position, endPos);
        }
        else
        {
            if (lineRenderers.Count > 0 && lineRenderers[0].enabled) foreach (var lr in lineRenderers) lr.enabled = false;
        }

        // RENDER STUN VISUAL
        if (showEnemyRing && isTargetSpecial && ringRenderer != null)
        {
            ringRenderer.enabled = true;
            // Gambar ring di posisi endPos (biar ikut slide dari kepala ke badan)
            DrawStunEffect(endPos);
            ringRenderer.startColor = specialLineColor.Evaluate(0.5f);
            ringRenderer.endColor = specialLineColor.Evaluate(1f);
        }
        else if (ringRenderer != null && ringRenderer.enabled)
        {
            ringRenderer.enabled = false;
        }

        if (lineObj.activeInHierarchy) UpdateCollider();
    }

    void DrawStunEffect(Vector3 center)
    {
        ringRenderer.loop = false;
        int ptCount = ringSegments;
        float heightStep = stunCoilHeight / ptCount;
        float angleStep = 25f;
        float rot = -Time.time * stunCoilSpeed * 50f;

        for (int i = 0; i < ptCount; i++)
        {
            float angle = i * angleStep + rot;
            float currentRadius = ringRadius * (1f - (float)i / ptCount * 0.2f);

            float x = Mathf.Cos(Mathf.Deg2Rad * angle) * currentRadius;
            float z = Mathf.Sin(Mathf.Deg2Rad * angle) * currentRadius;
            float y = (i * heightStep) - (stunCoilHeight / 2f);

            ringRenderer.SetPosition(i, center + new Vector3(x, y, z));
        }
    }

    void DrawMagicalWaveMulti(Vector3 start, Vector3 end)
    {
        float dist = Vector3.Distance(start, end);
        if (dist < 0.01f) return;

        Vector3 dir = (end - start).normalized;
        if (float.IsNaN(dir.x)) dir = Vector3.forward;

        Vector3 waveUp = Mathf.Abs(Vector3.Dot(dir, Vector3.up)) > 0.99f ? Vector3.forward : Vector3.up;
        Vector3 side = Vector3.Cross(dir, waveUp).normalized;
        Vector3 finalUp = Vector3.Cross(side, dir).normalized;

        for (int k = 0; k < lineRenderers.Count; k++)
        {
            LineRenderer lr = lineRenderers[k];
            lr.positionCount = points;
            float offsetMult = k - (numberOfLines - 1) / 2f;
            Vector3 vOffset = finalUp * offsetMult * lineSpacing;

            for (int i = 0; i < points; i++)
            {
                float t = (float)i / (points - 1);
                float h = Mathf.Sin(t * dist * frequency - Time.time * waveSpeed + k * 0.2f) * amplitude;
                lr.SetPosition(i, Vector3.Lerp(start, end, t) + vOffset + finalUp * h);
            }
        }
    }

    // --- INPUT & LOGIC ---
    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //if (animator != null) animator.SetBool("Shoot", true);
            if (camRotation.IsAttackLocked && isAttacking[museIdx] == false) return;
            if (isAttached) CancelAttack(museIdx);
            else if (fireRoutine == null) StartAttack(museIdx);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            //if (animator != null) animator.SetBool("Shoot", false);
            if (fireRoutine != null) CancelAttack(museIdx);
        }
    }

    private void StartAttack(int who)
    {
        isAttacking[who] = true;
        camRotation.IsAttackLocked = true;
        ResetIgnored();

        snapVelocity = Vector3.zero;

        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, maxLineLength, hittableLayers))
        {
            targetPoint = hit.point; // Ini titik Center/Hit tujuan
            initialHitPoint = hit.point; // Simpan titik tabrak awal (misal kepala)
            Ignore(hit.collider);
        }
        else
        {
            targetPoint = playerCamera.transform.position + playerCamera.transform.forward * maxLineLength;
            initialHitPoint = targetPoint;
        }

        lineObj.SetActive(true);
        currentTipPosition = fireOrigin.position; // Tali mulai dari tangan
        fireRoutine = StartCoroutine(FireLine());
    }

    private void CancelAttack(int who)
    {
        camRotation.CancelLineAttack(gameObject);
        isAttacking[who] = false;
        isAttached = false;

        snapVelocity = Vector3.zero;

        if (isTargetSpecial && lockedTargetTransform != null)
        {
            if (cachedRb != null)
            {
                cachedRb.isKinematic = wasKinematic;
            }
        }

        lockedTargetTransform = null;
        cachedRb = null;

        showEnemyRing = false;
        isTargetSpecial = false;

        if (fireRoutine != null) { StopCoroutine(fireRoutine); fireRoutine = null; }

        lineObj.SetActive(false);
        ResetIgnored();
        camRotation.IsAttackLocked = false;
    }

    private IEnumerator FireLine()
    {
        Vector3 start = fireOrigin.position;
        // Hitung jarak ke titik tabrak awal (initialHitPoint) bukan targetPoint yang mungkin berubah
        float dist = Vector3.Distance(start, initialHitPoint);
        float len = 0f;

        while (len < dist)
        {
            len = Mathf.Min(len + lineSpeed * Time.deltaTime, dist);
            Vector3 dir = (initialHitPoint - start).normalized;
            currentTipPosition = start + dir * len;
            yield return null;
        }

        // Pastikan ujung tali pas di titik tabrak sebelum logic attached jalan
        currentTipPosition = initialHitPoint;

        if (ignoredCollider)
        {
            yield return new WaitForSeconds(attachDelay);

            isAttached = true;
            CheckTargetStatus(ignoredCollider);
        }
        else CancelAttack(museIdx);

        fireRoutine = null;
    }

    private void CheckTargetStatus(Collider col)
    {
        string tagToCheck = col.tag;

        if (col.transform.parent != null && (tagToCheck == "Untagged" || tagToCheck == "Default"))
        {
            tagToCheck = col.transform.parent.tag;
        }

        if (specialTags.Contains(tagToCheck))
        {
            isTargetSpecial = true;
            showEnemyRing = true;

            lockedTargetTransform = col.transform.parent != null ? col.transform.parent : col.transform;
            lockedPosition = lockedTargetTransform.position;
            lockedRotation = lockedTargetTransform.rotation;

            // Saat attached, targetPoint kita ubah menjadi Pusat Badan (Lock Position)
            // Tapi currentTipPosition masih di initialHitPoint (Kepala).
            // LateUpdate akan memuluskan perpindahan dari Kepala -> Pusat Badan.
            targetPoint = lockedPosition;

            cachedRb = lockedTargetTransform.GetComponent<Rigidbody>();
            if (cachedRb != null)
            {
                wasKinematic = cachedRb.isKinematic;
                cachedRb.isKinematic = true;
            }
        }
        else
        {
            isTargetSpecial = false;
            showEnemyRing = false;
            lockedTargetTransform = null;
        }
    }

    // --- TIMPA FUNGSI INI ---
    private void UpdateCollider()
    {
        Vector3 a = fireOrigin.position;
        Vector3 b = currentTipPosition;

        float dist = Vector3.Distance(a, b);
        float colLen = dist - startOffset - colliderEndOffset;

        // Kalau belum panjang, sembunyikan
        if (colLen <= 0) 
        { 
            lineCol.size = Vector3.zero; 
            return; 
        }

        // --- LOGIKA BARU (BOX) ---
        // X & Y = Tebal (2x radius), Z = Panjang
        lineCol.size = new Vector3(colliderRadius * 2, colliderRadius * 2, colLen);
        
        lineCol.center = Vector3.zero;

        Transform t = lineCol.transform;
        t.position = a + (b - a).normalized * (startOffset + colLen / 2f);
        t.LookAt(b);
    }

    private void Ignore(Collider col) { if (!lineCol || !col) return; Physics.IgnoreCollision(lineCol, col, true); ignoredCollider = col; }
    private void ResetIgnored() { if (ignoredCollider && lineCol) Physics.IgnoreCollision(lineCol, ignoredCollider, false); ignoredCollider = null; }
}