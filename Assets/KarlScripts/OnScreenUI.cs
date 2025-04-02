using UnityEngine;
using UnityEngine.UI;
//using UnitySystem;

public class OnScreenUI : MonoBehaviour
{
    public Rigidbody roverRigidbody;
    public TMPro.TextMeshProUGUI speedText;

    public TMPro.TextMeshProUGUI coords;

    private RoverController rc;
    private SimulationController sm;

    private void Start(){
      rc = GetComponent<RoverController>();
      sm = FindAnyObjectByType<SimulationController>();
    }

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
            float speedInKmH = horizontalSpeed;
            
            //* 3.6f;




            speedText.text = "Average Speed: " + (Mathf.Round(speedInKmH * 100f) / 100f) + " m/s";

            DataManager dm = sm.GetDataManager();
            Debug.Log($"Temparature Day: {dm.EnvironmentSettings.environment.temperatureDay}"); // Example of accessing rover data
    }




    void UpdateCoords(){
      Vector3 pos = roverRigidbody.position;
     
        coords.text = "X: " + pos.x.ToString("F2") + 
                        " Y: " + pos.y.ToString("F2") + 
                        " Z: " + pos.z.ToString("F2");

        
        
    }

    /*
  
  void getTemp(){
      sm
    }




    */




}
