using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

// GeoJSON Data Classes 
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

// Checkpoint Data Class 
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

// Main Manager Script 
public class CheckpointManager : MonoBehaviour {
    [Header("GeoJSON & Prefab Settings")]
    [Tooltip("Drag your GeoJSON file here")]
    public TextAsset geoJsonFile;
    [Tooltip("Drag your checkpoint prefab here")]
    public GameObject checkpointPrefab;

    [Header("Terrain Conversion Settings")]
    [Tooltip("If true, compute the geo origin and size dynamically from the GeoJSON data")]
    public bool autoComputeGeoBounds = true;
    [Tooltip("Manually defined geo origin (min longitude, min latitude) if autoComputeGeoBounds is false")]
    public Vector2 terrainGeoOrigin;
    [Tooltip("Manually defined geo size (delta longitude, delta latitude) if autoComputeGeoBounds is false")]
    public Vector2 terrainGeoSize;
    [Tooltip("Optional scaling factor to adjust geo fraction; 1 = no extra scaling")]
    public float geoScaleFactor = 1f;
    [Tooltip("Optional: Assign a Terrain to automatically set the Unity terrain size")]
    public Terrain myTerrain;
    [Tooltip("Manually define Unity terrain size (width, length) if no Terrain is assigned")]
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
    [Tooltip("Popup scale (1 = default size, less than 1 reduces the pop-up)")]
    public float popupScale = 1f;

    public Font customFont;

    void Start() {

        if (customFont != null) {
            if (titleText != null)
                titleText.font = customFont;
            if (descriptionText != null)
                descriptionText.font = customFont;
        }
        // Set Unity terrain size automatically from a Terrain component if assigned
        if (myTerrain != null) {
            Vector3 size = myTerrain.terrainData.size;
            unityTerrainSize = new Vector2(size.x, size.z);
        }
        if (unityTerrainSize == Vector2.zero) {
            Debug.LogWarning("Unity terrain size is not set. Please assign a Terrain or set unityTerrainSize manually in the Inspector.");
        }

        LoadCheckpointsFromGeoJson();
    }

    void LoadCheckpointsFromGeoJson() {
        if (geoJsonFile == null) {
            Debug.LogError("GeoJSON file not assigned!");
            return;
        }

        // Deserialize the GeoJSON data 
        GeoJsonData data = JsonConvert.DeserializeObject<GeoJsonData>(geoJsonFile.text);
        if (data == null || data.features == null) {
            Debug.LogError("Failed to parse GeoJSON data.");
            return;
        }

        // Dynamically compute geo bounds if enabled
        if (autoComputeGeoBounds) {
            double minLon = double.MaxValue, maxLon = double.MinValue;
            double minLat = double.MaxValue, maxLat = double.MinValue;

            foreach (Feature feature in data.features) {
                if (feature.geometry != null && feature.geometry.type == "Point") {
                    double lon = feature.geometry.coordinates[0];
                    double lat = feature.geometry.coordinates[1];
                    if (lon < minLon) minLon = lon;
                    if (lon > maxLon) maxLon = lon;
                    if (lat < minLat) minLat = lat;
                    if (lat > maxLat) maxLat = lat;
                }
            }
            terrainGeoOrigin = new Vector2((float)minLon, (float)minLat);
            terrainGeoSize = new Vector2((float)(maxLon - minLon), (float)(maxLat - minLat));
            Debug.Log($"Computed Geo Bounds: Origin = {terrainGeoOrigin}, Size = {terrainGeoSize}");
        }

        // Instantiate each checkpoint
        foreach (Feature feature in data.features) {
            if (feature.geometry != null && feature.geometry.type == "Point") {
                double lon = feature.geometry.coordinates[0];
                double lat = feature.geometry.coordinates[1];

                // Convert the geo coordinates to Unity local coordinates
                Vector3 localPos = ConvertGeoToLocal(lon, lat);

                // Instantiate the checkpoint prefab
                GameObject checkpointObj = Instantiate(checkpointPrefab, localPos, Quaternion.identity);

                // Optionally set the object's name
                if (feature.properties.ContainsKey("title"))
                    checkpointObj.name = feature.properties["title"].ToString();

                // Extract UI properties
                string title = feature.properties.ContainsKey("title") ? feature.properties["title"].ToString() : "No Title";
                string description = feature.properties.ContainsKey("description") ? feature.properties["description"].ToString() : "";
                string imagePath = feature.properties.ContainsKey("image") ? feature.properties["image"].ToString() : "";

                // Create a CheckpointData object
                CheckpointData cpData = new CheckpointData(title, description, imagePath);

                // Setup the checkpoint component (ensure your prefab has a Checkpoint script)
                Checkpoint cp = checkpointObj.GetComponent<Checkpoint>();
                if (cp != null) {
                    cp.Setup(cpData, this);
                }
            }
        }
    }

    /// <summary>
    /// Converts geo coordinates (longitude, latitude) to local Unity coordinates.
    /// Assumes that terrainGeoOrigin and terrainGeoSize are defined in the same coordinate space as the data.
    /// </summary>
    Vector3 ConvertGeoToLocal(double lon, double lat) {
        float adjustedX = (float)lon;
        float adjustedZ = (float)lat;
        float fractionX = ((adjustedX - terrainGeoOrigin.x) / terrainGeoSize.x) * geoScaleFactor;
        float fractionZ = ((adjustedZ - terrainGeoOrigin.y) / terrainGeoSize.y) * geoScaleFactor;
        float unityX = fractionX * unityTerrainSize.x;
        float unityZ = fractionZ * unityTerrainSize.y;
        return new Vector3(unityX, 0, unityZ);
    }

    /// <summary>
    /// Displays the checkpoint UI panel with the provided data
    /// </summary>
    public void ShowCheckpointUI(CheckpointData data) {
        if (checkpointUIPanel != null) {
            checkpointUIPanel.SetActive(true);
            // Optionally adjust the size of the UI panel.
            checkpointUIPanel.transform.localScale = new Vector3(popupScale, popupScale, popupScale);
            if (titleText != null) titleText.text = data.title;
            if (descriptionText != null) descriptionText.text = data.description;
            if (checkpointImage != null) {
                Sprite sprite = Resources.Load<Sprite>(GetResourcePath(data.imagePath));
                checkpointImage.sprite = sprite;
            }
        }
    }

    /// <summary>
    /// Hides the checkpoint UI panel
    /// </summary>
    public void HideCheckpointUI() {
        if (checkpointUIPanel != null) {
            checkpointUIPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Helper method to convert an asset path (e.g., "Assets/Checkpoint/Images/pathfinder.jpg")
    /// to a Resources path ("Checkpoint/Images/pathfinder")
    /// </summary>
    string GetResourcePath(string assetPath) {
        string withoutAssets = assetPath.Replace("Assets/", "");
        int dotIndex = withoutAssets.LastIndexOf('.');
        if (dotIndex >= 0)
            withoutAssets = withoutAssets.Substring(0, dotIndex);
        return withoutAssets;
    }
}
