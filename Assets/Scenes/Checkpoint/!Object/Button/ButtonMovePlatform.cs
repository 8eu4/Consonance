using UnityEngine;

public class ButtonMovePlatform : MonoBehaviour, IButtonAction
{
    public Rigidbody platformRb;
    public Vector3 onPosition;
    public float moveSpeed = 3f;

    private Vector3 offPosition;

    private void Start()
    {
        if (platformRb == null)
            platformRb = GetComponent<Rigidbody>();

        platformRb.isKinematic = true;
        platformRb.interpolation = RigidbodyInterpolation.Interpolate;

        offPosition = platformRb.position;
    }

    public void OnButtonPressed()
    {
        StopAllCoroutines();
        StartCoroutine(MoveTo(onPosition));
    }

    public void OnButtonReleased()
    {
        StopAllCoroutines();
        StartCoroutine(MoveTo(offPosition));
    }

    System.Collections.IEnumerator MoveTo(Vector3 target)
    {
        while (Vector3.Distance(platformRb.position, target) > 0.01f)
        {
            Vector3 next = Vector3.MoveTowards(platformRb.position, target, moveSpeed * Time.deltaTime);
            platformRb.MovePosition(next);
            yield return new WaitForFixedUpdate();
        }

        platformRb.MovePosition(target);
    }
}
