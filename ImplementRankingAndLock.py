
import os
import re

file_path = r"Assets\scripts\SchoolLobbyManager.cs"

with open(file_path, 'r', encoding='utf-8', errors='ignore') as f:
    content = f.read()

# 1. Update TogglePanel (Add Ranking Logic)
# Find existing BindCloseButton call inside TogglePanel and insert PopulateRanking check after it
new_toggle_logic = """
                // 2. ?기 버튼(CloseButton) 기능 ?결
                BindCloseButton(panel);

                // [FIX] ?점(Shop) ?널 채우?
                if (panel == shopPanel)
                {
                    PopulateShop(panel);
                }
                
                // [FIX] ??택(Carousel) ?널 복구
                if (panel == songSelectPanel)
                {
                    PopulateSongSelect(panel);
                }

                // [FIX] Ranking Panel
                if (panel == rankingPanel)
                {
                    PopulateRanking(panel);
                }
"""

# We attempt to replace the specific block if found, otherwise we might need a broader replace
# Let's try to replace the whole 'if (isOpening)' block inside TogglePanel if we can match it well.
# Or simpler: Replace the previous Shop/SongSelect block with the new one including Ranking.

toggle_regex = r'// 2\. .*?BindCloseButton\(panel\);.*?if \(panel == shopPanel\).*?PopulateSongSelect\(panel\);\s+}'
# The formatting in file might be messy. Let's strictly replace the method TogglePanel completely for safety.

new_toggle_panel = """
    private void TogglePanel(GameObject panel)
    {
        if (panel != null)
        {
            // [FIX] Capture state BEFORE closing
            bool wasActive = panel.activeSelf;
            CloseAllPanels();

            bool isOpening = !wasActive;
            panel.SetActive(isOpening);

            if (isOpening) 
            {
                panel.transform.SetAsLastSibling();
                
                RectTransform rt = panel.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(0.15f, 0.15f);
                    rt.anchorMax = new Vector2(0.85f, 0.85f);
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;
                }

                BindCloseButton(panel);

                if (panel == shopPanel) PopulateShop(panel);
                if (panel == songSelectPanel) PopulateSongSelect(panel);
                if (panel == rankingPanel) PopulateRanking(panel);
            }
        }
        else
        {
            Debug.LogWarning("Panel not assigned in Inspector.");
        }
    }
"""

