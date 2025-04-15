using UnityEngine;
using Unity.AI.Navigation; // For NavMeshSurface
using UnityEngine.AI; // For NavMesh
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Manages NavMesh generation and provides events for other systems to know when the NavMesh is ready.
/// </summary>
public class NavMeshManager : MonoBehaviour
{
    public static NavMeshManager Instance { get; private set; }

    [Header("NavMesh Surface References")]
    [SerializeField] private NavMeshSurface mainSurface;
    [SerializeField] private List<NavMeshSurface> additionalSurfaces = new List<NavMeshSurface>();
    
    [Header("NavMesh Settings")]
    [SerializeField, Range(0, 60)] private float maxSlope = 25f; // Maximum walkable slope in degrees
    
    [Header("Exclusion Settings")]
    [SerializeField] private GameObject[] objectsToExclude;
    [SerializeField] private int exclusionLayer = 2; // Ignore Raycast layer by default
    [SerializeField] private bool findRoverAutomatically = true;
    [SerializeField] private string roverTag = "Player";
    
    [Header("Generation Settings")]
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private float generationDelay = 2f;
    [SerializeField] private bool useChunkedBuild = true;
    [SerializeField] private int chunkSize = 500;
    [SerializeField] private float timeout = 60f; // Maximum time to wait for NavMesh generation
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    
    // Status
    private bool isGenerating = false;
    private bool isNavMeshReady = false;
    
    // Events
    public event Action OnNavMeshGenerationStarted;
    public event Action OnNavMeshReady;
    public event Action<string> OnNavMeshGenerationFailed;
    
