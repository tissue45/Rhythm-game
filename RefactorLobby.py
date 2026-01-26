
import os

file_path = r"Assets\scripts\SchoolLobbyManager.cs"

with open(file_path, 'r', encoding='utf-8', errors='ignore') as f:
    content = f.read()

# 1. New CreateSongCard Implementation (Strip Style)
new_create_card = """
    private void CreateSongCard(Transform parent, string title, bool unlock, int index)
    {
        if (parent == null) return;

        GameObject card = new GameObject("SongStrip_" + title);
        card.layer = LayerMask.NameToLayer("UI");
        card.transform.SetParent(parent, false);

        // Strip Background
        UnityEngine.UI.Image img = card.AddComponent<UnityEngine.UI.Image>();
        // Unlocked: Dark Blue Strip | Locked: Grey
        img.color = unlock ? new Color(0.05f, 0.1f, 0.3f, 0.9f) : new Color(0.1f, 0.1f, 0.1f, 0.8f);

        RectTransform rt = card.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(500, 80); // Long strip
        rt.localScale = Vector3.one;
        rt.anchoredPosition = Vector3.zero;

        // Title Text (Left Aligned)
        GameObject txtObj = new GameObject("Title");
        txtObj.transform.SetParent(card.transform, false);
        var txt = txtObj.AddComponent<TMPro.TextMeshProUGUI>();
        txt.text = unlock ? title : $"{title} (Locked)";
        txt.alignment = TMPro.TextAlignmentOptions.Left; // Left align
        txt.fontSize = 32;
        txt.color = Color.white;
        txt.raycastTarget = false;
        
        RectTransform txtRt = txtObj.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero; 
        txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = new Vector2(40, 0); // Left padding
        txtRt.offsetMax = Vector2.zero;

        // Button Logic
        UnityEngine.UI.Button btn = card.AddComponent<UnityEngine.UI.Button>();
        btn.onClick.AddListener(() => {
            if (unlock)
            {
                _selectedSongTitle = title;
                Debug.Log($"[Lobby] Selected: {title}");
                
                // Snap Carousel
                var cv = parent.GetComponentInParent<CarouselView>();
                if (cv != null) cv.SnapTo(index);
            }
        });
    }
"""

# 2. New PopulateSongSelect Implementation (Split Screen)
new_populate = """
    private void PopulateSongSelect(GameObject panel)
    {
        Debug.Log($"[Lobby] Layout: DJMAX Style Split Screen");

        // --- 1. Clean Up Old ---
        foreach (Transform child in panel.transform)
        {
            if (child.name == "CloseButton" || child.name == "StartButton") continue;
            Destroy(child.gameObject);
        }

        // --- 2. Left Panel (Detail) ---
        GameObject leftPanel = new GameObject("LeftDetailPanel");
        leftPanel.transform.SetParent(panel.transform, false);
        leftPanel.layer = LayerMask.NameToLayer("UI");
        RectTransform leftRt = leftPanel.AddComponent<RectTransform>();
        leftRt.anchorMin = new Vector2(0, 0); 
        leftRt.anchorMax = new Vector2(0.4f, 1); // 40% Width
        leftRt.offsetMin = Vector2.zero; leftRt.offsetMax = Vector2.zero;
        
        // Album Art Placeholders
        GameObject artObj = new GameObject("AlbumArt");
        artObj.transform.SetParent(leftPanel.transform, false);
        var artImg = artObj.AddComponent<UnityEngine.UI.Image>();
        artImg.color = Color.black; // Placeholder
        RectTransform artRt = artObj.GetComponent<RectTransform>();
        artRt.sizeDelta = new Vector2(400, 400);
        artRt.anchoredPosition = new Vector2(0, 50);

        // Song Title Text (Big)
        GameObject titleObj = new GameObject("DetailTitle");
        titleObj.transform.SetParent(leftPanel.transform, false);
        var titleTxt = titleObj.AddComponent<TMPro.TextMeshProUGUI>();
        titleTxt.text = "SELECT SONG";
        titleTxt.fontSize = 50;
        titleTxt.alignment = TMPro.TextAlignmentOptions.Center;
        titleTxt.color = Color.cyan;
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchoredPosition = new Vector2(0, -200);
        titleRt.sizeDelta = new Vector2(500, 100);

        // --- 3. Right Panel (List Container) ---
        GameObject rightPanel = new GameObject("CarouselContainer");
        rightPanel.transform.SetParent(panel.transform, false);
        rightPanel.layer = LayerMask.NameToLayer("UI");
        RectTransform rightRt = rightPanel.AddComponent<RectTransform>();
        rightRt.anchorMin = new Vector2(0.4f, 0); 
        rightRt.anchorMax = new Vector2(1, 1); // 60% Width
        rightRt.offsetMin = Vector2.zero; rightRt.offsetMax = Vector2.zero;

        // ScrollRect
        var scroll = rightPanel.AddComponent<UnityEngine.UI.ScrollRect>();
        rightPanel.AddComponent<UnityEngine.UI.Image>().color = new Color(0,0,0,0);
        
        // Content
        GameObject contentObj = new GameObject("Content");
        contentObj.layer = LayerMask.NameToLayer("UI");
        contentObj.transform.SetParent(rightPanel.transform, false);
        var contentRt = contentObj.AddComponent<RectTransform>();
        contentRt.sizeDelta = new Vector2(500, 1500); // Taller for vertical list
        
        scroll.content = contentRt;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = UnityEngine.UI.ScrollRect.MovementType.Elastic;

        // CarouselView
        var cv = rightPanel.AddComponent<CarouselView>();
        cv.container = contentRt;
        cv.spacing = 100f; // Tighter spacing for list
        cv.scaleFactor = 1.05f; // Slight zoom for selected
        cv.isVertical = true; // Force vertical

        // --- 4. Populate List ---
        string[] songs = { "GALAXIAS!", "Sodapop", "Coming Soon" };
        for(int i=0; i<songs.Length; i++)
        {
            CreateSongCard(contentRt, songs[i], i <= 1, i);
        }

        // --- 5. Init & Bind Selection ---
        cv.Invoke("InitializeItems", 0.1f);
        cv.OnItemSelected = (index) => {
            if (index >= 0 && index < songs.Length)
            {
                string s = songs[index];
                _selectedSongTitle = s;
                titleTxt.text = s; // Update Left Panel Title
                
                // Update Album Art Color (Fake Art)
                if(s == "GALAXIAS!") artImg.color = new Color(1, 0, 0.5f);
                else if(s == "Sodapop") artImg.color = new Color(0, 0.5f, 1f);
                else artImg.color = Color.grey;
            }
        };

        // Bind Start Button
        BindSongSelectStartButton(panel);
    }
"""

# START REPLACEMENT LOGIC
import re

# Replace CreateSongCard
# Regex matches: private void CreateSongCard ... (up to next private/public method)
content = re.sub(
    r'private void CreateSongCard\(.*?\n    }', 
    new_create_card.strip(), 
    content, 
    flags=re.DOTALL
)

# Replace PopulateSongSelect
content = re.sub(
    r'private void PopulateSongSelect\(.*?\n    }', 
    new_populate.strip(), 
    content, 
    flags=re.DOTALL
)

# Write back
with open(file_path, 'w', encoding='utf-8', errors='ignore') as f:
    f.write(content)

print("Successfully refactored SchoolLobbyManager.cs")
