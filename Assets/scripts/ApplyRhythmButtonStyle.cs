using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 로비의 모든 버튼을 나노바나나 스타일로 자동 변환
/// </summary>
public class ApplyRhythmButtonStyle : MonoBehaviour
{
    [ContextMenu("Apply Rhythm Style to All Buttons")]
    public void ApplyStyleToAllButtons()
    {
        // Canvas 찾기
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas not found!");
            return;
        }

        // 모든 버튼 찾기
        Button[] buttons = canvas.GetComponentsInChildren<Button>(true);
        
        int converted = 0;
        foreach (Button btn in buttons)
        {
            string buttonName = btn.gameObject.name.ToUpper();
            
            // 버튼 타입 결정
            RhythmMenuButton.ButtonType type = RhythmMenuButton.ButtonType.Normal;
            
            if (buttonName.Contains("START") || buttonName.Contains("GAME"))
            {
                type = RhythmMenuButton.ButtonType.Primary;
            }
            else if (buttonName.Contains("EXIT") || buttonName.Contains("QUIT"))
            {
                type = RhythmMenuButton.ButtonType.Exit;
            }
            
            // 기존 RhythmMenuButton 제거
            RhythmMenuButton oldStyle = btn.GetComponent<RhythmMenuButton>();
            if (oldStyle != null)
            {
                DestroyImmediate(oldStyle);
            }
            
            // 새 스타일 적용
            RhythmMenuButton newStyle = btn.gameObject.AddComponent<RhythmMenuButton>();
            newStyle.buttonType = type;
            
            // 버튼 크기 조정
            RectTransform rt = btn.GetComponent<RectTransform>();
            if (rt != null)
            {
                // 최소 크기 보장
                if (rt.sizeDelta.x < 200) rt.sizeDelta = new Vector2(300, rt.sizeDelta.y);
                if (rt.sizeDelta.y < 60) rt.sizeDelta = new Vector2(rt.sizeDelta.x, 80);
            }
            
            converted++;
            Debug.Log($"Applied Rhythm Style to: {btn.gameObject.name} ({type})");
        }
        
        Debug.Log($"✅ Successfully converted {converted} buttons to Rhythm Style!");
        
        #if UNITY_EDITOR
        EditorUtility.SetDirty(canvas.gameObject);
        #endif
    }
}
