using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

// PathfindingBrain controls rover navigation using NavMesh while avoiding rocks dynamically.
public class PathfindingBrain : BrainBase
{
    // --- Settings ---

    [Header("Pathfinding Settings")]
    [SerializeField] private float waypointReachedDistance = 1.5f;  // Distance at which a waypoint is considered "reached"
    [SerializeField] private float steeringSpeed = 30.0f;           // How quickly the rover can steer

    [Header("NavMesh Settings")]
    [SerializeField] private float navMeshPathUpdateInterval = 0.5f;  // How often the path is recalculated

    [Header("Waypoints")]
    [SerializeField] private bool useCheckpoints = true;              // Whether to use dynamically generated checkpoints
    [SerializeField] private bool loopWaypoints = true;               // Whether to loop back after reaching the last waypoint
    [SerializeField] private List<Transform> manualWaypoints = new List<Transform>();  // Manually set waypoints (if not using checkpoints)

    [Header("Rock Avoidance Settings")]
    [SerializeField] private float rockDetectionDistance = 2.0f;        // Radius to detect nearby rocks
    [SerializeField] private float rockAvoidanceSteering = 1.0f;         // Steering strength when avoiding rocks
    [SerializeField] private List<string> rockTags = new List<string> { "Basaltic", "Andesitic", "Sedimentary", "Clay", "Quartz", "Volcaniclastic" }; // Recognized rock tags
    [SerializeField] private float forwardDetectionOffset = 3.0f;        // Distance in front of the rover to start detecting rocks
    private float avoidanceSteeringTimer = 0f;                           // Timer controlling avoidance steering
    private float avoidanceSteeringDuration = 1.0f;                      // How long avoidance steering lasts
    private Vector3 lastAvoidanceVector = Vector3.zero;                  // Last calculated avoidance direction

    [Header("Checkpoint Detection")]
    [SerializeField] private float checkpointDetectionDelay = 2.0f;   // Time to wait for checkpoints to spawn
    [SerializeField] private float checkpointRefreshInterval = 5.0f;  // Time between checkpoint search retries
    [SerializeField] private int maxCheckpointSearchAttempts = 5;     // Maximum attempts to find checkpoints
    [SerializeField] private LayerMask terrainLayerMask = -1;         // Terrain layer used for raycasting

    [Header("Debug")]
    [SerializeField] private bool debugMode = true;                  // Enables extra debug logging and gizmos

    // --- Internal State ---

    private NavMeshPath navMeshPath;          // Stores the current calculated NavMesh path
    private Vector3[] pathCorners = new Vector3[0]; // Corner points along the current path
    private List<Transform> waypoints = new List<Transform>(); // List of all active waypoints
    private int currentWaypointIndex = 0;     // Which waypoint we're currently targeting
    private int currentPathIndex = 0;         // Which corner we're currently targeting
    private Vector3 currentTarget;            // Current move target
    private bool isPathValid = false;         // Whether the current path is valid
    private bool isNavigating = false;        // Whether rover is currently navigating
    private bool hasReachedAllWaypoints = false; // True if all waypoints were reached
    private float lastPathCalculationTime = 0f;  // Last time we recalculated the path
    private bool isAvoidingRock = false;          // Whether currently performing rock avoidance
    private int checkpointSearchAttempts = 0;     // Number of times we've searched for checkpoints
    private bool checkpointsFound = false;        // Whether valid checkpoints were found

    // --- Initialization ---

