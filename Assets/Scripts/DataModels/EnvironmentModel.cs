using UnityEngine;

[CreateAssetMenu(fileName = "EnvironmentData", menuName = "ScriptableObjects/Rover")]
public class EnvironmentModel : ScriptableObject
{
    public float gravity;
    public float atmosphericDensity;  
    public float atmosphericPressure;
    public float cO2Concentration;
    public float nitrogenConcentration;
    public float argonConcentration;
    
    // Temperature ranges (°C)
    public float temperatureDay;             
    public float temperatureNight;
    public float temperatureVariation;
    
    // Day-night cycle
    public float dayNightCycleDuration;
    public float sunriseTime;
    public float sunsetTime;
    
    // Weather conditions
    public float windSpeed;         
    public float windGustProbability;             
    public float windGustMultiplier;
    public float dustStormProbability;          
    public float dustStormDurationMinutes;
    public float dustStormDurationMultiplier;
    public float dustStormIntensity;    
}