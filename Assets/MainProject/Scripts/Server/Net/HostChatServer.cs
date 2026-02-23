using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class HostChatServer : MonoBehaviour
{
    TcpListener listener;
    CancellationTokenSource cts;

    readonly ConcurrentDictionary<long, TcpClient> clients = new();
    readonly ConcurrentDictionary<long, string> clientNames = new();
    readonly ConcurrentDictionary<long, string> clientPlayerIds = new();

    long nextId = 0;

    int listenPort;
    public bool IsRunning => listener != null;

    public void StartServer(int port, string advertisedIp = null)
    {
        if (IsRunning) return;

        listenPort = port;
        cts = new CancellationTokenSource();

        listener = new TcpListener(IPAddress.Any, listenPort);
        listener.Start();

        string ipInfo = string.IsNullOrEmpty(advertisedIp) ? "" : $" (연결됨 : {advertisedIp}:{listenPort})";
        Debug.Log($"[HOST] 서버 생성 : 0.0.0.0:{listenPort}{ipInfo}");

        _ = AcceptLoop(cts.Token);
    }

    public void StopServer()
    {
        try { cts?.Cancel(); } catch { }
        try { listener?.Stop(); } catch { }

        foreach (var kv in clients)
        {
            try { kv.Value.Close(); } catch { }
        }

        clients.Clear();
        clientNames.Clear();
        listener = null;
        cts = null;
    }

    async Task AcceptLoop(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var tcp = await listener.AcceptTcpClientAsync();
                long id = Interlocked.Increment(ref nextId);
                clients[id] = tcp;
                _ = HandleClient(id, tcp, ct);
            }
        }
        catch (Exception e)
        {
            if (!ct.IsCancellationRequested)
                Debug.LogError("[HOST] 확인 루프 에러: " + e.Message);
        }
    }

    async Task HandleClient(long id, TcpClient tcp, CancellationToken ct)
    {
        try
        {
            var stream = tcp.GetStream();

            while (!ct.IsCancellationRequested)
            {
                string json = await NetJsonRpc.ReadAsync(stream, ct);
                if (json == null) break;

                string type = NetJsonRpc.PeekType(json);
                if (type == "join")
                {
                    var join = JsonUtility.FromJson<JoinReq>(json);
                    clientNames[id] = join.name;
                    clientPlayerIds[id] = join.playerId;

                    await BroadcastSys(join.playerId, $"{join.name}님이 들어왔습니다.", ct);
                }
                else if (type == "chat")
                {
                    var chat = JsonUtility.FromJson<ChatReq>(json);
                    await BroadcastChat(chat.name, chat.message, ct);
                }
            }
        }
        catch (Exception e)
        {
            if (!ct.IsCancellationRequested)
                Debug.LogWarning("[HOST] 클라이언트 에러: " + e.Message);
        }
        finally
        {
            clients.TryRemove(id, out _);
            clientNames.TryRemove(id, out var name);
            clientPlayerIds.TryRemove(id, out var playerId);

            if (!string.IsNullOrEmpty(name))
            {
                _ = BroadcastSys(playerId, $"{name}님이 나가셨습니다.", CancellationToken.None);
            }

            try { tcp.Close(); } catch { }
        }
    }

    async Task BroadcastSys(string playerId, string message, CancellationToken ct)
    {
        var msg = new SysMsg { playerId = playerId, message = message };
        string json = JsonUtility.ToJson(msg);

        foreach (var kv in clients)
        {
            try { await NetJsonRpc.WriteAsync(kv.Value.GetStream(), json, ct); }
            catch { }
        }
    }

    async Task BroadcastChat(string name, string message, CancellationToken ct)
    {
        var msg = new ChatBroadcast { name = name, message = message };
        string json = JsonUtility.ToJson(msg);

        foreach (var kv in clients)
        {
            try { await NetJsonRpc.WriteAsync(kv.Value.GetStream(), json, ct); }
            catch { }
        }
    }

    void OnDestroy()
    {
        StopServer();
    }
}