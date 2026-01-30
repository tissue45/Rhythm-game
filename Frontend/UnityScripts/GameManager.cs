using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 게임 전체 흐름을 관리하는 메인 매니저
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public bool isPlaying = false;
    public bool isPaused = false;

    [Header("Score")]
    public int score = 0;
    public int combo = 0;
    public int maxCombo = 0;

    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;

    [Header("Audio")]
    public AudioSource musicSource;
    public AudioClip gameMusic;

    private void Awake()
    {
        // Singleton 패턴
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        gameOverPanel.SetActive(false);
        UpdateUI();
    }

    /// <summary>
    /// 게임 시작
    /// </summary>
    public void StartGame()
    {
        isPlaying = true;
        isPaused = false;
        score = 0;
        combo = 0;
        maxCombo = 0;

        // 음악 재생
        if (musicSource != null && gameMusic != null)
        {
            musicSource.clip = gameMusic;
            musicSource.Play();
        }

        UpdateUI();
    }

    /// <summary>
    /// 게임 일시정지
    /// </summary>
    public void PauseGame()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        
        if (musicSource != null)
        {
            if (isPaused)
                musicSource.Pause();
            else
                musicSource.UnPause();
        }
    }

    /// <summary>
    /// 게임 종료
    /// </summary>
    public void EndGame()
    {
        isPlaying = false;
        
        if (musicSource != null)
        {
            musicSource.Stop();
        }

        // 게임 오버 UI 표시
        gameOverPanel.SetActive(true);
        finalScoreText.text = $"Score: {score}\nMax Combo: {maxCombo}";
    }

    /// <summary>
    /// 점수 추가
    /// </summary>
    public void AddScore(int points)
    {
        score += points;
        combo++;
        
        if (combo > maxCombo)
        {
            maxCombo = combo;
        }

        UpdateUI();
    }

    /// <summary>
    /// 콤보 초기화 (미스 시)
    /// </summary>
    public void ResetCombo()
    {
        combo = 0;
        UpdateUI();
    }

    /// <summary>
    /// UI 업데이트
    /// </summary>
    private void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }

        if (comboText != null)
        {
            if (combo > 0)
            {
                comboText.text = $"Combo: {combo}";
                comboText.gameObject.SetActive(true);
            }
            else
            {
                comboText.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 게임 재시작
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}
