using UnityEngine;
using UnityEngine.UI;
using TMPro;  // Import TMP for TextMeshPro support

public class TimeSlider : MonoBehaviour
{
    public Slider TimeSpeedSlider;  
    public TMP_Text SpeedDisplayText;  // Changed to TMP_Text for TMP support
    private float normalTimeScale = 1f;  
    private float targetTimeScale;  

    void Start()
    {
        targetTimeScale = TimeSpeedSlider.value;  
        UpdateSpeedText();  
    }

    void Update()
    {
        targetTimeScale = TimeSpeedSlider.value;  
        UpdateSpeedText();  

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            Time.timeScale = targetTimeScale;  
        }
        else
        {
            Time.timeScale = normalTimeScale;  
        }
    }

    void UpdateSpeedText()
    {
        if (SpeedDisplayText != null)
        {
            SpeedDisplayText.text = "Speed: " + targetTimeScale.ToString("F1") + "x";
        }
    }
}