using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using WeZard.RelayProtocol;

public class RelayChatClient : MonoBehaviour
{
    public static RelayChatClient Instance { get; private set; }

    [Header("Relay")]
    [SerializeField] string relayHost = "mintcat.arheneos.com";
    [SerializeField] int relayPort = 7878;

    TcpClient tcp;
    NetworkStream stream;
    CancellationTokenSource cts;

    readonly ConcurrentQueue<Action> mainThread = new();

    // ok 응답 대기 (create_room / join_room)
    readonly Dictionary<string, TaskCompletionSource<OkMsg>> okWaiters = new();
    readonly object waiterLock = new();

    public bool IsConnected => tcp != null && tcp.Connected;

    public event Action<SysMsg> OnSystem;
    public event Action<ChatMsg> OnChat;
    public event Action<ErrMsg> OnError;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }

    public void Configure(string host, int port)
    {
        relayHost = host;
        relayPort = port;
    }

    public async Task<bool> ConnectAsync(int timeoutMs = 2000)
    {
        if (IsConnected) return true;

        try
        {
            tcp = new TcpClient();
            var task = tcp.ConnectAsync(relayHost, relayPort);
            if (await Task.WhenAny(task, Task.Delay(timeoutMs)) != task)
                throw new Exception("connect timeout");

            tcp.NoDelay = true;
            stream = tcp.GetStream();
            cts = new CancellationTokenSource();

            _ = ReceiveLoop(cts.Token);
            return true;
        }
        catch (Exception e)
        {
            Disconnect();
            mainThread.Enqueue(() => OnError?.Invoke(new ErrMsg { code="CONNECT_FAIL", message=e.Message }));
            return false;
        }
    }

    public void Disconnect()
    {
        try { cts?.Cancel(); } catch {}
        try { stream?.Close(); } catch {}
        try { tcp?.Close(); } catch {}
        cts = null;
        stream = null;
        tcp = null;

        lock (waiterLock)
        {
            foreach (var kv in okWaiters) kv.Value.TrySetCanceled();
            okWaiters.Clear();
        }
    }

    async Task ReceiveLoop(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                string json = await NetJsonRpc.ReadAsync(stream, ct);
                if (json == null) break;

                string type = NetJsonRpc.PeekType(json);

                if (type == "ok")
                {
                    var ok = JsonUtility.FromJson<OkMsg>(json);
                    TaskCompletionSource<OkMsg> tcs = null;

                    lock (waiterLock)
                    {
                        if (okWaiters.TryGetValue(ok.request, out tcs))
                            okWaiters.Remove(ok.request);
                    }

                    tcs?.TrySetResult(ok);
                }
                else if (type == "err")
                {
                    var err = JsonUtility.FromJson<ErrMsg>(json);
                    mainThread.Enqueue(() => OnError?.Invoke(err));
                }
                else if (type == "sys")
                {
                    var sys = JsonUtility.FromJson<SysMsg>(json);
                    mainThread.Enqueue(() => OnSystem?.Invoke(sys));
                }
                else if (type == "chat")
                {
                    var chat = JsonUtility.FromJson<ChatMsg>(json);
                    mainThread.Enqueue(() => OnChat?.Invoke(chat));
                }
            }
        }
        catch (Exception e)
        {
            if (!ct.IsCancellationRequested)
                mainThread.Enqueue(() => OnError?.Invoke(new ErrMsg { code="RECV_FAIL", message=e.Message }));
        }
        finally
        {
            mainThread.Enqueue(() => OnSystem?.Invoke(new SysMsg { message="서버 연결 종료" }));
            Disconnect();
        }
    }

    void Update()
    {
        while (mainThread.TryDequeue(out var a))
            a?.Invoke();
    }

    Task<OkMsg> WaitOk(string request, int timeoutMs = 2000)
    {
        var tcs = new TaskCompletionSource<OkMsg>(TaskCreationOptions.RunContinuationsAsynchronously);
        lock (waiterLock) okWaiters[request] = tcs;

        _ = Task.Run(async () =>
        {
            await Task.Delay(timeoutMs);
            lock (waiterLock)
            {
                if (okWaiters.TryGetValue(request, out var pending) && pending == tcs)
                {
                    okWaiters.Remove(request);
                    tcs.TrySetException(new TimeoutException($"wait ok timeout: {request}"));
                }
            }
        });

        return tcs.Task;
    }

    async Task SendJson(string json)
    {
        if (!IsConnected) throw new Exception("Not connected");
        await NetJsonRpc.WriteAsync(stream, json, cts.Token);
    }

    public async Task CreateRoomAsync(string roomId, string ownerId, string ownerName)
    {
        await SendJson(JsonUtility.ToJson(new CreateRoomReq { roomId=roomId, ownerId=ownerId, ownerName=ownerName }));
        await WaitOk("create_room");
    }

    public async Task JoinRoomAsync(string roomId, string playerId, string playerName)
    {
        await SendJson(JsonUtility.ToJson(new JoinRoomReq { roomId=roomId, playerId=playerId, playerName=playerName }));
        await WaitOk("join_room");
    }

    public async Task SendChatAsync(string roomId, string playerId, string message)
    {
        await SendJson(JsonUtility.ToJson(new WeZard.RelayProtocol.ChatReq { roomId=roomId, playerId=playerId, message=message }));
    }
}