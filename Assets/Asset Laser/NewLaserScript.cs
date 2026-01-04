using UnityEngine;

public class NewLaserScript : MonoBehaviour
{
    private LineRenderer lr;
    [SerializeField] private Transform startPoint;

    void Start()
    {
        lr = GetComponent<LineRenderer>();
    }

    void Update()
    {
        lr.SetPosition(0, startPoint.position);

        RaycastHit hit;

        if (Physics.Raycast(transform.position, -transform.right, out hit))
        {
            if (hit.collider)
            {
                lr.SetPosition(1, hit.point);

                string tagKena = hit.transform.tag;

                if (tagKena == "Player" || 
                    tagKena == "Remi" || 
                    tagKena == "Domi" || 
                    tagKena == "Conductor")
                {
                    // --- UBAH PANGGILAN DI SINI ---
                    // Panggil RespawnLaser (bukan Manager lagi)
                    if (RespawnLaser.instance != null)
                    {
                        RespawnLaser.instance.KillAndRespawn(hit.transform.gameObject);
                    }
                    else
                    {
                        hit.transform.gameObject.SetActive(false);
                    }
                    // ------------------------------
                }
            }
        }
        else
        {
            lr.SetPosition(1, transform.position - transform.right * 5000);
        }
    }
}