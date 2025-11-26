namespace SocketServer.Models.Huntpoint;

public class GameResult
{

    public string RoomId { get; set; } = default!;    
    public DateTime GameDate { get; set; }
    public string[] PlayersNames { get; set; } = default!;        
    public int[] Score { get; set; } = default!;    
    public string PresetName { get; set; } = default!;
    public string GameName { get; set; } = default!;
}
