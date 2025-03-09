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

        UpdateWheelsVisuals();
    }

    private void ApplyMovement(float throttle)
    {
        foreach (WheelController wheel in driveWheels)
        {
            if (Mathf.Abs(throttle) > 0.1f)
            {
                wheel.ApplyDrive(throttle);
            }
            else
            {
                wheel.wheelCollider.motorTorque = 0;
            }
        }
    }

    private void ApplySteering(float steer)
    {
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
}