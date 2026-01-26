using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Unity 메뉴에서 바로 실행할 수 있는 버튼 스타일 적용 도구
/// Tools > Apply Paper Button Style 클릭
/// </summary>
public class NeonButtonStyleMenu
{
    [MenuItem("Tools/📄 Apply Paper Button Style to All Buttons")]
    public static void ApplyNeonStyleToAllButtons()
    {
        Debug.Log("📄 Starting to apply paper style to all buttons...");
        
        // 현재 씬의 모든 버튼 찾기
        Button[] allButtons = Object.FindObjectsOfType<Button>(true);
        
        int styleApplied = 0;
        
        foreach (Button btn in allButtons)
        {
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
            
            // [FIX] 기존의 모든 시각적 컴포넌트 제거
            // 기존 Shadow 제거
            Shadow[] oldShadows = btn.GetComponents<Shadow>();
            foreach (Shadow s in oldShadows)
            {
                if (s != null) Object.DestroyImmediate(s);
            }
            
            // 기존 Outline 제거
            Outline[] oldOutlines = btn.GetComponentsInChildren<Outline>();
            foreach (Outline o in oldOutlines)
            {
                if (o != null) Object.DestroyImmediate(o);
            }
            
            // 배경 이미지 색상 강제 설정
            Image bgImage = btn.GetComponent<Image>();
            if (bgImage != null)
            {
                Color newColor;
                if (style.isPrimaryButton)
                {
                    newColor = new Color(0.9f, 0.95f, 1f, 1f); // 밝은 파스텔 블루
                    Debug.Log($"  📄 Applied PASTEL BLUE color (paper style)");
                }
                else if (style.isExitButton)
                {
                    newColor = new Color(0.95f, 0.95f, 0.95f, 1f); // 연한 회색
                    Debug.Log($"  📄 Applied LIGHT GRAY color (paper style)");
                }
                else
                {
                    newColor = new Color(1f, 1f, 1f, 1f); // 순백색
                    Debug.Log($"  📄 Applied WHITE color (paper style)");
                }
                bgImage.color = newColor;
                bgImage.sprite = null; // 기존 스프라이트 제거
                bgImage.type = Image.Type.Simple;
                
                // normalBgColor도 설정
                style.normalBgColor = newColor;
                if (style.isPrimaryButton)
                {
                    style.hoverBgColor = new Color(0.85f, 0.92f, 1f, 1f);
                }
                else if (style.isExitButton)
                {
                    style.hoverBgColor = new Color(0.9f, 0.9f, 0.9f, 1f);
                }
                else
                {
                    style.hoverBgColor = new Color(0.95f, 0.95f, 0.98f, 1f);
                }
                
                // 종이 그림자 추가
                Shadow paperShadow = btn.gameObject.AddComponent<Shadow>();
                paperShadow.effectColor = new Color(0, 0, 0, 0.15f);
                paperShadow.effectDistance = new Vector2(4, -4);
                Debug.Log($"  ☁️ Added soft shadow");
            }
            else
            {
                Debug.LogWarning($"  ⚠️ No Image component on {btn.gameObject.name}");
            }
            
            // 텍스트 스타일 강제 적용
            TextMeshProUGUI btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.fontSize = style.isPrimaryButton ? 44 : 36;
                btnText.fontStyle = FontStyles.Bold;
                
                // 종이 스타일에 맞는 텍스트 색상
                if (style.isPrimaryButton)
                {
                    btnText.color = new Color(0.2f, 0.3f, 0.5f, 1f); // 진한 블루
                }
                else if (style.isExitButton)
                {
                    btnText.color = new Color(0.3f, 0.3f, 0.3f, 1f); // 회색
                }
                else
                {
                    btnText.color = new Color(0.1f, 0.1f, 0.1f, 1f); // 거의 검은색
                }
                
                btnText.alignment = TMPro.TextAlignmentOptions.Center;
                
                // 외곽선 제거 (종이는 깔끔하게)
                Outline outline = btnText.GetComponent<Outline>();
                if (outline != null)
                {
                    Object.DestroyImmediate(outline);
                }
                
                Debug.Log($"  📝 Applied paper text style");
            }
            
            // 변경사항 저장
            EditorUtility.SetDirty(btn.gameObject);
            
            styleApplied++;
        }
        
        Debug.Log($"📄 <color=blue>✅ Successfully applied PAPER style to {styleApplied} buttons!</color>");
        
        // 씬 저장 표시
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );
        
        // 성공 메시지
        EditorUtility.DisplayDialog(
            "Paper Style Applied!", 
            $"Successfully applied clean paper style to {styleApplied} buttons!\n\n✓ White/pastel backgrounds\n✓ Soft shadows\n✓ Clean typography\n\nCheck the Console for details.", 
            "OK"
        );
    }
}
#endif
