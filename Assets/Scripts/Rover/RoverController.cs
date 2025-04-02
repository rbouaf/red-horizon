using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoverController : MonoBehaviour
{
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
    public float drainRate = 0.1f;  // Drain per second
    public float boostDrainMultiplier = 3f;  // Extra drain when boosting

    public TMPro.TextMeshProUGUI batteryText;  // Reference to UI Text

    [Header("Solar Battery Charging")]
    public float solarChargeRate = 0.5f; // Charge per second

    [Header("Center of Mass")]
    public Vector3 centerOfMassOffset = new Vector3(0, -0.5f, 0);
    private float maxPowerWatts;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        
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
    }

    private void ApplyMovement(float throttle)
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
    }

    private void ApplySteering(float steer)
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
        if (batteryLevel < 100f && IsInSunlight() && IsStationary())
        {
            batteryLevel += solarChargeRate * Time.deltaTime;
            batteryLevel = Mathf.Clamp(batteryLevel, 0f, 100f);

            UpdateBatteryUI();  
        }
    }

    private bool IsStationary()
    {
        return rb.linearVelocity.magnitude < 0.1f;  // Consider stationary if rovers barely moving
    }

    private bool IsInSunlight()
    {
        // For now always true, update later w sun exposure
        return true;
    }

    public Rigidbody GetRigidBody(){
        return rb;
    }
}