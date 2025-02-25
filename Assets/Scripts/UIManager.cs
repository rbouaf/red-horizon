using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject startMenuPanel; // We'll link the panel with the Start Button
    [SerializeField] private GameObject mapSelectionPanel; // We'll link the panel that has the map selection UI

    [SerializeField] private Transform marsGlobe; // reference to the Mars sphere transform

    // If you animate the globe's transform, you can store the "start" position, "end" position etc. 
    private Vector3 startPosition = new Vector3(0, 0, 0);
    private Vector3 endPosition   = new Vector3(6, 1, 0);

    // We'll do a simple interpolation in code. 
    // Alternatively, you can do a Unity Animation or a tween system like DOTween.
    private bool isTransitioning = false;
    private float transitionTime = 0.5f;
    private float elapsedTime = 0f;

    private void Start()
    {
        // Make sure Start Menu is visible and Map Selection is hidden at the beginning.
        startMenuPanel.SetActive(true);
        mapSelectionPanel.SetActive(false);
    }

    public void OnStartButtonClicked()
    {
        // Hide the start menu UI
        startMenuPanel.SetActive(false);

        // Show the map selection UI
        mapSelectionPanel.SetActive(true);

        // Start the transition of the globe
        isTransitioning = true;
        elapsedTime = 0f;
    }

    private void Update()
    {
        if (isTransitioning)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / transitionTime);

            // Move globe from start to end
            marsGlobe.position = Vector3.Lerp(startPosition, endPosition, t);
            // Optionally scale up:
            marsGlobe.localScale = Vector3.Lerp(new Vector3(2,2,2), new Vector3(8,9,8), t);

            if (t >= 1f)
            {
                isTransitioning = false;
            }
        }
    }
}
