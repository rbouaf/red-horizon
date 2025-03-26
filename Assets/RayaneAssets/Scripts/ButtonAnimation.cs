using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ButtonFeedback : MonoBehaviour
{
    // How long the flash/shake effect lasts.
    public float flashDuration = 0.2f;
    // The color the button flashes (orange).
    public Color flashColor = new Color(1f, 0.5f, 0f, 1f);
    // How much the button shakes horizontally (in pixels).
    public float shakeAmount = 10f;

    // Optionally assign the target Image manually if not found automatically.
    [SerializeField] private Image targetImage;

    private Vector3 originalPosition;
    private Color originalColor;

    private void Awake()
    {
        // Try to find the Image component if not assigned.
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
            if (targetImage == null)
            {
                targetImage = GetComponentInChildren<Image>();
            }
            if (targetImage == null)
            {
                targetImage = GetComponentInParent<Image>();
            }
        }

        if (targetImage == null)
        {
            Debug.LogError("ButtonFeedback: No Image component found on this GameObject or its parents/children. Please assign one in the Inspector.");
        }
        else
        {
            originalColor = targetImage.color;
        }
        originalPosition = transform.localPosition;
    }

    // Call this method when the button is clicked.
    public void PlayFeedback()
    {
        if (targetImage != null)
        {
            StopAllCoroutines();
            StartCoroutine(FeedbackRoutine());
        }
    }

    private IEnumerator FeedbackRoutine()
    {
        float timer = 0f;
        while (timer < flashDuration)
        {
            timer += Time.deltaTime;
            float t = timer / flashDuration;
            
            // Flash effect: Ping-pong interpolation for color transition.
            float flashT = Mathf.PingPong(t * 2f, 1f);
            targetImage.color = Color.Lerp(originalColor, flashColor, flashT);
            
            // Shake effect: Oscillate horizontally using a sine wave.
            float shakeOffset = Mathf.Sin(timer * Mathf.PI * 10f) * shakeAmount * (1f - t);
            transform.localPosition = originalPosition + new Vector3(shakeOffset, 0f, 0f);
            
            yield return null;
        }
        // Reset button state.
        targetImage.color = originalColor;
        transform.localPosition = originalPosition;
    }
}
