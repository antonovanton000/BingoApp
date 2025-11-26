namespace SocketServer.Models.Huntpoint;

public class RoomTimerSettings
{
    public DateTime Timestamp { get; set; }
    public string RoomId { get; set; }        
    public int StartTime { get; set; }
    public int AfterRevealTime { get; set; }
    public int UnhideTime { get; set; }
    public int ChangeTime { get; set; }
}
