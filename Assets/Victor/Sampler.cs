using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class SamplerTaker : MonoBehaviour
{
    public TMPro.TextMeshProUGUI Sampler;
    // Reference a MonoBehaviour that implements IBrain (assign this in the Unity Inspector)
    public AIBrain BrainComponent;


    void Start()
    {
        Sampler.text = "";
        if (BrainComponent == null)
        {
            Debug.LogError("Assigned BrainComponent does not implement IBrain!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CanBeTaken") && BrainComponent != null)
        {
            string chosenSample = BrainComponent.DecideSample(other.gameObject);
            Sampler.text = "Touched: " + chosenSample;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Sampler.text = "";
    }
}
