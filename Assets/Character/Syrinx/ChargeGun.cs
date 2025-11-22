using UnityEngine;
using System.Collections;

public class ChargeGun : MonoBehaviour
{
    [Header("References")]
    public Transform firePoint;
    public Transform player;
    public GameObject laserSightPrefab;
    public GameObject laserBeamPrefab;
    public GameObject telegraphPrefab;
    public AudioSource chargeAudio;
    public SyrinxMovement movementScript;

    [Header("Timings")]
    public Vector2 reloadTimeRange = new Vector2(1f, 5f);
    public float chargeTime = 2f;
    public float fixedShootOffset = 1.5f;
    public float laserTime = 1.2f;
    public float laserGrowTime = 0.25f;

    [Header("Ground Settings")]
    public float groundY = 0f;
    public float telegraphFadeSpeed = 2f;

    [Header("Ranges")]
    public float attackRange = 6f;   // distance to attack
    //public float viewRange = 10f;    // distance before chasing

    [Header("Behavior")]
    public bool stopMovementWhileFiring = true;

    private Transform currentSight;
    private GameObject currentTelegraph;
    private Vector3 lockedTargetPos;
    private Material telegraphMat;

    private void Start()
    {
        StartCoroutine(FireRoutine());
    }

    private IEnumerator FireRoutine()
    {
        while (true)
        {
            if (player == null)
            {
                yield return null;
                continue;
            }

            float distance = Vector3.Distance(transform.position, player.position);

            // 🚫 Skip if target too far
            if (distance > attackRange)
            {
                yield return null;
                continue;
            }

            // ⏳ Random reload before shooting
            float reloadTime = Random.Range(reloadTimeRange.x, reloadTimeRange.y);
            yield return new WaitForSeconds(reloadTime);

            // ⚡ Play charge sound
            if (chargeAudio != null)
                chargeAudio.Play();

            // 🎯 Spawn telegraph + sight
            if (laserSightPrefab != null)
                currentSight = Instantiate(laserSightPrefab).transform;

            if (telegraphPrefab != null)
            {
                currentTelegraph = Instantiate(telegraphPrefab);
                telegraphMat = currentTelegraph.GetComponentInChildren<Renderer>()?.material;
                if (telegraphMat != null)
                    SetTelegraphAlpha(0f);
            }

            float elapsed = 0f;
            bool hasLockedTarget = false;

            // ⚙️ CHARGE PHASE
            while (elapsed < chargeTime)
            {
                elapsed += Time.deltaTime;

                if (player != null)
                {
                    Vector3 targetPos = new Vector3(player.position.x, groundY, player.position.z);

                    if (!hasLockedTarget)
                    {
                        UpdateSight(targetPos);
                        UpdateTelegraph(targetPos);

                        if (elapsed >= (chargeTime - fixedShootOffset))
                        {
                            hasLockedTarget = true;
                            lockedTargetPos = targetPos;
                        }
                    }
                    else
                    {
                        UpdateSight(lockedTargetPos);
                        UpdateTelegraph(lockedTargetPos);
                    }
                }

                if (telegraphMat != null)
                {
                    float alpha = Mathf.Clamp01(elapsed / chargeTime);
                    SetTelegraphAlpha(alpha);
                }

                yield return null;
            }

            // 💥 FIRE LASER
            if (laserBeamPrefab != null)
                yield return StartCoroutine(FireLaserBeam(lockedTargetPos));

            // 🧹 CLEANUP
            if (currentSight != null)
                Destroy(currentSight.gameObject);
            if (currentTelegraph != null)
                StartCoroutine(FadeOutAndDestroyTelegraph());
        }
    }

    private void UpdateSight(Vector3 targetPos)
    {
        if (currentSight == null) return;
        LineRenderer lr = currentSight.GetComponent<LineRenderer>();
        if (lr != null && firePoint != null)
        {
            lr.SetPosition(0, firePoint.position);
            lr.SetPosition(1, targetPos);
        }
    }

    private void UpdateTelegraph(Vector3 targetPos)
    {
        if (currentTelegraph == null) return;
        currentTelegraph.transform.position = targetPos + Vector3.up * 0.01f;
    }

    private IEnumerator FireLaserBeam(Vector3 targetPos)
    {
        if (firePoint == null) yield break;

        if (stopMovementWhileFiring && movementScript != null)
            movementScript.PauseMovement();

        GameObject laser = Instantiate(laserBeamPrefab);
        LineRenderer lr = laser.GetComponent<LineRenderer>();
        if (lr == null)
        {
            if (stopMovementWhileFiring && movementScript != null)
                movementScript.ResumeMovement();
            yield break;
        }

        Vector3 start = firePoint.position;
        lr.SetPosition(0, start);
        lr.SetPosition(1, start);

        float elapsed = 0f;
        while (elapsed < laserGrowTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / laserGrowTime;
            Vector3 endPos = Vector3.Lerp(start, targetPos, t);
            lr.SetPosition(1, endPos);
            yield return null;
        }

        lr.SetPosition(1, targetPos);
        yield return new WaitForSeconds(laserTime);

        if (stopMovementWhileFiring && movementScript != null)
            movementScript.ResumeMovement();

        Destroy(laser);
    }

    private void SetTelegraphAlpha(float alpha)
    {
        if (telegraphMat != null)
        {
            Color c = telegraphMat.color;
            c.a = alpha;
            telegraphMat.color = c;
        }
    }

    private IEnumerator FadeOutAndDestroyTelegraph()
    {
        if (telegraphMat == null || currentTelegraph == null)
        {
            Destroy(currentTelegraph);
            yield break;
        }

        float alpha = telegraphMat.color.a;
        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * telegraphFadeSpeed;
            SetTelegraphAlpha(alpha);
            yield return null;
        }

        Destroy(currentTelegraph);
    }
}
