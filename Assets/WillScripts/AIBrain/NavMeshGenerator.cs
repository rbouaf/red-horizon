using UnityEngine;
using Unity.AI.Navigation;
using System.Collections;
using System.Collections.Generic;

/// Generates a NavMesh at runtime for the Mars terrain using NavMeshSurface.
public class NavMeshGenerator : MonoBehaviour
{
    [Header("NavMesh Generation")]
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private float generationDelay = 2f;
    [SerializeField] private bool showDebugInfo = true;
    
    [Header("NavMesh Surface References")]
    [SerializeField] private NavMeshSurface mainSurface;
    [SerializeField] private List<NavMeshSurface> additionalSurfaces = new List<NavMeshSurface>();
    
    [Header("Performance")]
    [SerializeField] private bool useChunkedBuild = true;
    [SerializeField] private int chunkSize = 500;
    
    private bool isGenerating = false;
    
    private void Awake()
    {
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
        if (generateOnStart && mainSurface != null)
        {
            StartCoroutine(GenerateNavMeshWithDelay());
        }
    }
    
    /// Waits for delay, then generates the NavMesh
    private IEnumerator GenerateNavMeshWithDelay()
    {
        if (isGenerating)
            yield break;
            
        isGenerating = true;
        
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
        
        // Track time for performance measurement
        float startTime = Time.realtimeSinceStartup;
        
        if (useChunkedBuild && mainSurface != null)
        {
            yield return StartCoroutine(BuildNavMeshInChunks());
        }
        else
        {
            // Build the main surface
            if (mainSurface != null)
            {
                mainSurface.BuildNavMesh();
            }
            
            // Build any additional surfaces
            foreach (var surface in additionalSurfaces)
            {
                if (surface != null)
                {
                    surface.BuildNavMesh();
                }
            }
        }
        
        // Calculate and log generation time
        float generationTime = Time.realtimeSinceStartup - startTime;
        
        if (showDebugInfo)
        {
            Debug.Log($"NavMesh generation process completed in {generationTime:F2} seconds.");
        }
        
        isGenerating = false;
    }
    
    /// Builds the NavMesh in chunks for better performance on large terrains
    private IEnumerator BuildNavMeshInChunks()
    {
        if (mainSurface == null) yield break;
        
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
        
        for (int x = 0; x < xChunks; x++)
        {
            for (int z = 0; z < zChunks; z++)
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
                
                processedChunks++;
                if (showDebugInfo && processedChunks % 5 == 0)
                {
                    Debug.Log($"NavMesh generation progress: {processedChunks}/{totalChunks} chunks");
                }
                
                // Yield every few chunks to prevent freezing
                if (processedChunks % 3 == 0)
                {
                    yield return null;
                }
            }
        }
        
        // Restore original size and center
        mainSurface.size = originalSize;
        mainSurface.center = originalCenter;
        
        Debug.Log($"Completed building NavMesh in {totalChunks} chunks");
    }
    
    /// Public method to manually trigger NavMesh generation
    public void GenerateNavMesh()
    {
        if (!isGenerating)
        {
            StartCoroutine(GenerateNavMeshWithDelay());
        }
        else
        {
            Debug.LogWarning("NavMesh generation already in progress");
        }
    }
    
    /// Clears all NavMesh data
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
        
        Debug.Log("NavMesh data cleared");
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