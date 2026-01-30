using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopLogic : MonoBehaviour
{
    private float _forceColorTimer = 0f;

    private void OnEnable()
    {
        // 1. 닫기 버튼 (X) 연결
        Transform closeBtnTrans = transform.Find("MainPanel/CloseButton");
        if (closeBtnTrans != null)
        {
            Button closeBtn = closeBtnTrans.GetComponent<Button>();
            if (closeBtn != null)
            {
                closeBtn.onClick.RemoveAllListeners(); // 중복 방지
                closeBtn.onClick.AddListener(ClosePanel);
            }
        }

        // 2. 배경 클릭 시 닫기
        if (GetComponent<Button>() != null)
        {
            GetComponent<Button>().onClick.RemoveAllListeners();
            GetComponent<Button>().onClick.AddListener(ClosePanel);
        }

        // 3. 충전 버튼 연결
        Transform chargeBtnTrans = transform.Find("MainPanel/ChargeButton");
        if (chargeBtnTrans != null)
        {
            Button chargeBtn = chargeBtnTrans.GetComponent<Button>();
            if (chargeBtn != null)
            {
                chargeBtn.onClick.RemoveAllListeners();
                chargeBtn.onClick.AddListener(OnChargeClick);
            }
        }
        
        // Start forcing colors logic
        _forceColorTimer = 1.0f; // Force for 1 second
    }

    private void Update()
    {
        if (_forceColorTimer > 0)
        {
            _forceColorTimer -= Time.deltaTime;
            ForceUniformItemColors();
        }
    }

    private void ForceUniformItemColors()
    {
        // Find ALL Images under this panel
        Image[] allImages = GetComponentsInChildren<Image>(true);
        // Find ALL Texts to fix types
        TMPro.TextMeshProUGUI[] allTexts = GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
        
        Color navyColor = new Color(0.15f, 0.2f, 0.35f, 1f); // Item: Deep Navy
        Color blackColor = new Color(0.05f, 0.05f, 0.05f, 0.98f); // Background: Almost Black
        
        // [FIX] Resize Close Button (Red X) ONLY
        foreach (var img in allImages)
        {
            // 1. Close Button Fix
            if (img.gameObject.name.Contains("Close") || img.gameObject.name.Contains("X"))
            {
                if (img.color.r > 0.5f && img.color.g < 0.3f && img.rectTransform.sizeDelta.x > 50)
                {
                    img.rectTransform.sizeDelta = new Vector2(40, 40); 
                }
                continue; // Don't recolor the close button itself
            }
            
            // 2. Background Panel -> BLACK
            if (img.gameObject.name == "MainPanel" || img.gameObject.name == "PremiumShop" || img.gameObject.name == "ShopPanel" || img.gameObject.name == "Background")
            {
                 // Check if it's the big background
                 if (img.rectTransform.sizeDelta.x > 800 || img.rectTransform.rect.width > 800)
                 {
                     img.color = blackColor;
                     continue;
                 }
            }

            // 3. Item Backgrounds -> NAVY
            // Skip buttons if needed, but usually item backgrounds are buttons or panels
            // We want to target the Red/Green/Blue boxes
            
            // Logic: If color is clearly Red, Green, or Blue (Custom colors), change to Navy
            // Red-ish (Study support)
            bool isRed = img.color.r > 0.6f && img.color.g < 0.3f && img.color.b < 0.3f;
            // Green-ish (Avatar)
            bool isGreen = img.color.g > 0.6f && img.color.r < 0.3f && img.color.b < 0.3f;
            // Blue-ish (Package)
            bool isBlue = img.color.b > 0.6f && img.color.r < 0.3f && img.color.g < 0.3f; // Exclude charge button?
            // "Charge" button is usually standard UI blue, might want to keep it or force it too.
            // Let's force item boxes.
            
            if (isRed || isGreen || (isBlue && !img.gameObject.name.Contains("Charge")))
            {
                img.color = navyColor;
            }
        }

        // [FIX] Correct Typos (GEMS -> COIN)
        foreach (var txt in allTexts)
        {
            if (txt.text.Contains("GEMS"))
            {
               txt.text = txt.text.Replace("GEMS", "COIN");
            }
        }
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    public void OnChargeClick()
    {
        // [FIX] Determine URL based on environment
        string baseUrl = "http://localhost:5173";
        // Check if SchoolLobbyManager exists to get deployed URL
        if (SchoolLobbyManager.Instance != null && !SchoolLobbyManager.Instance.useLocalDevelopment)
        {
            baseUrl = SchoolLobbyManager.Instance.deployedFrontendUrl;
        }
        
        string url = $"{baseUrl}/payment";
        Application.OpenURL(url);
        Debug.Log($"[Shop] Opening Payment URL: {url}");
    }
}
