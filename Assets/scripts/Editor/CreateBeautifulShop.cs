using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class CreateBeautifulShop : EditorWindow
{
    private static TMP_FontAsset koreanFont;

    [MenuItem("Tools/Fix Rhythm Game/Create Beautiful Shop UI")]
    public static void CreateShop()
    {
        // 폰트 로드
        koreanFont = LoadKoreanFont();
        if (koreanFont == null)
        {
            EditorUtility.DisplayDialog("Error", "한글 폰트(NotoSansKR-Regular SDF)를 찾을 수 없습니다.\n먼저 폰트를 생성해주세요.", "OK");
            return;
        }

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Error", "Canvas를 찾을 수 없습니다.", "OK");
            return;
        }

        // 기존 상점 패널 찾기
        Transform oldShop = canvas.transform.Find("StepUpShopPanel");
        if (oldShop != null) DestroyImmediate(oldShop.gameObject);

        oldShop = canvas.transform.Find("ShopPanel"); // 기존 것도
        if (oldShop != null)
        {
            oldShop.name = "ShopPanel_Old";
            oldShop.gameObject.SetActive(false);
        }

        // ==========================================
        // 1. 전체 오버레이 (배경 어둡게)
        // ==========================================
        GameObject shopCanvas = new GameObject("StepUpShopPanel");
        shopCanvas.transform.SetParent(canvas.transform, false);
        
        RectTransform shopRt = shopCanvas.AddComponent<RectTransform>();
        shopRt.anchorMin = Vector2.zero;
        shopRt.anchorMax = Vector2.one;
        shopRt.sizeDelta = Vector2.zero;
        shopRt.anchoredPosition = Vector2.zero;

        // 배경 블러 느낌 (반투명 블랙)
        Image bgImg = shopCanvas.AddComponent<Image>();
        bgImg.color = new Color(0, 0, 0, 0.85f);
        
        // 클릭 막기 (Raycast Target)
        bgImg.raycastTarget = true; 

        // ==========================================
        // 2. 메인 패널 (팝업창)
        // ==========================================
        GameObject panelObj = new GameObject("MainPanel");
        panelObj.transform.SetParent(shopCanvas.transform, false);

        RectTransform panelRt = panelObj.AddComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.sizeDelta = new Vector2(900, 600); // 넉넉한 크기

        // 배경 (유리 느낌의 짙은 네이비)
        Image panelImg = panelObj.AddComponent<Image>();
        panelImg.color = new Color(0.08f, 0.1f, 0.15f, 0.98f);
        
        // 외곽선 (글로우)
        Outline outline = panelObj.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0.8f, 1f, 0.5f); // 시안색 글로우
        outline.effectDistance = new Vector2(2, -2);

        // ==========================================
        // 3. 헤더 (타이틀 + 닫기)
        // ==========================================
        GameObject headerObj = new GameObject("Header");
        headerObj.transform.SetParent(panelObj.transform, false);
        
        RectTransform headerRt = headerObj.AddComponent<RectTransform>();
        headerRt.anchorMin = new Vector2(0, 1);
        headerRt.anchorMax = new Vector2(1, 1);
        headerRt.pivot = new Vector2(0.5f, 1);
        headerRt.sizeDelta = new Vector2(0, 80);
        headerRt.anchoredPosition = Vector2.zero;

        // 타이틀 텍스트
        CreateText(headerObj, "TitleText", "상점", 36, FontStyles.Bold, TextAlignmentOptions.Center, new Color(0f, 0.9f, 1f),
            new Vector2(0, 0), new Vector2(0, 1), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0, 0));

        // 하단 분리선
        GameObject lineObj = new GameObject("Line");
        lineObj.transform.SetParent(headerObj.transform, false);
        RectTransform lineRt = lineObj.AddComponent<RectTransform>();
        lineRt.anchorMin = new Vector2(0, 0);
        lineRt.anchorMax = new Vector2(1, 0);
        lineRt.sizeDelta = new Vector2(0, 2);
        lineRt.anchoredPosition = Vector2.zero;
        Image lineImg = lineObj.AddComponent<Image>();
        lineImg.color = new Color(1f, 1f, 1f, 0.1f);


        // ==========================================
        // 4. 내 정보 (코인 표시) - 우측 상단으로 이동 or 타이틀 옆
        // ==========================================
        GameObject coinInfoObj = new GameObject("CoinInfo");
        coinInfoObj.transform.SetParent(headerObj.transform, false);
        RectTransform coinInfoRt = coinInfoObj.AddComponent<RectTransform>();
        coinInfoRt.anchorMin = new Vector2(0, 0.5f); // 좌측
        coinInfoRt.anchorMax = new Vector2(0, 0.5f);
        coinInfoRt.pivot = new Vector2(0, 0.5f);
        coinInfoRt.anchoredPosition = new Vector2(40, 0);
        coinInfoRt.sizeDelta = new Vector2(200, 40);

        // 코인 아이콘
        CreateText(coinInfoObj, "Icon", "💎", 24, FontStyles.Normal, TextAlignmentOptions.MidlineLeft, Color.white,
            new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(0,0), new Vector2(30, 0));
        
        // 코인 텍스트
        CreateText(coinInfoObj, "Amount", "1,000 G", 20, FontStyles.Bold, TextAlignmentOptions.MidlineLeft, new Color(1f, 0.8f, 0f),
             new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0.5f), new Vector2(35, 0), new Vector2(0, 0));


        // ==========================================
        // 5. 상품 목록 (그리드)
        // ==========================================
        GameObject gridObj = new GameObject("ProductGrid");
        gridObj.transform.SetParent(panelObj.transform, false);
        RectTransform gridRt = gridObj.AddComponent<RectTransform>();
        gridRt.anchorMin = new Vector2(0, 0);
        gridRt.anchorMax = new Vector2(1, 1);
        gridRt.offsetMin = new Vector2(40, 100); // 하단 여백 (충전 버튼용)
        gridRt.offsetMax = new Vector2(-40, -100); // 상단 여백

        GridLayoutGroup grid = gridObj.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(250, 320);
        grid.spacing = new Vector2(30, 30);
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.childAlignment = TextAnchor.UpperCenter;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;

        // 샘플 상품 추가
        CreateProductCard(gridObj, "K-POP 패키지 Vol.1", "2,000 G", "NewJeans, IVE 등\n인기 곡 5곡 포함!", false);
        CreateProductCard(gridObj, "네온 아바타 세트", "5,000 G", "빛나는 네온 스타일\n한정판 아바타", false);
        CreateProductCard(gridObj, "스타 노트 스킨", "1,500 G", "노트가 별 모양으로\n변경됩니다.", false);
        CreateProductCard(gridObj, "경험치 2배 (1시간)", "500 G", "빠른 레벨업을 위한\n필수 아이템!", true); // 세일 중

        // ==========================================
        // 6. 하단 액션바 (충전하기)
        // ==========================================
        GameObject footerObj = new GameObject("Footer");
        footerObj.transform.SetParent(panelObj.transform, false);
        RectTransform footerRt = footerObj.AddComponent<RectTransform>();
        footerRt.anchorMin = new Vector2(0, 0);
        footerRt.anchorMax = new Vector2(1, 0);
        footerRt.pivot = new Vector2(0.5f, 0);
        footerRt.sizeDelta = new Vector2(0, 80);
        footerRt.anchoredPosition = Vector2.zero;

        // 충전 버튼
        GameObject chargeBtnObj = new GameObject("ChargeButton");
        chargeBtnObj.transform.SetParent(footerObj.transform, false);
        
        Image chargeBtnImg = chargeBtnObj.AddComponent<Image>();
        chargeBtnImg.color = new Color(0.2f, 0.25f, 0.8f); // 토스 블루 느낌

        Button chargeBtn = chargeBtnObj.AddComponent<Button>();
        ColorBlock cb = chargeBtn.colors;
        cb.normalColor = new Color(0.2f, 0.25f, 0.8f);
        cb.highlightedColor = new Color(0.3f, 0.35f, 0.9f);
        chargeBtn.colors = cb;

        RectTransform chargeBtnRt = chargeBtnObj.GetComponent<RectTransform>();
        chargeBtnRt.sizeDelta = new Vector2(200, 50);
        chargeBtnRt.anchoredPosition = new Vector2(0, 20); // 중앙 하단

        CreateText(chargeBtnObj, "Text", "코인 충전하기", 18, FontStyles.Bold, TextAlignmentOptions.Center, Color.white,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

        // ==========================================
        // 7. 닫기 버튼 (우측 상단 X)
        // ==========================================
        GameObject closeBtnObj = new GameObject("CloseButton");
        closeBtnObj.transform.SetParent(panelObj.transform, false);
        RectTransform closeBtnRt = closeBtnObj.AddComponent<RectTransform>();
        closeBtnRt.anchorMin = new Vector2(1, 1);
        closeBtnRt.anchorMax = new Vector2(1, 1);
        closeBtnRt.pivot = new Vector2(1, 1);
        closeBtnRt.anchoredPosition = new Vector2(-20, -20);
        closeBtnRt.sizeDelta = new Vector2(40, 40);

        Image closeBtnImg = closeBtnObj.AddComponent<Image>();
        closeBtnImg.color = new Color(1, 1, 1, 0.1f); // 반투명

        Button closeBtn = closeBtnObj.AddComponent<Button>();
        
        CreateText(closeBtnObj, "Text", "X", 20, FontStyles.Bold, TextAlignmentOptions.Center, Color.white,
             Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);


        // ==========================================
        // 8. 스크립트 연결은 수동으로 해야 함
        // ==========================================
        // 일단 UI만 생성하고, 기능 연결은 SchoolLobbyManager에서 찾아서 하도록 가이드 필요
        // 또는 버튼에 리스너 추가하는 컴포넌트 부착

        shopCanvas.SetActive(false); // 일단 숨김
        Selection.activeGameObject = shopCanvas;
        Debug.Log("Beautiful Shop UI Created!");
    }

    private static void CreateProductCard(GameObject parent, string title, string price, string desc, bool isSale)
    {
        GameObject card = new GameObject("ProductCard");
        card.transform.SetParent(parent.transform, false);
        
        Image cardImg = card.AddComponent<Image>();
        cardImg.color = new Color(0.15f, 0.18f, 0.25f); // 카드 배경

        Outline outline = card.AddComponent<Outline>();
        outline.effectColor = new Color(1, 1, 1, 0.05f);
        
        Button btn = card.AddComponent<Button>();

        // 상품명
        CreateText(card, "Title", title, 18, FontStyles.Bold, TextAlignmentOptions.TopLeft, new Color(0.8f, 0.9f, 1f),
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(20, -20), new Vector2(-40, 30));

        // 설명
        CreateText(card, "Desc", desc, 14, FontStyles.Normal, TextAlignmentOptions.TopLeft, new Color(0.6f, 0.7f, 0.8f),
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(20, -60), new Vector2(-40, 60));

        // 가격 태그 (하단)
        CreateText(card, "Price", price, 20, FontStyles.Bold, TextAlignmentOptions.BottomRight, new Color(1f, 0.8f, 0.2f),
             new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(-20, 20), new Vector2(-40, 30));

        // 아이콘 자리 (중앙)
        GameObject iconPlaceholder = new GameObject("IconPlaceholder");
        iconPlaceholder.transform.SetParent(card.transform, false);
        RectTransform iconRt = iconPlaceholder.AddComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0.5f, 0.5f);
        iconRt.anchorMax = new Vector2(0.5f, 0.5f);
        iconRt.sizeDelta = new Vector2(80, 80);
        Image iconImg = iconPlaceholder.AddComponent<Image>();
        iconImg.color = new Color(1, 1, 1, 0.1f); // 임시 박스
        
        // 구매 버튼 (오버레이 스타일) - 클릭 시 동작하도록
        // 여기서는 생략, 전체 카드 클릭이 구매로 이어지게 하거나.
    }

    private static TextMeshProUGUI CreateText(GameObject parent, string name, string content, float fontSize, FontStyles style, TextAlignmentOptions align, Color color,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        GameObject txtObj = new GameObject(name);
        txtObj.transform.SetParent(parent.transform, false);
        
        TextMeshProUGUI txt = txtObj.AddComponent<TextMeshProUGUI>();
        txt.text = content;
        txt.font = koreanFont; // 한글 폰트 적용
        txt.fontSize = fontSize;
        txt.fontStyle = style;
        txt.alignment = align;
        txt.color = color;
        
        RectTransform rt = txtObj.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        if (size != Vector2.zero) rt.sizeDelta = size;

        return txt;
    }

    private static TMP_FontAsset LoadKoreanFont()
    {
        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts/NotoSansKR-Regular SDF");
        if (font != null) return font;
        font = Resources.Load<TMP_FontAsset>("NotoSansKR-Regular SDF");
        if (font != null) return font;
        string[] guids = AssetDatabase.FindAssets("NotoSansKR-Regular SDF t:TMP_FontAsset");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        }
        return font;
    }
}
