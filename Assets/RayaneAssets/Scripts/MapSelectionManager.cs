using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;  // For TextMeshPro support

[System.Serializable]
public class InterestPoint
{
    public string pointName;
    public float latitude;
    public float longitude;
    public string description;
    public Sprite[] images; // Up to 3 images
}

public class MapSelectionManager : MonoBehaviour
{
    [SerializeField] private InterestPoint[] interestPoints;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI pointNameText;
    [SerializeField] private TextMeshProUGUI latLongText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image[] imagesUI; // Array of Image UI elements

    [Header("Globe Rotation")]
    [SerializeField] private Transform marsGlobe;
    [SerializeField] private float rotationSpeed = 1f; // Speed of rotation

    [Header("Marker Settings")]
    [SerializeField] private float markerBaseScale = 0.04f; // Marker size
    [SerializeField] private float markerBounceScale = 1.5f;  // Bounce multiplier

    // The viewport coordinate where we want the marker to appear.
    // (0.25, 0.5) roughly means centered on the left half.
    [Header("Desired Marker Position (Viewport)")]
    [SerializeField] private Vector2 desiredViewportPos = new Vector2(0.25f, 0.5f);

    private int currentIndex = 0;
    private Quaternion targetRotation;
    private bool isRotating = false;

    // Array to hold the spawned marker objects
    private GameObject[] pointMarkers;

    private void Start()
    {
        // Spawn markers for each interest point on the globe.
        // (Assumes the globe is a Unity sphere with a local radius of 0.5.)
        if (interestPoints != null && interestPoints.Length > 0)
        {
            pointMarkers = new GameObject[interestPoints.Length];
            for (int i = 0; i < interestPoints.Length; i++)
            {
                // Create a marker as a small sphere.
                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker.transform.SetParent(marsGlobe, false);  // Parent it to the globe.
                marker.transform.localScale = Vector3.one * markerBaseScale;
                
                // Remove collider so it doesn’t interfere with interactions.
                Destroy(marker.GetComponent<SphereCollider>());
                
                // Create an unlit material for a glowy, shadow-less look.
                Material markerMat = new Material(Shader.Find("Unlit/Color"));
                markerMat.color = Color.cyan;
                marker.GetComponent<MeshRenderer>().material = markerMat;
                
                // Compute the marker’s local direction from latitude/longitude.
                float latRad = Mathf.Deg2Rad * interestPoints[i].latitude;
                float lonRad = Mathf.Deg2Rad * interestPoints[i].longitude;
                Vector3 localDir = new Vector3(
                    Mathf.Cos(latRad) * Mathf.Cos(lonRad),
                    Mathf.Sin(latRad),
                    Mathf.Cos(latRad) * Mathf.Sin(lonRad)
                ).normalized;
                // Place the marker on the globe’s surface (assuming radius = 0.5).
                float radius = 0.5f;
                marker.transform.localPosition = localDir * radius;
                
                pointMarkers[i] = marker;
            }
        }

        // Show the first interest point by default.
        ShowInterestPoint(currentIndex);
    }

    public void NextPoint()
    {
        currentIndex++;
        if (currentIndex >= interestPoints.Length) currentIndex = 0;
        ShowInterestPoint(currentIndex);
    }

    public void PrevPoint()
    {
        currentIndex--;
        if (currentIndex < 0) currentIndex = interestPoints.Length - 1;
        ShowInterestPoint(currentIndex);
    }

    private void ShowInterestPoint(int index)
    {
        if (interestPoints == null || interestPoints.Length == 0) return;

        InterestPoint point = interestPoints[index];

        // Update UI text fields.
        pointNameText.text = point.pointName;
        latLongText.text = $"Lat: {point.latitude}, Long: {point.longitude}";
        descriptionText.text = point.description;
        // (Optionally update images.)

        // Reset all markers to default (cyan color and base scale).
        if (pointMarkers != null)
        {
            for (int i = 0; i < pointMarkers.Length; i++)
            {
                MeshRenderer mr = pointMarkers[i].GetComponent<MeshRenderer>();
                mr.material.color = Color.cyan;
                pointMarkers[i].transform.localScale = Vector3.one * markerBaseScale;
            }
            // For the selected marker, change its color to white and animate a bounce effect.
            MeshRenderer selectedMR = pointMarkers[index].GetComponent<MeshRenderer>();
            selectedMR.material.color = Color.white;
            StartCoroutine(AnimateMarker(pointMarkers[index]));
        }

        // Rotate the globe so the selected interest point faces the desired direction.
        RotateGlobeToPoint(point.latitude, point.longitude);
    }

    private void RotateGlobeToPoint(float latitude, float longitude)
    {
        // 1. Compute the interest point's local direction as if the globe were unrotated.
        float latRad = Mathf.Deg2Rad * latitude;
        float lonRad = Mathf.Deg2Rad * longitude;
        Vector3 pointLocalDir = new Vector3(
            Mathf.Cos(latRad) * Mathf.Cos(lonRad),
            Mathf.Sin(latRad),
            Mathf.Cos(latRad) * Mathf.Sin(lonRad)
        ).normalized;

        // 2. Compute the current world direction of the interest point marker.
        Vector3 currentWorldDir = marsGlobe.rotation * pointLocalDir;

        // 3. Determine the desired world direction.
        //    We convert our desired viewport position (e.g., (0.25, 0.5)) into a world point,
        //    using the distance from the camera to the globe’s center.
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("Main Camera not found");
            return;
        }
        float distance = Vector3.Distance(cam.transform.position, marsGlobe.position);
        Vector3 desiredWorldPos = cam.ViewportToWorldPoint(new Vector3(desiredViewportPos.x, desiredViewportPos.y, distance));
        Vector3 desiredDir = (desiredWorldPos - marsGlobe.position).normalized;

        // 4. Apply an additional -90° rotation about the Y-axis (to shift right).
        desiredDir = Quaternion.AngleAxis(-56f, Vector3.up) * desiredDir;

        // 5. Compute the delta rotation that rotates the current world direction to the desired direction.
        Quaternion deltaRotation = Quaternion.FromToRotation(currentWorldDir, desiredDir);
        targetRotation = deltaRotation * marsGlobe.rotation;
        isRotating = true;
    }

    private void Update()
    {
        if (isRotating)
        {
            // Smoothly rotate the globe toward the target rotation.
            marsGlobe.rotation = Quaternion.Slerp(marsGlobe.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            if (Quaternion.Angle(marsGlobe.rotation, targetRotation) < 0.1f)
            {
                marsGlobe.rotation = targetRotation;
                isRotating = false;
            }
        }
    }

    private IEnumerator AnimateMarker(GameObject marker)
    {
        float duration = 0.5f; // Bounce animation duration.
        Vector3 initialScale = marker.transform.localScale;
        Vector3 targetScale = initialScale * markerBounceScale;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float easeValue = EaseOutBack(t);
            marker.transform.localScale = Vector3.Lerp(initialScale, targetScale, easeValue);
            yield return null;
        }
        marker.transform.localScale = targetScale;
    }

    // Easing function for a bouncy, overshooting effect (ease out back).
    private float EaseOutBack(float t)
    {
        float s = 1.70158f;
        t = t - 1;
        return (t * t * ((s + 1) * t + s) + 1);
    }
}
