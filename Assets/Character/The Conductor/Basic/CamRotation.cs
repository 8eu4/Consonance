using Unity.VisualScripting;
using UnityEngine;

public class CamRotation : MonoBehaviour
{
    GameObject player;

    public float sensX;
    public float sensY;

    Transform orientation;

    float xRotation;
    float yRotation;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        orientation = player.transform.Find("Orientation").transform;
    }


    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * sensY;
        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}
