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
                checkpoint.name = feature.properties.ContainsKey("name") ? feature.properties["name"].ToString() : "Checkpoint";
            }
        }
    }

    Vector3 ConvertGeoToLocal(double lon, double lat) {
        // Convert from center-origin full map coordinates to bottom-left origin.
        // Full map: longitude -180, 180, latitude -90, 90
        float globalX = (float)(lon + 180.0);
        float globalZ = (float)(lat + 90.0);

        // If each smaller terrain has its own origin, subtract the terrain's bottom-left coordinates here.
        
        // ex:
        // float localX = globalX - terrainOriginX;
        // float localZ = globalZ - terrainOriginZ;
        // return new Vector3(localX, 0, localZ);

        // For now, return globalX and globalZ as the local coordinates.
        return new Vector3(globalX, 0, globalZ);
    }
}
