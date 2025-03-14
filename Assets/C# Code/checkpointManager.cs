using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json; // Make sure the Newtonsoft JSON package is imported.
using UnityEngine.UI;

#region GeoJSON Data Classes
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
#endregion

#region Checkpoint Data Class
// A plain class to hold checkpoint data.
public class CheckpointData {
    public string title;
    public string description;
    public string imagePath;
    public CheckpointData(string title, string description, string imagePath) {
        this.title = title;
        this.description = description;
        this.imagePath = imagePath;
    }
}
#endregion

public class CheckpointManager : MonoBehaviour {
    [Header("GeoJSON & Prefab Settings")]
    [Tooltip("Drag your GeoJSON file here")]
    public TextAsset geoJsonFile;
    [Tooltip("Drag your checkpoint prefab here")]
    public GameObject checkpointPrefab;
    
    [Header("Terrain Conversion Settings")]
    [Tooltip("Bottom-left of your terrain in adjusted degrees (0 to 360 for longitude, 0 to 180 for latitude)")]
    public Vector2 terrainGeoOrigin;
    [Tooltip("Geo size of the terrain in degrees (longitude span, latitude span)")]
    public Vector2 terrainGeoSize;
    [Tooltip("Actual terrain size in Unity units (X,Z)")]
    public Vector2 unityTerrainSize;

    [Header("UI Settings")]
    [Tooltip("UI Panel that displays the checkpoint info")]
    public GameObject checkpointUIPanel;
    [Tooltip("UI Text element for the title")]
    public Text titleText;
    [Tooltip("UI Text element for the description")]
    public Text descriptionText;
    [Tooltip("UI Image element for the checkpoint image")]
    public Image checkpointImage;

    void Start() {
        LoadCheckpointsFromGeoJson();
    }

    void LoadCheckpointsFromGeoJson() {
        if (geoJsonFile == null) {
            Debug.LogError("GeoJSON file not assigned!");
            return;
        }

        // Deserialize the GeoJSON data using Newtonsoft.Json
        GeoJsonData data = JsonConvert.DeserializeObject<GeoJsonData>(geoJsonFile.text);
        if (data == null || data.features == null) {
            Debug.LogError("Failed to parse GeoJSON data.");
            return;
        }

        // Loop through each feature
        foreach (Feature feature in data.features) {
            // Check if the feature represents a point.
            if (feature.geometry != null && feature.geometry.type == "Point") {
                double lon = feature.geometry.coordinates[0];
                double lat = feature.geometry.coordinates[1];

                // Convert geo coordinates to a Unity world position using your custom logic.
                Vector3 localPos = ConvertGeoToLocal(lon, lat);

                // Instantiate the checkpoint prefab at the calculated position.
                GameObject checkpointObj = Instantiate(checkpointPrefab, localPos, Quaternion.identity);
                
                // Optionally set the GameObject's name if the property exists.
                if (feature.properties.ContainsKey("title"))
                    checkpointObj.name = feature.properties["title"].ToString();

                // Extract additional properties for use in the UI.
                string title = feature.properties.ContainsKey("title") ? feature.properties["title"].ToString() : "No Title";
                string description = feature.properties.ContainsKey("description") ? feature.properties["description"].ToString() : "";
                string imagePath = feature.properties.ContainsKey("image") ? feature.properties["image"].ToString() : "";

                // Create a data object to pass to the checkpoint.
                CheckpointData cpData = new CheckpointData(title, description, imagePath);

                // Initialize the Checkpoint component (make sure your prefab has the Checkpoint.cs script attached).
                Checkpoint cp = checkpointObj.GetComponent<Checkpoint>();
                if (cp != null) {
                    cp.Setup(cpData, this);
                }
            }
        }
    }

    /// <summary>
    /// Converts global geo coordinates (in degrees) to local Unity coordinates on your terrain.
    /// Assumes full map geo coordinates are adjusted from a center-origin (-180 to 180, -90 to 90) to a bottom-left origin (0,0).
    /// </summary>
    Vector3 ConvertGeoToLocal(double lon, double lat) {
        // Adjust from center-origin to bottom-left origin: (0,0) to (360,180)
        float adjustedX = (float)(lon + 180.0);
        float adjustedZ = (float)(lat + 90.0);

        // Calculate the fraction along each axis relative to your terrain's geo extent.
        float fractionX = (adjustedX - terrainGeoOrigin.x) / terrainGeoSize.x;
        float fractionZ = (adjustedZ - terrainGeoOrigin.y) / terrainGeoSize.y;

        // Map the fraction to Unity terrain units.
        float unityX = fractionX * unityTerrainSize.x;
        float unityZ = fractionZ * unityTerrainSize.y;

        // Y (vertical) is set to 0, or you can sample your terrain's height if needed.
        return new Vector3(unityX, 0, unityZ);
    }

    /// <summary>
    /// Shows the checkpoint UI with details from the given data.
    /// </summary>
    public void ShowCheckpointUI(CheckpointData data) {
        if (checkpointUIPanel) {
            checkpointUIPanel.SetActive(true);
            if (titleText) titleText.text = data.title;
            if (descriptionText) descriptionText.text = data.description;
            if (checkpointImage) {
                // Convert the asset path to a Resources loadable path.
                Sprite sprite = Resources.Load<Sprite>(GetResourcePath(data.imagePath));
                checkpointImage.sprite = sprite;
            }
        }
    }

    /// <summary>
    /// Hides the checkpoint UI.
    /// </summary>
    public void HideCheckpointUI() {
        if (checkpointUIPanel) {
            checkpointUIPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Helper method to convert an asset path to a Resources path.
    /// Example: "Assets/Checkpoint/Images/pathfinder.jpg" becomes "Checkpoint/Images/pathfinder"
    /// </summary>
    string GetResourcePath(string assetPath) {
        string withoutAssets = assetPath.Replace("Assets/", "");
        int dotIndex = withoutAssets.LastIndexOf('.');
        if (dotIndex >= 0)
            withoutAssets = withoutAssets.Substring(0, dotIndex);
        return withoutAssets;
    }
}
