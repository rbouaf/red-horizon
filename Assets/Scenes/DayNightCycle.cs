using UnityEngine;

public class DayNightCycle : MonoBehaviour {

    // Reference to the directional light (Sun)
    public Light sunLight;

    // Minimum and maximum intensity for the sun
    public float minSunIntensity = 0.1f;
    public float maxSunIntensity = 1.0f;

    // Duration of a full day in seconds 
    public float fullDayDuration = 88775f;

    

    private float timeOfDay = 0f;  // Time of day in degrees (0-360)

    void Start()
    {
        if (sunLight == null)
        {
            sunLight = GetComponent<Light>();
        }
    }

    void Update()
    {
        // Get time speedup factor and apply it to the day/night cycle
        float timeScale = 1f;

        // Speed of the day/night cycle (in degrees per second)
        float dayCycleSpeed = 360f / fullDayDuration * timeScale;

        // Increment the time of day based on the speed
        timeOfDay += dayCycleSpeed * Time.deltaTime;

        // Ensure timeOfDay stays within 0-360 degrees
        timeOfDay %= 360f;

        // Rotate the sun around the X-axis (simulating the day/night cycle)
        transform.rotation = Quaternion.Euler(new Vector3((timeOfDay / 360f) * 360f, 0, 0));

        // Adjust the sun's intensity based on the time of day
        float normalizedTime = Mathf.Sin(Mathf.Deg2Rad * timeOfDay);
        sunLight.intensity = Mathf.Lerp(minSunIntensity, maxSunIntensity, normalizedTime);

    }
}