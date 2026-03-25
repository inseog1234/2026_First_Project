using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NetWorldManager : MonoBehaviour
{
    [Header("Remote Player Visual")]
    [SerializeField] Color remoteColor = new Color(0.7f, 1f, 0.7f);
    [SerializeField] float nameHeight = 1.2f;
    [SerializeField] float hpBarHeight = -0.9f;

    readonly Dictionary<string, RemotePlayerView> remotePlayers = new();
    readonly Dictionary<string, Enemy> netEnemies = new();
    readonly Dictionary<string, ExpOrb> netOrbs = new();

    void Start()
    {
        WezardUdpClient.Instance.OnSnapJsonPart += OnSnapPart;
        WezardUdpClient.Instance.OnEventJson += OnEvt;
    }

    void OnDestroy()
    {
        if (WezardUdpClient.Instance == null) return;
        WezardUdpClient.Instance.OnSnapJsonPart -= OnSnapPart;
        WezardUdpClient.Instance.OnEventJson -= OnEvt;
    }

    // ===== 이벤트 처리(Exp/Kill) =====
    void OnEvt(string json)
    {
        // {"t":"evt","room":"...","kind":"exp","pid":"...","v":1}
        if (!json.Contains("\"room\":\"" + Session.CurrentRoomId + "\"")) return;

        string kind = Extract(json, "\"kind\":\"", "\"");
        string pid = Extract(json, "\"pid\":\"", "\"");
        int v = (int)ExtractFloat(json, "\"v\":");

        if (kind == "exp" && pid == LocalProfile.Id)
        {
            // 서버 확정 EXP 지급
            PlayerControll.Player.Instance.AddExp(v);
        }
        else if (kind == "kill" && pid == LocalProfile.Id)
        {
            KillManager.Instance.AddKill(v);
        }
    }

    // ===== 스냅샷 처리 =====
    void OnSnapPart(string json)
    {
        // part/total은 MVP라서 일단 “한 파트 단독” 기준으로 처리(적/오브 많으면 part 여러개가 와도 순차 적용됨)
        if (!json.Contains("\"room\":\"" + Session.CurrentRoomId + "\"")) return;

        // players
        ApplyPlayers(json);

        // enemies
        ApplyEnemies(json);

        // orbs
        ApplyOrbs(json);
    }

    void ApplyPlayers(string json)
    {
        // 내 플레이어 포함해서 전체 옴.
        // {"players":[{"id":"..","n":"..","x":..,"y":..,"z":..,"yaw":..,"hp":..}, ...]}

        foreach (var p in ExtractObjects(json, "\"players\":[", "]"))
        {
            string id = Extract(p, "\"id\":\"", "\"");
            if (string.IsNullOrEmpty(id)) continue;

            float x = ExtractFloat(p, "\"x\":");
            float y = ExtractFloat(p, "\"y\":");
            float z = ExtractFloat(p, "\"z\":");
            float hp = ExtractFloat(p, "\"hp\":");
            float yaw = ExtractFloat(p, "\"yaw\":");
            string name = Extract(p, "\"n\":\"", "\"");

            if (id == LocalProfile.Id)
            {
                // 내 HP는 서버 권위로 덮어쓰기
                // Unit.Hp가 protected라서 "NetSetHp" 같은 함수가 필요 -> 아래 수정 항목 참고
                var unit = PlayerControll.Player.Instance;
                unit.NetSetHp(hp);
                continue;
            }

            if (!remotePlayers.TryGetValue(id, out var view))
            {
                view = RemotePlayerView.Create(name, remoteColor, nameHeight, hpBarHeight);
                remotePlayers[id] = view;
            }

            view.SetName(name);
            view.SetHp01(hp / 100f);
            view.SetTarget(new Vector3(x, y, z), yaw);
        }

        // (선택) timeout 제거는 나중에
    }

    void ApplyEnemies(string json)
    {
        // enemies chunk
        var arr = ExtractObjects(json, "\"enemies\":[", "]");
        // 서버는 전체를 보낼 수도 있고 chunk일 수도 있음. 여기서는 "존재하면 갱신, 없으면 유지" 방식
        foreach (var e in arr)
        {
            string id = Extract(e, "\"id\":\"", "\"");
            if (string.IsNullOrEmpty(id)) continue;

            float x = ExtractFloat(e, "\"x\":");
            float y = ExtractFloat(e, "\"y\":");
            float z = ExtractFloat(e, "\"z\":");

            if (!netEnemies.TryGetValue(id, out var enemy))
            {
                // 풀에서 하나 꺼내서 프록시로 사용(기존 Enemy 스크립트 OFF)
                var prefab = EnemyPooling.Instance.pools[0].prefab;
                enemy = EnemyPooling.Instance.Get(prefab, new Vector2(x, z));
                enemy.enabled = false; // Enemy/Unit Update/FixedUpdate 방지
                enemy.gameObject.tag = "Enemy";

                var nid = enemy.GetComponent<NetEntityId>();
                if (nid == null) nid = enemy.gameObject.AddComponent<NetEntityId>();
                nid.id = id;

                netEnemies[id] = enemy;
            }

            enemy.transform.position = new Vector3(x, z, z);
        }

        // 제거 처리(풀 반환): 서버가 chunk라서 없다고 제거”하면 깜빡임.
        // MVP는 제거 이벤트로 처리하는게 맞는데 지금 서버는 제거 이벤트 안 보냄.
        // 대신: 적 hp<=0이면 다음 snap에서 안 오게 될 때 일정 시간 후 정리하는 방식으로 확장 가능.
    }

    void ApplyOrbs(string json)
    {
        var arr = ExtractObjects(json, "\"orbs\":[", "]");
        var alive = new HashSet<string>();

        foreach (var o in arr)
        {
            string id = Extract(o, "\"id\":\"", "\"");
            if (string.IsNullOrEmpty(id)) continue;

            float x = ExtractFloat(o, "\"x\":");
            float z = ExtractFloat(o, "\"z\":");
            int v = (int)ExtractFloat(o, "\"v\":");

            alive.Add(id);

            if (!netOrbs.TryGetValue(id, out var orb))
            {
                orb = ExpOrbPooling.Instance.Get(new Vector2(x, z), v);
                if (orb == null) continue;

                orb.enabled = false; // 싱글 Collect 로직 OFF
                var nid = orb.GetComponent<NetEntityId>();
                if (nid == null) nid = orb.gameObject.AddComponent<NetEntityId>();
                nid.id = id;

                var proxy = orb.GetComponent<NetOrbProxy>();
                if (proxy == null) proxy = orb.gameObject.AddComponent<NetOrbProxy>();
                proxy.Init(id);

                netOrbs[id] = orb;
            }

            orb.transform.position = new Vector3(x, z, z);
        }

        // 서버에서 사라진 오브는 풀로 반환
        var toRemove = new List<string>();
        foreach (var kv in netOrbs)
        {
            if (!alive.Contains(kv.Key))
            {
                ExpOrbPooling.Instance.Return(kv.Value);
                toRemove.Add(kv.Key);
            }
        }
        foreach (var k in toRemove) netOrbs.Remove(k);
    }

    // 파서
    static string Extract(string s, string start, string end)
    {
        int i = s.IndexOf(start);
        if (i < 0) return "";
        i += start.Length;
        int j = s.IndexOf(end, i);
        if (j < 0) return "";
        return s.Substring(i, j - i);
    }

    static float ExtractFloat(string s, string key)
    {
        int i = s.IndexOf(key);
        if (i < 0) return 0f;
        i += key.Length;
        int j = i;
        while (j < s.Length && "0123456789.-".IndexOf(s[j]) >= 0) j++;
        float.TryParse(s.Substring(i, j - i), out var v);
        return v;
    }

    static List<string> ExtractObjects(string json, string startKey, string endBracket)
    {
        // startKey 위치의 배열 내부를 "{...},{...}" 단위로 자르기? 분해? 
        int s = json.IndexOf(startKey);
        if (s < 0) return new List<string>();
        s += startKey.Length;

        int e = json.IndexOf(endBracket, s);
        if (e < 0) return new List<string>();

        var body = json.Substring(s, e - s);
        var list = new List<string>();

        int depth = 0;
        int objStart = -1;
        for (int i = 0; i < body.Length; i++)
        {
            char c = body[i];
            if (c == '{')
            {
                if (depth == 0) objStart = i;
                depth++;
            }
            else if (c == '}')
            {
                depth--;
                if (depth == 0 && objStart >= 0)
                {
                    list.Add(body.Substring(objStart, i - objStart + 1));
                    objStart = -1;
                }
            }
        }
        return list;
    }
}