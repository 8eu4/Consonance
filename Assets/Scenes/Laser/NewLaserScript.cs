using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewLaserScript : MonoBehaviour
{
    private LineRenderer lr;

    [SerializeField]
    private Transform startPoint;

    void Start()
    {
        lr = GetComponent<LineRenderer>();
    }

    void Update()
    {
        // Titik awal laser
        lr.SetPosition(0, startPoint.position);

        RaycastHit hit;

        // Raycast ke arah kiri lokal
        if (Physics.Raycast(transform.position, -transform.right, out hit))
        {
            if (hit.collider)
            {
                // Jika sinar mengenai sesuatu, set ujung laser di titik tabrakan
                lr.SetPosition(1, hit.point);

                // Jika objek yang terkena memiliki tag "Player", hancurkan objek tersebut
                if (hit.transform.tag == "Player")
                {
                    Destroy(hit.transform.gameObject);
                }
            }
        }
        else
        {
            // Jika tidak mengenai apa pun, buat laser tetap panjang (misal 5000 satuan)
            lr.SetPosition(1, transform.position - transform.right * 5000);
        }

        if (hit.transform.CompareTag("Player"))
{
    Debug.Log("Laser hit Player!");
    Destroy(hit.transform.gameObject);
}

    }
}
