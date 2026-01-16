using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // [FIX] Added TMPro namespace

public class GameUIBuilder : MonoBehaviour
{
    private GameObject pauseMenuPanel;
    private GameManager gameManager;

    void Start()
    {
        // [FIX] 로비/Main 씬에서는 게임 UI(타이머 등)를 생성하지 않음
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName.Contains("Lobby") || sceneName.Contains("Main")) return;

        gameManager = GetComponent<GameManager>();
        
        // Canvas 생성 없으면 만들기
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("GameCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();
            canvas.sortingOrder = 100; // [FIX] 다른 UI보다 위에 오도록 강제 설정
        }

        // EventSystem 확인
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // [FIX] Force Configure CanvasScaler for WebGL/Responsive Scaling
        // [FIX] Force Configure CanvasScaler for WebGL/Responsive Scaling
        CanvasScaler forceScaler = canvas.GetComponent<CanvasScaler>();
        if (forceScaler == null) forceScaler = canvas.gameObject.AddComponent<CanvasScaler>();
        
        forceScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        forceScaler.referenceResolution = new Vector2(1920, 1080);
        forceScaler.matchWidthOrHeight = 0.5f;

        CreateUI(canvas.transform);
    }

    public void CreateUI(Transform parent, GameManager gm = null)
    {
        if (gm != null) this.gameManager = gm;

        // [FIX] CLEANUP: Destroy existing UI elements to prevent overlap
        Transform existingTimer = parent.Find("TimerText");
        if (existingTimer != null) DestroyImmediate(existingTimer.gameObject);

        Transform existingScore = parent.Find("ScoreContainer");
        if (existingScore != null) DestroyImmediate(existingScore.gameObject);

        Transform existingPause = parent.Find("PauseMenuPanel");
        if (existingPause != null) DestroyImmediate(existingPause.gameObject);

        // 0. 남은 시간 텍스트 (왼쪽 상단 구석)
        // [FIX] Move Timer to Top-Left
        CreateTimerText(parent);

        // [FIX] 0-1. Score Text (Top Right)
        // Created with specific styling requirements
        // [FIX] SCORE UI - RESULT SCREEN STYLE (Dark Background + SCORE Label)
        // Parent container with dark background
        GameObject scoreContainer = new GameObject("ScoreContainer");
        scoreContainer.transform.SetParent(parent, false);
        RectTransform scRt = scoreContainer.AddComponent<RectTransform>();
        scRt.anchorMin = new Vector2(0.35f, 0.75f); // Below MENTAL gauge
        scRt.anchorMax = new Vector2(0.65f, 0.85f);
        scRt.offsetMin = Vector2.zero; scRt.offsetMax = Vector2.zero;

        // Add dark background bar like result screen
        Image scoreBg = scoreContainer.AddComponent<Image>();
        
        // Use Unity's built-in white sprite
        scoreBg.sprite = Resources.Load<Sprite>("UI/Skin/Background");
        if (scoreBg.sprite == null)
        {
            // Fallback: create minimal sprite
            scoreBg.sprite = Sprite.Create(
                Texture2D.whiteTexture,
                new Rect(0, 0, 1, 1),
                new Vector2(0.5f, 0.5f),
                1f
            );
        }
        
        scoreBg.type = Image.Type.Sliced; // Allow stretching
        scoreBg.type = Image.Type.Sliced; // Allow stretching
        scoreBg.color = Color.clear; // [FIX] Completely Remove Background
        
        // Remove outline or make it invisible
        // var scoreOutline = scoreContainer.AddComponent<UnityEngine.UI.Outline>();
        // scoreOutline.effectColor = Color.clear;

        // Child text object for "SCORE X"
        GameObject scoreTextObj = new GameObject("ScoreText");
        scoreTextObj.transform.SetParent(scoreContainer.transform, false);
        RectTransform txtRt = scoreTextObj.AddComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = Vector2.zero; txtRt.offsetMax = Vector2.zero;

        TextMeshProUGUI scoreTxt = scoreTextObj.AddComponent<TextMeshProUGUI>();
        scoreTxt.text = "SCORE 0";
        scoreTxt.fontSize = 60; // Large size
        // [FIX] Enable Auto Sizing for WebGL/Small Screens
        scoreTxt.enableAutoSizing = true;
        scoreTxt.fontSizeMin = 20;
        scoreTxt.fontSizeMax = 60;
        
        scoreTxt.fontStyle = FontStyles.Italic | FontStyles.Bold; // Italic & Bold like result screen
        scoreTxt.alignment = TextAlignmentOptions.Center;
        
        // [FIX] METALLIC SILVER GRADIENT (Result Screen Style)
        scoreTxt.enableVertexGradient = true;
        scoreTxt.colorGradient = new VertexGradient(
            new Color(1f, 1f, 1f),      // Top Left (White)
            new Color(1f, 1f, 1f),      // Top Right (White)
            new Color(0.7f, 0.7f, 0.8f),// Bottom Left (Silver/Blueish Gray)
            new Color(0.7f, 0.7f, 0.8f) // Bottom Right
        );
        scoreTxt.color = Color.white; // Base color white for gradient to work
        
        // Small outline for depth
        scoreTxt.outlineWidth = 0.15f;
        scoreTxt.outlineColor = new Color(0, 0, 0, 0.6f);

        if (gameManager != null) {
            gameManager.scoreText = scoreTxt;
        }

        // 1. 일시정지 버튼 (우측 상단 고정) -> [FIX] uGUI 버튼 클릭 문제로 인해 GameManager의 IMGUI 버튼으로 대체함
        /*
        CreateButton(parent, "PauseButton", "PAUSE", new Vector2(-100, -80), new Vector2(150, 60), () => {
            Debug.Log("[GameUIBuilder] Pause Button Clicked!");
            gameManager.PauseGame();
            pauseMenuPanel.SetActive(true);
        }, true); 
        */

        // 2. 일시정지 메뉴 패널 (초기엔 비활성화)
        CreatePauseMenu(parent);

        // [REMOVED] In-Game Rank Gauge
    }

    /* [REMOVED]
    void CreateInGameRankGauge(Transform parent)
    {
       // ... Method removed ...
    }
    */
    
    // 텍스트 생성 헬퍼
    void CreateTimerText(Transform parent)
    {
        GameObject textObj = new GameObject("TimerText");
        textObj.transform.SetParent(parent, false);
        
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1); // Top Left
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(250, -50); // Padding from corner
        rect.sizeDelta = new Vector2(400, 120); // Larger height
        
        // [FIX] Upgrade to TextMeshProUGUI for Styling
        TextMeshProUGUI txt = textObj.AddComponent<TextMeshProUGUI>();
        txt.text = "00:00";
        txt.alignment = TextAlignmentOptions.Left;
        txt.fontSize = 80; // Bigger Font
        // [FIX] Enable Auto Sizing
        txt.enableAutoSizing = true;
        txt.fontSizeMin = 30;
        txt.fontSizeMax = 80;
        
        txt.fontStyle = FontStyles.Italic | FontStyles.Bold; // Dynamic Style
        
        // [STYLE] Metallic Silver Gradient
        txt.enableVertexGradient = true;
        txt.colorGradient = new VertexGradient(
            new Color(1f, 1f, 1f),       // Top Left (White)
            new Color(0.9f, 0.9f, 1f),   // Top Right (White-ish)
            new Color(0.6f, 0.6f, 0.75f), // Bottom Left (Silver)
            new Color(0.6f, 0.6f, 0.75f)  // Bottom Right (Silver)
        );
        txt.color = Color.white;
        
        // [STYLE] Outline & Shadow
        txt.outlineWidth = 0.2f;
        txt.outlineColor = new Color(0, 0, 0, 0.8f);
        
        // Add Shadow component for extra depth if needed, or rely on TMP shader
        // For now, clear visibility is key
        
        if (gameManager != null) gameManager.timerText = txt;
    }

    void CreateText(Transform parent, string name, string content, Vector2 pos, int fontSize, Color color)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        RectTransform anchoredPosRx = textObj.AddComponent<RectTransform>();
        
        Text txt = textObj.AddComponent<Text>(); // Legacy Text based on prev code
        txt.text = content;
        // [FIX] Unity 최신 버전에서 Arial.ttf가 없을 수 있음 -> 기본 폰트 사용
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.font = font;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = color;
        txt.fontSize = fontSize;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;

        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f); // 상단 중앙
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(200, 100);
    }

    void CreatePauseMenu(Transform parent)
    {
        pauseMenuPanel = new GameObject("PauseMenuPanel");
        pauseMenuPanel.transform.SetParent(parent, false);
        
        // 반투명 배경
        Image bg = pauseMenuPanel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.8f);
        RectTransform rect = bg.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        // 메뉴 컨테이너
        GameObject container = new GameObject("Container");
        container.transform.SetParent(pauseMenuPanel.transform, false);
        
        // Resume 버튼
        CreateButton(container.transform, "ResumeButton", "RESUME", new Vector2(0, 50), new Vector2(200, 60), () => {
            gameManager.ResumeGame();
            pauseMenuPanel.SetActive(false);
        });

        // Lobby 버튼
        CreateButton(container.transform, "LobbyButton", "RETURN TO LOBBY", new Vector2(0, -50), new Vector2(200, 60), () => {
            gameManager.ReturnToLobby();
        });

        pauseMenuPanel.SetActive(false);
    }

    // 버튼 생성 헬퍼
    GameObject CreateButton(Transform parent, string name, string text, Vector2 anchoredPos, Vector2 size, UnityEngine.Events.UnityAction onClick, bool isAnchorTopRight = false)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

        Button btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(onClick);

        RectTransform rect = btnObj.GetComponent<RectTransform>();
        if (isAnchorTopRight)
        {
            rect.anchorMin = Vector2.one; // (1, 1) 우측 상단
            rect.anchorMax = Vector2.one;
            rect.pivot = Vector2.one;
        }
        else
        {
            // 기본값: 중앙
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
        }
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPos;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        Text txt = textObj.AddComponent<Text>();
        txt.text = text;
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.font = font;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        txt.fontSize = 24; // 글자 크기 조금 키움
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        // [FIX] 텍스트가 버튼 클릭을 가로채지 않도록 설정
        txt.raycastTarget = false;

        // [FIX] 버튼이 다른 UI 위에 오도록 맨 나중으로 순서 변경
        btnObj.transform.SetAsLastSibling();

        return btnObj;
    }
}