    public override void Initialize(GameObject gameObject)
    {
        base.Initialize(gameObject);
        Debug.Log("[PathfindingBrain] Initializing...");
        navMeshPath = new NavMeshPath();

        // Auto-detect the terrain layer if not set
        if (terrainLayerMask.value == -1 && Terrain.activeTerrain != null)
        {
            terrainLayerMask = 1 << Terrain.activeTerrain.gameObject.layer;
            Debug.Log($"[PathfindingBrain] Auto-detected terrain layer: {Terrain.activeTerrain.gameObject.layer}");
        }

        if (!useCheckpoints && manualWaypoints.Count > 0)
        {
            // Using manually specified waypoints
            waypoints = manualWaypoints;
            checkpointsFound = true;
            currentStatus = "Ready to navigate (manual waypoints)";
            StartCoroutine(DelayedNavigationStart(1.0f));
        }
        else
        {
            // Dynamically find checkpoints after a short delay
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

    // Main Update Logic (Called every frame by RoverBrainManager) ---
    public override void Think()
    {
        // Early exit if paused or in invalid states
        if (isPaused() || roverController == null || !isNavigating || hasReachedAllWaypoints || !checkpointsFound)
            return;

        // If we're currently avoiding a rock, continue steering to avoid it
        if (avoidanceSteeringTimer > 0f)
        {
            roverController.ApplyBrakes(0.05f); // Light braking
            float steerAmount = Mathf.Clamp(Vector3.SignedAngle(roverGameObject.transform.forward, lastAvoidanceVector, Vector3.up) / 30f, -1f, 1f);

            roverController.ApplySteering(steerAmount * rockAvoidanceSteering);
            roverController.ApplyMovement(0.5f);

            avoidanceSteeringTimer -= Time.deltaTime;
            return; 
        }

        // If no valid path or time to update, calculate new path
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
            // Braking and evasive steering when rocks are detected
            roverController.ApplyBrakes(1.0f);
            Vector3 finalForce = (seekForce + avoidForce * 2f).normalized;

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
        // Find objects tagged as "Checkpoint" in the scene
        GameObject[] checkpointObjects = GameObject.FindGameObjectsWithTag("Checkpoint");

        Debug.Log($"[PathfindingBrain] Found {checkpointObjects.Length} objects with Checkpoint tag");

        if (checkpointObjects.Length > 0)
        {
            // Sort checkpoints alphabetically by name for consistent order
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
            // If no tagged checkpoints found, fallback to searching by name
            FindCheckpointsByName();
        }
    }
    
    private void FindCheckpointsByName()
    {
        // Search all GameObjects for "checkpoint", "waypoint" or "navsample" in their name
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
            // Sort waypoints alphabetically for deterministic navigation
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

        // Check if rover is on the NavMesh
        NavMeshHit hit;
        if (!NavMesh.SamplePosition(roverGameObject.transform.position, out hit, 5f, NavMesh.AllAreas))
        {
            Debug.LogWarning("[PathfindingBrain] Rover not on NavMesh - navigation may be limited");
            currentStatus = "Warning: Not on NavMesh";
        }

        // Reset navigation variables
        currentWaypointIndex = 0;
        currentPathIndex = 0;
        isNavigating = true;
        hasReachedAllWaypoints = false;

        // Calculate path to first waypoint
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
        
        // Adjust waypoint Y position to terrain height
        float terrainY = SampleTerrainHeightAt(waypointPos.x, waypointPos.z);
        Vector3 targetPos = new Vector3(waypointPos.x, terrainY, waypointPos.z);

        // Snap start and target to NavMesh
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
            targetPos = endHit.position;
        }

        // Actually calculate the NavMesh path
        if (NavMesh.CalculatePath(startPos, targetPos, NavMesh.AllAreas, navMeshPath))
        {
            pathCorners = navMeshPath.corners;

            if (pathCorners.Length == 0)
            {
                Debug.LogError("[PathfindingBrain] Path calculation returned zero corners!");
                isPathValid = false;
                return;
            }

            // Optional: draw the path lines for debug visualization
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
        // Try to sample height from active terrain
        Terrain terrain = Terrain.activeTerrain;
        if (terrain != null)
        {
            Vector3 terrainPos = terrain.transform.position;
            float height = terrain.SampleHeight(new Vector3(x, 0, z)) + terrainPos.y;
            return height;
        }
        
        // If no terrain found, use physics raycast
        RaycastHit hit;
        if (Physics.Raycast(new Vector3(x, 1000, z), Vector3.down, out hit, 2000f, terrainLayerMask))
        {
            Debug.Log($"[PathfindingBrain] Raycast hit ground at y = {hit.point.y}");
            return hit.point.y;
        }
        
        // If all fails, return default height
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
        
        // Calculate a flat 2D direction to target
        Vector3 targetPos = new Vector3(currentTarget.x, 0, currentTarget.z);
        Vector3 currentPos = new Vector3(roverGameObject.transform.position.x, 0, roverGameObject.transform.position.z);
        Vector3 direction = (targetPos - currentPos).normalized;
        
        // Calculate steering angle
        float targetAngle = Vector3.SignedAngle(roverGameObject.transform.forward, direction, Vector3.up);
        float steerInput = Mathf.Clamp(targetAngle / steeringSpeed, -1f, 1f);

        // Reduce speed when sharply turning
        float angleFactor = 1.0f - (Mathf.Abs(steerInput) * 0.5f);
        float throttleInput = angleFactor;

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
                currentWaypointIndex = 0;
                Debug.Log("[PathfindingBrain] Looping back to first waypoint");
            }
            else
            {
                // All waypoints completed
                hasReachedAllWaypoints = true;
                roverController.ApplyMovement(0);
                roverController.ApplySteering(0);
                currentStatus = "Completed all waypoints";
                return;
            }
        }

        // Recalculate path to the next waypoint
        currentPathIndex = 0;
        CalculateNavMeshPath();
        currentStatus = $"Navigating to waypoint {currentWaypointIndex + 1}/{waypoints.Count}";
    }

    // --- Public Helper ---

    public void RescanForCheckpoints()
    {
        checkpointSearchAttempts = 0;
        checkpointsFound = false;
        StartCoroutine(DelayedCheckpointSearch());
    }

    // --- Debug Visualization (Gizmos) ---

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
