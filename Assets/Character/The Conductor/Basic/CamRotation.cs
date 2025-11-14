using NUnit.Framework;
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
    [SerializeField] private float smoothLockOnSpeed = 1f;
    private Transform lockOnTarget = null;

    [Header("References")]
    [SerializeField] private SwitchCharacter switchCharacterScript;
    [SerializeField] private StringLineAttack[] StringLineAttackScript;
    [SerializeField] private Transform Conductor;
    [SerializeField] private Transform Domi;
    [SerializeField] private Transform Remi;

    private bool DomiLineIsAttached = false;
    private bool RemiLineIsAttached = false;
    private Quaternion DomiRotation;
    private Quaternion RemiRotation;


    private bool doLerp = false;
    private Vector3 dirToTarget;



    private Quaternion targetRotation;
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        orientation = Player.transform.Find("Orientation").transform;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        targetRotation = transform.rotation;
    }

    void LateUpdate()
    {
        if (Player == null || orientation == null) return;

        if (isAttackLocked && lockOnTarget != null && switchCharacterScript.CurrentPlayer == Conductor)
        {
            dirToTarget = (lockOnTarget.position - transform.position).normalized;
            targetRotation = Quaternion.LookRotation(dirToTarget);

            if (doLerp)
            {
                smoothLockOnSpeed += Time.deltaTime * 100f;
                float angleDifference = Quaternion.Angle(transform.rotation, targetRotation);
                if (angleDifference < 0.5f) doLerp = false;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * smoothLockOnSpeed);
            }
            else
            {
                transform.rotation = targetRotation;
            }

            Vector3 currentEuler = transform.rotation.eulerAngles;
            yRotation = currentEuler.y;
            xRotation = currentEuler.x;
            if (xRotation > 180f) xRotation -= 360f;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        }

        else if (switchCharacterScript.CurrentPlayer == Domi && DomiLineIsAttached)
        {
            transform.rotation = DomiRotation;

            Vector3 currentEuler = transform.rotation.eulerAngles;
            yRotation = currentEuler.y;
            xRotation = currentEuler.x;
            if (xRotation > 180f) xRotation -= 360f;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        }

        else if (switchCharacterScript.CurrentPlayer == Remi && RemiLineIsAttached)
        {
            transform.rotation = RemiRotation;

            Vector3 currentEuler = transform.rotation.eulerAngles;
            yRotation = currentEuler.y;
            xRotation = currentEuler.x;
            if (xRotation > 180f) xRotation -= 360f;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        }
        // free look
        else
        {
            doLerp = true;
            smoothLockOnSpeed = 1f;

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
    public void LockLookAt(Vector3 targetPoint, GameObject character)
    {
        transform.LookAt(targetPoint);
        Vector3 currentEuler = transform.rotation.eulerAngles;

        float newX = currentEuler.x;
        if (newX > 180f) newX -= 360f;

        xRotation = Mathf.Clamp(newX, -90f, 90f);
        yRotation = currentEuler.y;

        if (Domi.gameObject == character && !DomiLineIsAttached) // Domi
        {
            DomiLineIsAttached = true;
            DomiRotation = transform.rotation;
        }
        else if (Remi.gameObject == character && !RemiLineIsAttached) // Remi
        {
            RemiLineIsAttached = true;
            RemiRotation = transform.rotation;
        }

        if (orientation != null)
            orientation.rotation = Quaternion.Euler(0, yRotation, 0);
        if (Player != null)
            Player.transform.GetChild(0).rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public void CancelLineAttack(GameObject character)
    {
        if (Domi.gameObject == character)
        {
            DomiLineIsAttached = false;
        }
        else if (Remi.gameObject == character)
        {
            RemiLineIsAttached = false;
        }
    }

    public bool IsAttackLocked
    {
        get { return isAttackLocked; }
        set { isAttackLocked = value; }

    }
}
