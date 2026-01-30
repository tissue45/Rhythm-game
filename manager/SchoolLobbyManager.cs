using UnityEngine;

using UnityEngine.SceneManagement;

using System.Linq;



public class SchoolLobbyManager : MonoBehaviour

{

    [Header("Song Options")]
    public TMPro.TMP_FontAsset uiFont; // [FIX] Custom Font Support to avoid Missing Material errors

    public Sprite[] albumCovers; // [NEW] User assignable album covers

    public Sprite mainLogo; // [NEW] Main Logo to replace text



    public static SchoolLobbyManager Instance { get; private set; }



    [Header("UI Panels (Optional)")]

    public GameObject songSelectPanel; // [RESTORED]
    public GameObject qrConnectionPanel; // [NEW] QR Connection Panel
    
    // [RESTORED] Additional Panel References
    public GameObject rankingPanel;
    public GameObject customizationPanel;
    public GameObject shopPanel;
    public GameObject profilePanel;
    public GameObject optionsPanel;
    
    // [RESTORED] Original Lobby Manager Reference
    private LobbyManager _originalLobbyManager;

    [Header("UI Managers")]
    public LobbyUIManager uiManager; // [FIX] Added to resolve compile error
    public CoverFlowSnapController snapController; 


    // ... (Existing Variables)

    // [NEW] 배포 환경 URL 설정
    [Header("Deployment Settings")]
    public string deployedFrontendUrl = "https://stepup-rhythm.vercel.app";
    public bool useLocalDevelopment = false; // Inspector에서 설정 가능

    private void PopulateQRConnection(GameObject panel)
    {
        Debug.Log("[Lobby] Populating QR Connection...");

        foreach (Transform child in panel.transform) Destroy(child.gameObject);

        // 1. Background (Gradient-like dark overlay)
        var bgImg = panel.GetComponent<UnityEngine.UI.Image>();
        if (bgImg == null) bgImg = panel.AddComponent<UnityEngine.UI.Image>();
        bgImg.color = new Color(0.05f, 0.05f, 0.1f, 0.95f);

        // 2. Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panel.transform, false);
        var titleTxt = titleObj.AddComponent<TMPro.TextMeshProUGUI>();
        titleTxt.text = "MOBILE CONTROLLER";
        titleTxt.fontSize = 42;
        titleTxt.fontStyle = TMPro.FontStyles.Bold;
        titleTxt.alignment = TMPro.TextAlignmentOptions.Center;
        titleTxt.color = new Color(0f, 0.9f, 1f); // Cyan
        
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0, 0.88f); titleRt.anchorMax = new Vector2(1, 0.98f);
        titleRt.offsetMin = Vector2.zero; titleRt.offsetMax = Vector2.zero;

        // 3. Subtitle (Selected Song)
        GameObject subtitleObj = new GameObject("Subtitle");
        subtitleObj.transform.SetParent(panel.transform, false);
        var subTxt = subtitleObj.AddComponent<TMPro.TextMeshProUGUI>();
        subTxt.text = $"Playing: <color=#FFD700>{_selectedSongTitle}</color>";
        subTxt.fontSize = 24;
        subTxt.alignment = TMPro.TextAlignmentOptions.Center;
        subTxt.color = Color.white;
        
        RectTransform subRt = subtitleObj.GetComponent<RectTransform>();
        subRt.anchorMin = new Vector2(0, 0.82f); subRt.anchorMax = new Vector2(1, 0.88f);
        subRt.offsetMin = Vector2.zero; subRt.offsetMax = Vector2.zero;

        // 4. QR Code Container with border
        GameObject qrContainer = new GameObject("QRContainer");
        qrContainer.transform.SetParent(panel.transform, false);
        var qrContainerImg = qrContainer.AddComponent<UnityEngine.UI.Image>();
        qrContainerImg.color = Color.white;
        
        RectTransform qrContRt = qrContainer.GetComponent<RectTransform>();
        qrContRt.anchorMin = new Vector2(0.5f, 0.5f); qrContRt.anchorMax = new Vector2(0.5f, 0.5f);
        qrContRt.pivot = new Vector2(0.5f, 0.5f);
        qrContRt.sizeDelta = new Vector2(320, 320);
        qrContRt.anchoredPosition = new Vector2(0, 40);

        GameObject qrObj = new GameObject("QRCode");
        qrObj.transform.SetParent(qrContainer.transform, false);
        var qrRawImg = qrObj.AddComponent<UnityEngine.UI.RawImage>();
        qrRawImg.color = Color.white;

        var qrDisp = qrObj.GetComponent<QRDisplay>();
        if (qrDisp == null) qrDisp = qrObj.AddComponent<QRDisplay>();

        // [NEW] 환경에 따른 URL 설정
        string controllerUrl;
        if (useLocalDevelopment)
        {
            // 로컬 개발 환경
            string localIP = GetLocalIPAddress();
            controllerUrl = $"http://{localIP}:5173/controller";
        }
        else
        {
            // 배포 환경 (Vercel)
            controllerUrl = $"{deployedFrontendUrl}/controller";
        }
        
        // QRDisplay에 URL 설정 및 QR 코드 생성
        qrDisp.deployedFrontendUrl = deployedFrontendUrl;
        qrDisp.useLocalDevelopment = useLocalDevelopment;
        qrDisp.SetUrlAndGenerate(controllerUrl);
        
        Debug.Log($"[QR] Generated URL: {controllerUrl}");
        
        RectTransform qrRt = qrObj.GetComponent<RectTransform>();
        qrRt.anchorMin = new Vector2(0.5f, 0.5f); qrRt.anchorMax = new Vector2(0.5f, 0.5f);
        qrRt.pivot = new Vector2(0.5f, 0.5f);
        qrRt.sizeDelta = new Vector2(300, 300);
        qrRt.anchoredPosition = Vector2.zero;

        // 5. Instructions
        GameObject insObj = new GameObject("Instructions");
        insObj.transform.SetParent(panel.transform, false);
        var insTxt = insObj.AddComponent<TMPro.TextMeshProUGUI>();
        insTxt.text = "Scan QR code with your phone\n<size=16><color=#888888>or visit:</color></size>\n<size=18><color=#00BFFF>" + controllerUrl + "</color></size>";
        insTxt.fontSize = 22;
        insTxt.alignment = TMPro.TextAlignmentOptions.Center;
        insTxt.color = Color.white;
        
        RectTransform insRt = insObj.GetComponent<RectTransform>();
        insRt.anchorMin = new Vector2(0.05f, 0.22f); insRt.anchorMax = new Vector2(0.95f, 0.35f);
        insRt.offsetMin = Vector2.zero; insRt.offsetMax = Vector2.zero;

        // 6. GAME START Button (모바일 연결 후 게임 시작)
        GameObject startBtnObj = new GameObject("Btn_MobileStart");
        startBtnObj.transform.SetParent(panel.transform, false);
        var startImg = startBtnObj.AddComponent<UnityEngine.UI.Image>();
        startImg.color = new Color(0f, 0.8f, 0.4f); // Green
        var startBtn = startBtnObj.AddComponent<UnityEngine.UI.Button>();
        
        // Hover effect
        UnityEngine.UI.ColorBlock startCb = startBtn.colors;
        startCb.highlightedColor = new Color(0f, 1f, 0.5f);
        startCb.pressedColor = new Color(0f, 0.6f, 0.3f);
        startBtn.colors = startCb;
        
        startBtn.onClick.AddListener(() => { 
            Debug.Log("[Mobile] Starting game with mobile controller...");
            panel.SetActive(false);
            StartGameLoop(); 
        });
        
        GameObject startTxtObj = new GameObject("Text");
        startTxtObj.transform.SetParent(startBtnObj.transform, false);
        var stTxt = startTxtObj.AddComponent<TMPro.TextMeshProUGUI>();
        stTxt.text = "GAME START"; 
        stTxt.fontSize = 32; 
        stTxt.fontStyle = TMPro.FontStyles.Bold;
        stTxt.color = Color.white;
        stTxt.alignment = TMPro.TextAlignmentOptions.Center;
        
        RectTransform stTxtRt = startTxtObj.GetComponent<RectTransform>();
        stTxtRt.anchorMin = Vector2.zero; stTxtRt.anchorMax = Vector2.one;
        stTxtRt.offsetMin = Vector2.zero; stTxtRt.offsetMax = Vector2.zero;
        
        RectTransform startRt = startBtnObj.GetComponent<RectTransform>();
        startRt.anchorMin = new Vector2(0.3f, 0.1f); startRt.anchorMax = new Vector2(0.7f, 0.2f);
        startRt.offsetMin = Vector2.zero; startRt.offsetMax = Vector2.zero;

        // 7. Close Button (X in corner)
        GameObject closeBtnObj = new GameObject("Btn_Close");
        closeBtnObj.transform.SetParent(panel.transform, false);
        var closeImg = closeBtnObj.AddComponent<UnityEngine.UI.Image>();
        closeImg.color = new Color(0.6f, 0.1f, 0.1f, 0.8f);
        var closeBtn = closeBtnObj.AddComponent<UnityEngine.UI.Button>();
        
        UnityEngine.UI.ColorBlock closeCb = closeBtn.colors;
        closeCb.highlightedColor = new Color(0.8f, 0.2f, 0.2f);
        closeBtn.colors = closeCb;
        
        closeBtn.onClick.AddListener(() => { TogglePanel(panel); });
        
        GameObject closeTxtObj = new GameObject("Text");
        closeTxtObj.transform.SetParent(closeBtnObj.transform, false);
        var clTxt = closeTxtObj.AddComponent<TMPro.TextMeshProUGUI>();
        clTxt.text = "X"; 
        clTxt.fontSize = 28; 
        clTxt.fontStyle = TMPro.FontStyles.Bold;
        clTxt.color = Color.white;
        clTxt.alignment = TMPro.TextAlignmentOptions.Center;
        
        RectTransform clTxtRt = closeTxtObj.GetComponent<RectTransform>();
        clTxtRt.anchorMin = Vector2.zero; clTxtRt.anchorMax = Vector2.one;
        clTxtRt.offsetMin = Vector2.zero; clTxtRt.offsetMax = Vector2.zero;
        
        RectTransform closeRt = closeBtnObj.GetComponent<RectTransform>();
        closeRt.anchorMin = new Vector2(0.92f, 0.92f); closeRt.anchorMax = new Vector2(0.98f, 0.98f);
        closeRt.offsetMin = Vector2.zero; closeRt.offsetMax = Vector2.zero;

        // 8. Back Button (돌아가기)
        GameObject backBtnObj = new GameObject("Btn_Back");
        backBtnObj.transform.SetParent(panel.transform, false);
        var backImg = backBtnObj.AddComponent<UnityEngine.UI.Image>();
        backImg.color = new Color(0.3f, 0.3f, 0.4f, 0.9f);
        var backBtn = backBtnObj.AddComponent<UnityEngine.UI.Button>();
        backBtn.onClick.AddListener(() => { 
            panel.SetActive(false);
            if (songSelectPanel != null) songSelectPanel.SetActive(true);
        });
        
        GameObject backTxtObj = new GameObject("Text");
        backTxtObj.transform.SetParent(backBtnObj.transform, false);
        var bkTxt = backTxtObj.AddComponent<TMPro.TextMeshProUGUI>();
        bkTxt.text = "BACK"; 
        bkTxt.fontSize = 20; 
        bkTxt.color = Color.white;
        bkTxt.alignment = TMPro.TextAlignmentOptions.Center;
        
        RectTransform bkTxtRt = backTxtObj.GetComponent<RectTransform>();
        bkTxtRt.anchorMin = Vector2.zero; bkTxtRt.anchorMax = Vector2.one;
        bkTxtRt.offsetMin = Vector2.zero; bkTxtRt.offsetMax = Vector2.zero;
        
