using System.Collections.Generic;
using UnityEngine;

public class WalkableSurfaceDetector
{
    private VoxelGrid grid;
    private float agentHeight;
    private float agentRadius;
    private float maxSlope;
    
    public WalkableSurfaceDetector(VoxelGrid grid, float agentHeight, float agentRadius, float maxSlope)
    {
        this.grid = grid;
        this.agentHeight = agentHeight;
        this.agentRadius = agentRadius;
        this.maxSlope = maxSlope;
    }
    
    public void Process()
    {
        Debug.Log("Starting walkable surface detection...");

        bool[,,] walkable = new bool[grid.sizeX, grid.sizeY, grid.sizeZ];
        
        for (int x = 0; x < grid.sizeX; x++)
        {
            for (int z = 0; z < grid.sizeZ; z++)
            {
                for (int y = 0; y < grid.sizeY; y++)
                {
                    if (grid.voxels[x, y, z])
                    {
                        if (HasClearance(x, y, z) && IsSlopeWalkable(x, y, z))
                        {
                            walkable[x, y, z] = true;
                        }
                    }
                }
            }
        }
        
        grid.voxels = walkable;
        Debug.Log("Completed walkable surface detection");
    }
    
    private bool HasClearance(int x, int y, int z)
    {
        int clearanceNeeded = Mathf.CeilToInt(agentHeight / grid.voxelSize);
        
        for (int i = 1; i <= clearanceNeeded; i++)
        {
            if (y + i < grid.sizeY && grid.voxels[x, y + i, z])
            {
                return false;
            }
        }
        
        return true;
    }
    
    private bool IsSlopeWalkable(int x, int y, int z)
    {
        // Sample points in a 3x3 neighborhood to estimate the surface normal
        Vector3 normal = EstimateSurfaceNormal(x, y, z);
        
        // Calculate the angle between the normal and the up vector
        float angle = Vector3.Angle(normal, Vector3.up);
        
        // Check if the angle is less than or equal to the maximum allowed slope
        return angle <= maxSlope;
    }
    
    private Vector3 EstimateSurfaceNormal(int x, int y, int z)
    {
        // We'll use the 3x3 neighborhood to estimate the surface normal
        Vector3 sum = Vector3.zero;
        int count = 0;
        
        // Sample heights in the neighborhood
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dz = -1; dz <= 1; dz++)
            {
                // Skip the center point
                if (dx == 0 && dz == 0)
                    continue;
                    
                int nx = x + dx;
                int nz = z + dz;
                
                // Skip if out of bounds
                if (nx < 0 || nx >= grid.sizeX || nz < 0 || nz >= grid.sizeZ)
                    continue;
                
                // Find the highest solid voxel at this position
                int ny = -1;
                for (int checkY = grid.sizeY - 1; checkY >= 0; checkY--)
                {
                    if (grid.voxels[nx, checkY, nz])
                    {
                        ny = checkY;
                        break;
                    }
                }
                
                // Skip if no solid voxel found
                if (ny == -1)
                    continue;
                
                // Calculate the direction vector from the current point to this neighbor
                Vector3 direction = new Vector3(
                    dx * grid.voxelSize,
                    (ny - y) * grid.voxelSize,
                    dz * grid.voxelSize
                ).normalized;
                
                // Add to the sum
                sum += direction;
                count++;
            }
        }
        
        // If we couldn't sample any neighbors, default to up vector
        if (count == 0)
            return Vector3.up;
            
        // Get the average direction and normalize
        Vector3 avg = sum / count;
        
        // Cross product to get perpendicular vectors
        Vector3 tangent1 = Vector3.Cross(avg, Vector3.right).normalized;
        if (tangent1.magnitude < 0.001f)
            tangent1 = Vector3.Cross(avg, Vector3.forward).normalized;
            
        Vector3 tangent2 = Vector3.Cross(avg, tangent1).normalized;
        
        // Calculate the normal using the cross product of the tangents
        Vector3 normal = Vector3.Cross(tangent1, tangent2).normalized;
        
        // Ensure the normal points upward
        if (Vector3.Dot(normal, Vector3.up) < 0)
            normal = -normal;
            
        return normal;
    }
    
    // Optional: Add this method for more accurate surface normal estimation
    private Vector3 EstimateSurfaceNormalAlternative(int x, int y, int z)
    {
        // An alternative method using least squares plane fitting
        // This creates a more accurate normal but is computationally more expensive
        
        // Sample points in the neighborhood
        List<Vector3> points = new List<Vector3>();
        
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dz = -1; dz <= 1; dz++)
            {
                int nx = x + dx;
                int nz = z + dz;
                
                if (nx < 0 || nx >= grid.sizeX || nz < 0 || nz >= grid.sizeZ)
                    continue;
                
                // Find the highest solid voxel
                for (int ny = grid.sizeY - 1; ny >= 0; ny--)
                {
                    if (grid.voxels[nx, ny, nz])
                    {
                        points.Add(new Vector3(nx * grid.voxelSize, ny * grid.voxelSize, nz * grid.voxelSize));
                        break;
                    }
                }
            }
        }
        
        if (points.Count < 3)
            return Vector3.up;
            
        // Calculate centroid
        Vector3 centroid = Vector3.zero;
        foreach (Vector3 point in points)
            centroid += point;
        centroid /= points.Count;
        
        // Calculate covariance matrix
        float xx = 0, xy = 0, xz = 0, yy = 0, yz = 0, zz = 0;
        
        foreach (Vector3 point in points)
        {
            Vector3 r = point - centroid;
            xx += r.x * r.x;
            xy += r.x * r.y;
            xz += r.x * r.z;
            yy += r.y * r.y;
            yz += r.y * r.z;
            zz += r.z * r.z;
        }
        
        // Find smallest eigenvector (this is the normal)
        float detX = yy * zz - yz * yz;
        float detY = xx * zz - xz * xz;
        float detZ = xx * yy - xy * xy;
        
        Vector3 normal;
        
        if (detX >= detY && detX >= detZ)
            normal = new Vector3(1, (xz * yz - xy * zz) / detX, (xy * yz - xz * yy) / detX).normalized;
        else if (detY >= detX && detY >= detZ)
            normal = new Vector3((yz * xz - xy * zz) / detY, 1, (xy * xz - yz * xx) / detY).normalized;
        else
            normal = new Vector3((yz * xy - xz * yy) / detZ, (xz * xy - yz * xx) / detZ, 1).normalized;
        
        // Ensure the normal points upward
        if (normal.y < 0)
            normal = -normal;
            
        return normal;
    }
}