using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Collections;
using System;

[System.Serializable]
public class SupabaseAuthResponse
{
    public string access_token;
    public string token_type;
    public int expires_in;
    public string refresh_token;
    public SupabaseUser user;
}

[System.Serializable]
public class SupabaseUser
{
    public string id;
    public string email;
}

[System.Serializable]
public class UserProfile
{
    public string id;
    public string email;
    public string name;
    public int coins;
    public string[] inventory;
}

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }

    [Header("Network Settings")]
    public int port = 7777;
    private UdpClient udpClient;
    private Thread receiveThread;
    private bool isRunning = false;

    // 메인 스레드에서 처리를 위한 큐
    private ConcurrentQueue<int> inputQueue = new ConcurrentQueue<int>();

    // 연결 이벤트
    public bool isConnected = false;
    public System.Action OnConnected;

    [Header("Supabase Settings")]
    [SerializeField] private string supabaseUrl = "https://zyqbuuovliissozugjfq.supabase.co";
    [SerializeField] private string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Inp5cWJ1dW92bGlpc3NvenVnamZxIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTQ2MzQ4MTgsImV4cCI6MjA3MDIxMDgxOH0.TYj-kGTlsGlznZCYX4M1yIilu0z1iNZ6tcWg5iLIaHE";

    public bool IsLoggedIn { get; private set; }
    public string CurrentUserId { get; private set; }
    public string CurrentUserEmail { get; private set; }
    private string accessToken;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null); // Ensure it's a root object
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        StartServer();
    }

    private void StartServer()
    {
        try
        {
            // 이미 실행 중이면 무시
            if (udpClient != null) return;

            udpClient = new UdpClient(port);
            // [FIX] 소켓 옵션 설정 (재사용 가능하도록)
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            
            isRunning = true;
            receiveThread = new Thread(new ThreadStart(ReceiveData));
            receiveThread.IsBackground = true;
            receiveThread.Start();
            Debug.Log($"UDP Server started on port {port}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to start UDP Server: {e.Message}");
        }
    }

    private void ReceiveData()
    {
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
        while (isRunning)
        {
            try
            {
                if (udpClient == null) break;

                byte[] data = udpClient.Receive(ref remoteEndPoint);
                string text = Encoding.UTF8.GetString(data);
                
                // [NEW] 연결 신호 처리
                if (text == "CONNECT")
                {
                    isConnected = true;
                    inputQueue.Enqueue(-1);
                    return;
                }

                // 1. JSON 파싱 시도 (Mobile Controller)
                if (text.StartsWith("{"))
                {
                    try
                    {
                        MobileInputData inputData = JsonUtility.FromJson<MobileInputData>(text);
                        if (inputData != null && inputData.type == "tap")
                        {
                            inputQueue.Enqueue(inputData.lane);
                        }
                    }
                    catch { /* JSON 파싱 실패 시 무시 */ }
                }
                // 2. 기존 단순 숫자 파싱 (Simple/Legacy)
                else if (int.TryParse(text, out int laneIndex))
                {
                    if (laneIndex >= 0 && laneIndex < 4)
                    {
                        inputQueue.Enqueue(laneIndex);
                    }
                }
            }
            catch (SocketException)
            {
                // 소켓 종료 시 발생할 수 있음
            }
            catch (ThreadAbortException)
            {
                // 스레드 종료 시 발생. 정상적인 동작이므로 무시.
            }
            catch (System.Exception e)
            {
                Debug.LogError($"UDP Receive Error: {e.Message}");
            }
        }
    }

    private void Update()
    {
        // [TEST] Unity 에디터 테스트용: Ctrl+L로 가짜 로그인
        #if UNITY_EDITOR
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.L))
        {
            TestLogin();
        }
        #endif

        // 큐에 쌓인 입력을 메인 스레드에서 처리
        while (inputQueue.TryDequeue(out int value))
        {
            if (value == -1)
            {
                // 연결 이벤트 발생
                Debug.Log("[NetworkManager] Client Connected!");
                OnConnected?.Invoke();
            }
            else
            {
                if (InputManager.Instance != null)
                {
                    InputManager.Instance.TriggerLane(value);
                }
            }
        }
    }

    /// <summary>
    /// [DEV] 테스트용 로그인 (에디터 전용)
    /// </summary>
    public void TestLogin()
    {
        Debug.Log("[NetworkManager] ✅ TEST LOGIN ACTIVATED (Editor Only)");
        CurrentUserId = "test-user-" + UnityEngine.Random.Range(1000, 9999);
        CurrentUserEmail = "test@example.com";
        IsLoggedIn = true;
        accessToken = "test-token-" + System.Guid.NewGuid().ToString();
        
        // Trigger UI update
        OnConnected?.Invoke();
    }

    private void OnDestroy()
    {
        StopServer();
    }

    private void OnApplicationQuit()
    {
        StopServer();
    }

    private void StopServer()
    {
        isRunning = false;
        if (udpClient != null)
        {
            try { udpClient.Close(); } catch {}
            udpClient = null;
        }
        if (receiveThread != null && receiveThread.IsAlive)
        {
            try { receiveThread.Abort(); } catch {}
        }
    }

    [System.Serializable]
    public class MobileInputData
    {
        public string type; // "tap", "tilt", "shake"
        public int lane;
        public long timestamp;
    }

    // [ABUSE_FIX] Allow React to inject login session
    public void ReceiveReactLogin(string json)
    {
        Debug.Log($"[Network] Received Login from React: {json}");
        try
        {
            // json = { "id": "...", "email": "...", "name": "..." }
            var data = JsonUtility.FromJson<UserProfile>(json);
            if (data != null)
            {
                CurrentUserId = data.id;
                CurrentUserEmail = data.email;
                IsLoggedIn = true;
                
                // Trigger event
                // We might need to manually set the profile if not fetching
                OnConnected?.Invoke();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[Network] React Login Parse Error: {e.Message}");
        }
    }

    // --- Supabase Integration ---

    public void SignUp(string email, string password, Action<bool, string> callback)
    {
        StartCoroutine(AuthRequest("/auth/v1/signup", email, password, callback));
    }

    public void SignIn(string email, string password, Action<bool, string> callback)
    {
        StartCoroutine(AuthRequest("/auth/v1/token?grant_type=password", email, password, (success, message) => {
            if (success)
            {
                try {
                    var data = JsonUtility.FromJson<SupabaseAuthResponse>(message);
                    if (data != null && data.user != null) {
                        accessToken = data.access_token;
                        CurrentUserId = data.user.id;
                        CurrentUserEmail = data.user.email;
                        IsLoggedIn = true;
                        Debug.Log($"[Network] Logged in as {CurrentUserEmail}");
                        callback(true, "Login Successful");
                    } else {
                        callback(false, "Login Failed: Invalid Response");
                    }
                } catch (Exception e) {
                    callback(false, "Parse Error: " + e.Message);
                }
            }
            else
            {
                callback(false, message);
            }
        }));
    }

    [System.Serializable]
    public struct AuthPayload {
        public string email;
        public string password;
    }

    private IEnumerator AuthRequest(string endpoint, string email, string password, Action<bool, string> callback)
    {
        string url = supabaseUrl + endpoint;
        AuthPayload userData = new AuthPayload { email = email, password = password };
        string json = JsonUtility.ToJson(userData);

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("apikey", supabaseKey);

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Network] Auth Error: {www.error} : {www.downloadHandler.text}");
                callback(false, www.downloadHandler.text);
            }
            else
            {
                callback(true, www.downloadHandler.text);
            }
        }
    }

    public void GetRanking(Action<string> callback)
    {
        // Simple fetch from a 'ranking' table
        StartCoroutine(GetRequest("/rest/v1/ranking?select=*&order=score.desc&limit=10", callback));
    }

    // [NEW] Local Mock Rankings for Test Mode
    private List<RankingSubmission> mockRankings = new List<RankingSubmission>();

    // [NEW] Submit Score to Ranking
    public void SubmitScore(string songName, int score, int maxCombo, int perfect, int great, int bad, int miss, Action<bool> callback)
    {
        if (!IsLoggedIn)
        {
            Debug.LogWarning("[Network] Cannot submit score: Not logged in.");
            callback(false);
            return;
        }

        // Handle Test Login (Mock Success AND Storage)
        // Handle Test Login (Mock Success AND Storage)
        if (!string.IsNullOrEmpty(accessToken) && accessToken.StartsWith("test-token-"))
        {
            Debug.Log("[Network] Test Login detected. Storing mock score locally.");
            
            // Create mock submission
            var submission = new RankingSubmission
            {
                user_id = CurrentUserId,
                user_name = CurrentUserEmail.Split('@')[0], // Use display name
                song_name = songName,
                score = score,
                max_combo = maxCombo,
                perfect = perfect,
                great = great,
                bad = bad,
                miss = miss,
                created_at = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            };
            
            // Store locally
            mockRankings.Add(submission);
            
            // Simulate network delay
            StartCoroutine(MockSubmitDelay(callback));
            return;
        }

        // Real Submission
        GetUserProfile((profile) => {
            string userName = profile != null ? profile.name : CurrentUserEmail.Split('@')[0];
            
            RankingSubmission submission = new RankingSubmission
            {
                user_id = CurrentUserId,
                user_name = userName,
                song_name = songName,
                score = score,
                max_combo = maxCombo,
                perfect = perfect,
                great = great,
                bad = bad,
                miss = miss,
                created_at = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            };

            string json = JsonUtility.ToJson(submission);
            Debug.Log($"[Network] Submitting Score JSON: {json}");
            StartCoroutine(PostRequest("/rest/v1/ranking", json, callback));
        });
    }

    private IEnumerator MockSubmitDelay(Action<bool> callback)
    {
        yield return new WaitForSeconds(0.5f);
        Debug.Log("[Network] ✅ Mock Score stored & submitted successfully!");
        callback(true);
    }

    private IEnumerator PostRequest(string endpoint, string json, Action<bool> callback)
    {
        string url = supabaseUrl + endpoint;
        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("apikey", supabaseKey);
            // Changed to 'representation' to see the server response
            www.SetRequestHeader("Prefer", "return=representation");
            if (!string.IsNullOrEmpty(accessToken))
                www.SetRequestHeader("Authorization", "Bearer " + accessToken);

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Network] POST Error: {www.error} | Response: {www.downloadHandler.text}");
                callback(false);
            }
            else
            {
                Debug.Log($"[Network] Score submitted successfully! Response: {www.downloadHandler.text}");
                callback(true);
            }
        }
    }

    // [NEW] Get ranking filtered by song name (Merge Real + Mock)
    public void GetRankingBySong(string songName, Action<string> callback)
    {
        // 1. Fetch Real Data
        string endpoint = $"/rest/v1/ranking?select=*&order=score.desc&limit=10";
        if (!string.IsNullOrEmpty(songName) && songName != "all")
        {
            endpoint += $"&song_name=eq.{songName}";
        }

        StartCoroutine(GetRequest(endpoint, (realJson) => {
            Debug.Log($"[Network] Real Ranking JSON for {songName}: {realJson}");
            // 2. Merge with Mock Data if exists
            if (mockRankings.Count > 0)
            {
                List<RankingSubmission> mergedList = new List<RankingSubmission>();

                // Parse Real Data
                if (!string.IsNullOrEmpty(realJson) && realJson != "[]")
                {
                    try
                    {
                        string wrappedJson = "{ \"array\": " + realJson + "}";
                        Wrapper<RankingSubmission> wrapper = JsonUtility.FromJson<Wrapper<RankingSubmission>>(wrappedJson);
                        if (wrapper != null && wrapper.array != null)
                        {
                            mergedList.AddRange(wrapper.array);
                        }
                    }
                    catch (Exception e) { Debug.LogWarning($"[Network] Failed to parse real ranking for merging: {e.Message}"); }
                }

                // Append Mock Data (Filter by song)
                var filteredMock = mockRankings;
                if (!string.IsNullOrEmpty(songName) && songName != "all")
                {
                    filteredMock = mockRankings.FindAll(r => r.song_name == songName);
                }
                mergedList.AddRange(filteredMock);

                // Sort Descending by Score
                mergedList.Sort((a, b) => b.score.CompareTo(a.score));

                // Take Top 10
                if (mergedList.Count > 10) mergedList = mergedList.GetRange(0, 10);

                // Serialize back to JSON array
                string finalJson = "[";
                for (int i = 0; i < mergedList.Count; i++)
                {
                    finalJson += JsonUtility.ToJson(mergedList[i]);
                    if (i < mergedList.Count - 1) finalJson += ",";
                }
                finalJson += "]";

                callback(finalJson);
            }
            else
            {
                // No mock data, return real data as is
                callback(realJson);
            }
        }));
    }

    private IEnumerator GetRequest(string endpoint, Action<string> callback)
    {
        string url = supabaseUrl + endpoint;
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            www.SetRequestHeader("apikey", supabaseKey);
            if (!string.IsNullOrEmpty(accessToken))
                www.SetRequestHeader("Authorization", "Bearer " + accessToken);

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Network] GET Error: {www.error}");
                callback(null);
            }
            else
            {
                callback(www.downloadHandler.text);
            }
        }
    }

    public void GetUserProfile(Action<UserProfile> callback)
    {
        if (!IsLoggedIn)
        {
            Debug.LogError("[Network] Cannot get profile: Not logged in.");
            callback(null);
            return;
        }

        StartCoroutine(GetRequest($"/rest/v1/users?select=*&id=eq.{CurrentUserId}", (json) => {
            if (string.IsNullOrEmpty(json))
            {
                callback(null);
                return;
            }

            try
            {
                string cleanJson = json.Trim();
                if (cleanJson.StartsWith("[") && cleanJson.EndsWith("]"))
                {
                    cleanJson = cleanJson.Substring(1, cleanJson.Length - 2);
                }
                
                if (string.IsNullOrEmpty(cleanJson)) 
                {
                     callback(null); 
                     return;
                }

                UserProfile profile = JsonUtility.FromJson<UserProfile>(cleanJson);
                callback(profile);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Network] Profile Parse Error: {e.Message} / JSON: {json}");
                callback(null);
            }
        }));
    }

    public void UpdateProfileName(string name, Action<bool> callback)
    {
        if (!IsLoggedIn) return;
        string json = $"{{\"name\": \"{name}\"}}";
        StartCoroutine(PatchRequest($"/rest/v1/users?id=eq.{CurrentUserId}", json, callback));
    }

    public void UpdateUserShop(int newCoins, string[] newInventory, Action<bool> callback)
    {
        if (!IsLoggedIn) return;

        string invJson = "[";
        if (newInventory != null)
        {
            for(int i=0; i<newInventory.Length; i++)
            {
                invJson += $"\"{newInventory[i]}\"";
                if (i < newInventory.Length - 1) invJson += ",";
            }
        }
        invJson += "]";

        string json = $"{{\"coins\": {newCoins}, \"inventory\": {invJson}}}";
        
        StartCoroutine(PatchRequest($"/rest/v1/users?id=eq.{CurrentUserId}", json, callback));
    }

    private IEnumerator PatchRequest(string endpoint, string json, Action<bool> callback)
    {
        string url = supabaseUrl + endpoint;
        using (UnityWebRequest www = new UnityWebRequest(url, "PATCH"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("apikey", supabaseKey);
            if (!string.IsNullOrEmpty(accessToken))
                www.SetRequestHeader("Authorization", "Bearer " + accessToken);

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Network] PATCH Error: {www.error}");
                callback(false);
            }
            else
            {
                Debug.Log($"[Network] PATCH Success: {www.downloadHandler.text}");
                callback(true);
            }
        }
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] array;
    }

    [System.Serializable]
    private class RankingSubmission
    {
        public string user_id;
        public string user_name;
        public string song_name;
        public int score;
        public int max_combo;
        public int perfect;
        public int great;
        public int bad;
        public int miss;
        public string created_at;
    }
}
