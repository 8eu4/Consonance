using UnityEngine;
using System.Collections;

public class DestroyAfterTime : MonoBehaviour
{
    // You can set the time in the Inspector, or let the particle system handle its own destruction (see below)
    public float timeToLive = 2f;

    void Start()
    {
        // Destroy the effect object after a set duration
        Destroy(gameObject, timeToLive);
    }

    // Alternative for Particle Systems: 
    // In the Unity Editor, select your Particle System component,
    // uncheck "Looping", check "Play On Awake", and set "Stop Action" to "Destroy".
    // This script might not be necessary if configured correctly in the Editor.
}
