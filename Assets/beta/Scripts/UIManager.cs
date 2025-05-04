using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // For TextMeshPro support

public class UIManager : MonoBehaviour
{
	private void Awake()
    {
        Screen.SetResolution(910, 512, FullScreenMode.Windowed);
    }

	[SerializeField] private GameObject startMenuPanel;
	[SerializeField] private GameObject mapSelectionPanel;
	[SerializeField] private Transform marsGlobe;
	[SerializeField] private Transform atmosphereTransform;
	[SerializeField] private ParticleSystem warp1;
	[SerializeField] private ParticleSystem warp2;

	private Vector3 startPosition;
	private Vector3 endPosition = new Vector3(6, 1, 0);
	private Vector3 startScale;
	private Vector3 endScale = new Vector3(8, 8, 8);
	private float transitionTime = 2f;
	private CanvasGroup startMenuCanvasGroup;
	private CanvasGroup mapSelectionCanvasGroup;

	private void Start()
	{
		// Save initial transform values of the globe
		startPosition = marsGlobe.position;
		startScale = marsGlobe.localScale;
		SetSkyboxExposure();

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

		
		startMenuPanel.SetActive(true);
		mapSelectionPanel.SetActive(false);
		warp1.gameObject.SetActive(false);
		warp2.gameObject.SetActive(false);
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

	void SetSkyboxExposure()
	{
		// Get the current skybox material from RenderSettings
		Material skyboxMat = RenderSettings.skybox;

		// Make sure the material has the _Exposure property (Procedural Skybox does)
		if (skyboxMat.HasProperty("_Exposure"))
		{
			// Decrease it to darken
			float newExposure = 0.9f;

			// Apply the new exposure value
			skyboxMat.SetFloat("_Exposure", newExposure);

			Debug.Log("Skybox Exposure set to: " + newExposure);
		}
		else
		{
			Debug.LogWarning("Skybox material does not have an _Exposure property!");
		}
	}
	void DarkenSkyboxExposure()
	{
		// Get the current skybox material from RenderSettings
		Material skyboxMat = RenderSettings.skybox;

		// Make sure the material has the _Exposure property (Procedural Skybox does)
		if (skyboxMat.HasProperty("_Exposure"))
		{
			// Grab the current exposure

			// Decrease it to darken
			float newExposure = 0f;

			// Apply the new exposure value
			skyboxMat.SetFloat("_Exposure", newExposure);

			Debug.Log("Skybox Exposure set to: " + newExposure);
		}
		else
		{
			Debug.LogWarning("Skybox material does not have an _Exposure property!");
		}
	}

	private IEnumerator FadeOutAtmosphere(Transform atmosphereTransform, float duration)
	{
		// Get all MeshRenderers on the atmosphere object (and its children)
		MeshRenderer[] renderers = atmosphereTransform.GetComponentsInChildren<MeshRenderer>();
		float elapsedTime = 0f;

		// Assume the FadeAlpha property is set up in your shader (named "_FadeAlpha")
		while (elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;
			float t = Mathf.Clamp01(elapsedTime / duration);
			// Lerp from fully visible (1) to fully faded (0)
			float currentFade = Mathf.Lerp(1f, 0f, t);

			// Apply the fade value to each material on each renderer
			foreach (MeshRenderer renderer in renderers)
			{
				foreach (Material mat in renderer.materials)
				{
					mat.SetFloat("_SetAlpha", currentFade);
				}
			}
			yield return null;
		}

		// Ensure the atmosphere is completely faded out
		foreach (MeshRenderer renderer in renderers)
		{
			foreach (Material mat in renderer.materials)
			{
				mat.SetFloat("_SetAlpha", 0f);
			}
		}
		foreach (MeshRenderer renderer in renderers)
		{
			renderer.gameObject.SetActive(false);
		}
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
		foreach (MeshRenderer renderer in renderers)
		{
			renderer.gameObject.SetActive(false);
		}
	}

}
