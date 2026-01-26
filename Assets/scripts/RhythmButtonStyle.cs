using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// 종이 같은 심플한 버튼 스타일 - 부드러운 그림자와 깔끔한 디자인
/// </summary>
[ExecuteInEditMode]
public class RhythmButtonStyle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    // ... (기존 변수들)

    // [FIX] 클릭 이벤트 강제 전달
    public void OnPointerClick(PointerEventData eventData)
    {
        if (Application.isPlaying)
        {
            Button btn = GetComponent<Button>();
            if (btn != null && btn.interactable)
            {
                btn.onClick.Invoke();
            }
        }
    }
    
    // ... (기존 메서드들)
    [Header("Button Type")]
    public bool isPrimaryButton = false; // GAME START
    public bool isExitButton = false;    // EXIT
    
    [Header("Backward Compatibility")]
    public bool isMainButton = false;
    public bool isTextOnly = false;
    public float skewAngle = 0f;
    public float paddingLeft = 60f;
    public float paddingRight = 20f;
    public Vector2 textOffset = Vector2.zero;
    public TMPro.TextAlignmentOptions textAlignment = TMPro.TextAlignmentOptions.Center;
    public Color normalBgColor = new Color(1f, 1f, 1f, 1f); // 흰색 종이
    public Color hoverBgColor = new Color(0.95f, 0.95f, 0.95f, 1f);
    public Color accentColor = new Color(0f, 0f, 0f, 1f); // 검은색 텍스트
    
    // [NEW] 아이콘 스프라이트
    public Sprite iconSprite;

    public TextMeshProUGUI btnText { get { return buttonText; } }
    
    private Image backgroundImage;
    private Shadow paperShadow;
    private TextMeshProUGUI buttonText;
    private RectTransform rectTransform;
    
    private Vector3 originalScale;
    private Vector2 originalPosition; // [FIX] Vector2로 변경 (anchoredPosition은 Vector2)
    private bool isHovered = false;
    private bool isInitialized = false;

    void OnEnable()
    {
        if (!isInitialized)
        {
            Initialize();
        }
    }

    void Start()
    {
        if (!isInitialized)
        {
            Initialize();
        }
    }

    void OnValidate()
    {
        if (Application.isPlaying) return;
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this != null) Initialize();
        };
        #endif
    }

    private void Initialize()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            originalScale = rectTransform.localScale;
            // [FIX] anchoredPosition을 사용해야 함 (localPosition은 Canvas에서 문제 발생)
            originalPosition = rectTransform.anchoredPosition;
        }

        // 텍스트 찾기
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText == null)
        {
            Transform textTransform = transform.Find("Text (TMP)");
            if (textTransform != null)
            {
                buttonText = textTransform.GetComponent<TextMeshProUGUI>();
            }
        }

        // [FIX] 배경 이미지 설정 (충돌 방지)
        // 이미 TextMeshProUGUI 등 다른 Graphic이 붙어있으면 Image를 추가할 수 없음
        Graphic existingGraphic = GetComponent<Graphic>();
        if (existingGraphic != null && !(existingGraphic is Image))
        {
            backgroundImage = null; 
        }
        else
        {
            backgroundImage = GetComponent<Image>();
            if (backgroundImage == null)
            {
                backgroundImage = gameObject.AddComponent<Image>();
            }
        }

        ApplyPaperStyle();
        isInitialized = true;
    }

    public void InitializeStyle()
    {
        Initialize();
    }

    public void UpdateVisuals()
    {
        ApplyPaperStyle();
    }

    // [FIX] 안전한 삭제 메서드 추가
    private void SafeDestroy(Object obj)
    {
        if (obj == null) return;
        
        if (Application.isPlaying)
        {
            Destroy(obj);
        }
        else
        {
            #if UNITY_EDITOR
            // OnValidate 등에서 안전하게 삭제하기 위해 delayCall 사용
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (obj != null) DestroyImmediate(obj);
            };
            #else
            DestroyImmediate(obj);
            #endif
        }
    }

    // [NEW] 런타임 캡슐 스프라이트 캐싱
    private static Sprite _capsuleSprite;

    private Sprite GetCapsuleSprite()
    {
        if (_capsuleSprite == null)
        {
            // 64x64 하얀색 원형 텍스처 생성
            int size = 64;
            Texture2D tex = new Texture2D(size, size);
            Color[] colors = new Color[size * size];
            float center = size / 2f;
            float radius = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    float alpha = 1f - Mathf.Clamp01(dist - radius + 1f); // 안티에일리어싱
                    colors[y * size + x] = new Color(1, 1, 1, alpha);
                }
            }
            tex.SetPixels(colors);
            tex.Apply();
            
            // 9슬라이싱이 가능한 스프라이트 생성 (가운데영역 보호)
            _capsuleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, new Vector4(size/2, size/2, size/2, size/2));
        }
        return _capsuleSprite;
    }

    private void ApplyPaperStyle() // Ultra Neon Style
    {
        // 1. 색상 팔레트 (더 밝고 강렬하게)
        Color neonCyan = new Color(0.2f, 0.9f, 1f, 1f); // 더 밝은 시안
        Color neonPink = new Color(1f, 0.3f, 0.8f, 1f); // 더 밝은 핑크
        Color deepDarkBg = new Color(0.02f, 0.02f, 0.05f, 0.9f); // 거의 검은색+투명도
        
        Color mainGlowColor = isExitButton ? neonPink : neonCyan;

        // 2. 배경 설정 (가능할 때만)
        if (backgroundImage != null)
        {
            backgroundImage.sprite = GetCapsuleSprite();
            backgroundImage.type = Image.Type.Sliced;
            backgroundImage.color = deepDarkBg;
        }

        // 3. 상태별 색상
        normalBgColor = deepDarkBg;

        hoverBgColor = mainGlowColor * 0.2f; // 호버 시 배경도 살짝 빛남
        hoverBgColor.a = 0.9f;
        accentColor = Color.white; // 텍스트는 흰색으로 (가독성+빛남)

        // 4. 강력한 네온 글로우 효과
        
        // 기존 Shadow/Outline 제거
        foreach(var s in GetComponents<Shadow>()) SafeDestroy(s);
        foreach(var o in GetComponents<Outline>()) SafeDestroy(o);

        // Layer 1: 테두리 (Outline) - 선명한 빛
        Outline outline = gameObject.AddComponent<Outline>();
        outline.effectColor = mainGlowColor;
        outline.effectDistance = new Vector2(2, -2);
        
        // Layer 2: 코어 글로우 (Shadow)
        Shadow glow1 = gameObject.AddComponent<Shadow>();
        glow1.effectColor = new Color(mainGlowColor.r, mainGlowColor.g, mainGlowColor.b, 0.6f);
        glow1.effectDistance = new Vector2(0, 0);

        // 5. 텍스트 스타일 (특수문자 제거 및 글로우)
        if (buttonText != null)
        {
            // [FIX] 특수문자 제거
            string cleanText = buttonText.text.Replace("□", "").Replace("▶", "").Replace("★", "").Trim();
            buttonText.text = cleanText;

            buttonText.fontSize = isMainButton ? 40 : 32;
            buttonText.fontStyle = FontStyles.Bold;
            buttonText.color = Color.white; 
            buttonText.alignment = textAlignment;

            // 텍스트 글로우
            Outline[] textOutlines = buttonText.GetComponents<Outline>();
            foreach(var o in textOutlines) SafeDestroy(o);
            
            Outline tOutline = buttonText.gameObject.AddComponent<Outline>();
            tOutline.effectColor = mainGlowColor;
            tOutline.effectDistance = new Vector2(1, -1);
            
            // [NEW] 아이콘 추가 로직
            if (iconSprite != null)
            {
                // 아이콘 오브젝트 찾거나 생성
                Transform iconTrans = transform.Find("Icon");
                GameObject iconObj;
                if (iconTrans == null)
                {
                    iconObj = new GameObject("Icon");
                    iconObj.transform.SetParent(transform, false);
                }
                else
                {
                    iconObj = iconTrans.gameObject;
                }
                
                Image iconImg = iconObj.GetComponent<Image>();
                if (iconImg == null) iconImg = iconObj.AddComponent<Image>();
                
                iconImg.sprite = iconSprite;
                iconImg.color = Color.white; // 아이콘 색상
                
                // 아이콘 위치 및 크기 (왼쪽에 배치)
                RectTransform iconRt = iconObj.GetComponent<RectTransform>();
                iconRt.anchorMin = new Vector2(0, 0.5f); 
                iconRt.anchorMax = new Vector2(0, 0.5f);
                iconRt.pivot = new Vector2(0.5f, 0.5f);
                iconRt.sizeDelta = new Vector2(40, 40); // 적절한 크기
                
                // 텍스트 길이에 따라 위치 조정 (단순하게 왼쪽 고정)
                // 텍스트가 중앙 정렬이 많으므로, 아이콘을 텍스트 왼쪽으로 밀어넣기는 복잡함
                // 그래서 버튼의 왼쪽에 배치하고 텍스트를 약간 오른쪽으로 밀거나, 
                // 아이콘을 텍스트 앞부분에 배치하는 것처럼 보이게 함
                
                float textWidth = buttonText.preferredWidth;
                float totalWidth = textWidth + 50; // 아이콘 + 간격
                float startX = -totalWidth / 2f;
                
                // 텍스트 위치 조정 (아이콘 공간 확보)
                // 하지만 TMP 정렬때문에 복잡하니, 아이콘을 절대 위치로 잡음 (약간 왼쪽)
                iconRt.anchoredPosition = new Vector2(50, 0); // 왼쪽 마진
                
                // 만약 텍스트랑 겹치면?
                // 텍스트 정렬을 바꿀 순 없으니... 
                // 그냥 아이콘을 버튼 왼쪽에 장식처럼 배치
                
                // 아이콘 글로우
                Outline iOutline = iconObj.GetComponent<Outline>();
                if (iOutline == null) iOutline = iconObj.AddComponent<Outline>();
                iOutline.effectColor = mainGlowColor;
                iOutline.effectDistance = new Vector2(1, -1);
            }
        }


        // 6. 버튼 크기 - [DISABLED] FinalFix.cs에서 설정한 크기 유지
        // if (rectTransform != null && !isTextOnly)
        // {
        //     float width = isMainButton ? 360 : 280;
        //     float height = 70;
        //     rectTransform.sizeDelta = new Vector2(width, height);
        // }
        
        // 변수 연결
        paperShadow = glow1; // 호버 시 이펙트 제어용
    }

    void Update()
    {
        if (!Application.isPlaying) return;
        if (backgroundImage == null) return;

        // 부드러운 색상 전환
        Color targetColor = isHovered ? hoverBgColor : normalBgColor;
        backgroundImage.color = Color.Lerp(backgroundImage.color, targetColor, Time.deltaTime * 8f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        
        if (rectTransform != null && Application.isPlaying)
        {
            // 살짝 커짐
            rectTransform.localScale = originalScale * 1.05f;
            
            // 글로우 강화 (더 밝고 멀리 퍼지게)
            if (paperShadow != null)
            {
                Color c = paperShadow.effectColor;
                c.a = 1f; // 완전 불투명 (진한 네온)
                paperShadow.effectColor = c;
            }
            
            Outline outline = GetComponent<Outline>();
            if (outline != null)
            {
                outline.effectDistance = new Vector2(3, -3); // 입체감 증가
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        
        if (rectTransform != null && Application.isPlaying)
        {
            // 원래 크기
            rectTransform.localScale = originalScale;
            
            // 글로우 복구
            if (paperShadow != null)
            {
                Color c = paperShadow.effectColor;
                c.a = 0.8f; // 약간 투명
                paperShadow.effectColor = c;
            }
            
            Outline outline = GetComponent<Outline>();
            if (outline != null)
            {
                outline.effectDistance = new Vector2(2, -2); // 원래 입체감
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (rectTransform != null && Application.isPlaying)
        {
            // 눌림 효과
            rectTransform.localScale = originalScale * 0.95f;
            
            Outline outline = GetComponent<Outline>();
            if (outline != null)
            {
                outline.effectDistance = new Vector2(1, -1); // 납작하게
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (rectTransform != null && Application.isPlaying)
        {
            // 호버 상태 복귀
            if (isHovered)
            {
                rectTransform.localScale = originalScale * 1.05f;
                
                Outline outline = GetComponent<Outline>();
                if (outline != null)
                {
                    outline.effectDistance = new Vector2(3, -3);
                }
            }
            else
            {
                rectTransform.localScale = originalScale;
                
                Outline outline = GetComponent<Outline>();
                if (outline != null)
                {
                    outline.effectDistance = new Vector2(2, -2);
                }
            }
        }
    }
}
