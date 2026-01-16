using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(LobbyManager))]
[ExecuteAlways] // ?ë””?°ì—?œë„ ?¤í¬ë¦½íŠ¸ê°€ ?Œì•„ê°€ê²???
public class LobbyUIBuilder : MonoBehaviour
{
    [Header("Custom UI Images")]
    public Sprite borderSprite; // ê²Œì„ ?Œë‘ë¦?(?„ìš”?†ìœ¼ë©?ë¹„ì›Œ?ì„¸??
    public Sprite titleLogoSprite; // ?€?´í? ë¡œê³  ?´ë?ì§€
    public Sprite startButtonSprite; // ê²Œì„ ?œì‘ ë²„íŠ¼ ?´ë?ì§€
    public Sprite quitButtonSprite; // ê²Œì„ ì¢…ë£Œ ë²„íŠ¼ ?´ë?ì§€

    [Header("UI Settings")]
    public Vector2 logoSize = new Vector2(1100, 1100); // ë¡œê³  ?¬ê¸°
    public Vector2 startButtonSize = new Vector2(1000, 1000); // ?œì‘ ë²„íŠ¼ ?¬ê¸°
    public Vector2 quitButtonSize = new Vector2(1000, 1000); // ì¢…ë£Œ ë²„íŠ¼ ?¬ê¸°
    
    [Header("UI Positions")]
    public Vector2 logoPosition = Vector2.zero; // ë¡œê³  ?„ì¹˜
    public Vector2 startButtonPosition = new Vector2(0, -170); // ?œì‘ ë²„íŠ¼ ?„ì¹˜
    public Vector2 quitButtonPosition = new Vector2(0, -400); // ì¢…ë£Œ ë²„íŠ¼ ?„ì¹˜

    private bool _isDirty = false;

