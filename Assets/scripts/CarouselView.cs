using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CarouselView : MonoBehaviour, IEndDragHandler, IScrollHandler
{
    public RectTransform container; // The content panel of the ScrollRect
    public float spacing = 150f;    // Distance between items
    public float scaleFactor = 1.2f; // Scale of the center item
    public float scaleSpeed = 5f;   // Speed of scaling animation
    public bool isVertical = false; // [NEW] Direction

    private ScrollRect scrollRect;
    private RectTransform[] items;
    private int selectedIndex = 0;
    public System.Action<int> OnItemSelected; // [NEW] Callback
    private float targetPos;
    private float lastScrollTime = 0f;
    private float scrollCooldown = 0.15f; // Prevent too fast scrolling
    
    // LoL Style specific
    public Color normalColor = new Color(0.3f, 0.3f, 0.4f, 0.8f);
    public Color selectedColor = new Color(0.6f, 0.9f, 1f, 1f); // Bright Ice Blue / Cyan

    void Start()
    {
        scrollRect = GetComponent<ScrollRect>();
        if(scrollRect) isVertical = scrollRect.vertical;
        
        // Initialize logic once items are added
        Invoke("InitializeItems", 0.1f);
    }

    public void InitializeItems()
    {
        if (container == null) return;
        
        int childCount = container.childCount;
        Debug.Log($"[CarouselView] Initializing... Found {childCount} items.");

        items = new RectTransform[childCount];
        for (int i = 0; i < childCount; i++)
        {
            items[i] = container.GetChild(i).GetComponent<RectTransform>();
            
            // [FIX] 강제 위치 정렬 (겹침 방지)
            if (isVertical)
            {
                items[i].anchoredPosition = new Vector2(0, -i * spacing);
            }
            else
            {
                items[i].anchoredPosition = new Vector2(i * spacing, 0); 
            }
            Debug.Log($"[CarouselView] Aligned Item {i} at {items[i].anchoredPosition}");
        }
        
        // Initial Snap
        if (childCount > 0)
        {
            SnapTo(0);
        }
    }

    void Update()
    {
        if (items == null || items.Length == 0) return;

        // 1. Snapping Logic (Smooth Move)
        if (isVertical)
        {
            float currentPos = container.anchoredPosition.y;
            float newPos = Mathf.Lerp(currentPos, targetPos, Time.deltaTime * 10f);
            container.anchoredPosition = new Vector2(container.anchoredPosition.x, newPos);
        }
        else
        {
            float currentPos = container.anchoredPosition.x;
            float newPos = Mathf.Lerp(currentPos, targetPos, Time.deltaTime * 10f);
            container.anchoredPosition = new Vector2(newPos, container.anchoredPosition.y);
        }

        // 2. 3D Scaling Effect
        if (isVertical)
        {
            float centerY = -container.anchoredPosition.y;
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == null) { InitializeItems(); return; } // [FIX] Safety Check

                RectTransform item = items[i];
                float dist = Mathf.Abs(item.anchoredPosition.y - centerY);
                float t = Mathf.Clamp01(dist / (spacing * 2.0f)); 
                float scale = Mathf.Lerp(scaleFactor, 0.8f, t);
                item.localScale = Vector3.Lerp(item.localScale, new Vector3(scale, scale, 1f), Time.deltaTime * scaleSpeed);
                
                Image bg = item.GetComponent<Image>();
                if (bg) bg.color = Color.Lerp(selectedColor, normalColor, t);
            }
        }
        else
        {
            float centerX = -container.anchoredPosition.x;
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == null) { InitializeItems(); return; } // [FIX] Safety Check

                RectTransform item = items[i];
                float dist = Mathf.Abs(item.anchoredPosition.x - centerX);
                float t = Mathf.Clamp01(dist / (spacing * 1.5f)); 
                float scale = Mathf.Lerp(scaleFactor, 0.8f, t);
                item.localScale = Vector3.Lerp(item.localScale, new Vector3(scale, scale, 1f), Time.deltaTime * scaleSpeed);
                
                Image bg = item.GetComponent<Image>();
                if (bg) bg.color = Color.Lerp(selectedColor, normalColor, t);
            }
        }
    }
    
    // [NEW] Mouse Wheel Support
    public void OnScroll(PointerEventData eventData)
    {
        if (Time.time - lastScrollTime < scrollCooldown) return;

        if (eventData.scrollDelta.y > 0) // Scroll Up
        {
            SelectPrev();
            lastScrollTime = Time.time;
        }
        else if (eventData.scrollDelta.y < 0) // Scroll Down
        {
            SelectNext();
            lastScrollTime = Time.time;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        float center = isVertical ? -container.anchoredPosition.y : -container.anchoredPosition.x;
        float minDist = float.MaxValue;
        int closestIndex = 0;

        for (int i = 0; i < items.Length; i++)
        {
            float pos = isVertical ? items[i].anchoredPosition.y : items[i].anchoredPosition.x;
            float dist = Mathf.Abs(pos - center);
            if (dist < minDist)
            {
                minDist = dist;
                closestIndex = i;
            }
        }

        SnapTo(closestIndex);
    }

    public void SnapTo(int index)
    {
        if (index < 0 || index >= items.Length) return;
        selectedIndex = index;
        
        if (isVertical)
            targetPos = -items[index].anchoredPosition.y;
        else
            targetPos = -items[index].anchoredPosition.x;
            
        OnItemSelected?.Invoke(selectedIndex); // [NEW] Notify selection
    }
    
    public void SelectNext() { SnapTo(selectedIndex + 1); }
    public void SelectPrev() { SnapTo(selectedIndex - 1); }
}
