using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class QRDisplay : MonoBehaviour
{
    public RawImage qrImage;
    
    [Header("QR Settings")]
    public string controllerUrl = "";
    public int qrSize = 350;
    
    [Header("Deployment Settings")]
    [Tooltip("배포된 프론트엔드 URL (예: https://stepup-rhythm.vercel.app)")]
    public string deployedFrontendUrl = "https://stepup-rhythm.vercel.app";
    
    [Tooltip("로컬 개발 모드 사용 여부")]
    public bool useLocalDevelopment = false;
    
    private Texture2D currentQRTexture;
    
    void Start()
    {
        if (qrImage == null)
        {
            qrImage = GetComponent<RawImage>();
        }
        
        // 자동으로 URL이 설정되지 않았으면 기본값 사용
        if (string.IsNullOrEmpty(controllerUrl))
        {
            controllerUrl = GetControllerUrl();
        }
        
        GenerateQRCode(controllerUrl);
    }
    
    /// <summary>
    /// 환경에 따른 컨트롤러 URL 반환
    /// </summary>
    public string GetControllerUrl()
    {
        if (useLocalDevelopment)
        {
            // 로컬 개발 환경: 로컬 IP 사용
            string localIP = GetLocalIPAddress();
            return $"http://{localIP}:5173/controller";
        }
        else
        {
            // 배포 환경: Vercel URL 사용
            return $"{deployedFrontendUrl}/controller";
        }
    }
    
    /// <summary>
    /// URL을 설정하고 QR 코드 생성
    /// </summary>
    public void SetUrlAndGenerate(string url)
    {
        controllerUrl = url;
        GenerateQRCode(url);
    }
    
    /// <summary>
    /// 동적으로 QR 코드 생성 (Google Charts API 사용)
    /// </summary>
    public void GenerateQRCode(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            Debug.LogWarning("[QRDisplay] Content is empty, cannot generate QR code.");
            return;
        }
        
        StartCoroutine(FetchQRCodeFromAPI(content));
    }
    
    /// <summary>
    /// QR 코드 이미지 API에서 가져오기 (goqr.me API 사용 - 무료, 안정적)
    /// </summary>
    private IEnumerator FetchQRCodeFromAPI(string content)
    {
        // URL 인코딩
        string encodedContent = UnityWebRequest.EscapeURL(content);
        
        // goqr.me API 사용 (무료, HTTPS, 안정적)
        string apiUrl = $"https://api.qrserver.com/v1/create-qr-code/?size={qrSize}x{qrSize}&data={encodedContent}&format=png";
        
        Debug.Log($"[QRDisplay] Generating QR for: {content}");
        Debug.Log($"[QRDisplay] API URL: {apiUrl}");
        
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(apiUrl))
        {
            yield return www.SendWebRequest();
            
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[QRDisplay] Failed to generate QR: {www.error}");
                
                // Fallback: Resources에서 정적 QR 이미지 로드
                LoadStaticQR();
            }
            else
            {
                // 성공적으로 QR 코드 이미지 생성
                currentQRTexture = DownloadHandlerTexture.GetContent(www);
                
                if (qrImage != null)
                {
                    qrImage.texture = currentQRTexture;
                    Debug.Log("[QRDisplay] QR Code generated successfully!");
                }
            }
        }
    }
    
    /// <summary>
    /// 정적 QR 코드 이미지 로드 (Fallback)
    /// </summary>
    private void LoadStaticQR()
    {
        if (qrImage == null)
        {
            qrImage = GetComponent<RawImage>();
        }

        if (qrImage != null)
        {
            Texture2D tex = Resources.Load<Texture2D>("qrcode");
            if (tex != null)
            {
                qrImage.texture = tex;
                Debug.Log("[QRDisplay] Loaded static QR code from Resources.");
            }
            else
            {
                Debug.LogWarning("[QRDisplay] QR Code texture not found in Resources/qrcode");
            }
        }
    }
    
    /// <summary>
    /// QR 코드 새로고침
    /// </summary>
    public void RefreshQR()
    {
        controllerUrl = GetControllerUrl();
        GenerateQRCode(controllerUrl);
    }
    
    /// <summary>
    /// 로컬 IP 주소 가져오기
    /// </summary>
    private string GetLocalIPAddress()
    {
        try
        {
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[QRDisplay] Failed to get IP: {e.Message}");
        }
        return "127.0.0.1";
    }
    
    void OnDestroy()
    {
        // 텍스처 정리
        if (currentQRTexture != null)
        {
            Destroy(currentQRTexture);
        }
    }
}
