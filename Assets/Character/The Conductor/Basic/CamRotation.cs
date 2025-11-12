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

    private bool doLerp = false;
    private Vector3 dirToTarget;

    [Space]
    [SerializeField] private float slowTimeScale = 0.2f;


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

        // slow time
        Time.timeScale = slowTimeScale;
        //also slow the script debug console



        if (Player == null || orientation == null) return; // Keamanan

        if (isAttackLocked && lockOnTarget != null)
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
            else transform.rotation = targetRotation;

            Vector3 currentEuler = transform.rotation.eulerAngles;
            yRotation = currentEuler.y;
            xRotation = currentEuler.x;
            if (xRotation > 180f) xRotation -= 360f;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        }
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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            //print("target rotation: " + targetRotation);
            //print("lockOnTarget.position - transform.position: " + (lockOnTarget.position - transform.position));
            //print("transform rotation: " + transform.rotation);
            //print("dirToTarget: " + dirToTarget);
            print("dolerp: " + doLerp);
            //print("smoothLockOnSpeed: " + smoothLockOnSpeed);
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
        get { return isAttackLocked; }
        set { isAttackLocked = value; }

    }
}
