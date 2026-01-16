using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 게임 전체 흐름을 관리하는 메인 매니저 (Game_first, Game_second 씬 모두 지원)
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scene Settings")]
    public bool isSecondScene = false;   // Game_second 씬인지 여부

    [Header("Result UI Assets")]
    public Sprite[] resultAlbumArt; // [0]=Scene1, [1]=Scene2... Album Arts for Result Screen (0: Galaxias, 1: Sodapop)
    public string[] songTitles = { "GALAXIAS!", "Sodapop" };
    
    [Header("Result UI Colors")]
    public Color resultBgColor = new Color(0.05f, 0.08f, 0.25f, 0.95f); // Deep Navy
    public Color retryBtnColor = new Color(0, 0.6f, 1f); // Cyan
    public Color mainBtnColor = new Color(0.2f, 0.2f, 0.3f); // Dark Purple

    [Header("Game State")]
    public bool isPlaying = false;
    public bool isPaused = false;

    [Header("Score")]
    public int score = 0;
    public int combo = 0;
    public int maxCombo = 0;

    [Header("HP Settings")]
    public float maxHp = 100f;           // 최대 체력
    public float currentHp;              // 현재 체력
    public float hpDecreaseAmount = 10f; // 놓쳤을 때 감소량
    public Color mentalGaugeColor = new Color(0.15f, 0.6f, 0.95f); // [NEW] Customizable Mental Gauge Color
    public Image hpBarFill;              // [NEW] HP Bar Fill Image
    public TextMeshProUGUI hpNumericText; // [NEW] HP Numeric Display (e.g., "100 / 100")
    // public Slider hpSlider; // Removed

    // [NEW] Cumulative Rank Logic
    private float maxPossibleScore = 0f;

    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    public GameObject gameOverPanel; // [FIX] Restored
    public TextMeshProUGUI finalScoreText;
    public Image inGameRankGaugeFill; // [NEW] In-Game Gauge UI Reference
    public TextMeshProUGUI timerText; // [NEW] Timer Text Reference (Updated to TMP)

    [Header("Audio")]
    public List<AudioSource> musicSources = new List<AudioSource>(); 
    public AudioSource sfxSource;   
    public AudioClip gameMusic;     // Galaxias 또는 Sodapop 음악 파일
    public AudioClip galaxiasMusic; // Game_first용 (Galaxias)
    public AudioClip sodapopMusic;  // Game_second용 (Sodapop)
    private AudioSource mainMusicSource; // 메인 배경음악 전용
    
    // [BALANCED TUNING] 배경음악과 붐 효과의 균형 조정
    // 0.5 -> 1.3으로 점프하면 2.6배 부스트 (배경음악 충분히 들리면서 붐도 더 확실함)
    private float baseVolume = 0.5f;   
    private float boostVolume = 1.3f; 

    [Header("Stats")]
    public int perfectCount = 0;
    public int greatCount = 0;
    public int badCount = 0;
    public int missCount = 0; 
    [Header("Music Timing")]
    public float songPosition = 0f;          // 현재 노래 재생 시간
    
    // [NEW] Currency & Shop System
    [Header("Shop System")]
    public int coins = 5000; // Initial test coins
    public List<string> ownedItems = new List<string>();

    private Coroutine volumeBoostCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            AudioConfiguration config = AudioSettings.GetConfiguration();
            config.dspBufferSize = 256; 
            AudioSettings.Reset(config);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (scoreText == null) scoreText = GameObject.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();
        if (comboText == null) comboText = GameObject.Find("ComboText")?.GetComponent<TextMeshProUGUI>();
        if (gameOverPanel == null) gameOverPanel = GameObject.Find("GameOverPanel");
        if (gameOverPanel == null) gameOverPanel = GameObject.Find("GameOverPanel");
        if (finalScoreText == null) finalScoreText = GameObject.Find("FinalScoreText")?.GetComponent<TextMeshProUGUI>();
        if (inGameRankGaugeFill == null) inGameRankGaugeFill = GameObject.Find("InGameRankGaugeFill")?.GetComponent<Image>();

        // SFX용 AudioSource 생성
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        
        // 메인 배경음악용 AudioSource 생성
        mainMusicSource = gameObject.AddComponent<AudioSource>();
        mainMusicSource.playOnAwake = false;
        mainMusicSource.loop = true;
        mainMusicSource.volume = baseVolume;
        mainMusicSource.spatialBlend = 0f; // 2D 사운드로 설정
        mainMusicSource.priority = 0; // 최고 우선순위
        
        // musicSources 리스트에 추가
        musicSources.Clear();
        musicSources.Add(mainMusicSource);
        
        Debug.Log($"[GameManager] AudioSource 생성 완료! Volume: {baseVolume}");
        
        Debug.Log($"[GameManager] 게임매니저 초기화 완료! Scene: {(isSecondScene ? "Game_second (Sodapop)" : "Game_first (Galaxias)")}");

        // [FIX] Ensure coins are initialized (Inspector override protection)
        if (coins <= 0)
        {
            coins = 5000;
            Debug.Log("[GameManager] Coins initialized to 5000 (was 0 or negative)");
        }
        Debug.Log($"[GameManager] Current Coins: {coins}");

        if (Object.FindObjectOfType<NetworkManager>() == null)
        {
            GameObject netObj = new GameObject("NetworkManager");
            netObj.AddComponent<NetworkManager>();
        }
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        Debug.Log($"[GameManager] OnSceneLoaded: {scene.name}");
        
        // 1. Clean up legacy/orphan UI first
        CleanupLegacyUI();
        
        // [FIX] Destroy ALL UI elements from previous game to prevent "old screen" bug
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        foreach (var canvas in allCanvases)
        {
            // Destroy all children except the canvas itself
            foreach (Transform child in canvas.transform)
            {
                if (child.name.Contains("GameOver") || 
                    child.name.Contains("Score") || 
                    child.name.Contains("Combo") ||
                    child.name.Contains("HP") ||
                    child.name.Contains("Rank"))
                {
                    Debug.Log($"[GameManager] Destroying old UI element: {child.name}");
                    Destroy(child.gameObject);
                }
            }
        }

        // 2. Check if we are in a Lobby or Main Menu scene
        // Contains("Lobby") or "Main" covers likely names (SchoolLobby, MainMenu, etc.)
        if (scene.name.Contains("Lobby") || scene.name.Contains("Main"))
        {
            Debug.Log("[GameManager] Lobby/Main Scene loaded. Stopping Game & Cleaning UI.");
            
            isPlaying = false;
            
            // [FIX] Stop ALL AudioSources when loading lobby
            AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
            foreach (var audioSource in allAudioSources)
            {
                if (audioSource != null && audioSource.isPlaying)
                {
                    audioSource.Stop();
                    Debug.Log($"[GameManager] Stopped AudioSource on {audioSource.gameObject.name} (Lobby Load)");
                }
            }
            
            // [FIX] Explicitly destroy ALL Game UI elements that might have leaked
            GameObject[] uiToDelete = {
                GameObject.Find("GameOverPanel"),
                GameObject.Find("HPBarContainer"),
                GameObject.Find("RankGaugeContainer"),
                GameObject.Find("ScoreContainer"),
                GameObject.Find("TimerText"),
                GameObject.Find("ScoreText"),
                GameObject.Find("PauseMenuPanel"),
                // [FIX] Add Combo UI elements
                GameObject.Find("ComboText"),
                GameObject.Find("ComboNum"),
                GameObject.Find("ComboDisplay") 
            };

            foreach (var obj in uiToDelete)
            {
                if (obj != null) 
                {
                    Debug.Log($"[GameManager] Destroying leaked Game UI in Lobby: {obj.name}");
                    Destroy(obj);
                }
            }
            
            return; // STOP HERE! Do not StartGame()
        }

        // 3. Game Scene Logic (Only if NOT Lobby)
        Debug.Log("[GameManager] Game Scene loaded. Initializing Game.");
        
        // [FIX] Clear previous audio clip AND reset volume to prevent mixing and loud audio
        if (mainMusicSource != null)
        {
            mainMusicSource.Stop();
            mainMusicSource.clip = null;
            mainMusicSource.volume = baseVolume; // Reset to base volume
            Debug.Log($"[GameManager] Cleared previous audio clip and reset volume to {baseVolume}");
        }
        
        // [FIX] Select music based on scene name BEFORE starting game
        SelectMusicForScene();
        
        // Reset State for new round
        score = 0; combo = 0; currentHp = maxHp;
        perfectCount = 0; greatCount = 0; badCount = 0; missCount = 0;
        
        // Cleanup old GameOverPanel reference
        if (gameOverPanel != null) 
        {
             Destroy(gameOverPanel); 
             gameOverPanel = null;
        }

        // Re-initialize UI details
        if (scoreText != null)
        {
            scoreText.enableVertexGradient = true;
            scoreText.colorGradient = new VertexGradient(
                new Color(0.8f, 1f, 1f), new Color(0.8f, 1f, 1f), 
                new Color(0f, 0.5f, 1f), new Color(0f, 0.5f, 1f)
            );
            scoreText.outlineWidth = 0.2f;
            scoreText.outlineColor = new Color(0, 0, 0, 0.8f);
        }

        // Trigger StartGame logic ONLY for game scenes
        StartGame();
    }

    private void Start()
    {
        // [FIX] Cleanup Legacy UI (Green Score, etc.) immediately
        CleanupLegacyUI();

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        currentHp = maxHp; // Init HP
        UpdateUI();

        // [FIX] Style Score Text (Gradient & Outline)
        if (scoreText != null)
        {
            scoreText.enableVertexGradient = true;
            scoreText.colorGradient = new VertexGradient(
                new Color(0.8f, 1f, 1f), // Top Left (Light Cyan)
                new Color(0.8f, 1f, 1f), // Top Right
                new Color(0f, 0.5f, 1f), // Bottom Left (Deep Blue)
                new Color(0f, 0.5f, 1f)  // Bottom Right
            );
            scoreText.outlineWidth = 0.2f;
            scoreText.outlineColor = new Color(0, 0, 0, 0.8f);
            scoreText.fontSize = 50; // Ensure visible
        }

        // 씬에 따라 적절한 음악 선택
        SelectMusicForScene();

        // [NEW] Ensure UI is created ONLY in Game Scenes
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (sceneName != "SchoolLobby")
        {
            // CreateInGameRankGauge(); // [REMOVED] User requested deletion
            // CreateHPBar(); // Moved to StartGame()
        }
        else
        {
            Debug.Log("[GameManager] Lobby Scene detected. Skipping In-Game UI creation (HP/Rank).");
            // Ensure no stray UI exists
            GameObject hpC = GameObject.Find("HPBarContainer"); if(hpC) Destroy(hpC);
            GameObject rgC = GameObject.Find("RankGaugeContainer"); if(rgC) Destroy(rgC);
        }

        Debug.Log($"[GameManager] HP CHECK: Current={currentHp}, Max={maxHp}"); 

        // [NEW] Calculate Max Score for Cumulative Gauge
        NoteSpawner spawner = FindObjectOfType<NoteSpawner>();
        if (spawner != null)
        {
            if (spawner.beatmap == null || spawner.beatmap.notes.Count == 0) spawner.GenerateTestBeatmap();
            maxPossibleScore = spawner.beatmap.notes.Count * 1.0f; // 1.0 per note
            Debug.Log($"[GameManager] Max Possible Score: {maxPossibleScore}");
        }

        StartGame(); 
    }

    // [FIX] Robust UI Cleanup to remove "Green Score" or other artifacts
    private void CleanupLegacyUI()
    {
        // 1. Find all TextMeshProUGUI objects
        TextMeshProUGUI[] allTmp = FindObjectsOfType<TextMeshProUGUI>();
        foreach(var t in allTmp)
        {
            // Skip the active ScoreText we are using
            if (scoreText != null && t.gameObject == scoreText.gameObject) continue;
            
            // Skip FinalScoreText (Result Screen)
            if (finalScoreText != null && t.gameObject == finalScoreText.gameObject) continue;

            // Check for "SCORE" or "COMBO" in text or name
            string tText = (t.text ?? "").ToUpper();
            string tName = t.name.ToUpper();
            
            if (tText.Contains("SCORE") || tName.Contains("SCORE") || 
                tText.Contains("COMBO") || tName.Contains("COMBO"))
            {
                // Safety: Don't kill GameOverPanel children if they are valid
                if (t.transform.root.name.Contains("GameOver")) continue;
                
                // Safety: Don't kill our "ScoreText" inside "ScoreContainer" if referencing failed matches
                if (t.transform.parent != null && t.transform.parent.name == "ScoreContainer") continue;

                Debug.Log($"[GameManager] Removing LEGACY/DUPLICATE UI: {t.name} (Text: {t.text})");
                t.gameObject.SetActive(false);
                Destroy(t.gameObject); // Destroy the object
            }
        }
        
        // 2. Check Legacy Text (UnityEngine.UI.Text)
        Text[] allTxt = FindObjectsOfType<Text>();
        foreach(var t in allTxt)
        {
            string tText = (t.text ?? "").ToUpper();
            if (tText.Contains("SCORE") || tText.Contains("COMBO"))
            {
                 Debug.Log($"[GameManager] Removing LEGACY Text UI: {t.name}");
                 t.gameObject.SetActive(false);
                 Destroy(t.gameObject);
            }
        }
    }

    // [HELPER] Create a simple white sprite ensures Image.Type.Filled works
    private Sprite _whiteSprite;
    private Sprite GetWhiteSprite()
    {
        if (_whiteSprite == null)
        {
            Texture2D tex = new Texture2D(2, 2);
            var white = Color.white;
            tex.SetPixels(new Color[] {white, white, white, white});
            tex.Apply();
            _whiteSprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f));
        }
        return _whiteSprite;
    }

    private void Update()
    {
        if (isPlaying && mainMusicSource != null && mainMusicSource.isPlaying)
        {
            songPosition += Time.deltaTime;
        }
        
        // [FIX] Check HP every frame to ensure EndGame is called (but only once!)
        if (isPlaying && currentHp <= 0)
        {
            Debug.Log("[GameManager] HP reached 0! Calling EndGame()");
            currentHp = 0; // Clamp to 0
            EndGame(); // This sets isPlaying = false, so won't be called again
            return; // Don't process further
        }
        
        // [FIX] Update Timer Every Frame (Smooth Countdown)
        if (timerText != null && mainMusicSource != null && mainMusicSource.clip != null)
        {
             float remainingTime = mainMusicSource.clip.length - mainMusicSource.time;
             if (remainingTime < 0) remainingTime = 0;
             
             int minutes = Mathf.FloorToInt(remainingTime / 60F);
             int seconds = Mathf.FloorToInt(remainingTime - minutes * 60);
             // Always show 2 digits
             timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
        
        // [DEBUG] Detect clicks on result screen
        if (!isPlaying && Input.GetMouseButtonDown(0))
        {
            var eventSystem = UnityEngine.EventSystems.EventSystem.current;
            if (eventSystem != null)
            {
                var pointerData = new UnityEngine.EventSystems.PointerEventData(eventSystem);
                pointerData.position = Input.mousePosition;
                
                var results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
                eventSystem.RaycastAll(pointerData, results);
                
                if (results.Count > 0)
                {
                    string clickedName = results[0].gameObject.name;
                    GameObject clickedObj = results[0].gameObject;
                    
                    Debug.Log($"[GameManager DEBUG] Clicked: {clickedName} | Parent: {clickedObj.transform.parent?.name} | Layer: {LayerMask.LayerToName(clickedObj.layer)}");
                    
                    // [FIX] Manually trigger button actions since onClick doesn't work
                    if (clickedName == "Btn_RETRY" || (clickedObj.transform.parent != null && clickedObj.transform.parent.name == "Btn_RETRY"))
                    {
                        Debug.Log("[GameManager] ===== MANUAL RETRY CLICKED =====");
                        RestartGame();
                    }
                    else if (clickedName == "Btn_MAINMENU" || (clickedObj.transform.parent != null && clickedObj.transform.parent.name == "Btn_MAINMENU"))
                    {
                        Debug.Log("[GameManager] ===== MANUAL MAIN CLICKED =====");
                        ReturnToLobby();
                    }
                }
                else
                {
                    Debug.LogWarning("[GameManager UI DEBUG] Click detected but NO UI hit! (Raycast returned 0 results)");
                }
            }
        }
    }

    private void SelectMusicForScene()
    {
        // [FIX] Use scene name directly instead of isSecondScene flag to prevent confusion
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        // [FIX] Clear previous music assignment
        gameMusic = null;
        
        if (sceneName == "Game_second")
        {
            // Game_second 씬: Sodapop 사용
            if (sodapopMusic != null)
            {
                gameMusic = sodapopMusic;
                Debug.Log("[GameManager] ✓ Selected Sodapop music for Game_second scene");
            }
            else
            {
                gameMusic = Resources.Load<AudioClip>("Sodapop");
                if (gameMusic != null)
                {
                    Debug.Log("[GameManager] ✓ Loaded Sodapop from Resources");
                }
                else
                {
                    Debug.LogError("[GameManager] ✗ Sodapop music not found! Assign in Inspector or add to Resources folder.");
                }
            }
        }
        else if (sceneName == "Game_first")
        {
            // Game_first 씬: Galaxias 사용
            if (galaxiasMusic != null)
            {
                gameMusic = galaxiasMusic;
                Debug.Log("[GameManager] ✓ Selected Galaxias music for Game_first scene");
            }
            else
            {
                Debug.LogError("[GameManager] ✗ Galaxias music not assigned! Assign in Inspector.");
            }
        }
        else
        {
            Debug.LogWarning($"[GameManager] Unknown scene '{sceneName}' - no music selected");
        }
    }

    public void StartGame()
    {
        // [FIX] Re-validate music selection to ensure correct audio
        SelectMusicForScene();
        
        // [FIX] Verify music is assigned before starting
        if (gameMusic == null)
        {
            Debug.LogError("[GameManager] ✗ gameMusic is NULL! Cannot start game without music.");
            return;
        }
        
        // Create HP Bar ONLY when game starts
        CreateHPBar();
        
        // [FIX] Create Score UI using GameUIBuilder
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            // [FIX] MonoBehaviour must be added via AddComponent, NOT new()
            GameUIBuilder uiBuilder = gameObject.GetComponent<GameUIBuilder>();
            if (uiBuilder == null) uiBuilder = gameObject.AddComponent<GameUIBuilder>();
            
            uiBuilder.CreateUI(canvas.transform, this);
            Debug.Log("[GameManager] GameUIBuilder created Score UI");
        }
        else
        {
            Debug.LogError("[GameManager] No Canvas found for UI creation!");
        }
        
        // Reset State
        score = 0; combo = 0; currentHp = maxHp;
        perfectCount = 0; greatCount = 0; badCount = 0; missCount = 0;
        
        isPlaying = true;
        Time.timeScale = 1f; // [FIX] Ensure time is running
        isPaused = false;
        maxCombo = 0;
        
        // [CRITICAL FIX] Always destroy old gameOverPanel (including Inspector-assigned one!)
        if (gameOverPanel != null)
        {
            Debug.Log("[GameManager] Destroying old/Inspector gameOverPanel completely");
            Destroy(gameOverPanel);
            gameOverPanel = null;
        }
        
        // Also search for any orphaned panels in scene
        GameObject[] oldPanels = GameObject.FindGameObjectsWithTag("Untagged");
        foreach (var obj in oldPanels)
        {
            if (obj.name == "GameOverPanel" || obj.name.Contains("GameOver"))
            {
                Debug.LogWarning($"[GameManager] Found orphaned panel: {obj.name}, destroying it");
                Destroy(obj);
            }
        }
        
        Debug.Log("[GameManager] GameOverPanel cleaned. Will create fresh panel in EndGame().");

        // 배경음악 재생
        if (mainMusicSource != null && gameMusic != null)
        {
            mainMusicSource.clip = gameMusic;
            mainMusicSource.volume = baseVolume;
            
            Debug.Log($"[GameManager] AudioSource 설정: Volume={mainMusicSource.volume}, Clip={gameMusic.name}");
            Debug.Log($"[GameManager] AudioSource 상태: enabled={mainMusicSource.enabled}, mute={mainMusicSource.mute}");
            
            mainMusicSource.Play();
            
            songPosition = 0f;
            
            string musicName = isSecondScene ? "Sodapop" : "Galaxias";
            Debug.Log($"[GameManager] {musicName} 재생 시작! isPlaying={mainMusicSource.isPlaying}");
        }
        else
        {
            string musicName = isSecondScene ? "Sodapop.wav" : "Galaxias";
            Debug.LogWarning($"[GameManager] {musicName}가 할당되지 않았습니다! Inspector에서 할당하세요.");
            Debug.LogWarning($"[GameManager] mainMusicSource={mainMusicSource}, gameMusic={gameMusic}");
        }
        
        Debug.Log($"[GameManager] Game Started! Score={score}, Combo={combo}, HP={currentHp}");
        
        // [FIX] Start note spawning
        NoteSpawner spawner = FindObjectOfType<NoteSpawner>();
        if (spawner != null)
        {
            spawner.StartSpawning();
            Debug.Log("[GameManager] NoteSpawner started!");
        }
        else
        {
            Debug.LogWarning("[GameManager] NoteSpawner not found!");
        }
        
        UpdateUI();


    }

    public void OnBubbleMiss()
    {
        if (!isPlaying) return;

        combo = 0;
        currentHp -= hpDecreaseAmount;  // 체력 감소!
        
        Debug.Log($"[GameManager] MISS! HP: {currentHp}/{maxHp} (decreased by {hpDecreaseAmount})");
       
        if (currentHp <= 0)             // 체력이 0 이하면
        {
            currentHp = 0;
            Debug.Log("[GameManager] HP is 0! Calling EndGame() from OnBubbleMiss");
            EndGame();                  // 게임 오버
        }
        UpdateUI();                     // HP바 업데이트
    }

    public void PerfectHitBoost()
    {
        if (mainMusicSource != null && mainMusicSource.isPlaying)
        {
            // 즉시 부스트 볼륨으로!
            mainMusicSource.volume = boostVolume;

            // 부드럽게 원래 볼륨으로 복귀
            if (volumeBoostCoroutine != null) StopCoroutine(volumeBoostCoroutine);
            volumeBoostCoroutine = StartCoroutine(LerpMusicVolume());
        }
    }

    private IEnumerator LerpMusicVolume()
    {
        // 0.5초에 걸쳐 부드럽게 복귀
        float duration = 0.5f; 
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // 부드럽게 돌아오는 곡선 (t^3)
            float currentVol = Mathf.Lerp(boostVolume, baseVolume, t * t * t);
            
            if (mainMusicSource != null)
            {
                mainMusicSource.volume = currentVol;
            }
            yield return null;
        }

        // 최종적으로 정확한 baseVolume으로
        if (mainMusicSource != null)
        {
            mainMusicSource.volume = baseVolume;
        }
    }

    public void EndGame()
    {
        isPlaying = false;
        
        // [FIX] Stop ALL AudioSources in the scene (including spawners, etc.)
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        foreach (var audioSource in allAudioSources)
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
                Debug.Log($"[GameManager] Stopped AudioSource on {audioSource.gameObject.name}");
            }
        }
        
        // [FIX] Clear mainMusicSource clip to prevent audio mixing on next game
        if (mainMusicSource != null)
        {
            mainMusicSource.clip = null;
            Debug.Log("[GameManager] Cleared mainMusicSource clip in EndGame");
        }
        
        Debug.Log($"[GameManager] Total AudioSources stopped: {allAudioSources.Length}");
        
        // Hide Game UI
        if (scoreText != null) scoreText.gameObject.SetActive(false);
        if (comboText != null) comboText.gameObject.SetActive(false);
        
        // Hide other specific UI elements
        if (hpBarFill != null && hpBarFill.transform.parent != null) 
            hpBarFill.transform.parent.gameObject.SetActive(false);
        if (inGameRankGaugeFill != null && inGameRankGaugeFill.transform.parent != null)
            inGameRankGaugeFill.transform.parent.gameObject.SetActive(false);

        // [FIX] Unlock Cursor for UI Interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Hide miscellaneous texts
        TextMeshProUGUI[] allTexts = Object.FindObjectsOfType<TextMeshProUGUI>();
        foreach (var txt in allTexts)
        {
            if (txt.transform.root.name.Contains("GameOver")) continue;
            string n = txt.name.ToLower();
            if (n.Contains("score") || n.Contains("combo") || n.Contains("hp") || n.Contains("time"))
                txt.gameObject.SetActive(false);
        }

        // Cleanup Bubbles
        BubbleNote[] allBubbles = Object.FindObjectsOfType<BubbleNote>();
        foreach (var bubble in allBubbles) Destroy(bubble.gameObject);
        
        StopAnimations(); // [NEW] Stop characters dancing
        
        // Create/Find Canvas
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject cObj = new GameObject("Canvas");
            canvas = cObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            // [FIX] Setup CanvasScaler for responsive UI
            CanvasScaler scaler = cObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            
            cObj.AddComponent<GraphicRaycaster>();
        }
        else
        {
            // Ensure Raycaster exists
            if (canvas.GetComponent<GraphicRaycaster>() == null) canvas.gameObject.AddComponent<GraphicRaycaster>();
            
            // [FIX] Ensure Scaler exists on existing canvas too
            CanvasScaler existingScaler = canvas.GetComponent<CanvasScaler>();
            if (existingScaler == null) existingScaler = canvas.gameObject.AddComponent<CanvasScaler>();
            existingScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            existingScaler.referenceResolution = new Vector2(1920, 1080);
            existingScaler.matchWidthOrHeight = 0.5f;
        }
        canvas.sortingOrder = 9999; // Ensure top

        // [FIX] Ensure EventSystem exists for button clicks
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
             GameObject eventSystem = new GameObject("EventSystem");
             eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
             eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // [FIX] Destroy known overlapping containers to prevent raycast blocking
        GameObject hpC = GameObject.Find("HPBarContainer"); if(hpC) Destroy(hpC);
        GameObject rgC = GameObject.Find("RankGaugeContainer"); if(rgC) Destroy(rgC);
        GameObject pmC = GameObject.Find("PauseMenuPanel"); if(pmC) Destroy(pmC); // From GameUIBuilder

        StartCoroutine(ShowResultAnimation(canvas));
    }

    private IEnumerator ShowResultAnimation(Canvas canvas)
    {
        // [FIX] 0. Cleanup Old Particles
        CleanUpParticles();

        // [FIX] UNIFIED POPUP LAYOUT
        // 1. Main Background Overlay (Dimmed)
        if (gameOverPanel != null) Destroy(gameOverPanel);
        gameOverPanel = new GameObject("GameOverPanel");
        gameOverPanel.transform.SetParent(canvas.transform, false);
        
        RectTransform safeRt = gameOverPanel.AddComponent<RectTransform>();
        safeRt.anchorMin = Vector2.zero; safeRt.anchorMax = Vector2.one;
        safeRt.sizeDelta = Vector2.zero;

        Image overlay = gameOverPanel.AddComponent<Image>();
        overlay.color = new Color(0, 0, 0, 0.85f); // Darker dim
        overlay.raycastTarget = true; // Block input to game

        GameObject popupObj = new GameObject("ResultPopup");
        popupObj.transform.SetParent(gameOverPanel.transform, false);
        Image popupImg = popupObj.AddComponent<Image>();
        // [FIX] Use Customizable Color
        popupImg.color = resultBgColor;
        
        // Add Outline/Stroke if possible (Unity default doesn't have simple stroke, skipping)
        
        RectTransform popRt = popupObj.GetComponent<RectTransform>();
        popRt.anchorMin = new Vector2(0.1f, 0.1f); 
        popRt.anchorMax = new Vector2(0.9f, 0.9f); // 80% screen
        popRt.offsetMin = Vector2.zero; popRt.offsetMax = Vector2.zero;

        // --- CONTENT INSIDE POPUP ---
        
        // A. Header Area (Album + Title)
        // Album Art (Left Big Square)
        GameObject albumObj = new GameObject("AlbumArt");
        albumObj.transform.SetParent(popupObj.transform, false);
        Image albumImg = albumObj.AddComponent<Image>();
        albumImg.color = Color.gray; 

        // [LOGIC] Try to find album art
        int songIndex = isSecondScene ? 1 : 0;
        Sprite targetSprite = null;
        if (resultAlbumArt != null && resultAlbumArt.Length > songIndex) targetSprite = resultAlbumArt[songIndex];
        
        if (targetSprite != null) {
            albumImg.sprite = targetSprite;
            albumImg.color = Color.white;
        }

        RectTransform albRt = albumObj.GetComponent<RectTransform>();
        // [FIX] Spacing Adjustment: Moved Album UP slightly
        albRt.anchorMin = new Vector2(0.05f, 0.50f); // Was 0.45
        albRt.anchorMax = new Vector2(0.35f, 0.90f); // 
        albRt.offsetMin = Vector2.zero; albRt.offsetMax = Vector2.zero;
        albumImg.preserveAspect = true;

        // Song Title (Next to Album, Top)
        GameObject titleObj = new GameObject("SongTitle");
        titleObj.transform.SetParent(popupObj.transform, false);
        TextMeshProUGUI tTxt = titleObj.AddComponent<TextMeshProUGUI>();
        string sTitle = (songTitles != null && songTitles.Length > songIndex) ? songTitles[songIndex] : "Song Name";
        tTxt.text = sTitle;
        tTxt.fontSize = 40; 
        tTxt.fontStyle = FontStyles.Bold;
        tTxt.color = Color.white;
        tTxt.alignment = TextAlignmentOptions.Left;
        
        RectTransform tRt = titleObj.GetComponent<RectTransform>();
        tRt.anchorMin = new Vector2(0.38f, 0.80f); tRt.anchorMax = new Vector2(0.8f, 0.90f);
        tRt.offsetMin = Vector2.zero; tRt.offsetMax = Vector2.zero;

        // Difficulty Badge (Below Title)
        GameObject diffBadge = new GameObject("DiffBadge");
        diffBadge.transform.SetParent(popupObj.transform, false);
        Image dImg = diffBadge.AddComponent<Image>();
        dImg.color = new Color(0, 0.5f, 1f); // Blue
        RectTransform dbRt = diffBadge.GetComponent<RectTransform>();
        dbRt.anchorMin = new Vector2(0.38f, 0.72f); dbRt.anchorMax = new Vector2(0.50f, 0.78f);
        dbRt.offsetMin = Vector2.zero; dbRt.offsetMax = Vector2.zero;
        
        GameObject dbTxtObj = new GameObject("Txt");
        dbTxtObj.transform.SetParent(diffBadge.transform, false);
        TextMeshProUGUI dbTxt = dbTxtObj.AddComponent<TextMeshProUGUI>();
        dbTxt.text = "HARD"; dbTxt.fontSize = 20; dbTxt.alignment = TextAlignmentOptions.Center; dbTxt.color = Color.white;
        RectTransform dbtRt = dbTxtObj.GetComponent<RectTransform>();
        dbtRt.anchorMin = Vector2.zero; dbtRt.anchorMax = Vector2.one; dbtRt.offsetMin = Vector2.zero; dbtRt.offsetMax = Vector2.zero;

        // [FIX] Calc Stats First (Moved Up to ensure variables exist)
        float acc = 1.0f;
        int total = perfectCount + greatCount + badCount + missCount;
        if (total > 0) acc = (perfectCount * 1.0f + greatCount * 0.8f + badCount * 0.5f) / total;
        
        string rChar = "F"; 
        Color rankColor = Color.gray; 
        Color pColor1 = Color.gray; 

        Color pColor2 = Color.white; // [FIX] Re-added pColor2

        if (acc >= 0.95f) { rChar = "S"; rankColor = new Color(1, 0.85f, 0.1f); pColor1 = new Color(1, 0.9f, 0.2f); }
        else if (acc >= 0.85f) { rChar = "A"; rankColor = new Color(1, 0.4f, 0.8f); pColor1 = new Color(1, 0.5f, 0.9f); }
        else if (acc >= 0.70f) { rChar = "B"; rankColor = new Color(0.2f, 0.6f, 1f); pColor1 = new Color(0, 0.8f, 1f); }
        else if (acc >= 0.50f) { rChar = "C"; rankColor = Color.green; pColor1 = Color.green; }
        else { rChar = "F"; rankColor = new Color(0.8f, 0.1f, 0.1f); pColor1 = Color.red; pColor2 = Color.black; }

        // B. RANK & SCORE (Right Side Emphasis)
        // Rank (Huge, Right area - Moved UP)
        GameObject rankObj = new GameObject("RankBig");
        rankObj.transform.SetParent(popupObj.transform, false);
        RectTransform rRt = rankObj.AddComponent<RectTransform>();
        
        // Position Rank Higher
        rRt.anchorMin = new Vector2(0.65f, 0.55f); // Was 0.45
        rRt.anchorMax = new Vector2(0.95f, 0.90f); // Was 0.85
        rRt.offsetMin = Vector2.zero; rRt.offsetMax = Vector2.zero;
        
        TextMeshProUGUI rTxt = rankObj.AddComponent<TextMeshProUGUI>();
        rTxt.text = rChar;
        rTxt.fontSize = 220; 
        rTxt.fontStyle = FontStyles.Bold | FontStyles.Italic;
        rTxt.alignment = TextAlignmentOptions.Center;
        
        // [FIX] Metallic Gradient for Rank
        rTxt.enableVertexGradient = true;
        VertexGradient rankGrad = new VertexGradient(
            new Color(1f, 1f, 1f), // TL White
            new Color(1f, 1f, 1f), // TR White
            rankColor, // BL Main Color
            rankColor  // BR Main Color
        );
        // Specialized Gradients
        if (rChar == "S") rankGrad = new VertexGradient(new Color(1f, 1f, 0.8f), new Color(1f, 1f, 0.8f), new Color(1f, 0.6f, 0f), new Color(1f, 0.5f, 0f)); // Gold
        else if (rChar == "A") rankGrad = new VertexGradient(Color.white, Color.white, new Color(0.7f, 0.7f, 0.8f), new Color(0.5f, 0.5f, 0.6f)); // Silver/Steel
        else if (rChar == "B") rankGrad = new VertexGradient(new Color(1f, 0.8f, 0.6f), new Color(1f, 0.8f, 0.6f), new Color(0.6f, 0.4f, 0.2f), new Color(0.5f, 0.3f, 0.1f)); // Bronze
        
        rTxt.colorGradient = rankGrad;
        rTxt.color = Color.white; // Base color white to let gradient show
        rTxt.alpha = 0; // Start Invisible

        // Outline for Rank
        var rOl = rankObj.AddComponent<UnityEngine.UI.Outline>();
        rOl.effectColor = new Color(0,0,0,0.8f);
        rOl.effectDistance = new Vector2(5, -5);

        // SCORE (Moved Left, Below Difficulty)
        GameObject scoreObj = new GameObject("ScoreDisplay");
        scoreObj.transform.SetParent(popupObj.transform, false);
        RectTransform scRt = scoreObj.AddComponent<RectTransform>();
        // Position below "HARD" badge
        scRt.anchorMin = new Vector2(0.38f, 0.58f); 
        scRt.anchorMax = new Vector2(0.90f, 0.68f);
        scRt.offsetMin = Vector2.zero; scRt.offsetMax = Vector2.zero;
        
        TextMeshProUGUI scoreTxt = scoreObj.AddComponent<TextMeshProUGUI>();
        scoreTxt.text = "SCORE  000000"; 
        scoreTxt.alignment = TextAlignmentOptions.Left; // Align Left with Title/Badge
        scoreTxt.fontSize = 55;
        scoreTxt.fontStyle = FontStyles.Bold | FontStyles.Italic;
        // Silver Metallic Gradient for Score
        scoreTxt.enableVertexGradient = true;
        scoreTxt.colorGradient = new VertexGradient(
            new Color(0.9f, 0.9f, 0.9f), 
            new Color(0.9f, 0.9f, 0.9f), 
            new Color(0.4f, 0.4f, 0.5f), 
            new Color(0.3f, 0.3f, 0.4f)
        );
        scoreTxt.color = Color.white;
        
        // Add specific Outline to Score
        var sOl = scoreObj.AddComponent<UnityEngine.UI.Outline>();
        sOl.effectColor = Color.black; 
        sOl.effectDistance = new Vector2(2, -2);

        // C. Stats (Middle Strip)
        GameObject statsObj = new GameObject("StatsRow");
        statsObj.transform.SetParent(popupObj.transform, false);
        RectTransform stRt = statsObj.AddComponent<RectTransform>();
        // [FIX] Spacing Adjustment: Moved Stats DOWN to create gap with Album
        stRt.anchorMin = new Vector2(0.05f, 0.20f);  // Was 0.25
        stRt.anchorMax = new Vector2(0.70f, 0.40f);  // Was 0.45
        stRt.offsetMin = Vector2.zero; stRt.offsetMax = Vector2.zero;
        
        // Background for Stats
        Image stBg = statsObj.AddComponent<Image>();
        stBg.color = new Color(0,0,0,0.3f); 
        
        HorizontalLayoutGroup hlg = statsObj.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleCenter; hlg.spacing = 30;
        
         void CreateStat(string l, int v) {
            GameObject g = new GameObject(l);
            g.transform.SetParent(statsObj.transform, false);
            VerticalLayoutGroup vl = g.AddComponent<VerticalLayoutGroup>();
            vl.childAlignment = TextAnchor.MiddleCenter;
            
            TextMeshProUGUI tl = new GameObject("L").AddComponent<TextMeshProUGUI>();
            tl.transform.SetParent(g.transform, false);
            tl.text = l; tl.fontSize = 20; tl.color = Color.gray; tl.alignment = TextAlignmentOptions.Center;
            
            TextMeshProUGUI tv = new GameObject("V").AddComponent<TextMeshProUGUI>();
            tv.transform.SetParent(g.transform, false);
            tv.text = v.ToString(); tv.fontSize = 30; tv.color = Color.white; tv.alignment = TextAlignmentOptions.Center; tv.fontStyle = FontStyles.Bold;
        }
        CreateStat("PERFECT", perfectCount);
        CreateStat("GREAT", greatCount);
        CreateStat("BAD", badCount);
        CreateStat("MISS", missCount);
        CreateStat("COMBO", maxCombo);

        // D. Buttons (Bottom)
        // Ensure they are Raycast Targets!
        GameObject actionPanel = new GameObject("ActionButtons");
        actionPanel.transform.SetParent(popupObj.transform, false);
        RectTransform actRt = actionPanel.AddComponent<RectTransform>();
        actRt.anchorMin = new Vector2(0.2f, 0.05f); actRt.anchorMax = new Vector2(0.8f, 0.18f); // Bottom area
        actRt.offsetMin = Vector2.zero; actRt.offsetMax = Vector2.zero;
        
        HorizontalLayoutGroup actLayout = actionPanel.AddComponent<HorizontalLayoutGroup>();
        actLayout.childAlignment = TextAnchor.MiddleCenter;
        actLayout.spacing = 50;
        actLayout.childControlHeight = true; actLayout.childControlWidth = true;

        void CreatePopupBtn(string label, Color col, UnityEngine.Events.UnityAction action) {
            GameObject btnObj = new GameObject("Btn_" + label.Replace(" ", ""));
            btnObj.transform.SetParent(actionPanel.transform, false);
            Image bImg = btnObj.AddComponent<Image>();
            bImg.color = col;
            bImg.raycastTarget = true; // IMPORTANT

            Button btn = btnObj.AddComponent<Button>();
            btn.onClick.AddListener(action);
            
            // Add Text
            GameObject tObj = new GameObject("Text");
            tObj.transform.SetParent(btnObj.transform, false);
            TextMeshProUGUI t = tObj.AddComponent<TextMeshProUGUI>();
            t.text = label; t.fontSize = 28; t.color = Color.white; t.alignment = TextAlignmentOptions.Center; t.fontStyle = FontStyles.Bold;
            t.raycastTarget = false; // IMPORTANT: Do not block raycast
            
            RectTransform tr = tObj.GetComponent<RectTransform>();
            tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
            tr.offsetMin = Vector2.zero; tr.offsetMax = Vector2.zero;
        }

        CreatePopupBtn("RETRY", retryBtnColor, RestartGame);
        CreatePopupBtn("MAIN MENU", mainBtnColor, ReturnToLobby);
        
        // --- ANIMATION ---
        // Score Count
        float elapsedTime = 0f;
        while(elapsedTime < 1.0f) {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / 1.0f;
            t = 1 - (1-t)*(1-t); // Ease Out
            int dScore = (int)Mathf.Lerp(0, score, t);
            
            // SCORE ANIMATION (With Label "SCORE")
            scoreTxt.text = $"SCORE  {dScore:N0}";
            yield return null;
        }
        scoreTxt.text = $"SCORE  {score:N0}";
        
        // Show Buttons
        if (actionPanel != null) actionPanel.SetActive(true);
        
        // Rank Stamp Animation (Emphasis)
        if (rTxt != null)
        {
            rTxt.gameObject.SetActive(true);
            // Huge Scale Start
            rTxt.transform.localScale = Vector3.one * 5.0f; 
            // Huge Scale Start
            rTxt.transform.localScale = Vector3.one * 5.0f; 
            
            // NOTE: Alpha must be handled carefully with Gradient. Gradient alpha is multiplied by vertex alpha.
            rTxt.alpha = 0f; 

            float rElapsed = 0;
            float rDur = 0.4f;
            while(rElapsed < rDur) {
                 rElapsed += Time.unscaledDeltaTime;
                 float t = rElapsed / rDur;
                 // Easing: Bounce Out
                 float s = Mathf.Lerp(5.0f, 1.0f, t * t); 
                 rTxt.alpha = t; // Fade In
                 rTxt.transform.localScale = Vector3.one * s; 
                 yield return null;
            }
            // Shake Effect
            Vector3 basePos = rTxt.transform.localPosition;
            for(int i=0; i<5; i++) {
                rTxt.transform.localPosition = basePos + (Vector3)(UnityEngine.Random.insideUnitCircle * 10f);
                yield return new WaitForSecondsRealtime(0.02f);
            }
            rTxt.transform.localPosition = basePos;
            rTxt.transform.localScale = Vector3.one;
            rTxt.alpha = 1f;
        }

        // Particles
        if (popupObj != null) 
             SpawnResultParticles(pColor1, pColor2, popupObj.transform.position);

        // Unlock Cursor again just in case
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        yield return null;
    }

    private void CleanUpParticles()
    {
        // Find by type implies we find ALL particles. Be careful not to kill gameplay ones if gameplay is active?
        // But this is EndGame.
        ParticleSystem[] allPs = FindObjectsOfType<ParticleSystem>();
        foreach(var ps in allPs)
        {
             // heuristic: don't kill our new ones if called repeatedly (won't happen)
             // kill everything else to be safe?
             // Or assumes the "Confetti" is a root object.
             if (ps.transform.parent == null || ps.name.Contains("Confetti") || ps.name.Contains("Firework"))
             {
                 Destroy(ps.gameObject);
             }
        }
    }

    private void SpawnResultParticles(Color c1, Color c2, Vector3 spawnPos)
    {
        GameObject pObj = new GameObject("ResultParticles");
        pObj.transform.position = spawnPos; // Spawn at Rank Text Position
        
        ParticleSystem ps = pObj.AddComponent<ParticleSystem>();
        var renderer = pObj.GetComponent<ParticleSystemRenderer>();
        // [FIX] Force "Default-Particle" for Soft Glow texture
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit")); 
        renderer.material.SetFloat("_Mode", 2); // Fade
        
        // Try to load a known circle texture or just use soft particle default
        // We will trust Standard Unlit with soft soft particles.
        
        var main = ps.main;
        main.startColor = new ParticleSystem.MinMaxGradient(c1, c2);
        main.startSize = new ParticleSystem.MinMaxCurve(0.5f, 1.2f); // Soft Glow
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 1.0f); // Slow Rise
        main.startLifetime = 2.5f;
        main.maxParticles = 50; 
        main.loop = true;
        
        var emission = ps.emission;
        emission.rateOverTime = 10; 
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere; 
        shape.radius = 3.5f; // Localized
        
        var rot = ps.rotationOverLifetime;
        rot.enabled = true;
        rot.z = 20f;
    }

    public void AddScore(int points)
    {
        score += points;
        combo++;
        if (combo > maxCombo) maxCombo = combo;
        UpdateUI();
    }

    // [NEW] Stop Character Animations
    private void StopAnimations()
    {
        Animator[] anims = FindObjectsOfType<Animator>();
        foreach (var anim in anims)
        {
             anim.enabled = false; // Simple freeze
             // OR: anim.speed = 0;
        }
    }

    // [NEW] Visual Judgment Logic (Enhanced Position)
    private void ShowJudgment(string type, Color color, Vector3 worldPos = default)
    {
        // 1. Find or Create Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        // 2. Create Text Object
        GameObject jobj = new GameObject("Judgment_" + type);
        jobj.transform.SetParent(canvas.transform, false);
        
        RectTransform rt = jobj.AddComponent<RectTransform>();
        
        // [FIX] Position Logic
        if (worldPos != default)
        {
             // Convert World to Screen
             Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
             // Convert Screen to Canvas Local
             Vector2 localPos;
             RectTransformUtility.ScreenPointToLocalPointInRectangle(
                 canvas.transform as RectTransform, 
                 screenPos, 
                 canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main, 
                 out localPos);
             
             rt.anchoredPosition = localPos + new Vector2(0, 100); // 100px above hit
        }
        else
        {
             rt.anchoredPosition = new Vector2(0, 150); // Fallback Center
        }
        
        TextMeshProUGUI tmp = jobj.AddComponent<TextMeshProUGUI>();
        tmp.text = type;
        tmp.fontSize = 65; // Slightly larger
        tmp.fontStyle = FontStyles.Bold | FontStyles.Italic;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color;
        
        // Add Outline
        var outline = jobj.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor = new Color(0,0,0,0.8f); // Darker outline
        outline.effectDistance = new Vector2(3, -3);

        // 3. Show Combo below
        if (combo > 1) {
             GameObject cobj = new GameObject("ComboNum");
             cobj.transform.SetParent(jobj.transform, false);
             TextMeshProUGUI ctmp = cobj.AddComponent<TextMeshProUGUI>();
             ctmp.text = combo + " COMBO";
             ctmp.fontSize = 40;
             ctmp.fontStyle = FontStyles.Italic;
             ctmp.color = Color.Lerp(Color.white, color, 0.5f); // Tinted combo
             ctmp.alignment = TextAlignmentOptions.Center;
             cobj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -70);
        }

        // 4. Animate (Popup & Fade)
        StartCoroutine(AnimateJudgment(rt, tmp, jobj));
    }

    private IEnumerator AnimateJudgment(RectTransform rt, TextMeshProUGUI tmp, GameObject obj)
    {
        float t = 0;
        // Pop Up (Bounce)
        while(t < 0.15f) {
            t += Time.deltaTime;
            float p = t / 0.15f;
            // BackOut Easing
            float s = Mathf.Lerp(0.5f, 1.2f, Mathf.Sin(p * Mathf.PI * 0.5f) + 0.2f); 
            rt.localScale = Vector3.one * s;
            yield return null;
        }
        rt.localScale = Vector3.one;

        // Wait
        yield return new WaitForSeconds(0.4f); 

        // Fade Out & Float Up
        t = 0;
        Color startColor = tmp.color;
        Vector2 startPos = rt.anchoredPosition;
        while(t < 0.3f) {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, t/0.3f);
            tmp.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            rt.anchoredPosition = startPos + new Vector2(0, 50 * (t/0.3f)); 
            yield return null;
        }

        Destroy(obj);
    }

    public void AddPerfect(Vector3 pos = default) 
    { 
        perfectCount++; 
        AddScore(100); 
        currentHp = Mathf.Min(currentHp + 2f, maxHp); 
        ShowJudgment("PERFECT", new Color(0, 0.8f, 1f), pos); // Cyan
        PerfectHitBoost(); 
    }
    public void AddGreat(Vector3 pos = default) 
    { 
        greatCount++; 
        AddScore(80); 
        currentHp = Mathf.Min(currentHp + 1f, maxHp); 
        ShowJudgment("GREAT", Color.green, pos);
    }
    public void AddBad(Vector3 pos = default) 
    { 
        badCount++; 
        AddScore(50); 
        combo = 0; 
        currentHp = Mathf.Max(currentHp - 5f, 0f); 
        ShowJudgment("BAD", new Color(1, 0.5f, 0), pos); // Orange
        UpdateUI(); 
    }
    public void AddMiss(Vector3 pos = default) 
    { 
        missCount++; 
        combo = 0; 
        currentHp = Mathf.Max(currentHp - 10f, 0f); 
        ShowJudgment("MISS", Color.red, pos);
        OnBubbleMiss(); 
    }

    public void ResetCombo()
    {
        combo = 0;
        UpdateUI();
    }

    public void PlaySFX(AudioClip clip, float volume = 1.0f)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    private void UpdateUI()
    {
        if (scoreText != null) 
        {
            // [FIX] REMOVED Color Tags to preserver Metallic Silver Gradient
            scoreText.text = $"SCORE   <size=120%>{score:N0}</size>"; 
        }

        
        // [REMOVED] Legacy Pink Combo Text (Replaced by floating combo)
        if (comboText != null) comboText.gameObject.SetActive(false);
        
        // [FIX] HP Bar Update (Image Fill)
        if (hpBarFill != null)
        {
            float hpPercent = (float)currentHp / maxHp;
            hpBarFill.fillAmount = hpPercent;
            
            // [FIX] Apply color every frame so Inspector changes allow real-time editing
            hpBarFill.color = mentalGaugeColor; 
            
            // Update Numeric Display
            if (hpNumericText != null)
            {
                hpNumericText.text = $"{(int)currentHp} / {(int)maxHp}";
            }
        }

        else
        {
            // [DEBUG] HP Bar missing?
            // Debug.LogWarning("[GameManager UI] hpBarFill is NULL! Attempting to find...");
            GameObject foundFill = GameObject.Find("HPBarContainer/FillParent/Fill");
            if (foundFill != null) hpBarFill = foundFill.GetComponent<Image>();
        }

        // [FIX] Cumulative Rank Gauge Update
        if (inGameRankGaugeFill != null)
        {
            float currentWeightedScore = (perfectCount * 1.0f) + (greatCount * 0.8f) + (badCount * 0.5f);
            
            float fill = 0f;
            if (maxPossibleScore > 0)
            {
                fill = currentWeightedScore / maxPossibleScore;
            }
            
            // Limit to 1.0
            if (fill > 1.0f) fill = 1.0f;

            inGameRankGaugeFill.fillAmount = fill;
            
            // Rank Color
            if (fill >= 0.95f) inGameRankGaugeFill.color = new Color(1f, 0.8f, 0.2f); // S (Gold)
            else if (fill >= 0.85f) inGameRankGaugeFill.color = new Color(1f, 0.4f, 0.8f); // A (Pink)
            else if (fill >= 0.70f) inGameRankGaugeFill.color = new Color(0.2f, 0.6f, 1f); // B (Blue)
            else inGameRankGaugeFill.color = Color.gray; // C/Building
        }

    }

    public void RestartGame()
    {
        Debug.Log("[GameManager] RestartGame: Destroying GameManager and reloading scene for clean restart");
        
        // [FIX] Stop ALL AudioSources before restarting
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        foreach (var audioSource in allAudioSources)
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
                Debug.Log($"[GameManager] Stopped AudioSource on {audioSource.gameObject.name} (Restart)");
            }
        }
        
        // [CRITICAL FIX] Destroy gameOverPanel
        if (gameOverPanel != null)
        {
            Destroy(gameOverPanel);
            gameOverPanel = null;
        }
        
        Time.timeScale = 1f;
        
        // [CRITICAL FIX] Destroy GameManager itself to ensure clean state
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        Destroy(this.gameObject); // This will destroy the entire GameManager
        
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    // [NEW] Game Flow Methods
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (mainMusicSource != null && mainMusicSource.isPlaying)
        {
            mainMusicSource.Pause();
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (mainMusicSource != null)
        {
            mainMusicSource.UnPause();
        }
    }

    public void ReturnToLobby()
    {
        // [FIX] Stop ALL AudioSources before returning to lobby
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        foreach (var audioSource in allAudioSources)
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
                Debug.Log($"[GameManager] Stopped AudioSource on {audioSource.gameObject.name} (ReturnToLobby)");
            }
        }
        
        Time.timeScale = 1f;
        // SchoolLobbyManager가 있는 Main 씬으로 이동
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
    }

    // [NEW] Shop Methods
    public bool TryPurchase(int cost, string itemName)
    {
        if (ownedItems.Contains(itemName)) return false; // 이미 보유 중

        if (coins >= cost)
        {
            coins -= cost;
            ownedItems.Add(itemName);
            Debug.Log($"[GameManager] Purchase Success: {itemName} (Remaining Coins: {coins})");
            return true;
        }
        else
        {
            Debug.Log($"[GameManager] Purchase Failed: Not enough coins (Required: {cost}, Current: {coins})");
            return false;
        }
    }

    // [NEW] In-Game Rank Gauge Creation (Moved from GameUIBuilder)
    private void CreateInGameRankGauge()
    {
        // 1. Find Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        // 2. Check if already exists
        if (canvas.transform.Find("InGameRankGauge") != null) return;

        Debug.Log("[GameManager] Creating In-Game Rank Gauge...");

        // Container
        GameObject gaugeObj = new GameObject("InGameRankGauge");
        gaugeObj.transform.SetParent(canvas.transform, false);
        
        RectTransform rt = gaugeObj.AddComponent<RectTransform>();
        // Position: Bottom Right
        rt.anchorMin = new Vector2(1f, 0f); 
        rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(1f, 0f);
        rt.anchoredPosition = new Vector2(-50, 50); // Padding from corner
        rt.sizeDelta = new Vector2(300, 30); // Wider and visible

        // Background
        Image bg = gaugeObj.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f); // Darker background

        // Label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(gaugeObj.transform, false);
        TextMeshProUGUI lbl = labelObj.AddComponent<TextMeshProUGUI>();
        lbl.text = "RANK ACCURACY";
        lbl.fontSize = 24;
        lbl.fontStyle = FontStyles.Bold;
        lbl.color = Color.white;
        lbl.alignment = TextAlignmentOptions.Right;
        
        RectTransform lblRt = labelObj.GetComponent<RectTransform>();
        lblRt.anchorMin = new Vector2(1, 1); lblRt.anchorMax = new Vector2(1, 1);
        lblRt.pivot = new Vector2(1, 0);
        lblRt.anchoredPosition = new Vector2(0, 10); // Above bar
        lblRt.sizeDelta = new Vector2(300, 30);

        // Fill Image
        GameObject fillObj = new GameObject("InGameRankGaugeFill"); 
        fillObj.transform.SetParent(gaugeObj.transform, false);
        
        Image fill = fillObj.AddComponent<Image>();
        fill.sprite = GetWhiteSprite(); // [FIX]
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = (int)Image.OriginHorizontal.Left;
        fill.color = Color.gray; 
        
        RectTransform fillRt = fillObj.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = Vector2.zero; fillRt.offsetMax = Vector2.zero;

        // Assign to field
        this.inGameRankGaugeFill = fill;

        // Threshold Markers Helper
        void CreateHudMarker(float percent, string markLabel)
        {
            GameObject m = new GameObject("Marker_" + markLabel);
            m.transform.SetParent(gaugeObj.transform, false);
            
            Image img = m.AddComponent<Image>();
            img.color = new Color(1, 1, 1, 0.5f);
            
            RectTransform mrt = m.GetComponent<RectTransform>();
            mrt.anchorMin = new Vector2(percent, 0);
            mrt.anchorMax = new Vector2(percent, 1);
            mrt.sizeDelta = new Vector2(2, 0);
            mrt.anchoredPosition = Vector2.zero;
            
            // Marker Label (S, A, B, C)
            GameObject txt = new GameObject("Txt");
            txt.transform.SetParent(m.transform, false);
            TextMeshProUGUI t = txt.AddComponent<TextMeshProUGUI>();
            t.text = markLabel;
            t.fontSize = 18;
            t.color = Color.yellow;
            t.alignment = TextAlignmentOptions.Center;
            
            RectTransform trt = txt.GetComponent<RectTransform>();
            trt.anchoredPosition = new Vector2(0, -25); // Below marker
        }

        CreateHudMarker(0.50f, "C"); 
        CreateHudMarker(0.70f, "B");
        CreateHudMarker(0.85f, "A");
        CreateHudMarker(0.95f, "S");
    }

    // [NEW] Logic to Create HP Bar (Repl. Slider)
    private void CreateHPBar()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        // Ensure canvas renders on top if ScreenSpaceOverlay
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay) canvas.sortingOrder = 99; // High but below result (9999)

        // Try to find existing old slider and destroy/hide it?
        Transform oldSlider = canvas.transform.Find("HpSlider"); 
        if (oldSlider != null) oldSlider.gameObject.SetActive(false);

        if (canvas.transform.Find("HPBarContainer") != null) return;
        
        Debug.Log("[GameManager] Creating Modern LIFE Bar...");

        GameObject barObj = new GameObject("HPBarContainer");
        barObj.transform.SetParent(canvas.transform, false);
        
        // [FIX] Ensure CanvasScaler exists for consistent sizing across resolutions
        UnityEngine.UI.CanvasScaler scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
        if (scaler == null) {
            scaler = canvas.gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        } else if (scaler.uiScaleMode == UnityEngine.UI.CanvasScaler.ScaleMode.ConstantPixelSize) {
            // Force upgrade to Screen Size if it's default
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }

        RectTransform rt = barObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f); // Top Center
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        // [FIX] Move further down to avoid Score overlapping (was -80)
        rt.anchoredPosition = new Vector2(0, -130); 
        rt.sizeDelta = new Vector2(900, 40); // Thinner and longer

        // [LIFE LABEL - Modern Cyan Gradient]
        GameObject lblObj = new GameObject("LifeLabel");
        lblObj.transform.SetParent(barObj.transform, false);
        TextMeshProUGUI lbl = lblObj.AddComponent<TextMeshProUGUI>();
        lbl.text = "MENTAL";
        lbl.fontSize = 26; // Adjusted for thinner bar
        lbl.fontStyle = FontStyles.Bold;
        lbl.alignment = TextAlignmentOptions.Center; // Center to prevent cutoff
        
        // Cyan Gradient on Label
        lbl.enableVertexGradient = true;
        lbl.colorGradient = new VertexGradient(
            new Color(0.3f, 1f, 1f), // Top Cyan
            new Color(0.3f, 1f, 1f),
            new Color(0.1f, 0.5f, 0.7f), // Bottom Dark Cyan
            new Color(0.1f, 0.5f, 0.7f)
        );
        
        RectTransform lblRt = lblObj.GetComponent<RectTransform>();
        lblRt.anchorMin = new Vector2(0, 0); lblRt.anchorMax = new Vector2(0.2f, 1); // Wider area
        lblRt.pivot = new Vector2(0, 0.5f);
        lblRt.anchoredPosition = new Vector2(5, 0); // Less padding
        lblRt.sizeDelta = new Vector2(0, 0); // Use anchor stretching

        // [BAR BACKGROUND - Dark with Outline]
        GameObject bgObj = new GameObject("BarBG");
        bgObj.transform.SetParent(barObj.transform, false);
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.sprite = GetWhiteSprite();
        bgImg.color = new Color(0.05f, 0.05f, 0.15f, 0.95f); // Deep dark blue-black
        
        RectTransform bgRt = bgObj.GetComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0.28f, 0); bgRt.anchorMax = new Vector2(1, 1); // More space for MENTAL
        bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;

        // Add Outline for 3D effect
        var outline = bgObj.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor = new Color(0.3f, 0.9f, 1f, 0.8f); // Cyan outline
        outline.effectDistance = new Vector2(2, -2);

        // [FILL PARENT - Padding]
        GameObject fillParent = new GameObject("FillParent");
        fillParent.transform.SetParent(bgObj.transform, false);
        RectTransform fpRt = fillParent.AddComponent<RectTransform>();
        fpRt.anchorMin = Vector2.zero; fpRt.anchorMax = Vector2.one;
        fpRt.offsetMin = new Vector2(3, 3); fpRt.offsetMax = new Vector2(-3, -3);

        // [FILL - Cyan Gradient]
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillParent.transform, false);
        Image fill = fillObj.AddComponent<Image>();
        fill.sprite = GetWhiteSprite();
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = (int)Image.OriginHorizontal.Left;
        
        // Darker Blue (less bright, more blue than cyan)
        fill.color = new Color(0.15f, 0.6f, 0.95f); // Softer blue
        
        RectTransform fillRt = fillObj.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = Vector2.zero; fillRt.offsetMax = Vector2.zero;

        this.hpBarFill = fill;

        // [NUMERIC DISPLAY - "356 / 356" style]
        GameObject numObj = new GameObject("HPNumeric");
        numObj.transform.SetParent(bgObj.transform, false);
        TextMeshProUGUI numTxt = numObj.AddComponent<TextMeshProUGUI>();
        numTxt.text = $"{(int)currentHp} / {(int)maxHp}";
        numTxt.fontSize = 22; // Larger numeric font
        numTxt.fontStyle = FontStyles.Bold;
        numTxt.alignment = TextAlignmentOptions.Center;
        numTxt.color = Color.white;
        
        RectTransform numRt = numObj.GetComponent<RectTransform>();
        numRt.anchorMin = Vector2.zero; numRt.anchorMax = Vector2.one;
        numRt.offsetMin = Vector2.zero; numRt.offsetMax = Vector2.zero;
        
        // Store reference for updating
        this.hpNumericText = numTxt;
        
        // [3D DEPTH] Add Shadow to entire bar container for depth
        var shadow = barObj.AddComponent<UnityEngine.UI.Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.7f);
        shadow.effectDistance = new Vector2(0, -4);
    }


}