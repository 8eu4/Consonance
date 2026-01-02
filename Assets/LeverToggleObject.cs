using UnityEngine;

public class LeverToggleObject : MonoBehaviour, ILeverAction
{
    public GameObject targetObject;

    public void OnLeverToggle(bool isOn)
    {
        if (targetObject == null) return;

        targetObject.SetActive(isOn);
    }
}
