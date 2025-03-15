
using UnityEngine;
public class VoxelGrid
{
    public bool[,,] voxels;
    public Vector3 origin;
    public float voxelSize;
    public int sizeX, sizeY, sizeZ;
    
    public VoxelGrid(Bounds bounds, float voxelSize)
    {
        this.voxelSize = voxelSize;
        this.origin = bounds.min;
        
        sizeX = Mathf.CeilToInt(bounds.size.x / voxelSize);
        sizeY = Mathf.CeilToInt(bounds.size.y / voxelSize);
        sizeZ = Mathf.CeilToInt(bounds.size.z / voxelSize);
        
        voxels = new bool[sizeX, sizeY, sizeZ];
    }
    
    public void Voxelize(LayerMask mask)
    {
        for (int x = 0; x < sizeX; x++)
        {
            for (int z = 0; z < sizeZ; z++)
            {
                Vector3 rayStart = origin + new Vector3(x * voxelSize, sizeY * voxelSize + 1f, z * voxelSize);
                if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, sizeY * voxelSize + 2f, mask))
                {
                    int y = WorldToVoxelY(hit.point.y);
                    if (y >= 0 && y < sizeY)
                    {
                        voxels[x, y, z] = true;
                    }
                }
            }
        }
    }
    
    public int WorldToVoxelX(float x) => Mathf.FloorToInt((x - origin.x) / voxelSize);
    public int WorldToVoxelY(float y) => Mathf.FloorToInt((y - origin.y) / voxelSize);
    public int WorldToVoxelZ(float z) => Mathf.FloorToInt((z - origin.z) / voxelSize);
    
    public Vector3 VoxelToWorld(int x, int y, int z) => 
        origin + new Vector3(x * voxelSize, y * voxelSize, z * voxelSize);
    
    public bool IsInBounds(int x, int y, int z) =>
        x >= 0 && x < sizeX && y >= 0 && y < sizeY && z >= 0 && z < sizeZ;
    
    
    public void Visualize(Transform parent)
    {
        GameObject container = new GameObject("VoxelVisualization");
        container.transform.parent = parent;
        
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                for (int z = 0; z < sizeZ; z++)
                {
                    if (voxels[x, y, z])
                    {
                        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        cube.transform.parent = container.transform;
                        cube.transform.localScale = Vector3.one * voxelSize * 0.9f;
                        cube.transform.position = VoxelToWorld(x, y, z) + new Vector3(voxelSize/2, voxelSize/2, voxelSize/2);
                        
                        // Use different colors for different heights
                        Renderer r = cube.GetComponent<Renderer>();
                        r.material.color = Color.HSVToRGB((float)y / sizeY, 0.7f, 0.7f);
                    }
                }
            }
        }
    }
}