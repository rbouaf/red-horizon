using UnityEngine;
using UnityEngine.AI;
using System;

/// <summary>
/// Manages the pre-baked NavMesh in the scene, and generates one at runtime if not found.
/// </summary>
public class NavMeshManager : MonoBehaviour
{
    public static NavMeshManager Instance { get; private set; }

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    // Status
    private bool isNavMeshReady = false;

    // Events
    public event Action OnNavMeshReady;
    public event Action<string> OnNavMeshGenerationFailed;

    private Unity.AI.Navigation.NavMeshSurface surface;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Get NavMeshSurface if it exists
        surface = GetComponent<Unity.AI.Navigation.NavMeshSurface>();
    }

    private void Start()
    {
        if (IsNavMeshPresent())
        {
            Debug.Log("[NavMeshManager] Using pre-baked NavMesh.");
            isNavMeshReady = true;
            OnNavMeshReady?.Invoke();
        }
        else
        {
            Debug.LogWarning("[NavMeshManager] No baked NavMesh found! Attempting runtime generation.");
            if (surface != null)
            {
                surface.BuildNavMesh();
                if (IsNavMeshPresent())
                {
                    Debug.Log("[NavMeshManager] Runtime NavMesh generation successful.");
                    isNavMeshReady = true;
                    OnNavMeshReady?.Invoke();
                }
                else
                {
                    Debug.LogError("[NavMeshManager] Runtime NavMesh generation failed.");
                    OnNavMeshGenerationFailed?.Invoke("Runtime NavMesh generation failed.");
                }
            }
            else
            {
                Debug.LogError("[NavMeshManager] No NavMeshSurface found for runtime generation.");
                OnNavMeshGenerationFailed?.Invoke("No NavMeshSurface for runtime generation.");
            }
        }
    }

    public bool IsNavMeshReady()
    {
        return isNavMeshReady;
    }

    public bool IsNavMeshPresent()
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(Vector3.zero, out hit, 500f, NavMesh.AllAreas))
            return true;

        if (NavMesh.SamplePosition(transform.position, out hit, 50f, NavMesh.AllAreas))
            return true;

        for (int x = -500; x <= 500; x += 200)
        {
            for (int z = -500; z <= 500; z += 200)
            {
                Vector3 testPos = new Vector3(x, 0, z);
                if (NavMesh.SamplePosition(testPos, out hit, 100f, NavMesh.AllAreas))
                    return true;
            }
        }

        return false;
    }

    public Vector3 GetClosestNavMeshPosition(Vector3 position, float maxDistance = 10f)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(position, out hit, maxDistance, NavMesh.AllAreas))
        {
            return hit.position;
        }

        Debug.LogWarning($"[NavMeshManager] Could not find NavMesh position near {position}");
        return position;
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 5f);
    }
}
