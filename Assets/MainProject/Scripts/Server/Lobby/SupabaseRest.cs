using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class SupabaseRest : MonoBehaviour
{
    [SerializeField] SupabaseConfig config;

    string RestBase => $"{config.supabaseUrl}/rest/v1";
    string RpcBase  => $"{config.supabaseUrl}/rest/v1/rpc";

    UnityWebRequest Req(string url, string method, string jsonBody = null)
    {
        var req = new UnityWebRequest(url, method);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("apikey", config.anonKey);
        req.SetRequestHeader("Authorization", $"Bearer {config.anonKey}");
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Accept", "application/json");

        if (jsonBody != null)
        {
            var bytes = Encoding.UTF8.GetBytes(jsonBody);
            req.uploadHandler = new UploadHandlerRaw(bytes);
        }
        return req;
    }

    async Task<string> Send(UnityWebRequest req)
    {
        var op = req.SendWebRequest();
        while (!op.isDone) await Task.Yield();
        if (req.result != UnityWebRequest.Result.Success)
            throw new System.Exception(req.error + "\n" + req.downloadHandler.text);
        return req.downloadHandler.text;
    }

    // 방 목록 가져오는 함수
    public Task<string> GetRoomsRaw()
    {
        string url = $"{RestBase}/rooms?select=id,name,owner_name,max_players,current_count,password_hash,created_at&order=created_at.desc";
        return Send(Req(url, "GET"));
    }

    // 방 만들기
    public Task<string> CreateRoomRaw(string name, string ownerName, long maxPlayers, string passwordOrNull)
    {
        string url = $"{RestBase}/rooms";
        var body =
            $"{{" +
            $"\"name\":{JsonStr(name)}," +
            $"\"owner_name\":{JsonStr(ownerName)}," +
            $"\"max_players\":{maxPlayers}," +
            $"\"password_hash\":{JsonNullableStr(passwordOrNull)}" +
            $"}}";

        var req = Req(url, "POST", body);
        req.SetRequestHeader("Prefer", "return=representation");
        return Send(req);
    }
    
    // 방 들어가기
    public Task<string> JoinRoomRaw(string roomId, string playerId, string playerName, string passwordOrNull)
    {
        string url = $"{RpcBase}/join_room";
        var body = $"{{\"p_room_id\":\"{roomId}\",\"p_player_id\":\"{playerId}\",\"p_player_name\":{JsonStr(playerName)},\"p_password\":{JsonNullableStr(passwordOrNull)}}}";
        return Send(Req(url, "POST", body));
    }

    // 방 나가기
    public Task<string> LeaveRoomRaw(string roomId, string playerId)
    {
        string url = $"{RpcBase}/leave_room";
        var body = $"{{\"p_room_id\":\"{roomId}\",\"p_player_id\":\"{playerId}\"}}";
        return Send(Req(url, "POST", body));
    }

    // 방 플레이어 목록 가져오기
    public Task<string> GetRoomPlayersRaw(string roomId)
    {
        string url = $"{RestBase}/room_players?select=player_id,player_name,joined_at&room_id=eq.{roomId}&order=joined_at.asc";
        return Send(Req(url, "GET"));
    }

    static string JsonStr(string s) => "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
    static string JsonNullableStr(string s) => string.IsNullOrEmpty(s) ? "null" : JsonStr(s);
}