using UnityEngine;

public class LeverMovePlatform : MonoBehaviour, ILeverAction
{
    public Rigidbody platformRb;
    public Vector3 onPosition;
    public float moveSpeed = 3f;

    private Vector3 offPosition;
    private bool toggleState;

    private void Start()
    {
        if (platformRb == null)
            platformRb = GetComponent<Rigidbody>();

        platformRb.interpolation = RigidbodyInterpolation.Interpolate;
        platformRb.isKinematic = true;

        offPosition = platformRb.position;
    }

    public void OnLeverToggle(bool isOn)
    {
        toggleState = isOn;
        StopAllCoroutines();
        StartCoroutine(MovePlatform());
    }

    System.Collections.IEnumerator MovePlatform()
    {
        Vector3 targetPos = toggleState ? onPosition : offPosition;

        while (Vector3.Distance(platformRb.position, targetPos) > 0.01f)
        {
            Vector3 next = Vector3.MoveTowards(platformRb.position, targetPos, moveSpeed * Time.deltaTime);
            platformRb.MovePosition(next);
            yield return new WaitForFixedUpdate();  // PENTING!
        }

        platformRb.MovePosition(targetPos);
    }
}
