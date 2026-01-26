using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RebuildLobbyButtons : MonoBehaviour
{
    [ContextMenu("Rebuild Buttons")]
    public void Build()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found!");
            return;
        }

        SchoolLobbyManager manager = FindFirstObjectByType<SchoolLobbyManager>();
        if (manager == null)
        {
            Debug.LogError("No SchoolLobbyManager found!");
            return;
        }

        // Create Container
        GameObject container = new GameObject("LobbyButtonsContainer");
        container.transform.SetParent(canvas.transform, false);
        
        // Right side layout
        RectTransform rt = container.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 0.5f);
        rt.anchorMax = new Vector2(1, 0.5f);
        rt.pivot = new Vector2(1, 0.5f);
        rt.anchoredPosition = new Vector2(-80, 0); // Padding from right
        rt.sizeDelta = new Vector2(320, 600);

        VerticalLayoutGroup vlg = container.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 50; // 간격 더 증가
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlHeight = true;
        vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = false;
        vlg.padding = new RectOffset(0, 0, 20, 20); // 위아래 패딩 증가

        // Create Buttons (PROFILE 제외)
        CreateButton(container.transform, "Btn_GameStart", "GAME START", () => manager.OnGameStartClick());
        CreateButton(container.transform, "Btn_Ranking", "RANKING", () => manager.OnRankingClick());
        CreateButton(container.transform, "Btn_Shop", "SHOP", () => manager.OnShopClick());
        CreateButton(container.transform, "Btn_Option", "OPTION", () => manager.OnOptionsClick());
        CreateButton(container.transform, "Btn_Exit", "EXIT", () => Application.Quit());

        Debug.Log("Lobby Buttons Rebuilt!");
    }

    private void CreateButton(Transform parent, string name, string label, UnityEngine.Events.UnityAction action)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.5f);

        Button btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(action);

        // LayoutElement 추가로 버튼 크기 일관성 유지
        LayoutElement le = btnObj.AddComponent<LayoutElement>();
        bool isGameStart = name.Contains("GameStart");
        bool isExit = name.Contains("Exit");

        if (isGameStart)
        {
            le.preferredWidth = 280;
            le.preferredHeight = 80;
        }
        else if (isExit)
        {
            le.preferredWidth = 280;
            le.preferredHeight = 60;
        }
        else
        {
            le.preferredWidth = 280;
            le.preferredHeight = 70;
        }

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);

        TextMeshProUGUI txt = textObj.AddComponent<TextMeshProUGUI>();
        txt.text = label;
        txt.fontSize = isGameStart ? 36 : (isExit ? 28 : 32);
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = Color.white;
        txt.fontStyle = FontStyles.Bold | FontStyles.Italic;

        RectTransform btnRt = btnObj.GetComponent<RectTransform>();
        btnRt.sizeDelta = new Vector2(280, isGameStart ? 80 : (isExit ? 60 : 70));

        RectTransform txtRt = textObj.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = Vector2.zero;
        txtRt.offsetMax = Vector2.zero;
    }
}
