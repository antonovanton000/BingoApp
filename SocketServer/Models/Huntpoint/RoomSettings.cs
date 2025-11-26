namespace SocketServer.Models.Huntpoint;

public class RoomSettings
{
    public bool HideCard { get; set; }
    public bool IsAutoBoardReveal { get; set; }
    public string GameName { get; set; }
    public bool IsSameSeedGeneration { get; set; } = false;
    public string PresetName { get; set; }
    public ExtraGameMode ExtraGameMode { get; set; } = ExtraGameMode.None;
    public bool IsLegendaryExtraPoints { get; set; }
    public bool IsForFirstDeath { get; set; }

}

