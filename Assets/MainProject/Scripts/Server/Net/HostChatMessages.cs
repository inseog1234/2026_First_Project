using System;

[Serializable]
public class JoinReq { public string type = "join"; public string playerId; public string name; }

[Serializable]
public class ChatReq { public string type = "chat"; public string name; public string message; }

[Serializable]
public class SysMsg { public string type = "sys"; public string playerId; public string message; }

[Serializable]
public class ChatBroadcast { public string type = "chat"; public string name; public string message; }