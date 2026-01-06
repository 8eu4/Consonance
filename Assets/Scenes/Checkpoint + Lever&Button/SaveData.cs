using System;
using UnityEngine;

[Serializable]
public class SaveData
{
    public string sceneName;
    public Vector3 position;
    public Quaternion rotation;
    public string questMessage;
    public int checkpointIndex = -1;

    public SaveData() { }

    public SaveData(string sceneName, Vector3 pos, Quaternion rot, string questMessage, int checkpointIndex)
    {
        this.sceneName = sceneName;
        this.position = pos;
        this.rotation = rot;
        this.questMessage = questMessage;
        this.checkpointIndex = checkpointIndex;
    }
}
