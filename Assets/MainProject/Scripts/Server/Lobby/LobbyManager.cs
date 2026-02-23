using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

[Serializable]
public class RoomRow
{
    public string id;
    public string name;
    public string owner_name;
    public int current_count;
    public long max_players;
    public string password_hash;
}

public class LobbyManager : MonoBehaviour
{
    [Header("기본 참조")]
    [SerializeField] SupabaseRest supa;
    [SerializeField] Transform serverListContent;
    [SerializeField] GameObject serverItemPrefab;

    [Header("입장 비밀번호 팝업 참조")]
    [SerializeField] GameObject joinRoomPopup;
    [SerializeField] TMP_InputField joinPasswordInput;

    [Header("방 생성 팝업")]
    [SerializeField] TMP_InputField createRoomName;
    [SerializeField] TMP_InputField createRoomPassword;
    [SerializeField] TMP_Text createRoomMaxCountText;
    [SerializeField] GameObject createRoomPopup;

    [Header("씬 이름")]
    [SerializeField] string NextSceneName;

    string pendingJoinRoomId;
    string pendingJoinRoomName;
    long createMaxPlayers = 1;

    async void Start()
    {
        if (joinRoomPopup) joinRoomPopup.SetActive(false);
        if (createRoomPopup) createRoomPopup.SetActive(false);

        await RefreshRooms();
    }

    public void OnRefreshRooms()
    {
        _ = RefreshRooms();
    }

    public async Task RefreshRooms()
    {
        ClearList();

        string json = await supa.GetRoomsRaw();
        var rooms = JsonArray<RoomRow>(json);

        foreach (var r in rooms)
        {
            var go = Instantiate(serverItemPrefab, serverListContent);
            var ui = go.GetComponent<ServerItemUI>();

            ui.Bind(
                roomName: r.name,
                hostName: r.owner_name,
                isLocked: !string.IsNullOrEmpty(r.password_hash),
                countText: $"{r.current_count}/{r.max_players}",
                onJoin: () => OnClickJoin(r)
            );
        }
    }

    public void OnClickJoin(RoomRow room)
    {
        if (room == null) return;

        // 비번방이면 팝업 띄움
        if (!string.IsNullOrEmpty(room.password_hash))
        {
            pendingJoinRoomId = room.id;
            pendingJoinRoomName = room.name;

            if (joinPasswordInput) joinPasswordInput.text = "";
            if (joinRoomPopup) joinRoomPopup.SetActive(true);

            if (joinPasswordInput) joinPasswordInput.ActivateInputField();
            return;
        }

        _ = TryJoin(room.id, room.name, null);
    }

    public void JoinRoomConfirm()
    {
        if (string.IsNullOrEmpty(pendingJoinRoomId))
        {
            Debug.Log("입장할 방 정보가 없습니다.");
            if (joinRoomPopup) joinRoomPopup.SetActive(false);
            return;
        }

        var pw = joinPasswordInput ? joinPasswordInput.text : null;
        if (joinRoomPopup) joinRoomPopup.SetActive(false);

        _ = TryJoin(pendingJoinRoomId, pendingJoinRoomName, pw);
    }

    public void OpenJoinRoomPopup(bool open)
    {
        if (joinRoomPopup) joinRoomPopup.SetActive(open);
    }

    public void OpenCreateRoomPopup(bool open)
    {
        if (createRoomPopup) createRoomPopup.SetActive(open);
        if (open && createRoomName) createRoomName.ActivateInputField();
    }

    public void PlusMaxPlayers()
    {
        createMaxPlayers = Mathf.Clamp((int)createMaxPlayers + 1, 1, 99);
        if (createRoomMaxCountText) createRoomMaxCountText.text = createMaxPlayers.ToString();
    }

    public void MinusMaxPlayers()
    {
        createMaxPlayers = Mathf.Clamp((int)createMaxPlayers - 1, 1, 99);
        if (createRoomMaxCountText) createRoomMaxCountText.text = createMaxPlayers.ToString();
    }

    public void CreateRoomConfirm()
    {
        _ = CreateRoomConfirmAsync();
    }

    async Task CreateRoomConfirmAsync()
    {
        var name = createRoomName ? createRoomName.text.Trim() : "";
        var pw = createRoomPassword ? createRoomPassword.text : "";

        if (string.IsNullOrEmpty(name))
        {
            Debug.Log("방 이름을 입력하세요");
            return;
        }

        string createdJson = await supa.CreateRoomRaw(name, PlayerIdentity.PlayerName, createMaxPlayers, string.IsNullOrEmpty(pw) ? null : pw);
        var createdArr = JsonArray<RoomRow>(createdJson);

        if (createdArr == null || createdArr.Count == 0)
        {
            Debug.Log("방 생성 실패(응답 없음)");
            return;
        }

        var created = createdArr[0];

        if (createRoomPopup) createRoomPopup.SetActive(false);

        await TryJoin(created.id, created.name, string.IsNullOrEmpty(pw) ? null : pw);
    }

    async Task TryJoin(string roomId, string roomName, string passwordOrNull)
    {
        string resultJson = await supa.JoinRoomRaw(
            roomId,
            PlayerIdentity.PlayerId,
            PlayerIdentity.PlayerName,
            passwordOrNull
        );

        var resList = JsonArray<JoinResult>(resultJson);
        if (resList == null || resList.Count == 0)
        {
            Debug.Log("입장 실패(응답 없음)");
            return;
        }

        var res = resList[0];

        if (!res.success)
        {
            if (res.error == "ROOM_FULL") Debug.Log("방이 가득 찼습니다");
            else if (res.error == "WRONG_PASSWORD") Debug.Log("비밀번호 불일치");
            else Debug.Log("입장 실패: " + res.error);
            return;
        }

        Session.CurrentRoomId = roomId;
        Session.CurrentRoomName = roomName;

        SceneTransitionController.Instance.LoadScene(NextSceneName);
    }

    void ClearList()
    {
        if (!serverListContent) return;

        for (int i = serverListContent.childCount - 1; i >= 0; i--)
            Destroy(serverListContent.GetChild(i).gameObject);
    }

    [Serializable] class JoinResult { public bool success; public string error; }

    static List<T> JsonArray<T>(string json)
    {
        string wrapped = "{\"items\":" + json + "}";
        var w = JsonUtility.FromJson<Wrap<T>>(wrapped);
        return w != null ? w.items : null;
    }

    [Serializable] class Wrap<T> { public List<T> items; }
}