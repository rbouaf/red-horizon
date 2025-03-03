using UnityEngine;


public class MinimapExpand : MonoBehaviour
{
    /*
    public RectTransform minimapRectTransform; 
    public Camera minimapCamera;         
    public Vector2 collapsedSize = new Vector2(334, 270);  
    public Vector2 expandedSize = new Vector2(1920, 1080); 
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
        minimapRectTransform.anchoredPosition = new Vector2(0, 0);
        minimapRectTransform.sizeDelta = expandedSize;
        minimapCamera.orthographicSize = 10f;
    }

    private void CollapseMinimap()
    {
        minimapRectTransform.anchoredPosition = originalPosition;
        minimapRectTransform.sizeDelta = collapsedSize;
        minimapCamera.orthographicSize = 5f;
    }
    */
}
