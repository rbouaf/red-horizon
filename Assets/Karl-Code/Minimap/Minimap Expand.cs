using UnityEngine;


public class MinimapExpand : MonoBehaviour
{
    
    public RectTransform minimapRectTransform; 
    public GameObject test;
    public Transform playerOnMap;
    public Camera minimapCamera;         
    public Vector2 collapsedSize = new Vector2(350, 280);  
    public Vector2 expandedSize = new Vector2(3000, 3000); 
    public Vector2 expandedSizePlayer = new Vector2(100, 100); 
    private bool isExpanded = false;        

    private Vector2 originalPosition;

    void Start()
    {
        originalPosition = minimapRectTransform.anchoredPosition;  
    }

    public void ToggleMinimap()
    {
        if (isExpanded)
        {
            CollapseMinimap();
        }
        else
        {
            ExpandMinimap();
        }

        isExpanded = !isExpanded;
    }

    private void ExpandMinimap()
    {
        minimapRectTransform.anchoredPosition = new Vector2(960, -500);
        minimapRectTransform.sizeDelta = expandedSize;
        minimapCamera.orthographicSize = 80f;
        //playerOnMap.localScale = new Vector3(200,200,200);

        int myNewLayer = LayerMask.NameToLayer("Player Minimap");
        test.layer = myNewLayer;
        
    }

    private void CollapseMinimap()
    {
        minimapRectTransform.anchoredPosition = originalPosition;
        minimapRectTransform.sizeDelta = collapsedSize;
        minimapCamera.orthographicSize = 25f;

        int myNewLayer = LayerMask.NameToLayer("Full-Screen Map");
        test.layer = myNewLayer;


    }
    
}
