using UnityEngine;

public class WheelController : MonoBehaviour
{
    public WheelCollider wheelCollider;
    public Transform visualWheel;

    public float maxTorque = 6690.77f;
    public float horsepower = 3f;
    public float terrainFriction = 1.0f;
    
    private float currentTorque = 0f;
    public float maxPower;

    public void ApplyTorque(float throttleInput, float rpm)
    {
        // Reset brake torque when applying motor torque
        wheelCollider.brakeTorque = 0f;
        
        // Apply torque based on power curve - torque decreases as RPM increases
        float normalizedRPM = Mathf.Abs(rpm) / 1000f; // Normalize RPM to 0-1 range based on expected max RPM
        float torqueFactor = Mathf.Clamp01(1.0f - normalizedRPM);  // Higher RPM = lower torque
        
        currentTorque = maxTorque * throttleInput * torqueFactor * terrainFriction;
        Debug.Log("Torque factor: " + currentTorque);
        wheelCollider.motorTorque = currentTorque;
    }

    public void ApplyBraking(float brakeForce)
    {
        wheelCollider.motorTorque = 0f;
        wheelCollider.brakeTorque = brakeForce;
    }

    public void UpdateWheelVisuals()
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
        
        // Update wheel friction curves to match terrain
        WheelFrictionCurve fwdFriction = wheelCollider.forwardFriction;
        fwdFriction.stiffness = terrainFriction;
        wheelCollider.forwardFriction = fwdFriction;
        
        WheelFrictionCurve sideFriction = wheelCollider.sidewaysFriction;
        sideFriction.stiffness = terrainFriction * 0.9f; // Slightly less side friction for better turning
        wheelCollider.sidewaysFriction = sideFriction;
    }

    // Surface detection to adjust friction based on terrain
    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.CompareTag("SandTerrain"))
        {
            SetFriction(0.7f);
        }
        else if (collision.collider.CompareTag("RockyTerrain"))
        {
            SetFriction(1.2f);
        }
        else
        {
            SetFriction(1.0f);
        }
    }
}