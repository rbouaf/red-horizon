using UnityEngine;
public class NavMeshGeneratorUI : MonoBehaviour
{
    public VoxelNavMeshGenerator generator;
    
    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 150, 30), "Generate NavMesh"))
        {
            generator.GenerateNavMesh();
        }
    }
}