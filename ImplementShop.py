
import os
import re

file_path = r"Assets\scripts\SchoolLobbyManager.cs"

with open(file_path, 'r', encoding='utf-8', errors='ignore') as f:
    content = f.read()

# 1. New CreateShopItem (Functional)
new_create_shop_item = """
    private void CreateShopItem(Transform parent, string name, int price, TMPro.TextMeshProUGUI coinTextRef)
    {
        GameObject item = new GameObject(name);
        item.transform.SetParent(parent, false);
        
        // Background
        UnityEngine.UI.Image img = item.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(0.1f, 0.12f, 0.15f, 0.9f); // Dark Slate

        // Layout
        UnityEngine.UI.VerticalLayoutGroup vlg = item.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
        vlg.padding = new RectOffset(10, 10, 10, 10);
        vlg.spacing = 10;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlHeight = false;
        vlg.childControlWidth = true;

        // 1. Icon (Placeholder Color)
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(item.transform, false);
        var iconImg = iconObj.AddComponent<UnityEngine.UI.Image>();
        iconImg.color = new Color(Random.value, Random.value, Random.value); // Random color for variety
        var iconLe = iconObj.AddComponent<UnityEngine.UI.LayoutElement>();
        iconLe.preferredHeight = 120;
        iconLe.flexibleHeight = 0;

        // 2. Name Text
        GameObject nameObj = new GameObject("Name");
        nameObj.transform.SetParent(item.transform, false);
        var nameTxt = nameObj.AddComponent<TMPro.TextMeshProUGUI>();
        nameTxt.text = name;
        nameTxt.fontSize = 24;
        nameTxt.alignment = TMPro.TextAlignmentOptions.Center;
        nameTxt.color = Color.white;
        var nameLe = nameObj.AddComponent<UnityEngine.UI.LayoutElement>();
        nameLe.preferredHeight = 30;

        // 3. Buy Button
        GameObject btnObj = new GameObject("Btn_Buy");
        btnObj.transform.SetParent(item.transform, false);
        var btnImg = btnObj.AddComponent<UnityEngine.UI.Image>();
        btnImg.color = new Color(0, 0.6f, 0.2f); // Green for Buy
        var btn = btnObj.AddComponent<UnityEngine.UI.Button>();
        var btnLe = btnObj.AddComponent<UnityEngine.UI.LayoutElement>();
        btnLe.preferredHeight = 50;

        // Button Text
        GameObject btnTxtObj = new GameObject("Text");
        btnTxtObj.transform.SetParent(btnObj.transform, false);
        var btnTxt = btnTxtObj.AddComponent<TMPro.TextMeshProUGUI>();
        btnTxt.fontSize = 22;
        btnTxt.alignment = TMPro.TextAlignmentOptions.Center;
        btnTxt.color = Color.white;

        // Check Ownership
        bool isOwned = false;
        if (GameManager.Instance != null && GameManager.Instance.ownedItems.Contains(name))
        {
            isOwned = true;
        }

        // Init State
        if (isOwned)
        {
            btnTxt.text = "OWNED";
            btn.interactable = false;
            btnImg.color = Color.gray;
        }
        else
        {
            btnTxt.text = $"{price} G";
            btn.interactable = true;
            btnImg.color = new Color(0, 0.6f, 0.2f);
        }

        // Click Logic
        btn.onClick.AddListener(() => {
            if (GameManager.Instance != null)
            {
                if (GameManager.Instance.TryPurchase(price, name))
                {
                    // Success
                    btnTxt.text = "OWNED";
                    btn.interactable = false;
                    btnImg.color = Color.gray;
                    
                    // Update Coin Display
                    if (coinTextRef != null)
                        coinTextRef.text = $"COIN: {GameManager.Instance.coins}";
                        
                    // Play FX?
                    Debug.Log($"[Shop] Bought {name}!");
                }
                else
                {
                    // Fail (Shake? Red flash?)
                    Debug.Log("[Shop] Not enough money!");
                    btnTxt.text = "NO $$$";
                    // Reset text after delay (simple coroutine or verify later)
                }
            }
        });
    }
"""

# 2. New PopulateShop (Proper Grid & Coin Display)
new_populate_shop = """
    private void PopulateShop(GameObject panel)
    {
        Debug.Log("[Lobby] Populating Shop Logic...");

        // clean up info texts or old containers if rebuilding
        // But we want to preserve the basic structure if it is already good
        // Let's just find/create the container.

        // 1. Coin Display
        TMPro.TextMeshProUGUI coinTxt = null;
        Transform coinTrans = panel.transform.Find("CoinDisplay");
        if (coinTrans != null) 
        {
            coinTxt = coinTrans.GetComponent<TMPro.TextMeshProUGUI>();
        }
        else
        {
            // Create Top Right Coin Display
            GameObject cObj = new GameObject("CoinDisplay");
            cObj.transform.SetParent(panel.transform, false);
            coinTxt = cObj.AddComponent<TMPro.TextMeshProUGUI>();
            coinTxt.fontSize = 36;
            coinTxt.color = Color.yellow;
            coinTxt.alignment = TMPro.TextAlignmentOptions.Right;
            
            RectTransform rt = cObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.7f, 0.9f);
            rt.anchorMax = new Vector2(0.95f, 0.98f);
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }

        if (coinTxt != null && GameManager.Instance != null)
        {
            coinTxt.text = $"COIN: {GameManager.Instance.coins}";
        }

        // 2. Items Container (Grid)
        Transform containerTrans = panel.transform.Find("ShopContainer");
        if (containerTrans != null) Destroy(containerTrans.gameObject); // Re-create to be safe

        GameObject container = new GameObject("ShopContainer");
        container.transform.SetParent(panel.transform, false);
        var grid = container.AddComponent<UnityEngine.UI.GridLayoutGroup>();
        grid.cellSize = new Vector2(200, 280); 
        grid.spacing = new Vector2(30, 30);
        grid.padding = new RectOffset(50, 50, 50, 50);
        grid.childAlignment = TextAnchor.UpperLeft;
        
        RectTransform contRt = container.GetComponent<RectTransform>();
        contRt.anchorMin = new Vector2(0.05f, 0.1f);
        contRt.anchorMax = new Vector2(0.95f, 0.85f);
        contRt.offsetMin = Vector2.zero;
        contRt.offsetMax = Vector2.zero;
        
        // Define Items
        string[] items = { "New Song", "Hard Mode", "Fancy Skin", "Note Pack 1", "Note Pack 2" };
        int[] prices = { 1000, 3000, 500, 1500, 2000 };
        
        for (int i = 0; i < items.Length; i++)
        {
            CreateShopItem(container.transform, items[i], prices[i], coinTxt);
        }
    }
"""

# START REPLACEMENT LOGIC

# Replace CreateShopItem
# Careful with regex, ensure it matches the whole function
content = re.sub(
    r'private void CreateShopItem\(.*?\n    }', 
    new_create_shop_item.strip(), 
    content, 
    flags=re.DOTALL
)

# Replace PopulateShop
content = re.sub(
    r'private void PopulateShop\(.*?\n    }', 
    new_populate_shop.strip(), 
    content, 
    flags=re.DOTALL
)

# Write back
with open(file_path, 'w', encoding='utf-8', errors='ignore') as f:
    f.write(content)

print("Successfully applied Shop Logic to SchoolLobbyManager.cs")
