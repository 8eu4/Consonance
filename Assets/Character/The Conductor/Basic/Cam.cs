using UnityEngine;

public class Cam : MonoBehaviour
{
    [SerializeField] private Transform cameraPosition;
    [SerializeField] private SwitchCharacter switchCharacterScript;

    void LateUpdate()
    {
        transform.position = cameraPosition.position;
    }
    public void setCameraPosition()
    {
        // GetChild(2) selalu component CameraPos
        GameObject Player = GameObject.FindGameObjectWithTag("Player");
        cameraPosition = Player.transform.GetChild(2).transform;
    }
}
