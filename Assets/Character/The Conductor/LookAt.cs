using UnityEngine;

public class LookAt : MonoBehaviour
{

    public float radius = 20;
    private bool isHit = false;
    public LineRenderer lineRenderer;

    public LayerMask AcceptableHit;

    void Start()
    {
        
    }

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, radius, AcceptableHit))
        {
            isHit = true;
            print("hit: " + hitInfo.collider.name);
            Debug.DrawLine(ray.origin, hitInfo.point, Color.red);

            lineRenderer.enabled = true;

            Vector3 origin = ray.origin;
            origin.y -= 0.2f;

            // Ray hit something
            lineRenderer.SetPosition(0, origin);
            lineRenderer.SetPosition(1, hitInfo.point);

        }
        else
        {
            isHit = false;
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * 1000, Color.green);

            lineRenderer.enabled = false;
        }
    }

    public bool getIsHit()
    {
        return isHit;
    }
}
