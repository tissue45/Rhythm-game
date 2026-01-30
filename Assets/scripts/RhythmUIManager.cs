using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 모든 버튼에 리듬게임 스타일 자동 적용
/// </summary>
public class RhythmUIManager : MonoBehaviour
{
    [Header("GAME START 버튼 강조")]
    public float gameStartSizeMultiplier = 1.3f;
    public Color gameStartGlowColor = new Color(0f, 1f, 0.5f); // 밝은 녹색

    void Start()
    {
        Invoke("ApplyRhythmStyle", 1.5f);
    }

    void ApplyRhythmStyle()
    {
        Button[] allButtons = FindObjectsOfType<Button>(true);

        foreach (Button btn in allButtons)
        {
            // [FIX] LOGIN 버튼은 제외 (FixAllButtons에서 이미 설정됨)
            if (btn.name == "LOGIN" || btn.name.Contains("Login"))
            {
                Debug.Log($"[RhythmUIManager] Skipping LOGIN button: {btn.name}");
                continue;
            }

            if (btn.GetComponent<RhythmButtonStyle>() != null) continue;

            RhythmButtonStyle style = btn.gameObject.AddComponent<RhythmButtonStyle>();

            // GAME START 버튼 특별 처리
            if (IsGameStartButton(btn.gameObject))
            {
                style.isMainButton = true;
                // 스케일 조정은 하지 않음 (LayoutElement가 크기 제어)
            }
            else if (btn.name.Contains("Exit") || btn.name.Contains("Quit"))
            {
                style.isTextOnly = true;
            }
        }
    }

    bool IsGameStartButton(GameObject btnObj)
    {
        string name = btnObj.name.ToLower();
        return name.Contains("start") || name.Contains("game");
    }

    // EnhanceGameStartButton 메서드는 이제 불필요하므로 제거하거나 통합됨
}
