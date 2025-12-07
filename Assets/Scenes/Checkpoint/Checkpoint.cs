using UnityEngine;

[RequireComponent(typeof(Transform))]
public class Checkpoint : MonoBehaviour
{
    [Header("Activation")]
    public float activationRadius = 2.0f;
    public bool autoActivate = true; // jika true: cukup masuk radius -> auto aktif
    public KeyCode interactKey = KeyCode.E; // jika autoActivate == false, tekan E untuk activate

    [Header("Optional")]
    public string checkpointName = "";

    Transform playerT;
    bool activated = false;

    void Start()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerT = playerObj.transform;
    }

    void Update()
    {
        if (playerT == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerT = p.transform;
            else return;
        }

        float dist = Vector3.Distance(playerT.position, transform.position);
        if (!activated && dist <= activationRadius)
        {
            if (autoActivate)
            {
                Debug.Log("Hi");
                ActivateCheckpoint();
            }
            else
            {
                // wait for key press
                if (Input.GetKeyDown(interactKey))
                {
                    Debug.Log("Hi");
                    ActivateCheckpoint();
                }
            }
        }
    }

    private void ActivateCheckpoint()
    {
        activated = true;
        if (RespawnManager.Instance != null)
        {
            RespawnManager.Instance.RegisterCheckpoint(transform);
        }
        // optional feedback
        Debug.Log("[Checkpoint] Activated: " + (string.IsNullOrEmpty(checkpointName) ? name : checkpointName));
        // you can add visual/sound feedback here (e.g., play animation)
    }

    // Draw radius in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationRadius);
    }
}
