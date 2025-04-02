using UnityEngine;
using System;
using System.Collections.Generic;


/// Manages different AI brain modules for the rover.
public class RoverBrainManager : MonoBehaviour
{
    [System.Serializable]
    public class BrainInfo
    {
        public string brainName;
        public MonoBehaviour brainComponent;
        public bool isActive = false;
        
        [HideInInspector]
        public IBrain brain;
    }
    
    [Header("Available Brains")]
    [SerializeField] private List<BrainInfo> availableBrains = new List<BrainInfo>();
    
    [Header("Settings")]
    [SerializeField] private bool activateDefaultBrainOnStart = true;
    [SerializeField] private int defaultBrainIndex = 0;
    
    // Events
    public event Action<string> OnBrainActivated;
    public event Action<string> OnBrainDeactivated;
    
    private IBrain activeBrain = null;
    private string activeBrainName = "";
    
    private void Awake()
    {
        // Initialize and validate all brains
        foreach (BrainInfo brainInfo in availableBrains)
        {
            if (brainInfo.brainComponent != null && brainInfo.brainComponent is IBrain)
            {
                brainInfo.brain = brainInfo.brainComponent as IBrain;
                brainInfo.brain.Initialize(gameObject);
            }
            else
            {
                Debug.LogError($"Brain '{brainInfo.brainName}' does not implement IBrain interface.");
            }
        }
    }
    
    private void Start()
    {
        if (activateDefaultBrainOnStart && availableBrains.Count > 0)
        {
            // Make sure default index is valid
            int index = Mathf.Clamp(defaultBrainIndex, 0, availableBrains.Count - 1);
            ActivateBrain(availableBrains[index].brainName);
        }
    }
    
    public bool ActivateBrain(string brainName)
    {
        // If the brain is already active, do nothing
        if (activeBrainName == brainName && activeBrain != null)
            return true;
            
        // First, deactivate current brain if any
        if (activeBrain != null)
        {
            activeBrain.SetPaused(true);
            OnBrainDeactivated?.Invoke(activeBrainName);
        }
        
        // Find and activate the requested brain
        foreach (BrainInfo brainInfo in availableBrains)
        {
            if (brainInfo.brainName == brainName && brainInfo.brain != null)
            {
                activeBrain = brainInfo.brain;
                activeBrainName = brainName;
                
                // Set all brains' active state for UI reflection
                foreach (var brain in availableBrains)
                {
                    brain.isActive = (brain.brainName == brainName);
                }
                
                activeBrain.SetPaused(false);
                Debug.Log($"Activated brain: {brainName}");
                
                OnBrainActivated?.Invoke(brainName);
                return true;
            }
        }
        
        Debug.LogWarning($"Failed to activate brain: {brainName} - not found or not initialized");
        return false;
    }
    
    public void DeactivateAllBrains()
    {
        if (activeBrain != null)
        {
            activeBrain.SetPaused(true);
            OnBrainDeactivated?.Invoke(activeBrainName);
            
            activeBrain = null;
            activeBrainName = "";
            
            // Set all brains inactive
            foreach (var brain in availableBrains)
            {
                brain.isActive = false;
            }
            
            Debug.Log("All brains deactivated");
        }
    }
    
    public string GetActiveBrainName()
    {
        return activeBrainName;
    }
    
    public string GetActiveBrainStatus()
    {
        return activeBrain != null ? activeBrain.GetStatus() : "No active brain";
    }
    
    public List<string> GetAvailableBrainNames()
    {
        List<string> names = new List<string>();
        foreach (BrainInfo brainInfo in availableBrains)
        {
            names.Add(brainInfo.brainName);
        }
        return names;
    }
    
    // Access a specific brain by name for advanced control
    public T GetBrain<T>(string brainName) where T : class, IBrain
    {
        foreach (BrainInfo brainInfo in availableBrains)
        {
            if (brainInfo.brainName == brainName && brainInfo.brain is T)
            {
                return brainInfo.brain as T;
            }
        }
        
        return null;
    }
    
    // Quick access to active brain with type cast
    public T GetActiveBrain<T>() where T : class, IBrain
    {
        if (activeBrain is T)
        {
            return activeBrain as T;
        }
        
        return null;
    }
    
    // UI helper method to cycle through available brains
    public void CycleToNextBrain()
    {
        if (availableBrains.Count <= 1)
            return;
            
        int currentIndex = -1;
        
        // Find current brain index
        for (int i = 0; i < availableBrains.Count; i++)
        {
            if (availableBrains[i].brainName == activeBrainName)
            {
                currentIndex = i;
                break;
            }
        }
        
        // Cycle to the next brain
        int nextIndex = (currentIndex + 1) % availableBrains.Count;
        ActivateBrain(availableBrains[nextIndex].brainName);
    }
}
