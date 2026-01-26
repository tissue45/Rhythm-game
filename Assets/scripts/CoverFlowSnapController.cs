using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Smooth scroll snap controller for cover flow song selection
/// </summary>
public class CoverFlowSnapController : MonoBehaviour, IEndDragHandler, IBeginDragHandler, IScrollHandler
{
    public ScrollRect scrollRect;
    public float cardWidth = 450f;
    public int cardCount = 3;
    public float snapSpeed = 10f;
    public float scrollSensitivity = 0.5f;
    public float viewportCenterX = 640f; // Center of 1280 viewport
    public System.Action<int> onSnapChanged;

    private bool _isDragging = false;
    private int _currentIndex = 0;
    private float _targetPosition = 0f;
    private RectTransform _content;
    private float _scrollAccumulator = 0f;

    void Start()
    {
        if (scrollRect != null)
        {
            _content = scrollRect.content;

            // Calculate viewport center if not set
            if (viewportCenterX <= 0)
            {
                var viewportRt = scrollRect.viewport != null ? scrollRect.viewport : scrollRect.GetComponent<RectTransform>();
                if (viewportRt != null)
                {
                    viewportCenterX = viewportRt.rect.width / 2;
                }
                if (viewportCenterX <= 0) viewportCenterX = 640f; // Fallback to 1280/2
            }

            // Initialize to first card
            SnapToIndex(0, true);
        }
    }

    void Update()
    {
        if (scrollRect == null || _content == null) return;

        // Smooth snap when not dragging
        if (!_isDragging)
        {
            float currentX = _content.anchoredPosition.x;
            float newX = Mathf.Lerp(currentX, _targetPosition, Time.deltaTime * snapSpeed);
            _content.anchoredPosition = new Vector2(newX, _content.anchoredPosition.y);
        }

        // Always update scales for visual feedback
        UpdateCardScales();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _isDragging = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _isDragging = false;

        // Calculate nearest card based on center position (Pivot 0.5)
        // ContentX moves left (negative) as we scroll right
        // Card X = index * Width
        // At center: ContentX + CardX = 0  =>  ContentX = -CardX
        // Index = -ContentX / Width
        float currentX = _content.anchoredPosition.x;
        int nearestIndex = Mathf.RoundToInt(-currentX / cardWidth);
        nearestIndex = Mathf.Clamp(nearestIndex, 0, cardCount - 1);

        SnapToIndex(nearestIndex, false);
    }

    public void OnScroll(PointerEventData eventData)
    {
        // Accumulate scroll input
        _scrollAccumulator += eventData.scrollDelta.y * scrollSensitivity;

        // When accumulated enough, change index
        if (_scrollAccumulator >= 1f)
        {
            _scrollAccumulator = 0f;
            SnapToIndex(_currentIndex - 1, false);
        }
        else if (_scrollAccumulator <= -1f)
        {
            _scrollAccumulator = 0f;
            SnapToIndex(_currentIndex + 1, false);
        }
    }

    public void SnapToIndex(int index, bool instant = false)
    {
        index = Mathf.Clamp(index, 0, cardCount - 1);
        _currentIndex = index;

        // Calculate target position to center the card
        // Content Pivot is Center(0.5), so local (0,0) is screen center
        // Card is at Local X = index * cardWidth
        // To Move Card to Center (0): Content X needs to be -CardX
        _targetPosition = -(index * cardWidth);

        if (instant && _content != null)
        {
            _content.anchoredPosition = new Vector2(_targetPosition, _content.anchoredPosition.y);
        }

        onSnapChanged?.Invoke(_currentIndex);
    }

    private void UpdateCardScales()
    {
        if (_content == null) return;

        float currentX = _content.anchoredPosition.x;

        for (int i = 0; i < _content.childCount; i++)
        {
            var child = _content.GetChild(i);
            var childRt = child as RectTransform;
            if (childRt == null) continue;

            // Calculate distance from viewport center (0)
            // Screen position = card's local X + content's X
            // Since pivot is center, 0 IS the center
            float cardScreenCenterX = childRt.anchoredPosition.x + currentX;
            float distanceFromCenter = Mathf.Abs(cardScreenCenterX);
            float normalizedDistance = distanceFromCenter / cardWidth;

            // Scale: 1.0 at center, 0.8 at sides
            float scale = Mathf.Lerp(1.0f, 0.8f, Mathf.Clamp01(normalizedDistance));
            childRt.localScale = Vector3.one * scale;

            // Alpha/brightness - centered card is fully visible
            float alpha = Mathf.Lerp(1f, 0.65f, Mathf.Clamp01(normalizedDistance));
            var img = child.GetComponent<Image>();
            if (img != null)
            {
                Color c = img.color;
                c.a = alpha;
                img.color = c;
            }
        }
    }

    public int GetCurrentIndex() => _currentIndex;
}
