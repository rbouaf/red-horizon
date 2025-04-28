using UnityEngine;

public class TerrainWallGenerator : MonoBehaviour
{
    public Terrain terrain;

    public Transform wallLeft;
    public Transform wallRight;
    public Transform wallTop;
    public Transform wallBottom;

    public float wallHeight = 100f;
    public float wallThickness = 2f;

    private void Start()
    {
        if (terrain == null)
        {
            terrain = FindObjectOfType<Terrain>();
        }

        if (terrain == null)
        {
            Debug.LogError("No terrain found in scene!");
            return;
        }

        Vector3 terrainPos = terrain.transform.position;
        Vector3 terrainSize = terrain.terrainData.size;

        // Position and scale walls
        wallLeft.position = new Vector3(terrainPos.x - wallThickness/2, terrainPos.y + wallHeight/2, terrainPos.z + terrainSize.z/2);
        wallLeft.localScale = new Vector3(wallThickness, wallHeight, terrainSize.z + 2 * wallThickness);

        wallRight.position = new Vector3(terrainPos.x + terrainSize.x + wallThickness/2, terrainPos.y + wallHeight/2, terrainPos.z + terrainSize.z/2);
        wallRight.localScale = new Vector3(wallThickness, wallHeight, terrainSize.z + 2 * wallThickness);

        wallTop.position = new Vector3(terrainPos.x + terrainSize.x/2, terrainPos.y + wallHeight/2, terrainPos.z + terrainSize.z + wallThickness/2);
        wallTop.localScale = new Vector3(terrainSize.x + 2 * wallThickness, wallHeight, wallThickness);

        wallBottom.position = new Vector3(terrainPos.x + terrainSize.x/2, terrainPos.y + wallHeight/2, terrainPos.z - wallThickness/2);
        wallBottom.localScale = new Vector3(terrainSize.x + 2 * wallThickness, wallHeight, wallThickness);
    }
}
