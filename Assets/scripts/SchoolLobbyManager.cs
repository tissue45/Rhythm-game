using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class SchoolLobbyManager : MonoBehaviour
{
    [Header("Song Options")]
    public Sprite[] albumCovers; // [NEW] User assignable album covers
    public Sprite mainLogo; // [NEW] Main Logo to replace text

    public static SchoolLobbyManager Instance { get; private set; }

    [Header("UI Panels (Optional)")]
    public GameObject songSelectPanel; // [NEW] Song Selection
    public GameObject rankingPanel;
    public GameObject customizationPanel;
    public GameObject shopPanel;
    public GameObject profilePanel;
    public GameObject optionsPanel;

    private LobbyManager _originalLobbyManager;

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
    }


    private void BindButtonEvents()
    {
        // [FIX] ?�?불일?문제 ?�?(Btn_GameStart vs Btn_Start)
        BindButton("Btn_GameStart", OnGameStartClick);
        BindButton("Btn_Start", OnGameStartClick); // ?? 로그??????�?
        BindButton("StartButton", OnGameStartClick); // ???몰라??�?

        BindButton("Btn_Ranking", OnRankingClick);
        BindButton("Btn_Shop", OnShopClick);
        // BindButton("Btn_Profile", OnProfileClick); // [REMOVED] User Request
        BindButton("Btn_Option", OnOptionsClick);
        BindButton("Btn_Exit", () => Application.Quit());
        
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
        _originalLobbyManager = FindObjectOfType<LobbyManager>();

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
             
        var img = songSelectPanel.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(0,0,0,0.85f);
             
        RectTransform rt = songSelectPanel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
             
        // Close Button
        GameObject closeBtn = new GameObject("CloseButton");
        closeBtn.transform.SetParent(songSelectPanel.transform, false);
        var btnImg = closeBtn.AddComponent<UnityEngine.UI.Image>();
        btnImg.color = Color.red;
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
        CreateVersionDisplay();
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
        versionText.text = "v1.1.0 | Build: 6859ebf | 2026-01-13\n<size=10>?�� Audio Separation Fix Applied</size>";
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
        // 1. TitleText 처리
        GameObject titleText = GameObject.Find("TitleText");
        if (titleText != null)
        {
            var g = titleText.GetComponent<UnityEngine.UI.Graphic>();
            if (g != null) g.raycastTarget = false;
        }

        // 2. 모든 ????TMP, Legacy Text)??Raycast ?�?(Inactive ???
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
        // ... (Debug code remains) ...
        // [DEBUG] 마우???�???무엇???�???? 로그 출력 (UI ?버깅??
        if (Input.GetMouseButtonDown(0))
        {
            if (UnityEngine.EventSystems.EventSystem.current == null) return;

            var pointerData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
            pointerData.position = Input.mousePosition;

            var results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
            UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointerData, results);

            if (results.Count > 0)
            {
                GameObject hitObj = results[0].gameObject;
                Debug.Log($"[UI DEBUG] Clicked on: {hitObj.name}");
                
                // [FIX] ???? ?�??가로채????즉시 꺼버리고 ???????
                if (hitObj.name.Contains("Text") || hitObj.GetComponent<TMPro.TextMeshProUGUI>() != null || hitObj.GetComponent<UnityEngine.UI.Text>() != null)
                {
                    var g = hitObj.GetComponent<UnityEngine.UI.Graphic>();
                    if (g != null && g.raycastTarget)
                    {
                        g.raycastTarget = false;
                        Debug.LogWarning($"[UI BLOCKER FOUND] Disabled Raycast on '{hitObj.name}'. CLICK AGAIN!");
                        
                        // (?????? 부?버튼??찾아??강제 ??? -> ??????????가 ????�?�???
                        // ???????경험?????부모의 버튼??찾아??Execute ????????
                        var parentBtn = hitObj.GetComponentInParent<UnityEngine.UI.Button>();
                        if (parentBtn != null)
                        {
                            Debug.Log($"[UI AUTO-FIX] Executing parent button '{parentBtn.name}' instead...");
                            parentBtn.onClick.Invoke();
                        }
                    }
                }
            }
        }
    }

    private System.Collections.IEnumerator SeekCharacterAnimations()
    {
        // [FIX] Main ???�?????????
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Main") yield break;

        yield return new WaitForSeconds(0.5f);

        string[] charNames = { "minA_1", "Minwoo_1" };
        foreach (string name in charNames)
        {
            GameObject obj = GameObject.Find(name);
            if (obj != null)
            {
                Animator anim = obj.GetComponentInChildren<Animator>();
                
                // [FIX] Animator가 ??????�??�?
                if (anim == null)
                {
                    Debug.LogWarning($"[SchoolLobbyManager] No Animator found on {name}. Adding one automatically.");
                    anim = obj.AddComponent<Animator>();
                    // Avatar??Controller가 ????T-Pose??????? ???컴포????존재?�???
                }

                if (anim != null)
                {
                    // Animator가 꺼져???켜주?
                    if (!anim.enabled) anim.enabled = true;
                    if (!anim.gameObject.activeInHierarchy) anim.gameObject.SetActive(true);
                    
                    // Controller ???
                    if (anim.runtimeAnimatorController == null)
                    {
                        Debug.LogWarning($"[SchoolLobbyManager] {name} found but NO Controller assigned!");
                        // ?�??????????�???????(Resource 로드 ??
                    }
                }

                if (anim != null)
                {
                    // ??????중인 ????�?가???
                    AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);
                    
                    Debug.Log($"[SchoolLobbyManager] Found Animator on {anim.gameObject.name}. State Length: {state.length}, Loop: {state.loop}");

                    if (state.length > 0)
                    {
                        float offset = 20f;
                        // 0~1 ?????�????�?Normalized Time)?�?변??
                        float normalizedTime = (offset % state.length) / state.length;
                        
                        // 강제?????�????
                        anim.Play(state.fullPathHash, 0, normalizedTime);
                        
                        // ???Play가 ??먹힐 경우??비해 Update???�?
                        anim.Update(0f);
                        
                        Debug.Log($"[SchoolLobbyManager] {name} seeked to {normalizedTime} (20s)");
                    }
                    else
                    {
                        // 길이가 0?�??�????��? ?�?Transition 중이거나 초기?????
                        // 강제?"Base Layer.Motion" �? ?�?�???????�??????
                        // MMD4Mecanim? 보통 기본 ??????????
                        anim.Play(0, -1, 0.2f); // 20% 지???강제 ??????(???
                        Debug.LogWarning($"[SchoolLobbyManager] {name} State length is 0. Tried forcing normalized time 0.2");
                    }
                }
                else
                {
                    Debug.LogWarning($"[SchoolLobbyManager] Found {name} but NO Animator found in children!");
                    // ????�???리스??출력
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
        if (songSelectPanel != null) 
        {
            Debug.Log($"[SchoolLobbyManager] Toggling Song Selection Panel: {songSelectPanel.name}");
            TogglePanel(songSelectPanel);
        }
        else
        {
            Debug.LogError("[SchoolLobbyManager] Song Selection Panel is NULL!");
        }
    }

    public void OnRankingClick()
    {
        Debug.Log("[SchoolLobbyManager] Ranking Clicked!");
        TogglePanel(rankingPanel);
    }

    public void OnShopClick()
    {
        Debug.Log("[SchoolLobbyManager] Shop Clicked!");
        TogglePanel(shopPanel);
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
    private string _selectedSongTitle = "GALAXIAS!"; // 기본?

    // [NEW] Real Game Start (called from Song Select Panel)
    public void StartGameLoop()
    {
        Debug.Log($"[SchoolLobbyManager] Starting Game Loop... Selected Song: {_selectedSongTitle}");
        
        // [FIX] ????곡에 ?????분기
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
                    rt.anchorMin = new Vector2(0.15f, 0.15f);
                    rt.anchorMax = new Vector2(0.85f, 0.85f);
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;
                }

                BindCloseButton(panel);

                if (panel == shopPanel) PopulateShop(panel);
                if (panel == songSelectPanel) PopulateSongSelect(panel);
                if (panel == rankingPanel) PopulateRanking(panel);
                if (panel == optionsPanel) PopulateOptions(panel); // [NEW]
            }
        }
        else
        {
            Debug.LogWarning("Panel not assigned in Inspector.");
        }
    }

    // SetMainMenuVisible 메서???????????? ??��?????�??비워??
    private void SetMainMenuVisible(bool visible) { }

    private void PopulateShop(GameObject panel)
    {
        Debug.Log("[Lobby] Populating Shop Logic... (Redesigned)");

        foreach (Transform child in panel.transform) Destroy(child.gameObject);

        // 1. Background
        var bgImg = panel.GetComponent<UnityEngine.UI.Image>();
        if (bgImg == null) bgImg = panel.AddComponent<UnityEngine.UI.Image>();
        bgImg.color = new Color(0, 0, 0, 0.95f);

        // 2. Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panel.transform, false);
        var titleTxt = titleObj.AddComponent<TMPro.TextMeshProUGUI>();
        titleTxt.text = "ITEM SHOP";
        titleTxt.fontSize = 48;
        titleTxt.fontStyle = TMPro.FontStyles.Bold;
        titleTxt.alignment = TMPro.TextAlignmentOptions.Center;
        titleTxt.color = new Color(0, 0.9f, 1f); // Cyan
        
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0, 0.88f); titleRt.anchorMax = new Vector2(1, 1);
        titleRt.offsetMin = Vector2.zero; titleRt.offsetMax = Vector2.zero;

        // 3. Coin Display (Top Left - Separated from Close Button)
        GameObject coinObj = new GameObject("CoinDisplay");
        coinObj.transform.SetParent(panel.transform, false);
        var coinTxt = coinObj.AddComponent<TMPro.TextMeshProUGUI>();
        
        // [DEBUG] Check GameManager existence
        if (GameManager.Instance == null)
        {
            Debug.LogError("[Shop] GameManager.Instance is NULL! Creating new GameManager...");
            GameObject gmObj = new GameObject("GameManager");
            gmObj.AddComponent<GameManager>();
            // Force coins initialization
            if (GameManager.Instance != null)
            {
                GameManager.Instance.coins = 5000;
                Debug.Log("[Shop] Created GameManager and set coins to 5000");
            }
        }
        else
        {
            Debug.Log($"[Shop] GameManager.Instance EXISTS. Current coins: {GameManager.Instance.coins}");
        }
        
        int currentCoins = (GameManager.Instance != null) ? GameManager.Instance.coins : 0;
        coinTxt.text = $"COINS: {currentCoins:N0} G";
        coinTxt.fontSize = 32;
        coinTxt.color = new Color(1f, 0.8f, 0.2f); // Gold
        coinTxt.alignment = TMPro.TextAlignmentOptions.Left; // Left align

        RectTransform coinRt = coinObj.GetComponent<RectTransform>();
        coinRt.anchorMin = new Vector2(0.05f, 0.88f); coinRt.anchorMax = new Vector2(0.4f, 0.98f); // Top Left
        coinRt.offsetMin = Vector2.zero; coinRt.offsetMax = Vector2.zero;

        // 4. Scroll Area
        GameObject scrollObj = new GameObject("ScrollArea");
        scrollObj.transform.SetParent(panel.transform, false);
        var scrollRt = scrollObj.AddComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0.1f, 0.1f); scrollRt.anchorMax = new Vector2(0.9f, 0.8f);
        scrollRt.offsetMin = Vector2.zero; scrollRt.offsetMax = Vector2.zero;

        var scroll = scrollObj.AddComponent<UnityEngine.UI.ScrollRect>();
        scroll.vertical = true; scroll.horizontal = false;
        scroll.movementType = UnityEngine.UI.ScrollRect.MovementType.Elastic;

        // Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollObj.transform, false);
        var vpRt = viewport.AddComponent<RectTransform>();
        vpRt.anchorMin = Vector2.zero; vpRt.anchorMax = Vector2.one;
        vpRt.offsetMin = Vector2.zero; vpRt.offsetMax = Vector2.zero;
        viewport.AddComponent<UnityEngine.UI.RectMask2D>();
        scroll.viewport = vpRt;

        // Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        var vlg = content.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
        vlg.padding = new RectOffset(10, 10, 10, 10);
        vlg.spacing = 15;
        vlg.childControlHeight = false; vlg.childControlWidth = true;
        vlg.childAlignment = TextAnchor.UpperCenter;
        
        var csf = content.AddComponent<UnityEngine.UI.ContentSizeFitter>();
        csf.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
        
        scroll.content = content.GetComponent<RectTransform>();

        // 6. Close Button (X) [MOVED UP for safety]
        GameObject closeBtnObj = new GameObject("Btn_Close");
        closeBtnObj.transform.SetParent(panel.transform, false);
        var closeImg = closeBtnObj.AddComponent<UnityEngine.UI.Image>();
        closeImg.color = new Color(0.8f, 0.2f, 0.2f, 1f); // Red
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
        clTxt.raycastTarget = false; // [FIX] Prevent blocking
        
        var clRt = closeTxtObj.GetComponent<RectTransform>();
        clRt.anchorMin = Vector2.zero; clRt.anchorMax = Vector2.one;
        clRt.offsetMin = Vector2.zero; clRt.offsetMax = Vector2.zero;
        
        // Ensure it stays on top
        closeBtnObj.transform.SetAsLastSibling(); 

        // 5. Items
        string[] items = { "Song: Hype Boy", "Song: Ditto", "Mode: HARD", "Skin: Neon", "Pack: K-Pop Vol.1" };
        int[] prices = { 1000, 1200, 3000, 500, 2000 };

        for(int i=0; i<items.Length; i++)
        {
            CreateShopItem(content.transform, items[i], prices[i], coinTxt);
        }
    }

    private void CreateShopItem(Transform parent, string name, int price, TMPro.TextMeshProUGUI coinRef)
    {
        GameObject item = new GameObject("ShopItem_" + name);
        item.transform.SetParent(parent, false);
        
        var le = item.AddComponent<UnityEngine.UI.LayoutElement>();
        le.preferredHeight = 100;
        
        var img = item.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(0.15f, 0.15f, 0.18f); // Dark strip

        // Horizontal Layout
        var hlg = item.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(20, 20, 10, 10);
        hlg.spacing = 20;
        hlg.childControlWidth = false; hlg.childControlHeight = false;
        hlg.childAlignment = TextAnchor.MiddleLeft;

        // Icon
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(item.transform, false);
        var iconImg = iconObj.AddComponent<UnityEngine.UI.Image>();
        iconImg.color = new Color(0, 0.5f, 0.8f); // Placeholder Blue
        var iconLe = iconObj.AddComponent<UnityEngine.UI.LayoutElement>();
        iconLe.preferredWidth = 80; iconLe.preferredHeight = 80;

        // Name
        GameObject nameObj = new GameObject("Name");
        nameObj.transform.SetParent(item.transform, false);
        var nameTxt = nameObj.AddComponent<TMPro.TextMeshProUGUI>();
        nameTxt.text = name;
        nameTxt.fontSize = 28;
        nameTxt.color = Color.white;
        nameTxt.alignment = TMPro.TextAlignmentOptions.Left;
        var nameLe = nameObj.AddComponent<UnityEngine.UI.LayoutElement>();
        nameLe.preferredWidth = 300; nameLe.preferredHeight = 40;

        // Buy Button
        GameObject btnObj = new GameObject("Btn_Buy");
        btnObj.transform.SetParent(item.transform, false);
        var btnImg = btnObj.AddComponent<UnityEngine.UI.Image>();
        var btn = btnObj.AddComponent<UnityEngine.UI.Button>();
        
        var btnLe = btnObj.AddComponent<UnityEngine.UI.LayoutElement>();
        btnLe.preferredWidth = 150; btnLe.preferredHeight = 60;

        // Button Text
        GameObject btnTxtObj = new GameObject("Text");
        btnTxtObj.transform.SetParent(btnObj.transform, false);
        var btnTxt = btnTxtObj.AddComponent<TMPro.TextMeshProUGUI>();
        btnTxt.fontSize = 24;
        btnTxt.alignment = TMPro.TextAlignmentOptions.Center;
        btnTxt.color = Color.black;
        btnTxt.raycastTarget = false; // [FIX]
        
        // [FIX] Use GetComponent instead of AddComponent to avoid duplicate component error
        var btnTxtRt = btnTxtObj.GetComponent<RectTransform>();
        btnTxtRt.anchorMin = Vector2.zero; btnTxtRt.anchorMax = Vector2.one;
        btnTxtRt.offsetMin = Vector2.zero; btnTxtRt.offsetMax = Vector2.zero;

        // Check Ownership
        bool isOwned = (GameManager.Instance != null && GameManager.Instance.ownedItems.Contains(name));

        if (isOwned)
        {
            btnTxt.text = "OWNED";
            btnImg.color = Color.gray;
            btn.interactable = false;
        }
        else
        {
            btnTxt.text = $"{price:N0} G";
            btnImg.color = new Color(0.2f, 1f, 0.4f); // Green
            btn.interactable = true;
        }

        // Logic
        // Logic
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => {
            if (GameManager.Instance != null && GameManager.Instance.coins >= price)
            {
                // Deduct
                GameManager.Instance.coins -= price;
                GameManager.Instance.ownedItems.Add(name);

                // Update UI
                btnTxt.text = "OWNED";
                btnImg.color = Color.gray;
                btn.interactable = false;
                
                // Update Coin Display
                if (coinRef != null) 
                    coinRef.text = $"COINS: {GameManager.Instance.coins:N0} G";
                
                Debug.Log($"[Shop] Successfully purchased {name}. Coins: {GameManager.Instance.coins}");
            }
            else
            {
                Debug.Log($"[Shop] Not enough coins! Price: {price}, Have: {GameManager.Instance?.coins}");
                // Optional: Feedback (Shake/Red Flash)
            }
        });
    }

    private void CreateSongCard(Transform parent, string title, bool unlock, int index)
    {
        if (parent == null) return;

        GameObject card = new GameObject("SongStrip_" + title);
        card.layer = LayerMask.NameToLayer("UI");
        card.transform.SetParent(parent, false);

        // Strip Background
        UnityEngine.UI.Image img = card.AddComponent<UnityEngine.UI.Image>();
        // Sleek Dark Strip with Cyan Highlight if unlocked
        img.color = unlock ? new Color(0.02f, 0.05f, 0.1f, 0.95f) : new Color(0.1f, 0.1f, 0.1f, 0.5f);

        RectTransform rt = card.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(550, 90); // Wider and slim
        rt.localScale = Vector3.one;
        rt.anchoredPosition = Vector3.zero;

        // Selection Highlight (Outline) - Optional simple implementation
        if(unlock)
        {
            var outlineObj = new GameObject("Outline");
            outlineObj.transform.SetParent(card.transform, false);
            var outImg = outlineObj.AddComponent<UnityEngine.UI.Image>();
            outImg.color = new Color(0, 1, 1, 0.1f); // Faint cyan glow
            var outRt = outlineObj.GetComponent<RectTransform>();
            outRt.anchorMin = Vector2.zero; outRt.anchorMax = Vector2.one;
            outRt.offsetMin = new Vector2(0,0); outRt.offsetMax = new Vector2(0,0);
        }

        // Title Text (Left Aligned, Modern Font Style)
        GameObject txtObj = new GameObject("Title");
        txtObj.transform.SetParent(card.transform, false);
        var txt = txtObj.AddComponent<TMPro.TextMeshProUGUI>();
        txt.text = unlock ? title.ToUpper() : title + " [LOCKED]";
        txt.alignment = TMPro.TextAlignmentOptions.Left; 
        txt.fontSize = 28;
        txt.fontStyle = TMPro.FontStyles.Bold;
        txt.color = unlock ? Color.white : Color.gray;
        txt.raycastTarget = false;
        
        RectTransform txtRt = txtObj.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero; 
        txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = new Vector2(30, 0); // Padding left
        txtRt.offsetMax = Vector2.zero;

        // Button Logic
        UnityEngine.UI.Button btn = card.AddComponent<UnityEngine.UI.Button>();
        UnityEngine.UI.ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(0, 0.8f, 1f, 0.2f); // Cyan Hover
        colors.pressedColor = new Color(0, 0.5f, 0.8f, 0.5f);
        btn.colors = colors;

        btn.onClick.AddListener(() => {
            if (unlock)
            {
                _selectedSongTitle = title;
                Debug.Log($"[Lobby] Selected: {title}");
                
                // Snap Carousel
                var cv = parent.GetComponentInParent<CarouselView>();
                if (cv != null) cv.SnapTo(index);
            }
        });
    }

    private void PopulateSongSelect(GameObject panel)
    {
        Debug.Log($"[Lobby] Layout: Polished DJMAX Style + Lock Logic");

        foreach (Transform child in panel.transform) Destroy(child.gameObject);

        // Background
        var bgImg = panel.GetComponent<UnityEngine.UI.Image>();
        if(bgImg == null) bgImg = panel.AddComponent<UnityEngine.UI.Image>();
        bgImg.color = new Color(0, 0, 0, 0.95f);

        // Left Panel
        GameObject leftPanel = new GameObject("LeftDetailPanel");
        leftPanel.transform.SetParent(panel.transform, false);
        RectTransform leftRt = leftPanel.AddComponent<RectTransform>();
        leftRt.anchorMin = new Vector2(0.05f, 0.1f); leftRt.anchorMax = new Vector2(0.45f, 0.9f); 
        leftRt.offsetMin = Vector2.zero; leftRt.offsetMax = Vector2.zero;
        
        // Album Art
        GameObject artObj = new GameObject("AlbumArt");
        artObj.transform.SetParent(leftPanel.transform, false);
        var artImg = artObj.AddComponent<UnityEngine.UI.Image>();
        artImg.color = new Color(0.2f, 0.2f, 0.2f);
        artImg.preserveAspect = true;
        RectTransform artRt = artObj.GetComponent<RectTransform>();
        artRt.anchorMin = new Vector2(0, 0.3f); artRt.anchorMax = new Vector2(1, 1);
        artRt.offsetMin = Vector2.zero; artRt.offsetMax = Vector2.zero;

        // Title
        GameObject titleObj = new GameObject("DetailTitle");
        titleObj.transform.SetParent(leftPanel.transform, false);
        var titleTxt = titleObj.AddComponent<TMPro.TextMeshProUGUI>();
        titleTxt.text = "SELECT TRACK";
        titleTxt.fontSize = 42;
        titleTxt.fontStyle = TMPro.FontStyles.Bold;
        titleTxt.alignment = TMPro.TextAlignmentOptions.Center;
        titleTxt.color = new Color(0, 0.9f, 1f);
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0, 0.2f); titleRt.anchorMax = new Vector2(1, 0.3f);
        titleRt.offsetMin = Vector2.zero; titleRt.offsetMax = Vector2.zero;

        // Start Button
        GameObject startBtnObj = new GameObject("Btn_StartGame");
        startBtnObj.transform.SetParent(leftPanel.transform, false);
        var startImg = startBtnObj.AddComponent<UnityEngine.UI.Image>();
        startImg.color = new Color(0, 0.7f, 0.9f);
        var startBtn = startBtnObj.AddComponent<UnityEngine.UI.Button>();
        RectTransform startRt = startBtnObj.GetComponent<RectTransform>();
        startRt.anchorMin = new Vector2(0.1f, 0); startRt.anchorMax = new Vector2(0.9f, 0.15f);
        startRt.offsetMin = Vector2.zero; startRt.offsetMax = Vector2.zero;
        
        GameObject stTxtObj = new GameObject("Text");
        stTxtObj.transform.SetParent(startBtnObj.transform, false);
        var stTxt = stTxtObj.AddComponent<TMPro.TextMeshProUGUI>();
        stTxt.text = "GAME START";
        stTxt.fontSize = 32;
        stTxt.alignment = TMPro.TextAlignmentOptions.Center;
        stTxt.color = Color.black; 
        var stRt = stTxtObj.GetComponent<RectTransform>();
        stRt.anchorMin = Vector2.zero; stRt.anchorMax = Vector2.one;
        stRt.offsetMin = Vector2.zero; stRt.offsetMax = Vector2.zero;

        startBtn.onClick.AddListener(()=> { StartGameLoop(); });

        // Right Panel
        GameObject rightPanel = new GameObject("CarouselContainer");
        rightPanel.transform.SetParent(panel.transform, false);
        RectTransform rightRt = rightPanel.AddComponent<RectTransform>();
        rightRt.anchorMin = new Vector2(0.5f, 0.1f); rightRt.anchorMax = new Vector2(0.95f, 0.9f);
        rightRt.offsetMin = Vector2.zero; rightRt.offsetMax = Vector2.zero;

        // Close Button
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
        var clRt = closeTxtObj.GetComponent<RectTransform>();
        clRt.anchorMin = Vector2.zero; clRt.anchorMax = Vector2.one;
        clRt.offsetMin = Vector2.zero; clRt.offsetMax = Vector2.zero;

        // Scroll Logic
        var scroll = rightPanel.AddComponent<UnityEngine.UI.ScrollRect>();
        rightPanel.AddComponent<UnityEngine.UI.Image>().color = new Color(0,0,0,0);
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(rightPanel.transform, false);
        var contentRt = contentObj.AddComponent<RectTransform>();
        contentRt.sizeDelta = new Vector2(500, 1200);
        scroll.content = contentRt;
        scroll.horizontal = false; scroll.vertical = true;
        scroll.movementType = UnityEngine.UI.ScrollRect.MovementType.Elastic;

        var cv = rightPanel.AddComponent<CarouselView>();
        cv.container = contentRt;
        cv.spacing = 110f; 
        cv.scaleFactor = 1.0f;
        cv.isVertical = true;

        string[] songs = { "GALAXIAS!", "Sodapop", "Coming Soon" };
        for(int i=0; i<songs.Length; i++)
        {
            CreateSongCard(contentRt, songs[i], i <= 1, i);
        }

        cv.Invoke("InitializeItems", 0.1f);
        cv.OnItemSelected = (index) => {
            if (index >= 0 && index < songs.Length)
            {
                string s = songs[index];
                _selectedSongTitle = s;
                titleTxt.text = s.ToUpper();
                
                if (albumCovers != null && index < albumCovers.Length && albumCovers[index] != null)
                {
                    artImg.sprite = albumCovers[index];
                    artImg.color = Color.white;
                }
                else
                {
                    if(s == "GALAXIAS!") artImg.color = new Color(1, 0, 0.5f);
                    else if(s == "Sodapop") artImg.color = new Color(0, 0.5f, 1f);
                    else artImg.color = new Color(0.2f, 0.2f, 0.2f);
                }

                // [FIX] LOCK LOGIC
                bool isLocked = (s == "Coming Soon" || s.Contains("Locked"));
                startBtn.interactable = !isLocked;
                stTxt.text = isLocked ? "LOCKED" : "GAME START";
                startImg.color = isLocked ? Color.gray : new Color(0, 0.7f, 0.9f);
                stTxt.color = isLocked ? new Color(0.3f, 0.3f, 0.3f) : Color.black;
            }
        };
    }


    private void BindSongSelectStartButton(GameObject panel)
    {
        // "StartButton" ???"Btn_Start" 찾기
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
        if (songSelectPanel != null) songSelectPanel.SetActive(false);
        if (rankingPanel != null) rankingPanel.SetActive(false);
        if (shopPanel != null) shopPanel.SetActive(false);
        if (profilePanel != null) profilePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (customizationPanel != null) customizationPanel.SetActive(false);
    }

    private void BindCloseButton(GameObject panel)
    {
        // ???????"CloseButton" ?�??가?버튼 찾기 (?????
        // 만약 ?�??"Close"?�?"Close"?찾으??? 보통 "CloseButton"???"Btn_Close" ??????????
        // ?�???"CloseButton" ???"Btn_Close" ?????????
        
        UnityEngine.UI.Button closeBtn = panel.GetComponentInChildren<UnityEngine.UI.Button>(); // ????�?버튼????????????(보통 ?�???�뿐???????
        
        // ?�?�?????찾고 ???
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
            // ?�?버튼?????? ?�??�? ?�????
            var txt = closeBtn.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (txt != null) txt.raycastTarget = false;
            var txt2 = closeBtn.GetComponentInChildren<UnityEngine.UI.Text>();
            if (txt2 != null) txt2.raycastTarget = false;

            // ?�???�?
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

    private void PopulateRanking(GameObject panel)
    {
        Debug.Log("[Lobby] Populating Ranking...");

        foreach (Transform child in panel.transform) Destroy(child.gameObject);

        // Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panel.transform, false);
        var titleTxt = titleObj.AddComponent<TMPro.TextMeshProUGUI>();
        titleTxt.text = "RANKING";
        titleTxt.fontSize = 50;
        titleTxt.fontStyle = TMPro.FontStyles.Bold;
        titleTxt.alignment = TMPro.TextAlignmentOptions.Center;
        titleTxt.color = Color.white;
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0, 0.85f); titleRt.anchorMax = new Vector2(1, 1);
        titleRt.offsetMin = Vector2.zero; titleRt.offsetMax = Vector2.zero;

        // Container (Scroll View)
        GameObject scrollObj = new GameObject("ScrollArea");
        scrollObj.transform.SetParent(panel.transform, false);
        var scrollRt = scrollObj.AddComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0.1f, 0.1f); scrollRt.anchorMax = new Vector2(0.9f, 0.8f);
        scrollRt.offsetMin = Vector2.zero; scrollRt.offsetMax = Vector2.zero;
        
        scrollObj.AddComponent<UnityEngine.UI.Image>().color = new Color(0,0,0,0.5f);
        var scroll = scrollObj.AddComponent<UnityEngine.UI.ScrollRect>();
        
        GameObject content = new GameObject("Content");
        content.transform.SetParent(scrollObj.transform, false);
        var vlg = content.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
        vlg.padding = new RectOffset(20, 20, 20, 20);
        vlg.spacing = 15;
        vlg.childControlHeight = false; vlg.childControlWidth = true;
        vlg.childAlignment = TextAnchor.UpperCenter;
        
        var csf = content.AddComponent<UnityEngine.UI.ContentSizeFitter>();
        csf.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;

        scroll.content = content.GetComponent<RectTransform>();
        scroll.vertical = true; scroll.horizontal = false;
        scroll.movementType = UnityEngine.UI.ScrollRect.MovementType.Elastic;

        // Viewport Mask
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollObj.transform, false);
        viewport.AddComponent<UnityEngine.UI.RectMask2D>();
        var vpRt = viewport.GetComponent<RectTransform>();
        vpRt.anchorMin = Vector2.zero; vpRt.anchorMax = Vector2.one;
        vpRt.offsetMin = Vector2.zero; vpRt.offsetMax = Vector2.zero;
        scroll.viewport = vpRt;
        content.transform.SetParent(viewport.transform, false);

        // Mock Data
        string[] names = { "RhythmKing", "ProGamer_KR", "SodapopLover", "Newbie01", "StepUpMaster", "Guest1234", "HiddenWhale", "NoMissClear", "GalaxiasFan", "Tester99" };
        int[] scores = { 1000000, 985000, 950000, 880000, 850000, 700000, 650000, 500000, 300000, 100000 };

        for(int i=0; i<names.Length; i++)
        {
            GameObject item = new GameObject($"Rank_{i}");
            item.transform.SetParent(content.transform, false);
            
            // Layout for Row
            var hlg = item.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(20, 20, 5, 5);
            hlg.spacing = 20;
            hlg.childControlWidth = false; hlg.childControlHeight = false;
            hlg.childAlignment = TextAnchor.MiddleLeft;

            var bgImg = item.AddComponent<UnityEngine.UI.Image>();
            bgImg.color = new Color(0,0,0,0.5f); // Standard dark background

            var le = item.AddComponent<UnityEngine.UI.LayoutElement>();
            le.preferredHeight = 80;

            // MEDAL ICON (Left)
            if (i < 3)
            {
                 GameObject medalObj = new GameObject("Medal");
                 medalObj.transform.SetParent(item.transform, false);
                 var mImg = medalObj.AddComponent<UnityEngine.UI.Image>();
                 // Simple colors for medals if no sprite
                 if(i==0) mImg.color = new Color(1f, 0.8f, 0.2f); // Gold
                 else if(i==1) mImg.color = new Color(0.8f, 0.8f, 0.9f); // Silver
                 else if(i==2) mImg.color = new Color(0.8f, 0.5f, 0.2f); // Bronze
                 
                 var mLe = medalObj.AddComponent<UnityEngine.UI.LayoutElement>();
                 mLe.preferredWidth = 50; mLe.preferredHeight = 50;
            }
            else
            {
                 // Spacer for non-medal rows
                 GameObject spacer = new GameObject("Spacer");
                 spacer.transform.SetParent(item.transform, false);
                 var sLe = spacer.AddComponent<UnityEngine.UI.LayoutElement>();
                 sLe.preferredWidth = 50; sLe.preferredHeight = 50;
            }

            GameObject txtObj = new GameObject("Info");
            txtObj.transform.SetParent(item.transform, false);
            var txt = txtObj.AddComponent<TMPro.TextMeshProUGUI>();
            txt.text = $"{i+1}. {names[i]}   -   {scores[i]:N0}";
            txt.fontSize = 32;
            txt.alignment = TMPro.TextAlignmentOptions.Left; // Left align next to medal
            txt.color = (i < 3) ? Color.yellow : Color.white;
            
            var tLe = txtObj.AddComponent<UnityEngine.UI.LayoutElement>();
            tLe.preferredWidth = 600; tLe.preferredHeight = 50;
        }

        // Close Button
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

}
