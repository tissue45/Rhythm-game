using UnityEngine;
using UnityEditor;
using TMPro;

public class FixFonts : EditorWindow
{
    [MenuItem("Tools/Fix Rhythm Game/Fix All Fonts (Force NotoSans)")]
    public static void FixAllFontsInScene()
    {
        // 1. 폰트 에셋 찾기
        // 저장하신 파일 이름이 "NotoSansKR-Regular SDF"라고 가정합니다.
        TMP_FontAsset koreanFont = Resources.Load<TMP_FontAsset>("Fonts/NotoSansKR-Regular SDF");
        
        // 만약 Resources 폴더에 없다면, 프로젝트 전체에서 검색
        if (koreanFont == null)
        {
            string[] guids = AssetDatabase.FindAssets("NotoSansKR-Regular SDF t:TMP_FontAsset");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                koreanFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            }
        }

        if (koreanFont == null)
        {
            EditorUtility.DisplayDialog("Error", "'NotoSansKR-Regular SDF' 폰트 파일을 찾을 수 없습니다.\n폰트가 만들어졌는지, 이름이 정확한지 확인해주세요.", "OK");
            return;
        }

        // 2. 씬에 있는 모든 텍스트 오브젝트 찾기
        TextMeshProUGUI[] allTexts = FindObjectsOfType<TextMeshProUGUI>(true); // true = 비활성화된 것도 포함
        int count = 0;

        foreach (TextMeshProUGUI text in allTexts)
        {
            // Undo 등록 (Ctrl+Z 되돌리기 가능하도록)
            Undo.RecordObject(text, "Change Font");

            // 폰트 교체
            text.font = koreanFont;
            
            // 머티리얼 프리셋 초기화 (보라색 네모 방지)
            text.fontSharedMaterial = koreanFont.material;

            count++;
        }

        // 3. 프리팹 내부 수정 사항 저장 (선택 사항)
        // UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Success", $"총 {count}개의 텍스트 폰트를 'NotoSansKR-Regular SDF'로 교체했습니다!", "OK");
    }
}
