using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class CamRotation : MonoBehaviour
{
    public float sensX;
    public float sensY;

    Transform orientation;

    float xRotation;
    float yRotation;

    GameObject Player;
    private bool isAttackLocked = false;

    [Header("Lock-On Settings")]
    [SerializeField] private float lockOnSpeed = 8f;
    private Transform lockOnTarget = null;

    [Header("References")]
    [SerializeField] private SwitchCharacter switchCharacterScript;
    
    
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        orientation = Player.transform.Find("Orientation").transform;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }


    void Update()
    {
        if (Player == null || orientation == null) return; // Keamanan

        if (isAttackLocked && lockOnTarget != null)
        {
            Vector3 dirToTarget = (lockOnTarget.position - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(dirToTarget);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * lockOnSpeed);

            Vector3 currentEuler = transform.rotation.eulerAngles;
            yRotation = currentEuler.y;
            xRotation = currentEuler.x;
            if (xRotation > 180f) xRotation -= 360f;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        }
        else
        {
            float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * sensX;
            float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * sensY;

            yRotation += mouseX;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        }

        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
        Player.transform.GetChild(0).rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public void SetLockOnTarget(Transform target)
    {
        lockOnTarget = target;
    }

    public void UpdateOrientation()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        orientation = Player.transform.Find("Orientation").transform;
        yRotation = orientation.eulerAngles.y;

    }
    public void LockLookAt(Vector3 targetPoint)
    {
        transform.LookAt(targetPoint);

        Vector3 currentEuler = transform.rotation.eulerAngles;

        float newX = currentEuler.x;
        if (newX > 180f)
        {
            newX -= 360f; 
        }

        xRotation = Mathf.Clamp(newX, -90f, 90f);
        yRotation = currentEuler.y;

        if (orientation != null)
            orientation.rotation = Quaternion.Euler(0, yRotation, 0);
        if (Player != null)
            Player.transform.GetChild(0).rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public bool IsAttackLocked
    {
        get{ return isAttackLocked; }
        set{ isAttackLocked = value; }
        
    }
}
