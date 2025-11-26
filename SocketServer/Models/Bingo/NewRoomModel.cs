namespace SocketServer.Models.Bingo;

public class NewRoomModel
{
    public string CreatorId { get; set; } = default!;
    public string RoomName { get; set; } = default!;
    public string BoardJSON { get; set; } = default!;
    public GameMode GameMode { get; set; }
    public ExtraGameMode GameExtraMode { get; set; }
    public bool HideCard { get; set; } = default!;
    public bool AsSpectator { get; set; }
    public bool IsAutoBoardReveal { get; set; }
    public bool IsAutoFogWall { get; set; }
    public string GameName { get; set; } = default!;
    public string PresetName { get; set; } = default!;
    public string? Password { get; set; }
    public int StartTimeSeconds { get; set; }
    public int AfterRevealSeconds { get; set; }
    public int UnhideTimeMinutes { get; set; }
    public int ChangeTimeMinutes { get; set; }
    public bool IsTripleBingoSelect { get; set; }

}
