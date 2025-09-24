using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class outputTextUI : MonoBehaviour
{
    public TextMeshProUGUI outputText;
    public Rigidbody rb;
    
    void Update()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        outputText.text = "speed: " + Mathf.Round(flatVel.magnitude).ToString();
        
    }
}
