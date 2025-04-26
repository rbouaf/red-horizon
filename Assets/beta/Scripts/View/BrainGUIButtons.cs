using UnityEngine;
using TMPro;

public class BrainGUIButtons : MonoBehaviour
{
    public TextMeshProUGUI PathfindingAI;
    public TextMeshProUGUI SamplingAI;    

    
    public void ShowMessagePath()
    {
        PathfindingAI.text = "Pathfinding AI Activated";
    
        Invoke("RevertPathfindingText", 2f);
    }

       public void ShowMessageSampling()
    {
        SamplingAI.text = "Sampling AI Activated";
       
        Invoke("RevertSamplingText", 2f);
    }

   
    void RevertPathfindingText()
    {
        PathfindingAI.text = "Pathfinding AI"; 
    }

    void RevertSamplingText()
    {
        SamplingAI.text = "Sampling AI";
    }
}
