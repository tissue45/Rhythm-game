using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 모든 버튼 문제를 한 번에 해결하는 통합 스크립트
/// </summary>
public class FixAllButtons : EditorWindow
{
    // [NEW] 한글 폰트 로드 헬퍼 함수
    private static TMP_FontAsset LoadKoreanFont()
    {
        // 1. Resources 폴더에서 로드 시도
        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts/NotoSansKR-Regular SDF");
        if (font != null) return font;

        font = Resources.Load<TMP_FontAsset>("NotoSansKR-Regular SDF");
        if (font != null) return font;

        // 2. 프로젝트 전체에서 검색 (에디터 전용)
        string[] guids = AssetDatabase.FindAssets("NotoSansKR-Regular SDF t:TMP_FontAsset");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        }
        
        // 없으면 기본 폰트라도 반환 (null 방지)
        if (font == null) 
        {
            Debug.LogWarning("Korean Font(NotoSansKR-Regular SDF) not found! Using default font.");
            return Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        }

        return font;
    }

    [MenuItem("Rhythm Game/Fix All Buttons (Complete)")]
    public static void FixEverything()
    {
        Debug.Log("=== Starting Complete Button Fix ===");

        // 1. Canvas 찾기
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas not found!");
            return;
        }

        // [CRITICAL FIX] 캔버스 해상도 고정 (글씨 깨짐 해결)
        SetupCanvasScaler(canvas);

        // 2. 강력한 삭제: 여러 번 순회하면서 완전히 삭제
        Debug.Log("Phase 1: Deleting all existing buttons...");

        // 2.1. LobbyButtonsContainer를 먼저 찾아서 완전히 삭제
        for (int i = 0; i < 3; i++) // 3번 반복해서 확실하게 삭제
        {
            GameObject oldContainer = GameObject.Find("LobbyButtonsContainer");
            if (oldContainer != null)
            {
                DestroyImmediate(oldContainer);
                Debug.Log($"[Pass {i+1}] Deleted LobbyButtonsContainer");
            }
        }

        // 2.2. 이름으로 버튼 찾아서 삭제 (Gemini가 만든 것들 포함)
        string[] buttonNames = {
            "Btn_GameStart", "Btn_Start", "StartButton",
            "Btn_Ranking", "Btn_Shop", "Btn_Profile",
            "Btn_Option", "Btn_Exit", "ExitButton",
            "QuitButton", "ConnectionPanel", "TitleText",
            "BackgroundPanel", "GameBorder", "CancelButton"
        };

        for (int pass = 0; pass < 2; pass++) // 2번 패스
        {
            foreach (string btnName in buttonNames)
            {
                GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
                foreach (GameObject obj in allObjects)
                {
                    if (obj.name == btnName)
                    {
                        DestroyImmediate(obj);
                        Debug.Log($"[Pass {pass+1}] Deleted: {btnName}");
                    }
                }
            }
        }

        // 2.3. [CRITICAL] 씬 전체에서 EXIT 버튼 강제 삭제
        GameObject[] exitObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int exitDeleteCount = 0;
        foreach (GameObject obj in exitObjects)
        {
            if (obj == null) continue;
            string lowerName = obj.name.ToLower();
            if (lowerName.Contains("exit") || lowerName.Contains("quit"))
            {
                Debug.Log($"[CRITICAL] Force deleting EXIT/QUIT button: {obj.name}");
                DestroyImmediate(obj);
                exitDeleteCount++;
            }
        }
        Debug.Log($"Deleted {exitDeleteCount} EXIT/QUIT related objects");

        // 2.4. Canvas의 모든 자식 검사 (LOGIN 제외)
        var toDelete = new System.Collections.Generic.List<GameObject>();
        foreach (Transform child in canvas.transform)
        {
            if (child == null) continue;

            // LOGIN은 반드시 유지
            if (child.name == "LOGIN") continue;

            // 버튼 관련 오브젝트는 모두 삭제
            if (child.GetComponent<Button>() != null ||
                child.name.Contains("Btn") ||
                child.name.Contains("Button") ||
                child.name.Contains("Container"))
            {
                toDelete.Add(child.gameObject);
            }
        }

        foreach (var obj in toDelete)
        {
            if (obj != null)
            {
                Debug.Log($"Deleting orphan: {obj.name}");
                DestroyImmediate(obj);
            }
        }

        Debug.Log($"Phase 1 Complete: Deleted {toDelete.Count} orphan objects");

        // 2.4. LobbyUIBuilder 컴포넌트 제거 (런타임에 버튼 재생성 방지)
        LobbyUIBuilder[] lobbyBuilders = Object.FindObjectsByType<LobbyUIBuilder>(FindObjectsSortMode.None);
        foreach (var builder in lobbyBuilders)
        {
            Debug.Log($"Disabling LobbyUIBuilder on {builder.gameObject.name}");
            builder.enabled = false; // 컴포넌트 비활성화
        }

        // 3. SchoolLobbyManager 찾기
        SchoolLobbyManager manager = Object.FindFirstObjectByType<SchoolLobbyManager>();
        if (manager == null)
        {
            Debug.LogError("SchoolLobbyManager not found!");
            return;
        }

        // 4. 새 컨테이너 생성
        GameObject container = new GameObject("LobbyButtonsContainer");
        container.transform.SetParent(canvas.transform, false);

        RectTransform containerRt = container.AddComponent<RectTransform>();
        containerRt.anchorMin = new Vector2(1, 0.5f);
        containerRt.anchorMax = new Vector2(1, 0.5f);
        containerRt.pivot = new Vector2(1, 0.5f);
        containerRt.anchoredPosition = new Vector2(-90, 0);
        containerRt.sizeDelta = new Vector2(310, 650);

        // 5. VerticalLayoutGroup 설정
        VerticalLayoutGroup vlg = container.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 30;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlHeight = false;
        vlg.childControlWidth = false;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = false;
        vlg.padding = new RectOffset(0, 0, 40, 40);

        Debug.Log("Phase 2: Created new container with layout");

        // 6. 버튼들 생성 (개선된 디자인)
        // [FIX] 버튼 디자인 개선 (크고 멋지게, 옵션 삭제)
        // GAME START (가장 크게 강조)
        CreateModernButton(container.transform, "Btn_GameStart", "GAME START",
            new Vector2(320, 110), 40,
            new Color(0.1f, 0.15f, 0.25f, 0.95f), // 다크 네이비
            new Color(0.2f, 0.25f, 0.35f, 1f),    // 호버 시 밝게
            () => manager.OnGameStartClick());

        // RANKING
        CreateModernButton(container.transform, "Btn_Ranking", "RANKING",
            new Vector2(320, 100), 36,
            new Color(0.1f, 0.15f, 0.25f, 0.95f),
            new Color(0.2f, 0.25f, 0.35f, 1f),
            () => manager.OnRankingClick());

        // SHOP
        CreateModernButton(container.transform, "Btn_Shop", "SHOP",
            new Vector2(320, 100), 36,
            new Color(0.1f, 0.15f, 0.25f, 0.95f),
            new Color(0.2f, 0.25f, 0.35f, 1f),
            () => manager.OnShopClick());

        // [REMOVED] Option Button
        // CreateModernButton(container.transform, "Btn_Option", "OPTION", ... );

        // [REMOVED] EXIT 버튼 제거 - 사용자 요청

        Debug.Log("Phase 3: Created all 4 buttons (GAME START, RANKING, SHOP, OPTION)");

        // RhythmButtonStyle 컴포넌트 제거 (깔끔한 버튼 유지)
        RemoveRhythmButtonStyles(container);

        // Layout 즉시 적용
        LayoutRebuilder.ForceRebuildLayoutImmediate(containerRt);

        // 7. LOGIN 버튼 수정
        FixLoginButton(canvas);

        // 7.5. 사용자 정보 패널 생성
        CreateUserInfoPanel(canvas);

        // 7.6. 인게임 로그인 패널 생성
        CreateInGameLoginPanel(canvas);

        // 8. Layout 강제 업데이트
        Canvas.ForceUpdateCanvases();

        // 9. 모든 변경사항을 Unity 에디터에 알림
        EditorUtility.SetDirty(canvas.gameObject);
        EditorUtility.SetDirty(container);

        // 10. 씬 저장 권장
        if (UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().isDirty)
        {
            Debug.Log("Scene has changes. Please save the scene (Ctrl+S)");
        }

        Debug.Log("=== Complete Button Fix Done ===");
        Debug.Log("✅ All buttons created successfully!");
        Debug.Log("💾 Remember to save the scene (Ctrl+S)");
    }

    static void CreateModernButton(Transform parent, string name, string label,
        Vector2 size, float fontSize, Color normalColor, Color hoverColor, UnityEngine.Events.UnityAction action)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform btnRt = btnObj.AddComponent<RectTransform>();
        btnRt.sizeDelta = size;

        LayoutElement le = btnObj.AddComponent<LayoutElement>();
        le.preferredWidth = size.x;
        le.preferredHeight = size.y;
        le.minWidth = size.x;
        le.minHeight = size.y;

        // Background with gradient effect (Normal Color 적용)
        Image img = btnObj.AddComponent<Image>();
        img.color = normalColor; // [FIX] 파라미터로 받은 색상 적용
        img.type = Image.Type.Simple;
        img.raycastTarget = true; // [FIX] Ensure button receives clicks

        // Shadow effect
        GameObject shadowObj = new GameObject("Shadow");
        shadowObj.transform.SetParent(btnObj.transform, false);
        Image shadowImg = shadowObj.AddComponent<Image>();
        shadowImg.color = new Color(0, 0, 0, 0.5f);
        shadowImg.raycastTarget = false; // [FIX] Don't block button clicks

        RectTransform shadowRt = shadowObj.GetComponent<RectTransform>();
        shadowRt.anchorMin = Vector2.zero;
        shadowRt.anchorMax = Vector2.one;
        shadowRt.sizeDelta = Vector2.zero;
        shadowRt.anchoredPosition = new Vector2(4, -4);
        shadowObj.transform.SetAsFirstSibling();

        // Glow border (양쪽)
        GameObject glowObj = new GameObject("Glow");
        glowObj.transform.SetParent(btnObj.transform, false);
        Image glowImg = glowObj.AddComponent<Image>();
        glowImg.color = new Color(0f, 1f, 1f, 0.8f);
        glowImg.raycastTarget = false; // [FIX] Don't block button clicks

        RectTransform glowRt = glowObj.GetComponent<RectTransform>();
        glowRt.anchorMin = new Vector2(0, 0);
        glowRt.anchorMax = new Vector2(0, 1);
        glowRt.pivot = new Vector2(0, 0.5f);
        glowRt.sizeDelta = new Vector2(4, 0);
        glowRt.anchoredPosition = Vector2.zero;

        // Button Component
        Button btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(action);
        btn.transition = Selectable.Transition.ColorTint;

        ColorBlock colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.3f, 1.3f, 1.3f, 1f);
        colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        colors.selectedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
        btn.colors = colors;

        // Text with outline
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);

        TextMeshProUGUI txt = textObj.AddComponent<TextMeshProUGUI>();
        txt.text = label;
        txt.fontSize = fontSize;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = Color.white;
        txt.fontStyle = FontStyles.Bold;
        txt.enableAutoSizing = false;

        // Text outline settings removed for cleaner look
        // txt.outlineColor = new Color(0, 0, 0, 0.8f);
        // txt.outlineWidth = 0.2f;

        RectTransform txtRt = textObj.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.sizeDelta = Vector2.zero;
        txtRt.anchoredPosition = Vector2.zero;

        EditorUtility.SetDirty(btnObj);
        Debug.Log($"Created modern button: {name}");
    }

    static void RemoveRhythmButtonStyles(GameObject container)
    {
        // 컨테이너의 모든 자식에서 RhythmButtonStyle 제거
        RhythmButtonStyle[] styles = container.GetComponentsInChildren<RhythmButtonStyle>();
        foreach (var style in styles)
        {
            DestroyImmediate(style);
        }
        if (styles.Length > 0)
        {
            Debug.Log($"Removed {styles.Length} RhythmButtonStyle components");
        }
    }

    static void FixLoginButton(Canvas canvas)
    {
        // [CRITICAL] 먼저 기존 LOGIN 버튼 완전히 삭제
        GameObject oldLogin = GameObject.Find("LOGIN");
        if (oldLogin != null)
        {
            DestroyImmediate(oldLogin);
            Debug.Log("Deleted old LOGIN button");
        }

        // 새로운 깔끔한 LOGIN 버튼 생성
        GameObject loginBtn = new GameObject("LOGIN");
        loginBtn.transform.SetParent(canvas.transform, false);

        RectTransform rt = loginBtn.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(30, -30);
        rt.sizeDelta = new Vector2(150, 60);

        // 깔끔한 배경 (GAME START와 동일한 스타일)
        Image img = loginBtn.AddComponent<Image>();
        img.color = new Color(0.0f, 0.8f, 1.0f, 1f); // 밝은 시안

        // 버튼 컴포넌트
        Button btn = loginBtn.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 1f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        btn.colors = colors;

        // 텍스트 (깔끔하게, 효과 없음)
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(loginBtn.transform, false);

        TextMeshProUGUI txt = textObj.AddComponent<TextMeshProUGUI>();
        txt.text = "LOGIN";
        txt.fontSize = 28;
        txt.fontStyle = FontStyles.Bold;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = Color.white;
        txt.raycastTarget = false;

        RectTransform txtRt = textObj.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.sizeDelta = Vector2.zero;
        txtRt.anchoredPosition = Vector2.zero;

        Debug.Log("Created clean LOGIN button (no neon effects)");

        // LoginManager 추가 또는 연결
        LoginManager loginManager = Object.FindFirstObjectByType<LoginManager>();
        if (loginManager == null)
        {
            GameObject managerObj = new GameObject("LoginManager");
            loginManager = managerObj.AddComponent<LoginManager>();
            Debug.Log("Created LoginManager");
        }

        // 버튼 레퍼런스 및 URL 설정
        loginManager.loginButton = btn;
        loginManager.backendApiUrl = "http://localhost:3000";
        loginManager.loginEndpoint = "/api/login";

        Debug.Log($"LoginManager configured: {loginManager.backendApiUrl}{loginManager.loginEndpoint}");

        EditorUtility.SetDirty(loginBtn);
        EditorUtility.SetDirty(loginManager);
    }

    static void CreateUserInfoPanel(Canvas canvas)
    {
        // 기존 패널 삭제
        GameObject oldPanel = GameObject.Find("UserInfoPanel");
        if (oldPanel != null)
        {
            DestroyImmediate(oldPanel);
        }

        // [NEW] 리듬게임 스타일 사용자 정보 패널 (LOGIN 버튼 옆)
        GameObject panel = new GameObject("UserInfoPanel");
        panel.transform.SetParent(canvas.transform, false);

        RectTransform panelRt = panel.AddComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0, 1);
        panelRt.anchorMax = new Vector2(0, 1);
        panelRt.pivot = new Vector2(0, 1);
        panelRt.anchoredPosition = new Vector2(180, -30); // LOGIN 버튼 옆
        panelRt.sizeDelta = new Vector2(350, 55);

        // 배경 (다크 블루, 거의 불투명)
        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.08f, 0.12f, 0.18f, 0.95f);

        // 좌측 시안 테두리
        GameObject leftBorder = new GameObject("LeftBorder");
        leftBorder.transform.SetParent(panel.transform, false);
        Image leftBorderImg = leftBorder.AddComponent<Image>();
        leftBorderImg.color = new Color(0.0f, 0.8f, 1.0f, 0.9f); // 시안

        RectTransform leftBorderRt = leftBorder.GetComponent<RectTransform>();
        leftBorderRt.anchorMin = new Vector2(0, 0);
        leftBorderRt.anchorMax = new Vector2(0, 1);
        leftBorderRt.pivot = new Vector2(0, 0.5f);
        leftBorderRt.sizeDelta = new Vector2(3, 0);
        leftBorderRt.anchoredPosition = Vector2.zero;

        // 사용자 닉네임 (크게, 왼쪽 정렬)
        GameObject nameObj = new GameObject("UserNameText");
        nameObj.transform.SetParent(panel.transform, false);

        TextMeshProUGUI nameTxt = nameObj.AddComponent<TextMeshProUGUI>();
        nameTxt.text = "Guest";
        nameTxt.fontSize = 20;
        nameTxt.fontStyle = FontStyles.Bold;
        nameTxt.color = Color.white;
        nameTxt.alignment = TextAlignmentOptions.MidlineLeft;

        RectTransform nameRt = nameObj.GetComponent<RectTransform>();
        nameRt.anchorMin = new Vector2(0, 0.5f);
        nameRt.anchorMax = new Vector2(1, 1);
        nameRt.pivot = new Vector2(0, 0.5f);
        nameRt.sizeDelta = new Vector2(-15, 0);
        nameRt.anchoredPosition = new Vector2(15, 5);

        // 레벨 표시 (작게, 왼쪽 하단)
        GameObject levelObj = new GameObject("LevelText");
        levelObj.transform.SetParent(panel.transform, false);

        TextMeshProUGUI levelTxt = levelObj.AddComponent<TextMeshProUGUI>();
        levelTxt.text = "Lv.1";
        levelTxt.fontSize = 14;
        levelTxt.fontStyle = FontStyles.Normal;
        levelTxt.color = new Color(0.5f, 0.8f, 1.0f, 1f); // 밝은 시안
        levelTxt.alignment = TextAlignmentOptions.MidlineLeft;

        RectTransform levelRt = levelObj.GetComponent<RectTransform>();
        levelRt.anchorMin = new Vector2(0, 0);
        levelRt.anchorMax = new Vector2(0.3f, 0.5f);
        levelRt.pivot = new Vector2(0, 0.5f);
        levelRt.sizeDelta = new Vector2(0, 0);
        levelRt.anchoredPosition = new Vector2(15, -5);

        // 코인 아이콘 + 숫자 (오른쪽 하단)
        GameObject coinContainer = new GameObject("CoinContainer");
        coinContainer.transform.SetParent(panel.transform, false);

        RectTransform coinContainerRt = coinContainer.AddComponent<RectTransform>();
        coinContainerRt.anchorMin = new Vector2(0.5f, 0);
        coinContainerRt.anchorMax = new Vector2(1, 0.5f);
        coinContainerRt.pivot = new Vector2(1, 0.5f);
        coinContainerRt.sizeDelta = new Vector2(0, 0);
        coinContainerRt.anchoredPosition = new Vector2(-15, -5);

        // 코인 아이콘 (이모지)
        GameObject coinIconObj = new GameObject("CoinIcon");
        coinIconObj.transform.SetParent(coinContainer.transform, false);

        TextMeshProUGUI coinIconTxt = coinIconObj.AddComponent<TextMeshProUGUI>();
        coinIconTxt.text = "💎";
        coinIconTxt.fontSize = 16;
        coinIconTxt.alignment = TextAlignmentOptions.MidlineRight;

        RectTransform coinIconRt = coinIconObj.GetComponent<RectTransform>();
        coinIconRt.anchorMin = new Vector2(0, 0);
        coinIconRt.anchorMax = new Vector2(0.2f, 1);
        coinIconRt.pivot = new Vector2(0, 0.5f);
        coinIconRt.sizeDelta = new Vector2(0, 0);
        coinIconRt.anchoredPosition = Vector2.zero;

        // 코인 숫자
        GameObject coinTextObj = new GameObject("CoinText");
        coinTextObj.transform.SetParent(coinContainer.transform, false);

        TextMeshProUGUI coinTxt = coinTextObj.AddComponent<TextMeshProUGUI>();
        coinTxt.text = "0";
        coinTxt.fontSize = 14;
        coinTxt.fontStyle = FontStyles.Bold;
        coinTxt.color = new Color(1f, 0.85f, 0.3f, 1f); // 골드 색상
        coinTxt.alignment = TextAlignmentOptions.MidlineRight;

        RectTransform coinTextRt = coinTextObj.GetComponent<RectTransform>();
        coinTextRt.anchorMin = new Vector2(0.2f, 0);
        coinTextRt.anchorMax = new Vector2(1, 1);
        coinTextRt.pivot = new Vector2(1, 0.5f);
        coinTextRt.sizeDelta = new Vector2(0, 0);
        coinTextRt.anchoredPosition = Vector2.zero;

        // [NEW] 한글 폰트 일괄 적용
        TMP_FontAsset krFont = LoadKoreanFont();
        if (krFont != null)
        {
            foreach (var txt in coinContainer.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                txt.font = krFont;
            }
        }

        // 처음엔 숨김
        panel.SetActive(false);

        EditorUtility.SetDirty(panel);
        Debug.Log("Created UserInfoPanel (Rhythm Game Style)");
    }

    static void CreateInGameLoginPanel(Canvas canvas)
    {
        // [FIX] 기존 패널 삭제 (비활성화된 것도 찾아서 제거)
        Transform existing = canvas.transform.Find("InGameLoginPanel");
        if (existing != null)
        {
            DestroyImmediate(existing.gameObject);
        }
        
        // 혹시 모르니 전체 검색해서 또 있으면 삭제 (중복 방지)
        var allPanels = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (var p in allPanels)
        {
            // 씬에 있는 InGameLoginPanel만 타겟
            if (p.name == "InGameLoginPanel" && p.scene.isLoaded)
            {
                DestroyImmediate(p);
            }
        }

        // 1. 배경 오버레이 (전체 화면 어둡게)
        GameObject overlay = new GameObject("InGameLoginPanel");
        overlay.transform.SetParent(canvas.transform, false);

        RectTransform overlayRt = overlay.AddComponent<RectTransform>();
        overlayRt.anchorMin = Vector2.zero;
        overlayRt.anchorMax = Vector2.one;
        overlayRt.sizeDelta = Vector2.zero;
        overlayRt.anchoredPosition = Vector2.zero;

        Image overlayImg = overlay.AddComponent<Image>();
        overlayImg.color = new Color(0, 0, 0, 0.85f);

        // 2. 로그인 패널 (중앙) - 더 크고 게임 스타일
        GameObject loginPanel = new GameObject("LoginPanel");
        loginPanel.transform.SetParent(overlay.transform, false);

        RectTransform panelRt = loginPanel.AddComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.anchoredPosition = Vector2.zero;
        panelRt.sizeDelta = new Vector2(550, 480); // 더 큰 사이즈

        Image panelImg = loginPanel.AddComponent<Image>();
        panelImg.color = new Color(0.12f, 0.15f, 0.22f, 0.98f); // 거의 불투명한 다크 블루

        // 3. 상단 테두리 (시안 글로우)
        GameObject topBorder = new GameObject("TopBorder");
        topBorder.transform.SetParent(loginPanel.transform, false);
        Image topBorderImg = topBorder.AddComponent<Image>();
        topBorderImg.color = new Color(0.0f, 0.8f, 1.0f, 0.8f); // 시안

        RectTransform topBorderRt = topBorder.GetComponent<RectTransform>();
        topBorderRt.anchorMin = new Vector2(0, 1);
        topBorderRt.anchorMax = new Vector2(1, 1);
        topBorderRt.pivot = new Vector2(0.5f, 1);
        topBorderRt.sizeDelta = new Vector2(0, 4);
        topBorderRt.anchoredPosition = Vector2.zero;

        // 4. 타이틀 (심플하고 모던하게)
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(loginPanel.transform, false);

        TextMeshProUGUI titleTxt = titleObj.AddComponent<TextMeshProUGUI>();
        titleTxt.text = "LOGIN";
        titleTxt.fontSize = 36;
        titleTxt.fontStyle = FontStyles.Bold;
        titleTxt.alignment = TextAlignmentOptions.Center;
        titleTxt.color = new Color(0.0f, 0.8f, 1.0f, 1f); // 시안

        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0, 1);
        titleRt.anchorMax = new Vector2(1, 1);
        titleRt.pivot = new Vector2(0.5f, 1);
        titleRt.anchoredPosition = new Vector2(0, -30);
        titleRt.sizeDelta = new Vector2(0, 50);

        // 5. 이메일 라벨
        GameObject emailLabelObj = new GameObject("EmailLabel");
        emailLabelObj.transform.SetParent(loginPanel.transform, false);

        TextMeshProUGUI emailLabelTxt = emailLabelObj.AddComponent<TextMeshProUGUI>();
        emailLabelTxt.text = "EMAIL";
        emailLabelTxt.fontSize = 14;
        emailLabelTxt.fontStyle = FontStyles.Bold;
        emailLabelTxt.alignment = TextAlignmentOptions.Left;
        emailLabelTxt.color = new Color(0.5f, 0.7f, 0.9f, 1f);

        RectTransform emailLabelRt = emailLabelObj.GetComponent<RectTransform>();
        emailLabelRt.anchorMin = new Vector2(0.5f, 1);
        emailLabelRt.anchorMax = new Vector2(0.5f, 1);
        emailLabelRt.pivot = new Vector2(0.5f, 1);
        emailLabelRt.anchoredPosition = new Vector2(0, -100);
        emailLabelRt.sizeDelta = new Vector2(460, 24);

        // 6. 이메일 입력 필드
        GameObject emailInputObj = new GameObject("EmailInput");
        emailInputObj.transform.SetParent(loginPanel.transform, false);

        RectTransform emailInputRt = emailInputObj.AddComponent<RectTransform>();
        emailInputRt.anchorMin = new Vector2(0.5f, 1);
        emailInputRt.anchorMax = new Vector2(0.5f, 1);
        emailInputRt.pivot = new Vector2(0.5f, 1);
        emailInputRt.anchoredPosition = new Vector2(0, -130);
        emailInputRt.sizeDelta = new Vector2(460, 55);

        Image emailInputImg = emailInputObj.AddComponent<Image>();
        emailInputImg.color = new Color(0.08f, 0.1f, 0.15f, 1f); // 더 어두운 입력창

        TMP_InputField emailInput = emailInputObj.AddComponent<TMP_InputField>();
        emailInput.textViewport = emailInputRt;
        emailInput.contentType = TMP_InputField.ContentType.EmailAddress;

        // [FIX] Tab 키로 다음 필드로 이동 지원
        emailInput.navigation = new Navigation
        {
            mode = Navigation.Mode.Automatic
        };

        // 이메일 입력 텍스트
        GameObject emailTextObj = new GameObject("Text");
        emailTextObj.transform.SetParent(emailInputObj.transform, false);

        TextMeshProUGUI emailInputText = emailTextObj.AddComponent<TextMeshProUGUI>();
        emailInputText.fontSize = 20;
        emailInputText.color = Color.white;
        emailInputText.alignment = TextAlignmentOptions.MidlineLeft;
        emailInputText.fontStyle = FontStyles.Normal;

        RectTransform emailTextRt = emailTextObj.GetComponent<RectTransform>();
        emailTextRt.anchorMin = Vector2.zero;
        emailTextRt.anchorMax = Vector2.one;
        emailTextRt.offsetMin = new Vector2(20, 5);
        emailTextRt.offsetMax = new Vector2(-20, -5);

        emailInput.textComponent = emailInputText;

        // 플레이스홀더
        GameObject emailPlaceholderObj = new GameObject("Placeholder");
        emailPlaceholderObj.transform.SetParent(emailInputObj.transform, false);

        TextMeshProUGUI emailPlaceholder = emailPlaceholderObj.AddComponent<TextMeshProUGUI>();
        emailPlaceholder.text = "example@email.com";
        emailPlaceholder.fontSize = 20;
        emailPlaceholder.color = new Color(0.4f, 0.4f, 0.45f, 1f);
        emailPlaceholder.fontStyle = FontStyles.Italic;
        emailPlaceholder.alignment = TextAlignmentOptions.MidlineLeft;

        RectTransform emailPlaceholderRt = emailPlaceholderObj.GetComponent<RectTransform>();
        emailPlaceholderRt.anchorMin = Vector2.zero;
        emailPlaceholderRt.anchorMax = Vector2.one;
        emailPlaceholderRt.offsetMin = new Vector2(20, 5);
        emailPlaceholderRt.offsetMax = new Vector2(-20, -5);

        emailInput.placeholder = emailPlaceholder;

        // 7. 비밀번호 라벨
        GameObject passwordLabelObj = new GameObject("PasswordLabel");
        passwordLabelObj.transform.SetParent(loginPanel.transform, false);

        TextMeshProUGUI passwordLabelTxt = passwordLabelObj.AddComponent<TextMeshProUGUI>();
        passwordLabelTxt.text = "PASSWORD";
        passwordLabelTxt.fontSize = 14;
        passwordLabelTxt.fontStyle = FontStyles.Bold;
        passwordLabelTxt.alignment = TextAlignmentOptions.Left;
        passwordLabelTxt.color = new Color(0.5f, 0.7f, 0.9f, 1f);

        RectTransform passwordLabelRt = passwordLabelObj.GetComponent<RectTransform>();
        passwordLabelRt.anchorMin = new Vector2(0.5f, 1);
        passwordLabelRt.anchorMax = new Vector2(0.5f, 1);
        passwordLabelRt.pivot = new Vector2(0.5f, 1);
        passwordLabelRt.anchoredPosition = new Vector2(0, -200);
        passwordLabelRt.sizeDelta = new Vector2(460, 24);

        // 8. 비밀번호 입력 필드
        GameObject passwordInputObj = new GameObject("PasswordInput");
        passwordInputObj.transform.SetParent(loginPanel.transform, false);

        RectTransform passwordInputRt = passwordInputObj.AddComponent<RectTransform>();
        passwordInputRt.anchorMin = new Vector2(0.5f, 1);
        passwordInputRt.anchorMax = new Vector2(0.5f, 1);
        passwordInputRt.pivot = new Vector2(0.5f, 1);
        passwordInputRt.anchoredPosition = new Vector2(0, -230);
        passwordInputRt.sizeDelta = new Vector2(460, 55);

        Image passwordInputImg = passwordInputObj.AddComponent<Image>();
        passwordInputImg.color = new Color(0.08f, 0.1f, 0.15f, 1f);

        TMP_InputField passwordInput = passwordInputObj.AddComponent<TMP_InputField>();
        passwordInput.textViewport = passwordInputRt;
        passwordInput.contentType = TMP_InputField.ContentType.Password;

        // [FIX] Tab 키 지원
        passwordInput.navigation = new Navigation
        {
            mode = Navigation.Mode.Automatic
        };

        // 비밀번호 입력 텍스트
        GameObject passwordTextObj = new GameObject("Text");
        passwordTextObj.transform.SetParent(passwordInputObj.transform, false);

        TextMeshProUGUI passwordInputText = passwordTextObj.AddComponent<TextMeshProUGUI>();
        passwordInputText.fontSize = 20;
        passwordInputText.color = Color.white;
        passwordInputText.alignment = TextAlignmentOptions.MidlineLeft;
        passwordInputText.fontStyle = FontStyles.Normal;

        RectTransform passwordTextRt = passwordTextObj.GetComponent<RectTransform>();
        passwordTextRt.anchorMin = Vector2.zero;
        passwordTextRt.anchorMax = Vector2.one;
        passwordTextRt.offsetMin = new Vector2(20, 5);
        passwordTextRt.offsetMax = new Vector2(-20, -5);

        passwordInput.textComponent = passwordInputText;

        // 플레이스홀더
        GameObject passwordPlaceholderObj = new GameObject("Placeholder");
        passwordPlaceholderObj.transform.SetParent(passwordInputObj.transform, false);

        TextMeshProUGUI passwordPlaceholder = passwordPlaceholderObj.AddComponent<TextMeshProUGUI>();
        passwordPlaceholder.text = "Enter your password";
        passwordPlaceholder.fontSize = 20;
        passwordPlaceholder.color = new Color(0.4f, 0.4f, 0.45f, 1f);
        passwordPlaceholder.fontStyle = FontStyles.Italic;
        passwordPlaceholder.alignment = TextAlignmentOptions.MidlineLeft;

        RectTransform passwordPlaceholderRt = passwordPlaceholderObj.GetComponent<RectTransform>();
        passwordPlaceholderRt.anchorMin = Vector2.zero;
        passwordPlaceholderRt.anchorMax = Vector2.one;
        passwordPlaceholderRt.offsetMin = new Vector2(20, 5);
        passwordPlaceholderRt.offsetMax = new Vector2(-20, -5);

        passwordInput.placeholder = passwordPlaceholder;

        // [NEW] Tab 키 명시적 네비게이션 설정
        Navigation emailNav = new Navigation();
        emailNav.mode = Navigation.Mode.Explicit;
        emailNav.selectOnDown = passwordInput;
        // emailNav.selectOnTab = passwordInput; // Navigation struct has no selectOnTab properly
        emailInput.navigation = emailNav;

        Navigation passwordNav = new Navigation();
        passwordNav.mode = Navigation.Mode.Explicit;
        passwordNav.selectOnUp = emailInput;
        passwordInput.navigation = passwordNav;

        // 9. 에러 메시지 텍스트
        GameObject errorObj = new GameObject("ErrorText");
        errorObj.transform.SetParent(loginPanel.transform, false);

        TextMeshProUGUI errorTxt = errorObj.AddComponent<TextMeshProUGUI>();
        errorTxt.text = "";
        errorTxt.fontSize = 14;
        errorTxt.alignment = TextAlignmentOptions.Center;
        errorTxt.color = new Color(1f, 0.3f, 0.3f, 1f);

        RectTransform errorRt = errorObj.GetComponent<RectTransform>();
        errorRt.anchorMin = new Vector2(0.5f, 0);
        errorRt.anchorMax = new Vector2(0.5f, 0);
        errorRt.pivot = new Vector2(0.5f, 0);
        errorRt.anchoredPosition = new Vector2(0, 110);
        errorRt.sizeDelta = new Vector2(460, 30);

        // 10. 로그인 버튼 (시안 그라데이션)
        GameObject loginBtnObj = new GameObject("LoginButton");
        loginBtnObj.transform.SetParent(loginPanel.transform, false);

        RectTransform loginBtnRt = loginBtnObj.AddComponent<RectTransform>();
        loginBtnRt.anchorMin = new Vector2(0.5f, 0);
        loginBtnRt.anchorMax = new Vector2(0.5f, 0);
        loginBtnRt.pivot = new Vector2(0.5f, 0);
        loginBtnRt.anchoredPosition = new Vector2(0, 45);
        loginBtnRt.sizeDelta = new Vector2(460, 60);

        Image loginBtnImg = loginBtnObj.AddComponent<Image>();
        loginBtnImg.color = new Color(0.0f, 0.8f, 1.0f, 1f); // 시안

        Button loginBtn = loginBtnObj.AddComponent<Button>();
        ColorBlock loginBtnColors = loginBtn.colors;
        loginBtnColors.normalColor = Color.white;
        loginBtnColors.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 1f);
        loginBtnColors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        loginBtn.colors = loginBtnColors;

        // 로그인 버튼 텍스트
        GameObject loginBtnTextObj = new GameObject("Text");
        loginBtnTextObj.transform.SetParent(loginBtnObj.transform, false);

        TextMeshProUGUI loginBtnTxt = loginBtnTextObj.AddComponent<TextMeshProUGUI>();
        loginBtnTxt.text = "LOG IN";
        loginBtnTxt.fontSize = 24;
        loginBtnTxt.fontStyle = FontStyles.Bold;
        loginBtnTxt.alignment = TextAlignmentOptions.Center;
        loginBtnTxt.color = Color.white;

        RectTransform loginBtnTextRt = loginBtnTextObj.GetComponent<RectTransform>();
        loginBtnTextRt.anchorMin = Vector2.zero;
        loginBtnTextRt.anchorMax = Vector2.one;
        loginBtnTextRt.sizeDelta = Vector2.zero;

        // 11. 닫기 버튼 (우측 상단)
        GameObject closeBtnObj = new GameObject("CloseButton");
        closeBtnObj.transform.SetParent(loginPanel.transform, false);

        RectTransform closeBtnRt = closeBtnObj.AddComponent<RectTransform>();
        closeBtnRt.anchorMin = new Vector2(1, 1);
        closeBtnRt.anchorMax = new Vector2(1, 1);
        closeBtnRt.pivot = new Vector2(1, 1);
        closeBtnRt.anchoredPosition = new Vector2(-10, -10);
        closeBtnRt.sizeDelta = new Vector2(40, 40);

        Image closeBtnImg = closeBtnObj.AddComponent<Image>();
        closeBtnImg.color = new Color(0.8f, 0.2f, 0.2f, 0.9f);

        Button closeBtn = closeBtnObj.AddComponent<Button>();
        ColorBlock closeBtnColors = closeBtn.colors;
        closeBtnColors.normalColor = Color.white;
        closeBtnColors.highlightedColor = new Color(1.3f, 1.3f, 1.3f, 1f);
        closeBtnColors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        closeBtn.colors = closeBtnColors;

        // 닫기 버튼 텍스트
        GameObject closeBtnTextObj = new GameObject("Text");
        closeBtnTextObj.transform.SetParent(closeBtnObj.transform, false);

        TextMeshProUGUI closeBtnTxt = closeBtnTextObj.AddComponent<TextMeshProUGUI>();
        closeBtnTxt.text = "X";
        closeBtnTxt.fontSize = 24;
        closeBtnTxt.fontStyle = FontStyles.Bold;
        closeBtnTxt.alignment = TextAlignmentOptions.Center;
        closeBtnTxt.color = Color.white;

        RectTransform closeBtnTextRt = closeBtnTextObj.GetComponent<RectTransform>();
        closeBtnTextRt.anchorMin = Vector2.zero;
        closeBtnTextRt.anchorMax = Vector2.one;
        closeBtnTextRt.sizeDelta = Vector2.zero;

        // [NEW] 한글 폰트 일괄 적용
        TMP_FontAsset krFont = LoadKoreanFont();
        if (krFont != null)
        {
            foreach (var txt in overlay.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                txt.font = krFont;
            }
        }

        // 처음엔 숨김
        overlay.SetActive(false);

        EditorUtility.SetDirty(overlay);
        Debug.Log("Created InGameLoginPanel with improved design and Tab support");
    }

    static void SetupCanvasScaler(Canvas canvas)
    {
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        
        Debug.Log("Canvas Scalar optimized for 1920x1080 resolution.");
    }
}
