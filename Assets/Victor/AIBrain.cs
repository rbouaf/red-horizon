using UnityEngine;
using System.Collections.Generic;

public class AIBrain : MonoBehaviour, IBrain 
{
    public string DecideSample(GameObject sampleObject)
    {
        // Attempt to get the SampleInfo component from the sampleObject
        SampleInfo info = sampleObject.GetComponent<SampleInfo>();
        if (info != null)
        {
            // Create a dictionary mapping element names to their concentration values
            Dictionary<string, float> sampleScores = new Dictionary<string, float>
            {
                { "Basaltic", info.basalticConcentration }
                { "Andesitic", info.andesiticConcentration },
                { "Sedimentary", info.sedimentaryConcentration },
                { "Clay", info.clayConcentration },
                { "Quartz", info.quartzConcentration },
                { "Volcaniclastic", info.volcaniclasticConcentration }
            };

            // Evaluate the sensor data by finding the element with the highest concentration
            string bestSample = "Unknown";
            float bestScore = float.MinValue;
            foreach (var sample in sampleScores)
            {
                if (sample.Value > bestScore)
                {
                    bestScore = sample.Value;
                    bestSample = sample.Key;
                }
            }
            Debug.Log("AI Brain decided on: " + bestSample + " based on sensor data.");
            return bestSample;
        }
        else
        {
            // Fallback decision if no sensor data is available
            Debug.LogWarning("SampleInfo component not found on sampleObject. Defaulting to Titanium.");
            return "Titanium";
        }
    }
}
