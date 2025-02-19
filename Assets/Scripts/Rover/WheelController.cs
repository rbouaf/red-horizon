using UnityEngine;

public class WheelController : MonoBehaviour
{
    public WheelCollider wheelCollider;
    public Transform visualWheel;

    public float maxTorque = 669.77f;
    public float horsepower = 0.023f;
    public float terrainFriction = 1.0f;
    
    private float currentTorque = 0f;
    

    public float maxPower;

    void Update()
    {
        UpdateWheelVisuals();
    }

    public void ApplyTorque(float throttleInput, float rpm)
    {
        // Convert power constraints into realistic torque application
        float maxPossibleTorque = maxPower / Mathf.Max(rpm * Mathf.Deg2Rad, 0.1f);
        currentTorque = Mathf.Clamp(throttleInput * maxTorque, -maxPossibleTorque, maxPossibleTorque);

        wheelCollider.motorTorque = currentTorque * terrainFriction;
    }

    public void ApplyBraking(float brakeForce)
    {
        wheelCollider.brakeTorque = brakeForce;
    }

    private void UpdateWheelVisuals()
    {
        if (visualWheel != null)
        {
            Vector3 position;
            Quaternion rotation;
            wheelCollider.GetWorldPose(out position, out rotation);

            visualWheel.position = position;
            visualWheel.rotation = rotation;
        }
    }

    public void SetFriction(float newFriction)
    {
        terrainFriction = Mathf.Clamp(newFriction, 0.5f, 1.5f);
    }
}
