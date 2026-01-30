using UnityEngine;
using UnityEditor;
using TMPro;

public class SetDefaultFont : EditorWindow
{
    [MenuItem("Tools/Fix Rhythm Game/Set Default Font to Korean")]
    public static void SetDefaultFontToKorean()
    {
        // 1. 한글 폰트 찾기
        TMP_FontAsset koreanFont = Resources.Load<TMP_FontAsset>("Fonts/NotoSansKR-Regular SDF");
        if (koreanFont == null) koreanFont = Resources.Load<TMP_FontAsset>("NotoSansKR-Regular SDF");

        if (koreanFont == null)
        {
            // 프로젝트 전체 검색
            string[] guids = AssetDatabase.FindAssets("NotoSansKR-Regular SDF t:TMP_FontAsset");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                koreanFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            }
        }

        if (koreanFont == null)
        {
            EditorUtility.DisplayDialog("Error", "한글 폰트(NotoSansKR-Regular SDF)를 찾을 수 없습니다.\n폰트를 먼저 생성해주세요.", "OK");
            return;
        }

        // 2. TMP Settings 찾기 및 수정
        TMP_Settings settings = Resources.Load<TMP_Settings>("TMP Settings");
        if (settings == null)
        {
             // GUID로 검색
            string[] guids = AssetDatabase.FindAssets("TMP Settings t:TMP_Settings");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                settings = AssetDatabase.LoadAssetAtPath<TMP_Settings>(path);
            }
        }

        if (settings != null)
        {
            // 리플렉션이나 SerializedObject를 사용하여 내부 필드 수정 (접근 권한 문제 우회)
            SerializedObject so = new SerializedObject(settings);
            SerializedProperty prop = so.FindProperty("m_defaultFontAsset");
            if (prop != null)
            {
                prop.objectReferenceValue = koreanFont;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
                EditorUtility.DisplayDialog("Success", "TextMeshPro의 기본 폰트를 'NotoSansKR-Regular SDF'로 변경했습니다!\n이제 모든 새 텍스트는 이 폰트를 사용합니다.", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "TMP Settings에서 defaultFontAsset 속성을 찾을 수 없습니다.", "OK");
            }
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "TMP Settings 파일을 찾을 수 없습니다.", "OK");
        }
    }
}
