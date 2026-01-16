using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;

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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
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
                    // 메인 스레드에서 이벤트 발생을 위해 큐에 특수 값(-1) 넣기
                    inputQueue.Enqueue(-1);
                    continue;
                }

                // 데이터 파싱 (예: "0", "1", "2", "3")
                if (int.TryParse(text, out int laneIndex))
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
}