# 2. Update PopulateSongSelect (Add Lock Logic)
# We need to inject the lock logic inside cv.OnItemSelected
new_populate_song_select = """
    private void PopulateSongSelect(GameObject panel)
    {
        Debug.Log($"[Lobby] Layout: Polished DJMAX Style + Lock Logic");

        foreach (Transform child in panel.transform) Destroy(child.gameObject);

        // Background
        var bgImg = panel.GetComponent<UnityEngine.UI.Image>();
        if(bgImg == null) bgImg = panel.AddComponent<UnityEngine.UI.Image>();
        bgImg.color = new Color(0, 0, 0, 0.95f);

        // Left Panel
        GameObject leftPanel = new GameObject("LeftDetailPanel");
        leftPanel.transform.SetParent(panel.transform, false);
        RectTransform leftRt = leftPanel.AddComponent<RectTransform>();
        leftRt.anchorMin = new Vector2(0.05f, 0.1f); leftRt.anchorMax = new Vector2(0.45f, 0.9f); 
        leftRt.offsetMin = Vector2.zero; leftRt.offsetMax = Vector2.zero;
        
        // Album Art
        GameObject artObj = new GameObject("AlbumArt");
        artObj.transform.SetParent(leftPanel.transform, false);
        var artImg = artObj.AddComponent<UnityEngine.UI.Image>();
        artImg.color = new Color(0.2f, 0.2f, 0.2f);
        artImg.preserveAspect = true;
        RectTransform artRt = artObj.GetComponent<RectTransform>();
        artRt.anchorMin = new Vector2(0, 0.3f); artRt.anchorMax = new Vector2(1, 1);
        artRt.offsetMin = Vector2.zero; artRt.offsetMax = Vector2.zero;

        // Title
        GameObject titleObj = new GameObject("DetailTitle");
        titleObj.transform.SetParent(leftPanel.transform, false);
        var titleTxt = titleObj.AddComponent<TMPro.TextMeshProUGUI>();
        titleTxt.text = "SELECT TRACK";
        titleTxt.fontSize = 42;
        titleTxt.fontStyle = TMPro.FontStyles.Bold;
        titleTxt.alignment = TMPro.TextAlignmentOptions.Center;
        titleTxt.color = new Color(0, 0.9f, 1f);
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0, 0.2f); titleRt.anchorMax = new Vector2(1, 0.3f);
        titleRt.offsetMin = Vector2.zero; titleRt.offsetMax = Vector2.zero;

        // Start Button
        GameObject startBtnObj = new GameObject("Btn_StartGame");
        startBtnObj.transform.SetParent(leftPanel.transform, false);
        var startImg = startBtnObj.AddComponent<UnityEngine.UI.Image>();
        startImg.color = new Color(0, 0.7f, 0.9f);
        var startBtn = startBtnObj.AddComponent<UnityEngine.UI.Button>();
        RectTransform startRt = startBtnObj.GetComponent<RectTransform>();
        startRt.anchorMin = new Vector2(0.1f, 0); startRt.anchorMax = new Vector2(0.9f, 0.15f);
        startRt.offsetMin = Vector2.zero; startRt.offsetMax = Vector2.zero;
        
        GameObject stTxtObj = new GameObject("Text");
        stTxtObj.transform.SetParent(startBtnObj.transform, false);
        var stTxt = stTxtObj.AddComponent<TMPro.TextMeshProUGUI>();
        stTxt.text = "GAME START";
        stTxt.fontSize = 32;
        stTxt.alignment = TMPro.TextAlignmentOptions.Center;
        stTxt.color = Color.black; 
        var stRt = stTxtObj.GetComponent<RectTransform>();
        stRt.anchorMin = Vector2.zero; stRt.anchorMax = Vector2.one;
        stRt.offsetMin = Vector2.zero; stRt.offsetMax = Vector2.zero;

        startBtn.onClick.AddListener(()=> { StartGameLoop(); });

        // Right Panel
        GameObject rightPanel = new GameObject("CarouselContainer");
        rightPanel.transform.SetParent(panel.transform, false);
        RectTransform rightRt = rightPanel.AddComponent<RectTransform>();
        rightRt.anchorMin = new Vector2(0.5f, 0.1f); rightRt.anchorMax = new Vector2(0.95f, 0.9f);
        rightRt.offsetMin = Vector2.zero; rightRt.offsetMax = Vector2.zero;

        // Close Button
        GameObject closeBtnObj = new GameObject("Btn_Close");
        closeBtnObj.transform.SetParent(panel.transform, false);
        var closeImg = closeBtnObj.AddComponent<UnityEngine.UI.Image>();
        closeImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        var closeBtn = closeBtnObj.AddComponent<UnityEngine.UI.Button>();
        closeBtn.onClick.AddListener(() => { TogglePanel(panel); });
        RectTransform closeRt = closeBtnObj.GetComponent<RectTransform>();
        closeRt.anchorMin = new Vector2(0.92f, 0.92f); closeRt.anchorMax = new Vector2(0.98f, 0.98f);
        closeRt.offsetMin = Vector2.zero; closeRt.offsetMax = Vector2.zero;
        GameObject closeTxtObj = new GameObject("Text");
        closeTxtObj.transform.SetParent(closeBtnObj.transform, false);
        var clTxt = closeTxtObj.AddComponent<TMPro.TextMeshProUGUI>();
        clTxt.text = "X";
        clTxt.fontSize = 28;
        clTxt.alignment = TMPro.TextAlignmentOptions.Center;
        clTxt.color = Color.white;
        var clRt = closeTxtObj.GetComponent<RectTransform>();
        clRt.anchorMin = Vector2.zero; clRt.anchorMax = Vector2.one;
        clRt.offsetMin = Vector2.zero; clRt.offsetMax = Vector2.zero;

        // Scroll Logic
        var scroll = rightPanel.AddComponent<UnityEngine.UI.ScrollRect>();
        rightPanel.AddComponent<UnityEngine.UI.Image>().color = new Color(0,0,0,0);
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(rightPanel.transform, false);
        var contentRt = contentObj.AddComponent<RectTransform>();
        contentRt.sizeDelta = new Vector2(500, 1200);
        scroll.content = contentRt;
        scroll.horizontal = false; scroll.vertical = true;
        scroll.movementType = UnityEngine.UI.ScrollRect.MovementType.Elastic;

        var cv = rightPanel.AddComponent<CarouselView>();
        cv.container = contentRt;
        cv.spacing = 110f; 
        cv.scaleFactor = 1.0f;
        cv.isVertical = true;

        string[] songs = { "GALAXIAS!", "Sodapop", "Coming Soon" };
        for(int i=0; i<songs.Length; i++)
        {
            CreateSongCard(contentRt, songs[i], i <= 1, i);
        }

        cv.Invoke("InitializeItems", 0.1f);
        cv.OnItemSelected = (index) => {
            if (index >= 0 && index < songs.Length)
            {
                string s = songs[index];
                _selectedSongTitle = s;
                titleTxt.text = s.ToUpper();
                
                if (albumCovers != null && index < albumCovers.Length && albumCovers[index] != null)
                {
                    artImg.sprite = albumCovers[index];
                    artImg.color = Color.white;
                }
                else
                {
                    if(s == "GALAXIAS!") artImg.color = new Color(1, 0, 0.5f);
                    else if(s == "Sodapop") artImg.color = new Color(0, 0.5f, 1f);
                    else artImg.color = new Color(0.2f, 0.2f, 0.2f);
                }

                // [FIX] LOCK LOGIC
                bool isLocked = (s == "Coming Soon" || s.Contains("Locked"));
                startBtn.interactable = !isLocked;
                stTxt.text = isLocked ? "LOCKED" : "GAME START";
                startImg.color = isLocked ? Color.gray : new Color(0, 0.7f, 0.9f);
                stTxt.color = isLocked ? new Color(0.3f, 0.3f, 0.3f) : Color.black;
            }
        };
    }
"""

