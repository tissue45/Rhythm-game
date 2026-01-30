using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

/// <summary>
/// 나노바나나 스타일 리듬게임 버튼 - 네온 글로우 + 그라데이션 + 애니메이션
/// </summary>
public class RhythmMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Button Style")]
    public ButtonType buttonType = ButtonType.Normal;
    
    [Header("Colors")]
    public Color primaryColor = new Color(0f, 1f, 1f); // 시안
    public Color secondaryColor = new Color(1f, 0f, 1f); // 마젠타
    
    private RectTransform rectTransform;
    private Image backgroundImage;
    private Image glowImage;
    private Image borderImage;
    private TextMeshProUGUI buttonText;
    private GameObject particleContainer;
    
    private Vector3 originalScale;
    private bool isHovered = false;

    public enum ButtonType
    {
        Normal,     // RANKING, SHOP, OPTION
        Primary,    // GAME START
        Exit        // EXIT
    }

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
        
        SetupButton();
    }

    void Start()
    {
        // 시작 애니메이션
        rectTransform.localScale = Vector3.zero;
        rectTransform.DOScale(originalScale, 0.5f).SetEase(Ease.OutBack).SetDelay(Random.Range(0f, 0.3f));
    }

    private void SetupButton()
    {
        // 기존 컴포넌트 정리
        Image[] existingImages = GetComponentsInChildren<Image>();
        foreach (Image img in existingImages)
        {
            if (img.gameObject != gameObject)
            {
                DestroyImmediate(img.gameObject);
            }
        }

        // 1. 배경 레이어
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(transform, false);
        backgroundImage = bgObj.AddComponent<Image>();
        RectTransform bgRT = bgObj.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;

        // 2. 글로우 레이어 (뒤에서 빛나는 효과)
        GameObject glowObj = new GameObject("Glow");
        glowObj.transform.SetParent(transform, false);
        glowObj.transform.SetAsFirstSibling(); // 맨 뒤로
        glowImage = glowObj.AddComponent<Image>();
        RectTransform glowRT = glowObj.GetComponent<RectTransform>();
        glowRT.anchorMin = Vector2.zero;
        glowRT.anchorMax = Vector2.one;
        glowRT.offsetMin = new Vector2(-10, -10);
        glowRT.offsetMax = new Vector2(10, 10);

        // 3. 테두리 레이어
        GameObject borderObj = new GameObject("Border");
        borderObj.transform.SetParent(transform, false);
        borderImage = borderObj.AddComponent<Image>();
        RectTransform borderRT = borderObj.GetComponent<RectTransform>();
        borderRT.anchorMin = Vector2.zero;
        borderRT.anchorMax = Vector2.one;
        borderRT.offsetMin = Vector2.zero;
        borderRT.offsetMax = Vector2.zero;

        // 4. 텍스트 레이어
        GameObject textObj = transform.Find("Text (TMP)")?.gameObject;
        if (textObj == null)
        {
            textObj = new GameObject("Text (TMP)");
            textObj.transform.SetParent(transform, false);
            buttonText = textObj.AddComponent<TextMeshProUGUI>();
        }
        else
        {
            buttonText = textObj.GetComponent<TextMeshProUGUI>();
        }
        
        RectTransform textRT = textObj.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        // 5. 파티클 컨테이너
        particleContainer = new GameObject("Particles");
        particleContainer.transform.SetParent(transform, false);
        CreateIdleParticles();

        // 스타일 적용
        ApplyStyle();
    }

    private void ApplyStyle()
    {
        if (buttonType == ButtonType.Primary)
        {
            // GAME START - 가장 화려하게
            primaryColor = new Color(1f, 0.3f, 0.8f); // 핑크
            secondaryColor = new Color(0.3f, 0.8f, 1f); // 시안
            
            // 그라데이션 배경
            backgroundImage.color = primaryColor;
            CreateGradientTexture(backgroundImage, primaryColor, secondaryColor);
            
            // 강렬한 글로우
            glowImage.color = new Color(primaryColor.r, primaryColor.g, primaryColor.b, 0.6f);
            
            // 두꺼운 테두리
            borderImage.color = Color.white;
            CreateBorderTexture(borderImage, 6f);
            
            // 텍스트
            if (buttonText != null)
            {
                buttonText.fontSize = 48;
                buttonText.fontStyle = FontStyles.Bold | FontStyles.Italic;
                buttonText.color = Color.white;
                
                // 외곽선
                Outline outline = buttonText.gameObject.GetComponent<Outline>();
                if (outline == null) outline = buttonText.gameObject.AddComponent<Outline>();
                outline.effectColor = new Color(0, 0, 0, 0.8f);
                outline.effectDistance = new Vector2(3, -3);
            }
        }
        else if (buttonType == ButtonType.Exit)
        {
            // EXIT - 레드 테마
            primaryColor = new Color(1f, 0.2f, 0.3f); // 레드
            secondaryColor = new Color(1f, 0.5f, 0f); // 오렌지
            
            backgroundImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            glowImage.color = new Color(primaryColor.r, primaryColor.g, primaryColor.b, 0.4f);
            borderImage.color = primaryColor;
            CreateBorderTexture(borderImage, 4f);
            
            if (buttonText != null)
            {
                buttonText.fontSize = 36;
                buttonText.fontStyle = FontStyles.Bold;
                buttonText.color = Color.white;
            }
        }
        else
        {
            // NORMAL - 시안/마젠타 테마
            backgroundImage.color = new Color(0.05f, 0.15f, 0.2f, 0.9f);
            glowImage.color = new Color(primaryColor.r, primaryColor.g, primaryColor.b, 0.3f);
            borderImage.color = primaryColor;
            CreateBorderTexture(borderImage, 3f);
            
            if (buttonText != null)
            {
                buttonText.fontSize = 40;
                buttonText.fontStyle = FontStyles.Bold | FontStyles.Italic;
                buttonText.color = Color.white;
                buttonText.alignment = TextAlignmentOptions.Center;
            }
        }
    }

    private void CreateGradientTexture(Image img, Color color1, Color color2)
    {
        Texture2D tex = new Texture2D(256, 1);
        for (int x = 0; x < 256; x++)
        {
            float t = x / 255f;
            tex.SetPixel(x, 0, Color.Lerp(color1, color2, t));
        }
        tex.Apply();
        
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, 256, 1), new Vector2(0.5f, 0.5f));
        img.sprite = sprite;
        img.type = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Horizontal;
    }

    private void CreateBorderTexture(Image img, float thickness)
    {
        int size = 128;
        Texture2D tex = new Texture2D(size, size);
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distX = Mathf.Min(x, size - 1 - x);
                float distY = Mathf.Min(y, size - 1 - y);
                float dist = Mathf.Min(distX, distY);
                
                float alpha = dist < thickness ? 1f : 0f;
                tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
        }
        tex.Apply();
        
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        img.sprite = sprite;
    }

    private void CreateIdleParticles()
    {
        GameObject particleObj = new GameObject("IdleParticles");
        particleObj.transform.SetParent(particleContainer.transform, false);
        ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.startColor = new ParticleSystem.MinMaxGradient(primaryColor, secondaryColor);
        main.startSize = new ParticleSystem.MinMaxCurve(2f, 5f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(10f, 30f);
        main.startLifetime = 1.5f;
        main.maxParticles = 20;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        var emission = ps.emission;
        emission.rateOverTime = 5;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Rectangle;
        RectTransform rt = GetComponent<RectTransform>();
        shape.scale = new Vector3(rt.sizeDelta.x, rt.sizeDelta.y, 1);

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = new Material(Shader.Find("Sprites/Default"));
    }

    void Update()
    {
        // 글로우 펄스 애니메이션
        if (glowImage != null)
        {
            float pulse = Mathf.Sin(Time.time * 2f) * 0.1f + 0.5f;
            Color glowColor = glowImage.color;
            glowColor.a = pulse * (isHovered ? 0.8f : 0.3f);
            glowImage.color = glowColor;
        }

        // 테두리 펄스
        if (borderImage != null && isHovered)
        {
            float pulse = Mathf.Sin(Time.time * 8f) * 0.2f + 0.8f;
            borderImage.color = primaryColor * pulse;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        
        // 확대 애니메이션
        rectTransform.DOKill();
        rectTransform.DOScale(originalScale * 1.1f, 0.2f).SetEase(Ease.OutBack);
        
        // 글로우 강화
        if (glowImage != null)
        {
            glowImage.DOKill();
            Color targetColor = glowImage.color;
            targetColor.a = 0.8f;
            glowImage.DOColor(targetColor, 0.2f);
        }
        
        // 텍스트 색상 변화
        if (buttonText != null)
        {
            buttonText.DOKill();
            buttonText.DOColor(primaryColor, 0.2f);
        }
        
        // 파티클 증가
        ParticleSystem ps = particleContainer.GetComponentInChildren<ParticleSystem>();
        if (ps != null)
        {
            var emission = ps.emission;
            emission.rateOverTime = 15;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        
        // 원래 크기로
        rectTransform.DOKill();
        rectTransform.DOScale(originalScale, 0.2f).SetEase(Ease.OutQuad);
        
        // 글로우 약화
        if (glowImage != null)
        {
            glowImage.DOKill();
            Color targetColor = glowImage.color;
            targetColor.a = 0.3f;
            glowImage.DOColor(targetColor, 0.2f);
        }
        
        // 텍스트 원래 색상
        if (buttonText != null)
        {
            buttonText.DOKill();
            buttonText.DOColor(Color.white, 0.2f);
        }
        
        // 파티클 감소
        ParticleSystem ps = particleContainer.GetComponentInChildren<ParticleSystem>();
        if (ps != null)
        {
            var emission = ps.emission;
            emission.rateOverTime = 5;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 클릭 애니메이션
        rectTransform.DOKill();
        rectTransform.DOScale(originalScale * 0.95f, 0.1f);
        
        // 클릭 파티클 폭발
        CreateClickExplosion();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // 원래 크기로 (호버 상태면 1.1배)
        rectTransform.DOKill();
        rectTransform.DOScale(originalScale * (isHovered ? 1.1f : 1f), 0.1f).SetEase(Ease.OutBack);
    }

    private void CreateClickExplosion()
    {
        GameObject explosionObj = new GameObject("ClickExplosion");
        explosionObj.transform.SetParent(transform, false);
        explosionObj.transform.localPosition = Vector3.zero;
        
        ParticleSystem ps = explosionObj.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.startColor = new ParticleSystem.MinMaxGradient(primaryColor, Color.white);
        main.startSpeed = new ParticleSystem.MinMaxCurve(50f, 150f);
        main.startSize = new ParticleSystem.MinMaxCurve(3f, 8f);
        main.startLifetime = 0.5f;
        main.loop = false;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[]{ new ParticleSystem.Burst(0f, 30) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 50f;

        Destroy(explosionObj, 1f);
    }
}
