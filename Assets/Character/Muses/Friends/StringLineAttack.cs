using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class StringLineAttack : MonoBehaviour
{
    [Header("Referensi Komponen")]
    [SerializeField] private Transform fireOrigin;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private CamRotation camRotationScript;

    [Header("Pengaturan Tembakan")]
    [SerializeField] private float lineSpeed = 15f;
    [SerializeField] private float maxLineLength = 100f;
    [SerializeField] private float attachDelay = 2f;
    [SerializeField] private LayerMask hittableLayers;

    [Header("Referensi Collider Fisik")]
    [Tooltip("Drag 'LineColliderObject' dari Hirarki ke sini")]
    [SerializeField] private GameObject lineColliderGameObject;
    [Tooltip("Drag komponen BoxCollider dari 'LineColliderObject' ke sini")]
    [SerializeField] private BoxCollider lineCollider;

    [Space]
    [Header("Pengaturan Bentuk Collider")]
    [Tooltip("Lebar collider (sumbu X) yang bisa dipijak")]
    [SerializeField] private float colliderWidth = 0.2f;
    [Tooltip("Tinggi collider (sumbu Y)")]
    [SerializeField] private float colliderHeight = 0.2f;
    [Tooltip("Jarak aman agar collider tidak glitch/menabrak target")]
    [SerializeField] private float colliderEndOffset = 0.1f;
    [Tooltip("Jarak aman tambahan agar tidak menabrak player")]
    [SerializeField] private float safetyMargin = 0.05f;

    // Variabel internal
    private LineRenderer lineRenderer;
    private Coroutine firingCoroutine;
    private bool isAttached = false;
    private Vector3 targetPoint;
    private bool iAmTheLockOwner = false;

    // Offset ini dihitung otomatis dari collider player
    private float colliderStartOffset;

    // Referensi ke collider player, diambil otomatis
    private CapsuleCollider playerCollider;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;

        // --- LOGIKA BARU: Ambil collider player otomatis ---
        playerCollider = GetComponent<CapsuleCollider>();
        if (playerCollider != null)
        {
            // Set offset awal = radius player + jarak aman
            colliderStartOffset = playerCollider.radius + safetyMargin;
        }
        else
        {
            Debug.LogWarning("StringLineAttack: Tidak ditemukan CapsuleCollider di " + gameObject.name + ". Menggunakan 'safetyMargin' sebagai offset awal.");
            // Fallback jika player tidak pakai CapsuleCollider
            colliderStartOffset = safetyMargin;
        }
        // --- Akhir Logika Baru ---

        if (lineColliderGameObject != null)
        {
            lineColliderGameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("StringLineAttack: 'Line Collider GameObject' belum di-set!");
        }
    }

    void Update()
    {
        if (gameObject.tag == "Player")
        {
            HandleInput();
        }
        else
        {
            if (iAmTheLockOwner)
            {
                iAmTheLockOwner = false;
                if (camRotationScript != null)
                    camRotationScript.isAttackLocked = false;
            }
        }
    }

    private void HandleInput()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            if (camRotationScript.isAttackLocked && !iAmTheLockOwner) return;

            if (isAttached)
            {
                CancelAttack();
            }
            else if (firingCoroutine == null)
            {
                StartAttack();
            }
        }
        else if (Input.GetButtonUp("Fire1"))
        {
            if (firingCoroutine != null)
            {
                CancelAttack();
            }
        }
    }

    void LateUpdate()
    {
        if (firingCoroutine != null || isAttached)
        {
            if (iAmTheLockOwner)
            {
                if (camRotationScript != null)
                    camRotationScript.LockLookAt(targetPoint);
                else
                    playerCamera.transform.LookAt(targetPoint);
            }

            if (lineRenderer.enabled)
            {
                lineRenderer.SetPosition(0, fireOrigin.position);
                if (isAttached)
                {
                    lineRenderer.SetPosition(1, targetPoint);
                }
            }

            if (lineColliderGameObject.activeInHierarchy)
            {
                UpdateLineColliderPosition();
            }
        }
    }

    private void StartAttack()
    {
        iAmTheLockOwner = true;
        if (camRotationScript != null)
            camRotationScript.isAttackLocked = true;

        RaycastHit hit;
        bool didHit = Physics.Raycast(
            playerCamera.transform.position,
            playerCamera.transform.forward,
            out hit,
            maxLineLength,
            hittableLayers
        );

        if (didHit)
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = playerCamera.transform.position + playerCamera.transform.forward * maxLineLength;
        }

        if (lineColliderGameObject != null)
        {
            lineColliderGameObject.SetActive(true);
        }

        firingCoroutine = StartCoroutine(FireLineCoroutine(targetPoint, didHit));
    }

    private IEnumerator FireLineCoroutine(Vector3 target, bool didHitObject)
    {
        lineRenderer.enabled = true;
        float currentLength = 0f;
        Vector3 initialStartPoint = fireOrigin.position;
        float totalDistance = Vector3.Distance(initialStartPoint, target);
        Vector3 initialDirection = (target - initialStartPoint).normalized;

        lineRenderer.SetPosition(0, initialStartPoint);
        lineRenderer.SetPosition(1, initialStartPoint);

        while (currentLength < totalDistance)
        {
            currentLength += lineSpeed * Time.deltaTime;
            currentLength = Mathf.Min(currentLength, totalDistance);
            Vector3 endPoint = initialStartPoint + initialDirection * currentLength;
            lineRenderer.SetPosition(1, endPoint);
            yield return null;
        }

        lineRenderer.SetPosition(1, target);

        if (didHitObject)
        {
            yield return new WaitForSeconds(attachDelay);
            if (firingCoroutine != null)
            {
                isAttached = true;
            }
        }
        else
        {
            CancelAttack();
        }

        firingCoroutine = null;
    }

    public void CancelAttack()
    {
        if (firingCoroutine != null)
        {
            StopCoroutine(firingCoroutine);
            firingCoroutine = null;
        }

        isAttached = false;
        lineRenderer.enabled = false;

        if (lineColliderGameObject != null)
        {
            lineColliderGameObject.SetActive(false);
        }

        if (iAmTheLockOwner)
        {
            iAmTheLockOwner = false;
            if (camRotationScript != null)
                camRotationScript.isAttackLocked = false;
        }
    }

    private void UpdateLineColliderPosition()
    {
        Vector3 startPoint = fireOrigin.position;
        Vector3 endPoint = lineRenderer.GetPosition(1);

        Vector3 direction = (endPoint - startPoint).normalized;
        float totalDistance = Vector3.Distance(startPoint, endPoint);

        // Gunakan colliderStartOffset (otomatis) dan colliderEndOffset (manual)
        float colliderLength = totalDistance - colliderStartOffset - colliderEndOffset;

        if (colliderLength <= 0)
        {
            lineCollider.size = Vector3.zero;
            return;
        }

        // --- LOGIKA BARU: Gunakan Width dan Height ---
        // Atur ukuran: Lebar (X), Tinggi (Y), Panjang (Z)
        lineCollider.size = new Vector3(colliderWidth, colliderHeight, colliderLength);
        // --- Akhir Logika Baru ---

        Vector3 colliderMidpoint = startPoint + direction * (colliderStartOffset + colliderLength / 2f);

        Transform colliderT = lineCollider.transform;
        colliderT.position = colliderMidpoint;
        colliderT.LookAt(endPoint);
    }
}