using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;


/*
Intended to use a Navmesh for Navigation but since the scene terrain is massive, it caused performance
 issues on laptops (not great when a demo is on a laptop...) So instead this is what is implemented,
 a script that plots a straight line between rover and next checkpoint, with no care if that checkpoint
 is in a massive pit or surrounded by a mountain range. It will go straight towards it..
 */
public class PathfindingBrain : BrainBase
{
    [Header("Simple Line Path Settings")]
    [SerializeField] private float waypointReachedDistance = 2f;
    [SerializeField] private int numberOfSegments = 5;
    [SerializeField] private float maxOffset = 2f;
    [SerializeField] private bool loopWaypoints = true;
    [SerializeField] private float checkpointDetectionDelay = 2f;

    [Header("Rock Avoidance Settings")]
    [SerializeField] private float rockDetectionDistance = 3f;
    [SerializeField] private float avoidanceSteeringStrength = 1.5f;
    [SerializeField] private float forwardDetectionOffset = 2f;
    [SerializeField] private List<string> rockTags = new List<string> { "Basaltic", "Andesitic", "Sedimentary", "Clay", "Quartz", "Volcaniclastic" };

    [Header("Debug")]
    [SerializeField] private bool debugMode = true;

    private List<Transform> checkpoints = new List<Transform>();
    private List<Vector3> path = new List<Vector3>();
    private int currentPathIndex = 0;
    private bool isNavigating = false;
    private bool checkpointsFound = false;
    private float avoidanceTimer = 0f;
    private float avoidanceDuration = 1.0f;
    private Vector3 avoidanceDirection = Vector3.zero;

    public override void Initialize(GameObject gameObject)
    {
        base.Initialize(gameObject);
        StartCoroutine(DelayedCheckpointSearch());
    }

    public override void Think()
    {
        if (isPaused() || !checkpointsFound)
            return;

        if (!isNavigating)
        {
            StartPathToNextCheckpoint();
        }

        if (path != null && currentPathIndex < path.Count)
        {
            if (AvoidRocks())
            {
                return; // Currently avoiding rock
            }

            MoveTowards(path[currentPathIndex]);

            if (Vector3.Distance(roverGameObject.transform.position, path[currentPathIndex]) <= waypointReachedDistance)
            {
                currentPathIndex++;
            }

            if (currentPathIndex >= path.Count)
            {
                OnReachedCheckpoint();
            }
        }
    }


    private IEnumerator DelayedCheckpointSearch()
    {
        Debug.Log("[PathfindingBrain] Waiting for checkpoints...");
        yield return new WaitForSeconds(checkpointDetectionDelay);
        FindCheckpoints();
    }

    private void FindCheckpoints()
    {
        checkpoints.Clear();
        GameObject[] checkpointObjects = GameObject.FindGameObjectsWithTag("Checkpoint");

        Debug.Log($"[PathfindingBrain] Found {checkpointObjects.Length} checkpoints.");

        if (checkpointObjects.Length > 0)
        {
            foreach (GameObject checkpoint in checkpointObjects)
            {
                checkpoints.Add(checkpoint.transform);
            }
            checkpointsFound = true;
        }
        else
        {
            Debug.LogError("[PathfindingBrain] No checkpoints found!");
        }
    }

    private void StartPathToNextCheckpoint()
    {
        if (checkpoints.Count == 0)
            return;

        SortCheckpointsByDistance(); // Sorts closest first

        Vector3 startPos = roverGameObject.transform.position;
        Vector3 checkpointCenter = GetCheckpointGroundPosition(checkpoints[0]);

        // --- NEW ---
        Vector3 toCheckpoint = (checkpointCenter - startPos);
        toCheckpoint.y = 0;
        Vector3 offsetDirection = toCheckpoint.normalized;

        float offsetDistance = 3.0f; // How far to stop *before* checkpoint center (tune this!)

        Vector3 adjustedEnd = checkpointCenter - offsetDirection * offsetDistance;
        adjustedEnd.y = checkpointCenter.y; // Keep same height after adjustment
        // ---

        GenerateRandomizedPath(startPos, adjustedEnd);
        currentPathIndex = 0;
        isNavigating = true;
    }


