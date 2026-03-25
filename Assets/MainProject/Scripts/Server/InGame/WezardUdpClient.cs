using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class WezardUdpClient : MonoBehaviour
{
    public static WezardUdpClient Instance { get; private set; }

    private UdpClient udp;
    private IPEndPoint serverEp;
    private CancellationTokenSource cts;

    private readonly ConcurrentQueue<Action> mainThread = new();

    private int currentTick = -1;
    private int expectedParts = 0;
    private int receivedParts = 0;
    private string snapRoom;
    private readonly ConcurrentDictionary<string, object> snapPlayers = new();
    private readonly ConcurrentDictionary<string, object> snapEnemies = new();
    private readonly ConcurrentDictionary<string, object> snapOrbs = new();

    public event Action<string> OnSnapJsonPart;
    public event Action<string> OnEventJson;

    void Awake()
    {
        if (Instance != null) 
        { 
            Destroy(gameObject); 
            return; 
        }
        
        Instance = this;
    }

    public void StartUdp()
    {
        if (udp != null) return;

        var ips = Dns.GetHostAddresses(NetMode.UdpHost);
        serverEp = new IPEndPoint(ips[0], NetMode.UdpPort);

        udp = new UdpClient(0);
        udp.Client.ReceiveBufferSize = 1 << 20;
        udp.Client.SendBufferSize = 1 << 20;

        cts = new CancellationTokenSource();
        _ = ReceiveLoop(cts.Token);

        SendHello();
    }

    public void StopUdp()
    {
        try { cts?.Cancel(); } catch {}
        try { udp?.Close(); } catch {}
        cts = null;
        udp = null;
    }

    void Update()
    {
        while (mainThread.TryDequeue(out var a))
            a?.Invoke();
    }

    void SendHello()
    {
        var json = $"{{\"t\":\"hello\",\"room\":\"{NetMode.RoomId}\",\"pid\":\"{NetMode.PlayerId}\",\"name\":\"{Escape(NetMode.PlayerName)}\"}}";
        SendRaw(json);
    }

    public void SendPos(Vector3 pos, float yaw)
    {
        var json = $"{{\"t\":\"pos\",\"room\":\"{NetMode.RoomId}\",\"pid\":\"{NetMode.PlayerId}\"," +
                   $"\"x\":{pos.x:F3},\"y\":{pos.y:F3},\"z\":{pos.z:F3},\"yaw\":{yaw:F2}}}";
        SendRaw(json);
    }

    public void SendHit(string enemyId, float dmg)
    {
        var json = $"{{\"t\":\"hit\",\"room\":\"{NetMode.RoomId}\",\"pid\":\"{NetMode.PlayerId}\",\"eid\":\"{enemyId}\",\"dmg\":{dmg:F3}}}";
        SendRaw(json);
    }

    public void SendPickup(string orbId)
    {
        var json = $"{{\"t\":\"pickup\",\"room\":\"{NetMode.RoomId}\",\"pid\":\"{NetMode.PlayerId}\",\"oid\":\"{orbId}\"}}";
        SendRaw(json);
    }

    void SendRaw(string json)
    {
        if (udp == null) return;
        var bytes = Encoding.UTF8.GetBytes(json);
        udp.Send(bytes, bytes.Length, serverEp);
    }

    async Task ReceiveLoop(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var r = await udp.ReceiveAsync();
                var text = Encoding.UTF8.GetString(r.Buffer);

                if (text.Contains("\"t\":\"snap\""))
                {
                    var copy = text;
                    mainThread.Enqueue(() => OnSnapJsonPart?.Invoke(copy));
                }
                else if (text.Contains("\"t\":\"evt\""))
                {
                    var copy = text;
                    mainThread.Enqueue(() => OnEventJson?.Invoke(copy));
                }
            }
        }
        catch { }
    }

    static string Escape(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");
}