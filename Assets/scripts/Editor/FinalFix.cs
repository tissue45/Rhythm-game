using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class FinalFix : EditorWindow
{
    private static TMP_FontAsset koreanFont;

    [MenuItem("Tools/Fix Rhythm Game/🚀 FINAL DEPLOYMENT FIX (Click This!)")]
    public static void RunFinalFix()
    {
        koreanFont = LoadKoreanFont();
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        Debug.Log("--- 🔥 Final Fix Started 🔥 ---");

        // 1. [대청소] 화면 정리 (중복 UI 삭제)
        CleanUpEverything(canvas.transform);

        // 2. [매니저] UI 매니저 생성 및 설정
        LobbyUIManager uiManager = SetupUIManager();

        // 3. [상점] 상점 생성
        GameObject newShop = CreatePremiumShop(canvas);

        // 4. [로비] 버튼 위치 수정 (-450)
        FixLobbyButtons(canvas);

        // 5. [로그인] 개선된 로그인 패널 생성 (배경 + 1학년)
        // uiManager를 통해 생성 (디자인 통일)
        GameObject userInfo = uiManager.CreateUserInfoPanel(canvas.transform);

        // EXTRA. [랭킹] 랭킹 패널 생성 (Missing Reference Fix)
        GameObject rankingPanel = uiManager.CreateRankingPanel(canvas.transform);

        // 6. [매니저] SchoolLobbyManager 연결
        SchoolLobbyManager manager = FindObjectOfType<SchoolLobbyManager>();
        if (manager != null)
        {
            manager.shopPanel = newShop;
            manager.rankingPanel = rankingPanel; // [FIX] Assign Ranking Panel
            
            // [FIX] SongSelectPanel 연결 (GAME START 버튼 오류 방지)
            GameObject songPanel = GameObject.Find("SongSelectPanel");
            if (songPanel != null)
            {
                manager.songSelectPanel = songPanel;
                Debug.Log("[FinalFix] Connected SongSelectPanel to manager");
            }
            else
            {
                Debug.LogWarning("[FinalFix] SongSelectPanel not found - GAME START may not work");
            }
            
            EditorUtility.SetDirty(manager);
        }

        EditorUtility.DisplayDialog("완료", "✅ UI 대청소 완료\n✅ 랭킹/상점/곡선택 패널 연결 완료\n✅ 리듬게임 스타일 버튼 적용\n\n이제 Play 해보세요!", "확인");
    }

    private static LobbyUIManager SetupUIManager()
    {
        LobbyUIManager manager = FindObjectOfType<LobbyUIManager>();
        if (manager == null)
        {
            GameObject obj = new GameObject("LobbyUIManager");
            manager = obj.AddComponent<LobbyUIManager>();
        }
        manager.Setup(); // 폰트 로드 등 초기화
        return manager;
    }

    private static void CleanUpEverything(Transform canvas)
    {
        string[] garbages = { 
            "UserInfoPanel", "ShopPanel", "StepUpShopPanel", "PremiumShop", 
            "ShopCanvas", "LobbyButtonsContainer", "RightMenuVector", 
            "OptionsPanel", "ProfilePanel", "RankingPanel"
        };

        foreach (string name in garbages) DeleteGameObjectByName(canvas, name);

        for (int i = canvas.childCount - 1; i >= 0; i--)
        {
            Transform child = canvas.GetChild(i);
            if (child.name.Contains("UserInfoPanel")) DestroyImmediate(child.gameObject);
        }
    }

    // --- 상점 (기존 유지) ---
    private static GameObject CreatePremiumShop(Canvas canvas)
    {
        GameObject shopCanvas = new GameObject("StepUpShopPanel");
        shopCanvas.transform.SetParent(canvas.transform, false);
        
        RectTransform shopRt = shopCanvas.AddComponent<RectTransform>();
        shopRt.anchorMin = Vector2.zero; shopRt.anchorMax = Vector2.one;
        shopRt.sizeDelta = Vector2.zero; shopRt.anchoredPosition = Vector2.zero;

        Image bgImg = shopCanvas.AddComponent<Image>();
        bgImg.color = new Color(0, 0, 0, 0.95f); bgImg.raycastTarget = true;
        shopCanvas.AddComponent<Button>();
        shopCanvas.AddComponent<ShopLogic>();

        GameObject panelObj = new GameObject("MainPanel");
        panelObj.transform.SetParent(shopCanvas.transform, false);
        RectTransform panelRt = panelObj.AddComponent<RectTransform>();
        panelRt.sizeDelta = new Vector2(1100, 700);

        Image panelImg = panelObj.AddComponent<Image>();
        panelImg.color = new Color(0.12f, 0.12f, 0.15f);
        panelObj.AddComponent<Outline>().effectColor = new Color(0.4f, 0.8f, 1f, 0.6f);

        CreateText(panelObj, "Title", "상점", 42, FontStyles.Bold, TextAlignmentOptions.Center, Color.white, new Vector2(0, 310));
        CreateText(panelObj, "Coin", "💎 1,000 G", 28, FontStyles.Bold, TextAlignmentOptions.Right, new Color(1f, 0.8f, 0.2f), new Vector2(0, 250));

        GameObject gridObj = new GameObject("Grid");
        gridObj.transform.SetParent(panelObj.transform, false);
        RectTransform gridRt = gridObj.AddComponent<RectTransform>();
        gridRt.anchoredPosition = new Vector2(0, 20); gridRt.sizeDelta = new Vector2(1000, 450);

        GridLayoutGroup grid = gridObj.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(300, 400); grid.spacing = new Vector2(40, 0);
        grid.childAlignment = TextAnchor.MiddleCenter;

        Color cardColor = new Color(0.18f, 0.18f, 0.22f); 
        CreateProductCard(gridObj, "K-POP 패키지 Vol.1", "2,000 G", "NewJeans, IVE 등\n인기 곡 5곡 포함!", cardColor);
        CreateProductCard(gridObj, "네온 아바타 세트", "5,000 G", "빛나는 네온 스타일\n한정판 아바타", cardColor);
        CreateProductCard(gridObj, "스타 노트 스킨", "1,500 G", "노트가 별 모양으로\n변경됩니다.", cardColor);

        // 충전 버튼 (하단 검은 팝업 제거됨)
        GameObject chargeBtnObj = new GameObject("ChargeButton");
        chargeBtnObj.transform.SetParent(panelObj.transform, false);
        RectTransform chargeBtnRt = chargeBtnObj.AddComponent<RectTransform>();
        chargeBtnRt.anchoredPosition = new Vector2(0, -300); chargeBtnRt.sizeDelta = new Vector2(300, 60);

        Image chargeImg = chargeBtnObj.AddComponent<Image>();
        chargeImg.color = new Color(0.2f, 0.5f, 1f);
        Button chargeBtn = chargeBtnObj.AddComponent<Button>();
        CreateText(chargeBtnObj, "Txt", "코인 충전하기", 24, FontStyles.Bold, TextAlignmentOptions.Center, Color.white, Vector2.zero);

        GameObject closeBtnObj = new GameObject("CloseButton");
        closeBtnObj.transform.SetParent(panelObj.transform, false);
        RectTransform closeRt = closeBtnObj.AddComponent<RectTransform>();
        closeRt.anchorMin = Vector2.one; closeRt.anchorMax = Vector2.one;
        closeRt.anchoredPosition = new Vector2(-30, -30); closeRt.sizeDelta = new Vector2(50, 50);
        Image closeImg = closeBtnObj.AddComponent<Image>();
        closeImg.color = new Color(1,1,1,0.1f);
        closeBtnObj.AddComponent<Button>();
        CreateText(closeBtnObj, "X", "X", 30, FontStyles.Bold, TextAlignmentOptions.Center, Color.white, Vector2.zero);

        shopCanvas.SetActive(false);
        return shopCanvas;
    }

    // --- 로비 버튼 수정 (완전 재생성) ---
    private static void FixLobbyButtons(Canvas canvas)
    {
        // 1. 기존 컨테이너 완전 삭제
        Transform container = canvas.transform.Find("LobbyButtonsContainer");
        if (container != null) DestroyImmediate(container.gameObject);

        // 2. 개별 버튼들도 이름으로 찾아서 모두 삭제 (혹시 모를 중복 방지)
        string[] buttonNames = { "Btn_GameStart", "Btn_Ranking", "Btn_Shop", "Btn_Option" };
        foreach (string btnName in buttonNames)
        {
            GameObject oldBtn = GameObject.Find(btnName);
            if (oldBtn != null) DestroyImmediate(oldBtn);
        }

        // 3. 새 컨테이너 생성
        GameObject newContainer = new GameObject("LobbyButtonsContainer");
        newContainer.transform.SetParent(canvas.transform, false);
        RectTransform rt = newContainer.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 0.5f); rt.anchorMax = new Vector2(1, 0.5f);
        
        rt.anchoredPosition = new Vector2(-450, 0); 
        rt.sizeDelta = new Vector2(600, 600);

        VerticalLayoutGroup vlg = newContainer.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 30; vlg.childAlignment = TextAnchor.MiddleRight;
        vlg.childControlWidth = false; vlg.childControlHeight = false;

        SchoolLobbyManager mgr = FindObjectOfType<SchoolLobbyManager>();
        
        Color unifiedBlue = new Color(0.2f, 0.55f, 0.8f, 1f); // User preferred blue
        
        // 4. 완전히 새로운 버튼 생성 (RhythmButtonStyle 절대 추가 안함)
        // [User Request] Cyan Background + Black Text style
        Color cyanColor = new Color(0f, 1f, 1f, 1f); 
        Color blackText = Color.black;

        CreateCleanButton(newContainer.transform, "Btn_GameStart", "GAME START", cyanColor, blackText, 400, 90, mgr.OnGameStartClick);
        CreateCleanButton(newContainer.transform, "Btn_Ranking", "RANKING", cyanColor, blackText, 400, 90, mgr.OnRankingClick);
        CreateCleanButton(newContainer.transform, "Btn_Shop", "SHOP", cyanColor, blackText, 400, 90, mgr.OnShopClick);
        
        // 5. [강제 연결] 버튼 이벤트 재확인 및 바인딩
        Button gameStartBtn = newContainer.transform.Find("Btn_GameStart")?.GetComponent<Button>();
        Button rankingBtn = newContainer.transform.Find("Btn_Ranking")?.GetComponent<Button>();
        Button shopBtn = newContainer.transform.Find("Btn_Shop")?.GetComponent<Button>();
        
        if (gameStartBtn != null)
        {
            gameStartBtn.onClick.RemoveAllListeners();
            gameStartBtn.onClick.AddListener(mgr.OnGameStartClick);
            Debug.Log("[FinalFix] GAME START button event bound successfully");
        }
        
        if (rankingBtn != null)
        {
            rankingBtn.onClick.RemoveAllListeners();
            rankingBtn.onClick.AddListener(mgr.OnRankingClick);
            Debug.Log("[FinalFix] RANKING button event bound successfully");
        }
        
        if (shopBtn != null)
        {
            shopBtn.onClick.RemoveAllListeners();
            shopBtn.onClick.AddListener(mgr.OnShopClick);
            Debug.Log("[FinalFix] SHOP button event bound successfully");
        }
    }

    private static void DeleteGameObjectByName(Transform parent, string name)
    {
        bool found = true;
        while (found)
        {
            found = false;
            foreach(Transform child in parent) 
            {
                if(child.name.Contains(name)) 
                {
                    DestroyImmediate(child.gameObject);
                    found = true;
                    break; 
                }
            }
        }
    }

    private static void CreateProductCard(GameObject parent, string title, string price, string desc, Color color)
    {
        GameObject card = new GameObject("Card");
        card.transform.SetParent(parent.transform, false);
        card.AddComponent<Image>().color = color;
        card.AddComponent<Button>();
        card.AddComponent<Outline>().effectColor = new Color(1,1,1,0.1f);
        
        CreateText(card, "T", title, 20, FontStyles.Bold, TextAlignmentOptions.Center, Color.white, new Vector2(0, 120));
        CreateText(card, "D", desc, 16, FontStyles.Normal, TextAlignmentOptions.Top, Color.gray, new Vector2(0, -50));
        CreateText(card, "P", price, 24, FontStyles.Bold, TextAlignmentOptions.Bottom, new Color(1f, 0.8f, 0.2f), new Vector2(0, -150));
    }

    private static void CreateCleanButton(Transform parent, string name, string text, Color bgColor, Color txtColor, float width, float height, UnityEngine.Events.UnityAction action)
    {
        // 완전히 깨끗한 버튼 생성 (RhythmButtonStyle 절대 추가 안함)
        GameObject btn = new GameObject(name);
        btn.transform.SetParent(parent, false);
        
        // RectTransform 먼저 설정
        RectTransform btnRt = btn.AddComponent<RectTransform>();
        btnRt.sizeDelta = new Vector2(width, height);
        
        // === 리듬게임 스타일 배경 ===
        Image img = btn.AddComponent<Image>();
        img.color = bgColor;
        img.type = Image.Type.Sliced; // 둥근 모서리 지원
        
        // 네온 글로우 효과 (Outline)
        Outline outline = btn.AddComponent<Outline>();
        outline.effectColor = new Color(bgColor.r, bgColor.g, bgColor.b, 0.6f);
        outline.effectDistance = new Vector2(4, -4);
        
        // 추가 글로우 레이어 (Shadow)
        Shadow glow = btn.AddComponent<Shadow>();
        glow.effectColor = new Color(bgColor.r, bgColor.g, bgColor.b, 0.4f);
        glow.effectDistance = new Vector2(0, 0);
        
        // Button 추가
        Button b = btn.AddComponent<Button>();
        if(action != null) b.onClick.AddListener(action);
        
        // 호버 색상 설정 (더 밝게)
        ColorBlock colors = b.colors;
        colors.normalColor = Color.white; // Tint applied to base color
        colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        b.colors = colors;
        
        // LayoutElement로 크기 강제
        LayoutElement le = btn.AddComponent<LayoutElement>();
        le.preferredWidth = width;
        le.preferredHeight = height;
        le.minWidth = width;
        le.minHeight = height;
        
        // === 텍스트 (네온 스타일) ===
        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(btn.transform, false);
        
        TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 42;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = txtColor;
        tmp.font = koreanFont;
        
        // 텍스트 글로우
        Outline textOutline = txtObj.AddComponent<Outline>();
        textOutline.effectColor = new Color(0.8f, 1f, 1f, 0.8f); // 시안 글로우
        textOutline.effectDistance = new Vector2(2, -2);
        
        RectTransform txtRt = txtObj.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.sizeDelta = Vector2.zero;
        txtRt.anchoredPosition = Vector2.zero;
        
        Debug.Log($"[FinalFix] Created rhythm-style button '{name}' with size {width}x{height}");
    }

    private static void CreateText(GameObject p, string n, string t, float s, FontStyles st, TextAlignmentOptions a, Color c, Vector2 offset)
    {
        GameObject o = new GameObject(n);
        o.transform.SetParent(p.transform, false);
        TextMeshProUGUI tmp = o.AddComponent<TextMeshProUGUI>();
        tmp.text = t; tmp.fontSize = s; tmp.fontStyle = st; tmp.alignment = a; tmp.color = c; tmp.font = koreanFont;
        o.GetComponent<RectTransform>().anchoredPosition = offset;
        o.GetComponent<RectTransform>().sizeDelta = new Vector2(p.GetComponent<RectTransform>().rect.width - 20, 150);
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
