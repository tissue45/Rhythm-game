using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(LobbyManager))]
[ExecuteAlways] // ?韒�?域�?嶅� ?欠�謔踫䂻穈� ?嵸�穈�窶???
public class LobbyUIBuilder : MonoBehaviour
{
    [Header("Custom UI Images")]
    public Sprite borderSprite; // 窶嵸� ?𣕑�謔?(?��?�尐諰?赬��?韠�??
    public Sprite titleLogoSprite; // ?�?渣? 諢𨁈� ?渠?鴔�
    public Sprite startButtonSprite; // 窶嵸� ?𨰰� 貒�𢩦 ?渠?鴔�
    public Sprite quitButtonSprite; // 窶嵸� 鮈�� 貒�𢩦 ?渠?鴔�

    [Header("UI Settings")]
    public Vector2 logoSize = new Vector2(1100, 1100); // 諢𨁈� ?禹萼
    public Vector2 startButtonSize = new Vector2(1000, 1000); // ?𨰰� 貒�𢩦 ?禹萼
    public Vector2 quitButtonSize = new Vector2(1000, 1000); // 鮈�� 貒�𢩦 ?禹萼
    
    [Header("UI Positions")]
    public Vector2 logoPosition = Vector2.zero; // 諢𨁈� ?��
    public Vector2 startButtonPosition = new Vector2(0, -170); // ?𨰰� 貒�𢩦 ?��
    public Vector2 quitButtonPosition = new Vector2(0, -400); // 鮈�� 貒�𢩦 ?��

    private bool _isDirty = false;

    private void Start()
    {
        if (Application.isPlaying)
        {
            // NetworkManager 생성
            if (Object.FindFirstObjectByType<NetworkManager>() == null)
            {
                new GameObject("NetworkManager").AddComponent<NetworkManager>();
            }

            // [DISABLED] 모든 UI 조작 비활성화 - FixAllButtons가 처리함
            // BuildUI();
            // SetGameReady();
            // StartButton, QuitButton 숨김 로직 제거됨

            // 네트워크 이벤트
            NetworkManager net = Object.FindFirstObjectByType<NetworkManager>();
            if (net != null)
            {
                net.OnConnected += OnClientConnected;
            }
        }
    }

    private void OnClientConnected()
    {
        // ?國盒?䁪庖 諻竾� ?𨰰�?䁯? ?𢤹� 儦渥𠂔?賈𠹻???𨰰�
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
        
        // QR 儠竾 ?渠?鴔???刷萼篣?(篧竾?瞘)
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
            // ?渠? ?國盒?䁯𩸭 ?�尐諰?諻竾� ?𨰰�
            GetComponent<LobbyManager>().StartGame();
        }
        else
        {
            // ?國盒 ???䁯𩸭 ?�尐諰?QR 儠竾� ?刺� ?𨰰�
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
        // ?賄擪?軭�?韠� 穈𨩆𦚯 諻竾�𣕑庖 穈桿� ?�烄
        _isDirty = true;
    }

    private void Update()
    {
        // ?韒�??諈刺�???𣕑�, 穈𨩆𦚯 諻竾�嵸�?潺庖 UI ?木� 篞賈收篣?
        if (!Application.isPlaying && _isDirty)
        {
            _isDirty = false;
            BuildUI();
        }
    }

    [ContextMenu("Build Lobby UI")]
    public void BuildUI()
    {
        // 1. Canvas 麆樽萼 ?韒� ?吖�
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

        // 1.5 EventSystem 麆樽萼 ?韒� ?吖� (UI ?渠早 ?��?䇹�)
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // 2. 諻國祭 (Panel)
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

        // [NEW] 窶嵸� ?𣕑�謔?(Border)
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

        // [NEW] 3D 諻國祭 ?吖�
        Create3DBackground();

#if UNITY_EDITOR
        MakeTextureReadable(startButtonSprite);
        MakeTextureReadable(quitButtonSprite);
#endif

        // 3. ?�?渣? (Text or Logo Image)
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
            // ?科𦚯鴞??木� (?科鹻??鴔�??
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

        // [MODIFIED] ?𥔱? ?䇹痍?潺� ?韒� ?吖� 赬��?桶�
        // [NEW] ?國盒 ?�篣??竾庖 (QR 儠竾�) -> ?韒� ?吖� ??
        // CreateConnectionPanel(canvas.transform);

        // 4. ?𨰰� 貒�𢩦 (?渠?鴔� 鴔�?? ?科𦚯鴞??�� 魽域� 穈�?? -> ?韒� ?吖� ??
        /*
        CreateButton(canvas.transform, "StartButton", "START GAME", startButtonPosition, new Color(0.2f, 0.2f, 0.2f), new Color(0f, 1f, 0.5f), startButtonSprite, startButtonSize, () => {
            OnStartButtonClicked();
        });
        */

        // 5. 鮈�� 貒�𢩦 (?渠?鴔� 鴔�?? ?科𦚯鴞??�� 魽域� 穈�?? -> ?韒� ?吖� ??
        /*
        CreateButton(canvas.transform, "QuitButton", "EXIT", quitButtonPosition, new Color(0.2f, 0.2f, 0.2f), new Color(1f, 0.2f, 0.5f), quitButtonSprite, quitButtonSize, () => {
            GetComponent<LobbyManager>().QuitGame();
        });
        */
        
        // 儦渠�???𣕑�?軤擪 ?刷頃
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
            
            // 1. ?�眼 ?竾庖 諻國祭 (?渠�??諻属�諈?
            Image img = panelObj.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.9f);
            
