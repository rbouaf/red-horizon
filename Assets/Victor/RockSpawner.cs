using UnityEngine;

public class RockSpawner : MonoBehaviour
{
    [Header("Terrain Settings")]

    public Terrain terrain;

    [Header("Rock Prefabs")]
    public GameObject basalticPrefab;
    public GameObject andesiticPrefab;
    public GameObject sedimentaryPrefab;
    public GameObject clayPrefab;
    public GameObject quartzPrefab;
    public GameObject volcaniclasticPrefab;

    [Header("Rock Rarity (Concentration)")]
    public float basalticConcentration = 0.9f;
    public float andesiticConcentration = 0.7f;
    public float sedimentaryConcentration = 0.3f;
    public float clayConcentration = 0.4f;
    public float quartzConcentration = 0.05f;
    public float volcaniclasticConcentration = 0.1f;

    [Header("Spawn Settings")]
    //  to scale the spawn count based on the rarity value.
    public float spawnMultiplier = 100f;

    void Start()
    {
        SpawnRocks();
    }

    void SpawnRocks()
    {
        // Get terrain boundaries for placement.
        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainSize = terrainData.size;
        Vector3 terrainPos = terrain.transform.position;

        
        SpawnRockType(basalticPrefab, basalticConcentration, terrainPos, terrainSize);
        SpawnRockType(andesiticPrefab, andesiticConcentration, terrainPos, terrainSize);
        SpawnRockType(sedimentaryPrefab, sedimentaryConcentration, terrainPos, terrainSize);
        SpawnRockType(clayPrefab, clayConcentration, terrainPos, terrainSize);
        SpawnRockType(quartzPrefab, quartzConcentration, terrainPos, terrainSize);
        SpawnRockType(volcaniclasticPrefab, volcaniclasticConcentration, terrainPos, terrainSize);
    }

    void SpawnRockType(GameObject prefab, float concentration, Vector3 terrainPos, Vector3 terrainSize)
    {
        // Calculate how many rocks to spawn.
        int count = Mathf.RoundToInt(concentration * spawnMultiplier);
        for (int i = 0; i < count; i++)
        {
            // pick a random position on the terrain.
            float x = Random.Range(terrainPos.x, terrainPos.x + terrainSize.x);
            float z = Random.Range(terrainPos.z, terrainPos.z + terrainSize.z);
            // Use the terrain's height to place the rock on the surface.
            float y = terrain.SampleHeight(new Vector3(x, 0, z)) + terrainPos.y;
            Vector3 spawnPosition = new Vector3(x, y, z);
            
            
            Instantiate(prefab, spawnPosition, Quaternion.identity);
        }
    }
}
