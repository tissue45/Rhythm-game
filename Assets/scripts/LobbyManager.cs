using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    [Header("Scene Names")]
    public string gameSceneName = "Main"; // [FIX] Default to Main scene

    private void Start()
    {
        // [FIX] Auto-find buttons if they are not hooked up in Inspector
        var startBtn = GameObject.Find("StartButton")?.GetComponent<UnityEngine.UI.Button>();
        if (startBtn != null) 
        {
            startBtn.onClick.RemoveAllListeners();
            startBtn.onClick.AddListener(StartGame);
            startBtn.gameObject.SetActive(true); // Ensure visible
        }

        var quitBtn = GameObject.Find("QuitButton")?.GetComponent<UnityEngine.UI.Button>();
        if (quitBtn != null)
        {
            quitBtn.onClick.RemoveAllListeners();
            quitBtn.onClick.AddListener(QuitGame);
            quitBtn.gameObject.SetActive(true); // Ensure visible
        }
    }

    public void StartGame()
    {
        Debug.Log($"[LobbyManager] Loading Main Scene...");
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        // 어플리케이션 종료
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        Debug.Log("Game Quit"); 
    }
}
