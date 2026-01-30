using UnityEngine;

/// <summary>
/// 씬 시작 시 자동으로 UI를 설정하는 스크립트
/// LobbyUIBuilder를 대체하여 깔끔한 UI만 표시
/// </summary>
[DefaultExecutionOrder(-100)] // 다른 스크립트보다 먼저 실행
public class AutoSetupUI : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("[AutoSetupUI] Starting UI cleanup...");

        // 1. LobbyUIBuilder 완전히 비활성화
        DisableAllLobbyUIBuilders();

        // 2. 잘못된 UI 요소들 삭제
        CleanupBadUI();

        Debug.Log("[AutoSetupUI] UI cleanup complete!");
    }

    private void DisableAllLobbyUIBuilders()
    {
        LobbyUIBuilder[] builders = Object.FindObjectsByType<LobbyUIBuilder>(FindObjectsSortMode.None);
        foreach (var builder in builders)
        {
            builder.enabled = false;
            Debug.Log($"[AutoSetupUI] Disabled LobbyUIBuilder on {builder.gameObject.name}");
        }
    }

    private void CleanupBadUI()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("[AutoSetupUI] No Canvas found!");
            return;
        }

        // Gemini가 만든 UI 요소들 삭제
        string[] badUINames = {
            "StartButton", "QuitButton", "ConnectionPanel",
            "TitleText", "BackgroundPanel", "GameBorder",
            "CancelButton"
        };

        int deletedCount = 0;
        foreach (string badName in badUINames)
        {
            GameObject badObj = GameObject.Find(badName);
            if (badObj != null)
            {
                Destroy(badObj);
                deletedCount++;
                Debug.Log($"[AutoSetupUI] Deleted bad UI: {badName}");
            }
        }

        Debug.Log($"[AutoSetupUI] Deleted {deletedCount} bad UI elements");
    }
}
