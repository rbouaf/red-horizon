using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoverController : MonoBehaviour
{
    public RoverModel roverModel;
    public List<WheelController> driveWheels;
    public List<WheelController> steeringWheels;

    public float maxHorsepower = 30f;
    public float maxTorquePerWheel = 6690.77f;
    public float maxSteerAngle = 30f;
    public float brakingMultiplier = 5f;

    private float maxPowerWatts;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -0.5f, 0);

        maxPowerWatts = HorsepowerToWatts(maxHorsepower);
        foreach (WheelController wheel in driveWheels)
        {
            wheel.maxPower = HorsepowerToWatts(wheel.horsepower);
        }
    }

    private void Update()
    {
        float throttleInput = Input.GetAxis("Vertical");
        float steerInput = Input.GetAxis("Horizontal");
        float brakeInput = Input.GetKey(KeyCode.Space) ? 1.0f : 0.0f;
        
        // Apply the player inputs to the rover
        ApplyMovement(throttleInput);
        ApplySteering(steerInput);
        ApplyBraking(brakeInput);
        
        // Update wheel visuals
        foreach (WheelController wheel in driveWheels)
        {
            wheel.UpdateWheelVisuals();
        }
        
        foreach (WheelController wheel in steeringWheels)
        {
            wheel.UpdateWheelVisuals();
        }
    }

    private void ApplyMovement(float throttle)
    {
        float rpmAverage = 0f;
        int wheelCount = 0;
        
        // Calculate average RPM of all drive wheels
        foreach (WheelController wheel in driveWheels)
        {
            if (wheel.wheelCollider.isGrounded)
            {
                rpmAverage += wheel.wheelCollider.rpm;
                wheelCount++;
            }
        }
        
        if (wheelCount > 0)
        {
            rpmAverage /= wheelCount;
        }

        // Apply torque to each drive wheel
        foreach (WheelController wheel in driveWheels)
        {
            wheel.ApplyTorque(throttle, rpmAverage);
        }
    }

    private void ApplySteering(float steer)
    {
        float steerAngle = steer * maxSteerAngle;
        
        foreach (WheelController wheel in steeringWheels)
        {
            wheel.wheelCollider.steerAngle = steerAngle;
        }
    }

    private void ApplyBraking(float brakeForce)
    {
        float brakeTorque = brakeForce * maxTorquePerWheel * brakingMultiplier;

        foreach (WheelController wheel in driveWheels)
        {
            wheel.ApplyBraking(brakeTorque);
        }
    }

    private static float HorsepowerToWatts(float horsePower)
    {
        return horsePower * 745.7f;
    }
}