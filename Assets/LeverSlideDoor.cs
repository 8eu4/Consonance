using UnityEngine;
using System.Collections;

public class LeverSlideDoor : MonoBehaviour, ILeverAction
{
    [Header("Door Settings")]
    public Transform door;
    public Vector3 openOffset = new Vector3(3f, 0f, 0f);
    public float moveDuration = 1f;

    private Vector3 closedPos;
    private Vector3 openPos;
    private Coroutine moveRoutine;

    private void Start()
    {
        if (door != null)
        {
            closedPos = door.position;
            openPos = closedPos + openOffset;
        }
    }

    public void OnLeverToggle(bool isOn)
    {
        if (door == null) return;

        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(MoveDoor(isOn));
    }

    IEnumerator MoveDoor(bool open)
    {
        Vector3 start = door.position;
        Vector3 target = open ? openPos : closedPos;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / moveDuration;
            door.position = Vector3.Lerp(start, target, t);
            yield return null;
        }

        door.position = target;
    }
}
