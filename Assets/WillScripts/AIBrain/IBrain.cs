using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Interface for all AI brain implementations.
/// </summary>
public interface IBrain
{
    /// <summary>
    /// Initialize the brain with necessary references and settings
    /// </summary>
    /// <param name="gameObject">The GameObject this brain is attached to</param>
    void Initialize(GameObject gameObject);
    
    /// <summary>
    /// Main thinking method that should be called each update
    /// </summary>
    void Think();
    
    /// <summary>
    /// Gets the current status/state of the brain
    /// </summary>
    /// <returns>Status information as string</returns>
    string GetStatus();
    
    /// <summary>
    /// Pauses or resumes the brain processing
    /// </summary>
    /// <param name="isPaused">Whether to pause (true) or resume (false)</param>
    void SetPaused(bool isPaused);
}
