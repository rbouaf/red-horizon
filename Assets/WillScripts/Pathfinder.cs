using System.Collections.Generic;
using UnityEngine;

public class PathFinder : MonoBehaviour
{
    public Transform target;
    public float speed = 3f;
    
    private List<Vector3> path;
    private int currentWaypoint = 0;
    
    void Start()
    {
        // Find reference to your custom nav mesh
        MeshCollider navMeshCollider = GameObject.Find("GeneratedNavMesh").GetComponent<MeshCollider>();
        
        // Request path (using some custom path finding algorithm)
        path = FindPath(transform.position, target.position, navMeshCollider);
    }
    
    void Update()
    {
        if (path != null && path.Count > 0 && currentWaypoint < path.Count)
        {
            // Move along path
            transform.position = Vector3.MoveTowards(transform.position, path[currentWaypoint], speed * Time.deltaTime);
            
            if (Vector3.Distance(transform.position, path[currentWaypoint]) < 0.1f)
            {
                currentWaypoint++;
            }
        }
    }
    
    List<Vector3> FindPath(Vector3 start, Vector3 end, MeshCollider navMesh)
    {
        // Implement A* or other pathfinding algorithm here
        // ...
        return new List<Vector3>();
    }
}