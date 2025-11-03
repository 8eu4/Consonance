using UnityEngine;

public class ConductorHP : MonoBehaviour
{
    private int _HP = 4;
    private bool _onHit = false;

    private void OnCollisionEnter(Collision collision)
    {
        
    }
    public bool onHit
    {
        get { return _onHit; }
    }
    public int HP
    {
        get { return _HP; }
        set { _HP = value; }
    }
    

}
