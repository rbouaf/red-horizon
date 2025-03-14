using UnityEngine;

public class CheckpointPopUp : MonoBehaviour
{
    private CheckpointData data;
    private CheckpointManager manager;
    private bool hasTriggered = false;

    // Call this method when instantiating the checkpoint.
    public void Setup(CheckpointData data, CheckpointManager manager)
    {
        this.data = data;
        this.manager = manager;
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the rover (or player) has entered the checkpoint trigger.
        if (!hasTriggered && other.CompareTag("Rover"))
        {
            hasTriggered = true;
            if (manager != null)
            {
                manager.ShowCheckpointUI(data);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Hide the UI when the rover leaves the trigger.
        if (hasTriggered && other.CompareTag("Rover"))
        {
            hasTriggered = false;
            if (manager != null)
            {
                manager.HideCheckpointUI();
            }
        }
    }
}
