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
        // Set titik awal laser
        lr.SetPosition(0, startPoint.position);

        RaycastHit hit;

        // Tembak Raycast
        if (Physics.Raycast(transform.position, -transform.right, out hit))
        {
            if (hit.collider)
            {
                // 1. Visual: Laser mentok di objek
                lr.SetPosition(1, hit.point);

                // 2. Logic: Cek Tag (Player ATAU Remi ATAU Domi ATAU Conductor)
                string tagKena = hit.transform.tag;

                if (tagKena == "Player" || 
                    tagKena == "Remi" || 
                    tagKena == "Domi" || 
                    tagKena == "Conductor")
                {
                    Debug.Log("Membunuh: " + tagKena);
                    Destroy(hit.transform.gameObject);
                    
                    // TIPS: Kalau kamu punya sistem Respawn (bukan destroy),
                    // Panggil script respawn-nya di sini, jangan Destroy.
                    // Contoh: hit.transform.GetComponent<HealthSystem>().Die();
                }
            }
        }
        else
        {
            // Jika tidak kena apa-apa, laser tembus panjang
            lr.SetPosition(1, transform.position - transform.right * 5000);
        }
    }
}