# 3. Add PopulateRanking
new_populate_ranking = """
    private void PopulateRanking(GameObject panel)
    {
        Debug.Log("[Lobby] Populating Ranking...");

        foreach (Transform child in panel.transform) Destroy(child.gameObject);

        // Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panel.transform, false);
        var titleTxt = titleObj.AddComponent<TMPro.TextMeshProUGUI>();
        titleTxt.text = "RANKING";
        titleTxt.fontSize = 50;
        titleTxt.fontStyle = TMPro.FontStyles.Bold;
        titleTxt.alignment = TMPro.TextAlignmentOptions.Center;
        titleTxt.color = Color.white;
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0, 0.85f); titleRt.anchorMax = new Vector2(1, 1);
        titleRt.offsetMin = Vector2.zero; titleRt.offsetMax = Vector2.zero;

        // Container (Scroll View)
        GameObject scrollObj = new GameObject("ScrollArea");
        scrollObj.transform.SetParent(panel.transform, false);
        var scrollRt = scrollObj.AddComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0.1f, 0.1f); scrollRt.anchorMax = new Vector2(0.9f, 0.8f);
        scrollRt.offsetMin = Vector2.zero; scrollRt.offsetMax = Vector2.zero;
        
        scrollObj.AddComponent<UnityEngine.UI.Image>().color = new Color(0,0,0,0.5f);
        var scroll = scrollObj.AddComponent<UnityEngine.UI.ScrollRect>();
        
        GameObject content = new GameObject("Content");
        content.transform.SetParent(scrollObj.transform, false);
        var vlg = content.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
        vlg.padding = new RectOffset(20, 20, 20, 20);
        vlg.spacing = 15;
        vlg.childControlHeight = false; vlg.childControlWidth = true;
        vlg.childAlignment = TextAnchor.UpperCenter;
        
        var csf = content.AddComponent<UnityEngine.UI.ContentSizeFitter>();
        csf.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;

        scroll.content = content.GetComponent<RectTransform>();
        scroll.vertical = true; scroll.horizontal = false;
        scroll.movementType = UnityEngine.UI.ScrollRect.MovementType.Elastic;

        // Viewport Mask
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollObj.transform, false);
        viewport.AddComponent<UnityEngine.UI.RectMask2D>();
        var vpRt = viewport.AddComponent<RectTransform>();
        vpRt.anchorMin = Vector2.zero; vpRt.anchorMax = Vector2.one;
        vpRt.offsetMin = Vector2.zero; vpRt.offsetMax = Vector2.zero;
        content.transform.SetParent(viewport.transform, false);

        // Mock Data
        string[] names = { "RhythmKing", "ProGamer_KR", "SodapopLover", "Newbie01", "StepUpMaster", "Guest1234", "HiddenWhale", "NoMissClear", "GalaxiasFan", "Tester99" };
        int[] scores = { 1000000, 985000, 950000, 880000, 850000, 700000, 650000, 500000, 300000, 100000 };

        for(int i=0; i<names.Length; i++)
        {
            GameObject item = new GameObject($"Rank_{i}");
            item.transform.SetParent(content.transform, false);
            
            var img = item.AddComponent<UnityEngine.UI.Image>();
            // 1st: Gold, 2nd: Silver, 3rd: Bronze, Others: Dark
            if(i==0) img.color = new Color(1f, 0.8f, 0.2f, 0.8f); 
            else if(i==1) img.color = new Color(0.8f, 0.8f, 0.9f, 0.8f);
            else if(i==2) img.color = new Color(0.8f, 0.5f, 0.2f, 0.8f);
            else img.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            var le = item.AddComponent<UnityEngine.UI.LayoutElement>();
            le.preferredHeight = 80;

            GameObject txtObj = new GameObject("Info");
            txtObj.transform.SetParent(item.transform, false);
            var txt = txtObj.AddComponent<TMPro.TextMeshProUGUI>();
            txt.text = $"{i+1}. {names[i]}   -   {scores[i]:N0}";
            txt.fontSize = 32;
            txt.alignment = TMPro.TextAlignmentOptions.Center;
            txt.color = (i < 3) ? Color.black : Color.white;
            
            var tRt = txtObj.AddComponent<RectTransform>();
            tRt.anchorMin = Vector2.zero; tRt.anchorMax = Vector2.one;
            tRt.offsetMin = Vector2.zero; tRt.offsetMax = Vector2.zero;
        }
    }
"""

# START REPLACEMENT logic

# 1. Replace TogglePanel
content = re.sub(
    r'private void TogglePanel\(.*?\n    }', 
    new_toggle_panel.strip(), 
    content, 
    flags=re.DOTALL
)

# 2. Replace PopulateSongSelect
content = re.sub(
    r'private void PopulateSongSelect\(.*?\n    }', 
    new_populate_song_select.strip(), 
    content, 
    flags=re.DOTALL
)

# 3. Add PopulateRanking at the end of class (before the last bracket)
# Find the last closing bracket
last_brace_index = content.rfind('}')
if last_brace_index != -1:
    content = content[:last_brace_index] + new_populate_ranking + "\n}"

with open(file_path, 'w', encoding='utf-8', errors='ignore') as f:
    f.write(content)

print("Applied Ranking and Lock Logic.")
