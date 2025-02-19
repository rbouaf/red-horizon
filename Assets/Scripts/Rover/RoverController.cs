using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoverController : MonoBehaviour
{
    public RoverModel roverModel;
    public List<WheelController> driveWheels;
    public List<WheelController> steeringWheels;

    public float maxHorsepower = 0.14f;
    public float maxTorquePerWheel = 669.77f;
    public float maxSteerAngle = 30f;

    private float maxPowerWatts;

    private void Start()
    {
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

        ApplyMovement(throttleInput);
        ApplySteering(steerInput);
        ApplyBraking(brakeInput);
    }

    private void ApplyMovement(float throttle)
    {
        float powerPerWheel = maxPowerWatts / driveWheels.Count; // Distribute power across 6 wheels
        float rpm = driveWheels[0].wheelCollider.rpm; // Approximate RPM from one wheel

        foreach (WheelController wheel in driveWheels)
        {
            wheel.ApplyTorque(throttle, rpm);
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
        float brakeTorque = brakeForce * maxTorquePerWheel * 5f; // Scaled brake force

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