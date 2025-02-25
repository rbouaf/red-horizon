using UnityEngine;
using UnityEngine.UI;
using TMPro;  // If using TextMeshPro

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

    private int currentIndex = 0;
    private Quaternion targetRotation;
    private bool isRotating = false;

    private void Start()
    {
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

        // Update UI
        pointNameText.text = point.pointName;
        latLongText.text = $"Lat: {point.latitude}, Long: {point.longitude}";
        descriptionText.text = point.description;

        // Update images
        // for (int i = 0; i < imagesUI.Length; i++)
        // {
        //     if (point.images != null && i < point.images.Length && point.images[i] != null)
        //     {
        //         imagesUI[i].sprite = point.images[i];
        //         imagesUI[i].enabled = true;
        //     }
        //     else
        //     {
        //         imagesUI[i].enabled = false;
        //     }
        // }

        // Rotate the globe to focus on the point
        RotateGlobeToPoint(point.latitude, point.longitude);
    }

    private void RotateGlobeToPoint(float latitude, float longitude)
    {
        // Convert lat/long to a direction vector on the sphere
        float latRad = Mathf.Deg2Rad * latitude;
        float lonRad = Mathf.Deg2Rad * longitude;

        Vector3 targetDir = new Vector3(
            Mathf.Cos(latRad) * Mathf.Cos(lonRad),
            Mathf.Sin(latRad),
            Mathf.Cos(latRad) * Mathf.Sin(lonRad)
        );

        // Set target rotation to face this direction
        targetRotation = Quaternion.LookRotation(targetDir, Vector3.up);
        isRotating = true; // Start the rotation animation
    }

    private void Update()
    {
        if (isRotating)
        {
            // Smoothly rotate the globe towards the target rotation
            marsGlobe.rotation = Quaternion.Slerp(marsGlobe.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            // Check if we reached the target (close enough)
            if (Quaternion.Angle(marsGlobe.rotation, targetRotation) < 0.1f)
            {
                marsGlobe.rotation = targetRotation; // Snap to exact position
                isRotating = false;
            }
        }
    }
}
