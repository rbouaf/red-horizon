using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoverController : MonoBehaviour
{
    private string roverId = "001";
    public RoverModel roverModel;
    public List<WheelController> driveWheels;
    public List<WheelController> steeringWheels;

    public float maxHorsepower = 10f;
    public float maxTorquePerWheel = 669.77f;
    public float maxSteerAngle = 30f;
    public float brakingMultiplier = 5f;

    [Header("Speed Slider")]
    public SpeedSlider speedSlider;  // Reference to Speed Slider script
    private float normalSpeed = 1f;

    [Header("Battery System")]
    public float batteryLevel = 100f;  // Start at 100%
    public float drainRate = 0.01f;  // Drain per second
    public float boostDrainMultiplier = 3f;  // Extra drain when boosting

    public TMPro.TextMeshProUGUI batteryText;  // Reference to UI Text

    [Header("Solar Battery Charging")]
    public float solarChargeRate = 1.0f; // Charge per second
    public float dustRate = 0.01f; //Decrease in solarChargeRate per second
    public Button brushButton; // Assigned in Inspector

    public Light sunLight;  

    public TMPro.TextMeshProUGUI chargingStatusText;

    [Header("Center of Mass")]
    public Vector3 centerOfMassOffset = new Vector3(0, -0.5f, 0);
    private float maxPowerWatts;
    private Rigidbody rb;

    [Header("Audio")]

    public AudioSource movementAudio;

    public float fadeSpeed = 0.1f; // How fast to fade in/out

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        SimulationController simController = GetComponent<SimulationController>();
        if (simController == null)
        {
            Debug.LogError("SimulationController not found!");
            return;
        }
        roverModel = simController.GetRoverModelById(roverId);
        if (roverModel == null)
        {
            Debug.LogError("Rover model not found!");
            return;
        }
        
        maxTorquePerWheel = roverModel.systems.mobility.wheelTorque;

        // Set center of mass for better stability
        if (rb != null)
        {
            rb.centerOfMass = centerOfMassOffset;
            Debug.Log("Center of mass set to: " + rb.centerOfMass);
        }

        // Set up power calculations
        maxPowerWatts = HorsepowerToWatts(maxHorsepower);
        foreach (WheelController wheel in driveWheels)
        {
            wheel.maxPower = HorsepowerToWatts(wheel.horsepower);
            wheel.maxTorque = maxTorquePerWheel;
        }

        //Connect brush button to CleanPanels function
        brushButton.onClick.AddListener(CleanPanels);


        // Rover Hum
        if (movementAudio != null && !movementAudio.isPlaying)
        {
            movementAudio.volume = 0f;
            movementAudio.Play();
        }
}

    private void Update()
    {
        float throttleInput = Input.GetAxis("Vertical");
        float steerInput = Input.GetAxis("Horizontal");
        float brakeInput = Input.GetKey(KeyCode.Space) ? 1.0f : 0.0f;
        
        ApplyMovement(throttleInput);
        ApplySteering(steerInput);
        ApplyBrakes(brakeInput);
        SolarCharge();
        MakePanelsDustier();
    }

    public void ApplyMovement(float throttle)
    {
        // No movement if the battery level is at zero
        if (batteryLevel <= 0)
        {
            foreach (WheelController wheel in driveWheels)
            {
                wheel.wheelCollider.motorTorque = 0;  // Ensure no power is applied
            }
            return;
        }

        foreach (WheelController wheel in driveWheels)
        {
            if (Mathf.Abs(throttle) > 0.1f)
            {
                // Use the slider's selected speed when Shift is held
                float speedMultiplier = Input.GetKey(KeyCode.LeftShift) ? speedSlider.GetSelectedSpeed() * boostDrainMultiplier : normalSpeed;
                wheel.ApplyDrive(throttle * speedMultiplier); 

                // Call DrainBattery() to drain power based on speed
                DrainBattery(speedMultiplier); 
            }
            else
            {
                wheel.wheelCollider.motorTorque = 0;
            }
        }

        // Adjust rover movement audio with speed
        if (movementAudio != null)
        {
            // Check for any movement input (throttle or steering)
            bool inputPressed =
                Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f ||
                Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f;

            // Base volume on actual rover speed
            float speed = rb.linearVelocity.magnitude;
            float maxSpeed = 2.0f; 
            float targetVolume = Mathf.Clamp01(speed / maxSpeed) * 0.4f;

            if (inputPressed)
            {
                // Start audio if not playing
                if (!movementAudio.isPlaying)
                {
                    movementAudio.Play();
                }
            }

            // Adjust volume based on speed
            movementAudio.volume = Mathf.MoveTowards(movementAudio.volume, targetVolume, fadeSpeed * Time.deltaTime);

            // Stop audio if volume is 0 and no input
            if (!inputPressed && movementAudio.volume <= 0.01f && movementAudio.isPlaying)
            {
                movementAudio.Stop();
            }
        }
    }

    public void ApplySteering(float steer)
    {
        // No steering if battery level is zero
        if (batteryLevel <= 0)
        {
            return;
        }

        // Calculate steering angle
        float steerAngle = steer * maxSteerAngle;
        
        // Apply to all steering wheels
        foreach (WheelController wheel in steeringWheels)
        {
            wheel.wheelCollider.steerAngle = steerAngle;
        }
    }

    private void ApplyBrakes(float brake)
    {
        foreach (WheelController wheel in driveWheels)
        {
            wheel.ApplyBrake(brake * brakingMultiplier);
        }
    }

    private void UpdateWheelsVisuals()
    {
        foreach (WheelController wheel in driveWheels)
        {
            Quaternion rot;
            Vector3 pos;
            wheel.wheelCollider.GetWorldPose(out pos, out rot);
            wheel.wheelModel.position = pos;
            wheel.wheelModel.rotation = rot;
        }
    }

    private static float HorsepowerToWatts(float horsePower)
    {
        return horsePower * 745.7f;
    }
    
    public bool IsGrounded()
    {
        foreach (WheelController wheel in driveWheels)
        {
            if (wheel.wheelCollider.isGrounded)
                return true;
        }
        
        foreach (WheelController wheel in steeringWheels)
        {
            if (wheel.wheelCollider.isGrounded)
                return true;
        }
        
        return false;
    }

    private void DrainBattery(float speedMultiplier)
    {
        if (batteryLevel > 0)
        {
            batteryLevel -= (drainRate * speedMultiplier) * Time.deltaTime;
            batteryLevel = Mathf.Clamp(batteryLevel, 0f, 100f);
            
            UpdateBatteryUI();
        }
    }

    private void UpdateBatteryUI()
    {
        if (batteryText != null)
        {
            batteryText.text = "Battery: " + batteryLevel.ToString("F1") + "%";
        }
    }

    private void SolarCharge()
    {
        bool charging = false;

        if (batteryLevel < 100f && IsInSunlight() && IsStationary())
        {
            batteryLevel += solarChargeRate * Time.deltaTime;
            batteryLevel = Mathf.Clamp(batteryLevel, 0f, 100f);
            UpdateBatteryUI();  
            charging = true;
        }

        // Show or hide charging text
        if (chargingStatusText != null)
        {
            chargingStatusText.gameObject.SetActive(charging);
        }
    }

    private void MakePanelsDustier()
    {
        if (solarChargeRate >= dustRate)
        {
            solarChargeRate -= dustRate * Time.deltaTime;
            solarChargeRate = Mathf.Max(0f, solarChargeRate);
        }
    }

    public void CleanPanels()
    {
        solarChargeRate = 1.0f; //Reset the charging rate to default value
    }

    private bool IsStationary()
    {
        return ((Math.Abs(rb.linearVelocity.x) <= 0.2f) && (Math.Abs(rb.linearVelocity.z) <= 0.2f));  // Consider stationary if rovers barely moving
    }

    private bool IsInSunlight()
    {
        // Consider it day if sun intensity is strong enough
        return sunLight != null && sunLight.intensity > 0.2f;
    }

    public Rigidbody GetRigidBody(){
        return rb;
    }
}