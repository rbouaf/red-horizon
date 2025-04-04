using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class OnScreenUI : MonoBehaviour
{
    public Rigidbody roverRigidbody;
    public TMPro.TextMeshProUGUI speedText;
    public TMPro.TextMeshProUGUI coords;
    public TMPro.TextMeshProUGUI weather;

    public Slider updateSlider; 

    private RoverController rc;
    private SimulationController sm;

    private float currentTemperature;
    private float targetTemperature;
    private float minTemp;
    private float maxTemp;

    private float fluctuationRange = 0.1f;
    private float timeSinceLastUpdate = 0f;

    private void Start()
    {
        rc = GetComponent<RoverController>();
        sm = FindAnyObjectByType<SimulationController>();

        if (sm == null)
        {
            StartCoroutine(WaitForSimulationController());
            return;
        }

        StartCoroutine(WaitForDataManager());


         if (updateSlider != null)
        {
            updateSlider.onValueChanged.AddListener(OnSliderValueChanged);
        }
    }

    private IEnumerator WaitForSimulationController()
    {
        while (sm == null)
        {
            sm = FindAnyObjectByType<SimulationController>();
            yield return new WaitForSeconds(0.1f);
        }

        StartCoroutine(WaitForDataManager());
    }

    private IEnumerator WaitForDataManager()
    {
        DataManager dm = null;

        while (dm == null)
        {
            dm = sm.GetDataManager();
            yield return new WaitForSeconds(0.1f);
        }

        initializeTemp(dm);
    }

      void OnSliderValueChanged(float value)
    { 
        fluctuationRange = Mathf.Lerp(0.1f, 5f, (value - 1) / 19f);
      
    }

    void initializeTemp(DataManager dm)
    {
        if (dm.EnvironmentSettings == null || dm.EnvironmentSettings.environment == null)
        {
            return;
        }

        minTemp = dm.EnvironmentSettings.environment.temperatureNight;
        maxTemp = dm.EnvironmentSettings.environment.temperatureDay;


        currentTemperature = Random.Range(minTemp, maxTemp);
        targetTemperature = Random.Range(minTemp, maxTemp);
    }

    void Update()
    {
       timeSinceLastUpdate += Time.deltaTime;
        
        if (timeSinceLastUpdate >= 1f)  {
            UpdateTemp();
            timeSinceLastUpdate = 0f;  
        }
        UpdateCoords();
    }

    void FixedUpdate()
    {
        UpdateSpeed();
    }

    void UpdateSpeed()
    {
       Vector3 flatVelocity = Vector3.ProjectOnPlane(roverRigidbody.linearVelocity, Vector3.up);
            float horizontalSpeed = flatVelocity.magnitude;
            float speedInKmH = horizontalSpeed;
            
  


            speedText.text = "Average Speed: " + (Mathf.Round(speedInKmH * 100f) / 100f) + " m/s";

           
    }

    void UpdateTemp()
    {
       float fluctuation = Random.Range(-fluctuationRange, fluctuationRange);

    
        currentTemperature = Mathf.Clamp(currentTemperature + fluctuation, minTemp, maxTemp);
        weather.text = "Temperature:" + Mathf.Round(currentTemperature) + "°C";
    }

    void UpdateCoords(){
      Vector3 pos = roverRigidbody.position;
     
        coords.text = "X: " + pos.x.ToString("F2") + 
                        " Y: " + pos.y.ToString("F2") + 
                        " Z: " + pos.z.ToString("F2");

        
        
    }
}



    



    




