using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// A simplified AI brain for rover pathfinding using Unity's NavMesh system.
/// </summary>
public class PathfindingBrain : BrainBase
{
    [Header("Pathfinding Settings")]
    [SerializeField] private float waypointReachedDistance = 1.5f;
    [SerializeField] private float steeringSpeed = 30.0f;
    
    [Header("NavMesh Settings")]
    [SerializeField] private float navMeshPathUpdateInterval = 0.5f;
    
    [Header("Waypoints")]
    [SerializeField] private bool useCheckpoints = true;
    [SerializeField] private bool loopWaypoints = true;
    [SerializeField] private List<Transform> manualWaypoints = new List<Transform>();
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    [SerializeField] private bool drawPathGizmos = true;
    
    // Navigation data
    private NavMeshPath navMeshPath;
    private Vector3[] pathCorners = new Vector3[0];
    private List<Transform> waypoints = new List<Transform>();
    private int currentWaypointIndex = 0;
    private int currentPathIndex = 0;
    private Vector3 currentTarget;
    private bool isPathValid = false;
    private bool isNavigating = false;
    private bool hasReachedAllWaypoints = false;
    
    // Path recalculation
    private float lastPathCalculationTime = 0f;
    
    public override void Initialize(GameObject gameObject)
    {
        base.Initialize(gameObject);
        
        Debug.Log($"[PathfindingBrain] Initializing on GameObject: {gameObject.name}");
        
        // Initialize NavMesh path
        navMeshPath = new NavMeshPath();
        
        // Get and verify RoverController
        roverController = gameObject.GetComponent<RoverController>();
        
        if (roverController == null)
        {
            Debug.LogError("[PathfindingBrain] No RoverController found! Navigation will not work correctly.");
        }
        
        // Collect waypoints
        if (useCheckpoints)
        {
            Debug.Log("[PathfindingBrain] Using checkpoint mode to find waypoints");
            FindCheckpoints();
        }
        else
        {
            Debug.Log($"[PathfindingBrain] Using manual waypoints: {manualWaypoints.Count} defined");
            waypoints = manualWaypoints;
        }
        
        if (waypoints.Count > 0)
        {
            Debug.Log($"[PathfindingBrain] Found {waypoints.Count} waypoints, ready to navigate");
            currentStatus = "Ready to navigate";
            
            // Start navigation automatically
            StartNavigation();
        }
        else
        {
            Debug.LogError("[PathfindingBrain] No waypoints found or defined! Cannot navigate.");
            currentStatus = "No waypoints found";
        }
    }
    
    public override void Think()
    {
        // Skip if paused, no controller, not navigating or reached all waypoints
        if (isPaused() || roverController == null || !isNavigating || hasReachedAllWaypoints)
            return;
            
        // Check if we need to recalculate the path
        if (!isPathValid || Time.time > lastPathCalculationTime + navMeshPathUpdateInterval)
        {
            CalculateNavMeshPath();
        }
        
        // Navigation logic
        if (isPathValid && pathCorners != null && pathCorners.Length > 0)
        {
            // Update current target position from the path
            UpdateCurrentTarget();
            
            // Move rover toward current target
            MoveTowardsTarget();
            
            // Check if reached current path point
            if (HasReachedTarget(currentTarget))
            {
                if (currentPathIndex < pathCorners.Length - 1)
                {
                    // Move to next point in path
                    currentPathIndex++;
                    Debug.Log($"[PathfindingBrain] Moving to next path point: {currentPathIndex}/{pathCorners.Length-1}");
                }
                else
                {
                    // Reached end of current path segment
                    Debug.Log($"[PathfindingBrain] Reached end of path to waypoint {currentWaypointIndex + 1}");
                    OnReachedWaypoint();
                }
            }
        }
        else
        {
            // Stop the rover if no valid path
            if (roverController != null)
            {
                roverController.ApplyMovement(0);
                roverController.ApplySteering(0);
            }
        }
    }
    
