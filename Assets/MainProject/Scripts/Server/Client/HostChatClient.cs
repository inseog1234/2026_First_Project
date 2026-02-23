using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class HostChatClient : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] TMP_InputField input;
    [SerializeField] ChatLogTMP chatLog;

    TcpClient tcp;
    CancellationTokenSource cts;
    readonly ConcurrentQueue<Action> mainThread = new();

    public bool IsConnected => tcp != null && tcp.Connected;

    void Awake()
    {
        if (input != null)
        {
            input.onEndEdit.RemoveListener(OnEndEditSubmit);
            input.onEndEdit.AddListener(OnEndEditSubmit);
        }
    }

    void OnEndEditSubmit(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        string msg = text.Trim();
        if (msg.Length == 0) return;

        input.text = "";
        input.ActivateInputField();

        SendChat(LocalProfile.Name, msg);
    }

    public async void Connect(string hostIp, int hostPort, string myName)
    {
        try
        {
            tcp = new TcpClient();
            await tcp.ConnectAsync(hostIp, hostPort);
            cts = new CancellationTokenSource();

            var join = new JoinReq { playerId = LocalProfile.Id, name = myName };
            await NetJsonRpc.WriteAsync(tcp.GetStream(), JsonUtility.ToJson(join), cts.Token);

            _ = ReceiveLoop(cts.Token);

            mainThread.Enqueue(() => chatLog?.AddSystem($"서버를 열었습니다.: {hostIp}:{hostPort}"));
        }
        catch (Exception e)
        {
            mainThread.Enqueue(() => chatLog?.AddSystem($"연결 실패: {e.Message}"));
            Debug.LogError(e);
        }
    }

    public async void SendChat(string myName, string message)
    {
        if (!IsConnected) return;
        if (string.IsNullOrWhiteSpace(message)) return;

        try
        {
            var chat = new ChatReq { name = myName, message = message };
            await NetJsonRpc.WriteAsync(tcp.GetStream(), JsonUtility.ToJson(chat), cts.Token);
        }
        catch (Exception e)
        {
            Debug.LogWarning("채팅 보내기 에러: " + e.Message);
        }
    }

    async Task ReceiveLoop(CancellationToken ct)
    {
        try
        {
            var stream = tcp.GetStream();
            while (!ct.IsCancellationRequested)
            {
                string json = await NetJsonRpc.ReadAsync(stream, ct);
                if (json == null) break;

                string type = NetJsonRpc.PeekType(json);
                if (type == "sys")
                {
                    var m = JsonUtility.FromJson<SysMsg>(json);

                    mainThread.Enqueue(() =>
                    {
                        if (m.playerId == LocalProfile.Id)
                        {
                            chatLog?.AddSystem("입장하였습니다.");
                        }
                        else
                        {
                            chatLog?.AddSystem(m.message);
                        }
                    });
                }
                else if (type == "chat")
                {
                    var m = JsonUtility.FromJson<ChatBroadcast>(json);
                    mainThread.Enqueue(() => chatLog?.AddChat(m.name, m.message));
                }
            }
        }
        catch (Exception e)
        {
            if (!ct.IsCancellationRequested)
                Debug.LogWarning("리시브 루프 에러: " + e.Message);
        }
        finally
        {
            mainThread.Enqueue(() => chatLog?.AddSystem("서버 연결 종료"));
            Disconnect();
        }
    }

    void Update()
    {
        while (mainThread.TryDequeue(out var a))
            a?.Invoke();
    }

    public void Disconnect()
    {
        try { cts?.Cancel(); } catch { }
        try { tcp?.Close(); } catch { }
        cts = null;
        tcp = null;
    }

    void OnDestroy()
    {
        if (input != null) input.onEndEdit.RemoveListener(OnEndEditSubmit);
        Disconnect();
    }
}