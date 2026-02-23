using System;
using System.Net;
using UnityEngine;

public class RoomChatBootstrap : MonoBehaviour
{
    [SerializeField] SupabaseRest supa;
    [SerializeField] HostChatServer server;
    [SerializeField] HostChatClient client;

    [Header("설정")]
    [SerializeField] int defaultPort = 7777;

    async void Start()
    {
        if (string.IsNullOrEmpty(Session.CurrentRoomId))
        {
            Debug.LogError("RoomId 없음");
            return;
        }

        if (Session.IsHost)
        {
            string ip = !string.IsNullOrEmpty(Session.HostIp) ? Session.HostIp : (GetLocalIPv4() ?? "127.0.0.1");
            int port = (Session.HostPort > 0) ? Session.HostPort : defaultPort;

            Session.HostIp = ip;
            Session.HostPort = port;

            server.StartServer(port, ip);
            client.Connect(ip, port, LocalProfile.Name);
            return;
        }

        try
        {
            string json = await supa.GetRoomByIdRaw(Session.CurrentRoomId);
            var room = ParseSingleRoom(json);

            Session.HostIp = room.host_ip;
            Session.HostPort = room.host_port;

            if (string.IsNullOrEmpty(Session.HostIp) || Session.HostPort <= 0)
            {
                Debug.LogError("방 host_ip/host_port가 비어있음. 방 생성 시 rooms에 저장됐는지 확인하셈");
                return;
            }

            client.Connect(Session.HostIp, Session.HostPort, LocalProfile.Name);
        }
        catch (Exception e)
        {
            Debug.LogError("방 정보 조회/접속 실패: " + e.Message);
        }
    }

    [Serializable]
    class RoomInfo { public string host_ip; public int host_port; }

    static RoomInfo ParseSingleRoom(string jsonArray)
    {
        string wrapped = "{\"items\":" + jsonArray + "}";
        var w = JsonUtility.FromJson<Wrap<RoomInfo>>(wrapped);
        return w.items[0];
    }
    [Serializable] class Wrap<T> { public System.Collections.Generic.List<T> items; }

    static string GetLocalIPv4()
    {
        try
        {
            foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    return ip.ToString();
            }
        }
        catch { }
        return null;
    }
}