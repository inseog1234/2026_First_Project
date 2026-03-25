public static class NetMode
{
    public static bool IsMultiplayer => !string.IsNullOrEmpty(Session.CurrentRoomId);
    public static string RoomId => Session.CurrentRoomId;
    public static string PlayerId => LocalProfile.Id;
    public static string PlayerName => LocalProfile.Name;

    public const string UdpHost = "mintcat.arheneos.com";
    public const int UdpPort = 7979;
}