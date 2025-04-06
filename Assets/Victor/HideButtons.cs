using UnityEngine;
using UnityEngine.UI;

public class HideButtons : MonoBehaviour
{
    public Button[] buttonsToHide;

    public void Hidebuttons()
    {
        foreach (Button btn in buttonsToHide)
        {
            if (btn != null)
            {
                btn.gameObject.SetActive(false);
            }
        }
    }
}

