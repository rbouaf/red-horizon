using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json; // Ensure you have the Newtonsoft JSON package imported

// Define classes to match the GeoJSON structure
[System.Serializable]
public class GeoJsonData {
    public string type;
    public List<Feature> features;
}

[System.Serializable]
public class Feature {
    public string type;
    public Geometry geometry;
    public Dictionary<string, object> properties;
}

[System.Serializable]
public class Geometry {
    public string type;
    public List<double> coordinates;
}

public class CheckpointLoader : MonoBehaviour {
    public TextAsset geoJsonFile;       // Drag your GeoJSON file here in the Inspector
    public GameObject checkpointPrefab; // Drag your checkpoint prefab here

    // Define your terrain conversion parameters
    public Vector2 terrainGeoOrigin; // Bottom-left of your terrain in adjusted degrees
    public Vector2 terrainGeoSize;   // Size of the terrain in degrees (longitude span, latitude span)
    public Vector2 unityTerrainSize; // Actual terrain size in Unity units 

    void Start() {
        LoadCheckpointsFromGeoJson();
    }

    void LoadCheckpointsFromGeoJson() {
        if (geoJsonFile == null) {
            Debug.LogError("GeoJSON file not assigned!");
            return;
        }

        // Deserialize the GeoJSON data
        GeoJsonData data = JsonConvert.DeserializeObject<GeoJsonData>(geoJsonFile.text);

        foreach (Feature feature in data.features) {
            if (feature.geometry.type == "Point") {
                double lon = feature.geometry.coordinates[0];
                double lat = feature.geometry.coordinates[1];

                // Convert geo coordinate to local Unity coordinate
                Vector3 localPos = ConvertGeoToLocal(lon, lat);
                
                // Instantiate the checkpoint prefab
                GameObject checkpoint = Instantiate(checkpointPrefab, localPos, Quaternion.identity);
                // Optionally set the name from properties
                if (feature.properties.ContainsKey("title"))
                    checkpoint.name = feature.properties["title"].ToString();
            }
        }
    }

    /// <summary>
    /// Converts full-map geo coordinates (in degrees) to local Unity coordinates on a terrain.
    /// Assumes full map uses a center-origin (lon: -180 to 180, lat: -90 to 90).
    /// </summary>
    Vector3 ConvertGeoToLocal(double lon, double lat) {
        // Adjust from center-origin to bottom-left origin: (0,0) to (360,180)
        float adjustedX = (float)(lon + 180.0);
        float adjustedZ = (float)(lat + 90.0);

        // Calculate the fraction along each axis relative to the terrain's geo extent
        float fractionX = (adjustedX - terrainGeoOrigin.x) / terrainGeoSize.x;
        float fractionZ = (adjustedZ - terrainGeoOrigin.y) / terrainGeoSize.y;

        // Map the fraction to Unity terrain units
        float unityX = fractionX * unityTerrainSize.x;
        float unityZ = fractionZ * unityTerrainSize.y;

        // Y value can be adjusted by sampling the terrain's heightmap if needed; for now, we set it to 0.
        return new Vector3(unityX, 0, unityZ);
    }
}
