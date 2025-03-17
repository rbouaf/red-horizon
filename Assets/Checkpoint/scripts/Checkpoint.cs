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
        Debug.Log($"OnTriggerEnter triggered on {gameObject.name} with {other.gameObject.name}");
        if (!hasTriggered && other.CompareTag("Rover")) {
            hasTriggered = true;
            if (manager != null) {
                manager.ShowCheckpointUI(data);
            }
        }
    }

    void OnTriggerExit(Collider other) {
        Debug.Log($"OnTriggerExit triggered on {gameObject.name} with {other.gameObject.name}");
        if (hasTriggered && other.CompareTag("Rover")) {
            hasTriggered = false;
            if (manager != null) {
                manager.HideCheckpointUI();
                manager.HideButton();
            }
        }
    }
}
