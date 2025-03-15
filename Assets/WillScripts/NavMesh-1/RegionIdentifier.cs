using System.Collections.Generic;
using UnityEngine;

public class RegionIdentifier
{
    private VoxelGrid grid;
    private int[,] heightField;
    
    public RegionIdentifier(VoxelGrid grid)
    {
        this.grid = grid;
        heightField = new int[grid.sizeX, grid.sizeZ];
        CreateHeightField();
    }
    
    private void CreateHeightField()
    {
        for (int x = 0; x < grid.sizeX; x++)
        {
            for (int z = 0; z < grid.sizeZ; z++)
            {
                heightField[x, z] = -1;
                
                // Find highest walkable voxel
                for (int y = grid.sizeY - 1; y >= 0; y--)
                {
                    if (grid.voxels[x, y, z])
                    {
                        heightField[x, z] = y;
                        break;
                    }
                }
            }
        }
    }
    
    public Dictionary<int, List<Vector2Int>> IdentifyRegions()
    {
        int[,] regions = new int[grid.sizeX, grid.sizeZ];
        int currentRegion = 0;
        
        // Initialize
        for (int x = 0; x < grid.sizeX; x++)
        {
            for (int z = 0; z < grid.sizeZ; z++)
            {
                regions[x, z] = heightField[x, z] >= 0 ? -1 : -2;
            }
        }
        
        // Flood fill to identify regions
        Dictionary<int, List<Vector2Int>> regionCells = new Dictionary<int, List<Vector2Int>>();
        
        for (int x = 0; x < grid.sizeX; x++)
        {
            for (int z = 0; z < grid.sizeZ; z++)
            {
                if (regions[x, z] == -1)
                {
                    List<Vector2Int> cells = new List<Vector2Int>();
                    FloodFill(regions, x, z, currentRegion, cells);
                    regionCells[currentRegion] = cells;
                    currentRegion++;
                }
            }
        }
        
        return regionCells;
    }
    
    private void FloodFill(int[,] regions, int x, int z, int regionId, List<Vector2Int> cells)
{
    Queue<Vector2Int> queue = new Queue<Vector2Int>();
    queue.Enqueue(new Vector2Int(x, z));
    int safetyCounter = 0;
    int maxCells = grid.sizeX * grid.sizeZ; // Upper limit
    
    while (queue.Count > 0 && safetyCounter < maxCells)
    {
        safetyCounter++;
        Vector2Int current = queue.Dequeue();
        
        // Skip if already processed or invalid
        if (current.x < 0 || current.x >= grid.sizeX || current.y < 0 || current.y >= grid.sizeZ || 
            regions[current.x, current.y] != -1)
            continue;
            
        // Mark as processed
        regions[current.x, current.y] = regionId;
        cells.Add(current);
        
        // Add neighbors to queue
        queue.Enqueue(new Vector2Int(current.x + 1, current.y));
        queue.Enqueue(new Vector2Int(current.x - 1, current.y));
        queue.Enqueue(new Vector2Int(current.x, current.y + 1));
        queue.Enqueue(new Vector2Int(current.x, current.y - 1));
    }
    
    if (safetyCounter >= maxCells)
        Debug.LogWarning("FloodFill reached safety limit - possible infinite loop detected");
}
}