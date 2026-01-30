using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using System;
using System.Collections;

/// <summary>
/// 웹 로그인과 Unity를 연동하는 매니저
/// </summary>
public class LoginManager : MonoBehaviour
{
    public static LoginManager Instance { get; private set; }

    [Header("Login Settings")]
    [Tooltip("백엔드 API URL (로컬: http://localhost:3000, 배포: https://your-render-url.com)")]
    public string backendApiUrl = "http://localhost:3000";

    [Tooltip("로그인 API 엔드포인트")]
    public string loginEndpoint = "/api/login";

    private string loginDataPath;

    [Header("Login Status")]
    public bool isLoggedIn = false;
    public string currentUserName = "";
    public string currentUserEmail = "";

    [Header("UI References")]
    public Button loginButton;
    public TextMeshProUGUI loginStatusText;

    private GameObject userInfoPanel;
    private TextMeshProUGUI userNameText;
    private TextMeshProUGUI userEmailText;
    private TextMeshProUGUI userLevelText; // [NEW] 레벨 텍스트
    private TextMeshProUGUI userCoinText;  // [NEW] 코인 텍스트

    // 데이터 저장용
    public int currentUserCoin = 1000; // [NEW] 기본 지급 코인
    public int currentUserLevel = 1;   // [NEW] 기본 레벨

