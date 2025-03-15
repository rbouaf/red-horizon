using UnityEngine;
using System;
using System.Collections.Generic;



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

         //SampleTaker.text = "Touched: " + other.gameObject.name;
    
        List<string> mylist = new List<string> {"Magnesium", "Aluminium", "Titanium", "Iron", "Chromium"};



        int randomNumber = UnityEngine.Random.Range(1,mylist.Count);
        Debug.Log(randomNumber);

    

        SampleTaker.text = "Touched: " + mylist[randomNumber];
        
      


        }
    }

   
    private void OnTriggerExit(Collider other)
    {
        SampleTaker.text = "";
    }
}
