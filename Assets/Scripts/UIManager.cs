using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // For TextMeshPro support

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject startMenuPanel; 
    [SerializeField] private GameObject mapSelectionPanel; 
    [SerializeField] private Transform marsGlobe; 

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

        // Activate only the start menu at launch
        startMenuPanel.SetActive(true);
        mapSelectionPanel.SetActive(false);
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
}
