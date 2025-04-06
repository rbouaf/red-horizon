using UnityEngine;

/// <summary>
/// Base class for all rover brain implementations
/// Provides common functionality and implements the IBrain interface
/// </summary>
public abstract class BrainBase : MonoBehaviour, IBrain
{
    [Header("Common Brain Properties")]
    [SerializeField] protected bool isPausedState = false;
    
    // Reference to the rover body
    protected GameObject roverGameObject;
    protected RoverController roverController;
    
    // Status reporting for UI display
    protected string currentStatus = "Initializing";
    
    public virtual void Initialize(GameObject gameObject)
    {
        roverGameObject = gameObject;
        roverController = gameObject.GetComponent<RoverController>();
        
        if (roverController == null)
        {
            Debug.LogWarning($"{GetType().Name}: No RoverController found on target rover");
        }
        
        currentStatus = "Ready";
    }
    
    public virtual void Think()
    {
        // Default implementation does nothing
        // Override in derived classes to implement AI behavior
    }
    
    public void SetPaused(bool paused)
    {
        isPausedState = paused;
        currentStatus = isPausedState ? "Paused" : "Active";
    }
    
    public bool isPaused()
    {
        return isPausedState;
    }
    
    public string GetStatus()
    {
        return currentStatus;
    }
    
    protected void LogDebug(string message)
    {
        Debug.Log($"[{GetType().Name}] {message}");
    }
}