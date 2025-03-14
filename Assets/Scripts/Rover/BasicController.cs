using UnityEngine;


public class BasicController : MonoBehaviour
{
    [Header("References")]
    public Rigidbody roverRigidbody;
    public WheelCollider[] wheelColliders;
    
    [Header("Physics Overrides")]
    public bool applyPhysicsOverrides = true;
    public float wheelMass = 10f;
    public float forwardExtremumSlip = 0.4f;
    public float forwardExtremumValue = 1.0f;
    public float forwardAsymptoteSlip = 0.8f;
    public float forwardAsymptoteValue = 0.5f;
    
    [Header("Force Override")]
    public bool useForceOverride = false;
    public float forceAmount = 5000f;
    
    [Header("Gravity Override")]
    public bool useCustomGravity = false;
    public float gravityMultiplier = 0.5f; // Reduce gravity for Mars
    
    private bool hasAppliedFixes = false;
    
    private void Start()
    {
        // Auto-find components if not assigned
        if (roverRigidbody == null)
            roverRigidbody = GetComponent<Rigidbody>();
            
        if (wheelColliders == null || wheelColliders.Length == 0)
            wheelColliders = GetComponentsInChildren<WheelCollider>();
            
        // Apply fixes on start
        if (applyPhysicsOverrides)
        {
            FixWheelPhysics();
        }
    }
    
    private void FixedUpdate()
    {
        // Apply custom gravity if enabled
        if (useCustomGravity && roverRigidbody != null)
        {
            Vector3 gravity = Physics.gravity * gravityMultiplier;
            roverRigidbody.AddForce(gravity * roverRigidbody.mass);
        }
        
        // Apply emergency force if enabled
        if (useForceOverride && roverRigidbody != null)
        {
            float input = Input.GetAxis("Vertical");
            if (Mathf.Abs(input) > 0.1f)
            {
                Vector3 force = transform.forward * input * forceAmount;
                roverRigidbody.AddForce(force);
                Debug.Log($"Applied emergency force: {force.magnitude:F1}N");
            }
            
            float turnInput = Input.GetAxis("Horizontal");
            if (Mathf.Abs(turnInput) > 0.1f)
            {
                Vector3 torque = transform.up * turnInput * forceAmount * 0.2f;
                roverRigidbody.AddTorque(torque);
            }
        }
    }
    
