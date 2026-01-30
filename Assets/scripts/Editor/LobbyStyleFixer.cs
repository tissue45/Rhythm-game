using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class LobbyStyleFixer : EditorWindow
{
    [MenuItem("Rhythm Game/Fix Lobby Buttons")]
    public static void FixLobbyButtons()
    {
        Debug.Log("Starting Lobby Button Fix...");

        // 1. Fix Game Start Button (Main)
        FixButton("Btn_GameStart", true);
        FixButton("Btn_Start", true);

        // 2. Fix Sub Buttons
        FixButton("Btn_Ranking", false);
        FixButton("Btn_Shop", false);
        FixButton("Btn_Profile", false);
        FixButton("Btn_Option", false);
        FixButton("Btn_Exit", false);
        FixButton("ExitButton", false);

        // 3. Fix LOGIN Button (Special Style)
        FixLoginButton();
        
        // 3. Fix Layout Container Alignment (Right Side)
        GameObject rankingBtn = GameObject.Find("Btn_Ranking");
        if (rankingBtn != null && rankingBtn.transform.parent != null)
        {
            var vlg = rankingBtn.transform.parent.GetComponent<VerticalLayoutGroup>();
            if (vlg != null)
            {
                vlg.spacing = 50; // 간격 더 증가
                vlg.childAlignment = TextAnchor.MiddleCenter; // 중앙 정렬
                vlg.childControlHeight = true;
                vlg.childControlWidth = true;
                vlg.childForceExpandHeight = false;
                vlg.childForceExpandWidth = false;
                vlg.padding = new RectOffset(0, 0, 20, 20); // 위아래 패딩 증가
                Debug.Log("Updated Menu Layout: Spacing=50, Alignment=MiddleCenter");
            }

            // Shift Parent Right if needed (by Anchors)
            RectTransform parentRt = rankingBtn.transform.parent.GetComponent<RectTransform>();
            if (parentRt != null)
            {
                // Anchor Middle Right
                parentRt.anchorMin = new Vector2(1, 0.5f);
                parentRt.anchorMax = new Vector2(1, 0.5f);
                parentRt.pivot = new Vector2(1, 0.5f);
                parentRt.anchoredPosition = new Vector2(-80, 0); // 80px padding from edge
                parentRt.sizeDelta = new Vector2(320, 600);
                Debug.Log("Anchored Menu to Right Edge with proper spacing");
            }
        }

        // 3.5. PROFILE 버튼 숨기기
        GameObject profileBtn = GameObject.Find("Btn_Profile");
        if (profileBtn != null)
        {
            profileBtn.SetActive(false);
            Debug.Log("Hidden Btn_Profile");
        }

        Debug.Log("Lobby Buttons Fixed!");
    }

    static void FixButton(string btnName, bool isMain)
    {
        GameObject obj = GameObject.Find(btnName);
        if (obj == null) return;

        Debug.Log($"Fixing {btnName} (Main: {isMain})");

        var style = obj.GetComponent<RhythmButtonStyle>();
        if (style == null) style = obj.AddComponent<RhythmButtonStyle>();

        // FORCE SETTINGS
        style.isMainButton = isMain;
        style.skewAngle = 0f; // Straight Design

        bool isExit = btnName.Contains("Exit") || btnName.Contains("Quit");
        style.isTextOnly = isExit;

        // 일관된 레이아웃 설정
        LayoutElement le = obj.GetComponent<LayoutElement>();
        if (le == null) le = obj.AddComponent<LayoutElement>();

        RectTransform rt = obj.GetComponent<RectTransform>();

        if (isMain)
        {
            // Main Button: GAME START
            style.normalBgColor = Color.white;
            style.paddingLeft = 50f;
            style.textAlignment = TextAlignmentOptions.Center;

            rt.sizeDelta = new Vector2(280, 80);
            le.preferredWidth = 280;
            le.preferredHeight = 80;
            le.minWidth = 280;
            le.minHeight = 80;

            if (style.btnText != null) style.btnText.fontSize = 36;
        }
        else if (isExit)
        {
            // Exit Button: 빨간색 배경 스타일
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

            if (style.btnText != null)
            {
                string currentText = style.btnText.text;
                if (!currentText.Contains("⏻")) style.btnText.text = "⏻ EXIT";
                style.btnText.fontSize = 30;
                style.btnText.alignment = TextAlignmentOptions.Center;
            }
        }
        else
        {
            // Sub Buttons: RANKING, SHOP, OPTION
            style.normalBgColor = new Color(0f, 0.05f, 0.1f, 0.8f);
            style.paddingLeft = 50f;
            style.textAlignment = TextAlignmentOptions.Center;

            rt.sizeDelta = new Vector2(280, 70);
            le.preferredWidth = 280;
            le.preferredHeight = 70;
            le.minWidth = 280;
            le.minHeight = 70;

            if (style.btnText != null) style.btnText.fontSize = 32;
        }

        // Hover & Accent Colors
        style.hoverBgColor = new Color(0.1f, 0.15f, 0.25f, 0.9f);
        style.accentColor = new Color(0f, 1f, 1f, 1f);

        style.InitializeStyle();
        style.UpdateVisuals();

        EditorUtility.SetDirty(style);
        EditorUtility.SetDirty(obj);
    }

    static void FixLoginButton()
    {
        GameObject loginBtn = GameObject.Find("LOGIN");
        if (loginBtn == null)
        {
            // Canvas 찾아서 LOGIN 버튼 생성
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            loginBtn = new GameObject("LOGIN");
            loginBtn.transform.SetParent(canvas.transform, false);

            // Button 컴포넌트 추가
            var btn = loginBtn.AddComponent<Button>();
            var img = loginBtn.AddComponent<Image>();
            img.color = new Color(0.1f, 0.5f, 0.7f, 0.9f); // 파란색

            // Text 추가
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(loginBtn.transform, false);
            var txt = textObj.AddComponent<TextMeshProUGUI>();
            txt.text = "LOGIN";
            txt.fontSize = 24;
            txt.fontStyle = FontStyles.Bold;
            txt.alignment = TextAlignmentOptions.Center;
            txt.color = Color.white;

            RectTransform txtRt = textObj.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.sizeDelta = Vector2.zero;

            // 위치 설정 (왼쪽 상단)
            RectTransform rt = loginBtn.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(20, -20);
            rt.sizeDelta = new Vector2(120, 50);

            Debug.Log("Created LOGIN button");
        }
        else
        {
            // 기존 LOGIN 버튼 스타일 수정
            var img = loginBtn.GetComponent<Image>();
            if (img != null)
            {
                img.color = new Color(0.1f, 0.5f, 0.7f, 0.9f); // 파란색 배경
            }

            var txt = loginBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
            {
                txt.fontSize = 24;
                txt.fontStyle = FontStyles.Bold;
                txt.color = Color.white;
            }

            // 위치 및 크기 조정
            RectTransform rt = loginBtn.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0, 1);
                rt.anchoredPosition = new Vector2(20, -20);
                rt.sizeDelta = new Vector2(120, 50);
            }

            Debug.Log("Updated LOGIN button style");
        }

        EditorUtility.SetDirty(loginBtn);
    }
}
