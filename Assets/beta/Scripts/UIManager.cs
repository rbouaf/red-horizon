using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // For TextMeshPro support

public class UIManager : MonoBehaviour
{
	[SerializeField] private GameObject startMenuPanel;
	[SerializeField] private GameObject mapSelectionPanel;
	[SerializeField] private Transform marsGlobe;
	[SerializeField] private GameObject loadingPanel; // Assign in inspector

	private Vector3 startPosition;
	private Vector3 endPosition = new Vector3(6, 1, 0);
	private Vector3 startScale;
	private Vector3 endScale = new Vector3(8, 8, 8);
	private float transitionTime = 2f;
	private CanvasGroup startMenuCanvasGroup;
	private CanvasGroup mapSelectionCanvasGroup;
	private CanvasGroup loadingPanelCanvasGroup;

	private void Start()
	{
		// Save initial transform values of the globe
		startPosition = marsGlobe.position;
		startScale = marsGlobe.localScale;

		// Ensure the start menu has a CanvasGroup for fading
		startMenuCanvasGroup = startMenuPanel.GetComponent<CanvasGroup>();
		if (startMenuCanvasGroup == null)
		{
			startMenuCanvasGroup = startMenuPanel.AddComponent<CanvasGroup>();
		}
		startMenuCanvasGroup.alpha = 1f; // Fully visible

		// Ensure the map selection panel has a CanvasGroup for fading
		mapSelectionCanvasGroup = mapSelectionPanel.GetComponent<CanvasGroup>();
		if (mapSelectionCanvasGroup == null)
		{
			mapSelectionCanvasGroup = mapSelectionPanel.AddComponent<CanvasGroup>();
		}
		mapSelectionCanvasGroup.alpha = 0f; // Start hidden

		// Ensure the loading panel has a CanvasGroup for fading
		CanvasGroup loadingPanelCanvasGroup = loadingPanel.GetComponent<CanvasGroup>();
		if (loadingPanelCanvasGroup == null)
		{
			loadingPanelCanvasGroup = loadingPanel.AddComponent<CanvasGroup>();
		}
		loadingPanelCanvasGroup.alpha = 0f; // Start hidden
		startMenuPanel.SetActive(true);
		mapSelectionPanel.SetActive(false);
		loadingPanel.SetActive(false);
	}

	public void OnStartButtonClicked()
	{
		// Begin fade-out and transition
		StartCoroutine(FadeOutAndTransition());
	}

	private IEnumerator FadeOutAndTransition()
	{
		// Fade out the start menu over 1 second
		yield return StartCoroutine(FadeOutCanvasGroup(startMenuCanvasGroup, 1f));

		// Switch panels after fade-out is complete
		startMenuPanel.SetActive(false);
		mapSelectionPanel.SetActive(true);

		// Make markers visible by calling ShowMarkers on MapSelectionManager.
		FindObjectOfType<MapSelectionManager>().ShowMarkers();

		// Start the globe transition concurrently
		StartCoroutine(GlobeTransition());

		// Delay 1 second before starting the fade-in of the map selection panel
		yield return new WaitForSeconds(1f);
		StartCoroutine(FadeInCanvasGroup(mapSelectionCanvasGroup, 2f));
	}

	private IEnumerator GlobeTransition()
	{
		float elapsedTime = 0f;
		while (elapsedTime < transitionTime)
		{
			elapsedTime += Time.deltaTime;
			float rawT = Mathf.Clamp01(elapsedTime / transitionTime);
			// Apply SmoothStep for ease in–out: smooth acceleration and deceleration
			float t = rawT * rawT * (3f - 2f * rawT);

			marsGlobe.position = Vector3.Lerp(startPosition, endPosition, t);
			marsGlobe.localScale = Vector3.Lerp(startScale, endScale, t);

			yield return null;
		}
		// Ensure the globe stays at the final position and scale
		marsGlobe.position = endPosition;
		marsGlobe.localScale = endScale;
	}

	private IEnumerator FadeOutCanvasGroup(CanvasGroup cg, float duration)
	{
		float time = 0f;
		while (time < duration)
		{
			time += Time.deltaTime;
			cg.alpha = Mathf.Lerp(1f, 0f, time / duration);
			yield return null;
		}
		cg.alpha = 0f;
	}

	private IEnumerator FadeInCanvasGroup(CanvasGroup cg, float duration)
	{
		float time = 0f;
		while (time < duration)
		{
			time += Time.deltaTime;
			cg.alpha = Mathf.Lerp(0f, 1f, time / duration);
			yield return null;
		}
		cg.alpha = 1f;
	}
	public IEnumerator ShowLoadingScreen(string sceneName)
	{
		// Fade out the map selection panel.
		yield return StartCoroutine(FadeOutCanvasGroup(mapSelectionCanvasGroup, 1f));
		Debug.LogError("fading globe...");

		yield return StartCoroutine(FadeOutGlobe(marsGlobe, 1f));


		// Wait for a 1 second delay.
		yield return new WaitForSeconds(1f);

		// Activate and fade in the loading panel.
		loadingPanel.SetActive(true);
		yield return StartCoroutine(FadeInCanvasGroup(loadingPanelCanvasGroup, 1f));

		
	}
	private IEnumerator FadeOutGlobe(Transform globeTransform, float duration)
	{
		Debug.LogError("Fading out globe");

		// Get all MeshRenderers on the globe (including children)
		MeshRenderer[] renderers = globeTransform.GetComponentsInChildren<MeshRenderer>();
		float elapsedTime = 0f;

		// Cache the original colors for each renderer and material.
		Color[][] originalColors = new Color[renderers.Length][];
		for (int i = 0; i < renderers.Length; i++)
		{
			Material[] mats = renderers[i].materials;
			originalColors[i] = new Color[mats.Length];
			for (int j = 0; j < mats.Length; j++)
			{
				originalColors[i][j] = mats[j].color;
			}
		}

		// Fade loop: interpolate the alpha from 1 to 0.
		while (elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;
			float t = Mathf.Clamp01(elapsedTime / duration);
			for (int i = 0; i < renderers.Length; i++)
			{
				Material[] mats = renderers[i].materials;
				for (int j = 0; j < mats.Length; j++)
				{
					Color c = originalColors[i][j];
					c.a = Mathf.Lerp(1f, 0f, t);
					mats[j].color = c;
				}
			}
			yield return null;
		}
		// Ensure alpha is fully 0 at the end.
		for (int i = 0; i < renderers.Length; i++)
		{
			Material[] mats = renderers[i].materials;
			for (int j = 0; j < mats.Length; j++)
			{
				Color c = originalColors[i][j];
				c.a = 0f;
				mats[j].color = c;
			}
		}
	}

}
