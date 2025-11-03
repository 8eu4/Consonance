using UnityEngine;

public class Cam : MonoBehaviour
{
    [SerializeField] private Transform cameraPosition;
    [SerializeField] private SwitchCharacter switchCharacterScript;

    private int activeCharacterIndex;

    void Start()
    {
        activeCharacterIndex = switchCharacterScript.getActiveCharacterIndex();
    }

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