    private void GenerateRandomizedPath(Vector3 start, Vector3 end)
    {
        path.Clear();
        Vector3 direction = (end - start).normalized;
        float totalDistance = Vector3.Distance(start, end);
        float segmentLength = totalDistance / numberOfSegments;

        for (int i = 1; i <= numberOfSegments; i++)
        {
            Vector3 point = start + direction * segmentLength * i;

            Vector2 randomOffset = Random.insideUnitCircle * maxOffset;
            point.x += randomOffset.x;
            point.z += randomOffset.y;

            // ✨ Sample terrain height correctly for each point
            point.y = Terrain.activeTerrain.SampleHeight(new Vector3(point.x, 0, point.z)) 
                    + Terrain.activeTerrain.transform.position.y;

            path.Add(point);

            if (debugMode)
                Debug.Log($"[PathfindingBrain] Generated segment waypoint {i}: {point}");
        }

        // ✨ Re-sample terrain height for the final end position
        Vector3 finalEnd = end;
        finalEnd.y = Terrain.activeTerrain.SampleHeight(new Vector3(end.x, 0, end.z)) 
                + Terrain.activeTerrain.transform.position.y;

        path.Add(finalEnd);
    }



    private void MoveTowards(Vector3 target)
    {
        Vector3 dir = (target - roverGameObject.transform.position);
        dir.y = 0;
        dir.Normalize();

        float angle = Vector3.SignedAngle(roverGameObject.transform.forward, dir, Vector3.up);
        float steerInput = Mathf.Clamp(angle / 45f, -1f, 1f);

        roverController.ApplySteering(steerInput);
        roverController.ApplyMovement(1f);

        if (debugMode)
        {
            Debug.DrawLine(roverGameObject.transform.position, target, Color.green);
        }
    }

    private bool AvoidRocks()
    {
        if (avoidanceTimer > 0f)
        {
            roverController.ApplySteering(avoidanceDirection.x * avoidanceSteeringStrength);
            roverController.ApplyMovement(0.5f);
            avoidanceTimer -= Time.deltaTime;
            return true;
        }

        Vector3 scanOrigin = roverGameObject.transform.position + roverGameObject.transform.forward * forwardDetectionOffset;
        Collider[] hits = Physics.OverlapSphere(scanOrigin, rockDetectionDistance);

        foreach (Collider hit in hits)
        {
            if (rockTags.Contains(hit.tag))
            {
                if (debugMode)
                    Debug.Log($"[PathfindingBrain] Detected rock tagged {hit.tag}, initiating avoidance.");

                float side = Random.value > 0.5f ? 1f : -1f;
                avoidanceDirection = new Vector3(side, 0, 0);
                avoidanceTimer = avoidanceDuration;

                if (debugMode)
                    Debug.DrawLine(scanOrigin, hit.transform.position, Color.red, 2f);

                return true;
            }
        }

        return false;
    }

    private void OnReachedCheckpoint()
    {
        Debug.Log("[PathfindingBrain] Reached checkpoint.");

        isNavigating = false;

        if (loopWaypoints)
        {
            Transform first = checkpoints[0];
            checkpoints.RemoveAt(0);
            checkpoints.Add(first);
            Debug.Log("[PathfindingBrain] Looping checkpoints.");
        }
        else
        {
            checkpoints.RemoveAt(0);
        }
    }

    private void SortCheckpointsByDistance()
    {
        if (checkpoints.Count == 0)
            return;

        Vector3 roverPos = roverGameObject.transform.position;

        checkpoints.Sort((a, b) =>
        {
            float distA = Vector3.SqrMagnitude(a.position - roverPos);
            float distB = Vector3.SqrMagnitude(b.position - roverPos);
            return distA.CompareTo(distB);
        });
    }

    private Vector3 GetCheckpointGroundPosition(Transform checkpoint)
    {
        Vector3 origin = checkpoint.position + Vector3.up * 5f;
        Ray ray = new Ray(origin, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 20f))
        {
            return new Vector3(checkpoint.position.x, hit.point.y, checkpoint.position.z);
        }
        else if (Terrain.activeTerrain != null)
        {
            float terrainHeight = Terrain.activeTerrain.SampleHeight(checkpoint.position);
            return new Vector3(checkpoint.position.x, terrainHeight, checkpoint.position.z);
        }
        else
        {
            Debug.LogWarning("[PathfindingBrain] No ground found under checkpoint! Defaulting to original Y.");
            return checkpoint.position;
        }
    }

    private void OnDrawGizmos()
    {
        if (!debugMode || path == null)
            return;

        Gizmos.color = Color.magenta;
        for (int i = 0; i < path.Count; i++)
        {
            Gizmos.DrawSphere(path[i], 0.5f);

            if (i < path.Count - 1)
                Gizmos.DrawLine(path[i], path[i + 1]);
        }
    }
}
