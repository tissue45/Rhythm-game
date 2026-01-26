using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 모든 버튼에 나노바나나 스타일을 즉시 적용하는 유틸리티
/// </summary>
public class ApplyNeonButtonStyle : MonoBehaviour
{
    [ContextMenu("Apply Neon Style to All Buttons NOW")]
    public void ApplyStyleToAllButtons()
    {
        // 모든 버튼 찾기
        Button[] allButtons = FindObjectsOfType<Button>(true);
        
        int styleApplied = 0;
        
        foreach (Button btn in allButtons)
        {
            string btnName = btn.gameObject.name.ToUpper();
            
            // RhythmButtonStyle 컴포넌트 확인
            RhythmButtonStyle style = btn.GetComponent<RhythmButtonStyle>();
            if (style == null)
            {
                style = btn.gameObject.AddComponent<RhythmButtonStyle>();
            }
            
            // 버튼 타입 결정
            if (btnName.Contains("GAMESTART") || btnName.Contains("START"))
            {
                style.isPrimaryButton = true;
                style.isMainButton = true;
                Debug.Log($"✅ PRIMARY: {btn.gameObject.name}");
            }
            else if (btnName.Contains("EXIT") || btnName.Contains("QUIT"))
            {
                style.isExitButton = true;
                Debug.Log($"✅ EXIT: {btn.gameObject.name}");
            }
            else
            {
                // RANKING, SHOP, OPTION 등
                style.isPrimaryButton = false;
                style.isExitButton = false;
                Debug.Log($"✅ NORMAL: {btn.gameObject.name}");
            }
            
            // 스타일 즉시 적용
            style.InitializeStyle();
            
            // 배경 이미지 색상 강제 설정
            Image bgImage = btn.GetComponent<Image>();
            if (bgImage != null)
            {
                if (style.isPrimaryButton)
                {
                    bgImage.color = new Color(1f, 0.2f, 0.8f); // 핑크
                }
                else if (style.isExitButton)
                {
                    bgImage.color = new Color(0.2f, 0.05f, 0.05f, 0.9f); // 다크 레드
                }
                else
                {
                    bgImage.color = new Color(0.05f, 0.15f, 0.2f, 0.9f); // 다크 시안
                }
            }
            
            styleApplied++;
        }
        
        Debug.Log($"🎨 <color=cyan>Successfully applied NEON style to {styleApplied} buttons!</color>");
        
        #if UNITY_EDITOR
        EditorUtility.SetDirty(gameObject);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        #endif
    }
}
