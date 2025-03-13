using System;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class Teleport_To_Spawn : MonoBehaviour
{
    public Vector3 targetPosition; // Set this manually in the inspector for now
    public Button teleportButton;  // Assign the UI button manually in the inspector
    public Rigidbody rb; // Assign the rigid body in the inspector

    void Start()
    {
        teleportButton.onClick.RemoveAllListeners();
        teleportButton.onClick.AddListener(TeleportRover);
    }

    public void TeleportRover()
    {
        if ((rb.linearVelocity == Vector3.zero) && (rb.angularVelocity == Vector3.zero))
        {
            //rb.rotation = Quaternion.identity;
            rb.MovePosition(targetPosition);
            Debug.Log("Rover teleported to: " + targetPosition);
        }
    }
}
