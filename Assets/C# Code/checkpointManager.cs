using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class GeoJsonData {
    public string type;
    public List<Feature> features;
}

public class Feature {
    public string type;
    public Geometry geometry;
    public Dictionary<string, object> properties;
}

public class Geometry {
    public string type;
    public List<double> coordinates;
}

public class checkpointManager : MonoBehaviour {
    public TextAsset geoJsonFile;       // assign  GeoJSON file in the Inspector
    public GameObject checkpointPrefab; // unity prefab object created for every checkpoint (temporary)

    void Start() {
        LoadCheckpointsFromGeoJson();
    }

    void LoadCheckpointsFromGeoJson() {
        if (geoJsonFile == null) {
            Debug.LogError("GeoJSON file not assigned!");
            return;
        }
        
        GeoJsonData data = JsonConvert.DeserializeObject<GeoJsonData>(geoJsonFile.text);

        foreach (Feature feature in data.features) {
            if (feature.geometry.type == "Point") {
                double lon = feature.geometry.coordinates[0];
                double lat = feature.geometry.coordinates[1];

                // Convert from full map (mars trek) to Unity local coordinates
                Vector3 localPos = ConvertGeoToLocal(lon, lat);
                
                // Instantiate the checkpoint at the converted position
                GameObject checkpoint = Instantiate(checkpointPrefab, localPos, Quaternion.identity);
                checkpoint.name = feature.properties.ContainsKey("title") ? feature.properties["title"].ToString() : "Checkpoint";
            }
        }
    }

   /// <summary>
    /// Converts full-map geo coordinates (in degrees) to local Unity coordinates on a specific terrain.
    /// </summary>
    /// <param name="lon">Longitude in degrees (full map center-origin)</param>
    /// <param name="lat">Latitude in degrees (full map center-origin)</param>
    /// <param name="terrainGeoOrigin">Bottom-left geo coordinate of the terrain (adjusted system: 0-360, 0-180)</param>
    /// <param name="terrainGeoSize">Geo extents of the terrain in degrees</param>
    /// <param name="unityTerrainSize">Terrain dimensions in Unity units (e.g., from terrain.terrainData.size)</param>
    /// <returns>Local Unity coordinate for the checkpoint</returns>
    public Vector3 ConvertGeoToLocal(double lon, double lat, Vector2 terrainGeoOrigin, Vector2 terrainGeoSize, Vector2 unityTerrainSize)
    {
        // Convert from center-origin to bottom-left origin (adjusted coordinates in degrees)
        float adjustedX = (float)(lon + 180.0);
        float adjustedZ = (float)(lat + 90.0);

        // Determine how far into the terrain's geo extent the coordinate lies
        float fractionX = (adjustedX - terrainGeoOrigin.x) / terrainGeoSize.x;
        float fractionZ = (adjustedZ - terrainGeoOrigin.y) / terrainGeoSize.y;

        // Scale by the Unity terrain size to get the local coordinate
        float unityX = fractionX * unityTerrainSize.x;
        float unityZ = fractionZ * unityTerrainSize.y;

        return new Vector3(unityX, 0, unityZ);
    }
    
    
    public void UpdateCheckpointPosition(Transform checkpoint, Terrain terrain, double lon, double lat, Vector2 terrainGeoOrigin, Vector2 terrainGeoSize)
    {
        // Get the Unity terrain size 
        Vector2 unityTerrainSize = new Vector2(terrain.terrainData.size.x, terrain.terrainData.size.z);
        
        // Convert the geo coordinate to a local Unity coordinate
        Vector3 localPos = ConvertGeoToLocal(lon, lat, terrainGeoOrigin, terrainGeoSize, unityTerrainSize);
        
        // Optionally adjust Y using the terrain's heightmap
        float newY = terrain.SampleHeight(localPos) + terrain.transform.position.y;
        checkpoint.position = new Vector3(localPos.x, newY, localPos.z);
    }
}