    private void FindCheckpoints()
    {
        // Look for objects with "Checkpoint" tag
        GameObject[] checkpointObjects = GameObject.FindGameObjectsWithTag("Checkpoint");
        
        Debug.Log($"[PathfindingBrain] Searching for checkpoints with 'Checkpoint' tag...");
        
        if (checkpointObjects.Length > 0)
        {
            // Sort by name to ensure proper order
            System.Array.Sort(checkpointObjects, (a, b) => a.name.CompareTo(b.name));
            
            waypoints.Clear();
            foreach (GameObject checkpoint in checkpointObjects)
            {
                waypoints.Add(checkpoint.transform);
                Debug.Log($"[PathfindingBrain] Found checkpoint: {checkpoint.name} at position {checkpoint.transform.position}");
            }
        }
        else
        {
            Debug.LogWarning("[PathfindingBrain] No checkpoints found with 'Checkpoint' tag!");
            
            // Alternative approach - try finding objects with "checkpoint" in their name
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.ToLower().Contains("checkpoint") || obj.name.ToLower().Contains("waypoint"))
                {
                    waypoints.Add(obj.transform);
                    Debug.Log($"[PathfindingBrain] Found potential checkpoint by name: {obj.name}");
                }
            }
        }
    }
    
    public void StartNavigation()
    {
        if (waypoints.Count == 0)
        {
            Debug.LogError("Cannot start navigation: No waypoints defined");
            return;
        }
        
        // Check if NavMesh exists
        NavMeshHit hit;
        if (!NavMesh.SamplePosition(roverGameObject.transform.position, out hit, 5f, NavMesh.AllAreas))
        {
            Debug.LogError("Cannot start navigation: Rover not on NavMesh");
            currentStatus = "Error: Not on NavMesh";
            return;
        }
        
        currentWaypointIndex = 0;
        currentPathIndex = 0;
        isNavigating = true;
        hasReachedAllWaypoints = false;
        CalculateNavMeshPath();
        
        currentStatus = $"Navigating to waypoint {currentWaypointIndex + 1}/{waypoints.Count}";
    }
    
    public void StopNavigation()
    {
        isNavigating = false;
        
        // Stop the rover
        if (roverController != null)
        {
            roverController.ApplyMovement(0);
            roverController.ApplySteering(0);
        }
        
        currentStatus = "Navigation stopped";
    }
    
    private void CalculateNavMeshPath()
    {
        if (currentWaypointIndex >= waypoints.Count || waypoints[currentWaypointIndex] == null)
        {
            Debug.LogError($"[PathfindingBrain] Invalid waypoint index: {currentWaypointIndex}");
            isPathValid = false;
            return;
        }
        
        // Get current position and target waypoint
        Vector3 startPos = roverGameObject.transform.position;
        Vector3 targetPos = waypoints[currentWaypointIndex].position;
        
        // Check if start and end positions are on NavMesh
        NavMeshHit startHit, endHit;
        bool startOnNavMesh = NavMesh.SamplePosition(startPos, out startHit, 5f, NavMesh.AllAreas);
        bool endOnNavMesh = NavMesh.SamplePosition(targetPos, out endHit, 5f, NavMesh.AllAreas);
        
        if (!startOnNavMesh) startPos = startHit.position;
        if (!endOnNavMesh) targetPos = endHit.position;
        
        // Calculate path using NavMesh
        if (NavMesh.CalculatePath(startPos, targetPos, NavMesh.AllAreas, navMeshPath))
        {
            // Store path corners
            pathCorners = navMeshPath.corners;
            
            // Log path calculation
            Debug.Log($"[PathfindingBrain] Path calculated with {pathCorners.Length} corners");
            
            // Draw path in scene for debugging
            if (debugMode && pathCorners.Length > 0)
            {
                for (int i = 0; i < pathCorners.Length - 1; i++)
                {
                    Debug.DrawLine(pathCorners[i], pathCorners[i + 1], Color.green, 3.0f);
                }
            }
            
            // Reset current path index to start at the beginning
            currentPathIndex = 0;
            isPathValid = true;
            lastPathCalculationTime = Time.time;
        }
        else
        {
            Debug.LogError($"[PathfindingBrain] Failed to calculate NavMesh path!");
            isPathValid = false;
        }
    }
    
    private void UpdateCurrentTarget()
    {
        // Check if we have a valid path with points
        if (pathCorners == null || pathCorners.Length == 0 || currentPathIndex >= pathCorners.Length)
        {
            return;
        }
        
        // Set the current target to the current path corner
        currentTarget = pathCorners[currentPathIndex];
        
        // Draw debug visualization
        if (debugMode)
        {
            Debug.DrawLine(roverGameObject.transform.position, currentTarget, Color.red, Time.deltaTime);
        }
    }
    
    private void MoveTowardsTarget()
    {
        if (currentTarget == Vector3.zero || roverController == null)
            return;
            
        // Calculate direction to target (ignore Y axis for flat movement)
        Vector3 targetPos = new Vector3(currentTarget.x, 0, currentTarget.z);
        Vector3 currentPos = new Vector3(roverGameObject.transform.position.x, 0, roverGameObject.transform.position.z);
        Vector3 direction = (targetPos - currentPos).normalized;
        
        // Calculate angle between forward direction and target direction
        float targetAngle = Vector3.SignedAngle(roverGameObject.transform.forward, direction, Vector3.up);
        
        // Determine steering based on angle
        float steerInput = Mathf.Clamp(targetAngle / steeringSpeed, -1f, 1f);
        
        // Reduce throttle when turning sharply
        float throttleInput = 1.0f;
        
        // Apply steering and throttle
        roverController.ApplySteering(steerInput);
        roverController.ApplyMovement(throttleInput);
    }
    
    private bool HasReachedTarget(Vector3 target)
    {
        // Check distance to target in horizontal plane
        Vector3 currentPos = roverGameObject.transform.position;
        Vector3 targetPos = target;
        
        // Ignore Y axis for distance calculation
        currentPos.y = 0;
        targetPos.y = 0;
        
        float distanceToTarget = Vector3.Distance(currentPos, targetPos);
        
        return distanceToTarget <= waypointReachedDistance;
    }
    
    private void OnReachedWaypoint()
    {
        Debug.Log($"[PathfindingBrain] Reached waypoint {currentWaypointIndex + 1}");
        
        // Move to next waypoint
        currentWaypointIndex++;
        
        // Check if we've completed all waypoints
        if (currentWaypointIndex >= waypoints.Count)
        {
            if (loopWaypoints)
            {
                // Loop back to first waypoint
                currentWaypointIndex = 0;
                Debug.Log("[PathfindingBrain] Looping back to first waypoint");
            }
            else
            {
                // End navigation
                hasReachedAllWaypoints = true;
                currentStatus = "Completed all waypoints";
                
                // Stop the rover
                if (roverController != null)
                {
                    roverController.ApplyMovement(0);
                    roverController.ApplySteering(0);
                }
                
                return;
            }
        }
        
        // Calculate path to the next waypoint
        currentPathIndex = 0;
        CalculateNavMeshPath();
        currentStatus = $"Navigating to waypoint {currentWaypointIndex + 1}/{waypoints.Count}";
    }
    
    private void OnDrawGizmos()
    {
        if (!drawPathGizmos || !Application.isPlaying)
            return;
            
        // Draw NavMesh path
        if (isPathValid && pathCorners != null && pathCorners.Length > 0)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < pathCorners.Length - 1; i++)
            {
                Gizmos.DrawLine(pathCorners[i], pathCorners[i + 1]);
                Gizmos.DrawSphere(pathCorners[i], 0.2f);
            }
            
            // Draw last point
            Gizmos.DrawSphere(pathCorners[pathCorners.Length - 1], 0.2f);
            
            // Draw current target
            if (currentPathIndex < pathCorners.Length)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(currentTarget, 0.5f);
                Gizmos.DrawLine(roverGameObject.transform.position, currentTarget);
            }
        }
        
        // Draw waypoints
        Gizmos.color = Color.blue;
        foreach (Transform waypoint in waypoints)
        {
            if (waypoint != null)
            {
                Gizmos.DrawSphere(waypoint.position, 1f);
            }
        }
    }
}