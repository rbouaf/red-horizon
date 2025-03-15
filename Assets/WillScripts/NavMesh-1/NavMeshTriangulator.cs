using System.Collections.Generic;
using UnityEngine;

public class NavMeshTriangulator
{
    public Mesh TriangulateContours(List<List<Vector3>> contours)
    {
        List<Vector3> allVertices = new List<Vector3>();
        List<int> allTriangles = new List<int>();
        
        foreach (List<Vector3> contour in contours)
        {
            if (contour.Count < 3)
                continue;
                
            int baseIndex = allVertices.Count;
            allVertices.AddRange(contour);
            
            // Triangulate using ear clipping
            List<int> triangleIndices = EarClippingTriangulation(contour);
            
            foreach (int index in triangleIndices)
            {
                allTriangles.Add(baseIndex + index);
            }
        }
        
        Mesh navMesh = new Mesh();
        navMesh.vertices = allVertices.ToArray();
        navMesh.triangles = allTriangles.ToArray();
        navMesh.RecalculateNormals();
        
        return navMesh;
    }
    
    private List<int> EarClippingTriangulation(List<Vector3> polygon)
    {
        List<int> triangles = new List<int>();
        if (polygon.Count < 3)
            return triangles;
            
        // Create a copy of the polygon indices
        List<int> remainingVertices = new List<int>();
        for (int i = 0; i < polygon.Count; i++)
            remainingVertices.Add(i);
            
        // We'll iterate until we can't form any more triangles
        while (remainingVertices.Count > 3)
        {
            bool earFound = false;
            
            // Try to find an ear in the remaining vertices
            for (int i = 0; i < remainingVertices.Count; i++)
            {
                int prev = (i - 1 + remainingVertices.Count) % remainingVertices.Count;
                int curr = i;
                int next = (i + 1) % remainingVertices.Count;
                
                int prevIndex = remainingVertices[prev];
                int currIndex = remainingVertices[curr];
                int nextIndex = remainingVertices[next];
                
                Vector3 prevVertex = polygon[prevIndex];
                Vector3 currVertex = polygon[currIndex];
                Vector3 nextVertex = polygon[nextIndex];
                
                // Check if the vertex forms an ear
                if (IsEar(prevVertex, currVertex, nextVertex, polygon, remainingVertices))
                {
                    // Add the ear as a triangle
                    triangles.Add(prevIndex);
                    triangles.Add(currIndex);
                    triangles.Add(nextIndex);
                    
                    // Remove the ear tip from remaining vertices
                    remainingVertices.RemoveAt(curr);
                    earFound = true;
                    break;
                }
            }
            
            // If no ear was found, we have a problem with the polygon
            // This can happen with self-intersecting polygons
            if (!earFound)
            {
                Debug.LogWarning("Failed to find ear in polygon. Polygon may be self-intersecting.");
                break;
            }
        }
        
        // Add the final triangle
        if (remainingVertices.Count == 3)
        {
            triangles.Add(remainingVertices[0]);
            triangles.Add(remainingVertices[1]);
            triangles.Add(remainingVertices[2]);
        }
        
        return triangles;
    }
    
    private bool IsEar(Vector3 a, Vector3 b, Vector3 c, List<Vector3> polygon, List<int> remainingVertices)
    {
        // First, check if the vertex is convex
        if (!IsConvex(a, b, c))
            return false;
            
        // Then check if any other vertex is inside this triangle
        for (int i = 0; i < remainingVertices.Count; i++)
        {
            int vertexIndex = remainingVertices[i];
            Vector3 p = polygon[vertexIndex];
            
            // Skip vertices that form the triangle
            if (p == a || p == b || p == c)
                continue;
                
            if (IsPointInTriangle(p, a, b, c))
                return false;
        }
        
        return true;
    }
    
    private bool IsConvex(Vector3 a, Vector3 b, Vector3 c)
    {
        // Project to 2D (assuming polygon is roughly on a plane)
        Vector2 a2D = new Vector2(a.x, a.z);
        Vector2 b2D = new Vector2(b.x, b.z);
        Vector2 c2D = new Vector2(c.x, c.z);
        
        // Calculate the cross product of vectors (b-a) and (c-b)
        float cross = (b2D.x - a2D.x) * (c2D.y - b2D.y) - (b2D.y - a2D.y) * (c2D.x - b2D.x);
        
        // If cross product is positive, the vertex is convex
        return cross > 0;
    }
    
    private bool IsPointInTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
    {
        // Project to 2D (assuming polygon is roughly on a plane)
        Vector2 p2D = new Vector2(p.x, p.z);
        Vector2 a2D = new Vector2(a.x, a.z);
        Vector2 b2D = new Vector2(b.x, b.z);
        Vector2 c2D = new Vector2(c.x, c.z);
        
        // Compute barycentric coordinates
        float areaABC = Vector2Extensions.SignedArea(a2D, b2D, c2D);
        float areaPBC = Vector2Extensions.SignedArea(p2D, b2D, c2D);
        float areaPCA = Vector2Extensions.SignedArea(p2D, c2D, a2D);
        
        // Calculate barycentric coordinates
        float alpha = areaPBC / areaABC;
        float beta = areaPCA / areaABC;
        float gamma = 1.0f - alpha - beta;
        
        // Check if point is inside the triangle
        return alpha >= 0 && beta >= 0 && gamma >= 0;
    }
}

// Extension method for Vector2 to calculate signed area
public static class Vector2Extensions
{
    public static float SignedArea(Vector2 a, Vector2 b, Vector2 c)
    {
        return ((b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x)) / 2.0f;
    }
}