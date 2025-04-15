using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using TMPro;

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
    public TextMeshProUGUI titleText;
    [Tooltip("UI Text element for the description")]
    public TextMeshProUGUI descriptionText;
    [Tooltip("UI Image element for the checkpoint image")]
    public Image checkpointImage;
    public GameObject toggleButton;
    public Font customFont;

     // Store the last checkpoint data so we can re-show it if needed.
    private CheckpointData currentCheckpointData;

    void Start() {

        // Hide the UI panel at start
        if (checkpointUIPanel != null) {
            checkpointUIPanel.SetActive(false);
        }
         if (titleText != null) {
            titleText.gameObject.SetActive(false);
        }
        if (descriptionText != null) {
            descriptionText.gameObject.SetActive(false);
        }
        if (checkpointImage != null) {
            checkpointImage.gameObject.SetActive(false);
        }
        if (toggleButton != null) {
            toggleButton.SetActive(false);
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

                // Setup the checkpoint component
                Checkpoint cp = checkpointObj.GetComponent<Checkpoint>();
                if (cp != null) {
                    cp.Setup(cpData, this);
                }
            }
        }
    }

    /// 
    /// Converts geo coordinates (longitude, latitude) to local Unity coordinates.
    /// Assumes that terrainGeoOrigin and terrainGeoSize are defined in the same coordinate space as the data.
    /// 
    Vector3 ConvertGeoToLocal(double lon, double lat) {
        float adjustedX = (float)lon;
        float adjustedZ = (float)lat;
        float fractionX = ((adjustedX - terrainGeoOrigin.x) / terrainGeoSize.x) * geoScaleFactor;
        float fractionZ = ((adjustedZ - terrainGeoOrigin.y) / terrainGeoSize.y) * geoScaleFactor;
        float unityX = fractionX * unityTerrainSize.x;
        float unityZ = fractionZ * unityTerrainSize.y;
        return new Vector3(unityX, 0, unityZ);
    }

    /// 
    /// Displays the checkpoint UI panel with the provided data
    /// 
    public void ShowCheckpointUI(CheckpointData data) {
        currentCheckpointData = data; //store current data
        if (checkpointUIPanel != null) {
            checkpointUIPanel.SetActive(true);
        }
        if (titleText != null) {
            titleText.gameObject.SetActive(true);
            titleText.text = data.title;
        }
        if (descriptionText != null) {
            descriptionText.gameObject.SetActive(true);
            descriptionText.text = data.description;
        }
        if (checkpointImage != null) {

            checkpointImage.gameObject.SetActive(true);
            Sprite sprite = Resources.Load<Sprite>(data.imagePath);
            if (sprite == null) {
            Debug.LogError("Sprite not found at path: " + data.imagePath);
            }
            else {
            checkpointImage.sprite = sprite;
            }
        }
          if (toggleButton != null) {
            toggleButton.SetActive(true);
        }
    }

    /// 
    /// Hides the checkpoint UI panel
    /// 
    public void HideCheckpointUI() {
        if (checkpointUIPanel != null) {
            checkpointUIPanel.SetActive(false);
        }
        if (titleText != null) {
            titleText.gameObject.SetActive(false);
        }
        if (descriptionText != null) {
            descriptionText.gameObject.SetActive(false);
        }
        if (checkpointImage != null) {
            checkpointImage.gameObject.SetActive(false);
        }
    }

    public void HideButton() {
         if (toggleButton != null) {
            toggleButton.SetActive(false);
        }
    }

    /// 
    /// Toggles the checkpoint UI on or off.
    /// If the UI is off, it reactivates it with the stored data.
    ///

    public void ToggleCheckpointUI() {
        if (checkpointUIPanel == null)
            return;

        if (checkpointUIPanel.activeSelf) {
            HideCheckpointUI();
        }
        else {
            // Only show if we have stored checkpoint data.
            if (currentCheckpointData != null) {
                ShowCheckpointUI(currentCheckpointData);
            }
        }
    }
}
