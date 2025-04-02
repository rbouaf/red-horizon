using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Base abstract class for all AI brains providing common functionality.
/// </summary>
public abstract class BrainBase : MonoBehaviour, IBrain
{
    [Header("Brain Settings")]
    [SerializeField] protected bool debugMode = false;
    [SerializeField] protected float thinkInterval = 0.2f; // How often (in seconds) the brain thinks
    
    protected GameObject roverGameObject;
    protected Rigidbody roverRigidbody;
    protected RoverController roverController;
    
    protected bool isPaused = false;
    protected float lastThinkTime = 0f;
    
    protected string currentStatus = "Initializing";
    
    public virtual void Initialize(GameObject gameObject)
    {
        this.roverGameObject = gameObject;
        this.roverRigidbody = gameObject.GetComponent<Rigidbody>();
        this.roverController = gameObject.GetComponent<RoverController>();
        
        if (roverRigidbody == null || roverController == null)
        {
            Debug.LogError("Brain initialization failed: Missing required components on rover");
            currentStatus = "Error: Missing components";
        }
        else
        {
            currentStatus = "Initialized";
        }
    }
    
    public abstract void Think();
    
    protected virtual void Update()
    {
        if (isPaused || Time.time < lastThinkTime + thinkInterval)
            return;
            
        lastThinkTime = Time.time;
        Think();
    }
    
    public virtual string GetStatus()
    {
        return currentStatus;
    }
    
    public virtual void SetPaused(bool isPaused)
    {
        this.isPaused = isPaused;
        currentStatus = isPaused ? "Paused" : "Active";
    }
    
    protected virtual void LogDebug(string message)
    {
        if (debugMode)
        {
            Debug.Log($"[{GetType().Name}] {message}");
        }
    }
}
