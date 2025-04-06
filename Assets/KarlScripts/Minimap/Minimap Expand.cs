using UnityEngine;
using TMPro;

public class MinimapExpand : MonoBehaviour
{
    public RectTransform minimap;
    public RectTransform background;

    public Camera minimapCamera; 


    public GameObject button1;
    public GameObject button2;
    public GameObject button3;

    public GameObject envatmosphericcontent;
    public GameObject background1;

    public TMPro.TextMeshProUGUI ExpandMinimize;


    public Vector2 normalSize = new Vector2(350, 280);  
    public Vector2 expandedSize = new Vector2(2000, 1500); 

    private bool isExpanded = false;
    private Vector2 originalPosition; 
    private Vector2 originalPosition2; 

    void Start()
    {
        originalPosition = minimap.anchoredPosition; 
        originalPosition2 = background.anchoredPosition;
        minimapCamera.orthographicSize = 10f;
        ExpandMinimize.text = "Expand";
    }

    public void ToggleMinimap()
    {
        if (isExpanded)
        {
        
            minimapCamera.orthographicSize = 10f;
            minimap.localScale = Vector3.one;
            minimap.anchoredPosition = originalPosition; 

            background.localScale = Vector3.one;
            background.anchoredPosition = originalPosition2;

            button1.gameObject.SetActive(true);
            button2.gameObject.SetActive(true);
            button3.gameObject.SetActive(true);
            envatmosphericcontent.gameObject.SetActive(true);
            background1.gameObject.SetActive(true);
            ExpandMinimize.text = "Expand";

        }
        else
        {


            minimapCamera.orthographicSize = 30f;
            
            minimap.localScale = new Vector3(8f,8f, 1f);
            minimap.anchoredPosition = Vector2.zero;  

            background.localScale = new Vector3(8f, 8f, 1f);  
            background.anchoredPosition = Vector2.zero;  

            button1.gameObject.SetActive(false);
            button2.gameObject.SetActive(false);
            button3.gameObject.SetActive(false);
            envatmosphericcontent.gameObject.SetActive(false);
            background1.gameObject.SetActive(false);
            ExpandMinimize.text = "Minimize";
        }

        isExpanded = !isExpanded; 
    }
}
