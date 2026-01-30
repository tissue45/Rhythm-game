using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameLobby : MonoBehaviour
{
    [Header("UI References")]
    public GameObject settingsPanel;

    [Header("Scene Names")]
    public string gameSceneName = "Game";

    [Header("Audio")]
    public AudioSource backgroundMusic;

    private void Start()
    {
        if (backgroundMusic != null && !backgroundMusic.isPlaying)
        {
            backgroundMusic.Play();
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    public void StartGame()
    {
        Debug.Log($"씬 로드 중: {gameSceneName}");
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("게임 종료 중...");
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            bool isActive = settingsPanel.activeSelf;
            settingsPanel.SetActive(!isActive);
        }
    }
}