using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Main 씬의 버튼 간격과 스타일을 수정하는 에디터 스크립트
/// </summary>
public class FixButtonLayout : EditorWindow
{
    [MenuItem("Rhythm Game/Fix Button Layout")]
    public static void FixLayout()
    {
        Debug.Log("[FixButtonLayout] Starting button layout fix...");

        // 1. 버튼 컨테이너 찾기
        GameObject container = GameObject.Find("LobbyButtonsContainer");
        if (container == null)
        {
            Debug.LogWarning("LobbyButtonsContainer not found. Please rebuild buttons first.");
            return;
        }

        // 2. VerticalLayoutGroup 설정 수정
        VerticalLayoutGroup vlg = container.GetComponent<VerticalLayoutGroup>();
        if (vlg != null)
        {
            vlg.spacing = 50; // 간격 더 증가
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = false;
            vlg.padding = new RectOffset(0, 0, 20, 20); // 위아래 패딩 증가
            Debug.Log("[FixButtonLayout] Updated VerticalLayoutGroup spacing to 50");
        }

        // 3. 컨테이너 위치 및 크기 조정
        RectTransform containerRt = container.GetComponent<RectTransform>();
        if (containerRt != null)
        {
            containerRt.anchorMin = new Vector2(1, 0.5f);
            containerRt.anchorMax = new Vector2(1, 0.5f);
            containerRt.pivot = new Vector2(1, 0.5f);
            containerRt.anchoredPosition = new Vector2(-80, 0);
            containerRt.sizeDelta = new Vector2(320, 600);
            Debug.Log("[FixButtonLayout] Updated container position and size");
        }

        // 3.5. PROFILE 버튼 숨기기
        GameObject profileBtn = GameObject.Find("Btn_Profile");
        if (profileBtn != null)
        {
            profileBtn.SetActive(false);
            Debug.Log("[FixButtonLayout] Hidden Btn_Profile");
        }

        // 4. 각 버튼 스타일 수정
        FixButtonStyle("Btn_GameStart", true, false);
        FixButtonStyle("Btn_Ranking", false, false);
        FixButtonStyle("Btn_Shop", false, false);
        FixButtonStyle("Btn_Option", false, false);
        FixButtonStyle("Btn_Exit", false, true);

        Debug.Log("[FixButtonLayout] Button layout fix complete!");
    }

    static void FixButtonStyle(string btnName, bool isMain, bool isExit)
    {
        GameObject obj = GameObject.Find(btnName);
        if (obj == null)
        {
            Debug.LogWarning($"Button {btnName} not found. Skipping.");
            return;
        }

        Debug.Log($"[FixButtonLayout] Fixing {btnName}...");

        // RhythmButtonStyle 추가 또는 가져오기
        RhythmButtonStyle style = obj.GetComponent<RhythmButtonStyle>();
        if (style == null)
        {
            style = obj.AddComponent<RhythmButtonStyle>();
        }

        // 스타일 설정
        style.isMainButton = isMain;
        style.isTextOnly = isExit;
        style.skewAngle = 0f;

        // LayoutElement 설정
        LayoutElement le = obj.GetComponent<LayoutElement>();
        if (le == null)
        {
            le = obj.AddComponent<LayoutElement>();
        }

        // RectTransform 설정
        RectTransform rt = obj.GetComponent<RectTransform>();

        if (isMain)
        {
            // GAME START 버튼
            style.normalBgColor = Color.white;
            style.paddingLeft = 50f;
            style.textAlignment = TextAlignmentOptions.Center;

            rt.sizeDelta = new Vector2(280, 80);
            le.preferredWidth = 280;
            le.preferredHeight = 80;
            le.minWidth = 280;
            le.minHeight = 80;

            // 텍스트 크기 조정
            TextMeshProUGUI txt = obj.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.fontSize = 36;
        }
        else if (isExit)
        {
            // EXIT 버튼 - 빨간색 스타일
            style.normalBgColor = new Color(0.6f, 0.1f, 0.1f, 0.8f); // 어두운 빨강
            style.hoverBgColor = new Color(0.8f, 0.2f, 0.2f, 0.9f); // 밝은 빨강
            style.accentColor = new Color(1f, 0.3f, 0.3f, 1f); // 빨간색 강조
            style.paddingLeft = 50f;
            style.textAlignment = TextAlignmentOptions.Center;
            style.textOffset = Vector2.zero;

            rt.sizeDelta = new Vector2(280, 65);
            le.preferredWidth = 280;
            le.preferredHeight = 65;
            le.minWidth = 280;
            le.minHeight = 65;

            // 텍스트 설정
            TextMeshProUGUI txt = obj.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
            {
                if (!txt.text.Contains("⏻")) txt.text = "⏻ EXIT";
                txt.fontSize = 30;
                txt.alignment = TextAlignmentOptions.Center;
            }
        }
        else
        {
            // RANKING, SHOP, OPTION 버튼
            style.normalBgColor = new Color(0f, 0.05f, 0.1f, 0.8f);
            style.paddingLeft = 50f;
            style.textAlignment = TextAlignmentOptions.Center;

            rt.sizeDelta = new Vector2(280, 70);
            le.preferredWidth = 280;
            le.preferredHeight = 70;
            le.minWidth = 280;
            le.minHeight = 70;

            // 텍스트 크기 조정
            TextMeshProUGUI txt = obj.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.fontSize = 32;
        }

        // 공통 색상 설정
        style.hoverBgColor = new Color(0.1f, 0.15f, 0.25f, 0.9f);
        style.accentColor = new Color(0f, 1f, 1f, 1f);

        // 스타일 적용
        style.InitializeStyle();
        style.UpdateVisuals();

        EditorUtility.SetDirty(style);
        EditorUtility.SetDirty(obj);

        Debug.Log($"[FixButtonLayout] Fixed {btnName} successfully");
    }
}
