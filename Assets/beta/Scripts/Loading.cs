using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingScreenManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject loadingPanel;            
    [SerializeField] private CanvasGroup loadingPanelCanvasGroup;
    [SerializeField] private ParticleSystem warp1, warp2;

    [Header("Timings")]
    [SerializeField] private float fadeInDuration  = 1.5f;
    [SerializeField] private float fadeOutDuration = 1.0f;

    private void Awake()
    {
        // Ensure panel is active but invisible at start
        loadingPanel.SetActive(true);
        loadingPanelCanvasGroup.alpha = 0f;

        // Ensure warps are hidden until after fade‐in
        warp1.gameObject.SetActive(false);
        warp2.gameObject.SetActive(false);
    }

    private void Start()
    {
        StartCoroutine(DoLoading());
    }

    private IEnumerator DoLoading()
{
	warp1.gameObject.SetActive(true);
	warp1.Play();
	warp2.gameObject.SetActive(true);
	warp2.Play();
    // 1) Make sure the panel is active and starts fully transparent
    loadingPanel.SetActive(true);
    loadingPanelCanvasGroup.alpha = 0f;
	
	yield return new WaitForSeconds(0.5f);

    // 2) Fade in once (from 0 → 1) and then hold at 1
    yield return FadeCanvasGroup(loadingPanelCanvasGroup, 0f, 1f, fadeInDuration);


    // 4) Async‐load the next scene (holding activation until it's done)
    AsyncOperation op = SceneManager.LoadSceneAsync(Loader.NextScene);
    op.allowSceneActivation = false;

    // 5) Wait until the heavy load is essentially finished
    while (op.progress < 0.9f)
        yield return null;

    // 6) Now let Unity swap scenes (no fade-out)
    op.allowSceneActivation = true;
}


    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float startAlpha, float endAlpha, float duration)
    {
        float time = 0f;
        cg.alpha = startAlpha;
        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, time / duration);
            yield return null;
        }
        cg.alpha = endAlpha;
    }
}
