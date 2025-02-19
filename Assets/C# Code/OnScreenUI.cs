using UnityEngine;
using UnityEngine.UI;

public class OnScreenUI : MonoBehaviour
{
    public Rigidbody roverRigidbody;
    public TMPro.TextMeshProUGUI speedText;

    public TMPro.TextMeshProUGUI coords;


    void Update()
    {
        UpdateCoords();
    }

    void FixedUpdate(){
         UpdateSpeed();
    }

     void UpdateSpeed()
    {
       Vector3 flatVelocity = Vector3.ProjectOnPlane(roverRigidbody.linearVelocity, Vector3.up);
            float horizontalSpeed = flatVelocity.magnitude;
            float speedInKmH = horizontalSpeed * 3.6f;


            // MaxSpeed Perseverance: 0.09 mph

            /*if (speedInKmH > 1 && speedInKmH < 50){
                speedInKmH = 0.10;
            }
            else{
                speedInKmH = 0.14;
            }
            */
            

            speedText.text = "Average Speed: " + speedInKmH + " km/h";
    }

    void UpdateCoords(){
      Vector3 pos = roverRigidbody.position;
     
        coords.text = "X: " + pos.x.ToString("F2") + 
                        " Y: " + pos.y.ToString("F2") + 
                        " Z: " + pos.z.ToString("F2");
        
    }




}
