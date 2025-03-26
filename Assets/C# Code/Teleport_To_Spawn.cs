using System;
using System.Collections;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Teleport_To_Spawn : MonoBehaviour
{
    public Vector3 targetPosition; // Set this manually in the inspector for now
    public Button teleportButton;  // Assign the UI button manually in the inspector
    public Rigidbody rb; // Assign the rigid body in the inspector
    public TextMeshProUGUI buttonText; //Assign the text of the button in the inspector
    private String originalButtonText; //Store the original text of the teleport button

    void Start()
    {
        teleportButton.onClick.RemoveAllListeners();
        teleportButton.onClick.AddListener(TeleportRover);
        originalButtonText = buttonText.text;
    }

    public void TeleportRover()
    {
        if ((rb.linearVelocity == Vector3.zero) && (rb.angularVelocity == Vector3.zero))
        if ((Math.Abs(rb.linearVelocity.x) <= 0.2f) && (Math.Abs(rb.linearVelocity.z) <= 0.2f) && (rb.angularVelocity == Vector3.zero))
        {
            rb.position = targetPosition;
            Debug.Log("Teleported rover to " + targetPosition);
        }
        else
        {
            StartCoroutine(ShowMovingWarning());
        }
    }

    IEnumerator ShowMovingWarning() 
    {
        buttonText.text = "Must not be moving!";
        yield return new WaitForSeconds(3);
        buttonText.text = originalButtonText;
    }
}