using UnityEngine;

public class RedDot : MonoBehaviour
{

    public Transform rover;
    public float offsetY = 110f;  

    void LateUpdate()
    {
        if (rover == null) return;

       transform.position = new Vector3(rover.position.x, offsetY, rover.position.z); 
    }
    
}
