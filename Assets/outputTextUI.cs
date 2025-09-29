using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class outputTextUI : MonoBehaviour
{
    public TextMeshProUGUI outputText;
    public Rigidbody rb;
    
    private LookAt lookAtScript;

    void Start()
    {
        lookAtScript = GameObject.FindGameObjectWithTag("CameraHolder").transform.GetChild(0).GetComponent<LookAt>();
        
    }

    void Update()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        outputText.text = "speed: " + Mathf.Round(flatVel.magnitude).ToString() + "\nHit: " + lookAtScript.getIsHit();
        
    }
}
