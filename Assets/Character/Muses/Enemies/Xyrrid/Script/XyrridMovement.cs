using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class XyrridMovement : MonoBehaviour
{
    // =========================
    // Cached Components
    // =========================
    private NavMeshAgent agent;
    private Animator anim;

    // =========================
    // Targeting
    // =========================
    [Header("Targeting")]
    public NearestTargetDetector targetDetector;
    public Transform currentTarget;

    // =========================
    // Chase Settings
    // =========================
    [Header("Chase Settings")]
    public float viewRange = 12f;
    public float stopChaseDistance = 6f;
    public float chaseSpeed = 5f;

    // =========================
    // Line of Sight (LOS)
    // =========================
    [Header("Line of Sight")]
    public LayerMask obstacleMask;
    public float eyeHeight = 1.2f;
    public float losCheckInterval = 0.12f;

    // =========================
    // Path Validation (SMART)
    // =========================
    [Header("Path Validation")]
    public float pathCheckInterval = 0.25f;
    public float targetMoveThreshold = 0.5f;

    // =========================
    // Circle Reposition / Strafing
    // =========================
    [Header("Strafe & Reposition")]
    [Tooltip("Jarak lateral untuk strafe (unit)")]
    public float strafeDistance = 3f;
    [Tooltip("Seberapa besar radius sample NavMesh untuk titik strafe")]
    public float strafeSampleRadius = 0.5f;
    [Tooltip("Berapa sering (detik) ganti arah/cek strafe baru")]
    public float strafeChangeInterval = 1.0f;
    [Tooltip("Jika true, Xyrrid akan aktif men-strafe saat di posisi tembak")]
    public bool enableStrafe = true;
    [Tooltip("Kecepatan rotasi saat melihat target (deg/sec)")]
    public float lookRotationSpeedDeg = 360f;

    // =========================
    // Internal State
    // =========================
    private bool isChasing;
    private bool isRepositioning;
    private float repositionTimer;

    // LOS cache
    private float losCheckTimer = 0f;
    private bool hasLineOfSightCached = false;

    // Path caching / reuse (avoid GC)
    private NavMeshPath cachedPath;          // for main path to target
    private NavMeshPath cachedStrafePath;    // for strafe candidate points
    private float pathCheckTimer = 0f;
    private Vector3 lastTargetPosition;
    private bool hasValidPath = false;

    // Strafe state
    private int currentStrafeDir = 1; // 1 = right, -1 = left
    private float strafeTimer = 0f;
    private Vector3 strafeTargetPos = Vector3.zero;
    private bool hasStrafeDestination = false;

    private static readonly int AnimX = Animator.StringToHash("InputX");
    private static readonly int AnimZ = Animator.StringToHash("InputZ");

    // Reusable temp vectors to avoid allocations
    private Vector3 tmpA = Vector3.zero;
    private Vector3 tmpB = Vector3.zero;

    // =========================
    // Unity Lifecycle
    // =========================
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        // Decouple rotation - we rotate manually in combat mode
        agent.updateRotation = false;

        agent.speed = chaseSpeed;
        agent.acceleration = 16f;
        agent.angularSpeed = 720f;
        agent.stoppingDistance = stopChaseDistance;

        // reuse NavMeshPath instances
        cachedPath = new NavMeshPath();
        cachedStrafePath = new NavMeshPath();

        losCheckTimer = 0f;
        pathCheckTimer = 0f;
        lastTargetPosition = Vector3.positiveInfinity;
        strafeTimer = Random.Range(0f, strafeChangeInterval); // desynchronize between enemies
    }

    private void Update()
    {
        ResolveTarget();
        UpdateState();
        UpdateAnimator();
    }

    // =========================
    // Target Resolution
    // =========================
    private void ResolveTarget()
    {
        if (targetDetector == null) return;
        currentTarget = targetDetector.currentTarget;
    }

    // =========================
    // State Logic
    // =========================
    private void UpdateState()
    {
        if (currentTarget == null)
        {
            agent.ResetPath();
            agent.isStopped = true;
            isChasing = false;
            hasStrafeDestination = false;
            return;
        }

        float distance = Vector3.Distance(transform.position, currentTarget.position);

        // handle repositioning timer
        if (isRepositioning)
        {
            repositionTimer -= Time.deltaTime;
            if (repositionTimer <= 0f)
                isRepositioning = false;

            // still allow the agent to finish the reposition path
            return;
        }

        // LOS cache at interval
        losCheckTimer -= Time.deltaTime;
        if (losCheckTimer <= 0f)
        {
            losCheckTimer = losCheckInterval;
            hasLineOfSightCached = CheckLineOfSight();
        }

        // SMART STOP / COMBAT ENTRY
        bool inAttackPosition = (distance <= stopChaseDistance && hasLineOfSightCached);

        // If in attack position, enter strafing/combat behaviour (if enabled).
        if (inAttackPosition && enableStrafe)
        {
            // Keep agent active so it can move to strafe destinations, but rotation locked to look at target
            StartChase(); // ensure agent isn't globally flagged as stopped
            HandleStrafe();
            LookAtTargetSmooth();
            return;
        }

        // If close but blocked, keep chasing (to get LOS)
        if (distance <= stopChaseDistance && !hasLineOfSightCached)
        {
            StartChase();
        }
        else if (distance > viewRange)
        {
            StartChase();
        }
        else
        {
            StartChase();
        }

        // PATH VALIDATION (controlled frequency)
        pathCheckTimer -= Time.deltaTime;
        bool targetMoved = Vector3.Distance(currentTarget.position, lastTargetPosition) > targetMoveThreshold;

        if (pathCheckTimer <= 0f || targetMoved)
        {
            pathCheckTimer = pathCheckInterval;
            lastTargetPosition = currentTarget.position;

            bool calc = NavMesh.CalculatePath(agent.transform.position, currentTarget.position, NavMesh.AllAreas, cachedPath);
            hasValidPath = calc && cachedPath.status == NavMeshPathStatus.PathComplete;
        }

        if (hasValidPath)
        {
            if (agent.isStopped)
                agent.isStopped = false;

            bool needToSetPath = !agent.hasPath || agent.isStopped || Vector3.Distance(agent.destination, currentTarget.position) > 0.5f;
            if (needToSetPath)
                agent.SetPath(cachedPath);
        }
        else
        {
            agent.ResetPath();
            agent.isStopped = true;
            hasStrafeDestination = false;
        }

        // When not in combat, ensure facing roughly movement direction if needed
        if (!inAttackPosition)
        {
            FaceSteeringDirection();
        }
    }

    // =========================
    // Strafe Logic
    // =========================
    private void HandleStrafe()
    {
        // If movement is paused externally, don't try to strafe
        if (agent.isStopped) return;

        // countdown timer for when to choose/refresh strafe target
        strafeTimer -= Time.deltaTime;
        if (strafeTimer > 0f && hasStrafeDestination)
        {
            // still moving to current strafe destination
            return;
        }

        // choose new strafe direction randomly or flip on obstacles
        // compute lateral direction relative to target:
        // lateral = cross(up, toTarget) -> points 'right' relative to direction from this->target
        tmpA = transform.position - currentTarget.position;
        tmpA.y = 0f;
        if (tmpA.sqrMagnitude < 0.001f)
        {
            // if we're almost at same position, pick arbitrary direction
            tmpA = transform.forward;
        }
        tmpA.Normalize();

        // right vector relative to target
        tmpB = Vector3.Cross(Vector3.up, tmpA).normalized; // this is "right" relative to direction to target

        // choose dir (randomize sometimes)
        if (Random.value < 0.3f) // 30% chance to flip direction occasionally
            currentStrafeDir *= -1;

        // try to sample a strafe candidate on chosen side
        Vector3 candidate = transform.position + tmpB * (strafeDistance * currentStrafeDir);

        // sample navmesh
        NavMeshHit sampleHit;
        bool sampled = NavMesh.SamplePosition(candidate, out sampleHit, strafeSampleRadius, NavMesh.AllAreas);

        if (!sampled)
        {
            // try opposite side before giving up
            currentStrafeDir *= -1;
            candidate = transform.position + tmpB * (strafeDistance * currentStrafeDir);
            sampled = NavMesh.SamplePosition(candidate, out sampleHit, strafeSampleRadius, NavMesh.AllAreas);
        }

        if (!sampled)
        {
            // cannot find valid strafe spot, cancel strafe this cycle and wait a bit
            hasStrafeDestination = false;
            strafeTimer = 0.15f; // short backoff
            return;
        }

        // Validate path to sampled point (avoid partial path)
        bool calc = NavMesh.CalculatePath(agent.transform.position, sampleHit.position, NavMesh.AllAreas, cachedStrafePath);
        if (!calc || cachedStrafePath.status != NavMeshPathStatus.PathComplete)
        {
            // try opposite direction once
            currentStrafeDir *= -1;
            candidate = transform.position + tmpB * (strafeDistance * currentStrafeDir);
            sampled = NavMesh.SamplePosition(candidate, out sampleHit, strafeSampleRadius, NavMesh.AllAreas);

            if (!sampled)
            {
                hasStrafeDestination = false;
                strafeTimer = 0.2f;
                return;
            }

            calc = NavMesh.CalculatePath(agent.transform.position, sampleHit.position, NavMesh.AllAreas, cachedStrafePath);
            if (!calc || cachedStrafePath.status != NavMeshPathStatus.PathComplete)
            {
                hasStrafeDestination = false;
                strafeTimer = 0.25f;
                return;
            }
        }

        // Good strafe point found and path is complete
        hasStrafeDestination = true;
        strafeTargetPos = sampleHit.position;
        agent.SetPath(cachedStrafePath);

        // set next change time (randomized a bit to avoid robotic timing)
        strafeTimer = strafeChangeInterval * Random.Range(0.8f, 1.3f);
    }

    // =========================
    // Chase Control
    // =========================
    private void StartChase()
    {
        if (isChasing) return;
        isChasing = true;
        agent.isStopped = false;
    }

    private void StopChase()
    {
        if (!isChasing) return;
        isChasing = false;
        agent.isStopped = true;
        agent.ResetPath();
        hasStrafeDestination = false;
    }

    // =========================
    // Circle Reposition (NavMesh Safe)
    // =========================
    public void Reposition(float duration = -1f)
    {
        if (currentTarget == null) return;

        Vector3 dir = (transform.position - currentTarget.position).normalized;
        Vector3 circlePoint = currentTarget.position + dir * strafeDistance;

        if (NavMesh.SamplePosition(circlePoint, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            agent.isStopped = false;
            agent.SetDestination(hit.position);
            isRepositioning = true;
            repositionTimer = (duration > 0f) ? duration : 1.2f;
        }
    }

    // =========================
    // Line of Sight helper (single raycast)
    // =========================
    private bool CheckLineOfSight()
    {
        if (currentTarget == null) return false;

        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 targetPos = currentTarget.position + Vector3.up * eyeHeight;

        Vector3 dir = targetPos - origin;
        float dist = dir.magnitude;
        if (dist <= 0.001f) return true;

        dir /= dist;
        return !Physics.Raycast(origin, dir, dist, obstacleMask);
    }

    // =========================
    // Look at target smoothly (used in combat)
    // =========================
    private void LookAtTargetSmooth()
    {
        if (currentTarget == null) return;

        Vector3 dir = currentTarget.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        // rotate at a fixed angular speed (deg/sec)
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, lookRotationSpeedDeg * Time.deltaTime);
    }

    // =========================
    // Facing when not in combat (keeps character facing movement)
    // =========================
    private void FaceSteeringDirection()
    {
        if (!agent.hasPath) return;

        Vector3 dir = agent.steeringTarget - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 12f * Time.deltaTime);
    }

    // =========================
    // Animator Sync
    // =========================
    private void UpdateAnimator()
    {
        Vector3 velocity = agent.velocity;
        Vector3 local = transform.InverseTransformDirection(velocity);

        float x = Mathf.Clamp(local.x / chaseSpeed, -1f, 1f);
        float z = Mathf.Clamp(local.z / chaseSpeed, -1f, 1f);

        anim.SetFloat(AnimX, x, 0.1f, Time.deltaTime);
        anim.SetFloat(AnimZ, z, 0.1f, Time.deltaTime);
    }

    // =========================
    // External Control
    // =========================
    public void PauseMovement()
    {
        agent.isStopped = true;
    }

    public void ResumeMovement()
    {
        agent.isStopped = false;
    }

    public bool IsChasing() => isChasing;
}
