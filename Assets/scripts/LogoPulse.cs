using UnityEngine;
using UnityEngine.UI;

public class LogoPulse : MonoBehaviour
{
    [Header("Hover Settings")]
    public float hoverRange = 10f; // Pixel distance to float up/down
    public float hoverSpeed = 2.0f; // Speed of the float

    private Vector2 _startPos;
    private RectTransform _rectTransform;

    private void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        _startPos = _rectTransform.anchoredPosition;
    }

    private void Update()
    {
        if (_rectTransform == null) return;

        // Gentle Hover (Sine Wave on Y axis)
        float newY = _startPos.y + Mathf.Sin(Time.time * hoverSpeed) * hoverRange;
        _rectTransform.anchoredPosition = new Vector2(_startPos.x, newY);
    }
}
