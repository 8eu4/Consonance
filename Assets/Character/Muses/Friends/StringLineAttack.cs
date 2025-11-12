using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class StringLineAttack : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Transform fireOrigin;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private CamRotation camRotationScript;

    [Header("Shoot Settings")]
    [SerializeField] private float lineSpeed = 15f;
    [SerializeField] private float maxLineLength = 100f;
    [SerializeField] private float attachDelay = 2f;
    [SerializeField] private LayerMask hittableLayers;
    [SerializeField] private float textureScrollSpeed = 1f;

    [Header("Collider")]
    [SerializeField] private GameObject lineColliderGameObject;
    [SerializeField] private CapsuleCollider lineCollider;

    [Space]
    [SerializeField] private float colliderRadius = 0.5f;
    [SerializeField] private float colliderEndOffset = 0.1f;
    [SerializeField] private float safetyMargin = 0.05f;

    [Space]
    [SerializeField] private CapsuleCollider playerCollider;

    // Variabel internal
    private LineRenderer lineRenderer;
    private Coroutine firingCoroutine;
    private bool isAttached = false;
    private Vector3 targetPoint;
    private bool iAmTheLockOwner = false;
    private float colliderStartOffset;
    private Collider ignoredTargetCollider = null; // Target yang di-ignore

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;

        if (playerCollider != null)
        {
            colliderStartOffset = playerCollider.radius/2 + safetyMargin;
        }
        else
        {
            Debug.LogWarning("StringLineAttack: Tidak ditemukan CapsuleCollider di " + gameObject.name + ". Menggunakan 'safetyMargin' sebagai offset awal.");
            colliderStartOffset = safetyMargin;
        }

        if (lineColliderGameObject != null)
        {
            lineColliderGameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("StringLineAttack: 'Line Collider GameObject' belum di-set!");
        }

        if (lineCollider != null)
        {
            lineCollider.direction = 2; // Orientasi kapsul sepanjang sumbu Z
            lineCollider.center = Vector3.zero;

            // Selalu abaikan diri sendiri (Player)
            if (playerCollider != null)
            {
                Physics.IgnoreCollision(playerCollider, lineCollider, true);
            }
            else
            {
                Debug.LogWarning("StringLineAttack: Gagal menemukan playerCollider untuk di-ignore.");
            }
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
                    camRotationScript.IsAttackLocked = false;
            }
        }

        if (lineRenderer.enabled && textureScrollSpeed != 0f)
        {
            // 1. Hitung offset baru berdasarkan waktu
            // Kita pakai Time.time agar gerakannya mulus dan konsisten
            // Kita pakai - (minus) agar tekstur bergerak "maju" (dari asal ke target)
            float newOffsetX = Time.time * -textureScrollSpeed;

            // 2. Terapkan offset ke material LineRenderer
            // Kita hanya perlu menggeser sumbu X (horizontal di UV map)
            // (Catatan: Ini akan otomatis membuat instance material baru)
            lineRenderer.material.mainTextureOffset = new Vector2(newOffsetX, 0f);
        }
    }

    private void HandleInput()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            if (camRotationScript.IsAttackLocked && !iAmTheLockOwner) return;

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
            camRotationScript.IsAttackLocked = true;

        // Reset target lama jika ada
        if (ignoredTargetCollider != null && lineCollider != null)
        {
            Physics.IgnoreCollision(lineCollider, ignoredTargetCollider, false);
            ignoredTargetCollider = null;
        }

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

            // Ignore target yang baru ditembak
            if (hit.collider != null && lineCollider != null)
            {
                Physics.IgnoreCollision(lineCollider, hit.collider, true);
                ignoredTargetCollider = hit.collider; // Simpan referensi
            }
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

        // Aktifkan lagi kolisi dengan target yang tadi di-ignore
        if (ignoredTargetCollider != null && lineCollider != null)
        {
            Physics.IgnoreCollision(lineCollider, ignoredTargetCollider, false);
            ignoredTargetCollider = null;
        }

        if (iAmTheLockOwner)
        {
            iAmTheLockOwner = false;
            if (camRotationScript != null)
                camRotationScript.IsAttackLocked = false;
        }
    }

    private void UpdateLineColliderPosition()
    {
        Vector3 startPoint = fireOrigin.position;
        Vector3 endPoint = lineRenderer.GetPosition(1);

        Vector3 direction = (endPoint - startPoint).normalized;
        float totalDistance = Vector3.Distance(startPoint, endPoint);

        float colliderLength = totalDistance - colliderStartOffset - colliderEndOffset;

        if (colliderLength <= 0)
        {
            lineCollider.height = 0f;
            return;
        }

        lineCollider.direction = 2; // Sumbu Z
        lineCollider.radius = colliderRadius;
        lineCollider.height = colliderLength;
        lineCollider.center = Vector3.zero;

        Vector3 colliderMidpoint = startPoint + direction * (colliderStartOffset + colliderLength / 2f);

        Transform colliderT = lineCollider.transform;
        colliderT.position = colliderMidpoint;
        colliderT.LookAt(endPoint);
    }
}