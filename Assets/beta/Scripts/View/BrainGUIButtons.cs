using UnityEngine;
using TMPro;

public class BrainGUIButtons : MonoBehaviour
{
    public TextMeshProUGUI PathfindingAI;
    public TextMeshProUGUI SamplingAI;    

    public TextMeshProUGUI FreeRoamAI; 

    
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

    public void ShowMessageFreeRoam()
    {
        FreeRoamAI.text = "Free Roam Activated";
       
        Invoke("RevertFreeRoamText", 2f);
    }

void RevertFreeRoamText()
    {
        FreeRoamAI.text = "Pathfinding AI"; 
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
