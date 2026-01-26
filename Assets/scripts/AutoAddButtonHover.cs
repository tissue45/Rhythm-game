using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 씬의 모든 버튼에 SimpleButtonHover 자동 추가
/// </summary>
public class AutoAddButtonHover : MonoBehaviour
{
    void Start()
    {
        Invoke("AddHoverToAllButtons", 1.5f);
    }

    void AddHoverToAllButtons()
    {
        Button[] allButtons = FindObjectsOfType<Button>(true);
        
        foreach (Button btn in allButtons)
        {
            // [FIX] LOGIN 버튼은 제외 (FixAllButtons에서 이미 설정됨)
            if (btn.name == "LOGIN" || btn.name.Contains("Login"))
            {
                Debug.Log($"[AutoAddButtonHover] Skipping LOGIN button: {btn.name}");
                continue;
            }

            // [FIX] Use DOTween RhythmButtonStyle instead of legacy SimpleButtonHover
            if (btn.GetComponent<RhythmButtonStyle>() == null)
            {
                // Remove legacy if exists
                var legacy = btn.GetComponent<SimpleButtonHover>();
                if (legacy != null) Destroy(legacy);

                btn.gameObject.AddComponent<RhythmButtonStyle>();
                Debug.Log($"[AutoAddButtonHover] Applied DOTween Style to {btn.name}");
            }
        }
    }
}