    // Chunked building state
    private Dictionary<GameObject, int> currentLayerStorage;
    private int currentOriginalLayerMask;
    
    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        
        // Auto-find the NavMeshSurface if not assigned
        if (mainSurface == null)
        {
            mainSurface = GetComponent<NavMeshSurface>();
            
            if (mainSurface == null)
            {
                Debug.LogWarning("No NavMeshSurface component found. Add a NavMeshSurface component to this GameObject.");
            }
        }
    }
    
    void Start()
    {
        if (generateOnStart)
        {
            GenerateNavMesh();
        }
        else
        {
            // Check if NavMesh already exists
            if (IsNavMeshPresent())
            {
                Debug.Log("NavMesh already exists in the scene.");
                isNavMeshReady = true;
                OnNavMeshReady?.Invoke();
            }
        }
    }
    
    /// <summary>
    /// Generates a NavMesh for the current scene.
    /// </summary>
    public void GenerateNavMesh()
    {
        if (isGenerating)
        {
            Debug.LogWarning("NavMesh generation already in progress.");
            return;
        }
        
        if (isNavMeshReady)
        {
            Debug.Log("NavMesh is already ready. Skipping generation.");
            return;
        }
        
        StartCoroutine(GenerateNavMeshWithDelay());
    }
    
    /// <summary>
    /// Waits for delay, then generates the NavMesh
    /// </summary>
    private IEnumerator GenerateNavMeshWithDelay()
    {
        isGenerating = true;
        OnNavMeshGenerationStarted?.Invoke();
        
        if (showDebugInfo)
        {
            Debug.Log($"Waiting {generationDelay} seconds before NavMesh generation...");
        }
        
        // Wait for a delay to ensure all terrain and objects are fully loaded/initialized
        yield return new WaitForSeconds(generationDelay);
        
        if (showDebugInfo)
        {
            Debug.Log("Starting NavMesh generation...");
        }
        
        // Find rover automatically if enabled
        if (findRoverAutomatically && (objectsToExclude == null || objectsToExclude.Length == 0))
        {
            GameObject rover = GameObject.FindGameObjectWithTag(roverTag);
            if (rover != null)
            {
                objectsToExclude = new GameObject[] { rover };
                Debug.Log($"Automatically found rover: {rover.name}");
            }
        }
        
        // Setup layer exclusion 
        Dictionary<GameObject, int> originalLayers = SetupExclusionLayers();
        
        // Save original layerMask
        int originalLayerMask = 0;
        
        if (mainSurface != null)
        {
            originalLayerMask = mainSurface.layerMask;
            
            // Modify layerMask to exclude the specified layer
            mainSurface.layerMask &= ~(1 << exclusionLayer);
            
            // Configure max slope using NavMeshBuildSettings
            ConfigureMaxSlope(mainSurface);
        }
        
        // Store for callbacks
        currentLayerStorage = originalLayers;
        currentOriginalLayerMask = originalLayerMask;
        
        // Build NavMesh based on method
        if (useChunkedBuild && mainSurface != null)
        {
            // Start a separate coroutine for chunked building
            StartCoroutine(BuildNavMeshInChunks());
        }
        else
        {
            try
            {
                // Build the main surface
                if (mainSurface != null)
                {
                    mainSurface.BuildNavMesh();
                    
                    // Build any additional surfaces
                    foreach (var surface in additionalSurfaces)
                    {
                        if (surface != null)
                        {
                            int originalSurfaceLayerMask = surface.layerMask;
                            surface.layerMask &= ~(1 << exclusionLayer);
                            ConfigureMaxSlope(surface);
                            surface.BuildNavMesh();
                            surface.layerMask = originalSurfaceLayerMask;
                        }
                    }
                    
                    FinalizeNavMeshGeneration(true, originalLayers, originalLayerMask);
                }
                else
                {
                    Debug.LogError("No main NavMeshSurface assigned!");
                    FinalizeNavMeshGeneration(false, originalLayers, originalLayerMask, "No main NavMeshSurface assigned!");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error during NavMesh generation: {ex.Message}");
                FinalizeNavMeshGeneration(false, originalLayers, originalLayerMask, $"Error during NavMesh generation: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// Configures the max slope on a NavMeshSurface
    /// </summary>
    private void ConfigureMaxSlope(NavMeshSurface surface)
    {
        if (surface == null) return;
        
        try
        {
            // NavMeshSurface doesn't directly expose maxSlope, so we need to configure the agent settings
            
            // Get the NavMeshBuildSettings that this surface will use
            NavMeshBuildSettings buildSettings = NavMesh.GetSettingsByID(surface.agentTypeID);
            
            // Create a custom agent type with our settings
            var radius = buildSettings.agentRadius;
            var height = buildSettings.agentHeight;
            var stepHeight = buildSettings.agentClimb;
            
            // Check if we can directly configure the settings through reflection
            var surfaceType = surface.GetType();
            
            // Try direct setting via SerializedObject if reflection doesn't work
            var serialized = new UnityEditor.SerializedObject(surface);
            var agentRampProp = serialized.FindProperty("m_AgentSlope");
            
            if (agentRampProp != null)
            {
                // Direct access to NavMeshSurface properties
                agentRampProp.floatValue = maxSlope;
                serialized.ApplyModifiedProperties();
                
                if (showDebugInfo)
                {
                    Debug.Log($"Set max slope to {maxSlope} via SerializedObject");
                }
            }
            else
            {
                if (showDebugInfo)
                {
                    Debug.Log($"Setting max slope to {maxSlope} on agent type {surface.agentTypeID}");
                    Debug.Log($"Current agent slope is {buildSettings.agentSlope}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error configuring max slope: {ex.Message}");
        }
    }
    
    private Dictionary<GameObject, int> SetupExclusionLayers()
    {
        Dictionary<GameObject, int> originalLayers = new Dictionary<GameObject, int>();
        if (objectsToExclude != null)
        {
            foreach (var obj in objectsToExclude)
            {
                if (obj != null)
                {
                    // Store original layer
                    originalLayers[obj] = obj.layer;
                    
                    // Change to exclusion layer
                    obj.layer = exclusionLayer;
                    
                    // Also change all children
                    foreach (Transform child in obj.GetComponentsInChildren<Transform>())
                    {
                        originalLayers[child.gameObject] = child.gameObject.layer;
                        child.gameObject.layer = exclusionLayer;
                    }
                    
                    Debug.Log($"Excluded object from NavMesh generation: {obj.name}");
                }
            }
        }
        
        return originalLayers;
    }
    
    private void RestoreExclusionLayers(Dictionary<GameObject, int> originalLayers)
    {
        if (originalLayers != null)
        {
            foreach (var entry in originalLayers)
            {
                if (entry.Key != null)
                {
                    entry.Key.layer = entry.Value;
                }
            }
            
            Debug.Log("Restored original layers for excluded objects");
        }
    }
    
    private void FinalizeNavMeshGeneration(bool success, Dictionary<GameObject, int> originalLayers, int originalLayerMask, string errorMessage = "")
    {
        // Restore original layerMask
        if (mainSurface != null)
        {
            mainSurface.layerMask = originalLayerMask;
        }
        
        // Restore original layers for excluded objects
        RestoreExclusionLayers(originalLayers);
        
        // Check if NavMesh was successfully generated
        if (success && IsNavMeshPresent())
        {
            Debug.Log("NavMesh generation completed successfully.");
            isNavMeshReady = true;
            OnNavMeshReady?.Invoke();
        }
        else
        {
            if (string.IsNullOrEmpty(errorMessage))
            {
                errorMessage = "NavMesh generation completed but no NavMesh was detected!";
                Debug.LogError(errorMessage);
            }
            
            OnNavMeshGenerationFailed?.Invoke(errorMessage);
        }
        
        isGenerating = false;
    }
    
    /// <summary>
    /// Builds the NavMesh in chunks for better performance
    /// </summary>
    private IEnumerator BuildNavMeshInChunks()
    {
        if (mainSurface == null)
        {
            FinalizeNavMeshGeneration(false, currentLayerStorage, currentOriginalLayerMask, "No NavMeshSurface found for chunked building");
            yield break;
        }
        
        // Store original size and center
        Vector3 originalSize = mainSurface.size;
        Vector3 originalCenter = mainSurface.center;
        
        // Calculate number of chunks in each dimension
        int xChunks = Mathf.CeilToInt(originalSize.x / chunkSize);
        int zChunks = Mathf.CeilToInt(originalSize.z / chunkSize);
        
        Debug.Log($"Building NavMesh in {xChunks}x{zChunks} chunks");
        
        // Calculate the actual chunk size to cover the entire area
        float xChunkSize = originalSize.x / xChunks;
        float zChunkSize = originalSize.z / zChunks;
        
        // Process each chunk
        int totalChunks = xChunks * zChunks;
        int processedChunks = 0;
        
        bool success = true;
        string errorMessage = "";
        
        // Using a for loop without try-catch for yielding
        for (int x = 0; x < xChunks && success; x++)
        {
            for (int z = 0; z < zChunks && success; z++)
            {
                try
                {
                    // Calculate chunk center based on original bounds
                    Vector3 chunkCenter = new Vector3(
                        originalCenter.x - originalSize.x/2 + xChunkSize/2 + x * xChunkSize,
                        originalCenter.y,
                        originalCenter.z - originalSize.z/2 + zChunkSize/2 + z * zChunkSize
                    );
                    
                    // Set chunk size and center
                    mainSurface.center = chunkCenter;
                    mainSurface.size = new Vector3(xChunkSize, originalSize.y, zChunkSize);
                    
                    // Build this chunk
                    mainSurface.BuildNavMesh();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error during chunk ({x},{z}) NavMesh generation: {ex.Message}");
                    success = false;
                    errorMessage = $"Error during chunked NavMesh generation: {ex.Message}";
                    break;
                }
                
                processedChunks++;
                if (showDebugInfo && processedChunks % 5 == 0)
                {
                    Debug.Log($"NavMesh generation progress: {processedChunks}/{totalChunks} chunks");
                }
                
                // Yield every few chunks to prevent freezing (outside try-catch)
                if (processedChunks % 3 == 0)
                {
                    yield return null;
                }
            }
            
            // Break outer loop if inner loop failed
            if (!success) break;
        }
        
        try
        {
            // Restore original size and center
            mainSurface.size = originalSize;
            mainSurface.center = originalCenter;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error restoring NavMeshSurface settings: {ex.Message}");
            if (success)
            {
                success = false;
                errorMessage = $"Error after NavMesh generation: {ex.Message}";
            }
        }
        
        if (success)
        {
            Debug.Log($"Completed building NavMesh in {processedChunks}/{totalChunks} chunks");
            FinalizeNavMeshGeneration(true, currentLayerStorage, currentOriginalLayerMask);
        }
        else
        {
            FinalizeNavMeshGeneration(false, currentLayerStorage, currentOriginalLayerMask, errorMessage);
        }
    }
    
    /// <summary>
    /// Checks if a NavMesh exists in the scene.
    /// </summary>
    public bool IsNavMeshPresent()
    {
        // First try at the center of the terrain/navmesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(Vector3.zero, out hit, 500f, NavMesh.AllAreas))
        {
            return true;
        }
        
        // Then try at this object's position
        if (NavMesh.SamplePosition(transform.position, out hit, 50f, NavMesh.AllAreas))
        {
            return true;
        }
        
        // Finally try a wide area search
        for (int x = -500; x <= 500; x += 200)
        {
            for (int z = -500; z <= 500; z += 200)
            {
                Vector3 testPos = new Vector3(x, 0, z);
                if (NavMesh.SamplePosition(testPos, out hit, 100f, NavMesh.AllAreas))
                {
                    Debug.Log($"NavMesh found at position {hit.position}");
                    return true;
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Returns whether the NavMesh is ready for use.
    /// </summary>
    public bool IsNavMeshReady()
    {
        return isNavMeshReady;
    }
    
    /// <summary>
    /// Clears all NavMesh data.
    /// </summary>
    public void ClearNavMesh()
    {
        if (mainSurface != null)
        {
            mainSurface.RemoveData();
        }
        
        foreach (var surface in additionalSurfaces)
        {
            if (surface != null)
            {
                surface.RemoveData();
            }
        }
        
        isNavMeshReady = false;
        Debug.Log("NavMesh data cleared");
    }
    
    /// <summary>
    /// Returns position on the NavMesh closest to the specified position.
    /// </summary>
    public Vector3 GetClosestNavMeshPosition(Vector3 position, float maxDistance = 10f)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(position, out hit, maxDistance, NavMesh.AllAreas))
        {
            return hit.position;
        }
        
        Debug.LogWarning($"Could not find NavMesh position near {position}");
        return position;
    }
    
    /// <summary>
    /// Get the current max slope setting.
    /// </summary>
    public float GetMaxSlope()
    {
        return maxSlope;
    }
    
    /// <summary>
    /// Set a new max slope value and regenerate the NavMesh if needed.
    /// </summary>
    public void SetMaxSlope(float newMaxSlope, bool regenerateNavMesh = true)
    {
        maxSlope = Mathf.Clamp(newMaxSlope, 0f, 60f);
        
        if (regenerateNavMesh && isNavMeshReady)
        {
            Debug.Log($"Regenerating NavMesh with new max slope: {maxSlope}");
            isNavMeshReady = false;
            ClearNavMesh();
            GenerateNavMesh();
        }
    }
    
    /// Update the visualizations
    private void OnDrawGizmosSelected()
    {
        // Draw the main surface bounds if available
        if (mainSurface != null)
        {
            Gizmos.color = new Color(0.2f, 0.8f, 0.8f, 0.2f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(mainSurface.center, mainSurface.size);
            
            Gizmos.color = new Color(0.2f, 0.8f, 0.8f, 0.7f);
            Gizmos.DrawWireCube(mainSurface.center, mainSurface.size);
        }
    }
}