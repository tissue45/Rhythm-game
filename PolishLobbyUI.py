
import os
import re

file_path = r"Assets\scripts\SchoolLobbyManager.cs"

with open(file_path, 'r', encoding='utf-8', errors='ignore') as f:
    content = f.read()

# 1. Add albumCovers field if missing
if "public Sprite[] albumCovers;" not in content:
    # Insert after class definition or similar field
    content = content.replace(
        "public class SchoolLobbyManager : MonoBehaviour\n{", 
        "public class SchoolLobbyManager : MonoBehaviour\n{\n    [Header(\"Song Options\")]\n    public Sprite[] albumCovers; // [NEW] User assignable album covers\n"
    )

# 2. Polished CreateSongCard (Strip Style - Refined)
new_create_card = """
    private void CreateSongCard(Transform parent, string title, bool unlock, int index)
    {
        if (parent == null) return;

        GameObject card = new GameObject("SongStrip_" + title);
        card.layer = LayerMask.NameToLayer("UI");
        card.transform.SetParent(parent, false);

        // Strip Background
        UnityEngine.UI.Image img = card.AddComponent<UnityEngine.UI.Image>();
        // Sleek Dark Strip with Cyan Highlight if unlocked
        img.color = unlock ? new Color(0.02f, 0.05f, 0.1f, 0.95f) : new Color(0.1f, 0.1f, 0.1f, 0.5f);

        RectTransform rt = card.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(550, 90); // Wider and slim
        rt.localScale = Vector3.one;
        rt.anchoredPosition = Vector3.zero;

        // Selection Highlight (Outline) - Optional simple implementation
        if(unlock)
        {
            var outlineObj = new GameObject("Outline");
            outlineObj.transform.SetParent(card.transform, false);
            var outImg = outlineObj.AddComponent<UnityEngine.UI.Image>();
            outImg.color = new Color(0, 1, 1, 0.1f); // Faint cyan glow
            var outRt = outlineObj.GetComponent<RectTransform>();
            outRt.anchorMin = Vector2.zero; outRt.anchorMax = Vector2.one;
            outRt.offsetMin = new Vector2(0,0); outRt.offsetMax = new Vector2(0,0);
        }

        // Title Text (Left Aligned, Modern Font Style)
        GameObject txtObj = new GameObject("Title");
        txtObj.transform.SetParent(card.transform, false);
        var txt = txtObj.AddComponent<TMPro.TextMeshProUGUI>();
        txt.text = unlock ? title.ToUpper() : title + " [LOCKED]";
        txt.alignment = TMPro.TextAlignmentOptions.Left; 
        txt.fontSize = 28;
        txt.fontStyle = TMPro.FontStyles.Bold;
        txt.color = unlock ? Color.white : Color.gray;
        txt.raycastTarget = false;
        
        RectTransform txtRt = txtObj.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero; 
        txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = new Vector2(30, 0); // Padding left
        txtRt.offsetMax = Vector2.zero;

        // Button Logic
        UnityEngine.UI.Button btn = card.AddComponent<UnityEngine.UI.Button>();
        UnityEngine.UI.ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(0, 0.8f, 1f, 0.2f); // Cyan Hover
        colors.pressedColor = new Color(0, 0.5f, 0.8f, 0.5f);
        btn.colors = colors;

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

# 3. Polished PopulateSongSelect (Split Screen + Album Art + Sleek Buttons)
new_populate = """
    private void PopulateSongSelect(GameObject panel)
    {
        Debug.Log($"[Lobby] Layout: Polished DJMAX Style");

        // --- 1. Clean Up Old ---
        foreach (Transform child in panel.transform)
        {
            Destroy(child.gameObject); // Clean EVERYTHING for a fresh rebuild
        }

        // Background Tint (Main Panel Background)
        var bgImg = panel.GetComponent<UnityEngine.UI.Image>();
        if(bgImg == null) bgImg = panel.AddComponent<UnityEngine.UI.Image>();
        bgImg.color = new Color(0, 0, 0, 0.95f); // Deep Black setup

        // --- 2. Left Panel (Detail & Action) ---
        GameObject leftPanel = new GameObject("LeftDetailPanel");
        leftPanel.transform.SetParent(panel.transform, false);
        leftPanel.layer = LayerMask.NameToLayer("UI");
        RectTransform leftRt = leftPanel.AddComponent<RectTransform>();
        leftRt.anchorMin = new Vector2(0.05f, 0.1f); 
        leftRt.anchorMax = new Vector2(0.45f, 0.9f); 
        leftRt.offsetMin = Vector2.zero; leftRt.offsetMax = Vector2.zero;
        
        // Album Art (Main Visual)
        GameObject artObj = new GameObject("AlbumArt");
        artObj.transform.SetParent(leftPanel.transform, false);
        var artImg = artObj.AddComponent<UnityEngine.UI.Image>();
        artImg.color = Color.darkGray; // Default
        artImg.preserveAspect = true;
        RectTransform artRt = artObj.GetComponent<RectTransform>();
        artRt.anchorMin = new Vector2(0, 0.3f); // Top part of left panel
        artRt.anchorMax = new Vector2(1, 1);
        artRt.offsetMin = Vector2.zero; artRt.offsetMax = Vector2.zero;

        // Song Title Text (Under Art)
        GameObject titleObj = new GameObject("DetailTitle");
        titleObj.transform.SetParent(leftPanel.transform, false);
        var titleTxt = titleObj.AddComponent<TMPro.TextMeshProUGUI>();
        titleTxt.text = "SELECT TRACK";
        titleTxt.fontSize = 42;
        titleTxt.fontStyle = TMPro.FontStyles.Bold;
        titleTxt.alignment = TMPro.TextAlignmentOptions.Center;
        titleTxt.color = new Color(0, 0.9f, 1f); // Cyan
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0, 0.2f); 
        titleRt.anchorMax = new Vector2(1, 0.3f);
        titleRt.offsetMin = Vector2.zero; titleRt.offsetMax = Vector2.zero;

        // START GAME Button (Sleek Cyan)
        GameObject startBtnObj = new GameObject("Btn_StartGame");
        startBtnObj.transform.SetParent(leftPanel.transform, false);
        var startImg = startBtnObj.AddComponent<UnityEngine.UI.Image>();
        startImg.color = new Color(0, 0.7f, 0.9f); // Cyan Base
        var startBtn = startBtnObj.AddComponent<UnityEngine.UI.Button>();
        RectTransform startRt = startBtnObj.GetComponent<RectTransform>();
        startRt.anchorMin = new Vector2(0.1f, 0); 
        startRt.anchorMax = new Vector2(0.9f, 0.15f); // Bottom area
        startRt.offsetMin = Vector2.zero; startRt.offsetMax = Vector2.zero;
        
        // Start Text
        GameObject stTxtObj = new GameObject("Text");
        stTxtObj.transform.SetParent(startBtnObj.transform, false);
        var stTxt = stTxtObj.AddComponent<TMPro.TextMeshProUGUI>();
        stTxt.text = "GAME START";
        stTxt.fontSize = 32;
        stTxt.alignment = TMPro.TextAlignmentOptions.Center;
        stTxt.color = Color.black; // Contrast
        var stRt = stTxtObj.GetComponent<RectTransform>();
        stRt.anchorMin = Vector2.zero; stRt.anchorMax = Vector2.one;
        stRt.offsetMin = Vector2.zero; stRt.offsetMax = Vector2.zero;

        // Bind Start Logic
        startBtn.onClick.AddListener(()=> { OnGameStartClick(); }); // Re-use existing logic, or bind directly
        // Note: OnGameStartClick in this class toggles panel. We want "Real Start".
        // Binding to internal StartGameLoop logic:
        startBtn.onClick.AddListener(()=> { StartGameLoop(); });


        // --- 3. Right Panel (List Container) ---
        GameObject rightPanel = new GameObject("CarouselContainer");
        rightPanel.transform.SetParent(panel.transform, false);
        rightPanel.layer = LayerMask.NameToLayer("UI");
        RectTransform rightRt = rightPanel.AddComponent<RectTransform>();
        rightRt.anchorMin = new Vector2(0.5f, 0.1f); 
        rightRt.anchorMax = new Vector2(0.95f, 0.9f);
        rightRt.offsetMin = Vector2.zero; rightRt.offsetMax = Vector2.zero;

        // Close Button (Top Right of Panel) - Sleek 'X' or 'Back'
        GameObject closeBtnObj = new GameObject("Btn_Close");
        closeBtnObj.transform.SetParent(panel.transform, false);
        var closeImg = closeBtnObj.AddComponent<UnityEngine.UI.Image>();
        closeImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        var closeBtn = closeBtnObj.AddComponent<UnityEngine.UI.Button>();
        closeBtn.onClick.AddListener(() => { TogglePanel(panel); });
        
        RectTransform closeRt = closeBtnObj.GetComponent<RectTransform>();
        closeRt.anchorMin = new Vector2(0.92f, 0.92f);
        closeRt.anchorMax = new Vector2(0.98f, 0.98f);
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


        // ScrollRect for List
        var scroll = rightPanel.AddComponent<UnityEngine.UI.ScrollRect>();
        rightPanel.AddComponent<UnityEngine.UI.Image>().color = new Color(0,0,0,0); // Transparent
        
        // Content
        GameObject contentObj = new GameObject("Content");
        contentObj.layer = LayerMask.NameToLayer("UI");
        contentObj.transform.SetParent(rightPanel.transform, false);
        var contentRt = contentObj.AddComponent<RectTransform>();
        contentRt.sizeDelta = new Vector2(500, 1200); // Taller for vertical list
        
        scroll.content = contentRt;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = UnityEngine.UI.ScrollRect.MovementType.Elastic;

        // CarouselView
        var cv = rightPanel.AddComponent<CarouselView>();
        cv.container = contentRt;
        cv.spacing = 110f; // Adjusted spacing
        cv.scaleFactor = 1.0f; // No heavy scaling for list look
        cv.isVertical = true;

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
                titleTxt.text = s.ToUpper();
                
                // [NEW] Update Album Art from Inspector Array
                if (albumCovers != null && index < albumCovers.Length && albumCovers[index] != null)
                {
                    artImg.sprite = albumCovers[index];
                    artImg.color = Color.white; // Show sprite
                }
                else
                {
                    // Fallback Colors if sprite missing
                    if(s == "GALAXIAS!") artImg.color = new Color(1, 0, 0.5f);
                    else if(s == "Sodapop") artImg.color = new Color(0, 0.5f, 1f);
                    else artImg.color = Color.grey;
                }
            }
        };
    }
"""

# START REPLACEMENT LOGIC

# Replace CreateSongCard
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

print("Successfully applied Polish Update to SchoolLobbyManager.cs")