    private void Start()
    {
        if (Application.isPlaying)
        {
            // NetworkManager ?†ìœ¼ë©??ì„±
            if (Object.FindFirstObjectByType<NetworkManager>() == null)
            {
                new GameObject("NetworkManager").AddComponent<NetworkManager>();
            }

            BuildUI();

            // [MODIFIED] ? ì? ?”ì²­?¼ë¡œ ?´ë‹¹ ë²„íŠ¼??ë°??¨ë„ ê°•ì œ ë¹„í™œ?±í™”
            // ì´ˆê¸° ?íƒœ: ë©”ì¸ ë©”ë‰´ (Start ë²„íŠ¼ ?œì‹œ) -> ?œê±°
            // SetGameReady(true);
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                 GameObject startBtn = FindChild(canvas.gameObject, "StartButton");
                 if (startBtn != null) startBtn.SetActive(false); // ?œì„±??X -> ë¹„í™œ?±í™”

                 GameObject quitBtn = FindChild(canvas.gameObject, "QuitButton");
                 if (quitBtn != null) quitBtn.SetActive(false); // ?œì„±??X -> ë¹„í™œ?±í™”

                 GameObject panel = FindChild(canvas.gameObject, "ConnectionPanel");
                 if (panel != null) panel.SetActive(false); // ë¹„í™œ?±í™”
            }

            // ?´ë²¤???°ê²°
            NetworkManager net = Object.FindFirstObjectByType<NetworkManager>();
            if (net != null)
            {
                net.OnConnected += OnClientConnected;
            }
        }
    }

    private void OnClientConnected()
    {
        // ?°ê²°?˜ë©´ ë°”ë¡œ ?œì‘?˜ì? ?Šê³  ì¹´ìš´?¸ë‹¤???œì‘
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            GameObject panel = FindChild(canvas.gameObject, "ConnectionPanel");
            if (panel != null && panel.activeSelf)
            {
                StartCoroutine(StartGameCountdown(panel));
            }
        }
    }

    private System.Collections.IEnumerator StartGameCountdown(GameObject panel)
    {
        GameObject card = FindChild(panel, "CardBackground");
        if (card == null) yield break;

        GameObject txtObj = FindChild(card, "InfoText");
        TextMeshProUGUI txt = (txtObj != null) ? txtObj.GetComponent<TextMeshProUGUI>() : null;
        
        // QR ì½”ë“œ ?´ë?ì§€???¨ê¸°ê¸?(ê¹”ë”?˜ê²Œ)
        GameObject qrObj = FindChild(card, "QRCode");
        if (qrObj != null) qrObj.SetActive(false);

        float duration = 3.0f;
        while (duration > 0)
        {
            if (txt != null) 
                txt.text = $"Connected!\nStarting in {Mathf.CeilToInt(duration)}...";
            
            yield return null;
            duration -= Time.deltaTime;
        }

        if (txt != null) txt.text = "GO!";
        yield return new WaitForSeconds(0.5f);

        GetComponent<LobbyManager>().StartGame();
    }

    public void OnStartButtonClicked()
    {
        NetworkManager net = NetworkManager.Instance;
        if (net != null && net.isConnected)
        {
            // ?´ë? ?°ê²°?˜ì–´ ?ˆìœ¼ë©?ë°”ë¡œ ?œì‘
            GetComponent<LobbyManager>().StartGame();
        }
        else
        {
            // ?°ê²° ???˜ì–´ ?ˆìœ¼ë©?QR ì½”ë“œ ?¨ë„ ?œì‹œ
            SetGameReady(false);
        }
    }

    private void SetGameReady(bool isReady)
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject startBtn = FindChild(canvas.gameObject, "StartButton");
        if (startBtn != null) startBtn.SetActive(isReady);

        GameObject panel = FindChild(canvas.gameObject, "ConnectionPanel");
        if (panel != null) panel.SetActive(!isReady);
    }

    private void OnValidate()
    {
        // ?¸ìŠ¤?™í„°?ì„œ ê°’ì´ ë°”ë€Œë©´ ê°±ì‹  ?ˆì•½
        _isDirty = true;
    }

    private void Update()
    {
        // ?ë””??ëª¨ë“œ???Œë§Œ, ê°’ì´ ë°”ë€Œì—ˆ?¼ë©´ UI ?¤ì‹œ ê·¸ë¦¬ê¸?
        if (!Application.isPlaying && _isDirty)
        {
            _isDirty = false;
            BuildUI();
        }
    }

    [ContextMenu("Build Lobby UI")]
    public void BuildUI()
    {
        // 1. Canvas ì°¾ê¸° ?ëŠ” ?ì„±
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<GraphicRaycaster>();
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }

        // 1.5 EventSystem ì°¾ê¸° ?ëŠ” ?ì„± (UI ?´ë¦­ ?„ìˆ˜?”ì†Œ)
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // 2. ë°°ê²½ (Panel)
        GameObject bgObj = FindChild(canvas.gameObject, "BackgroundPanel");
        if (bgObj == null)
        {
            bgObj = new GameObject("BackgroundPanel");
            bgObj.transform.SetParent(canvas.transform, false);
            Image img = bgObj.AddComponent<Image>();
            img.color = new Color(0.05f, 0.05f, 0.1f, 0.8f); 
            
            RectTransform rt = bgObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
        bgObj.transform.SetAsFirstSibling();

        // [NEW] ê²Œì„ ?Œë‘ë¦?(Border)
        if (borderSprite != null)
        {
            GameObject borderObj = FindChild(canvas.gameObject, "GameBorder");
            if (borderObj == null)
            {
                borderObj = new GameObject("GameBorder");
                borderObj.transform.SetParent(canvas.transform, false);
                Image img = borderObj.AddComponent<Image>();
                img.sprite = borderSprite;
                img.raycastTarget = false; 
                
                RectTransform rt = borderObj.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
        }

        // [NEW] 3D ë°°ê²½ ?ì„±
        Create3DBackground();

#if UNITY_EDITOR
        MakeTextureReadable(startButtonSprite);
        MakeTextureReadable(quitButtonSprite);
#endif

        // 3. ?€?´í? (Text or Logo Image)
        GameObject titleObj = FindChild(canvas.gameObject, "TitleText");
        if (titleObj == null)
        {
            titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(canvas.transform, false);
            
            RectTransform rt = titleObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.7f); 
            rt.anchorMax = new Vector2(0.5f, 0.7f);
            rt.anchoredPosition = Vector2.zero;
        }

        if (titleLogoSprite != null)
        {
            TextMeshProUGUI oldTxt = titleObj.GetComponent<TextMeshProUGUI>();
            if (oldTxt != null) DestroyImmediate(oldTxt);
            
            Image img = titleObj.GetComponent<Image>();
            if (img == null) img = titleObj.AddComponent<Image>();
            
            img.sprite = titleLogoSprite;
            img.preserveAspect = true; 
            // ?¬ì´ì¦??¤ì • (?¬ìš©??ì§€??
            titleObj.GetComponent<RectTransform>().sizeDelta = logoSize; 
            titleObj.GetComponent<RectTransform>().anchoredPosition = logoPosition;
        }
        else
        {
            Image oldImg = titleObj.GetComponent<Image>();
            if (oldImg != null) DestroyImmediate(oldImg);

            TextMeshProUGUI txt = titleObj.GetComponent<TextMeshProUGUI>();
            if (txt == null) txt = titleObj.AddComponent<TextMeshProUGUI>();
            
            txt.text = "STEP UP"; 
            txt.fontSize = 80;
            txt.alignment = TextAlignmentOptions.Center;
            txt.color = new Color(0f, 1f, 1f); 
            txt.fontStyle = FontStyles.Bold | FontStyles.Italic;
            txt.characterSpacing = 10;
            
            if (titleObj.GetComponent<Shadow>() == null)
            {
                Shadow shadow = titleObj.AddComponent<Shadow>();
                shadow.effectColor = new Color(0f, 0.5f, 0.5f, 0.5f);
                shadow.effectDistance = new Vector2(5, -5);
            }

            if (titleObj.GetComponent<Outline>() == null)
            {
                Outline outline = titleObj.AddComponent<Outline>();
                outline.effectColor = new Color(0f, 0.2f, 0.2f);
                outline.effectDistance = new Vector2(2, -2);
            }
            
            titleObj.GetComponent<RectTransform>().sizeDelta = new Vector2(1500, 300);
            titleObj.GetComponent<RectTransform>().anchoredPosition = logoPosition;
        }

        // [MODIFIED] ? ì? ?”ì²­?¼ë¡œ ?ë™ ?ì„± ë¹„í™œ?±í™”
        // [NEW] ?°ê²° ?€ê¸??”ë©´ (QR ì½”ë“œ) -> ?ë™ ?ì„± ??
        // CreateConnectionPanel(canvas.transform);

        // 4. ?œì‘ ë²„íŠ¼ (?´ë?ì§€ ì§€?? ?¬ì´ì¦??„ì¹˜ ì¡°ì ˆ ê°€?? -> ?ë™ ?ì„± ??
        /*
        CreateButton(canvas.transform, "StartButton", "START GAME", startButtonPosition, new Color(0.2f, 0.2f, 0.2f), new Color(0f, 1f, 0.5f), startButtonSprite, startButtonSize, () => {
            OnStartButtonClicked();
        });
        */

        // 5. ì¢…ë£Œ ë²„íŠ¼ (?´ë?ì§€ ì§€?? ?¬ì´ì¦??„ì¹˜ ì¡°ì ˆ ê°€?? -> ?ë™ ?ì„± ??
        /*
        CreateButton(canvas.transform, "QuitButton", "EXIT", quitButtonPosition, new Color(0.2f, 0.2f, 0.2f), new Color(1f, 0.2f, 0.5f), quitButtonSprite, quitButtonSize, () => {
            GetComponent<LobbyManager>().QuitGame();
        });
        */
        
        // ì¹´ë©”???Œë„?™ìŠ¤ ?¨ê³¼
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            if (mainCam.GetComponent<LobbyCameraEffect>() == null)
            {
                mainCam.gameObject.AddComponent<LobbyCameraEffect>();
            }
        }

        Debug.Log("Lobby UI Built Successfully!");
    }

    private void CreateConnectionPanel(Transform parent)
    {
        GameObject panelObj = FindChild(parent.gameObject, "ConnectionPanel");
        if (panelObj == null)
        {
            panelObj = new GameObject("ConnectionPanel");
            panelObj.transform.SetParent(parent, false);
            
            // 1. ?„ì²´ ?”ë©´ ë°°ê²½ (?´ë‘??ë°˜íˆ¬ëª?
            Image img = panelObj.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.9f);
            
            RectTransform rt = panelObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // 2. ì¹´ë“œ ë°°ê²½ (ì¤‘ì•™ ?•ë ¬)
            GameObject cardObj = new GameObject("CardBackground");
            cardObj.transform.SetParent(panelObj.transform, false);
            Image cardImg = cardObj.AddComponent<Image>();
            cardImg.color = new Color(0.15f, 0.15f, 0.2f, 1f); // ?¤í¬ ë¸”ë£¨ ê·¸ë ˆ??
            
            // ?¥ê·¼ ëª¨ì„œë¦??¨ê³¼ (Outline ì»´í¬?ŒíŠ¸ë¡??€ì²´í•˜ê±°ë‚˜ ?¤í”„?¼ì´???„ìš”, ?¬ê¸°???‰ìƒë§?
            if (borderSprite != null) cardImg.sprite = borderSprite; // ?Œë‘ë¦??¤í”„?¼ì´???¬í™œ??ê°€?¥í•˜ë©??¬ìš©

            RectTransform cardRt = cardObj.GetComponent<RectTransform>();
            cardRt.anchorMin = new Vector2(0.5f, 0.5f);
            cardRt.anchorMax = new Vector2(0.5f, 0.5f);
            cardRt.sizeDelta = new Vector2(800, 900); // ì¹´ë“œ ?¬ê¸°
            cardRt.anchoredPosition = Vector2.zero;

            // 3. QR ì½”ë“œ ?´ë?ì§€
            GameObject qrObj = new GameObject("QRCode");
            qrObj.transform.SetParent(cardObj.transform, false);
            RawImage qrImg = qrObj.AddComponent<RawImage>();
            
            Texture2D qrTex = Resources.Load<Texture2D>("qrcode");
            if (qrTex != null)
            {
                qrImg.texture = qrTex;
                qrObj.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 500);
            }
            else
            {
                qrImg.color = Color.white;
                qrObj.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 500);
            }
            qrObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 50);

            // 4. ?ˆë‚´ ?ìŠ¤??
            GameObject txtObj = new GameObject("InfoText");
            txtObj.transform.SetParent(cardObj.transform, false);
            TextMeshProUGUI txt = txtObj.AddComponent<TextMeshProUGUI>();
            txt.text = "Scan with your Phone";
            txt.fontSize = 50;
            txt.alignment = TextAlignmentOptions.Center;
            txt.color = new Color(0.8f, 0.9f, 1f);
            txt.fontStyle = FontStyles.Bold;
            
            RectTransform txtRt = txtObj.GetComponent<RectTransform>();
            txtRt.anchoredPosition = new Vector2(0, 350);
            txtRt.sizeDelta = new Vector2(700, 100);

            // 5. ?œë¸Œ ?ìŠ¤??
            GameObject subTxtObj = new GameObject("SubText");
            subTxtObj.transform.SetParent(cardObj.transform, false);
            TextMeshProUGUI subTxt = subTxtObj.AddComponent<TextMeshProUGUI>();
            subTxt.text = "Make sure both devices are on the\nSAME Wi-Fi Network";
            subTxt.fontSize = 30;
            subTxt.alignment = TextAlignmentOptions.Center;
            subTxt.color = new Color(0.6f, 0.6f, 0.7f);
            
            RectTransform subTxtRt = subTxtObj.GetComponent<RectTransform>();
            subTxtRt.anchoredPosition = new Vector2(0, -250);
            subTxtRt.sizeDelta = new Vector2(700, 100);

            // 6. ì·¨ì†Œ ë²„íŠ¼
            CreateButton(cardObj.transform, "CancelButton", "CANCEL", new Vector2(0, -380), new Color(0.3f, 0.3f, 0.3f), Color.white, null, new Vector2(300, 80), () => {
                SetGameReady(true); // ?¤ì‹œ ë©”ì¸?¼ë¡œ
            });
        }
        else
        {
            // ?´ë? ì¡´ì¬?˜ë©´ QR ì½”ë“œ ?´ë?ì§€ë§?ê°±ì‹  ?œë„
            Transform cardTrans = panelObj.transform.Find("CardBackground");
            if (cardTrans != null)
            {
                Transform qrTrans = cardTrans.Find("QRCode");
                if (qrTrans != null)
                {
                    RawImage qrImg = qrTrans.GetComponent<RawImage>();
                    Texture2D qrTex = Resources.Load<Texture2D>("qrcode");
                    if (qrTex != null) qrImg.texture = qrTex;
                }
            }
        }
    }

    private void CreateButton(Transform parent, string name, string text, Vector2 position, Color bgColor, Color textColor, Sprite sprite, Vector2 size, UnityEngine.Events.UnityAction action)
    {
        GameObject btnObj = FindChild(parent.gameObject, name);
        if (btnObj == null)
        {
            btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            
            Image img = btnObj.AddComponent<Image>();
            
            // ?´ë?ì§€ê°€ ?ˆìœ¼ë©??´ë?ì§€ ?¬ìš©, ?†ìœ¼ë©??‰ìƒ ?¬ìš©
            if (sprite != null)
            {
                img.sprite = sprite;
                img.color = Color.white; // ?´ë?ì§€ê°€ ?ˆìœ¼ë©??°ìƒ‰(?ë³¸??
                img.preserveAspect = true;
                img.alphaHitTestMinimumThreshold = 0.1f; // [FIX] ?¬ëª…??ë¶€ë¶??´ë¦­ ë¬´ì‹œ
            }
            else
            {
                img.color = bgColor;
            }

            Button btn = btnObj.AddComponent<Button>();

            // ?ìŠ¤??(?´ë?ì§€ê°€ ?†ì„ ?Œë§Œ ?ì„±)
            GameObject txtObj = FindChild(btnObj, "Text");
            if (sprite == null)
            {
                if (txtObj == null)
                {
                    txtObj = new GameObject("Text");
                    txtObj.transform.SetParent(btnObj.transform, false);
                    TextMeshProUGUI txt = txtObj.AddComponent<TextMeshProUGUI>();
                    txt.text = text;
                    txt.fontSize = 40; 
                    txt.alignment = TextAlignmentOptions.Center;
                    txt.color = textColor; 
                    txt.fontStyle = FontStyles.Bold | FontStyles.Italic;
                    
                    RectTransform txtRt = txtObj.GetComponent<RectTransform>();
                    txtRt.anchorMin = Vector2.zero;
                    txtRt.anchorMax = Vector2.one;
                    txtRt.offsetMin = Vector2.zero;
                    txtRt.offsetMax = Vector2.zero;
                }
            }
            else
            {
                // ?´ë?ì§€ê°€ ?ˆëŠ”???ìŠ¤???¤ë¸Œ?íŠ¸ê°€ ?¨ì•„?ˆìœ¼ë©??? œ (ê¹”ë”?˜ê²Œ)
                if (txtObj != null) DestroyImmediate(txtObj);
            }

            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f); 
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size; // ?¬ìš©??ì§€???¬ì´ì¦??ìš©
            rt.anchoredPosition = position;
            
            ColorBlock colors = btn.colors;
            if (sprite == null)
            {
                colors.normalColor = bgColor;
                colors.highlightedColor = bgColor + new Color(0.1f, 0.1f, 0.1f);
                colors.pressedColor = bgColor - new Color(0.1f, 0.1f, 0.1f);
                colors.selectedColor = bgColor;
            }
            else
            {
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f);
                colors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
                colors.selectedColor = Color.white;
            }
            btn.colors = colors;
        }
        else
        {
            // ?´ë? ì¡´ì¬?????…ë°?´íŠ¸ ë¡œì§
            Image img = btnObj.GetComponent<Image>();
            GameObject txtObj = FindChild(btnObj, "Text");

            if (sprite != null)
            {
                img.sprite = sprite;
                img.color = Color.white;
                img.preserveAspect = true;
                img.alphaHitTestMinimumThreshold = 0.1f; // [FIX] ?¬ëª…??ë¶€ë¶??´ë¦­ ë¬´ì‹œ
                if (txtObj != null) DestroyImmediate(txtObj);
            }
            
            // ?¬ì´ì¦??…ë°?´íŠ¸
            RectTransform rt = btnObj.GetComponent<RectTransform>();
            if (rt != null) 
            {
                rt.sizeDelta = size;
                rt.anchoredPosition = position; // [FIX] ?„ì¹˜??ê°™ì´ ?…ë°?´íŠ¸
            }

            // ê·¸ë¦¼???„ì›ƒ?¼ì¸ ?œê±° (ê¹”ë”?˜ê²Œ)
            Shadow shadow = btnObj.GetComponent<Shadow>();
            if (shadow != null) DestroyImmediate(shadow);
            Outline outline = btnObj.GetComponent<Outline>();
            if (outline != null) DestroyImmediate(outline);
            
            // ?°í???ë¦¬ìŠ¤???¬ì—°ê²?
            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                // [FIX] ë²„íŠ¼ ?‰ìƒ ?íƒœ??ê°™ì´ ?…ë°?´íŠ¸?´ì•¼ ??
                ColorBlock colors = btn.colors;
                if (sprite != null)
                {
                    colors.normalColor = Color.white;
                    colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f);
                    colors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
                    colors.selectedColor = Color.white;
                }
                else
                {
                    // ?´ë?ì§€ê°€ ?†ìœ¼ë©?ë°°ê²½???¬ìš© (ê¸°ì¡´ ë¡œì§ ? ì? ?ëŠ” ?…ë°?´íŠ¸)
                    // ?¬ê¸°?œëŠ” êµ³ì´ ê±´ë“œë¦¬ì? ?Šì•„???˜ì?ë§? ?•ì‹¤?˜ê²Œ ?˜ë ¤ë©??…ë°?´íŠ¸
                }
                btn.colors = colors;

                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(action);
            }
        }
        
        // ?ˆì „?¥ì¹˜
        Button buttonComponent = btnObj.GetComponent<Button>();
        if (buttonComponent != null)
        {
            buttonComponent.onClick.RemoveAllListeners();
            buttonComponent.onClick.AddListener(action);
        }
    }

    private void Create3DBackground()
    {
        // ê¸°ì¡´???ì„±??3D ë°°ê²½???ˆë‹¤ë©??? œ (?™êµ ëª¨ë¸???£ê¸° ?„í•´ ë¹„ì›Œ??
        GameObject oldBg = GameObject.Find("Background3D");
        if (oldBg != null)
        {
            DestroyImmediate(oldBg);
        }
        
        // ?¬ê¸°??school.fbxë¥?ë°°ì¹˜?˜ì‹œë©??©ë‹ˆ??
    }

    private GameObject FindChild(GameObject parent, string name)
    {
        Transform t = parent.transform.Find(name);
        if (t != null) return t.gameObject;
        return null;
    }

#if UNITY_EDITOR
    private void MakeTextureReadable(Sprite sprite)
    {
        if (sprite == null) return;
        try
        {
            string path = UnityEditor.AssetDatabase.GetAssetPath(sprite.texture);
            UnityEditor.TextureImporter importer = UnityEditor.AssetImporter.GetAtPath(path) as UnityEditor.TextureImporter;
            if (importer != null && !importer.isReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
                Debug.Log($"[LobbyUIBuilder] Automatically enabled Read/Write for {sprite.name}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to set Read/Write: {e.Message}");
        }
    }
#endif
}
