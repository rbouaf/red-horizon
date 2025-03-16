using UnityEngine;

public class Checkpoint : MonoBehaviour {
    private CheckpointData data;
    private CheckpointManager manager;
    private bool hasTriggered = false;

    public void Setup(CheckpointData data, CheckpointManager manager) {
        this.data = data;
        this.manager = manager;
    }

    void OnTriggerEnter(Collider other) {
        if (!hasTriggered && other.CompareTag("Rover")) { // Make sure rover has the "Rover" tag
            hasTriggered = true;
            if (manager != null) {
                manager.ShowCheckpointUI(data);
            }
        }
    }

    void OnTriggerExit(Collider other) {
        if (hasTriggered && other.CompareTag("Rover")) {
            hasTriggered = false;
            if (manager != null) {
                manager.HideCheckpointUI();
            }
        }
    }
}
