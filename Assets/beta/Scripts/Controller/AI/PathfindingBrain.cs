using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

// PathfindingBrain controls rover navigation using NavMesh while avoiding rocks dynamically.
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

    [Header("Rock Avoidance Settings")]
    [SerializeField] private float rockDetectionDistance = 2.0f;
    [SerializeField] private float rockAvoidanceSteering = 1.0f;
    [SerializeField] private List<string> rockTags = new List<string> { "Basaltic", "Andesitic", "Sedimentary", "Clay", "Quartz", "Volcaniclastic" };
    [SerializeField] private float forwardDetectionOffset = 3.0f;
    private float avoidanceSteeringTimer = 0f;
    private float avoidanceSteeringDuration = 1.0f;
    private Vector3 lastAvoidanceVector = Vector3.zero;


    [Header("Checkpoint Detection")]
    [SerializeField] private float checkpointDetectionDelay = 2.0f; // Wait time before searching for checkpoints
    [SerializeField] private float checkpointRefreshInterval = 5.0f; // How often to refresh checkpoint search
    [SerializeField] private int maxCheckpointSearchAttempts = 5; // Maximum number of attempts to find checkpoints
    [SerializeField] private LayerMask terrainLayerMask = -1; // Default to all layers for terrain detection

    [Header("Debug")]
    [SerializeField] private bool debugMode = true;

    private NavMeshPath navMeshPath;
    private Vector3[] pathCorners = new Vector3[0];
    private List<Transform> waypoints = new List<Transform>();
    private int currentWaypointIndex = 0;
    private int currentPathIndex = 0;
    private Vector3 currentTarget;
    private bool isPathValid = false;
    private bool isNavigating = false;
    private bool hasReachedAllWaypoints = false;
    private float lastPathCalculationTime = 0f;
    private bool isAvoidingRock = false;
    private int checkpointSearchAttempts = 0;
    private bool checkpointsFound = false;

    public override void Initialize(GameObject gameObject)
    {
        base.Initialize(gameObject);
        Debug.Log("[PathfindingBrain] Initializing...");
        navMeshPath = new NavMeshPath();

        // Auto-detect terrain layer if not set
        if (terrainLayerMask.value == -1 && Terrain.activeTerrain != null)
        {
            terrainLayerMask = 1 << Terrain.activeTerrain.gameObject.layer;
            Debug.Log($"[PathfindingBrain] Auto-detected terrain layer: {Terrain.activeTerrain.gameObject.layer}");
        }

        // For manually specified waypoints
        if (!useCheckpoints && manualWaypoints.Count > 0)
        {
            waypoints = manualWaypoints;
            checkpointsFound = true;
            currentStatus = "Ready to navigate (manual waypoints)";
            StartCoroutine(DelayedNavigationStart(1.0f));
        }
        else
        {
            // For dynamically created checkpoints, start a delayed search
            StartCoroutine(DelayedCheckpointSearch());
        }
    }

    private IEnumerator DelayedCheckpointSearch()
    {
        // Wait for checkpoints to be instantiated
        Debug.Log($"[PathfindingBrain] Waiting {checkpointDetectionDelay}s for checkpoints to be instantiated...");
        yield return new WaitForSeconds(checkpointDetectionDelay);

        while (!checkpointsFound && checkpointSearchAttempts < maxCheckpointSearchAttempts)
        {
            checkpointSearchAttempts++;
            Debug.Log($"[PathfindingBrain] Searching for checkpoints (attempt {checkpointSearchAttempts}/{maxCheckpointSearchAttempts})...");
            
            FindCheckpoints();
            
            if (waypoints.Count > 0)
            {
                checkpointsFound = true;
                currentStatus = $"Found {waypoints.Count} checkpoints, ready to navigate";
                StartCoroutine(DelayedNavigationStart(1.0f));
                break;
            }
            else
            {
                currentStatus = $"Searching for checkpoints ({checkpointSearchAttempts}/{maxCheckpointSearchAttempts})";
                yield return new WaitForSeconds(checkpointRefreshInterval);
            }
        }

        if (!checkpointsFound)
        {
            Debug.LogError($"[PathfindingBrain] Failed to find any checkpoints after {maxCheckpointSearchAttempts} attempts!");
            currentStatus = "ERROR: No checkpoints found";
        }
    }

    private IEnumerator DelayedNavigationStart(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartNavigation();
    }

    public override void Think()
    {
        if (isPaused() || roverController == null || !isNavigating || hasReachedAllWaypoints || !checkpointsFound)
            return;

        if (avoidanceSteeringTimer > 0f)
        {
            roverController.ApplyBrakes(0.05f);
            float steerAmount = Mathf.Clamp(Vector3.SignedAngle(roverGameObject.transform.forward, lastAvoidanceVector, Vector3.up) / 30f, -1f, 1f);

            roverController.ApplySteering(steerAmount * rockAvoidanceSteering);
            roverController.ApplyMovement(0.5f);

            avoidanceSteeringTimer -= Time.deltaTime;
            return; 
        }

        if (!isPathValid || Time.time > lastPathCalculationTime + navMeshPathUpdateInterval)
        {
            CalculateNavMeshPath();
        }

        if (isPathValid && pathCorners != null && pathCorners.Length > 0)
        {
            UpdateCurrentTarget();
            isAvoidingRock = AvoidRocks();

            if (!isAvoidingRock)
            {
                MoveTowardsTarget();
            }

            if (!isAvoidingRock && HasReachedTarget(currentTarget))
            {
                if (currentPathIndex < pathCorners.Length - 1)
                {
                    currentPathIndex++;
                }
                else
                {
                    OnReachedWaypoint();
                }
            }
        }
        else
        {
            roverController.ApplyMovement(0);
            roverController.ApplySteering(0);
        }
    }

    private bool AvoidRocks()
    {
        Vector3 scanOrigin = roverGameObject.transform.position + roverGameObject.transform.forward * forwardDetectionOffset;
        Collider[] colliders = Physics.OverlapSphere(scanOrigin, rockDetectionDistance);

        Vector3 seekForce = (currentTarget - roverGameObject.transform.position);
        seekForce.y = 0;
        seekForce = seekForce.normalized;

        Vector3 avoidForce = Vector3.zero;
        int rocksDetected = 0;
        float closestRockDistance = float.MaxValue;

        foreach (Collider col in colliders)
        {
            if (rockTags.Contains(col.tag))
            {
                Vector3 toRock = (col.transform.position - scanOrigin);
                toRock.y = 0;
                toRock = toRock.normalized;

                Vector3 lateralAvoidance = Vector3.Cross(Vector3.up, toRock).normalized;

                float distance = Vector3.Distance(scanOrigin, col.transform.position);
                float weight = Mathf.Clamp01((rockDetectionDistance - distance) / rockDetectionDistance);

                avoidForce += lateralAvoidance * weight;
                rocksDetected++;

                if (distance < closestRockDistance)
                    closestRockDistance = distance;
            }
        }

        if (rocksDetected > 0)
        {   
            roverController.ApplyBrakes(1.0f);
            Vector3 finalForce = (seekForce + avoidForce * 2f);
            finalForce.y = 0;
            finalForce = finalForce.normalized;

            lastAvoidanceVector = finalForce;
            avoidanceSteeringTimer = avoidanceSteeringDuration;

            float steerAmount = Mathf.Clamp(Vector3.SignedAngle(roverGameObject.transform.forward, finalForce, Vector3.up) / 30f, -1f, 1f);

            roverController.ApplySteering(steerAmount * rockAvoidanceSteering);
            roverController.ApplyMovement(0.5f);

            return true;
        }

        return false;
    }


    private void FindCheckpoints()
    {
        GameObject[] checkpointObjects = GameObject.FindGameObjectsWithTag("Checkpoint");
        
        Debug.Log($"[PathfindingBrain] Found {checkpointObjects.Length} objects with Checkpoint tag");
        
        if (checkpointObjects.Length > 0)
        {
            System.Array.Sort(checkpointObjects, (a, b) => a.name.CompareTo(b.name));
            waypoints.Clear();
            
            foreach (GameObject checkpoint in checkpointObjects)
            {
                waypoints.Add(checkpoint.transform);
                Debug.Log($"[PathfindingBrain] Added checkpoint: {checkpoint.name} at {checkpoint.transform.position}");
            }
        }
        else
        {
            // If checkpoint tag is not found, try other methods
            FindCheckpointsByName();
        }
    }
    
    private void FindCheckpointsByName()
    {
        // Look for objects with checkpoint in their name
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int foundCheckpoints = 0;
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.ToLower().Contains("checkpoint") || 
                obj.name.ToLower().Contains("waypoint") ||
                obj.name.ToLower().Contains("navsample"))
            {
                waypoints.Add(obj.transform);
                foundCheckpoints++;
                Debug.Log($"[PathfindingBrain] Found checkpoint by name: {obj.name} at {obj.transform.position}");
            }
        }
        
        if (foundCheckpoints > 0)
        {
            // Sort the waypoints based on their names for consistent ordering
            waypoints.Sort((a, b) => a.name.CompareTo(b.name));
            Debug.Log($"[PathfindingBrain] Found {foundCheckpoints} checkpoints by name search");
        }
    }

    private void StartNavigation()
    {
        if (waypoints.Count == 0)
        {
            Debug.LogError("[PathfindingBrain] Cannot start navigation: No waypoints defined");
            return;
        }

        NavMeshHit hit;
        if (!NavMesh.SamplePosition(roverGameObject.transform.position, out hit, 5f, NavMesh.AllAreas))
        {
            Debug.LogWarning("[PathfindingBrain] Rover not on NavMesh - navigation may be limited");
            currentStatus = "Warning: Not on NavMesh";
        }

        currentWaypointIndex = 0;
        currentPathIndex = 0;
        isNavigating = true;
        hasReachedAllWaypoints = false;
        CalculateNavMeshPath();
        
        currentStatus = $"Navigating to waypoint {currentWaypointIndex + 1}/{waypoints.Count}";
        Debug.Log($"[PathfindingBrain] Started navigation with {waypoints.Count} waypoints");
    }

    private void CalculateNavMeshPath()
    {
        if (currentWaypointIndex >= waypoints.Count || waypoints[currentWaypointIndex] == null)
        {
            Debug.LogError($"[PathfindingBrain] Invalid waypoint index: {currentWaypointIndex}");
            isPathValid = false;
            return;
        }

        Vector3 startPos = roverGameObject.transform.position;
        Transform waypointTransform = waypoints[currentWaypointIndex];
        Vector3 waypointPos = waypointTransform.position;
        
        // Find the terrain height at the checkpoint location
        float terrainY = SampleTerrainHeightAt(waypointPos.x, waypointPos.z);
        
        // Create target position at the terrain height
        Vector3 targetPos = new Vector3(waypointPos.x, terrainY, waypointPos.z);
        Debug.Log($"[PathfindingBrain] Waypoint {currentWaypointIndex + 1}: original pos {waypointPos}, target pos {targetPos}");

        // Find nearest points on NavMesh for both start and end positions
        NavMeshHit startHit, endHit;
        bool startOnNavMesh = NavMesh.SamplePosition(startPos, out startHit, 5f, NavMesh.AllAreas);
        bool endOnNavMesh = NavMesh.SamplePosition(targetPos, out endHit, 10f, NavMesh.AllAreas);

        if (!startOnNavMesh)
        {
            Debug.LogWarning($"[PathfindingBrain] Start position not on NavMesh! Using nearest point.");
            startPos = startHit.position;
        }
        
        if (!endOnNavMesh)
        {
            Debug.LogWarning($"[PathfindingBrain] Waypoint position not on NavMesh! Using nearest point.");
            targetPos = endHit.position;
        }
        else
        {
            // Use the NavMesh height to ensure we're on valid ground
            targetPos = endHit.position;
        }

        // Calculate path using NavMesh
        if (NavMesh.CalculatePath(startPos, targetPos, NavMesh.AllAreas, navMeshPath))
        {
            pathCorners = navMeshPath.corners;
            
            if (pathCorners.Length == 0)
            {
                Debug.LogError("[PathfindingBrain] Path calculation returned zero corners!");
                isPathValid = false;
                return;
            }
            
            // Log path calculation success
            Debug.Log($"[PathfindingBrain] Path calculated with {pathCorners.Length} corners");
            
            // Draw path in scene for debugging
            if (debugMode && pathCorners.Length > 0)
            {
                for (int i = 0; i < pathCorners.Length - 1; i++)
                {
                    Debug.DrawLine(pathCorners[i], pathCorners[i + 1], Color.green, navMeshPathUpdateInterval);
                }
            }
            
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

    private float SampleTerrainHeightAt(float x, float z)
    {
        // First try using active terrain
        Terrain terrain = Terrain.activeTerrain;
        if (terrain != null)
        {
            Vector3 terrainPos = terrain.transform.position;
            float height = terrain.SampleHeight(new Vector3(x, 0, z)) + terrainPos.y;
            return height;
        }
        
        // If no terrain, try raycast to find ground
        RaycastHit hit;
        if (Physics.Raycast(new Vector3(x, 1000, z), Vector3.down, out hit, 2000f, terrainLayerMask))
        {
            Debug.Log($"[PathfindingBrain] Raycast hit ground at y = {hit.point.y}");
            return hit.point.y;
        }
        
        Debug.LogWarning("[PathfindingBrain] Failed to determine terrain height! Using default Y = 0.");
        return 0f;
    }

    private void UpdateCurrentTarget()
    {
        if (pathCorners == null || pathCorners.Length == 0 || currentPathIndex >= pathCorners.Length)
            return;

        currentTarget = pathCorners[currentPathIndex];
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
        float angleFactor = 1.0f - (Mathf.Abs(steerInput) * 0.5f);
        float throttleInput = angleFactor;
        
        // Apply steering and throttle
        roverController.ApplySteering(steerInput);
        roverController.ApplyMovement(throttleInput);
    }

    private bool HasReachedTarget(Vector3 target)
    {
        Vector3 currentPos = roverGameObject.transform.position;
        currentPos.y = 0;
        Vector3 flatTarget = target;
        flatTarget.y = 0;

        return Vector3.Distance(currentPos, flatTarget) <= waypointReachedDistance;
    }

    private void OnReachedWaypoint()
    {
        Debug.Log($"[PathfindingBrain] Reached waypoint {currentWaypointIndex + 1}");
        
        // Move to next waypoint
        currentWaypointIndex++;

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
                roverController.ApplyMovement(0);
                roverController.ApplySteering(0);
                currentStatus = "Completed all waypoints";
                return;
            }
        }

        // Calculate path to the next waypoint
        currentPathIndex = 0;
        CalculateNavMeshPath();
        currentStatus = $"Navigating to waypoint {currentWaypointIndex + 1}/{waypoints.Count}";
    }

    // Forces a rescan for checkpoints
    public void RescanForCheckpoints()
    {
        checkpointSearchAttempts = 0;
        checkpointsFound = false;
        StartCoroutine(DelayedCheckpointSearch());
    }

    private void OnDrawGizmos()
    {
        if (!debugMode || !Application.isPlaying)
            return;

        if (isPathValid && pathCorners != null && pathCorners.Length > 0)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < pathCorners.Length - 1; i++)
            {
                Gizmos.DrawLine(pathCorners[i], pathCorners[i + 1]);
                Gizmos.DrawSphere(pathCorners[i], 0.2f);
            }
            Gizmos.DrawSphere(pathCorners[pathCorners.Length - 1], 0.2f);
        }

        if (lastAvoidanceVector != Vector3.zero && roverGameObject != null)
        {
            Gizmos.color = Color.magenta;
            Vector3 origin = roverGameObject.transform.position + roverGameObject.transform.forward * forwardDetectionOffset;
            Gizmos.DrawLine(origin, origin + lastAvoidanceVector * 5f);
            Gizmos.DrawSphere(origin + lastAvoidanceVector * 5f, 0.2f);
        }

        if (roverGameObject != null)
        {
            Gizmos.color = Color.red;
            Vector3 scanOrigin = roverGameObject.transform.position + roverGameObject.transform.forward * forwardDetectionOffset;
            Gizmos.DrawWireSphere(scanOrigin, rockDetectionDistance);
        }

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