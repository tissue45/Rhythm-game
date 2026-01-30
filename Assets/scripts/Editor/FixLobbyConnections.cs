using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class FixLobbyConnections : EditorWindow
{
    [MenuItem("Tools/Fix Rhythm Game/Fix Lobby Connections (Final)")]
    public static void FixConnections()
    {
        // 1. 매니저 찾기
        SchoolLobbyManager lobbyManager = FindObjectOfType<SchoolLobbyManager>();
        if (lobbyManager == null)
        {
            Debug.LogError("SchoolLobbyManager를 찾을 수 없습니다!");
            return;
        }

        Canvas canvas = FindObjectOfType<Canvas>();
        
        // 2. 예쁜 상점 찾기
        Transform beautifulShop = canvas.transform.Find("StepUpShopPanel");
        if (beautifulShop == null)
        {
            EditorUtility.DisplayDialog("Error", "'StepUpShopPanel'이 없습니다.\n먼저 'Create Beautiful Shop UI'를 실행해주세요.", "OK");
            return;
        }

        // 3. 구린 상점 제거
        Transform oldShop = canvas.transform.Find("ShopPanel");
        if (oldShop != null && oldShop != beautifulShop)
        {
            DestroyImmediate(oldShop.gameObject);
            Debug.Log("구형 ShopPanel 삭제 완료");
        }
        
        // 또 다른 구형 상점 이름들 확인
        Transform premiumShop = canvas.transform.Find("PremiumShopPanel");
        if (premiumShop != null) DestroyImmediate(premiumShop.gameObject);

        // 4. 매니저에 연결
        // 리플렉션으로 private/public 변수 상관없이 shopPanel 연결 시도
        SerializedObject so = new SerializedObject(lobbyManager);
        SerializedProperty shopProp = so.FindProperty("shopPanel");
        if (shopProp != null)
        {
            shopProp.objectReferenceValue = beautifulShop.gameObject;
            so.ApplyModifiedProperties();
            Debug.Log("LobbyManager에 Beautiful Shop 연결 완료!");
        }
        else
        {
            // shopPanel 변수 이름이 다를 경우를 대비해 수동 할당 (public이라고 가정)
            lobbyManager.shopPanel = beautifulShop.gameObject;
            EditorUtility.SetDirty(lobbyManager);
        }

        // 닫기 버튼 연결 (StepUpShopPanel 안의 CloseButton)
        Transform closeBtnTrans = beautifulShop.Find("MainPanel/CloseButton");
        if (closeBtnTrans != null)
        {
            Button closeBtn = closeBtnTrans.GetComponent<Button>();
            // 버튼 이벤트는 런타임에 처리되거나 매니저가 할당해야 함.
            // 여기서는 매니저가 shopPanel.SetActive(false)를 하도록 유도.
            // 에디터에서 OnClick 연결은 복잡하므로, ShopPanel 자체에 스크립트 추가 고려.
        }

        // 5. 로그인 정보창(UserInfoPanel) 복구
        Transform userInfo = canvas.transform.Find("UserInfoPanel");
        if (userInfo != null)
        {
            userInfo.gameObject.SetActive(true); // 일단 켜서 보이게 함
            
            // 위치 초기화 (화면 밖으로 나갔을 수도 있음)
            RectTransform rt = userInfo.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = new Vector2(180, -30); // LOGIN 버튼 옆
                rt.anchorMin = new Vector2(0, 1); // 좌상단
                rt.anchorMax = new Vector2(0, 1);
            }
            Debug.Log("UserInfoPanel 위치 복구 완료");
        }
        else
        {
            Debug.LogWarning("UserInfoPanel을 찾을 수 없습니다. 'Fix All Buttons'를 다시 실행해보세요.");
        }

        // 6. 뷰티풀 샵도 닫기 버튼을 누르면 꺼지게 세팅 (간단한 스크립트 부착)
        var closeHandler = beautifulShop.GetComponent<CloseOpenAction>();
        if (closeHandler == null) closeHandler = beautifulShop.gameObject.AddComponent<CloseOpenAction>();
        
        // 닫기 버튼에 기능 할당
        if (closeBtnTrans != null)
        {
            Button closeBtn = closeBtnTrans.GetComponent<Button>();
            // UnityEvent 연결은 런타임 전에는 까다로움. 
            // 대신 CloseOpenAction이 스스로 찾아서 처리하도록 함.
        }

        Debug.Log("=== 모든 연결 복구 완료! ===");
        EditorUtility.DisplayDialog("Success", "상점과 로그인 창을 올바르게 연결했습니다!\n이제 Play를 눌러 확인해보세요.", "OK");
    }
}

// 닫기 버튼 처리를 위한 간단한 헬퍼
public class CloseOpenAction : MonoBehaviour
{
    void Start()
    {
        Transform closeBtn = transform.Find("MainPanel/CloseButton");
        if (closeBtn != null)
        {
            closeBtn.GetComponent<Button>().onClick.AddListener(() => gameObject.SetActive(false));
        }
    }
}
