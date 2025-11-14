using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class StringLineAttack : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Transform fireOrigin;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private CamRotation camRotation;
    [SerializeField] private SwitchCharacter SwitchCharacterScript;

    [Header("Shoot Settings")]
    [SerializeField] private float lineSpeed = 15f;
    [SerializeField] private float maxLineLength = 100f;
    [SerializeField] private float attachDelay = 2f;
    [SerializeField] private LayerMask hittableLayers;
    [SerializeField] private float textureScrollSpeed = 1f;

    [Header("Collider")]
    [SerializeField] private GameObject lineObj; 
    [SerializeField] private CapsuleCollider lineCol;

    [Space]
    [SerializeField] private float colliderRadius = 0.5f;
    [SerializeField] private float colliderEndOffset = 0.1f;
    [SerializeField] private float safetyMargin = 0.05f;
    [SerializeField] private CapsuleCollider playerCol;

    private LineRenderer lr;
    private Coroutine fireRoutine;
    private bool isAttached;
    private bool? [] isAttacking; //Domi dan Remi
    private Vector3 targetPoint;

    private float startOffset;
    private Collider ignoredCollider;

    private int museIdx
    {
        get
        {
            if (CompareTag("Domi")) return 0;
            else if (CompareTag("Remi")) return 1;
            else return 2; // Not Domi or Remi
        }
    }

    void Start()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.enabled = false;

        isAttacking = new bool?[3] { false, false, null }; // Domi dan Remi

        startOffset = playerCol ? playerCol.radius + safetyMargin : safetyMargin;

        if (lineObj)
            lineObj.SetActive(false);

        if (playerCol && lineCol)
            Physics.IgnoreCollision(playerCol, lineCol, true);
    }

    void Update()
    {
        if (CompareTag("Player"))
            HandleInput();

        if (lr.enabled && textureScrollSpeed != 0)
            lr.material.mainTextureOffset = new Vector2(Time.time * -textureScrollSpeed, 0f);
            
    }
    void LateUpdate()
    {
        if (isAttacking[museIdx] == false && !isAttached) return;

        // Kamera mengikuti target
        if (isAttacking[museIdx] == true)
            camRotation.LockLookAt(targetPoint, gameObject);
        else if (isAttacking[museIdx] == false) camRotation.CancelLineAttack(gameObject);


        if (lr.enabled)
            lr.SetPosition(0, fireOrigin.position);

        if (isAttached)
            lr.SetPosition(1, targetPoint);

        if (lineObj.activeInHierarchy)
            UpdateCollider();
    }

    private void HandleInput()
    {
    if (Input.GetMouseButtonDown(0))
    {
        if (camRotation.IsAttackLocked && isAttacking[museIdx] == false) return;

        if (isAttached)
        { 
            CancelAttack(museIdx);
        }
    else if (fireRoutine == null) StartAttack(museIdx);
        }
        else if (Input.GetMouseButtonUp(0))
        {
        if (fireRoutine != null)
        {
            CancelAttack(museIdx);
        }
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
        else
        {
            targetPoint = origin + dir * maxLineLength;
        }

        lineObj.SetActive(true);
        lr.enabled = true;

        fireRoutine = StartCoroutine(FireLine());
    }
    private void CancelAttack(int who)
    {
        camRotation.CancelLineAttack(gameObject);

        isAttacking[who] = false;
        isAttached = false;

        if (fireRoutine != null)
        {
            StopCoroutine(fireRoutine);
            fireRoutine = null;
        }

        lr.enabled = false;
        lineObj.SetActive(false);

        ResetIgnored();
        camRotation.IsAttackLocked = false;
    }


    private IEnumerator FireLine()
    {
        Vector3 start = fireOrigin.position;
        Vector3 dir = (targetPoint - start).normalized;
        float dist = Vector3.Distance(start, targetPoint);

        float len = 0f;
        lr.SetPosition(0, start);

        while (len < dist)
        {
            len = Mathf.Min(len + lineSpeed * Time.deltaTime, dist);
            lr.SetPosition(1, start + dir * len);
            yield return null;
        }

        lr.SetPosition(1, targetPoint);

        // Jika memang kena target
        if (ignoredCollider)
        {
            yield return new WaitForSeconds(attachDelay);
            isAttached = true;
        }
        else
        {
            CancelAttack(museIdx);
        }

        fireRoutine = null;
    }

    private void UpdateCollider()
    {
        Vector3 a = fireOrigin.position;
        Vector3 b = lr.GetPosition(1);
        float dist = Vector3.Distance(a, b);

        float colLen = dist - startOffset - colliderEndOffset;
        if (colLen <= 0)
        {
            lineCol.height = 0;
            return;
        }

        lineCol.direction = 2;
        lineCol.radius = colliderRadius;
        lineCol.height = colLen;
        lineCol.center = Vector3.zero;

        Transform t = lineCol.transform;
        t.position = a + (b - a).normalized * (startOffset + colLen / 2f);
        t.LookAt(b);
    }

    private void Ignore(Collider col)
    {
        if (!lineCol || !col) return;
        Physics.IgnoreCollision(lineCol, col, true);
        ignoredCollider = col;
    }

    private void ResetIgnored()
    {
        if (ignoredCollider && lineCol)
            Physics.IgnoreCollision(lineCol, ignoredCollider, false);

        ignoredCollider = null;
    }
}
