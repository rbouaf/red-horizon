using UnityEngine;
using TMPro; // For TextMeshPro

public class PowerManager : MonoBehaviour
{
    public float batteryLevel = 100f;  // Battery starts full
    public float drainRate = 10f / 60f; // Power drains at 10% per minute
    public TMP_Text batteryText;           // Reference to UI text

    void Update()
    {
        DrainPower();  // Reduce battery over time
        UpdateUI();    // Update UI display
    }

    void DrainPower()
    {
        batteryLevel -= drainRate * Time.deltaTime;  
        batteryLevel = Mathf.Clamp(batteryLevel, 0f, 100f);  // Keep battery between 0-100%
    }

    void UpdateUI()
    {
        if (batteryText != null)
        {
            batteryText.text = "Battery: " + batteryLevel.ToString("F1") + "%";

            // Change text color based on battery level
            if (batteryLevel < 96f)
                batteryText.color = Color.red; // Low battery warning
            else
                batteryText.color = Color.white; 
        }
    }
}
