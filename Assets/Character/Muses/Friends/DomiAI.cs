using UnityEngine;

public class DomiAI : MonoBehaviour
{
    private Transform target;

    public void SetTarget(Transform player)
    {
        target = player;
    }

    public void RemoveTarget()
    {
        target = null;
    }

    void Update()
    {
        if (target != null)
        {
            transform.LookAt(target);
            transform.position = Vector3.MoveTowards(transform.position, target.position, 2f * Time.deltaTime);
        }
    }
}
