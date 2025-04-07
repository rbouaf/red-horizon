using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class AIsampleBrain : MonoBehaviour
{
    //(assign in inspector if AIBrain is not on the rover itself)
    public Transform roverTransform;

    public float samplingRadius = 15f;

    public LayerMask rockLayerMask;

    public TextMeshProUGUI messageText;

    public Color glowColor = Color.yellow;
    public float glowDuration = 6f;

    public float forwardThreshold = 0.5f;

    public void StartSampling()
    {
        StartCoroutine(SampleRoutine());
    }

    public IEnumerator SampleRoutine()
    {
        
        if (messageText != null)
        {
            messageText.text = "Sampling in process... Scanning...";
        }

       
        yield return new WaitForSeconds(4f);

        // Use the rover's position as the center of the scan.
        Vector3 sampleOrigin = (roverTransform != null) ? roverTransform.position : transform.position;
        Collider[] colliders = Physics.OverlapSphere(sampleOrigin, samplingRadius, rockLayerMask);
        List<GameObject> candidateRocks = new List<GameObject>();
        float highestRarity = float.MinValue;

        
        foreach (Collider col in colliders)
        {
            // Check if the rock is in front of the rover using dot product.
            Vector3 directionToRock = (col.transform.position - sampleOrigin).normalized;
            Vector3 roverForward = (roverTransform != null) ? roverTransform.forward : transform.forward;
            if (Vector3.Dot(directionToRock, roverForward) < forwardThreshold)
            {
                // Skip rocks that are not sufficiently in front.
                continue;
            }
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
               // add to candidates if two are tied for rarity
                else if (Mathf.Approximately(rarity, highestRarity))
                {
                    candidateRocks.Add(col.gameObject);
                }
            }
        }

        // Display the result
        if (candidateRocks.Count > 0)
        {
            // randomly choose one if there are multiple candidates.
            GameObject selectedRock = candidateRocks[Random.Range(0, candidateRocks.Count)];
            if (messageText != null)
            {
                messageText.text = "Best rock found: " + selectedRock.tag + "\nRarity score: " + highestRarity;
            }
            Debug.Log("Selected rock: " + selectedRock.tag + " with rarity: " + highestRarity);
            MakeRockGlow(selectedRock);
        }
        else
        {
            if (messageText != null)
            {
                messageText.text = "No rock found in range.";
            }
            Debug.Log("No rock available for sampling.");
        }
    }

    private void MakeRockGlow(GameObject rock)
    {
        Renderer rend = rock.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.EnableKeyword("_EMISSION");
            rend.material.SetColor("_EmissionColor", glowColor);

            // reset the emission after a delay.
            StartCoroutine(ResetGlow(rend));
        }
        else
        {
            Debug.LogWarning("Renderer not found on selected rock.");
        }
    }

    // Resets the emission after a specified duration.
    private IEnumerator ResetGlow(Renderer rend)
    {
        yield return new WaitForSeconds(glowDuration);
        
        rend.material.SetColor("_EmissionColor", Color.black);
    }
}
