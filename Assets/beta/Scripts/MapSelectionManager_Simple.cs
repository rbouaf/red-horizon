// File: Assets/beta/Scripts/MapSelectionManager_Simple.cs
// Purpose: A simplified version to select interest points and load scenes without 3D globe/markers.

using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Needed for Image, RectTransform
using TMPro; // Needed for TextMeshProUGUI
using UnityEngine.SceneManagement; // Needed for SceneManager

// --- InterestPoint Definition (Keep as is) ---
[System.Serializable]
public class InterestPoint_Simple // Renamed slightly to avoid potential conflict if original class is still accessible
{
	public string pointName;
	public float latitude;  // Kept for display, even if not used for rotation
	public float longitude; // Kept for display, even if not used for rotation
	public string description;
	// Removed: public Sprite[] images; // You mentioned removing UI elements tied to globe/markers
	public string sceneName;
}
// --- End of InterestPoint Definition ---

public class MapSelectionManager_Simple : MonoBehaviour
{
	[Header("Data")]
	[SerializeField] private InterestPoint_Simple[] interestPoints; // Use the renamed struct/class if you changed it

	[Header("UI References")]
	[SerializeField] private TextMeshProUGUI pointNameText;
	[SerializeField] private TextMeshProUGUI latLongText;
	[SerializeField] private TextMeshProUGUI descriptionText;
	// Removed: [SerializeField] private Image[] imagesUI;

	[Header("Panel Feedback")]
	[Tooltip("Assign your MapSelectionPanel's RectTransform here for the shake effect.")]
	[SerializeField] private RectTransform panelRectTransform;
	[Tooltip("Assign your MapSelectionPanel's Image component here for the flash effect.")]
	[SerializeField] private Image panelImage;

	// Internal State
	private int currentIndex = 0;

	private void Start()
	{
		// Ensure there are points to show
		if (interestPoints == null || interestPoints.Length == 0)
		{
			Debug.LogError("[MapSelectionManager_Simple] No interest points assigned!");
			// Optionally disable buttons or show an error message in the UI
			if (pointNameText != null) pointNameText.text = "Error: No Data";
			if (latLongText != null) latLongText.text = "";
			if (descriptionText != null) descriptionText.text = "Please assign Interest Points in the Inspector.";
			return;
		}

		// Show the first interest point by default.
		ShowInterestPoint(currentIndex);
	}

	public void NextPoint()
	{
		if (interestPoints == null || interestPoints.Length == 0) return;

		currentIndex++;
		if (currentIndex >= interestPoints.Length)
		{
			currentIndex = 0; // Loop back to the start
		}
		ShowInterestPoint(currentIndex);
		if (panelRectTransform != null && panelImage != null) // Only shake if references are set
		{
			StartCoroutine(FlashAndShakePanel());
		}
	}

	public void PrevPoint()
	{
		if (interestPoints == null || interestPoints.Length == 0) return;

		currentIndex--;
		if (currentIndex < 0)
		{
			currentIndex = interestPoints.Length - 1; // Loop back to the end
		}
		ShowInterestPoint(currentIndex);
		if (panelRectTransform != null && panelImage != null) // Only shake if references are set
		{
			StartCoroutine(FlashAndShakePanel());
		}
	}

	private void ShowInterestPoint(int index)
	{
		if (interestPoints == null || interestPoints.Length <= index || index < 0)
			return; // Safety check

		InterestPoint_Simple point = interestPoints[index];

		// Update UI text fields.
		if (pointNameText != null) pointNameText.text = point.pointName;
		if (latLongText != null) latLongText.text = $"Lat: {point.latitude:F5}, Long: {point.longitude:F5}"; // Formatted
		if (descriptionText != null) descriptionText.text = point.description;

		// No marker or globe logic needed here anymore
	}

	// Coroutine to flash the panel and shake it side to side. (Kept as requested)
	private IEnumerator FlashAndShakePanel()
	{
		// Ensure components are assigned before proceeding
		if (panelRectTransform == null || panelImage == null)
		{
			Debug.LogWarning("[MapSelectionManager_Simple] Panel RectTransform or Image not assigned for FlashAndShake effect.");
			yield break; // Exit the coroutine
		}

		// Duration and shake magnitude can be adjusted.
		float duration = 0.3f;
		float shakeMagnitude = 10f;
		// Get original values.
		Vector2 originalPos = panelRectTransform.anchoredPosition;
		Color originalColor = panelImage.color;
		// Define the flash color (white flash, adjust alpha if needed).
		Color flashColor = new Color(1f, 1f, 1f, originalColor.a); // Using white flash

		float elapsed = 0f;
		// Immediately set the flash color.
		panelImage.color = flashColor;
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float t = elapsed / duration;
			// Lerp back to original color.
			panelImage.color = Color.Lerp(flashColor, originalColor, t);
			// Calculate horizontal shake offset (oscillating and decaying).
			float offsetX = Mathf.Sin(t * Mathf.PI * 8f) * shakeMagnitude * (1f - t); // Slightly adjusted decay
			panelRectTransform.anchoredPosition = originalPos + new Vector2(offsetX, 0);
			yield return null;
		}
		// Restore original values.
		panelRectTransform.anchoredPosition = originalPos;
		panelImage.color = originalColor;
	}

	// --- Scene Loading Logic (Keep as is) ---
	public void GoToNextScene()
	{
		if (interestPoints == null || interestPoints.Length == 0)
		{
			Debug.LogError("[MapSelectionManager_Simple] No interest points set!");
			return;
		}

		// Safety check for valid index
		if (currentIndex < 0 || currentIndex >= interestPoints.Length)
		{
			Debug.LogError($"[MapSelectionManager_Simple] Invalid current index: {currentIndex}");
			return;
		}

		string nextSceneName = interestPoints[currentIndex].sceneName;
		if (string.IsNullOrEmpty(nextSceneName))
		{
			Debug.LogError($"[MapSelectionManager_Simple] Scene name is not set for interest point: {interestPoints[currentIndex].pointName}");
			return;
		}

		// Make sure the Loader class exists and is accessible
		// Ensure your Loader script (Loader.cs) is correctly set up in your project.
		try
		{
			Loader.NextScene = nextSceneName;
			SceneManager.LoadScene("LoadingScreen"); // Assumes you have a scene named "LoadingScreen"
		}
		catch (System.Exception e)
		{
			Debug.LogError($"[MapSelectionManager_Simple] Error loading scene: {e.Message}. Ensure Loader class is available and LoadingScreen scene exists.");
		}
	}
	public void BackToStart()
	{


		// Make sure the Loader class exists and is accessible
		// Ensure your Loader script (Loader.cs) is correctly set up in your project.
		try
		{
			Loader.NextScene = "StartMenu";
			SceneManager.LoadScene("LoadingScreen"); // Assumes you have a scene named "LoadingScreen"
		}
		catch (System.Exception e)
		{
			Debug.LogError($"[MapSelectionManager_Simple] Error loading scene: {e.Message}. Ensure Loader class is available and LoadingScreen scene exists.");
		}
	}
}