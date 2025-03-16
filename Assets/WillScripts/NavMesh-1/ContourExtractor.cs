using System.Collections.Generic;
using UnityEngine;

public class ContourExtractor
{
    private VoxelGrid grid;
    private Dictionary<int, List<Vector2Int>> regions;
    private int[,] regionGrid;
    
    public ContourExtractor(VoxelGrid grid, Dictionary<int, List<Vector2Int>> regions)
    {
        this.grid = grid;
        this.regions = regions;
        
        // Create a grid representation of the regions
        InitializeRegionGrid();
    }
    
    private void InitializeRegionGrid()
    {
        // Create a grid marking which cell belongs to which region
        regionGrid = new int[grid.sizeX, grid.sizeZ];
        
        // Initialize all cells to -1 (no region)
        for (int x = 0; x < grid.sizeX; x++)
        {
            for (int z = 0; z < grid.sizeZ; z++)
            {
                regionGrid[x, z] = -1;
            }
        }
        
        // Mark each region's cells
        foreach (var region in regions)
        {
            foreach (Vector2Int cell in region.Value)
            {
                regionGrid[cell.x, cell.y] = region.Key;
            }
        }
    }
    
    public List<List<Vector3>> ExtractContours()
    {
        List<List<Vector3>> allContours = new List<List<Vector3>>();
        
        foreach (var region in regions)
        {
            List<Vector3> contour = MarchingSquares(region.Key, region.Value);
            if (contour.Count > 0)
            {
                allContours.Add(contour);
            }
        }
        
        return allContours;
    }
    
    private List<Vector3> MarchingSquares(int regionId, List<Vector2Int> cells)
    {   
        // Upper Bound the number of iterations so that it does not freeze
        int maxIterations = grid.sizeX * grid.sizeZ * 2; 
        int iterations = 0;

        // Find the region bounds
        int minX = int.MaxValue, minZ = int.MaxValue;
        int maxX = int.MinValue, maxZ = int.MinValue;
        
        foreach (Vector2Int cell in cells)
        {
            minX = Mathf.Min(minX, cell.x);
            minZ = Mathf.Min(minZ, cell.y);
            maxX = Mathf.Max(maxX, cell.x);
            maxZ = Mathf.Max(maxZ, cell.y);
        }
        
        // Expand bounds by 1 to ensure we capture the boundary
        minX = Mathf.Max(0, minX - 1);
        minZ = Mathf.Max(0, minZ - 1);
        maxX = Mathf.Min(grid.sizeX - 1, maxX + 1);
        maxZ = Mathf.Min(grid.sizeZ - 1, maxZ + 1);
        
        // Create a HashSet for faster lookup
        HashSet<Vector2Int> cellSet = new HashSet<Vector2Int>(cells);
        
        // Find the starting edge - a cell in the region adjacent to a cell outside the region
        Vector2Int? startCell = null;
        Vector2Int? startNeighbor = null;
        
        for (int x = minX; x <= maxX && startCell == null; x++)
        {
            for (int z = minZ; z <= maxZ && startCell == null; z++)
            {
                if (cellSet.Contains(new Vector2Int(x, z)))
                {
                    // Check all 4 neighbors
                    Vector2Int[] neighbors = new Vector2Int[]
                    {
                        new Vector2Int(x + 1, z),
                        new Vector2Int(x, z + 1),
                        new Vector2Int(x - 1, z),
                        new Vector2Int(x, z - 1)
                    };
                    
                    foreach (Vector2Int neighbor in neighbors)
                    {
                        if (!cellSet.Contains(neighbor) && 
                            neighbor.x >= 0 && neighbor.x < grid.sizeX && 
                            neighbor.y >= 0 && neighbor.y < grid.sizeZ)
                        {
                            startCell = new Vector2Int(x, z);
                            startNeighbor = neighbor;
                            break;
                        }
                    }
                }
            }
        }
        
        // If no starting point was found, return empty contour
        if (startCell == null || startNeighbor == null)
            return new List<Vector3>();
            
        // Start marching
        List<Vector3> contour = new List<Vector3>();
        Vector2Int currentCell = startCell.Value;
        Vector2Int currentNeighbor = startNeighbor.Value;
        Vector2Int initialEdge = new Vector2Int(currentCell.x, currentCell.y);
        bool firstPoint = true;
        
        // Store visited edges to prevent infinite loops
        HashSet<Vector4> visitedEdges = new HashSet<Vector4>();
        
        do
        {
            iterations++;

            if (iterations > maxIterations) 
            {
                Debug.LogError("Marching squares exceeded maximum iterations - breaking early");
                break;
            }
            
            // If this exact edge has been visited, break to prevent loops
            Vector4 edge = new Vector4(currentCell.x, currentCell.y, currentNeighbor.x, currentNeighbor.y);
            if (visitedEdges.Contains(edge))
                break;
                
            visitedEdges.Add(edge);
            
            // Calculate the midpoint of the edge and add it to the contour
            Vector3 midpoint = new Vector3(
                (currentCell.x + currentNeighbor.x) * 0.5f, 
                GetHeight(currentCell), // Use the height of the region cell
                (currentCell.y + currentNeighbor.y) * 0.5f
            );
            
            contour.Add(midpoint);
            
            // Get the next edge using the marching squares algorithm
            GetNextEdge(currentCell, currentNeighbor, cellSet, out Vector2Int nextCell, out Vector2Int nextNeighbor);
            
            // Update current edge
            currentCell = nextCell;
            currentNeighbor = nextNeighbor;
            
            // Check if we've completed the loop
            if (firstPoint)
            {
                firstPoint = false;
                initialEdge = new Vector2Int(currentCell.x, currentCell.y);
            }
            
        } while (!(currentCell.Equals(initialEdge) && firstPoint == false));
        
        return contour;
    }
    
    private void GetNextEdge(Vector2Int currentCell, Vector2Int currentNeighbor, 
                            HashSet<Vector2Int> cellSet, 
                            out Vector2Int nextCell, out Vector2Int nextNeighbor)
    {
        // Determine direction from cell to neighbor
        int dx = currentNeighbor.x - currentCell.x;
        int dz = currentNeighbor.y - currentCell.y;
        
        // Try to turn right first (looking from region to outside)
        Vector2Int rightTurn = new Vector2Int(currentNeighbor.x - dz, currentNeighbor.y + dx);
        
        if (cellSet.Contains(rightTurn))
        {
            // The right turn is inside the region, so we go along that edge
            nextCell = rightTurn;
            nextNeighbor = currentNeighbor;
            return;
        }
        
        // Try going straight
        Vector2Int straight = new Vector2Int(currentNeighbor.x + dx, currentNeighbor.y + dz);
        
        if (!cellSet.Contains(straight) && IsInBounds(straight))
        {
            // Going straight keeps us on the boundary
            nextCell = currentNeighbor;
            nextNeighbor = straight;
            return;
        }
        
        // Turn left
        Vector2Int leftTurn = new Vector2Int(currentNeighbor.x + dz, currentNeighbor.y - dx);
        
        nextCell = leftTurn;
        nextNeighbor = straight;
    }
    
    private bool IsInBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < grid.sizeX && pos.y >= 0 && pos.y < grid.sizeZ;
    }
    
    private float GetHeight(Vector2Int cell)
    {
        // Get the height of the voxel at this position
        for (int y = grid.sizeY - 1; y >= 0; y--)
        {
            if (grid.voxels[cell.x, y, cell.y])
            {
                return y + 0.01f; // Slightly above the voxel surface
            }
        }
        return 0f;
    }
}