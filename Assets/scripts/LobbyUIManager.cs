using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyUIManager : MonoBehaviour
{
    [Header("Fonts")]
    public TMP_FontAsset uiFont;

    [Header("Colors")]
    public Color themeDark = new Color(0.12f, 0.12f, 0.15f);
    public Color themeCyan = new Color(0.2f, 0.55f, 0.8f, 1f);
    public Color themeGold = new Color(1f, 0.8f, 0.2f);

    public void Setup()
    {
        if (uiFont == null) uiFont = Resources.Load<TMP_FontAsset>("Fonts/NotoSansKR-Regular SDF");
        if (uiFont == null) uiFont = Resources.Load<TMP_FontAsset>("NotoSansKR-Regular SDF");
    }

    // --- 1. 개선된 로그인 정보 패널 (배경 추가 + 1학년 컨셉) ---
    public GameObject CreateUserInfoPanel(Transform parent)
    {
        GameObject panelObj = new GameObject("UserInfoPanel");
        panelObj.transform.SetParent(parent, false);
        
        RectTransform rt = panelObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(280, -20);
        rt.sizeDelta = new Vector2(450, 70);

        // 배경 추가 (반투명 블랙)
        Image bg = panelObj.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.7f); // 어두운 배경

        HorizontalLayoutGroup hlg = panelObj.AddComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(20, 20, 10, 10);
        hlg.spacing = 20;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlWidth = true; hlg.childControlHeight = true;

        // "1학년" 텍스트 (기존 Lv.1 대체)
        CreateSimpleText(panelObj, "UserLevelText", "1학년", 24, themeCyan);
        CreateSimpleText(panelObj, "UserNameText", "Guest", 24, Color.white);
        CreateSimpleText(panelObj, "UserCoinText", "0 G", 24, themeGold);

        return panelObj;
    }

    // --- 2. 곡 선택 카드 (BPM, 작곡가 정보 포함) ---
    public void CreateSongCard(Transform parent, string titleStr, bool isUnlocked, int index)
    {
        GameObject card = new GameObject($"Card_{index}");
        card.transform.SetParent(parent, false);
        
        // 배경
        var img = card.AddComponent<Image>();
        img.color = isUnlocked ? new Color(0.95f, 0.95f, 0.95f) : new Color(0.3f, 0.3f, 0.3f);
        
        var btn = card.AddComponent<Button>();
        btn.interactable = isUnlocked;
        
        var le = card.AddComponent<LayoutElement>();
        le.preferredWidth = 350; le.preferredHeight = 480;
        
        // 아트웍
        GameObject artObj = new GameObject("Art");
        artObj.transform.SetParent(card.transform, false);
        var artImg = artObj.AddComponent<Image>();
        artImg.color = isUnlocked ? new Color(0.1f, 0.15f, 0.3f) : Color.black; 
        
        RectTransform artRt = artObj.AddComponent<RectTransform>();
        artRt.anchorMin = new Vector2(0, 0.35f); artRt.anchorMax = new Vector2(1, 1);
        artRt.offsetMin = new Vector2(10, 0); artRt.offsetMax = new Vector2(-10, -10);

        // 제목
        GameObject tObj = new GameObject("Title");
        tObj.transform.SetParent(card.transform, false);
        var tTxt = tObj.AddComponent<TextMeshProUGUI>();
        if(uiFont) tTxt.font = uiFont;
        tTxt.text = titleStr;
        tTxt.fontSize = 26; tTxt.alignment = TextAlignmentOptions.Center;
        tTxt.fontStyle = FontStyles.Bold;
        tTxt.color = isUnlocked ? Color.black : Color.gray;
        
        RectTransform tRt = tObj.AddComponent<RectTransform>();
        tRt.anchorMin = new Vector2(0, 0.2f); tRt.anchorMax = new Vector2(1, 0.35f);
        tRt.offsetMin = Vector2.zero; tRt.offsetMax = Vector2.zero;

        // [정보 추가] BPM | 작곡가
        string infoStr = "120 BPM | Unknown";
        if (titleStr.Contains("GALAXIAS")) infoStr = "155 BPM | Galantis";
        else if (titleStr.Contains("SODAPOP")) infoStr = "128 BPM | Soda P";
        
        GameObject iObj = new GameObject("Info");
        iObj.transform.SetParent(card.transform, false);
        var iTxt = iObj.AddComponent<TextMeshProUGUI>();
        if(uiFont) iTxt.font = uiFont;
        iTxt.text = infoStr;
        iTxt.fontSize = 18; iTxt.alignment = TextAlignmentOptions.Center;
        iTxt.color = new Color(0.4f, 0.4f, 0.4f);
        
        RectTransform iRt = iObj.AddComponent<RectTransform>();
        iRt.anchorMin = new Vector2(0, 0.1f); iRt.anchorMax = new Vector2(1, 0.2f);
        iRt.offsetMin = Vector2.zero; iRt.offsetMax = Vector2.zero;

        // 잠금 아이콘
        if (!isUnlocked)
        {
            GameObject lObj = new GameObject("Lock");
            lObj.transform.SetParent(card.transform, false);
            var lTxt = lObj.AddComponent<TextMeshProUGUI>();
            lTxt.text = "🔒"; lTxt.fontSize = 60; lTxt.alignment = TextAlignmentOptions.Center;
            RectTransform lRt = lObj.GetComponent<RectTransform>();
            lRt.anchorMin = Vector2.zero; lRt.anchorMax = Vector2.one;
            lRt.sizeDelta = Vector2.zero;
        }

        // 선택 이벤트
        btn.onClick.AddListener(() => {
            if(isUnlocked) Debug.Log($"Selected {titleStr}");
        });
    }

    private void CreateSimpleText(GameObject parent, string name, string content, float fontSize, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent.transform, false);
        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.color = color;
        if(uiFont) tmp.font = uiFont;
        tmp.alignment = TextAlignmentOptions.Left;
        
        // Full stretch for center alignment potential
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(800, 100);
    }

    public GameObject CreateRankingPanel(Transform parent)
    {
        GameObject panelObj = new GameObject("RankingPanel");
        panelObj.transform.SetParent(parent, false);

        RectTransform rt = panelObj.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

        Image bg = panelObj.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.9f);

        var title = CreateSimpleText_ReturnObj(panelObj, "Title", "RANKING", 48, themeCyan);
        title.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 300);
        title.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        var msg = CreateSimpleText_ReturnObj(panelObj, "Message", "랭킹 시스템 준비 중...", 32, Color.gray);
        msg.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        msg.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // Close Button
        GameObject closeBtn = new GameObject("CloseBtn");
        closeBtn.transform.SetParent(panelObj.transform, false);
        RectTransform closeRt = closeBtn.AddComponent<RectTransform>();
        closeRt.anchorMin = new Vector2(0.92f, 0.92f); closeRt.anchorMax = new Vector2(0.98f, 0.98f);
        closeRt.offsetMin = Vector2.zero; closeRt.offsetMax = Vector2.zero;
        
        Image closeImg = closeBtn.AddComponent<Image>();
        closeImg.color = new Color(0.8f, 0.2f, 0.2f);
        Button btn = closeBtn.AddComponent<Button>();
        btn.onClick.AddListener(() => panelObj.SetActive(false));
         
        CreateSimpleText_ReturnObj(closeBtn, "X", "X", 30, Color.white).GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        panelObj.SetActive(false);
        return panelObj;
    }

    private GameObject CreateSimpleText_ReturnObj(GameObject parent, string name, string content, float fontSize, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent.transform, false);
        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.color = color;
        if(uiFont) tmp.font = uiFont;
        tmp.alignment = TextAlignmentOptions.Left;
        
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(800, 100);
        return obj;
    }
}
