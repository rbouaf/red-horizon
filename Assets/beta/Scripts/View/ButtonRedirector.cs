using UnityEngine;
using UnityEngine.UI;

public class ButtonRedirector : MonoBehaviour
{
    public Button targetButton;

    public void ShowTargetButton()
    {
        if (targetButton != null)
        {
            targetButton.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Target Button is not assigned!");
        }
    }
}
