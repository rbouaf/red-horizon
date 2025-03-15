using System.Collections.Generic;
using UnityEngine;

public class DelaunayNavMesh : MonoBehaviour
{
    public Terrain terrain;
    public int pointCount = 500;
    public float walkableHeightOffset = 0.1f;
    public float maxSlopeAngle = 30f; // Maximum slope for walkable area
    public Material navMeshMaterial;

    private List<Vector2> samplePoints;
    private Mesh navMesh;
    
    void Start()
    {
        GenerateNavMesh();
    }

    void GenerateNavMesh()
    {
        samplePoints = GenerateSamplePoints(pointCount);
        List<Triangle> triangles = BowyerWatson(samplePoints);
        triangles = FilterSteepTriangles(triangles);
        CreateNavMesh(samplePoints, triangles);
    }

    List<Vector2> GenerateSamplePoints(int count)
    {
        List<Vector2> points = new List<Vector2>();
        for (int i = 0; i < count; i++)
        {
            float x = Random.Range(0, terrain.terrainData.size.x);
            float z = Random.Range(0, terrain.terrainData.size.z);
            points.Add(new Vector2(x, z));
        }
        return points;
    }

    List<Triangle> BowyerWatson(List<Vector2> points)
    {
        // Implement the Bowyer-Watson algorithm for Delaunay triangulation
        List<Triangle> triangles = new List<Triangle>();
        // TODO: Insert implementation here
        return triangles;
    }

    List<Triangle> FilterSteepTriangles(List<Triangle> triangles)
    {
        List<Triangle> filteredTriangles = new List<Triangle>();
        foreach (Triangle triangle in triangles)
        {
            Vector3 v1 = GetWorldPoint(triangle.p1);
            Vector3 v2 = GetWorldPoint(triangle.p2);
            Vector3 v3 = GetWorldPoint(triangle.p3);

            Vector3 normal = Vector3.Cross(v2 - v1, v3 - v1).normalized;
            float slopeAngle = Vector3.Angle(Vector3.up, normal);

            if (slopeAngle <= maxSlopeAngle)
            {
                filteredTriangles.Add(triangle);
            }
        }
        return filteredTriangles;
    }

    Vector3 GetWorldPoint(Vector2 point)
    {
        float y = terrain.SampleHeight(new Vector3(point.x, 0, point.y)) + walkableHeightOffset;
        return new Vector3(point.x, y, point.y);
    }

    void CreateNavMesh(List<Vector2> points, List<Triangle> triangles)
    {
        if (triangles.Count == 0)
        {
            Debug.LogError("No triangles generated! Check maxSlopeAngle and sample point distribution.");
            return;
        }

        navMesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> meshTriangles = new List<int>();
        Dictionary<Vector2, int> pointIndexMap = new Dictionary<Vector2, int>();
        
        foreach (Vector2 point in points)
        {
            Vector3 worldPoint = GetWorldPoint(point);
            pointIndexMap[point] = vertices.Count;
            vertices.Add(worldPoint);
        }
        
        foreach (Triangle triangle in triangles)
        {
            meshTriangles.Add(pointIndexMap[triangle.p1]);
            meshTriangles.Add(pointIndexMap[triangle.p2]);
            meshTriangles.Add(pointIndexMap[triangle.p3]);
        }
        
        navMesh.vertices = vertices.ToArray();
        navMesh.triangles = meshTriangles.ToArray();
        navMesh.RecalculateNormals();
        
        GameObject navMeshObject = new GameObject("GeneratedNavMesh");
        navMeshObject.transform.position = Vector3.zero;
        
        MeshFilter meshFilter = navMeshObject.AddComponent<MeshFilter>();
        meshFilter.mesh = navMesh;
        MeshRenderer meshRenderer = navMeshObject.AddComponent<MeshRenderer>();
        
        if (navMeshMaterial == null)
        {
            navMeshMaterial = new Material(Shader.Find("Standard"));
        }
        meshRenderer.material = navMeshMaterial;

        Debug.Log("NavMesh Generated: " + vertices.Count + " vertices, " + (meshTriangles.Count / 3) + " triangles");
    }
}

public class Triangle
{
    public Vector2 p1, p2, p3;
    public Triangle(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        this.p1 = p1;
        this.p2 = p2;
        this.p3 = p3;
    }
}
