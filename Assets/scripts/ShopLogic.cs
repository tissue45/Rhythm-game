using UnityEngine;
using UnityEngine.UI;

public class ShopLogic : MonoBehaviour
{
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
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    public void OnChargeClick()
    {
        // 웹 결제 페이지 열기
        string url = "http://localhost:5173/payment";
        Application.OpenURL(url);
        Debug.Log($"[Shop] Opening Payment URL: {url}");
    }
}