        RectTransform backRt = backBtnObj.GetComponent<RectTransform>();
        backRt.anchorMin = new Vector2(0.02f, 0.92f); backRt.anchorMax = new Vector2(0.12f, 0.98f);
        backRt.offsetMin = Vector2.zero; backRt.offsetMax = Vector2.zero;
    }

    // Helper to get Local IP
    public string GetLocalIPAddress()
    {
        var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        return "127.0.0.1";
    }

    // ===== PANEL POPULATION METHODS =====



    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        // [ENABLED] LoginManager 활성화 - 인게임 로그인 기능 사용
        // LoginManager는 이제 인게임 패널로 작동하므로 비활성화하지 않음
        
        // [FIX] EventSystem 확인
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
             GameObject eventSystem = new GameObject("EventSystem");
             eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
             eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
             Debug.Log("[SchoolLobbyManager] Created EventSystem.");
        }

        // [FIX] Ensure Font is loaded
        if (uiFont == null)
        {
            uiFont = Resources.Load<TMPro.TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (uiFont == null)
            {
                // Fallback: Find in memory (including packages)
                var fonts = Resources.FindObjectsOfTypeAll<TMPro.TMP_FontAsset>();
                foreach (var f in fonts)
                {
                    if (f.name == "LiberationSans SDF")
                    {
                        uiFont = f;
                        break;
                    }
                }
            }
        }
    }

    private bool wasLoggedIn = false;

    // [FIX] 중복 에러 방지를 위해 고유한 이름 사용
    private void MonitorLoginStatus()
    {
        // [FIX] 로그인 상태 변경 감지 (실시간 동기화)
        if (NetworkManager.Instance != null)
        {
            if (NetworkManager.Instance.IsLoggedIn && !wasLoggedIn)
            {
                Debug.Log("Login Detected! Updating Lobby UI...");
                wasLoggedIn = true;
                UpdateProfileUI();
            }
            else if (!NetworkManager.Instance.IsLoggedIn && wasLoggedIn)
            {
                Debug.Log("Logout Detected! Updating Lobby UI...");
                wasLoggedIn = false;
                UpdateProfileUI();
            }
        }
    }

    // [FIX] BindButtonEvents continues below...






    private void BindButtonEvents()

    {

        // [FIX] ?ë¦?ë¶ì¼?ë¬¸ì  ?ê²?(Btn_GameStart vs Btn_Start)

        BindButton("Btn_GameStart", OnGameStartClick);

        BindButton("Btn_Start", OnGameStartClick); // ?? ë¡ê·¸??????ë¦?
        BindButton("StartButton", OnGameStartClick); // ???ëª°ë¼??ì¶?



        BindButton("Btn_Ranking", OnRankingClick);
        BindButton("Btn_Shop", OnShopClick);
        // BindButton("Btn_Profile", OnProfileClick); // [REMOVED] User Request
        // BindButton("Btn_Option", OnOptionsClick); // [REMOVED] Option Button
        // BindButton("Btn_Exit", () => Application.Quit()); // [REMOVED] User Request

        // [CRITICAL] Force delete ALL EXIT buttons from entire scene
        string[] exitNames = { "Btn_Exit", "EXIT", "ExitButton", "QuitButton", "Quit" };
        foreach (string exitName in exitNames)
        {
            GameObject exitObj = GameObject.Find(exitName);
            if (exitObj != null)
            {
                Debug.Log($"[SchoolLobbyManager] Force deleting: {exitName}");
                Destroy(exitObj);
            }
        }

        // [CRITICAL] Clean LOGIN button - remove neon effects
        GameObject loginBtn = GameObject.Find("LOGIN");
        if (loginBtn != null)
        {
            // Remove RhythmButtonStyle (neon effect)
            var rhythmStyle = loginBtn.GetComponent<RhythmButtonStyle>();
            if (rhythmStyle != null)
            {
                Debug.Log("[SchoolLobbyManager] Removing RhythmButtonStyle from LOGIN button");
                Destroy(rhythmStyle);
            }

            // Remove all Shadow and Outline effects
            var shadows = loginBtn.GetComponents<UnityEngine.UI.Shadow>();
            foreach (var s in shadows) Destroy(s);

            var outlines = loginBtn.GetComponents<UnityEngine.UI.Outline>();
            foreach (var o in outlines) Destroy(o);

            // Clean text effects too
            var textObj = loginBtn.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (textObj != null)
            {
                var textOutlines = textObj.GetComponents<UnityEngine.UI.Outline>();
                foreach (var o in textOutlines) Destroy(o);
            }

            // Reset image to clean cyan color
            var img = loginBtn.GetComponent<UnityEngine.UI.Image>();
            if (img != null)
            {
                img.color = new Color(0.0f, 0.8f, 1.0f, 1f);
            }
        }

        // [NEW] Hide Profile Button if it exists
        GameObject profileBtn = GameObject.Find("Btn_Profile");
        if (profileBtn != null) profileBtn.SetActive(false);

    }



    // ... (Start Method) ...







    private void PopulateOptions(GameObject panel)

    {

        Debug.Log("[Lobby] Populating Options UI...");



        foreach (Transform child in panel.transform) Destroy(child.gameObject);



        // 1. Background

        var bgImg = panel.GetComponent<UnityEngine.UI.Image>();

        if (bgImg == null) bgImg = panel.AddComponent<UnityEngine.UI.Image>();

        bgImg.color = new Color(0, 0, 0, 0.9f);



        // 2. Title

        GameObject titleObj = new GameObject("Title");

        titleObj.transform.SetParent(panel.transform, false);

        var titleTxt = titleObj.AddComponent<TMPro.TextMeshProUGUI>();

        titleTxt.text = "OPTIONS";

        titleTxt.fontSize = 48;

        titleTxt.fontStyle = TMPro.FontStyles.Bold;

        titleTxt.alignment = TMPro.TextAlignmentOptions.Center;

        titleTxt.color = Color.white;

        

        RectTransform titleRt = titleObj.GetComponent<RectTransform>();

        titleRt.anchorMin = new Vector2(0, 0.8f); titleRt.anchorMax = new Vector2(1, 1);

        titleRt.offsetMin = Vector2.zero; titleRt.offsetMax = Vector2.zero;



        // 3. Coming Soon Text

        GameObject msgObj = new GameObject("Message");

        msgObj.transform.SetParent(panel.transform, false);

        var msgTxt = msgObj.AddComponent<TMPro.TextMeshProUGUI>();

        msgTxt.text = "Work In Progress...\n(Coming Soon)";

        msgTxt.fontSize = 36;

        msgTxt.alignment = TMPro.TextAlignmentOptions.Center;

        msgTxt.color = Color.gray;

        

        RectTransform msgRt = msgObj.GetComponent<RectTransform>();

        msgRt.anchorMin = new Vector2(0, 0.2f); msgRt.anchorMax = new Vector2(1, 0.8f);

        msgRt.offsetMin = Vector2.zero; msgRt.offsetMax = Vector2.zero;



        // 4. Close Button

        GameObject closeBtnObj = new GameObject("Btn_Close");

        closeBtnObj.transform.SetParent(panel.transform, false);

        var closeImg = closeBtnObj.AddComponent<UnityEngine.UI.Image>();

        closeImg.color = new Color(0.8f, 0.2f, 0.2f); // Red

        var closeBtn = closeBtnObj.AddComponent<UnityEngine.UI.Button>();

        closeBtn.onClick.AddListener(() => { TogglePanel(panel); });

        

        RectTransform closeBtnRt = closeBtnObj.GetComponent<RectTransform>();

        closeBtnRt.anchorMin = new Vector2(0.92f, 0.92f); closeBtnRt.anchorMax = new Vector2(0.98f, 0.98f);

        closeBtnRt.offsetMin = Vector2.zero; closeBtnRt.offsetMax = Vector2.zero;



        GameObject closeTxtObj = new GameObject("Text");

        closeTxtObj.transform.SetParent(closeBtnObj.transform, false);

        var clTxt = closeTxtObj.AddComponent<TMPro.TextMeshProUGUI>();

        clTxt.text = "X";

        clTxt.fontSize = 28;

        clTxt.alignment = TMPro.TextAlignmentOptions.Center;

        clTxt.color = Color.white;

        

        var clRt = closeTxtObj.AddComponent<RectTransform>();

        clRt.anchorMin = Vector2.zero; clRt.anchorMax = Vector2.one;

        clRt.offsetMin = Vector2.zero; clRt.offsetMax = Vector2.zero;

        

        // Ensure blocking raycasts

        var graphic = panel.GetComponent<UnityEngine.UI.Graphic>();

        if(graphic != null) graphic.raycastTarget = true;

    }

    private System.Collections.IEnumerator Start()
    {
        // [FIX] 로그인 상태 모니터링 시작 (0.5초마다 체크)
        InvokeRepeating("MonitorLoginStatus", 1f, 0.5f);

        _originalLobbyManager = FindObjectOfType<LobbyManager>();

        // [NEW] 'power-button.png' 아이콘 로드 및 적용
        Sprite powerIcon = Resources.Load<Sprite>("power-button");
        if (powerIcon != null)
        {
            GameObject startBtn = GameObject.Find("Btn_Start");
            // 버튼 이름이 다를 수도 있으니 여러 후보 확인
            if (startBtn == null) startBtn = GameObject.Find("StartButton");
            if (startBtn == null) startBtn = GameObject.Find("Btn_GameStart");
            
            if (startBtn != null)
            {
                var style = startBtn.GetComponent<RhythmButtonStyle>();
                if (style != null)
                {
                    style.iconSprite = powerIcon;
                    style.InitializeStyle(); // 스타일 재적용
                    Debug.Log("Applied Power-Button Icon!");
                }
            }
            else
            {
                Debug.LogWarning("Start Button not found to apply icon.");
            }
        }
        else
        {
             Debug.LogWarning("Could not load 'power-button' sprite from Resources.");
        }

        // [NEW] Subscribe to Login Event
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.OnConnected += UpdateProfileUI;
            // Immediate check
            if (NetworkManager.Instance.IsLoggedIn) UpdateProfileUI();
        }

        // Wait for other scripts (LobbyUIBuilder) to finish

        yield return new WaitForEndOfFrame();



        // [NEW] User Request: Remove Title Text entirely

        var legacyTitle = GameObject.Find("TitleText");

        if (legacyTitle != null) 

        {

            legacyTitle.SetActive(false);

            Destroy(legacyTitle);

        }

        

        // Also check for any TMP with name 'Title' or similar just in case

        var allTitles = Resources.FindObjectsOfTypeAll<TMPro.TextMeshProUGUI>()

                .Where(t => (t.name == "TitleText" || t.name == "Title") && t.gameObject.scene.isLoaded).ToArray();

        foreach(var t in allTitles)

        {

            if (t != null && t.gameObject != null)

            {

                t.gameObject.SetActive(false);

                Destroy(t.gameObject);

            }

        }





        // [FIX] Stuck Ghost Button Cleanup (Re-applied)
        var ghostQuit = GameObject.Find("QuitButton");
        if (ghostQuit != null) 
        {
            ghostQuit.SetActive(false);
            Destroy(ghostQuit);
            Debug.Log("[SchoolLobbyManager] Destroyed ghost 'QuitButton'");
        }

        var ghostStart = GameObject.Find("StartButton");
        if (ghostStart != null)
        {
            // RhythmMenuButton이 없는 옛날 버튼만 삭제
            if (ghostStart.GetComponent<RhythmMenuButton>() == null)
            {
                    ghostStart.SetActive(false);
                    Destroy(ghostStart);
                    Debug.Log("[SchoolLobbyManager] Destroyed ghost 'StartButton'");
            }
        }

        // [FIX] Create Logo Explicitly (Since we destroyed TitleText)
        if (mainLogo != null)

        {

            GameObject logoObj = GameObject.Find("LobbyLogo");

            if (logoObj == null) 

            {

                logoObj = new GameObject("LobbyLogo");

                GameObject logoCanvas = GameObject.Find("Canvas");

                if (logoCanvas != null) logoObj.transform.SetParent(logoCanvas.transform, false);

            }



            UnityEngine.UI.Image logoImg = logoObj.GetComponent<UnityEngine.UI.Image>();

            if (logoImg == null) logoImg = logoObj.AddComponent<UnityEngine.UI.Image>();

            

            logoImg.sprite = mainLogo;

            logoImg.preserveAspect = true;

            logoImg.SetNativeSize();

            

            // [FIX] Resize and Position (CENTER of screen as requested)

            RectTransform logoRect = logoImg.rectTransform;

            logoRect.anchorMin = new Vector2(0.5f, 0.5f); 

            logoRect.anchorMax = new Vector2(0.5f, 0.5f);

            logoRect.pivot = new Vector2(0.5f, 0.5f);

            logoRect.anchoredPosition = new Vector2(0, 0); 

            logoRect.localScale = Vector3.one * 1.5f; // Slightly larger for center focus



            // Add Hover Animation (Replaced Pulse)

            LogoPulse hover = logoObj.GetComponent<LogoPulse>();

            if (hover == null) hover = logoObj.AddComponent<LogoPulse>();

            hover.hoverSpeed = 1.5f; // Slow and gentle

            hover.hoverRange = 15f; // Subtle movement



            Debug.Log("[SchoolLobbyManager] Created 'LobbyLogo' with Hover Effect.");

            

            // [NEW] Setup Dramatic Lighting

            SetupLobbyLighting();

            

            // [NEW] Auto Setup Post Processing (Bloom)

            if (gameObject.GetComponent<AutoSetupPostProcessing>() == null)

            {

               gameObject.AddComponent<AutoSetupPostProcessing>();

            }

            

            // [NEW] Create Version Display

            CreateVersionDisplay();

        }

        // [FIX] Always bind button events regardless of mainLogo
        // This ensures buttons work even if mainLogo is not assigned
        InitializeSongSelectPanelIfNeeded();
        BindButtonEvents();
        // [FIX] Robust Panel Lookup (Fallback)
        if (rankingPanel == null) 
        {
            rankingPanel = GameObject.Find("RankingPanel");
            if (rankingPanel != null) Debug.Log("[Lobby] RankingPanel found by name.");
        }
        if (shopPanel == null) 
        {
            shopPanel = GameObject.Find("StepUpShopPanel"); // FinalFix creates this name
            if (shopPanel == null) shopPanel = GameObject.Find("PremiumShop");
            if (shopPanel != null) Debug.Log("[Lobby] ShopPanel found by name.");
        }

        Debug.Log("[SchoolLobbyManager] Button events bound in Start()");
    }

    private void InitializeSongSelectPanelIfNeeded()
    {
        // Skip if already initialized
        if (songSelectPanel != null) return;

        // Try to find existing panel
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj != null)
        {
            Transform panel2Trans = canvasObj.transform.Find("SongSelectPanel2");
            if (panel2Trans != null)
            {
                songSelectPanel = panel2Trans.gameObject;
                Debug.Log("[SchoolLobbyManager] Found 'SongSelectPanel2' in InitializeSongSelectPanelIfNeeded.");
                return;
            }
        }

        GameObject activePanel = GameObject.Find("SongSelectPanel2");
        if (activePanel != null)
        {
            songSelectPanel = activePanel;
            return;
        }

        songSelectPanel = GameObject.Find("SongSelectPanel");
        if (songSelectPanel == null)
        {
            songSelectPanel = GameObject.Find("SongSelectionPanel");
        }

        // If still null, create a basic one
        if (songSelectPanel == null)
        {
            Debug.Log("[SchoolLobbyManager] Creating SongSelectPanel in InitializeSongSelectPanelIfNeeded...");
            GameObject canvas = GameObject.Find("Canvas");
            if (canvas == null)
            {
                canvas = new GameObject("Canvas");
                canvas.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }

            songSelectPanel = new GameObject("SongSelectPanel");
            songSelectPanel.transform.SetParent(canvas.transform, false);

            var img = songSelectPanel.AddComponent<UnityEngine.UI.Image>();
            img.color = new Color(0, 0, 0, 0.85f);

            RectTransform rt = songSelectPanel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            songSelectPanel.SetActive(false);
        }
    }



    private void SetupLobbyLighting()

    {

        // 1. Rim Light (Backlight)

        GameObject rimLightObj = GameObject.Find("LobbyRimLight");

        if (rimLightObj == null)

        {

            rimLightObj = new GameObject("LobbyRimLight");

            Light l = rimLightObj.AddComponent<Light>();

            l.type = LightType.Directional;

            l.color = new Color(0.4f, 0.6f, 1f); // Cyan/Blue Rim

            l.intensity = 1.5f;

            

            // Rotate to shine from behind/side

            rimLightObj.transform.rotation = Quaternion.Euler(0, 150, 0);

        }



        // 2. Main Key Light (Warm)

        GameObject keyLight = GameObject.Find("Directional Light");

        if (keyLight != null)

        {

            Light l = keyLight.GetComponent<Light>();

            if (l != null)

            {

                l.color = new Color(1f, 0.95f, 0.8f); // Warm Sunlight

                l.intensity = 1.2f;

            }

        }

        

        Debug.Log("[SchoolLobbyManager] Setup Dramatic Lighting (Rim + Warm Key).");



        // ... (SongSelectPanel Logic) ...

        // [FIX] Song Select Panel Priority

        GameObject canvasObj = GameObject.Find("Canvas");

        if (canvasObj != null)

        {

            Transform panel2Trans = canvasObj.transform.Find("SongSelectPanel2");

            if (panel2Trans != null)

            {

                songSelectPanel = panel2Trans.gameObject;

                Debug.Log("[SchoolLobbyManager] Found inactive 'SongSelectPanel2', using it as priority.");

            }

        }

        

        if (songSelectPanel == null || songSelectPanel.name != "SongSelectPanel2")

        {

            GameObject activeSync = GameObject.Find("SongSelectPanel2");

            if (activeSync != null) songSelectPanel = activeSync;

        }



        if (songSelectPanel == null)

        {

            songSelectPanel = GameObject.Find("SongSelectPanel");

            if (songSelectPanel == null) songSelectPanel = GameObject.Find("SongSelectionPanel");

        }



        // Cleanup Old Panels

        var oldPanels = FindObjectsOfType<Transform>().Where(t => t.name.Contains("SongSelectPanel") || t.name.Contains("SongSelection"));

        foreach(var t in oldPanels)

        {

            if (t.name == "Canvas") continue;

            if (t == null || t.gameObject == null) continue;

            DestroyImmediate(t.gameObject);

        }



        var roots = SceneManager.GetActiveScene().GetRootGameObjects();

        foreach (var r in roots)

        {

            if (r.name == "SongSelectPanel2") DestroyImmediate(r);

        }

         

        Debug.Log("[SchoolLobbyManager] Destroyed ALL old Song Select Panels.");

        Debug.Log("[SchoolLobbyManager] Creating FRESH SongSelectPanel...");

             

        GameObject canvas = GameObject.Find("Canvas");

        if (canvas == null)

        {

            canvas = new GameObject("Canvas");

            canvas.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

            canvas.AddComponent<UnityEngine.UI.CanvasScaler>();

            canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        }

             

        songSelectPanel = new GameObject("SongSelectPanel");

        songSelectPanel.transform.SetParent(canvas.transform, false);

        // [NEW] Create QR Connection Panel
        qrConnectionPanel = new GameObject("QRConnectionPanel");
        qrConnectionPanel.transform.SetParent(canvas.transform, false);
        var qrImg = qrConnectionPanel.AddComponent<UnityEngine.UI.Image>();
        qrImg.color = new Color(0,0,0,0.95f);
        RectTransform qrRt = qrConnectionPanel.GetComponent<RectTransform>();
        qrRt.anchorMin = Vector2.zero; qrRt.anchorMax = Vector2.one;
        qrRt.offsetMin = Vector2.zero; qrRt.offsetMax = Vector2.zero;
        qrConnectionPanel.SetActive(false); // Init Hidden

             

        var img = songSelectPanel.AddComponent<UnityEngine.UI.Image>();

        img.color = new Color(0,0,0,0.85f);

             

        RectTransform rt = songSelectPanel.GetComponent<RectTransform>();

        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;

        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

             

        // Close Button

        GameObject closeBtn = new GameObject("CloseButton");

        closeBtn.transform.SetParent(songSelectPanel.transform, false);

        var btnImg = closeBtn.AddComponent<UnityEngine.UI.Image>();

        btnImg.color = new Color(0, 0, 0, 0); // [FIX] 투명하게 (사용자 요청)

        var btn = closeBtn.AddComponent<UnityEngine.UI.Button>();

        var txtObj = new GameObject("Text (TMP)");

        txtObj.transform.SetParent(closeBtn.transform, false);

        var txt = txtObj.AddComponent<TMPro.TextMeshProUGUI>();

        txt.text = "CLOSE";

        txt.alignment = TMPro.TextAlignmentOptions.Center;

        txt.fontSize = 30;

        txt.color = Color.white;

             

        RectTransform btnRt = closeBtn.GetComponent<RectTransform>();

        btnRt.anchorMin = new Vector2(0.95f, 0.95f);

        btnRt.anchorMax = new Vector2(0.95f, 0.95f);

        btnRt.sizeDelta = new Vector2(150, 60);

        btnRt.anchoredPosition = new Vector2(-10, -10);



        // Start Button

        GameObject startBtnObj = new GameObject("StartButton");

        startBtnObj.transform.SetParent(songSelectPanel.transform, false);

        var startImg = startBtnObj.AddComponent<UnityEngine.UI.Image>();

        startImg.color = Color.green;

        var startBtn = startBtnObj.AddComponent<UnityEngine.UI.Button>();

        

        var startTxtObj = new GameObject("Text (TMP)");

        startTxtObj.transform.SetParent(startBtnObj.transform, false);

        var startTxt = startTxtObj.AddComponent<TMPro.TextMeshProUGUI>();

        startTxt.text = "START GAME";

        startTxt.alignment = TMPro.TextAlignmentOptions.Center;

        startTxt.fontSize = 40;

        startTxt.color = Color.black;

        

        RectTransform startRt = startBtnObj.GetComponent<RectTransform>();

        startRt.anchorMin = new Vector2(0.5f, 0.1f); startRt.anchorMax = new Vector2(0.5f, 0.1f);

        startRt.pivot = new Vector2(0.5f, 0.5f);

        startRt.sizeDelta = new Vector2(250, 80);

        startRt.anchoredPosition = new Vector2(0, 50);



        songSelectPanel.SetActive(false);



        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)

        {

            GameObject eventSystem = new GameObject("EventSystem");

            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();

            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            Debug.Log("[SchoolLobbyManager] Missing EventSystem created.");

        }



        StartCoroutine(SeekCharacterAnimations());

        RemoveUIBlockers();

        

        // [FIX] Bind Buttons LAST to ensure everything is ready

        BindButtonEvents();

        

        // [NEW] Create Version Display
        // CreateVersionDisplay(); // [DISABLED] Encoding Issue
        CreateVersionDisplaySafe();

    }



    // Helper for recursive find

    private Transform FindRecursive(Transform parent, string name)

    {

        foreach (Transform child in parent)

        {

            if (child.name == name) return child;

            var result = FindRecursive(child, name);

            if (result != null) return result;

        }

        return null;

    }



    private void CreateVersionDisplay()

    {

        GameObject canvas = GameObject.Find("Canvas");

        if (canvas == null) return;



        // Check if version display already exists

        if (GameObject.Find("VersionDisplay") != null) return;



        GameObject versionObj = new GameObject("VersionDisplay");

        versionObj.transform.SetParent(canvas.transform, false);



        var versionText = versionObj.AddComponent<TMPro.TextMeshProUGUI>();

        versionText.text = "v1.1.0 | Build: 6859ebf | 2026-01-13\n<size=10>?µ Audio Separation Fix Applied</size>";

        versionText.fontSize = 14;

        versionText.fontStyle = TMPro.FontStyles.Normal;

        versionText.alignment = TMPro.TextAlignmentOptions.Center;

        versionText.color = new Color(0.5f, 0.5f, 0.5f, 0.8f); // Semi-transparent gray

        versionText.raycastTarget = false; // Don't block clicks



        RectTransform versionRt = versionObj.GetComponent<RectTransform>();

        versionRt.anchorMin = new Vector2(0.5f, 0f); // Bottom center

        versionRt.anchorMax = new Vector2(0.5f, 0f);

        versionRt.pivot = new Vector2(0.5f, 0f);

        versionRt.anchoredPosition = new Vector2(0, 10); // 10px from bottom

        versionRt.sizeDelta = new Vector2(400, 50);



        Debug.Log("[SchoolLobbyManager] Created Version Display at bottom of screen.");

    }

    private void CreateVersionDisplaySafe()
    {
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null) return;
        if (GameObject.Find("VersionDisplay") != null) return;

        GameObject versionObj = new GameObject("VersionDisplay");
        versionObj.transform.SetParent(canvas.transform, false);

        var versionText = versionObj.AddComponent<TMPro.TextMeshProUGUI>();
        versionText.text = "Version: v1.1.0 | Build: 6859ebf | Updated: 2026-01-21\n<size=10>Backend Integration Applied</size>";
        versionText.fontSize = 14;
        versionText.fontStyle = TMPro.FontStyles.Normal;
        versionText.alignment = TMPro.TextAlignmentOptions.Center;
        versionText.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
        versionText.raycastTarget = false;

        RectTransform versionRt = versionObj.GetComponent<RectTransform>();
        versionRt.anchorMin = new Vector2(0.5f, 0f); versionRt.anchorMax = new Vector2(0.5f, 0f);
        versionRt.pivot = new Vector2(0.5f, 0f);
        versionRt.anchoredPosition = new Vector2(0, 10);
        versionRt.sizeDelta = new Vector2(400, 50);

        Debug.Log("[SchoolLobbyManager] Created Version Display (Safe).");
    }

    // [REMOVED] ApplyButtonStyle 메서드 - FixAllButtons가 모든 스타일을 처리하므로 더 이상 필요없음
    // private void ApplyButtonStyle(GameObject btnObj)
    // {
    //     if (btnObj == null) return;
    //
    //     // Remove generic hover if exists
    //     var legacy = btnObj.GetComponent<SimpleButtonHover>();
    //     if (legacy != null) Destroy(legacy);
    //
    //     // Add Premium Style if missing
    //     if (btnObj.GetComponent<RhythmButtonStyle>() == null)
    //     {
    //         btnObj.AddComponent<RhythmButtonStyle>();
    //     }
    // }

    // [NEW] Footer UI (Exit Button)
    // CreateFooterUI() 메서드 삭제됨 - 더 이상 호출되지 않음
    // AutoSetupUI.cs와 FixAllButtons.cs가 모든 UI 정리 및 생성을 처리함

    // ===============================================
    // 로그인/회원가입 UI 생성 메서드 삭제됨
    // LoginManager와 FixAllButtons가 모든 로그인 UI를 처리함
    // ===============================================

    private void BindButtonWithFallback(string primaryName, string secondaryName, UnityEngine.Events.UnityAction action)
    {
        BindButton(primaryName, action);
        if (GameObject.Find(primaryName) == null)
        {
            BindButton(secondaryName, action);
        }
    }

    private void BindButton(string btnName, UnityEngine.Events.UnityAction action)

    {

        // 1. Try GameObject.Find (Active only)

        GameObject btnObj = GameObject.Find(btnName);



        // 2. Try Recursive Search under Canvas (Finds inactive/hidden)

        if (btnObj == null)

        {

            GameObject canvas = GameObject.Find("Canvas");

            if (canvas != null)

            {

                Transform t = FindRecursive(canvas.transform, btnName);

                if (t != null) btnObj = t.gameObject;

            }

        }



        if (btnObj != null)
        {
            // [FIX] Ensure button Image receives clicks and children don't block
            var btnImage = btnObj.GetComponent<UnityEngine.UI.Image>();
            if (btnImage != null)
            {
                btnImage.raycastTarget = true;
            }

            // Disable raycast on all child Images (Shadow, Glow, etc.)
            foreach (Transform child in btnObj.transform)
            {
                var childImage = child.GetComponent<UnityEngine.UI.Image>();
                if (childImage != null)
                {
                    childImage.raycastTarget = false;
                }
                // Also disable on TMP text
                var childTMP = child.GetComponent<TMPro.TextMeshProUGUI>();
                if (childTMP != null)
                {
                    childTMP.raycastTarget = false;
                }
            }

            UnityEngine.UI.Button btn = btnObj.GetComponent<UnityEngine.UI.Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(action);
                Debug.Log($"[SchoolLobbyManager] Successfully Bound event to {btnName}");
            }
            else
            {
                Debug.LogWarning($"[SchoolLobbyManager] Found {btnName} but it has NO Button component!");
            }
        }

        else

        {

            Debug.LogWarning($"[SchoolLobbyManager] Could NOT find button: {btnName}");

        }

    }



    private void RemoveUIBlockers()

    {

        // 1. TitleText ì²ë¦¬

        GameObject titleText = GameObject.Find("TitleText");

        if (titleText != null)

        {

            var g = titleText.GetComponent<UnityEngine.UI.Graphic>();

            if (g != null) g.raycastTarget = false;

        }



        // 2. ëª¨ë  ????TMP, Legacy Text)??Raycast ?ê¸?(Inactive ???

#if UNITY_2020_1_OR_NEWER

        var allTmpTexts = FindObjectsOfType<TMPro.TextMeshProUGUI>(true);

#else

        var allTmpTexts = FindObjectsOfType<TMPro.TextMeshProUGUI>();

#endif

        foreach (var txt in allTmpTexts)

        {

            txt.raycastTarget = false;

        }



#if UNITY_2020_1_OR_NEWER

        var allLegacyTexts = FindObjectsOfType<UnityEngine.UI.Text>(true);

#else

        var allLegacyTexts = FindObjectsOfType<UnityEngine.UI.Text>();

#endif

        foreach (var txt in allLegacyTexts)

        {

            txt.raycastTarget = false;

        }



        Debug.Log("[SchoolLobbyManager] Disabled Raycast on ALL Texts (including inactive) to prevent blocking.");

    }



    private void Update()
    {
        // Debug logic removed for optimization
    }



    private System.Collections.IEnumerator SeekCharacterAnimations()

    {

        // [FIX] Main ???ë§?????????
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Main") yield break;



        yield return new WaitForSeconds(0.5f);



        string[] charNames = { "minA_1", "Minwoo_1" };

        foreach (string name in charNames)

        {

            GameObject obj = GameObject.Find(name);

            if (obj != null)

            {

                Animator anim = obj.GetComponentInChildren<Animator>();

                

                // [FIX] Animatorê°? ??????ì¶??ì£?

                if (anim == null)

                {

                    Debug.LogWarning($"[SchoolLobbyManager] No Animator found on {name}. Adding one automatically.");

                    anim = obj.AddComponent<Animator>();

                    // Avatar??Controllerê°? ????T-Pose??????? ???ì»´í¬????ì¡´ì¬?ê²???

                }



                if (anim != null)

                {

                    // Animatorê°? êº¼ì ¸???ì¼ì£¼?

                    if (!anim.enabled) anim.enabled = true;

                    if (!anim.gameObject.activeInHierarchy) anim.gameObject.SetActive(true);

                    

                    // Controller ???
                    if (anim.runtimeAnimatorController == null)

                    {

                        Debug.LogWarning($"[SchoolLobbyManager] {name} found but NO Controller assigned!");

                        // ?ê¸??????????ì¤???????(Resource ë¡ë ??

                    }

                }



                if (anim != null)

                {

                    // ??????ì¤ì¸ ????ë³?ê°????

                    AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);

                    

                    Debug.Log($"[SchoolLobbyManager] Found Animator on {anim.gameObject.name}. State Length: {state.length}, Loop: {state.loop}");



                    if (state.length > 0)

                    {

                        float offset = 20f;

                        // 0~1 ?????ê·????ê°?Normalized Time)?ë¡?ë³???

                        float normalizedTime = (offset % state.length) / state.length;

                        

                        // ê°ì ?????ì¹????
                        anim.Play(state.fullPathHash, 0, normalizedTime);

                        

                        // ???Playê°? ??ë¨¹í ê²½ì°??ë¹í´ Update???ì¶?
                        anim.Update(0f);

                        

                        Debug.Log($"[SchoolLobbyManager] {name} seeked to {normalizedTime} (20s)");

                    }

                    else

                    {

                        // ê¸¸ì´ê°? 0?ë©??ë§????ê? ?ì§?Transition ì¤ì´ê±°ë ì´ê¸°?????

                        // ê°ì ?"Base Layer.Motion" ê°? ?ë¦?ë¡???????ë³??????

                        // MMD4Mecanim? ë³´íµ ê¸°ë³¸ ??????????

                        anim.Play(0, -1, 0.2f); // 20% ì§????ê°ì  ??????(???

                        Debug.LogWarning($"[SchoolLobbyManager] {name} State length is 0. Tried forcing normalized time 0.2");

                    }

                }

                else

                {

                    Debug.LogWarning($"[SchoolLobbyManager] Found {name} but NO Animator found in children!");

                    // ????ë¸???ë¦¬ì¤??ì¶ë ¥

                    foreach (Transform t in obj.transform)

                    {

                        Debug.Log($"   Child: {t.name}");

                    }

                }

            }

            else

            {

                Debug.LogWarning($"[SchoolLobbyManager] Could NOT find character: {name}");

            }

        }

    }



    // --- UI Button Callbacks ---



    public void OnGameStartClick()
    {
        Debug.Log("[SchoolLobbyManager] Game Start Clicked! (Checking Song Selection Panel)");

        // [FIX] Try to initialize panel if null
        if (songSelectPanel == null)
        {
            Debug.LogWarning("[SchoolLobbyManager] songSelectPanel was NULL, attempting to initialize...");
            InitializeSongSelectPanelIfNeeded();
        }

        if (songSelectPanel != null)
        {
            Debug.Log($"[SchoolLobbyManager] Toggling Song Selection Panel: {songSelectPanel.name}");
            TogglePanel(songSelectPanel);
        }
        else
        {
            Debug.LogError("[SchoolLobbyManager] Song Selection Panel is still NULL after initialization attempt!");
        }
    }



    public void OnRankingClick()
    {
        Debug.Log("[SchoolLobbyManager] Ranking Clicked!");
        
        // 1. Try to find existing
        if (rankingPanel == null) rankingPanel = FindButtonPanel("RankingPanel");
        
        // 2. If still null, CREATE IT
        if (rankingPanel == null)
        {
             Debug.Log("[SchoolLobbyManager] RankingPanel missing! Creating new one...");
             Canvas canvas = FindObjectOfType<Canvas>();
             if (canvas != null)
             {
                 if (uiManager == null) uiManager = GetComponent<LobbyUIManager>();
                 if (uiManager == null) uiManager = gameObject.AddComponent<LobbyUIManager>();
                 
                 rankingPanel = uiManager.CreateRankingPanel(canvas.transform);
                 rankingPanel.SetActive(false);
             }
        }

        // [FIX] Runtime Visual Correction for Background
        if (rankingPanel != null)
        {
             UnityEngine.UI.Image bg = rankingPanel.GetComponent<UnityEngine.UI.Image>();
             if (bg != null)
             {
                 RectTransform rt = rankingPanel.GetComponent<RectTransform>();
                 rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                 rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
                 bg.color = new Color(0, 0, 0, 0.85f); 
             }
        }

        TogglePanel(rankingPanel);
    }

    public void OnShopClick()
    {
        Debug.Log("[SchoolLobbyManager] Shop Clicked!");
        if (shopPanel == null)
        {
            shopPanel = FindButtonPanel("StepUpShopPanel");
            if(shopPanel == null) shopPanel = FindButtonPanel("PremiumShop");
        }
        
        if (shopPanel == null)
        {
             Debug.LogWarning("Shop Panel completely missing.");
        }
        
        // [FIX] Runtime Visual Correction for Double Background Issue
        if (shopPanel != null)
        {
             UnityEngine.UI.Image bg = shopPanel.GetComponent<UnityEngine.UI.Image>();
             if (bg != null)
             {
                 // Make it full screen dim
                 RectTransform rt = shopPanel.GetComponent<RectTransform>();
                 rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                 rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
                 bg.color = new Color(0, 0, 0, 0.85f); // Beautiful Dim
             }
        }

        TogglePanel(shopPanel);
    }

    private GameObject FindButtonPanel(string name)
    {
        // GameObject.Find only finds active objects. We need to find inactive ones too.
        // Assuming they are under the main Canvas or root.
        foreach (Canvas c in Resources.FindObjectsOfTypeAll<Canvas>())
        {
            if (c.gameObject.scene.name != gameObject.scene.name) continue; // Skip prefabs
            Transform t = c.transform.Find(name);
            if (t != null) return t.gameObject;
        }
        return null;
    }







    public void OnProfileClick()

    {

        Debug.Log("[SchoolLobbyManager] Profile Clicked!");

        TogglePanel(profilePanel);

    }



    public void OnOptionsClick()

    {

        Debug.Log("[SchoolLobbyManager] Options Clicked!");

        TogglePanel(optionsPanel);

    }

    

    // [NEW] Selected Song Tracking

    private string _selectedSongTitle = "GALAXIAS!"; // ê¸°ë³¸?



    // [NEW] Real Game Start (called from Song Select Panel)

    public void StartGameLoop()

    {

        Debug.Log($"[SchoolLobbyManager] Starting Game Loop... Selected Song: {_selectedSongTitle}");

        

        // [FIX] ????ê³¡ì ?????ë¶ê¸°

        if (_selectedSongTitle.ToUpper().Contains("SODAPOP"))

        {

            UnityEngine.SceneManagement.SceneManager.LoadScene("Game_second");

        }

        else

        {

            // Default: GALAXIAS!

            UnityEngine.SceneManagement.SceneManager.LoadScene("Game_first");

        }

    }



    public void OnCharacterClick()

    {

        Debug.Log("[SchoolLobbyManager] Open Customization (Locker)");

        TogglePanel(customizationPanel);

    }



    public void OnTutorialClick()

    {

        Debug.Log("[SchoolLobbyManager] Start Tutorial (Orientation)");

        // SceneManager.LoadScene("Tutorial"); 

    }



    private void TogglePanel(GameObject panel)

    {

        if (panel != null)

        {

            // [FIX] Capture state BEFORE closing

            bool wasActive = panel.activeSelf;

            CloseAllPanels();



            bool isOpening = !wasActive;

            panel.SetActive(isOpening);



            if (isOpening) 

            {

                panel.transform.SetAsLastSibling();

                

                RectTransform rt = panel.GetComponent<RectTransform>();

                if (rt != null)

                {
                    // [FIX] User requested to expand background to fit text
                    rt.anchorMin = new Vector2(0.1f, 0.1f);
                    rt.anchorMax = new Vector2(0.9f, 0.9f);
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;
                }



                BindCloseButton(panel);



                if (panel == shopPanel) PopulateShop(panel);

                if (panel == songSelectPanel) PopulateSongSelect(panel);

                if (panel == rankingPanel) PopulateRanking(panel);

                if (panel == optionsPanel) PopulateOptions(panel); 
                
                if (panel == qrConnectionPanel) PopulateQRConnection(panel); // [NEW] Populate QR Logic

            }

        }

        else

        {

            Debug.LogWarning("Panel not assigned in Inspector.");

        }

    }



    // SetMainMenuVisible ë©ì???????????? ??¼ë?????ê±??ë¹ì??

    private void SetMainMenuVisible(bool visible) { }







    private void CreateSongCard(Transform parent, string title, bool unlock, int index)
    {
        if (parent == null) return;

        // Cover Flow Card (Square)
        GameObject card = new GameObject("SongCard_" + title);
        card.layer = LayerMask.NameToLayer("UI");
        card.transform.SetParent(parent, false);

        // Card Background (Album Art Container)
        UnityEngine.UI.Image img = card.AddComponent<UnityEngine.UI.Image>();
        img.color = unlock ? Color.white : new Color(0.1f, 0.1f, 0.1f, 0.8f);

        // [FIX] Preserve Aspect Ratio to prevent "Cutting off" / Squashing
        img.preserveAspect = true;

        // [FIX] Assign Album Art
        if (unlock && albumCovers != null && index < albumCovers.Length && albumCovers[index] != null)
        {
             img.sprite = albumCovers[index];
        }

        RectTransform rt = card.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(320, 420); // [조정] 높이 증가 (320 → 420)
        rt.localScale = Vector3.one;
        rt.anchoredPosition = new Vector2(0, 60);

        // Overlay for Lock
        if(!unlock)
        {
            GameObject lockOverlay = new GameObject("LockOverlay");
            lockOverlay.transform.SetParent(card.transform, false);
            var lockImg = lockOverlay.AddComponent<UnityEngine.UI.Image>();
            lockImg.color = new Color(0, 0, 0, 0.6f);

            RectTransform lrt = lockOverlay.GetComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
            lrt.sizeDelta = Vector2.zero;

            GameObject lockTxtObj = new GameObject("LockText");
            lockTxtObj.transform.SetParent(lockOverlay.transform, false);
            var lTxt = lockTxtObj.AddComponent<TMPro.TextMeshProUGUI>();
            lTxt.text = "LOCKED";
            lTxt.fontSize = 40;
            lTxt.alignment = TMPro.TextAlignmentOptions.Center;
            lTxt.color = Color.gray;
        }

        // Title Text (Below Card)
        GameObject txtObj = new GameObject("Title");
        txtObj.transform.SetParent(card.transform, false);
        var txt = txtObj.AddComponent<TMPro.TextMeshProUGUI>();
        txt.text = title.ToUpper();
        txt.alignment = TMPro.TextAlignmentOptions.Center;
        txt.fontSize = 28;
        txt.fontStyle = TMPro.FontStyles.Bold;
        txt.color = Color.white;

        // Shadow
        var outline = txtObj.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor = new Color(0,0,0,0.8f);
        outline.effectDistance = new Vector2(2, -2);

        RectTransform txtRt = txtObj.GetComponent<RectTransform>();
        txtRt.anchorMin = new Vector2(0, 0.72f); txtRt.anchorMax = new Vector2(1, 0.82f);
        txtRt.offsetMin = Vector2.zero; txtRt.offsetMax = Vector2.zero;

        // [NEW] Difficulty Indicator
        if (unlock && title != "Coming Soon")
        {
            GameObject diffObj = new GameObject("Difficulty");
            diffObj.transform.SetParent(card.transform, false);

            // 난이도 설정 (Galaxias = Easy, Sodapop = Hard)
            string diffText = (index == 0) ? "EASY" : "HARD";
            Color diffColor = (index == 0) ? new Color(0.3f, 0.9f, 0.4f) : new Color(1f, 0.4f, 0.4f);
            int diffLevel = (index == 0) ? 3 : 7; // 난이도 레벨 (1-10)

            var diffTxt = diffObj.AddComponent<TMPro.TextMeshProUGUI>();
            diffTxt.text = $"<color=#{ColorUtility.ToHtmlStringRGB(diffColor)}>{diffText}</color>  Lv.{diffLevel}";
            diffTxt.fontSize = 18;
            diffTxt.alignment = TMPro.TextAlignmentOptions.Center;
            diffTxt.color = Color.white;

            RectTransform diffRt = diffObj.GetComponent<RectTransform>();
            diffRt.anchorMin = new Vector2(0, 0.63f); diffRt.anchorMax = new Vector2(1, 0.72f);
            diffRt.offsetMin = Vector2.zero; diffRt.offsetMax = Vector2.zero;

            // [NEW] Top 3 Ranking Display
            GameObject rankContainer = new GameObject("RankingContainer");
            rankContainer.transform.SetParent(card.transform, false);

            var rankBg = rankContainer.AddComponent<UnityEngine.UI.Image>();
            rankBg.color = new Color(0, 0, 0, 0.5f);

            RectTransform rankContRt = rankContainer.GetComponent<RectTransform>();
            rankContRt.anchorMin = new Vector2(0.05f, 0.02f); rankContRt.anchorMax = new Vector2(0.95f, 0.62f);
            rankContRt.offsetMin = Vector2.zero; rankContRt.offsetMax = Vector2.zero;

            // Ranking Title
            GameObject rankTitleObj = new GameObject("RankTitle");
            rankTitleObj.transform.SetParent(rankContainer.transform, false);
            var rankTitleTxt = rankTitleObj.AddComponent<TMPro.TextMeshProUGUI>();
            rankTitleTxt.text = "TOP 3";
            rankTitleTxt.fontSize = 14;
            rankTitleTxt.fontStyle = TMPro.FontStyles.Bold;
            rankTitleTxt.alignment = TMPro.TextAlignmentOptions.Center;
            rankTitleTxt.color = new Color(1f, 0.85f, 0.2f); // Gold

            RectTransform rankTitleRt = rankTitleObj.GetComponent<RectTransform>();
            rankTitleRt.anchorMin = new Vector2(0, 0.82f); rankTitleRt.anchorMax = new Vector2(1, 1f);
            rankTitleRt.offsetMin = Vector2.zero; rankTitleRt.offsetMax = Vector2.zero;

            // Ranking Entries Container
            GameObject entriesObj = new GameObject("Entries");
            entriesObj.transform.SetParent(rankContainer.transform, false);

            RectTransform entriesRt = entriesObj.AddComponent<RectTransform>();
            entriesRt.anchorMin = new Vector2(0.05f, 0.05f); entriesRt.anchorMax = new Vector2(0.95f, 0.8f);
            entriesRt.offsetMin = Vector2.zero; entriesRt.offsetMax = Vector2.zero;

            var vlg = entriesObj.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
            vlg.spacing = 2;
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            vlg.childAlignment = TextAnchor.UpperCenter;

            // Fetch Top 3 Rankings for this song
            string songName = (index == 0) ? "Galaxias" : "Sodapop";
            FetchTop3Rankings(entriesObj.transform, songName);
        }

        // Button Logic (Click to Select)
        UnityEngine.UI.Button btn = card.AddComponent<UnityEngine.UI.Button>();
        // Invisible button over the art
        UnityEngine.UI.ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(1,1,1,1.1f); // Slight brighten
        btn.colors = cb;

        btn.onClick.AddListener(() => {
            // Snap Carousel
            var cv = parent.GetComponentInParent<CarouselView>();
            if (cv != null) cv.SnapTo(index);
        });
    }

    // [NEW] Fetch Top 3 rankings for a song
    private void FetchTop3Rankings(Transform container, string songName)
    {
        if (NetworkManager.Instance == null)
        {
            CreateRankingPlaceholder(container, "No data");
            return;
        }

        NetworkManager.Instance.GetRankingBySong(songName, (json) => {
            // Clear existing
            foreach (Transform child in container) Destroy(child.gameObject);

            if (string.IsNullOrEmpty(json))
            {
                CreateRankingPlaceholder(container, "No rankings yet");
                return;
            }

            try
            {
                RankEntry[] entries = JsonHelper.FromJson<RankEntry>(json);
                if (entries == null || entries.Length == 0)
                {
                    CreateRankingPlaceholder(container, "Be the first!");
                    return;
                }

                // Show top 3
                int count = Mathf.Min(3, entries.Length);
                for (int i = 0; i < count; i++)
                {
                    CreateMiniRankEntry(container, i + 1, entries[i].user_name, entries[i].score);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SongCard] Ranking Parse Error: {e.Message}");
                CreateRankingPlaceholder(container, "Error loading");
            }
        });
    }

    // [NEW] Create mini rank entry for song card
    private void CreateMiniRankEntry(Transform parent, int rank, string name, int score)
    {
        GameObject entry = new GameObject($"Rank{rank}");
        entry.transform.SetParent(parent, false);

        var le = entry.AddComponent<UnityEngine.UI.LayoutElement>();
        le.preferredHeight = 22;

        var txt = entry.AddComponent<TMPro.TextMeshProUGUI>();

        // Medal colors
        string medalColor = rank == 1 ? "#FFD700" : (rank == 2 ? "#C0C0C0" : "#CD7F32");
        string displayName = string.IsNullOrEmpty(name) ? "Unknown" : (name.Length > 8 ? name.Substring(0, 8) + ".." : name);

        txt.text = $"<color={medalColor}>{rank}.</color> {displayName} <color=#B0B0FF>{score:N0}</color>";
        txt.fontSize = 12;
        txt.alignment = TMPro.TextAlignmentOptions.Left;
        txt.color = Color.white;
    }

    // [NEW] Create placeholder text for empty rankings
    private void CreateRankingPlaceholder(Transform parent, string message)
    {
        GameObject placeholder = new GameObject("Placeholder");
        placeholder.transform.SetParent(parent, false);

        var txt = placeholder.AddComponent<TMPro.TextMeshProUGUI>();
        txt.text = message;
        txt.fontSize = 14;
        txt.fontStyle = TMPro.FontStyles.Italic;
        txt.alignment = TMPro.TextAlignmentOptions.Center;
        txt.color = new Color(0.6f, 0.6f, 0.6f);

        var rt = placeholder.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    // === Cover Flow Variables ===
    private int _currentSongIndex = 0;
    private string[] _cfSongs;
    private bool[] _cfUnlocked;
    private string[] _cfBpmInfo;
    private TMPro.TextMeshProUGUI _songTitleText;
    private TMPro.TextMeshProUGUI _songInfoText;

    private void PopulateSongSelect(GameObject panel)
    {
        Debug.Log("[Lobby] Smooth Scroll Cover Flow");
        foreach (Transform child in panel.transform) Destroy(child.gameObject);

        _cfSongs = new string[] { "GALAXIAS!", "SODAPOP", "Coming Soon" };
        _cfUnlocked = new bool[] { true, true, false };
        _cfBpmInfo = new string[] { "BPM 155  ·  ★★★☆☆  ·  Normal", "BPM 128  ·  ★★★★☆  ·  Hard", "BPM ???  ·  ?????  ·  Locked" };
        float cardWidth = 420f;

        // === DARK BACKGROUND ===
        var bgImg = panel.GetComponent<UnityEngine.UI.Image>();
        if (bgImg == null) bgImg = panel.AddComponent<UnityEngine.UI.Image>();
        bgImg.color = new Color(0.0f, 0.0f, 0.02f, 0.98f);

        // === HEADER ===
        GameObject header = new GameObject("Header");
        header.transform.SetParent(panel.transform, false);
        var headerTxt = header.AddComponent<TMPro.TextMeshProUGUI>();
        headerTxt.text = "♫  SELECT MUSIC  ♫";
        headerTxt.fontSize = 52;
        headerTxt.fontStyle = TMPro.FontStyles.Bold;
        headerTxt.alignment = TMPro.TextAlignmentOptions.Center;
        headerTxt.color = new Color(0f, 1f, 1f);
        headerTxt.raycastTarget = false;
        var headerRt = header.GetComponent<RectTransform>();
        headerRt.anchorMin = new Vector2(0, 0.90f); headerRt.anchorMax = new Vector2(1, 0.99f);
        headerRt.offsetMin = Vector2.zero; headerRt.offsetMax = Vector2.zero;

        // === SCROLL VIEW ===
        GameObject scrollObj = new GameObject("CoverFlowScroll");
        scrollObj.transform.SetParent(panel.transform, false);
        var scrollRt = scrollObj.AddComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0, 0.26f); scrollRt.anchorMax = new Vector2(1, 0.89f);
        scrollRt.offsetMin = Vector2.zero; scrollRt.offsetMax = Vector2.zero;

        scrollObj.AddComponent<UnityEngine.UI.Image>().color = Color.clear;

        var scrollRect = scrollObj.AddComponent<UnityEngine.UI.ScrollRect>();
        scrollRect.horizontal = true;
        scrollRect.vertical = false;
        scrollRect.movementType = UnityEngine.UI.ScrollRect.MovementType.Elastic;
        scrollRect.elasticity = 0.1f;
        scrollRect.inertia = true;
        scrollRect.decelerationRate = 0.08f;
        scrollRect.scrollSensitivity = 0; // Disable default scroll, use custom snap controller

        // Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollObj.transform, false);
        var viewRt = viewport.AddComponent<RectTransform>();
        viewRt.anchorMin = Vector2.zero; viewRt.anchorMax = Vector2.one;
        viewRt.offsetMin = Vector2.zero; viewRt.offsetMax = Vector2.zero;
        viewport.AddComponent<UnityEngine.UI.Mask>().showMaskGraphic = false;
        viewport.AddComponent<UnityEngine.UI.Image>();
        scrollRect.viewport = viewRt;

        // Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        var contentRt = content.AddComponent<RectTransform>();
        // [FIX] Center Pivot to allow moving both ways easily
        contentRt.anchorMin = new Vector2(0.5f, 0.5f);
        contentRt.anchorMax = new Vector2(0.5f, 0.5f);
        contentRt.pivot = new Vector2(0.5f, 0.5f);
        
        // Massive width to allow free scrolling
        contentRt.sizeDelta = new Vector2(5000, 450);
        scrollRect.content = contentRt;

        // Create Cards
        for (int i = 0; i < _cfSongs.Length; i++)
        {
            // Cards need to be centered relative to content's 0,0
            CreateScrollCard(contentRt, i, cardWidth);
        }

        // Add Snap Controller
        var snap = scrollObj.AddComponent<CoverFlowSnapController>();
        snap.scrollRect = scrollRect;
        snap.cardWidth = cardWidth;
        snap.cardCount = _cfSongs.Length;
        snap.snapSpeed = 12f;
        // snap.viewportCenterX is calc in Start()
        snap.onSnapChanged = OnCoverFlowIndexChanged;
        
        // [FIX] Force initial snap to index 0
        // We defer this slightly to ensure Layout is ready, or rely on Start()
        // But better to be explicit here if we can
        snap.SnapToIndex(0, true);

        // === SONG TITLE ===
        GameObject titleObj = new GameObject("SongTitle");
        titleObj.transform.SetParent(panel.transform, false);
        _songTitleText = titleObj.AddComponent<TMPro.TextMeshProUGUI>();
        _songTitleText.text = _cfSongs[0];
        _songTitleText.fontSize = 52;
        _songTitleText.fontStyle = TMPro.FontStyles.Bold;
        _songTitleText.alignment = TMPro.TextAlignmentOptions.Center;
        _songTitleText.color = Color.white;
        _songTitleText.raycastTarget = false;
        var titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0, 0.16f); titleRt.anchorMax = new Vector2(1, 0.25f);
        titleRt.offsetMin = Vector2.zero; titleRt.offsetMax = Vector2.zero;

        // === SONG INFO ===
        GameObject infoObj = new GameObject("SongInfo");
        infoObj.transform.SetParent(panel.transform, false);
        _songInfoText = infoObj.AddComponent<TMPro.TextMeshProUGUI>();
        _songInfoText.text = _cfBpmInfo[0];
        _songInfoText.fontSize = 28;
        _songInfoText.alignment = TMPro.TextAlignmentOptions.Center;
        _songInfoText.color = new Color(0f, 1f, 1f, 0.9f);
        _songInfoText.raycastTarget = false;
        var infoRt = infoObj.GetComponent<RectTransform>();
        infoRt.anchorMin = new Vector2(0, 0.09f); infoRt.anchorMax = new Vector2(1, 0.16f);
        infoRt.offsetMin = Vector2.zero; infoRt.offsetMax = Vector2.zero;

        // === PLAY BUTTON ===
        GameObject playBtn = new GameObject("PlayButton");
        playBtn.transform.SetParent(panel.transform, false);
        var playImg = playBtn.AddComponent<UnityEngine.UI.Image>();
        playImg.color = new Color(0f, 0.9f, 1f);
        var playBtnComp = playBtn.AddComponent<UnityEngine.UI.Button>();
        var playRt = playBtn.GetComponent<RectTransform>();
        playRt.anchorMin = new Vector2(0.28f, 0.01f); playRt.anchorMax = new Vector2(0.72f, 0.08f);
        playRt.offsetMin = Vector2.zero; playRt.offsetMax = Vector2.zero;

        // Play Glow
        GameObject glow = new GameObject("Glow");
        glow.transform.SetParent(playBtn.transform, false);
        glow.transform.SetAsFirstSibling();
        var glowImg = glow.AddComponent<UnityEngine.UI.Image>();
        glowImg.color = new Color(0f, 1f, 1f, 0.5f);
        glowImg.raycastTarget = false;
        var glowRt = glow.GetComponent<RectTransform>();
        glowRt.anchorMin = Vector2.zero; glowRt.anchorMax = Vector2.one;
        glowRt.offsetMin = new Vector2(-6, -6); glowRt.offsetMax = new Vector2(6, 6);

        // Play Text
        GameObject playTxtObj = new GameObject("Text");
        playTxtObj.transform.SetParent(playBtn.transform, false);
        var playTxt = playTxtObj.AddComponent<TMPro.TextMeshProUGUI>();
        playTxt.text = "▶  PLAY";
        playTxt.fontSize = 40;
        playTxt.fontStyle = TMPro.FontStyles.Bold;
        playTxt.alignment = TMPro.TextAlignmentOptions.Center;
        playTxt.color = new Color(0.01f, 0.01f, 0.05f);
        playTxt.raycastTarget = false;
        var playTxtRt = playTxtObj.GetComponent<RectTransform>();
        playTxtRt.anchorMin = Vector2.zero; playTxtRt.anchorMax = Vector2.one;
        playTxtRt.offsetMin = Vector2.zero; playTxtRt.offsetMax = Vector2.zero;

        playBtnComp.onClick.AddListener(() => {
            if (!_cfUnlocked[_currentSongIndex]) return;
            StartGameLoop();
        });

        // === CLOSE BUTTON (Styled Circle) ===
        GameObject closeBtn = new GameObject("CloseBtn");
        closeBtn.transform.SetParent(panel.transform, false);
        var closeRt = closeBtn.AddComponent<RectTransform>();
        closeRt.anchorMin = new Vector2(1, 1); closeRt.anchorMax = new Vector2(1, 1);
        closeRt.pivot = new Vector2(1, 1);
        closeRt.sizeDelta = new Vector2(60, 60);
        closeRt.anchoredPosition = new Vector2(-20, -15);

        // Glow effect behind button
        GameObject closeGlow = new GameObject("Glow");
        closeGlow.transform.SetParent(closeBtn.transform, false);
        var closeGlowImg = closeGlow.AddComponent<UnityEngine.UI.Image>();
        closeGlowImg.color = new Color(1f, 0.3f, 0.3f, 0.4f);
        closeGlowImg.raycastTarget = false;
        var closeGlowRt = closeGlow.GetComponent<RectTransform>();
        closeGlowRt.anchorMin = Vector2.zero; closeGlowRt.anchorMax = Vector2.one;
        closeGlowRt.offsetMin = new Vector2(-5, -5); closeGlowRt.offsetMax = new Vector2(5, 5);

        // Main button background
        var closeImg = closeBtn.AddComponent<UnityEngine.UI.Image>();
        closeImg.color = new Color(0.85f, 0.15f, 0.15f);
        var closeBtnComp = closeBtn.AddComponent<UnityEngine.UI.Button>();

        // Button colors for hover effect
        var colors = closeBtnComp.colors;
        colors.normalColor = new Color(0.85f, 0.15f, 0.15f);
        colors.highlightedColor = new Color(1f, 0.3f, 0.3f);
        colors.pressedColor = new Color(0.6f, 0.1f, 0.1f);
        closeBtnComp.colors = colors;
        closeBtnComp.onClick.AddListener(() => TogglePanel(panel));

        // X text using simple "X" character
        GameObject closeTxtObj = new GameObject("X");
        closeTxtObj.transform.SetParent(closeBtn.transform, false);
        var closeTxt = closeTxtObj.AddComponent<TMPro.TextMeshProUGUI>();
        closeTxt.text = "X";
        closeTxt.fontSize = 36;
        closeTxt.fontStyle = TMPro.FontStyles.Bold;
        closeTxt.alignment = TMPro.TextAlignmentOptions.Center;
        closeTxt.color = Color.white;
        closeTxt.raycastTarget = false;
        var closeTxtRt = closeTxtObj.GetComponent<RectTransform>();
        closeTxtRt.anchorMin = Vector2.zero; closeTxtRt.anchorMax = Vector2.one;
        closeTxtRt.offsetMin = Vector2.zero; closeTxtRt.offsetMax = Vector2.zero;

        // Initialize
        _currentSongIndex = 0;
        _selectedSongTitle = _cfSongs[0];
    }

    private void CreateScrollCard(RectTransform content, int index, float cardWidth)
    {
        GameObject card = new GameObject($"Card_{index}");
        card.transform.SetParent(content, false);

        var cardRt = card.AddComponent<RectTransform>();
        cardRt.anchorMin = new Vector2(0.5f, 0.5f);
        cardRt.anchorMax = new Vector2(0.5f, 0.5f);
        cardRt.pivot = new Vector2(0.5f, 0.5f);
        cardRt.sizeDelta = new Vector2(380, 380);
        
        // [FIX] Center the cards relative to the content center
        // Index 0 should be at 0 if it's the only one, but here we want them in a row
        // We rely on the SnapController to move the Content
        // So we just place them linearly starting from 0
        float xPos = (index * cardWidth); 
        
        // However, since we changed pivot to center, 0 is the center of content
        // Let's shift them so index 0 starts at a specific point or just keep linear
        // Actually, linear 0, 450, 900 is fine if SnapController moves content to -0, -450, -900
        
        cardRt.anchoredPosition = new Vector2(xPos, 0);

        // Neon Border
        var borderImg = card.AddComponent<UnityEngine.UI.Image>();
        borderImg.color = _cfUnlocked[index] ? new Color(0f, 1f, 1f, 0.7f) : new Color(0.4f, 0.4f, 0.4f, 0.7f);

        // Inner Album Art
        GameObject inner = new GameObject("AlbumArt");
        inner.transform.SetParent(card.transform, false);
        var innerImg = inner.AddComponent<UnityEngine.UI.Image>();
        innerImg.preserveAspect = true;
        innerImg.raycastTarget = false;

        if (_cfUnlocked[index] && albumCovers != null && index < albumCovers.Length && albumCovers[index] != null)
        {
            innerImg.sprite = albumCovers[index];
            innerImg.color = Color.white;
        }
        else
        {
            innerImg.color = _cfUnlocked[index] ? new Color(0.1f, 0.15f, 0.25f) : new Color(0.08f, 0.08f, 0.08f);
        }

        var innerRt = inner.GetComponent<RectTransform>();
        innerRt.anchorMin = Vector2.zero; innerRt.anchorMax = Vector2.one;
        innerRt.offsetMin = new Vector2(8, 8); innerRt.offsetMax = new Vector2(-8, -8);

        // Lock overlay
        if (!_cfUnlocked[index])
        {
            GameObject lockObj = new GameObject("Lock");
            lockObj.transform.SetParent(inner.transform, false);
            var lockImg = lockObj.AddComponent<UnityEngine.UI.Image>();
            lockImg.color = new Color(0, 0, 0, 0.75f);
            lockImg.raycastTarget = false;
            var lockRt = lockObj.GetComponent<RectTransform>();
            lockRt.anchorMin = Vector2.zero; lockRt.anchorMax = Vector2.one;
            lockRt.offsetMin = Vector2.zero; lockRt.offsetMax = Vector2.zero;

            GameObject lockIcon = new GameObject("Icon");
            lockIcon.transform.SetParent(lockObj.transform, false);
            var lockTxt = lockIcon.AddComponent<TMPro.TextMeshProUGUI>();
            lockTxt.text = "🔒";
            lockTxt.fontSize = 80;
            lockTxt.alignment = TMPro.TextAlignmentOptions.Center;
            lockTxt.raycastTarget = false;
            var iconRt = lockIcon.GetComponent<RectTransform>();
            iconRt.anchorMin = Vector2.zero; iconRt.anchorMax = Vector2.one;
            iconRt.offsetMin = Vector2.zero; iconRt.offsetMax = Vector2.zero;
        }
    }

    private void OnCoverFlowIndexChanged(int index)
    {
        if (index < 0 || index >= _cfSongs.Length) return;
        _currentSongIndex = index;

        if (_songTitleText != null) _songTitleText.text = _cfSongs[index];
        if (_songInfoText != null) _songInfoText.text = _cfBpmInfo[index];

        _selectedSongTitle = _cfUnlocked[index] ? _cfSongs[index] : "LOCKED";
        Debug.Log($"[CoverFlow] Selected: {_cfSongs[index]}");
    }





    private void BindSongSelectStartButton(GameObject panel)

    {

        // "StartButton" ???"Btn_Start" ì°¾ê¸°

        var buttons = panel.GetComponentsInChildren<UnityEngine.UI.Button>(true);

        UnityEngine.UI.Button startBtn = null;

        

        foreach (var btn in buttons)

        {

            if (btn.name.Contains("Start") || btn.name.Contains("Play") || 

                btn.GetComponentInChildren<TMPro.TextMeshProUGUI>()?.text.ToUpper().Contains("START") == true ||

                btn.GetComponentInChildren<TMPro.TextMeshProUGUI>()?.text.ToUpper().Contains("PLAY") == true)

            {

                startBtn = btn;

                break;

            }

        }



        if (startBtn != null)

        {

            startBtn.onClick.RemoveAllListeners();

            startBtn.onClick.AddListener(() => {

                StartGameLoop();

            });

            Debug.Log("[SchoolLobbyManager] Bound Song Select START button!");

        }

        else

        {

            Debug.LogWarning("[SchoolLobbyManager] Could NOT find Start Button in Song Select Panel!");

        }

    }







    private void CloseAllPanels()
    {
        // 1. Close Managed Panels
        if (songSelectPanel != null) songSelectPanel.SetActive(false);
        if (rankingPanel != null) rankingPanel.SetActive(false);
        if (shopPanel != null) shopPanel.SetActive(false);
        if (profilePanel != null) profilePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (customizationPanel != null) customizationPanel.SetActive(false);

        // 2. [FIX] Force Close Unmanaged/Legacy Panels by Name (to fix overlap issues)
        GameObject[] panelsToClose = new GameObject[] {
            GameObject.Find("SongSelectPanel"),
            GameObject.Find("SongSelectPanel2"), // Likely the culprit
            GameObject.Find("RankingPanel"),
            GameObject.Find("StepUpShopPanel"),
            GameObject.Find("PremiumShop"),
            GameObject.Find("Shop Popup")
        };

        foreach (var p in panelsToClose)
        {
            if (p != null && p.activeSelf) 
            {
                p.SetActive(false);
                Debug.Log($"[Lobby] Force closed stray panel: {p.name}");
            }
        }
    }



    private void UpdateProfileUI()
    {
        Debug.Log("[Lobby] Updating Profile UI...");

        // [FIX] 기존 WelcomeText 삭제 - UserInfoPanel로 대체
        GameObject oldWelcome = GameObject.Find("WelcomeText");
        if (oldWelcome != null)
        {
            Destroy(oldWelcome);
        }

        if (NetworkManager.Instance != null && NetworkManager.Instance.IsLoggedIn)
        {
            // UserInfoPanel 찾기
            GameObject userInfoPanel = GameObject.Find("UserInfoPanel");
            if (userInfoPanel != null)
            {
                userInfoPanel.SetActive(true);

                // 사용자 정보 업데이트
                NetworkManager.Instance.GetUserProfile((profile) => {
                    string displayName = "Player";
                    if (profile != null && !string.IsNullOrEmpty(profile.name))
                    {
                        displayName = profile.name;
                    }
                    else if (!string.IsNullOrEmpty(NetworkManager.Instance.CurrentUserEmail))
                    {
                        displayName = NetworkManager.Instance.CurrentUserEmail.Split('@')[0];
                    }

                    // UserNameText 업데이트
                    Transform nameTransform = userInfoPanel.transform.Find("UserNameText");
                    if (nameTransform != null)
                    {
                        var nameTxt = nameTransform.GetComponent<TMPro.TextMeshProUGUI>();
                        if (nameTxt != null)
                        {
                            nameTxt.text = displayName;
                        }
                    }

                    // LevelText 업데이트
                    Transform levelTransform = userInfoPanel.transform.Find("LevelText");
                    if (levelTransform != null)
                    {
                        var levelTxt = levelTransform.GetComponent<TMPro.TextMeshProUGUI>();
                        if (levelTxt != null)
                        {
                            levelTxt.text = "Logged In";
                        }
                    }

                    Debug.Log($"[Lobby] Updated user info: {displayName}");
                });
            }

            // Login 버튼 텍스트 변경
            GameObject loginBtn = GameObject.Find("LOGIN");
            if (loginBtn != null)
            {
                var btnTxt = loginBtn.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (btnTxt != null)
                {
                    btnTxt.text = "LOGOUT";
                }
            }
        }
        else
        {
            // 로그아웃 상태
            GameObject userInfoPanel = GameObject.Find("UserInfoPanel");
            if (userInfoPanel != null)
            {
                userInfoPanel.SetActive(false);
            }

            // Login 버튼 텍스트 복원
            GameObject loginBtn = GameObject.Find("LOGIN");
            if (loginBtn != null)
            {
                var btnTxt = loginBtn.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (btnTxt != null)
                {
                    btnTxt.text = "LOGIN";
                }
            }
        }
    }

    private void BindCloseButton(GameObject panel)

    {

        // ???????"CloseButton" ?ë¦??ê°??ë²í¼ ì°¾ê¸° (?????

        // ë§ì½ ?ë¦??"Close"?ë©?"Close"?ì°¾ì¼??? ë³´íµ "CloseButton"???"Btn_Close" ??????????

        // ?ê¸???"CloseButton" ???"Btn_Close" ?????????

        

        UnityEngine.UI.Button closeBtn = panel.GetComponentInChildren<UnityEngine.UI.Button>(); // ????ë¬?ë²í¼????????????(ë³´íµ ?ê¸???ë¿???????

        

        // ?ë¦?ë¡?????ì°¾ê³  ???

        UnityEngine.UI.Button[] allBtns = panel.GetComponentsInChildren<UnityEngine.UI.Button>(true);

        foreach (var btn in allBtns)

        {

            if (btn.name.Contains("Close") || btn.name.Contains("Exit") || btn.transform.Find("Text (TMP)")?.GetComponent<TMPro.TextMeshProUGUI>()?.text.Contains("CLOSE") == true)

            {

                closeBtn = btn;

                break;

            }

        }



        if (closeBtn != null)

        {

            // ?ê¸?ë²í¼?????? ?ë¦??ë§? ?ê²????
            var txt = closeBtn.GetComponentInChildren<TMPro.TextMeshProUGUI>();

            if (txt != null) txt.raycastTarget = false;

            var txt2 = closeBtn.GetComponentInChildren<UnityEngine.UI.Text>();

            if (txt2 != null) txt2.raycastTarget = false;



            // ?ë²???ê²?
            closeBtn.onClick.RemoveAllListeners();

            closeBtn.onClick.AddListener(() => {

                panel.SetActive(false);

                Debug.Log($"[SchoolLobbyManager] Closed panel: {panel.name}");

            });

            Debug.Log($"[SchoolLobbyManager] Bound Close Button in {panel.name}");

        }

        else

        {

            Debug.LogWarning($"[SchoolLobbyManager] Could NOT find Close Button in {panel.name}");

        }

    }





    // [NEW] Helper to find inactive objects recursively

    private GameObject FindObjectRecursive(GameObject root, string name)

    {

        if (root == null) return null;

        if (root.name == name) return root;

        

        foreach(Transform t in root.transform)

        {

            if (t.name == name) return t.gameObject;

            GameObject res = FindObjectRecursive(t.gameObject, name);

            if (res != null) return res;

        }

        return null;

    }



    private string currentRankingFilter = "all";

    private void PopulateRanking(GameObject panel)
    {
        Debug.Log("[Lobby] Populating Ranking...");

        foreach (Transform child in panel.transform) Destroy(child.gameObject);

        // Background
        var bgImg = panel.GetComponent<UnityEngine.UI.Image>();
        if (bgImg == null) bgImg = panel.AddComponent<UnityEngine.UI.Image>();
        bgImg.color = new Color(0.05f, 0.05f, 0.1f, 0.98f);

        // Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panel.transform, false);
        var titleTxt = titleObj.AddComponent<TMPro.TextMeshProUGUI>();
        titleTxt.text = "GLOBAL RANKING";
        titleTxt.fontSize = 42;
        titleTxt.fontStyle = TMPro.FontStyles.Bold;
        titleTxt.alignment = TMPro.TextAlignmentOptions.Center;
        titleTxt.color = new Color(0.7f, 0.5f, 1f);

        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0, 0.88f);
        titleRt.anchorMax = new Vector2(1, 0.98f);
        titleRt.offsetMin = Vector2.zero;
        titleRt.offsetMax = Vector2.zero;

        // [NEW] Song Filter Buttons Container
        GameObject filterContainer = new GameObject("FilterContainer");
        filterContainer.transform.SetParent(panel.transform, false);

        RectTransform filterRt = filterContainer.AddComponent<RectTransform>();
        filterRt.anchorMin = new Vector2(0.1f, 0.78f);
        filterRt.anchorMax = new Vector2(0.9f, 0.87f);
        filterRt.offsetMin = Vector2.zero;
        filterRt.offsetMax = Vector2.zero;

        var filterHlg = filterContainer.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        filterHlg.spacing = 15;
        filterHlg.childAlignment = TextAnchor.MiddleCenter;
        filterHlg.childControlWidth = false;
        filterHlg.childControlHeight = false;

        // Filter Buttons
        string[] filters = { "all", "Galaxias", "Sodapop" };
        string[] filterLabels = { "ALL SONGS", "GALAXIAS", "SODAPOP" };

        for (int i = 0; i < filters.Length; i++)
        {
            string filterValue = filters[i];
            CreateFilterButton(filterContainer.transform, filterLabels[i], filterValue, panel);
        }

        // Scroll View for Rankings
        GameObject scrollObj = new GameObject("ScrollArea");
        scrollObj.transform.SetParent(panel.transform, false);

        var scrollRt = scrollObj.AddComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0.05f, 0.1f);
        scrollRt.anchorMax = new Vector2(0.95f, 0.76f);
        scrollRt.offsetMin = Vector2.zero;
        scrollRt.offsetMax = Vector2.zero;

        scrollObj.AddComponent<UnityEngine.UI.Image>().color = new Color(0.08f, 0.08f, 0.12f, 0.9f);
        var scroll = scrollObj.AddComponent<UnityEngine.UI.ScrollRect>();

        // Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(scrollObj.transform, false);

        var vlg = content.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
        vlg.padding = new RectOffset(15, 15, 15, 15);
        vlg.spacing = 10;
        vlg.childControlHeight = false;
        vlg.childControlWidth = true;
        vlg.childAlignment = TextAnchor.UpperCenter;

        var csf = content.AddComponent<UnityEngine.UI.ContentSizeFitter>();
        csf.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;

        var contentRt = content.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0, 1);
        contentRt.anchorMax = new Vector2(1, 1);
        contentRt.pivot = new Vector2(0.5f, 1);

        // Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollObj.transform, false);
        viewport.AddComponent<UnityEngine.UI.RectMask2D>();

        var vpRt = viewport.GetComponent<RectTransform>();
        vpRt.anchorMin = Vector2.zero;
        vpRt.anchorMax = Vector2.one;
        vpRt.offsetMin = Vector2.zero;
        vpRt.offsetMax = Vector2.zero;

        scroll.viewport = vpRt;
        scroll.content = contentRt;
        scroll.vertical = true;
        scroll.horizontal = false;
        scroll.movementType = UnityEngine.UI.ScrollRect.MovementType.Elastic;

        content.transform.SetParent(viewport.transform, false);

        // Load rankings with current filter
        LoadRankings(content.transform, currentRankingFilter);

        // Close Button logic remains...
        GameObject closeBtnObj = new GameObject("Btn_Close");

        closeBtnObj.transform.SetParent(panel.transform, false);

        var closeImg = closeBtnObj.AddComponent<UnityEngine.UI.Image>();

        closeImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        var closeBtn = closeBtnObj.AddComponent<UnityEngine.UI.Button>();

        closeBtn.onClick.AddListener(() => { TogglePanel(panel); });

        

        RectTransform closeRt = closeBtnObj.GetComponent<RectTransform>();

        closeRt.anchorMin = new Vector2(0.92f, 0.92f); closeRt.anchorMax = new Vector2(0.98f, 0.98f);

        closeRt.offsetMin = Vector2.zero; closeRt.offsetMax = Vector2.zero;

        

        GameObject closeTxtObj = new GameObject("Text");

        closeTxtObj.transform.SetParent(closeBtnObj.transform, false);

        var clTxt = closeTxtObj.AddComponent<TMPro.TextMeshProUGUI>();

        clTxt.text = "X";

        clTxt.fontSize = 28;

        clTxt.alignment = TMPro.TextAlignmentOptions.Center;

        clTxt.color = Color.white;

        

        var clRt = closeTxtObj.AddComponent<RectTransform>();

        clRt.anchorMin = Vector2.zero; clRt.anchorMax = Vector2.one;

        clRt.offsetMin = Vector2.zero; clRt.offsetMax = Vector2.zero;

    }



    // [UPDATED] Helper for creating empty ranking message
    private void CreateNoDataMessage(Transform content)
    {
        GameObject item = new GameObject("NoData");
        item.transform.SetParent(content, false);

        var bgImg = item.AddComponent<UnityEngine.UI.Image>();
        bgImg.color = new Color(0, 0, 0, 0.3f);

        var le = item.AddComponent<UnityEngine.UI.LayoutElement>();
        le.preferredHeight = 100;

        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(item.transform, false);
        var txt = txtObj.AddComponent<TMPro.TextMeshProUGUI>();
        txt.text = "No rankings yet!\nPlay a game to be the first!";
        txt.fontSize = 24;
        txt.alignment = TMPro.TextAlignmentOptions.Center;
        txt.color = Color.gray;

        var txtRt = txtObj.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = Vector2.zero;
        txtRt.offsetMax = Vector2.zero;
    }

    private void CreateRankItem(Transform content, int i, string name, string songName, int score)
    {
        GameObject item = new GameObject($"Rank_{i}");
        item.transform.SetParent(content, false);

        var hlg = item.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(15, 15, 8, 8);
        hlg.spacing = 15;
        hlg.childControlWidth = false; hlg.childControlHeight = false;
        hlg.childAlignment = TextAnchor.MiddleLeft;

        var bgImg = item.AddComponent<UnityEngine.UI.Image>();
        bgImg.color = (i < 3) ? new Color(0.3f, 0.2f, 0.5f, 0.6f) : new Color(0, 0, 0, 0.5f);

        var le = item.AddComponent<UnityEngine.UI.LayoutElement>();
        le.preferredHeight = 70;

        // Medal/Rank number
        if (i < 3)
        {
            GameObject medalObj = new GameObject("Medal");
            medalObj.transform.SetParent(item.transform, false);
            var mImg = medalObj.AddComponent<UnityEngine.UI.Image>();
            if(i==0) mImg.color = new Color(1f, 0.85f, 0.2f); // Gold
            else if(i==1) mImg.color = new Color(0.8f, 0.8f, 0.9f); // Silver
            else if(i==2) mImg.color = new Color(0.85f, 0.55f, 0.2f); // Bronze
            var mLe = medalObj.AddComponent<UnityEngine.UI.LayoutElement>();
            mLe.preferredWidth = 45; mLe.preferredHeight = 45;
        }
        else
        {
            GameObject rankObj = new GameObject("Rank");
            rankObj.transform.SetParent(item.transform, false);
            var rankTxt = rankObj.AddComponent<TMPro.TextMeshProUGUI>();
            rankTxt.text = $"{i+1}";
            rankTxt.fontSize = 24;
            rankTxt.fontStyle = TMPro.FontStyles.Bold;
            rankTxt.alignment = TMPro.TextAlignmentOptions.Center;
            rankTxt.color = Color.gray;
            var rLe = rankObj.AddComponent<UnityEngine.UI.LayoutElement>();
            rLe.preferredWidth = 45; rLe.preferredHeight = 45;
        }

        // Player Name
        GameObject nameObj = new GameObject("Name");
        nameObj.transform.SetParent(item.transform, false);
        var nameTxt = nameObj.AddComponent<TMPro.TextMeshProUGUI>();
        nameTxt.text = string.IsNullOrEmpty(name) ? "Unknown" : name;
        nameTxt.fontSize = 26;
        nameTxt.fontStyle = TMPro.FontStyles.Bold;
        nameTxt.alignment = TMPro.TextAlignmentOptions.Left;
        nameTxt.color = (i < 3) ? Color.white : new Color(0.9f, 0.9f, 0.9f);
        var nLe = nameObj.AddComponent<UnityEngine.UI.LayoutElement>();
        nLe.preferredWidth = 250; nLe.preferredHeight = 45;

        // Song Name
        GameObject songObj = new GameObject("Song");
        songObj.transform.SetParent(item.transform, false);
        var songTxt = songObj.AddComponent<TMPro.TextMeshProUGUI>();
        songTxt.text = string.IsNullOrEmpty(songName) ? "-" : songName;
        songTxt.fontSize = 18;
        songTxt.alignment = TMPro.TextAlignmentOptions.Center;
        songTxt.color = new Color(0.6f, 0.6f, 0.7f);
        var sLe = songObj.AddComponent<UnityEngine.UI.LayoutElement>();
        sLe.preferredWidth = 120; sLe.preferredHeight = 45;

        // Score
        GameObject scoreObj = new GameObject("Score");
        scoreObj.transform.SetParent(item.transform, false);
        var scoreTxt = scoreObj.AddComponent<TMPro.TextMeshProUGUI>();
        scoreTxt.text = $"{score:N0}";
        scoreTxt.fontSize = 28;
        scoreTxt.fontStyle = TMPro.FontStyles.Bold;
        scoreTxt.alignment = TMPro.TextAlignmentOptions.Right;
        scoreTxt.color = new Color(0.6f, 0.4f, 1f); // Purple
        var scLe = scoreObj.AddComponent<UnityEngine.UI.LayoutElement>();
        scLe.preferredWidth = 180; scLe.preferredHeight = 45;
    }

    // [NEW] Create filter button for song filtering
    private void CreateFilterButton(Transform parent, string label, string filterValue, GameObject panel)
    {
        GameObject btnObj = new GameObject($"Filter_{filterValue}");
        btnObj.transform.SetParent(parent, false);

        var btnImg = btnObj.AddComponent<UnityEngine.UI.Image>();
        bool isActive = currentRankingFilter == filterValue;
        btnImg.color = isActive ? new Color(0.6f, 0.4f, 1f, 1f) : new Color(0.2f, 0.2f, 0.3f, 0.8f);

        var btn = btnObj.AddComponent<UnityEngine.UI.Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(0.7f, 0.5f, 1f, 1f);
        colors.pressedColor = new Color(0.5f, 0.3f, 0.8f, 1f);
        btn.colors = colors;

        btn.onClick.AddListener(() => {
            currentRankingFilter = filterValue;
            PopulateRanking(panel); // Refresh ranking panel
        });

        var le = btnObj.AddComponent<UnityEngine.UI.LayoutElement>();
        // [FIX] 버튼 크기 초광각 확대 (너비 300, 높이 60)
        le.preferredWidth = 300;
        le.preferredHeight = 60;

        // Button Text
        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(btnObj.transform, false);
        var txt = txtObj.AddComponent<TMPro.TextMeshProUGUI>();
        txt.text = label;
        txt.fontSize = 18;
        txt.fontStyle = TMPro.FontStyles.Bold;
        txt.alignment = TMPro.TextAlignmentOptions.Center;
        txt.color = isActive ? Color.white : new Color(0.7f, 0.7f, 0.7f);

        var txtRt = txtObj.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = Vector2.zero;
        txtRt.offsetMax = Vector2.zero;
    }

    // [NEW] Load rankings with song filter
    private void LoadRankings(Transform content, string songFilter)
    {
        Debug.Log($"[Ranking] Loading rankings with filter: {songFilter}");

        // Clear existing items
        foreach (Transform child in content) Destroy(child.gameObject);

        // Fetch from Supabase
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.GetRankingBySong(songFilter, (json) => {
                if (string.IsNullOrEmpty(json))
                {
                    CreateNoDataMessage(content);
                    return;
                }

                try
                {
                    RankEntry[] entries = JsonHelper.FromJson<RankEntry>(json);
                    if (entries == null || entries.Length == 0)
                    {
                        CreateNoDataMessage(content);
                        return;
                    }

                    for (int i = 0; i < entries.Length; i++)
                    {
                        CreateRankItem(content, i, entries[i].user_name, entries[i].song_name, entries[i].score);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[Ranking] Parse Error: {e.Message}");
                    CreateNoDataMessage(content);
                }
            });
        }
        else
        {
            Debug.LogWarning("[Ranking] NetworkManager not found!");
            CreateNoDataMessage(content);
        }
    }

    private void PopulateShop(GameObject panel)
    {
        // [FIX] 구형 상점 생성 코드 차단 (StepUpShopPanel 사용)
        // 모든 상점 로직은 ShopLogic.cs와 StepUpShopPanel에서 처리
        Debug.Log("[Lobby] PopulateShop disabled - using StepUpShopPanel instead");
    }



    private void OpenChargePopup(GameObject parentPanel)
    {
        GameObject popup = new GameObject("ChargePopup");
        popup.transform.SetParent(parentPanel.transform.parent, false); // Overlay on Canvas

        // Dimmer (Click to Close)
        var dim = popup.AddComponent<UnityEngine.UI.Image>();
        dim.color = new Color(0,0,0,0.8f);
        var dimBtn = popup.AddComponent<UnityEngine.UI.Button>();
        dimBtn.transition = UnityEngine.UI.Selectable.Transition.None;
        dimBtn.onClick.AddListener(()=> Destroy(popup));

        var dimRt = popup.GetComponent<RectTransform>();
        dimRt.anchorMin = Vector2.zero; dimRt.anchorMax = Vector2.one;
        dimRt.offsetMin = Vector2.zero; dimRt.offsetMax = Vector2.zero;

        // Container
        GameObject container = new GameObject("Container");
        container.transform.SetParent(popup.transform, false);
        var cImg = container.AddComponent<UnityEngine.UI.Image>();
        cImg.color = new Color(0.15f, 0.15f, 0.2f); // Dark aesthetics
        var cRt = container.GetComponent<RectTransform>();
        cRt.sizeDelta = new Vector2(800, 600);

        // Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(container.transform, false);
        var tTxt = titleObj.AddComponent<TMPro.TextMeshProUGUI>();
        if (uiFont != null) tTxt.font = uiFont; // [FIX]
        tTxt.text = "GEMS CHARGE";
        tTxt.fontSize = 40; tTxt.alignment = TMPro.TextAlignmentOptions.Center;
        tTxt.color = new Color(0, 0.8f, 1f); // Cyan
        tTxt.fontStyle = TMPro.FontStyles.Bold;
        
        RectTransform tRt = titleObj.GetComponent<RectTransform>();
        tRt.anchorMin = new Vector2(0, 0.85f); tRt.anchorMax = new Vector2(1, 1);
        tRt.offsetMin = Vector2.zero; tRt.offsetMax = Vector2.zero;

        // Close Button
        GameObject closeObj = new GameObject("Btn_Close");
        closeObj.transform.SetParent(container.transform, false);
        var clBtn = closeObj.AddComponent<UnityEngine.UI.Button>();
        var clImg = closeObj.AddComponent<UnityEngine.UI.Image>();
        clImg.color = Color.red;
        
        var clRt = closeObj.GetComponent<RectTransform>();
        clRt.anchorMin = new Vector2(0.92f, 0.92f); clRt.anchorMax = new Vector2(0.98f, 0.98f);
        
        GameObject clTxtObj = new GameObject("Text");
        clTxtObj.transform.SetParent(closeObj.transform, false);
        var clTxt = clTxtObj.AddComponent<TMPro.TextMeshProUGUI>();
        if (uiFont != null) clTxt.font = uiFont; // [FIX]
        clTxt.text = "X"; clTxt.color = Color.white; clTxt.alignment = TMPro.TextAlignmentOptions.Center;
        
        clBtn.onClick.AddListener(()=> Destroy(popup));

        // Package Items (Grid)
        GameObject gridObj = new GameObject("Grid");
        gridObj.transform.SetParent(container.transform, false);
        var grid = gridObj.AddComponent<UnityEngine.UI.GridLayoutGroup>();
        grid.cellSize = new Vector2(300, 180);
        grid.spacing = new Vector2(50, 50);
        grid.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 2;
        grid.childAlignment = TextAnchor.MiddleCenter;

        RectTransform gRt = gridObj.GetComponent<RectTransform>();
        gRt.anchorMin = new Vector2(0.1f, 0.1f); gRt.anchorMax = new Vector2(0.9f, 0.8f);
        gRt.offsetMin = Vector2.zero; gRt.offsetMax = Vector2.zero;

        // Define Packages
        var packages = new[] {
            new { amount = 1000, price = 1000, name = "Starter Pack" },
            new { amount = 3000, price = 3000, name = "Basic Pack" },
            new { amount = 5000, price = 5000, name = "Pro Pack" },
            new { amount = 10000, price = 10000, name = "Mega Pack" },
        };

        foreach(var pkg in packages)
        {
            GameObject pObj = new GameObject($"Pkg_{pkg.amount}");
            pObj.transform.SetParent(gridObj.transform, false);
            var pImg = pObj.AddComponent<UnityEngine.UI.Image>();
            pImg.color = new Color(0.25f, 0.25f, 0.35f);
            var pBtn = pObj.AddComponent<UnityEngine.UI.Button>();

            // Amount Text
            GameObject aObj = new GameObject("Amount");
            aObj.transform.SetParent(pObj.transform, false);
            var aTxt = aObj.AddComponent<TMPro.TextMeshProUGUI>();
            if (uiFont != null) aTxt.font = uiFont; // [FIX]
            aTxt.text = $"{pkg.amount:N0} Coins";
            aTxt.fontSize = 32; aTxt.color = Color.yellow; 
            aTxt.alignment = TMPro.TextAlignmentOptions.Center;
            RectTransform aRt = aObj.GetComponent<RectTransform>();
            aRt.anchorMin = new Vector2(0, 0.5f); aRt.anchorMax = new Vector2(1, 0.9f);

            // Price Text
            GameObject prObj = new GameObject("Price");
            prObj.transform.SetParent(pObj.transform, false);
            var prTxt = prObj.AddComponent<TMPro.TextMeshProUGUI>();
            if (uiFont != null) prTxt.font = uiFont; // [FIX]
            prTxt.text = $"{pkg.price:N0} KRW";
            prTxt.fontSize = 24; prTxt.color = Color.white;
            prTxt.alignment = TMPro.TextAlignmentOptions.Center;
            RectTransform prRt = prObj.GetComponent<RectTransform>();
            prRt.anchorMin = new Vector2(0, 0.1f); prRt.anchorMax = new Vector2(1, 0.5f);

            pBtn.onClick.AddListener(() => {
                Debug.Log($"[Shop] Requesting Payment: {pkg.price} KRW");
                
                #if UNITY_WEBGL && !UNITY_EDITOR
                // WebGL 빌드: React 앱에 메시지 전송
                string js = $@"
                    window.parent.postMessage({{
                        type: 'OPEN_PAYMENT',
                        amount: {pkg.price},
                        orderName: '{pkg.name}'
                    }}, '*');
                ";
                #pragma warning disable 618
                Application.ExternalEval(js);
                #pragma warning restore 618
                #else
                // 일반 빌드 (Windows/Mac): 브라우저로 결제 페이지 열기
                string userId = "";
                if (NetworkManager.Instance != null && NetworkManager.Instance.IsLoggedIn)
                {
                    // 로그인된 사용자 ID 가져오기 (구현 필요)
                    userId = NetworkManager.Instance.CurrentUserId ?? "";
                }
                
                // 패키지 ID 매핑
                string packageId = pkg.amount switch
                {
                    1000 => "pack_100",
                    3000 => "pack_500",
                    5000 => "pack_1000",
                    10000 => "pack_5000",
                    _ => "pack_100"
                };
                
                string paymentUrl = $"http://localhost:5173/payment?package={packageId}";
                Debug.Log($"[Shop] Opening payment URL: {paymentUrl}");
                Application.OpenURL(paymentUrl);
                #endif
                
                Destroy(popup); // Close popup
            });
        }
    }

    // [NEW] Helper for Parsing JSON Arrays
    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            string newJson = "{ \"array\": " + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
            return wrapper.array;
        }

        [System.Serializable]
        private class Wrapper<T>
        {
            public T[] array;
        }
    }

    [System.Serializable]
    public class RankEntry
    {
        public int id;
        public string user_name;  // Supabase column name
        public string song_name;  // Supabase column name
        public int score;
        public int max_combo;
        public int perfect;
        public int great;
        public int bad;
        public int miss;
        public string created_at;
    }

    // [NEW] Rhythm Game Style Song Card
    private void CreateRhythmSongCard(Transform parent, string titleStr, bool isUnlocked, int index)
    {
        GameObject card = new GameObject($"SongCard_{index}");
        card.transform.SetParent(parent, false);

        // Card Size
        var le = card.AddComponent<UnityEngine.UI.LayoutElement>();
        le.preferredWidth = 280;
        le.preferredHeight = 380;

        // === CARD BACKGROUND (Neon Border Effect) ===
        var cardBg = card.AddComponent<UnityEngine.UI.Image>();
        cardBg.color = isUnlocked ? new Color(0f, 0.8f, 1f, 0.3f) : new Color(0.3f, 0.3f, 0.3f, 0.5f); // Cyan glow or gray

        // === INNER CARD ===
        GameObject innerCard = new GameObject("Inner");
        innerCard.transform.SetParent(card.transform, false);
        var innerImg = innerCard.AddComponent<UnityEngine.UI.Image>();
        innerImg.color = new Color(0.05f, 0.05f, 0.12f, 1f); // Dark blue
        var innerRt = innerCard.GetComponent<RectTransform>();
        innerRt.anchorMin = Vector2.zero; innerRt.anchorMax = Vector2.one;
        innerRt.offsetMin = new Vector2(4, 4); innerRt.offsetMax = new Vector2(-4, -4);

        // === ALBUM ART ===
        GameObject artObj = new GameObject("AlbumArt");
        artObj.transform.SetParent(innerCard.transform, false);
        var artImg = artObj.AddComponent<UnityEngine.UI.Image>();
        artImg.preserveAspect = true;
        artImg.raycastTarget = false;

        // Load album cover
        if (isUnlocked && albumCovers != null && index < albumCovers.Length && albumCovers[index] != null)
        {
            artImg.sprite = albumCovers[index];
            artImg.color = Color.white;
        }
        else
        {
            artImg.color = isUnlocked ? new Color(0.1f, 0.2f, 0.4f) : new Color(0.15f, 0.15f, 0.15f);
        }

        var artRt = artObj.GetComponent<RectTransform>();
        artRt.anchorMin = new Vector2(0.05f, 0.30f); artRt.anchorMax = new Vector2(0.95f, 0.95f);
        artRt.offsetMin = Vector2.zero; artRt.offsetMax = Vector2.zero;

        // === SONG TITLE ===
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(innerCard.transform, false);
        var titleTxt = titleObj.AddComponent<TMPro.TextMeshProUGUI>();
        if (uiFont) titleTxt.font = uiFont;
        titleTxt.text = titleStr;
        titleTxt.fontSize = 22;
        titleTxt.fontStyle = TMPro.FontStyles.Bold;
        titleTxt.alignment = TMPro.TextAlignmentOptions.Center;
        titleTxt.color = isUnlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f);
        titleTxt.raycastTarget = false;
        titleTxt.enableWordWrapping = false;
        titleTxt.overflowMode = TMPro.TextOverflowModes.Ellipsis;

        var titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0, 0.15f); titleRt.anchorMax = new Vector2(1, 0.28f);
        titleRt.offsetMin = new Vector2(5, 0); titleRt.offsetMax = new Vector2(-5, 0);

        // === DIFFICULTY / BPM INFO ===
        string infoStr = "";
        if (titleStr.Contains("GALAXIAS")) infoStr = "★★★☆☆ | 155 BPM";
        else if (titleStr.Contains("SODAPOP")) infoStr = "★★★★☆ | 128 BPM";
        else infoStr = "???";

        GameObject infoObj = new GameObject("Info");
        infoObj.transform.SetParent(innerCard.transform, false);
        var infoTxt = infoObj.AddComponent<TMPro.TextMeshProUGUI>();
        if (uiFont) infoTxt.font = uiFont;
        infoTxt.text = infoStr;
        infoTxt.fontSize = 16;
        infoTxt.alignment = TMPro.TextAlignmentOptions.Center;
        infoTxt.color = isUnlocked ? new Color(0f, 1f, 1f, 0.8f) : new Color(0.4f, 0.4f, 0.4f);
        infoTxt.raycastTarget = false;

        var infoRt = infoObj.GetComponent<RectTransform>();
        infoRt.anchorMin = new Vector2(0, 0.02f); infoRt.anchorMax = new Vector2(1, 0.14f);
        infoRt.offsetMin = Vector2.zero; infoRt.offsetMax = Vector2.zero;

        // === LOCK OVERLAY ===
        if (!isUnlocked)
        {
            GameObject lockOverlay = new GameObject("LockOverlay");
            lockOverlay.transform.SetParent(innerCard.transform, false);
            var lockImg = lockOverlay.AddComponent<UnityEngine.UI.Image>();
            lockImg.color = new Color(0, 0, 0, 0.7f);
            lockImg.raycastTarget = false;
            var lockRt = lockOverlay.GetComponent<RectTransform>();
            lockRt.anchorMin = new Vector2(0.05f, 0.30f); lockRt.anchorMax = new Vector2(0.95f, 0.95f);
            lockRt.offsetMin = Vector2.zero; lockRt.offsetMax = Vector2.zero;

            GameObject lockIcon = new GameObject("LockIcon");
            lockIcon.transform.SetParent(lockOverlay.transform, false);
            var lockTxt = lockIcon.AddComponent<TMPro.TextMeshProUGUI>();
            lockTxt.text = "🔒";
            lockTxt.fontSize = 48;
            lockTxt.alignment = TMPro.TextAlignmentOptions.Center;
            lockTxt.raycastTarget = false;
            var lockIconRt = lockIcon.GetComponent<RectTransform>();
            lockIconRt.anchorMin = Vector2.zero; lockIconRt.anchorMax = Vector2.one;
            lockIconRt.offsetMin = Vector2.zero; lockIconRt.offsetMax = Vector2.zero;
        }

        // === BUTTON INTERACTION ===
        var btn = card.AddComponent<UnityEngine.UI.Button>();
        btn.interactable = isUnlocked;

        UnityEngine.UI.ColorBlock colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.3f, 1.3f, 1.3f);
        colors.pressedColor = new Color(0.9f, 0.9f, 0.9f);
        colors.disabledColor = new Color(0.6f, 0.6f, 0.6f);
        btn.colors = colors;

        // Selection callback
        int capturedIndex = index;
        string capturedTitle = titleStr;
        btn.onClick.AddListener(() =>
        {
            if (isUnlocked)
            {
                _selectedSongTitle = capturedTitle;
                Debug.Log($"[SongSelect] Selected: {capturedTitle}");

                // Update info panel
                var infoPanelTxt = _cachedSongPanel?.transform.Find("SongInfoPanel/InfoText")?.GetComponent<TMPro.TextMeshProUGUI>();
                if (infoPanelTxt != null)
                {
                    if (capturedTitle.Contains("GALAXIAS"))
                        infoPanelTxt.text = "BPM: 155  |  Difficulty: ★★★☆☆  |  Artist: Galantis";
                    else if (capturedTitle.Contains("SODAPOP"))
                        infoPanelTxt.text = "BPM: 128  |  Difficulty: ★★★★☆  |  Artist: Soda Pop";
                }

                // Visual feedback - highlight selected card
                HighlightSelectedCard(parent, capturedIndex);
            }
        });

        // Set default selection
        if (index == 0 && isUnlocked)
        {
            _selectedSongTitle = titleStr;
        }
    }

    // Helper: Highlight selected card
    private void HighlightSelectedCard(Transform parent, int selectedIndex)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            var cardObj = parent.GetChild(i).gameObject;
            var cardImg = cardObj.GetComponent<UnityEngine.UI.Image>();
            if (cardImg != null)
            {
                if (i == selectedIndex)
                {
                    cardImg.color = new Color(0f, 1f, 1f, 0.6f); // Bright cyan for selected
                }
                else
                {
                    var btn = cardObj.GetComponent<UnityEngine.UI.Button>();
                    bool unlocked = btn != null && btn.interactable;
                    cardImg.color = unlocked ? new Color(0f, 0.8f, 1f, 0.3f) : new Color(0.3f, 0.3f, 0.3f, 0.5f);
                }
            }
        }
    }

    // Cached panel reference for song card callbacks
    private GameObject _cachedSongPanel => songSelectPanel;

    // [NEW] Enhanced Song Card with Info (BPM, Composer)
    private void CreateSongCardWithInfo(Transform parent, string titleStr, bool isUnlocked, int index)
    {
        GameObject card = new GameObject($"Card_{index}");
        card.transform.SetParent(parent, false);
        
        // Background
        var img = card.AddComponent<UnityEngine.UI.Image>();
        img.color = isUnlocked ? new Color(0.95f, 0.95f, 0.95f) : new Color(0.3f, 0.3f, 0.3f);
        
        var btn = card.AddComponent<UnityEngine.UI.Button>();
        btn.interactable = isUnlocked;
        
        var le = card.AddComponent<UnityEngine.UI.LayoutElement>();
        le.preferredWidth = 350; le.preferredHeight = 480;
        
        // Art - Use album covers if available
        GameObject artObj = new GameObject("Art");
        artObj.transform.SetParent(card.transform, false);
        var artImg = artObj.AddComponent<UnityEngine.UI.Image>();

        // Try to load album cover
        if (isUnlocked && albumCovers != null && index < albumCovers.Length && albumCovers[index] != null)
        {
            artImg.sprite = albumCovers[index];
            artImg.color = Color.white;
            artImg.preserveAspect = true;
        }
        else
        {
            // Fallback: Try to load from Resources
            string resourceName = "";
            if (titleStr.Contains("GALAXIAS")) resourceName = "galaxias_cover";
            else if (titleStr.Contains("SODAPOP")) resourceName = "sodapop_cover";

            Sprite coverSprite = Resources.Load<Sprite>(resourceName);
            if (coverSprite != null)
            {
                artImg.sprite = coverSprite;
                artImg.color = Color.white;
                artImg.preserveAspect = true;
            }
            else
            {
                // Final fallback: colored placeholder
                artImg.color = isUnlocked ? new Color(0.1f, 0.15f, 0.3f) : Color.black;
            }
        }

        RectTransform artRt = artObj.GetComponent<RectTransform>();
        artRt.anchorMin = new Vector2(0, 0.35f); artRt.anchorMax = new Vector2(1, 1);
        artRt.offsetMin = new Vector2(10, 0); artRt.offsetMax = new Vector2(-10, -10);

        // Title
        GameObject tObj = new GameObject("Title");
        tObj.transform.SetParent(card.transform, false);
        var tTxt = tObj.AddComponent<TMPro.TextMeshProUGUI>();
        if(uiFont) tTxt.font = uiFont;
        tTxt.text = titleStr;
        tTxt.fontSize = 26; tTxt.alignment = TMPro.TextAlignmentOptions.Center;
        tTxt.fontStyle = TMPro.FontStyles.Bold;
        tTxt.color = isUnlocked ? Color.black : Color.gray;
        
        RectTransform tRt = tObj.GetComponent<RectTransform>();
        tRt.anchorMin = new Vector2(0, 0.2f); tRt.anchorMax = new Vector2(1, 0.35f);
        tRt.offsetMin = Vector2.zero; tRt.offsetMax = Vector2.zero;

        // [NEW] Info (BPM | Composer)
        string infoStr = "120 BPM | Unknown";
        if (titleStr.Contains("GALAXIAS")) infoStr = "155 BPM | Galantis";
        else if (titleStr.Contains("SODAPOP")) infoStr = "128 BPM | Soda P";
        
        GameObject iObj = new GameObject("Info");
        iObj.transform.SetParent(card.transform, false);
        var iTxt = iObj.AddComponent<TMPro.TextMeshProUGUI>();
        if(uiFont) iTxt.font = uiFont;
        iTxt.text = infoStr;
        iTxt.fontSize = 18; iTxt.alignment = TMPro.TextAlignmentOptions.Center;
        iTxt.color = new Color(0.4f, 0.4f, 0.4f);
        
        RectTransform iRt = iObj.GetComponent<RectTransform>();
        iRt.anchorMin = new Vector2(0, 0.1f); iRt.anchorMax = new Vector2(1, 0.2f);
        iRt.offsetMin = Vector2.zero; iRt.offsetMax = Vector2.zero;

        // Lock Icon if needed
        if (!isUnlocked)
        {
            GameObject lObj = new GameObject("Lock");
            lObj.transform.SetParent(card.transform, false);
            var lTxt = lObj.AddComponent<TMPro.TextMeshProUGUI>();
            lTxt.text = "🔒"; lTxt.fontSize = 60; lTxt.alignment = TMPro.TextAlignmentOptions.Center;
            lObj.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            lObj.GetComponent<RectTransform>().anchorMax = Vector2.one;
            lObj.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
        }

        // Selection Event
        btn.onClick.AddListener(() => {
            if(isUnlocked) Debug.Log($"Selected {titleStr}");
        });
    }
}

