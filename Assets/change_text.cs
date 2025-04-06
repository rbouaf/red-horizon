using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System;

public class change_text : MonoBehaviour
{
    public Button myButton;
    public TMP_Text buttonText; // Assigned in Inspector
    private String originalButtonText; //Store the original text of the brush button

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myButton.onClick.AddListener(ChangeButtonText);
        originalButtonText = buttonText.text;
    }

    void ChangeButtonText()
    {
        StartCoroutine(UpdateButtonText());
    }

    IEnumerator UpdateButtonText()
    {
        buttonText.text = "Brushing complete!";
        yield return new WaitForSeconds(3);
        buttonText.text = originalButtonText;
    }
}