    [ContextMenu("Fix Wheel Physics")]
    public void FixWheelPhysics()
    {
        if (wheelColliders == null || wheelColliders.Length == 0)
        {
            Debug.LogError("No wheel colliders found!");
            return;
        }
        
        hasAppliedFixes = true;
        
        // Fix common issues that prevent wheels from spinning
        foreach (WheelCollider wheel in wheelColliders)
        {
            if (wheel == null) continue;
            
            // 1. Make sure wheel has appropriate mass
            wheel.mass = wheelMass;
            
            // 2. Fix forward friction curve to allow slip
            WheelFrictionCurve forwardFriction = wheel.forwardFriction;
            forwardFriction.extremumSlip = forwardExtremumSlip;
            forwardFriction.extremumValue = forwardExtremumValue;
            forwardFriction.asymptoteSlip = forwardAsymptoteSlip;
            forwardFriction.asymptoteValue = forwardAsymptoteValue;
            wheel.forwardFriction = forwardFriction;
            
            // 3. Fix sideways friction curve
            WheelFrictionCurve sidewaysFriction = wheel.sidewaysFriction;
            sidewaysFriction.extremumSlip = 0.2f;
            sidewaysFriction.asymptoteSlip = 0.5f;
            sidewaysFriction.asymptoteValue = 0.75f;
            wheel.sidewaysFriction = sidewaysFriction;
            
            // 4. Make sure suspension settings are appropriate
            JointSpring suspension = wheel.suspensionSpring;
            if (suspension.spring <= 0)
                suspension.spring = 35000f;
            if (suspension.damper <= 0)
                suspension.damper = 4500f;
            wheel.suspensionSpring = suspension;
            
            // 5. Ensure wheel radius is appropriate
            if (wheel.radius <= 0.01f)
                wheel.radius = 0.4f;
                
            // 6. Ensure suspension distance is reasonable
            if (wheel.suspensionDistance <= 0.01f)
                wheel.suspensionDistance = 0.3f;
                
            Debug.Log($"Applied emergency physics fixes to wheel: {wheel.name}");
        }
        
        // Fix rigidbody if needed
        if (roverRigidbody != null)
        {
            // Make sure rigidbody isn't frozen
            if (roverRigidbody.constraints == RigidbodyConstraints.FreezeAll)
            {
                roverRigidbody.constraints = RigidbodyConstraints.None;
                Debug.Log("Unfroze rigidbody constraints");
            }
            
            // Ensure not kinematic
            if (roverRigidbody.isKinematic)
            {
                roverRigidbody.isKinematic = false;
                Debug.Log("Disabled isKinematic on rigidbody");
            }
            
            // Set reasonable drag values
            if (roverRigidbody.linearDamping > 1.0f)
            {
                roverRigidbody.linearDamping = 0.05f;
                Debug.Log("Adjusted high drag value");
            }
            
            // Set interpolation for smoother movement
            roverRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            
            // Set collision mode
            roverRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
        
        // Log completion
        Debug.Log("Emergency wheel physics fixes applied!");
    }
    
    private void OnGUI()
    {
        if (!hasAppliedFixes)
        {
            GUI.Box(new Rect(10, 10, 300, 100), "Wheel RPM Fix");
            GUI.Label(new Rect(20, 30, 280, 30), "Press 'Apply Fixes' to fix wheel rpm issues");
            
            if (GUI.Button(new Rect(20, 60, 120, 30), "Apply Fixes"))
            {
                FixWheelPhysics();
            }
            
            if (GUI.Button(new Rect(150, 60, 140, 30), "Force Override"))
            {
                useForceOverride = !useForceOverride;
                Debug.Log($"Force override: {useForceOverride}");
            }
        }
        else
        {
            // Monitoring panel to show wheel RPMs
            GUI.Box(new Rect(10, 10, 300, 30 + wheelColliders.Length * 20), "Wheel RPM Monitor");
            
            for (int i = 0; i < wheelColliders.Length; i++)
            {
                if (wheelColliders[i] != null)
                {
                    string wheelName = wheelColliders[i].name;
                    float rpm = wheelColliders[i].rpm;
                    string groundedState = wheelColliders[i].isGrounded ? "Grounded" : "No Ground";
                    
                    GUI.Label(new Rect(20, 30 + i * 20, 280, 20), 
                        $"{wheelName}: {rpm:F1} RPM - {groundedState}");
                }
            }
            
            // Toggle force override button
            if (GUI.Button(new Rect(10, 40 + wheelColliders.Length * 20, 140, 30), 
                $"Force: {(useForceOverride ? "ON" : "OFF")}"))
            {
                useForceOverride = !useForceOverride;
            }
            
            // Toggle gravity override button
            if (GUI.Button(new Rect(160, 40 + wheelColliders.Length * 20, 140, 30), 
                $"Gravity: {(useCustomGravity ? "MARS" : "EARTH")}"))
            {
                useCustomGravity = !useCustomGravity;
            }
        }
    }
    
    // Debug visualization for collision check
    private void OnDrawGizmos()
    {
        if (wheelColliders == null) return;
        
        foreach (WheelCollider wheel in wheelColliders)
        {
            if (wheel == null) continue;
            
            // Get wheel world position
            Vector3 wheelPos = wheel.transform.position;
            
            // Draw wheel radius
            Gizmos.color = wheel.isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(wheelPos, wheel.radius);
            
            // Draw suspension 
            Gizmos.color = Color.yellow;
            Vector3 suspensionDir = wheel.transform.TransformDirection(Vector3.down);
            Gizmos.DrawLine(wheelPos, wheelPos + suspensionDir * wheel.suspensionDistance);
            
            // Draw wheel direction
            Gizmos.color = Color.blue;
            Vector3 forwardDir = wheel.transform.TransformDirection(Vector3.forward);
            Gizmos.DrawRay(wheelPos, forwardDir * 0.5f);
        }
    }
}