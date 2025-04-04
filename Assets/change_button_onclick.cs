using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System;

public class change_button_onclick : MonoBehaviour
{
    public Button button;
    public TMP_Text buttonText; // Assigned in Inspector
    private String originalButtonText; //Store the original text of the brush button

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button.onClick.AddListener(ToggleButtontext);
        originalButtonText = buttonText.text;
    }

    void ToggleButtontext() 
    {
        if (buttonText.text == originalButtonText)
        {
            buttonText.text = "Show";
        }
        else 
        {
            buttonText.text = originalButtonText;
        }
    }
}

