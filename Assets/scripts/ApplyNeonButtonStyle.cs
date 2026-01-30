using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 모든 버튼에 나노바나나 스타일을 즉시 적용하는 유틸리티
/// Unity 에디터에서 GameObject에 추가 후 우클릭 -> "Apply Neon Style NOW" 실행
/// </summary>
public class ApplyNeonButtonStyle : MonoBehaviour
{
    [ContextMenu("🎨 Apply Neon Style NOW")]
    public void ApplyStyleToAllButtons()
    {
        Debug.Log("🎨 Starting to apply neon style to all buttons...");
        
        // 모든 버튼 찾기 (비활성화된 것도 포함)
        Button[] allButtons = Resources.FindObjectsOfTypeAll<Button>();
        
        int styleApplied = 0;
        
        foreach (Button btn in allButtons)
        {
            // 프리팹이나 씬에 없는 것 제외
            if (btn.gameObject.scene.name == null) continue;
            
            string btnName = btn.gameObject.name.ToUpper();
            
            Debug.Log($"Found button: {btn.gameObject.name}");
            
            // RhythmButtonStyle 컴포넌트 확인
            RhythmButtonStyle style = btn.GetComponent<RhythmButtonStyle>();
            if (style == null)
            {
                style = btn.gameObject.AddComponent<RhythmButtonStyle>();
                Debug.Log($"  Added RhythmButtonStyle to {btn.gameObject.name}");
            }
            
            // 버튼 타입 결정
            if (btnName.Contains("GAMESTART") || btnName.Contains("START"))
            {
                style.isPrimaryButton = true;
                style.isMainButton = true;
                Debug.Log($"  ✅ Set as PRIMARY: {btn.gameObject.name}");
            }
            else if (btnName.Contains("EXIT") || btnName.Contains("QUIT"))
            {
                style.isExitButton = true;
                Debug.Log($"  ✅ Set as EXIT: {btn.gameObject.name}");
            }
            else
            {
                // RANKING, SHOP, OPTION 등
                style.isPrimaryButton = false;
                style.isExitButton = false;
                Debug.Log($"  ✅ Set as NORMAL: {btn.gameObject.name}");
            }
            
            // 스타일 즉시 적용
            style.InitializeStyle();
            
            // 배경 이미지 색상 강제 설정
            Image bgImage = btn.GetComponent<Image>();
            if (bgImage != null)
            {
                Color newColor;
                if (style.isPrimaryButton)
                {
                    newColor = new Color(1f, 0.2f, 0.8f, 1f); // 핑크
                    Debug.Log($"  🎨 Applied PINK color");
                }
                else if (style.isExitButton)
                {
                    newColor = new Color(0.2f, 0.05f, 0.05f, 0.9f); // 다크 레드
                    Debug.Log($"  🎨 Applied DARK RED color");
                }
                else
                {
                    newColor = new Color(0.05f, 0.15f, 0.2f, 0.9f); // 다크 시안
                    Debug.Log($"  🎨 Applied DARK CYAN color");
                }
                bgImage.color = newColor;
            }
            else
            {
                Debug.LogWarning($"  ⚠️ No Image component on {btn.gameObject.name}");
            }
            
            // 텍스트 스타일 강제 적용
            TextMeshProUGUI btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.fontSize = style.isPrimaryButton ? 48 : 40;
                btnText.fontStyle = FontStyles.Bold | FontStyles.Italic;
                btnText.color = Color.white;
                Debug.Log($"  📝 Applied text style");
            }
            
            styleApplied++;
        }
        
        Debug.Log($"🎨 <color=cyan>✅ Successfully applied NEON style to {styleApplied} buttons!</color>");
        
        #if UNITY_EDITOR
        // 씬 저장 표시
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        #endif
    }
}
