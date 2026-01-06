using UnityEngine;

public class NearestTargetDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    public float viewRange = 12f;
    public string[] targetTags = { "Player", "Conductor", "Remi", "Domi" };
    public float scanInterval = 0.3f;

    [Header("Debug")]
    public Transform currentTarget;

    private float scanTimer;

    void Update()
    {
        scanTimer -= Time.deltaTime;
        if (scanTimer <= 0f)
        {
            currentTarget = FindNearestTarget();
            scanTimer = scanInterval;
        }
    }

    /// <summary>
    /// Cari target terdekat berdasarkan tag & viewRange
    /// </summary>
    public Transform FindNearestTarget()
    {
        float closestDist = float.MaxValue;
        Transform closest = null;

        foreach (string tag in targetTags)
        {
            GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject obj in objects)
            {
                float dist = Vector3.Distance(transform.position, obj.transform.position);
                if (dist <= viewRange && dist < closestDist)
                {
                    closestDist = dist;
                    closest = obj.transform;
                }
            }
        }

        return closest;
    }

    /// <summary>
    /// Helper cepat: cek apakah ada target valid
    /// </summary>
    public bool HasTarget()
    {
        return currentTarget != null;
    }
}