    private GameObject inGameLoginPanel;
    private TMP_InputField emailInputField;
    private TMP_InputField passwordInputField;
    private Button inGameLoginButton;
    private Button closeLoginButton;
    private TextMeshProUGUI errorMessageText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // 로그인 정보 파일 경로 설정
        loginDataPath = System.IO.Path.Combine(Application.persistentDataPath, "unity_login.json");
        Debug.Log($"[LoginManager] Login data path: {loginDataPath}");
    }

    private void Start()
    {
        // LOGIN 버튼 찾기
        if (loginButton == null)
        {
            GameObject loginBtn = GameObject.Find("LOGIN");
            if (loginBtn != null)
            {
                loginButton = loginBtn.GetComponent<Button>();
            }
        }

        // 버튼 이벤트 연결
        if (loginButton != null)
        {
            loginButton.onClick.RemoveAllListeners();
            loginButton.onClick.AddListener(OnLoginButtonClick);
            Debug.Log("[LoginManager] LOGIN button connected");
        }
        else
        {
            Debug.LogWarning("[LoginManager] loginButton is null in Start!");
        }

        // UserInfoPanel 찾기
        userInfoPanel = GameObject.Find("UserInfoPanel");
        if (userInfoPanel != null)
        {
            Transform nameTransform = userInfoPanel.transform.Find("UserNameText");
            Transform emailTransform = userInfoPanel.transform.Find("UserEmailText");

            if (nameTransform != null) userNameText = nameTransform.GetComponent<TextMeshProUGUI>();
            if (emailTransform != null) userEmailText = emailTransform.GetComponent<TextMeshProUGUI>();

            Debug.Log("[LoginManager] UserInfoPanel found and connected");
        }

        // InGameLoginPanel 찾기
        FindLoginPanel();

        // 로그인 상태 체크
        CheckLoginStatus();
        UpdateLoginUI();

        // [FIX] 만약 UserInfoPanel이 너무 많으면 정리 (중복 생성 버그 방지)
        CleanupDuplicateUserInfoPanels();
    }

    private void Update()
    {
        // [NEW] Enter 키로 로그인 제출
        if (inGameLoginPanel != null && inGameLoginPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                OnInGameLoginButtonClick();
            }

            // [NEW] Tab 키로 필드 이동
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (emailInputField != null && emailInputField.isFocused)
                {
                    if (passwordInputField != null)
                    {
                        passwordInputField.Select();
                        passwordInputField.ActivateInputField();
                    }
                }
            }

            // [NEW] ESC 키로 패널 닫기
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseLoginPanel();
            }
        }
    }

    private void FindLoginPanel()
    {
        if (inGameLoginPanel != null) return;

        Debug.Log("[LoginManager] InGameLoginPanel 찾기 시도...");

        // 1. Canvas 하위에서 찾기 (비활성화된 오브젝트도 찾음)
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            Transform t = canvas.transform.Find("InGameLoginPanel");
            if (t != null) inGameLoginPanel = t.gameObject;
        }

        // 2. 그래도 없으면 Resources.FindObjectsOfTypeAll 시도 (모든 오브젝트 검색)
        if (inGameLoginPanel == null)
        {
            var all = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach(var obj in all)
            {
                if (obj.name == "InGameLoginPanel" && obj.scene.isLoaded) // 씬에 있는 것만
                {
                    inGameLoginPanel = obj;
                    break;
                }
            }
        }

        if (inGameLoginPanel != null)
        {
            Debug.Log("[LoginManager] ✅ InGameLoginPanel을 찾았습니다!");
            Transform loginPanelTransform = inGameLoginPanel.transform.Find("LoginPanel");
            if (loginPanelTransform != null)
            {
                Transform emailInput = loginPanelTransform.Find("EmailInput");
                Transform passwordInput = loginPanelTransform.Find("PasswordInput");
                Transform loginBtn = loginPanelTransform.Find("LoginButton");
                Transform closeBtn = loginPanelTransform.Find("CloseButton");
                Transform errorText = loginPanelTransform.Find("ErrorText");

                if (emailInput != null) emailInputField = emailInput.GetComponent<TMP_InputField>();
                if (passwordInput != null) passwordInputField = passwordInput.GetComponent<TMP_InputField>();
                if (loginBtn != null)
                {
                    inGameLoginButton = loginBtn.GetComponent<Button>();
                    inGameLoginButton.onClick.RemoveAllListeners();
                    inGameLoginButton.onClick.AddListener(OnInGameLoginButtonClick);
                }
                if (closeBtn != null)
                {
                    closeLoginButton = closeBtn.GetComponent<Button>();
                    closeLoginButton.onClick.RemoveAllListeners();
                    closeLoginButton.onClick.AddListener(CloseLoginPanel);
                }
                if (errorText != null) errorMessageText = errorText.GetComponent<TextMeshProUGUI>();

                Debug.Log("[LoginManager] ✅ InGameLoginPanel found and connected");
            }
            else
            {
                Debug.LogError("[LoginManager] ❌ InGameLoginPanel은 있지만 LoginPanel 자식을 찾을 수 없습니다!");
            }
        }
        else
        {
            Debug.LogWarning("[LoginManager] ❌ InGameLoginPanel을 찾을 수 없습니다. ShowLoginPanel() 호출 시 다시 시도합니다.");
        }
    }

    private void CleanupDuplicateUserInfoPanels()
    {
        GameObject[] panels = GameObject.FindObjectsOfType<GameObject>();
        int count = 0;
        GameObject keep = null;
        
        foreach(var p in panels)
        {
            if (p.name == "UserInfoPanel" && p.transform.parent != null && p.transform.parent.name == "Canvas")
            {
                if (keep == null) keep = p;
                else 
                {
                    Destroy(p);
                    count++;
                }
            }
        }
        if (count > 0) Debug.Log($"[LoginManager] Cleaned up {count} duplicate UserInfoPanels.");
    }

    /// <summary>
    /// LOGIN 버튼 클릭 핸들러
    /// </summary>
    public void OnLoginButtonClick()
    {
        Debug.Log("========================================");
        Debug.Log("[LoginManager] ✅ LOGIN 버튼이 클릭되었습니다!");
        Debug.Log($"[LoginManager] 현재 로그인 상태: {isLoggedIn}");
        Debug.Log("========================================");

        if (isLoggedIn)
        {
            // 로그아웃
            Debug.Log("[LoginManager] 로그아웃 실행");
            Logout();
        }
        else
        {
            // 인게임 로그인 패널 열기
            Debug.Log("[LoginManager] 로그인 패널 열기 시도");
            ShowLoginPanel();
        }
    }

    /// <summary>
    /// 인게임 로그인 패널 표시
    /// </summary>
    public void ShowLoginPanel()
    {
        // 다시 찾기 시도 (비활성화 상태일 경우 대비)
        if (inGameLoginPanel == null) FindLoginPanel();

        if (inGameLoginPanel != null)
        {
            Debug.Log("[LoginManager] ✅ InGameLoginPanel 활성화");
            inGameLoginPanel.SetActive(true);
            
            // [FIX] 로고나 다른 UI보다 항상 위에 뜨도록 순서 변경
            inGameLoginPanel.transform.SetAsLastSibling();
            
            if (errorMessageText != null) errorMessageText.text = "";
            if (emailInputField != null) 
            {
                emailInputField.text = "";
                emailInputField.customCaretColor = true;
                emailInputField.caretColor = Color.cyan;
                emailInputField.caretWidth = 2;
                emailInputField.selectionColor = new Color(0, 1, 1, 0.3f);
            }
            if (passwordInputField != null) 
            {
                passwordInputField.text = "";
                passwordInputField.customCaretColor = true;
                passwordInputField.caretColor = Color.cyan;
                passwordInputField.caretWidth = 2;
                passwordInputField.selectionColor = new Color(0, 1, 1, 0.3f);
            }
        }
        else
        {
             Debug.LogError("[LoginManager] InGameLoginPanel을 씬에서 찾을 수 없습니다. 'Fox All Buttons'를 실행했는지 확인해주세요.");
        }
    }

    /// <summary>
    /// 인게임 로그인 패널 닫기
    /// </summary>
    public void CloseLoginPanel()
    {
        if (inGameLoginPanel != null)
        {
            inGameLoginPanel.SetActive(false);
            if (errorMessageText != null) errorMessageText.text = "";
            Debug.Log("[LoginManager] Closed in-game login panel");
        }
    }

    /// <summary>
    /// 인게임 로그인 버튼 클릭 핸들러
    /// </summary>
    public void OnInGameLoginButtonClick()
    {
        if (emailInputField == null || passwordInputField == null)
        {
            Debug.LogError("[LoginManager] Email or Password input field is null!");
            return;
        }

        string email = emailInputField.text.Trim();
        string password = passwordInputField.text.Trim();

        // 입력 검증
        if (string.IsNullOrEmpty(email))
        {
            ShowErrorMessage("Please enter your email.");
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            ShowErrorMessage("Please enter your password.");
            return;
        }

        // 로그인 API 호출
        StartCoroutine(PerformLogin(email, password));
    }

    /// <summary>
    /// 백엔드 API로 로그인 요청
    /// </summary>
    /// <summary>
    /// 백엔드 API (Supabase)로 로그인 요청
    /// </summary>
    private IEnumerator PerformLogin(string email, string password)
    {
        Debug.Log($"[LoginManager] Attempting login for: {email}");

        // 로그인 버튼 비활성화 (중복 클릭 방지)
        if (inGameLoginButton != null) inGameLoginButton.interactable = false;

        // NetworkManager를 통해 로그인 실행
        bool finished = false;
        
        // NetworkManager가 없으면 자동 생성 시도
        if (NetworkManager.Instance == null)
        {
             GameObject netMgrObj = new GameObject("NetworkManager");
             netMgrObj.AddComponent<NetworkManager>();
             Debug.Log("[LoginManager] NetworkManager created automatically.");
             yield return null; // 한 프레임 대기
        }

        if (NetworkManager.Instance == null)
        {
             ShowErrorMessage("NetworkManager not found!");
             if (inGameLoginButton != null) inGameLoginButton.interactable = true;
             yield break;
        }

        NetworkManager.Instance.SignIn(email, password, (success, message) => {
            if (success)
            {
                 Debug.Log("[LoginManager] NetworkManager Login Success!");
                 string tempName = email.Split('@')[0];
                 
                 NetworkManager.Instance.GetUserProfile((profile) => {
                     if (profile != null && !string.IsNullOrEmpty(profile.name))
                     {
                         OnLoginSuccess(profile.name, email);
                     }
                     else
                     {
                         OnLoginSuccess(tempName, email);
                     }
                     CloseLoginPanel();
                 });
            }
            else
            {
                // 메시지가 너무 길면 잘라서 표시
                string shortMsg = message.Length > 30 ? message.Substring(0, 30) + "..." : message;
                ShowErrorMessage("Login Failed: " + shortMsg);
                Debug.LogWarning($"[LoginManager] Login failed: {message}");
            }
            finished = true;
        });

        while (!finished) yield return null;

        if (inGameLoginButton != null) inGameLoginButton.interactable = true;
    }

    private void ShowErrorMessage(string message)
    {
        if (errorMessageText != null)
        {
            errorMessageText.text = message;
        }
        Debug.LogWarning($"[LoginManager] Error: {message}");
    }

    /// <summary>
    /// PlayerPrefs에서 로그인 정보 확인
    /// </summary>
    public void CheckLoginStatus()
    {
        if (PlayerPrefs.HasKey("userEmail") && PlayerPrefs.HasKey("userName"))
        {
            currentUserEmail = PlayerPrefs.GetString("userEmail");
            currentUserName = PlayerPrefs.GetString("userName");
            isLoggedIn = true;

            Debug.Log($"[LoginManager] Logged in as: {currentUserName} ({currentUserEmail})");
            
            // [FIX] Sync with NetworkManager
            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.ReceiveReactLogin($"{{\"id\":\"{PlayerPrefs.GetString("userId", "local-user")}\",\"email\":\"{currentUserEmail}\",\"name\":\"{currentUserName}\"}}");
                Debug.Log("[LoginManager] Synced login state with NetworkManager");
            }
        }
        else
        {
            isLoggedIn = false;
            Debug.Log("[LoginManager] Not logged in");
        }
    }

    /// <summary>
    /// 로그인 성공 시 호출 (웹에서 Unity로 전달)
    /// </summary>
    public void OnLoginSuccess(string userName, string userEmail)
    {
        currentUserName = userName;
        currentUserEmail = userEmail;
        isLoggedIn = true;

        // 로컬 저장
        PlayerPrefs.SetString("userName", userName);
        PlayerPrefs.SetString("userEmail", userEmail);
        PlayerPrefs.Save();

        Debug.Log($"[LoginManager] Login successful: {userName} ({userEmail})");
        UpdateLoginUI();

        // [NEW] 로그인 성공 팝업 표시
        ShowLoginSuccessPopup(userName);
    }

    /// <summary>
    /// 로그인 성공 팝업 표시
    /// </summary>
    private void ShowLoginSuccessPopup(string userName)
    {
        StartCoroutine(ShowSuccessPopupCoroutine(userName));
    }

    private IEnumerator ShowSuccessPopupCoroutine(string userName)
    {
        // Canvas 찾기
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) yield break;

        // 팝업 생성
        GameObject popup = new GameObject("LoginSuccessPopup");
        popup.transform.SetParent(canvas.transform, false);
        popup.transform.SetAsLastSibling();

        RectTransform popupRt = popup.AddComponent<RectTransform>();
        popupRt.anchorMin = new Vector2(0.5f, 0.5f);
        popupRt.anchorMax = new Vector2(0.5f, 0.5f);
        popupRt.pivot = new Vector2(0.5f, 0.5f);
        popupRt.anchoredPosition = Vector2.zero;
        popupRt.sizeDelta = new Vector2(450, 200);

        // 배경
        Image bgImg = popup.AddComponent<Image>();
        bgImg.color = new Color(0.1f, 0.15f, 0.2f, 0.98f);

        // 상단 테두리 (녹색)
        GameObject border = new GameObject("Border");
        border.transform.SetParent(popup.transform, false);
        Image borderImg = border.AddComponent<Image>();
        borderImg.color = new Color(0.2f, 0.9f, 0.4f, 1f); // 녹색

        RectTransform borderRt = border.GetComponent<RectTransform>();
        borderRt.anchorMin = new Vector2(0, 1);
        borderRt.anchorMax = new Vector2(1, 1);
        borderRt.pivot = new Vector2(0.5f, 1);
        borderRt.sizeDelta = new Vector2(0, 5);
        borderRt.anchoredPosition = Vector2.zero;

        // 체크 아이콘 (텍스트로 대체)
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(popup.transform, false);
        TextMeshProUGUI iconTxt = iconObj.AddComponent<TextMeshProUGUI>();
        iconTxt.text = "✓";
        iconTxt.fontSize = 60;
        iconTxt.alignment = TextAlignmentOptions.Center;
        iconTxt.color = new Color(0.2f, 0.9f, 0.4f, 1f);

        RectTransform iconRt = iconObj.GetComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0.5f, 1);
        iconRt.anchorMax = new Vector2(0.5f, 1);
        iconRt.pivot = new Vector2(0.5f, 1);
        iconRt.anchoredPosition = new Vector2(0, -20);
        iconRt.sizeDelta = new Vector2(100, 70);

        // 환영 메시지
        GameObject msgObj = new GameObject("Message");
        msgObj.transform.SetParent(popup.transform, false);
        TextMeshProUGUI msgTxt = msgObj.AddComponent<TextMeshProUGUI>();
        msgTxt.text = $"Welcome, {userName}!";
        msgTxt.fontSize = 28;
        msgTxt.fontStyle = TMPro.FontStyles.Bold;
        msgTxt.alignment = TextAlignmentOptions.Center;
        msgTxt.color = Color.white;

        RectTransform msgRt = msgObj.GetComponent<RectTransform>();
        msgRt.anchorMin = new Vector2(0, 0.2f);
        msgRt.anchorMax = new Vector2(1, 0.5f);
        msgRt.offsetMin = new Vector2(20, 0);
        msgRt.offsetMax = new Vector2(-20, 0);

        // 부가 메시지
        GameObject subMsgObj = new GameObject("SubMessage");
        subMsgObj.transform.SetParent(popup.transform, false);
        TextMeshProUGUI subMsgTxt = subMsgObj.AddComponent<TextMeshProUGUI>();
        subMsgTxt.text = "Login Successful!";
        subMsgTxt.fontSize = 18;
        subMsgTxt.alignment = TextAlignmentOptions.Center;
        subMsgTxt.color = new Color(0.6f, 0.7f, 0.8f, 1f);

        RectTransform subMsgRt = subMsgObj.GetComponent<RectTransform>();
        subMsgRt.anchorMin = new Vector2(0, 0);
        subMsgRt.anchorMax = new Vector2(1, 0.25f);
        subMsgRt.offsetMin = new Vector2(20, 20);
        subMsgRt.offsetMax = new Vector2(-20, 0);

        // 2초 후 자동 닫기
        yield return new WaitForSeconds(2.5f);

        // 페이드 아웃
        float fadeTime = 0.5f;
        float elapsed = 0f;
        CanvasGroup cg = popup.AddComponent<CanvasGroup>();

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            cg.alpha = 1f - (elapsed / fadeTime);
            yield return null;
        }

        Destroy(popup);
    }

    /// <summary>
    /// 로그아웃
    /// </summary>
    public void Logout()
    {
        isLoggedIn = false;
        currentUserName = "";
        currentUserEmail = "";

        PlayerPrefs.DeleteKey("userName");
        PlayerPrefs.DeleteKey("userEmail");
        PlayerPrefs.Save();

        Debug.Log("[LoginManager] Logged out");
        UpdateLoginUI();
    }

    /// <summary>
    /// UI 업데이트
    /// </summary>
    private void UpdateLoginUI()
    {
        // UserInfoPanel을 매번 다시 찾기 (씬이 변경되거나 늦게 생성될 수 있음)
        if (userInfoPanel == null)
        {
            userInfoPanel = GameObject.Find("UserInfoPanel");
            if (userInfoPanel != null)
            {
                Transform nameTransform = userInfoPanel.transform.Find("UserNameText");
                // Transform emailTransform = userInfoPanel.transform.Find("UserEmailText"); // (제거됨)
                Transform levelTransform = userInfoPanel.transform.Find("LevelText");
                Transform coinTransform = userInfoPanel.transform.Find("CoinContainer/CoinText"); // 계층 구조 반영

                if (nameTransform != null) userNameText = nameTransform.GetComponent<TextMeshProUGUI>();
                if (levelTransform != null) userLevelText = levelTransform.GetComponent<TextMeshProUGUI>();
                if (coinTransform != null) userCoinText = coinTransform.GetComponent<TextMeshProUGUI>();

                Debug.Log("[LoginManager] UserInfoPanel reconnected in UpdateLoginUI");
            }
        }

        // LOGIN/LOGOUT 버튼 업데이트
        if (loginButton != null)
        {
            var btnText = loginButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.text = isLoggedIn ? "LOGOUT" : "LOGIN";
            }

            // 버튼 크기 및 텍스트 자동 조정
            RectTransform rt = loginButton.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.sizeDelta = new Vector2(240, rt.sizeDelta.y);
            }

            if (btnText != null)
            {
                btnText.enableAutoSizing = true;
                btnText.fontSizeMin = 18;
                btnText.fontSizeMax = 36;
                btnText.margin = new Vector4(10, 0, 10, 0);
            }
        }

        // UserInfoPanel 업데이트
        if (userInfoPanel != null)
        {
            userInfoPanel.SetActive(isLoggedIn); // 로그인 시에만 표시
            
            if (isLoggedIn)
            {
                if (userNameText != null) userNameText.text = currentUserName;
                if (userLevelText != null) userLevelText.text = $"Lv.{currentUserLevel}";
                if (userCoinText != null) userCoinText.text = $"{currentUserCoin:N0}";

                // [FIX] UserInfoPanel 위치 강제 조정 (LOGIN 버튼 오른쪽)
                RectTransform panelRt = userInfoPanel.GetComponent<RectTransform>();
                if (panelRt != null)
                {
                    panelRt.anchorMin = new Vector2(0, 1);
                    panelRt.anchorMax = new Vector2(0, 1);
                    panelRt.pivot = new Vector2(0, 1);
                    panelRt.anchoredPosition = new Vector2(280, -20); // LOGIN 버튼(240) + 여백(40)
                    panelRt.sizeDelta = new Vector2(350, 60);
                }
            }
        }
    }


    /// <summary>
    /// 로그인 상태 팝업 표시
    /// </summary>
    private void ShowLoginStatus()
    {
        string message = isLoggedIn
            ? $"환영합니다, {currentUserName}님!\nEmail: {currentUserEmail}"
            : "로그인이 필요합니다.";

        Debug.Log($"[LoginManager] {message}");

        // Unity 에디터에서만 팝업 표시 - 유저 피드백으로 제거
        /*
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.DisplayDialog(
            isLoggedIn ? "로그인 성공" : "로그인 필요",
            message,
            "확인"
        );
        #endif
        */
    }


    [System.Serializable]
    public class LoginData
    {
        public string name;
        public string email;
    }

    [System.Serializable]
    public class LoginResponse
    {
        public bool success;
        public LoginData data;
    }

    [System.Serializable]
    public class LoginRequest
    {
        public string email;
        public string password;
    }

    [System.Serializable]
    public class LoginApiResponse
    {
        public bool success;
        public string message;
        public UserData user;
    }

    [System.Serializable]
    public class UserData
    {
        public string name;
        public string email;
    }
}
