using System;

namespace WeZard.RelayProtocol
{
    [Serializable] public class CreateRoomReq { public string type="create_room"; public string roomId; public string ownerId; public string ownerName; }
    [Serializable] public class JoinRoomReq   { public string type="join_room";   public string roomId; public string playerId; public string playerName; }
    [Serializable] public class LeaveRoomReq  { public string type="leave_room";  public string roomId; public string playerId; }
    [Serializable] public class ChatReq       { public string type="chat";        public string roomId; public string playerId; public string message; }

    [Serializable] public class OkMsg   { public string type="ok";  public string request; public string roomId; public string ownerId; }
    [Serializable] public class ErrMsg  { public string type="err"; public string code; public string message; }
    [Serializable] public class SysMsg  { public string type="sys"; public string roomId; public string playerId; public string message; }
    [Serializable] public class ChatMsg { public string type="chat"; public string roomId; public string playerId; public string playerName; public string message; }
}