using UnityEngine;
using UnityEngine.UI;

public class SpeedSlider : MonoBehaviour
{
    public Slider speedSlider;  // Reference to the UI Slider
    private float selectedSpeed = 1f;  // Stores the chosen speed

    private void Start()
    {
        speedSlider.onValueChanged.AddListener(UpdateSpeed);
        
        // Prevents external scripts from modifying the slider value
        speedSlider.SetValueWithoutNotify(speedSlider.value);
    }

    private void UpdateSpeed(float value)
    {
        selectedSpeed = value;
    }

    public float GetSelectedSpeed()
    {
        return selectedSpeed;
    }
}