using UnityEngine;

public class WheelController : MonoBehaviour
{
    public WheelCollider wheelCollider;
    public Transform wheelModel;

    public float horsepower = 10f;
    public float maxPower;
    public float maxTorque;

    private void Start()
    {
        if (wheelCollider == null)
            wheelCollider = GetComponent<WheelCollider>();

        if (wheelModel == null)
            wheelModel = transform.GetChild(0);
    }

    public void ApplyDrive(float power)
    {
        if (wheelCollider == null)
            return;

        wheelCollider.motorTorque = power;
    }

    public void ApplySteering(float steerAngle)
    {
        if (wheelCollider == null)
            return;

        wheelCollider.steerAngle = steerAngle;
    }

    public void ApplyBrake(float brakeTorque)
    {
        if (wheelCollider == null)
            return;

        wheelCollider.brakeTorque = brakeTorque;
    }
}