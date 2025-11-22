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

        // Tembakkan Raycast
        if (Physics.Raycast(transform.position, -transform.right, out hit))
        {
            // 1. Jika kena sesuatu, ujung laser berhenti di titik tabrakan
            lr.SetPosition(1, hit.point);

            // 2. Cek apakah objek tersebut Player ATAU NPC
            // Gunakan operator || (OR) untuk mengecek kedua kondisi
            if (hit.transform.CompareTag("Player") || hit.transform.CompareTag("NPC"))
            {
                Debug.Log("Laser membunuh: " + hit.transform.name);
                Destroy(hit.transform.gameObject);
            }
        }
        else
        {
            // Jika tidak mengenai apa pun, laser memanjang jauh
            lr.SetPosition(1, transform.position - transform.right * 5000);
        }
    }
}