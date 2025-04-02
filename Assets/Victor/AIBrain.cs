using UnityEngine;
using System.Collections.Generic;

public class AIBrain : MonoBehaviour
{
    public float samplingRadius = 10f;
    public LayerMask rockLayerMask;

    public string Sample()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, samplingRadius, rockLayerMask);
        List<GameObject> candidateRocks = new List<GameObject>();
        float highestRarity = float.MinValue;

        foreach (Collider col in colliders)
        {
            SampleInfo info = col.GetComponent<SampleInfo>();
            if (info != null)
            {
                float rarity = info.GetRarity();
                if (rarity > highestRarity)
                {
                    highestRarity = rarity;
                    candidateRocks.Clear();
                    candidateRocks.Add(col.gameObject);
                }
                else if (Mathf.Approximately(rarity, highestRarity))
                {
                    candidateRocks.Add(col.gameObject);
                }
            }
        }

        if (candidateRocks.Count > 0)
        {
            GameObject selectedRock = candidateRocks[Random.Range(0, candidateRocks.Count)];
            Debug.Log("Selected rock: " + selectedRock.name + " with rarity: " + highestRarity);
            return selectedRock.name;
        }
        Debug.Log("No rock available for sampling.");
        return "None";
    }
}
