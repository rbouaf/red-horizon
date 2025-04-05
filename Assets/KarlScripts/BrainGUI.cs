using UnityEngine;

public class BrainGUI : MonoBehaviour
{
    public GameObject menuPanel;
    

    void Start()
    {
        if (menuPanel.name == "Left-Side Buttons")
        {
            menuPanel.SetActive(true); 
        }
        else{
            menuPanel.SetActive(false);
        }
    }

    public void ToggleMenu()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(!menuPanel.activeSelf);
        }
    }

    
}


