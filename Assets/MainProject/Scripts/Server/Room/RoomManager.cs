using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RoomPlayerRow
{
    public string player_id;
    public string player_name;
    public string joined_at;
}

public class RoomManager : MonoBehaviour
{
    [Header("기본 참조")]
    [SerializeField] SupabaseRest supa;

    [Header("플레이어 목록 참조")]
    [SerializeField] Transform playerListContent;
    [SerializeField] GameObject playerItemPrefab;

    [Header("채팅 참조")]
    [SerializeField] ChatLogTMP chatLog;

    [Header("Scene")]
    [SerializeField] string PreviousSceneName;

    [Header("플레이어 목록 표시 딜레이")]
    [SerializeField] float pollInterval;

    readonly Dictionary<string, RoomPlayerRow> current = new();
    readonly List<GameObject> spawnedPlayerItems = new();

    Coroutine pollCo;

    void Start()
    {
        if (string.IsNullOrEmpty(Session.CurrentRoomId))
        {
            Debug.LogError("Session.CurrentRoomId가 비어있음. 로비에서 방 입장 후 Room 씬으로 와야 함.");
            return;
        }

        pollCo = StartCoroutine(PollPlayers());
    }

    void OnDestroy()
    {
        if (pollCo != null) StopCoroutine(pollCo);
    }

    IEnumerator PollPlayers()
    {
        yield return RefreshPlayersOnce();

        while (true)
        {
            yield return new WaitForSeconds(pollInterval);
            yield return RefreshPlayersOnce();
        }
    }

    IEnumerator RefreshPlayersOnce()
    {
        var task = supa.GetRoomPlayersRaw(Session.CurrentRoomId);
        while (!task.IsCompleted) yield return null;

        if (task.IsFaulted)
        {
            Debug.LogError(task.Exception);
            yield break;
        }

        string json = task.Result;
        var list = JsonArray<RoomPlayerRow>(json) ?? new List<RoomPlayerRow>();

        var next = new Dictionary<string, RoomPlayerRow>();
        foreach (var p in list)
        {
            if (string.IsNullOrEmpty(p.player_id)) continue;
            next[p.player_id] = p;
        }

        foreach (var kv in next)
        {
            if (!current.ContainsKey(kv.Key))
                AddSystemLog($"{kv.Value.player_name}님이 들어왔습니다.");
        }

        foreach (var kv in current)
        {
            if (!next.ContainsKey(kv.Key))
                AddSystemLog($"{kv.Value.player_name}님이 나가셨습니다.");
        }

        current.Clear();
        foreach (var kv in next) current[kv.Key] = kv.Value;

        RebuildPlayerListUI();
    }

    void RebuildPlayerListUI()
    {
        for (int i = 0; i < spawnedPlayerItems.Count; i++)
            Destroy(spawnedPlayerItems[i]);
        spawnedPlayerItems.Clear();

        var ordered = new List<RoomPlayerRow>(current.Values);
        ordered.Sort((a, b) => string.CompareOrdinal(a.joined_at, b.joined_at));

        foreach (var p in ordered)
        {
            var go = Instantiate(playerItemPrefab, playerListContent);
            spawnedPlayerItems.Add(go);

            var ui = go.GetComponent<PlayerListItemUI>();
            if (ui) ui.SetName(p.player_name);
        }
    }

    void AddSystemLog(string msg)
    {
        chatLog.AddSystem(msg);
    }

    public void OnClickLeave()
    {
        _ = LeaveAndGoLobby();
    }

    async System.Threading.Tasks.Task LeaveAndGoLobby()
    {
        if (pollCo != null) StopCoroutine(pollCo);
        pollCo = null;

        try
        {
            await supa.LeaveRoomRaw(Session.CurrentRoomId, PlayerIdentity.PlayerId);
        }
        catch (Exception e)
        {
            Debug.LogError("LeaveRoom 실패: " + e.Message);
        }

        Session.CurrentRoomId = null;
        Session.CurrentRoomName = null;

        SceneTransitionController.Instance.LoadScene(PreviousSceneName);
    }

    static List<T> JsonArray<T>(string json)
    {
        string wrapped = "{\"items\":" + json + "}";
        var w = JsonUtility.FromJson<Wrap<T>>(wrapped);
        return w != null ? w.items : null;
    }

    [Serializable] class Wrap<T> { public List<T> items; }
}