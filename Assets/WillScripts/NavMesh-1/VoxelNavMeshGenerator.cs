using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VoxelNavMeshGenerator : MonoBehaviour
{
    // Configuration parameters
    public float voxelSize = 0.25f;
    public float agentHeight = 2f;
    public float agentRadius = 0.5f;
    public float maxSlope = 45f;
    public LayerMask walkableMask;
    public Material navMeshMaterial;
    public bool visualizeVoxels = false;
    
    // References to helper classes
    private VoxelGrid voxelGrid;
    private WalkableSurfaceDetector surfaceDetector;
    private RegionIdentifier regionIdentifier;
    private ContourExtractor contourExtractor;
    private NavMeshTriangulator triangulator;
    
    public void GenerateNavMesh()
    {
        // Create our voxel grid
        voxelGrid = new VoxelGrid(GetSceneBounds(), voxelSize);
        voxelGrid.Voxelize(walkableMask);
        
        if (visualizeVoxels) {
            voxelGrid.Visualize(transform);
        }
        
        // Detect walkable surfaces
        surfaceDetector = new WalkableSurfaceDetector(voxelGrid, agentHeight, agentRadius, maxSlope);
        surfaceDetector.Process();
        
        // Identify regions
        regionIdentifier = new RegionIdentifier(voxelGrid);
        Dictionary<int, List<Vector2Int>> regions = regionIdentifier.IdentifyRegions();
        
        // Extract contours
        contourExtractor = new ContourExtractor(voxelGrid, regions);
        List<List<Vector3>> contours = contourExtractor.ExtractContours();
        
        // Triangulate and create the final mesh
        triangulator = new NavMeshTriangulator();
        Mesh navMesh = triangulator.TriangulateContours(contours);
        
        // Create a GameObject with the mesh
        CreateNavMeshObject(navMesh);
    }
    
    private Bounds GetSceneBounds()
    {
        // Get bounds from colliders or terrain in the scene using the non-deprecated method
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        Collider[] colliders = FindObjectsByType<Collider>(FindObjectsSortMode.None);
        
        foreach (Collider collider in colliders)
        {
            if ((walkableMask.value & (1 << collider.gameObject.layer)) != 0)
            {
                bounds.Encapsulate(collider.bounds);
            }
        }
        return bounds;
    }
    
    private void CreateNavMeshObject(Mesh navMesh)
    {
        GameObject navMeshObject = new GameObject("GeneratedNavMesh");
        navMeshObject.transform.position = Vector3.zero;
        
        MeshFilter meshFilter = navMeshObject.AddComponent<MeshFilter>();
        meshFilter.mesh = navMesh;
        
        MeshRenderer meshRenderer = navMeshObject.AddComponent<MeshRenderer>();
        if (navMeshMaterial == null)
        {
            navMeshMaterial = new Material(Shader.Find("Standard"));
            navMeshMaterial.color = new Color(0f, 0.7f, 1f, 0.5f);
        }
        meshRenderer.material = navMeshMaterial;
        
        // Add collider for interactions
        MeshCollider collider = navMeshObject.AddComponent<MeshCollider>();
        collider.sharedMesh = navMesh;
    }
}