            RectTransform rt = panelObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // 2. 儦渠� 諻國祭 (鴗𡢾� ?瑅䁥)
            GameObject cardObj = new GameObject("CardBackground");
            cardObj.transform.SetParent(panelObj.transform, false);
            Image cardImg = cardObj.AddComponent<Image>();
            cardImg.color = new Color(0.15f, 0.15f, 0.2f, 1f); // ?欠� 賳竾ㄗ 篞賈�??
            
            // ?伉滂 諈到�謔??刷頃 (Outline 儢渣𡢢?龲䂻諢??�麮渣�穇圉� ?欠�?潰𦚯???��, ?禹萼???吣�諤?
            if (borderSprite != null) cardImg.sprite = borderSprite; // ?𣕑�謔??欠�?潰𦚯???秒�??穈�?伕�諰??科鹻

            RectTransform cardRt = cardObj.GetComponent<RectTransform>();
            cardRt.anchorMin = new Vector2(0.5f, 0.5f);
            cardRt.anchorMax = new Vector2(0.5f, 0.5f);
            cardRt.sizeDelta = new Vector2(800, 900); // 儦渠� ?禹萼
            cardRt.anchoredPosition = Vector2.zero;

            // 3. QR 儠竾� ?渠?鴔�
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

            // 4. ?�� ?𣽁擪??
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

            // 5. ?嶅� ?𣽁擪??
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

            // 6. 鼒到� 貒�𢩦
            CreateButton(cardObj.transform, "CancelButton", "CANCEL", new Vector2(0, -380), new Color(0.3f, 0.3f, 0.3f), Color.white, null, new Vector2(300, 80), () => {
                SetGameReady(true); // ?木� 諰䇹𥘵?潺�
            });
        }
        else
        {
            // ?渠? 魽渥�?䁪庖 QR 儠竾� ?渠?鴔�諤?穈桿� ?嶅�
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
            
            // ?渠?鴔�穈� ?�尐諰??渠?鴔� ?科鹻, ?�尐諰??吣� ?科鹻
            if (sprite != null)
            {
                img.sprite = sprite;
                img.color = Color.white; // ?渠?鴔�穈� ?�尐諰??域�(?韒雩??
                img.preserveAspect = true;
                img.alphaHitTestMinimumThreshold = 0.1f; // [FIX] ?禺�??賱�賱??渠早 諡渥�
            }
            else
            {
                img.color = bgColor;
            }

            Button btn = btnObj.AddComponent<Button>();

            // ?𣽁擪??(?渠?鴔�穈� ?�� ?𣕑� ?吖�)
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
                // ?渠?鴔�穈� ?��???𣽁擪???月�?𠺝䂻穈� ?到�?�尐諰???� (篧竾�?瞘�)
                if (txtObj != null) DestroyImmediate(txtObj);
            }

            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f); 
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size; // ?科鹻??鴔�???科𦚯鴞??�鹻
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
            // ?渠? 魽渥�?????�㫲?渣䂻 諢𨰰�
            Image img = btnObj.GetComponent<Image>();
            GameObject txtObj = FindChild(btnObj, "Text");

            if (sprite != null)
            {
                img.sprite = sprite;
                img.color = Color.white;
                img.preserveAspect = true;
                img.alphaHitTestMinimumThreshold = 0.1f; // [FIX] ?禺�??賱�賱??渠早 諡渥�
                if (txtObj != null) DestroyImmediate(txtObj);
            }
            
            // ?科𦚯鴞??�㫲?渣䂻
            RectTransform rt = btnObj.GetComponent<RectTransform>();
            if (rt != null) 
            {
                rt.sizeDelta = size;
                rt.anchoredPosition = position; // [FIX] ?��??穈軤𦚯 ?�㫲?渣䂻
            }

            // 篞賈汝???��?潰𥘵 ?𨁈掠 (篧竾�?瞘�)
            Shadow shadow = btnObj.GetComponent<Shadow>();
            if (shadow != null) DestroyImmediate(shadow);
            Outline outline = btnObj.GetComponent<Outline>();
            if (outline != null) DestroyImmediate(outline);
            
            // ?堅???謔科擪???科㜊窶?
            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                // [FIX] 貒�𢩦 ?吣� ?��??穈軤𦚯 ?�㫲?渣䂻?渥焩 ??
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
                    // ?渠?鴔�穈� ?�尐諰?諻國祭???科鹻 (篣域● 諢𨰰� ?𥔱? ?韒� ?�㫲?渣䂻)
                    // ?禹萼?嶅� 窱喬𦚯 穇渠�謔科? ?𥇣�???䁯?諤? ?㻂𠹻?瞘� ?䁪𨸹諰??�㫲?渣䂻
                }
                btn.colors = colors;

                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(action);
            }
        }
        
        // ?��?伊�
        Button buttonComponent = btnObj.GetComponent<Button>();
        if (buttonComponent != null)
        {
            buttonComponent.onClick.RemoveAllListeners();
            buttonComponent.onClick.AddListener(action);
        }
    }

    private void Create3DBackground()
    {
        // 篣域●???吖�??3D 諻國祭???�𠹻諰???� (?軎� 諈刺桊???�萼 ?�㟲 赬��??
        GameObject oldBg = GameObject.Find("Background3D");
        if (oldBg != null)
        {
            DestroyImmediate(oldBg);
        }
        
        // ?禹萼??school.fbx諝?諻域�?䁯�諰??拘�??
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
