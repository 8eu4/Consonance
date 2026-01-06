using UnityEngine;

public class Cam : MonoBehaviour
{
    [SerializeField] private Transform condCameraPosition;
    [SerializeField] private SwitchCharacter switchCharacterScript;

    void LateUpdate()
    {
        transform.position = condCameraPosition.position;
    }
    public void setCameraPosition()
    {
        // GetChild(2) selalu component CameraPos
        GameObject Player = GameObject.FindGameObjectWithTag("Player");
        condCameraPosition = Player.transform.GetChild(2).transform;
    }
}
