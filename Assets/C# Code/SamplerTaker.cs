using UnityEngine;

public class SamplerTaker : MonoBehaviour
{
    public TMPro.TextMeshProUGUI SampleTaker; 


    void Start()
    {
        SampleTaker.text = "";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CanBeTaken"))
        {
         SampleTaker.text = "Touched: " + other.gameObject.name;
        }
    }

   
    private void OnTriggerExit(Collider other)
    {
        SampleTaker.text = "";
    }
}
