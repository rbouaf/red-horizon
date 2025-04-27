using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Manages different AI brain modules for the rover.
/// Depends on NavMeshManager for NavMesh status.
/// </summary>
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
    
    [Header("NavMesh Dependencies")]
    [SerializeField] private bool waitForNavMesh = true;
    [SerializeField] private NavMeshManager navMeshManager;
    
    [Header("Available Brains")]
    [SerializeField] private List<BrainInfo> availableBrains = new List<BrainInfo>();
    
    [Header("Settings")]
    [SerializeField] private bool activateDefaultBrainOnStart = true;
    [SerializeField] private int defaultBrainIndex = 0;

    public RoverController roverController;
    
    // Events
    public event Action<string> OnBrainActivated;
    public event Action<string> OnBrainDeactivated;
    
    private IBrain activeBrain = null;
    private string activeBrainName = "";
    private bool areBrainsInitialized = false;
    private bool isWaitingForNavMesh = false;
    
    private void Awake()
    {
        roverController = GetComponent<RoverController>();

        // Set up brain interfaces
        foreach (BrainInfo brainInfo in availableBrains)
        {
            if (brainInfo.brainComponent != null && brainInfo.brainComponent is IBrain)
            {
                brainInfo.brain = brainInfo.brainComponent as IBrain;
            }
            else
            {
                Debug.LogError($"Brain '{brainInfo.brainName}' does not implement IBrain interface.");
            }
        }

        // Find NavMeshManager if not assigned
        if (navMeshManager == null)
        {
            navMeshManager = FindAnyObjectByType<NavMeshManager>();
            if (navMeshManager == null && waitForNavMesh)
            {
                Debug.LogWarning("NavMeshManager not found! Creating a new instance.");
                GameObject navMeshManagerObj = new GameObject("NavMeshManager");
                navMeshManager = navMeshManagerObj.AddComponent<NavMeshManager>();
            }
        }
    }
    
    private void Start()
    {
        if (waitForNavMesh && navMeshManager != null)
        {
            // Check if NavMesh is already ready
            if (navMeshManager.IsNavMeshReady())
            {
                Debug.Log("NavMesh is already ready, initializing brains...");
                InitializeBrains();
            }
            else
            {
                // Subscribe to NavMeshManager events
                navMeshManager.OnNavMeshReady += OnNavMeshReady;
                navMeshManager.OnNavMeshGenerationFailed += OnNavMeshGenerationFailed;
                
                isWaitingForNavMesh = true;
                Debug.Log("Waiting for NavMesh to be ready...");
            }
        }
        else
        {
            // Don't wait for NavMesh, initialize brains immediately
            InitializeBrains();
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from NavMeshManager events
        if (navMeshManager != null)
        {
            navMeshManager.OnNavMeshReady -= OnNavMeshReady;
            navMeshManager.OnNavMeshGenerationFailed -= OnNavMeshGenerationFailed;
        }
    }
    
    private void OnNavMeshReady()
    {
        Debug.Log("NavMesh is ready, initializing brains...");
        isWaitingForNavMesh = false;
        InitializeBrains();
    }
    
    private void OnNavMeshGenerationFailed(string errorMessage)
    {
        Debug.LogError($"NavMesh generation failed: {errorMessage}");
        
        // Decide whether to initialize brains anyway
        if (waitForNavMesh)
        {
            Debug.LogWarning("Cannot initialize brains without NavMesh. Set waitForNavMesh to false to override.");
        }
        else
        {
            Debug.LogWarning("Initializing brains despite NavMesh generation failure.");
            InitializeBrains();
        }
    }
    
    private void Update()
    {
        // Skip if no brain is active or brains aren't initialized
        if (activeBrain == null || !areBrainsInitialized)
            return;
            
        // Let the active brain think
        if (!activeBrain.isPaused())
        {
            activeBrain.Think();
        }
    }
    
    public void InitializeBrains()
    {
        if (areBrainsInitialized)
            return;
            
        // Initialize all brains but keep them inactive
        Debug.Log("Initializing all brains...");
        foreach (BrainInfo brainInfo in availableBrains)
        {
            if (brainInfo.brain != null)
            {
                brainInfo.brain.Initialize(gameObject);
                brainInfo.brain.SetPaused(true);
                Debug.Log($"Initialized brain: {brainInfo.brainName}");
            }
        }
        
        areBrainsInitialized = true;
        
        // Activate default brain if specified
        if (activateDefaultBrainOnStart && availableBrains.Count > 0)
        {
            // Make sure default index is valid
            int index = Mathf.Clamp(defaultBrainIndex, 0, availableBrains.Count - 1);
            ActivateBrain(availableBrains[index].brainName);
        }
    }
    public void ActivatePathfindingBrain()
    {
        ActivateBrain("PathfindingBrain");
    }
    
    public bool ActivateBrain(string brainName)
    {
        // Don't allow activation until brains are initialized
        if (!areBrainsInitialized)
        {
            Debug.LogWarning($"Cannot activate brain '{brainName}' until brains are initialized!");
            return false;
        }
        
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
                roverController.manualControlEnabled = false;
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
            roverController.manualControlEnabled = true;
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
        if (availableBrains.Count <= 1 || !areBrainsInitialized)
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
    
    // Check if we're still waiting for NavMesh
    public bool IsWaitingForNavMesh()
    {
        return isWaitingForNavMesh;
    }
}