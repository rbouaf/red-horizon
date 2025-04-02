using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Interface for all AI brain implementations.
/// </summary>
public interface IBrain
{
    /// Initialize the brain with necessary references and settings
    /// The GameObject this brain is attached to
    void Initialize(GameObject gameObject);
    
    /// Main thinking method that should be called each update
    void Think();
    
    /// Gets the current status/state of the brain
    /// Returns Status information as string
    string GetStatus();
    
    /// Pauses or resumes the brain processing
    /// "isPaused" Whether to pause (true) or resume (false)
    void SetPaused(bool isPaused);
}
