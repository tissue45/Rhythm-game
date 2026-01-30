using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

/// <summary>
/// 간단한 버튼 호버 효과 - 마우스 올리면 살짝 커짐
/// </summary>
public class SimpleButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("호버 설정")]
    public float hoverScale = 1.1f;
    public float animationDuration = 0.2f;

    private Vector3 originalScale;
    private Tween scaleTween;

    void Start()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        scaleTween?.Kill();
        scaleTween = transform.DOScale(originalScale * hoverScale, animationDuration)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        scaleTween?.Kill();
        scaleTween = transform.DOScale(originalScale, animationDuration)
            .SetEase(Ease.OutSine)
            .SetUpdate(true);
    }

    void OnDestroy()
    {
        scaleTween?.Kill();
    }
}
