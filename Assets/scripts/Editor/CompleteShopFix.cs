using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class CompleteShopFix : EditorWindow
{
    private static TMP_FontAsset koreanFont;

    [MenuItem("Tools/Fix Rhythm Game/🔥 COMPLETE SHOP FIX (Run This!)")]
    public static void FixShopAndUI()
    {
        koreanFont = LoadKoreanFont();
        if (koreanFont == null)
        {
            Debug.LogError("한글 폰트(NotoSansKR-Regular SDF)가 없습니다!");
            return;
        }

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        // 1. 기존 상점 삭제
        string[] oldNames = { "ShopPanel", "StepUpShopPanel", "PremiumShop", "PremiumShopPanel", "ShopCanvas" };
        foreach (string name in oldNames)
        {
            Transform t = canvas.transform.Find(name);
            if (t != null) DestroyImmediate(t.gameObject);
        }
        
        var allImages = Resources.FindObjectsOfTypeAll<Image>();
        foreach (var img in allImages)
        {
            if (img.gameObject.scene.isLoaded && (img.name == "ShopPanel" || img.name.Contains("Premium Shop")))
                 DestroyImmediate(img.gameObject);
        }

        // 2. 아름다운 상점 생성 (업그레이드 버전)
        GameObject shopPanel = CreateBeautifulShop(canvas);

        // 3. 매니저 연결
        SchoolLobbyManager manager = FindObjectOfType<SchoolLobbyManager>();
        if (manager != null)
        {
            SerializedObject so = new SerializedObject(manager);
            SerializedProperty prop = so.FindProperty("shopPanel");
            if (prop != null) { prop.objectReferenceValue = shopPanel; so.ApplyModifiedProperties(); }
            else { manager.shopPanel = shopPanel; EditorUtility.SetDirty(manager); }
        }

        // 4. 로그인 정보창 위치 수정
        Transform userInfo = canvas.transform.Find("UserInfoPanel");
        if (userInfo != null)
        {
            RectTransform rt = userInfo.GetComponent<RectTransform>();
            // Y 위치를 0으로 올려서 버튼과 라인 맞춤. X는 LOGIN 버튼(240) 옆으로.
            rt.anchoredPosition = new Vector2(260, 0); 
            // 텍스트 정렬을 위해 높이 조정
            rt.sizeDelta = new Vector2(400, 60);
        }

        EditorUtility.DisplayDialog("완료", "상점 UI 디자인 및 기능 연결 완료!\nPlay를 눌러 확인하세요.", "확인");
    }

    private static GameObject CreateBeautifulShop(Canvas canvas)
    {
        GameObject shopCanvas = new GameObject("StepUpShopPanel");
        shopCanvas.transform.SetParent(canvas.transform, false);
        
        RectTransform shopRt = shopCanvas.AddComponent<RectTransform>();
        shopRt.anchorMin = Vector2.zero;
        shopRt.anchorMax = Vector2.one;
        shopRt.sizeDelta = Vector2.zero;
        shopRt.anchoredPosition = Vector2.zero;

        // 배경: 전체 화면 꽉 채우는 반투명 블랙 (1개만)
        Image bgImg = shopCanvas.AddComponent<Image>();
        bgImg.color = new Color(0, 0, 0, 0.92f); // 아주 어두운 배경
        bgImg.raycastTarget = true;
        
        // 배경 클릭 닫기용 버튼
        shopCanvas.AddComponent<Button>();

        // ★ 핵심: 로직 스크립트 부착
        shopCanvas.AddComponent<ShopLogic>();

        // 메인 패널 (팝업)
        GameObject panelObj = new GameObject("MainPanel");
        panelObj.transform.SetParent(shopCanvas.transform, false);

        RectTransform panelRt = panelObj.AddComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.sizeDelta = new Vector2(1200, 750); // 넉넉한 크기

        // 패널 배경 (카드 느낌)
        Image panelImg = panelObj.AddComponent<Image>();
        panelImg.color = new Color(0.12f, 0.12f, 0.15f, 1f); // 고급스러운 다크 그레이
        
        Outline outline = panelObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.2f, 0.6f, 1f, 0.5f); // 은은한 블루 테두리
        outline.effectDistance = new Vector2(2, -2);

        // 헤더
        CreateText(panelObj, "Title", "SHOP", 50, FontStyles.Bold, TextAlignmentOptions.Center, new Color(1f, 1f, 1f),
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -60), new Vector2(0, 100));

        // 내 코인 (우측 상단)
        CreateText(panelObj, "MyCoin", "💎 1,000 G", 30, FontStyles.Bold, TextAlignmentOptions.MidlineRight, new Color(1f, 0.8f, 0.2f),
             new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-60, -60), new Vector2(300, 60));

        // 상품 그리드
        GameObject gridObj = new GameObject("Grid");
        gridObj.transform.SetParent(panelObj.transform, false);
        RectTransform gridRt = gridObj.AddComponent<RectTransform>();
        gridRt.anchorMin = new Vector2(0.5f, 0.5f);
        gridRt.anchorMax = new Vector2(0.5f, 0.5f);
        gridRt.sizeDelta = new Vector2(1100, 450);
        gridRt.anchoredPosition = new Vector2(0, 30);

        GridLayoutGroup grid = gridObj.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(320, 420); 
        grid.spacing = new Vector2(40, 0);
        grid.childAlignment = TextAnchor.MiddleCenter;
        
        // 상품들 (색상 통일: 다크 네이비 베이스)
        Color cardBaseColor = new Color(0.18f, 0.18f, 0.22f); 
        
        CreateProductCard(gridObj, "K-POP 패키지 Vol.1", "2,000 G", "NewJeans, IVE 등\n인기 곡 5곡 포함!", cardBaseColor);
        CreateProductCard(gridObj, "네온 아바타 세트", "5,000 G", "빛나는 네온 스타일\n한정판 아바타", cardBaseColor);
        CreateProductCard(gridObj, "스타 노트 스킨", "1,500 G", "노트가 별 모양으로\n변경됩니다.", cardBaseColor);

        // 하단 충전 버튼
        GameObject chargeBtnObj = new GameObject("ChargeButton");
        chargeBtnObj.transform.SetParent(panelObj.transform, false);
        RectTransform chargeBtnRt = chargeBtnObj.AddComponent<RectTransform>();
        chargeBtnRt.anchorMin = new Vector2(0.5f, 0);
        chargeBtnRt.anchorMax = new Vector2(0.5f, 0);
        chargeBtnRt.anchoredPosition = new Vector2(0, 50);
        chargeBtnRt.sizeDelta = new Vector2(300, 70);

        Image chargeImg = chargeBtnObj.AddComponent<Image>();
        chargeImg.color = new Color(0.2f, 0.5f, 1f); // 선명한 블루
        Button chargeBtn = chargeBtnObj.AddComponent<Button>();
        chargeBtn.transition = Selectable.Transition.ColorTint;

        CreateText(chargeBtnObj, "Text", "코인 충전하기", 26, FontStyles.Bold, TextAlignmentOptions.Center, Color.white,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

        // 닫기 버튼 (X)
        GameObject closeBtnObj = new GameObject("CloseButton");
        closeBtnObj.transform.SetParent(panelObj.transform, false);
        RectTransform closeBtnRt = closeBtnObj.AddComponent<RectTransform>();
        closeBtnRt.anchorMin = new Vector2(1, 1);
        closeBtnRt.anchorMax = new Vector2(1, 1);
        closeBtnRt.anchoredPosition = new Vector2(-30, -30);
        closeBtnRt.sizeDelta = new Vector2(50, 50);

        Image closeImg = closeBtnObj.AddComponent<Image>();
        closeImg.color = new Color(1, 1, 1, 0.1f);
        Button closeBtn = closeBtnObj.AddComponent<Button>();
        
        CreateText(closeBtnObj, "X", "X", 30, FontStyles.Bold, TextAlignmentOptions.Center, new Color(0.7f, 0.7f, 0.7f),
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

        shopCanvas.SetActive(false);
        return shopCanvas;
    }

    private static void CreateProductCard(GameObject parent, string title, string price, string desc, Color bgColor)
    {
        GameObject card = new GameObject("Card");
        card.transform.SetParent(parent.transform, false);
        
        Image img = card.AddComponent<Image>();
        img.color = bgColor; 
        
        Button btn = card.AddComponent<Button>(); // 클릭 가능하게
        Outline ol = card.AddComponent<Outline>();
        ol.effectColor = new Color(1, 1, 1, 0.05f); // 아주 은은한 테두리

        // 상품명 (상단)
        CreateText(card, "Title", title, 24, FontStyles.Bold, TextAlignmentOptions.TopLeft, new Color(0.9f, 0.9f, 1f),
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(25, -25), new Vector2(-50, 50));
        
        // 설명 (중간)
        CreateText(card, "Desc", desc, 18, FontStyles.Normal, TextAlignmentOptions.TopLeft, new Color(0.6f, 0.6f, 0.7f),
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(25, -80), new Vector2(-50, 100));

        // 아이콘 (중앙)
        GameObject icon = new GameObject("Icon");
        icon.transform.SetParent(card.transform, false);
        RectTransform iconRt = icon.AddComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0.5f, 0.5f);
        iconRt.anchorMax = new Vector2(0.5f, 0.5f);
        iconRt.sizeDelta = new Vector2(80, 80);
        iconRt.anchoredPosition = new Vector2(0, 10);
        Image iconImg = icon.AddComponent<Image>();
        iconImg.color = new Color(1, 1, 1, 0.05f); // 반투명 박스

        // 하단 구매 버튼 영역
        GameObject buyDiv = new GameObject("BuyDiv");
        buyDiv.transform.SetParent(card.transform, false);
        RectTransform buyRt = buyDiv.AddComponent<RectTransform>();
        buyRt.anchorMin = new Vector2(0, 0);
        buyRt.anchorMax = new Vector2(1, 0);
        buyRt.sizeDelta = new Vector2(0, 60);
        
        Image buyImg = buyDiv.AddComponent<Image>();
        buyImg.color = new Color(0, 0, 0, 0.2f); // 하단 어둡게

        CreateText(buyDiv, "Price", price, 22, FontStyles.Bold, TextAlignmentOptions.Center, new Color(1f, 0.8f, 0.4f),
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
    }

    private static TextMeshProUGUI CreateText(GameObject parent, string name, string text, float size, FontStyles style, TextAlignmentOptions align, Color color,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 sizeDelta)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent.transform, false);
        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.font = koreanFont;
        tmp.fontStyle = style;
        tmp.alignment = align;
        tmp.color = color;
        
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        if (sizeDelta != Vector2.zero) rt.sizeDelta = sizeDelta;
        
        return tmp;
    }

    private static TMP_FontAsset LoadKoreanFont()
    {
        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts/NotoSansKR-Regular SDF");
        if (font == null) font = Resources.Load<TMP_FontAsset>("NotoSansKR-Regular SDF");
        if (font == null)
        {
            string[] guids = AssetDatabase.FindAssets("NotoSansKR-Regular SDF t:TMP_FontAsset");
            if (guids.Length > 0) font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
        return font;
    }
}
