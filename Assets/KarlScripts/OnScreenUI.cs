using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class OnScreenUI : MonoBehaviour
{
    public Rigidbody roverRigidbody;
    public TMPro.TextMeshProUGUI speedText;
    public TMPro.TextMeshProUGUI coords;
    public TMPro.TextMeshProUGUI environmentalInfoText;
    public TMPro.TextMeshProUGUI weather;
    public TMPro.TextMeshProUGUI weatherTypeText;
    public Slider updateSlider; 
    private RoverController rc;
    private SimulationController sm;
    private float currentTemperature;
    private float targetTemperature;
    private float minTemp;
    private float maxTemp;
    private float fluctuationRange = 0.1f;
    private float timeSinceLastUpdate = 0f;
    private string[] marsWeatherTypes = {
    "Sunny",
    "Cloudy",
    "Dust Storm",
    "Clear",
    "Windy",
    "Hazy"
};
    public float atmospheric_density;
    public float atmospheric_pressure;
    public float co2_concentration;
    public float nitrogen_concentration;
    public float argon_concentration;


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

        string randomWeather = marsWeatherTypes[Random.Range(0, marsWeatherTypes.Length)];
        weatherTypeText.text = randomWeather;

        string info = 
        "Atmospheric Density: 0.000 kg/m³\n" +  
        "Atmospheric Pressure: 0.00 kPa\n" + 
        "CO2 Concentration: 0.00 %\n" +  
        "Nitrogen Concentration: 0.00 %\n" + 
        "Argon Concentration: 0.00 %"; 

        environmentalInfoText.text = info;


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


          atmospheric_density = dm.EnvironmentSettings.environment.atmosphericDensity;
          atmospheric_pressure = dm.EnvironmentSettings.environment.atmosphericPressure;
          co2_concentration = dm.EnvironmentSettings.environment.co2Concentration;
          nitrogen_concentration = dm.EnvironmentSettings.environment.nitrogenConcentration;
          argon_concentration = dm.EnvironmentSettings.environment.argonConcentration;

        currentTemperature = Random.Range(minTemp, maxTemp);
        targetTemperature = Random.Range(minTemp, maxTemp);
    }

    void Update()
    {
       timeSinceLastUpdate += Time.deltaTime;
        
        if (timeSinceLastUpdate >= 1f)  {
            UpdateTemp();
            string info = 
            "Atmospheric Density: " + atmospheric_density.ToString("F3") + " kg/m³\n" +  
            "Atmospheric Pressure: " + atmospheric_pressure.ToString("F2") + " kPa\n" +  
            "CO2 Concentration: " + (co2_concentration * 100).ToString("F2") + " %\n" +  
            "Nitrogen Concentration: " + (nitrogen_concentration * 100).ToString("F2") + " %\n" +  
            "Argon Concentration: " + (argon_concentration * 100).ToString("F2") + " %";  

            environmentalInfoText.text = info;
            timeSinceLastUpdate = 0f;  
        }
        UpdateCoords();



        // FOR WEATHER TYPE
        if (Time.frameCount % 900 == 0)
        {
            string randomWeather = marsWeatherTypes[Random.Range(0, marsWeatherTypes.Length)];
            weatherTypeText.text = randomWeather;
        }

        // FOR ENV ATMOSPHERIC CONTENT
        
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
            
            if (speedInKmH < 0.1f)
            {
                speedInKmH = 0.0f;
            }

            speedText.text = "Average Speed: " + (Mathf.Round(speedInKmH * 100f) / 100f) + " m/s";

           
    }

    void UpdateTemp()
    {
       float fluctuation = Random.Range(-fluctuationRange, fluctuationRange);

    
        currentTemperature = Mathf.Clamp(currentTemperature + fluctuation, minTemp, maxTemp);
        weather.text = Mathf.Round(currentTemperature) + "°C";
    }

    void UpdateCoords(){
      Vector3 pos = roverRigidbody.position;
     
        coords.text = "X: " + pos.x.ToString("F2") + 
                        " Y: " + pos.y.ToString("F2") + 
                        " Z: " + pos.z.ToString("F2");

        
        
    }

    void EnvAirComposition(DataManager dm){
        if (dm.EnvironmentSettings == null || dm.EnvironmentSettings.environment == null)
        {
            return;
        }
         float atmospheric_density = dm.EnvironmentSettings.environment.atmosphericDensity;
         float atmospheric_pressure = dm.EnvironmentSettings.environment.atmosphericPressure;
         float co2_concentration = dm.EnvironmentSettings.environment.co2Concentration;
         float nitrogen_concentration = dm.EnvironmentSettings.environment.nitrogenConcentration;
         float argon_concentration = dm.EnvironmentSettings.environment.argonConcentration;
    }
}



    



    




