using UnityEngine;
using WeZard.RelayProtocol;

public class RoomChatBootstrap : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] ChatLogTMP chatLog;

    async void Start()
    {
        if (string.IsNullOrEmpty(Session.CurrentRoomId))
        {
            Debug.LogError("RoomId 없음");
            return;
        }

        var relay = RelayChatClient.Instance;
        if (relay == null)
        {
           return;
        }

        relay.OnSystem -= OnSys;
        relay.OnChat   -= OnChat;
        relay.OnError  -= OnErr;

        relay.OnSystem += OnSys;
        relay.OnChat   += OnChat;
        relay.OnError  += OnErr;

        bool ok = await relay.ConnectAsync();
        if (!ok) return;

        if (Session.IsHost)
            await relay.CreateRoomAsync(Session.CurrentRoomId, LocalProfile.Id, LocalProfile.Name);

        await relay.JoinRoomAsync(Session.CurrentRoomId, LocalProfile.Id, LocalProfile.Name);
    }

    void OnDestroy()
    {
        var relay = RelayChatClient.Instance;
        if (relay == null) return;

        relay.OnSystem -= OnSys;
        relay.OnChat   -= OnChat;
        relay.OnError  -= OnErr;
    }

    void OnSys(SysMsg m) => chatLog?.AddSystem(m.message);
    void OnChat(ChatMsg m) => chatLog?.AddChat(m.playerName, m.message);
    void OnErr(ErrMsg e) => chatLog?.AddSystem($"<color=#FF6666>{e.code}: {e.message}</color>");
}