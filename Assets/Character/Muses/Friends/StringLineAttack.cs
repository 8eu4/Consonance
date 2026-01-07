using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringLineAttack : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Transform fireOrigin;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private CamRotation camRotation;
    [SerializeField] private GameObject guitar;

    [Header("Animation")]
    public Animator animator;

    [Header("Attack Settings")]
    [SerializeField] private float lineSpeed = 25f;
    [SerializeField] private float maxLineLength = 5f;
    [SerializeField] private float breakDistance = 17f;
    [SerializeField] private float attachDelay = 0.5f;
    [SerializeField] private LayerMask hittableLayers;

    [Header("Smoothness Settings")]
    [Tooltip("Waktu slide dari titik tembak (hit) ke titik kunci (center) SETELAH attach.")]
    [SerializeField] private float slideDuration = 0.3f;

    // --- VISUAL SETTINGS ---
    [Header("Visual Settings")]
    [SerializeField] private float textureScrollSpeed = 2f;
    [SerializeField] private Material lineMaterial;
    [SerializeField] private float lineThickness = 0.02f;
    [SerializeField] private Gradient lineColor;
    [Space]
    [SerializeField] private GameObject[] musicNotePrefabs; // Masukkan 2 prefab note disini
    [SerializeField] private float noteSpawnInterval = 0.15f; // Jeda antar spawn note
    [SerializeField] private float noteTravelSpeed = 15f; // Kecepatan note terbang
    private Coroutine noteSpawnRoutine;
    [Space]
    [Header("Particle Feel Settings")]
    [SerializeField] private float minNoteSpeed = 10f; // Speed terlambat
    [SerializeField] private float maxNoteSpeed = 25f; // Speed tercepat
    [SerializeField] private float spawnSpreadRadius = 0.5f; // Seberapa lebar nyebarnya di awal


    [Header("Special Target Settings (Stun & Lock)")]
    [SerializeField] private List<string> specialTags = new List<string> { "Conductor", "Remi", "Domi", "Enemy" };
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
    private float currentRingAlpha = 0f;

    private Texture2D normalTexture;
    private Texture2D specialTexture;

    private Coroutine fireRoutine;
    private Coroutine ringFadeOutRoutine;

    private bool isAttached;
    private bool isLockingProcess; // Penanda sedang proses locking (fading in + sliding)
    private bool?[] isAttacking;

    // --- TRACKING TARGET VARIABEL ---
    private Vector3 targetPoint;
    private Vector3 currentTipPosition;
    private Vector3 initialHitPoint;

    // VARIABEL UNTUK MENEMPEL PADA OBJEK BERGERAK
    private Transform hitTransform;
    private Vector3 hitLocalOffset; // Offset titik tembak awal (misal: Kepala)

    private Vector3 snapVelocity;

    private float startOffset;
    private Collider ignoredCollider;

    private bool isTargetSpecial = false;

    // --- VARIABEL LOCKING KHUSUS ---
    private Transform lockedTargetTransform;
    private Vector3 lockedPosition;
    private Quaternion lockedRotation;
    private Rigidbody cachedRb;
    private bool wasKinematic;
    private Vector3 lockCenterOffset; // Offset ke titik tengah (misal: Perut)

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
        if (guitar) guitar.SetActive(false);
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
        Texture2D currentTex = normalTexture;

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
        // LOGIKA GITAR
        if (guitar != null)
        {
            bool isActivePlayer = transform.parent.parent.CompareTag("Player");
            bool shouldShowGuitar = isAttached && !isActivePlayer;
            if (guitar.activeSelf != shouldShowGuitar) guitar.SetActive(shouldShowGuitar);
        }

        bool isActive = false;
        Vector3 endPos = Vector3.zero;

        // --- UPDATE POSISI TARGET ---
        if (isAttacking[museIdx] == true || isAttached || isLockingProcess)
        {
            if (animator != null && gameObject.transform.parent.parent.CompareTag("Player")) animator.SetBool("Shoot", true);
            isActive = true;

            // Jika sedang dalam proses locking (Delay + Fade In + Slide),
            // Posisi targetPoint dikendalikan penuh oleh Coroutine FireLine agar mulus (Lerp).
            // Jadi kita SKIP update targetPoint manual di sini jika isLockingProcess == true.

            if (!isLockingProcess)
            {
                if (isAttached && isTargetSpecial && lockedTargetTransform != null)
                {
                    // STATE 1: SUDAH TERKUNCI FISIK (Lock Center)
                    lockedTargetTransform.position = lockedPosition;
                    lockedTargetTransform.rotation = lockedRotation;
                    targetPoint = lockedPosition + lockCenterOffset;
                }
                else if (hitTransform != null)
                {
                    // STATE 2: TRACKING BIASA (Ikuti titik tembak)
                    targetPoint = hitTransform.TransformPoint(hitLocalOffset);
                }
            }

            // CEK JARAK PUTUS
            float currentDistance = Vector3.Distance(fireOrigin.position, targetPoint);
            if (currentDistance > breakDistance)
            {
                CancelAttack(museIdx);
                return;
            }

            // SMOOTH DAMP LINE TIP
            if (isAttached || isLockingProcess)
            {
                currentTipPosition = Vector3.SmoothDamp(currentTipPosition, targetPoint, ref snapVelocity, slideDuration);
                endPos = currentTipPosition;
                if (isAttacking[museIdx] == true) camRotation.LockLookAt(endPos, gameObject);
            }
            else
            {
                endPos = currentTipPosition;
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

        // RENDER STUN VISUAL (RING)
        if (showEnemyRing && isTargetSpecial && ringRenderer != null)
        {
            ringRenderer.enabled = true;

            // Render di posisi targetPoint (yang sudah di-lerp oleh coroutine atau update)
            DrawStunEffect(targetPoint);

            Color c1 = specialLineColor.Evaluate(0.5f);
            Color c2 = specialLineColor.Evaluate(1f);
            c1.a = currentRingAlpha;
            c2.a = currentRingAlpha;

            ringRenderer.startColor = c1;
            ringRenderer.endColor = c2;
        }
        else if (ringRenderer != null && ringRenderer.enabled)
        {
            if (ringFadeOutRoutine == null) ringRenderer.enabled = false;
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
        snapVelocity = Vector3.zero;

        if (ringFadeOutRoutine != null) StopCoroutine(ringFadeOutRoutine);
        ringFadeOutRoutine = null;

        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, maxLineLength, hittableLayers))
        {
            hitTransform = hit.transform;
            hitLocalOffset = hitTransform.InverseTransformPoint(hit.point); // SIMPAN LOKASI HIT RELATIF

            targetPoint = hit.point;
            initialHitPoint = hit.point;
            Ignore(hit.collider);
        }
        else
        {
            hitTransform = null;
            targetPoint = playerCamera.transform.position + playerCamera.transform.forward * maxLineLength;
            initialHitPoint = targetPoint;
        }

        lineObj.SetActive(true);
        lineCol.enabled = false;
        currentTipPosition = fireOrigin.position;
        fireRoutine = StartCoroutine(FireLine());

        // MULAI SPAWN NOTES
        if (noteSpawnRoutine != null) StopCoroutine(noteSpawnRoutine);
        noteSpawnRoutine = StartCoroutine(SpawnNotesRoutine());
    }

    private void CancelAttack(int who)
    {
        // Visual Fade Out
        if (showEnemyRing && ringRenderer != null && ringRenderer.enabled)
        {
            if (ringFadeOutRoutine != null) StopCoroutine(ringFadeOutRoutine);
            ringFadeOutRoutine = StartCoroutine(FadeRingOut(0.2f, currentRingAlpha));
        }

        camRotation.CancelLineAttack(gameObject);
        isAttacking[who] = false;
        isAttached = false;
        isLockingProcess = false; // Reset flag locking

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
        hitTransform = null;

        showEnemyRing = false;
        isTargetSpecial = false;

        if (fireRoutine != null) { StopCoroutine(fireRoutine); fireRoutine = null; }

        if (noteSpawnRoutine != null) { StopCoroutine(noteSpawnRoutine); noteSpawnRoutine = null; }

        lineObj.SetActive(false);
        lineCol.enabled = false;

        ResetIgnored();
        camRotation.IsAttackLocked = false;
    }

    private IEnumerator SpawnNotesRoutine()
    {
        if (musicNotePrefabs == null || musicNotePrefabs.Length == 0) yield break;

        while (true)
        {
            int randIndex = Random.Range(0, musicNotePrefabs.Length);
            GameObject selectedPrefab = musicNotePrefabs[randIndex];

            if (selectedPrefab != null)
            {
                // 1. RANDOM SPAWN POSITION (Circle Spread)
                // Menggunakan Random.insideUnitCircle untuk menyebar di area bundar moncong gitar
                Vector2 randomCircle = Random.insideUnitCircle * spawnSpreadRadius;

                // Konversi circle 2D ke posisi 3D relatif terhadap arah gitar (Right & Up)
                Vector3 spawnOffset = (fireOrigin.right * randomCircle.x) + (fireOrigin.up * randomCircle.y);
                Vector3 spawnPos = fireOrigin.position + spawnOffset;

                GameObject note = Instantiate(selectedPrefab, spawnPos, Quaternion.identity);

                // 2. SETUP COMPONENT
                MusicNoteMover mover = note.GetComponent<MusicNoteMover>();
                if (mover == null) mover = note.AddComponent<MusicNoteMover>();

                // Tentukan Target
                Vector3 target = currentTipPosition;
                // Safety net jika target terlalu dekat di frame awal
                if (Vector3.Distance(fireOrigin.position, currentTipPosition) < 0.5f)
                {
                    target = fireOrigin.position + playerCamera.transform.forward * 10f;
                }

                // 3. RANDOM SPEED (Particle Feel)
                float randomSpeed = Random.Range(minNoteSpeed, maxNoteSpeed);

                mover.Initialize(target, randomSpeed);
            }

            // Randomize interval sedikit agar spawn tidak 'robotik' (misal: kadang 0.05, kadang 0.08)
            yield return new WaitForSeconds(Random.Range(noteSpawnInterval * 0.8f, noteSpawnInterval * 1.2f));
        }
    }

    private IEnumerator FadeRingOut(float duration, float startAlpha)
    {
        float timer = 0f;
        LineRenderer lr = ringRenderer;

        while (timer < duration && lr != null)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            float alpha = Mathf.Lerp(startAlpha, 0f, t);

            lr.enabled = true;
            Color c1 = specialLineColor.Evaluate(0.5f);
            Color c2 = specialLineColor.Evaluate(1f);
            c1.a = alpha;
            c2.a = alpha;
            lr.startColor = c1;
            lr.endColor = c2;

            yield return null;
        }
        if (lr != null) lr.enabled = false;
        currentRingAlpha = 0f;
        ringFadeOutRoutine = null;
    }

    private IEnumerator FireLine()
    {
        Vector3 start = fireOrigin.position;
        float dist = Vector3.Distance(start, initialHitPoint);
        float len = 0f;

        // --- FASE 1: LINE TRAVEL ---
        while (len < dist)
        {
            len = Mathf.Min(len + lineSpeed * Time.deltaTime, dist);
            Vector3 dir = (initialHitPoint - start).normalized;
            currentTipPosition = start + dir * len;
            yield return null;
        }

        currentTipPosition = initialHitPoint;

        // --- FASE 2: ATTACH & LOCKING SEQUENCE ---
        if (ignoredCollider)
        {
            bool potentialSpecial = CheckIfSpecial(ignoredCollider);

            if (potentialSpecial)
            {
                // Init Variables
                isTargetSpecial = true;
                showEnemyRing = true;
                currentRingAlpha = 0f;

                // Set referensi transform musuh
                lockedTargetTransform = ignoredCollider.transform.parent != null ? ignoredCollider.transform.parent : ignoredCollider.transform;

                // Hitung posisi awal (Hit Point) & akhir (Center) secara LOKAL relatif terhadap musuh
                // Ini penting agar jika musuh bergerak saat delay, kalkulasi tetap akurat
                Vector3 localHitPos = hitLocalOffset; // Sudah didapat di StartAttack
                Vector3 visualCenterWorld = GetVisualCenter(lockedTargetTransform.gameObject);
                Vector3 localCenterPos = hitTransform.InverseTransformPoint(visualCenterWorld);

                isLockingProcess = true; // Beritahu LateUpdate jangan ganggu targetPoint dulu

                float timer = 0f;
                while (timer < attachDelay)
                {
                    // Update timer & alpha
                    timer += Time.deltaTime;
                    float t = Mathf.Clamp01(timer / attachDelay);
                    currentRingAlpha = t;

                    // SLIDING LOGIC: Interpolasi posisi target dari Hit -> Center
                    // Kita convert kembali Local -> World setiap frame agar menempel pada musuh yg bergerak
                    if (hitTransform != null)
                    {
                        Vector3 currentLocalPos = Vector3.Lerp(localHitPos, localCenterPos, t);
                        targetPoint = hitTransform.TransformPoint(currentLocalPos);
                    }

                    yield return null;
                }

                isLockingProcess = false; // Selesai slide, kembalikan kontrol ke LateUpdate (via Attach state)
            }
            else
            {
                // Jika bukan special, delay biasa saja
                yield return new WaitForSeconds(attachDelay);
            }

            // --- FASE 3: ATTACHED ---
            isAttached = true;
            lineCol.enabled = true;

            if (potentialSpecial)
            {
                ApplyPhysicalLock(); // Kunci fisik musuh di posisi terakhir
            }
            else
            {
                CheckTargetStatus(ignoredCollider);
            }
        }
        else CancelAttack(museIdx);

        fireRoutine = null;
    }

    private bool CheckIfSpecial(Collider col)
    {
        string tagToCheck = col.tag;
        if (col.transform.parent != null && (tagToCheck == "Untagged" || tagToCheck == "Default"))
        {
            tagToCheck = col.transform.parent.tag;
        }
        return specialTags.Contains(tagToCheck);
    }

    private void ApplyPhysicalLock()
    {
        // Kunci posisi di titik saat ini (Center)
        lockedPosition = lockedTargetTransform.position;
        lockedRotation = lockedTargetTransform.rotation;

        // Hitung offset final agar update loop menjaga posisi ini
        Vector3 visualCenter = GetVisualCenter(lockedTargetTransform.gameObject);
        lockCenterOffset = visualCenter - lockedPosition;
        targetPoint = visualCenter;

        cachedRb = lockedTargetTransform.GetComponent<Rigidbody>();
        if (cachedRb != null)
        {
            wasKinematic = cachedRb.isKinematic;
            cachedRb.isKinematic = true;
        }
    }

    private void CheckTargetStatus(Collider col)
    {
        // Fallback method jika logic standard dipakai
        if (CheckIfSpecial(col))
        {
            isTargetSpecial = true;
            showEnemyRing = true;
            currentRingAlpha = 1f;

            lockedTargetTransform = col.transform.parent != null ? col.transform.parent : col.transform;
            ApplyPhysicalLock();
        }
        else
        {
            isTargetSpecial = false;
            showEnemyRing = false;
            lockedTargetTransform = null;
            lockCenterOffset = Vector3.zero;
        }
    }

    private Vector3 GetVisualCenter(GameObject enemy)
    {
        Renderer[] allRenderers = enemy.GetComponentsInChildren<Renderer>();
        if (allRenderers.Length == 0) return enemy.transform.position;

        Bounds combinedBounds = new Bounds();
        bool hasBounds = false;

        foreach (var r in allRenderers)
        {
            if (r is ParticleSystemRenderer || r is TrailRenderer || r is LineRenderer)
                continue;

            if (!hasBounds)
            {
                combinedBounds = r.bounds;
                hasBounds = true;
            }
            else
            {
                combinedBounds.Encapsulate(r.bounds);
            }
        }

        if (!hasBounds) return enemy.transform.position;
        return combinedBounds.center;
    }

    private void UpdateCollider()
    {
        Vector3 a = fireOrigin.position;
        Vector3 b = currentTipPosition;

        float dist = Vector3.Distance(a, b);
        float colLen = dist - startOffset - colliderEndOffset;

        if (colLen <= 0)
        {
            lineCol.size = Vector3.zero;
            return;
        }

        lineCol.size = new Vector3(colliderRadius * 2, colliderRadius * 2, colLen);
        lineCol.center = Vector3.zero;

        Transform t = lineCol.transform;
        t.position = a + (b - a).normalized * (startOffset + colLen / 2f);
        t.LookAt(b);
    }

    private void Ignore(Collider col) { if (!lineCol || !col) return; Physics.IgnoreCollision(lineCol, col, true); ignoredCollider = col; }
    private void ResetIgnored() { if (ignoredCollider && lineCol) Physics.IgnoreCollision(lineCol, ignoredCollider, false); ignoredCollider = null; }
}