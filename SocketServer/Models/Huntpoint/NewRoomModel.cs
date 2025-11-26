
namespace SocketServer.Models.Huntpoint;

public class NewRoomModel
{

    public string CreatorId { get; set; } = default!;
    public string RoomName { get; set; } = default!;
    public string BoardJSON { get; set; } = default!;
    public ExtraGameMode RoomExtraMode { get; set; }
    public bool HideCard { get; set; } = default!;
    public bool AsSpectator { get; set; }
    public bool IsAutoBoardReveal { get; set; }
    public string GameName { get; set; } = default!;
    public string PresetName { get; set; } = default!;
    public string? Password { get; set; }
    public string? LegendaryObjective { get; set; }
    public bool IsGenerateSameSeeds { get; set; }
    public bool IsLegendaryExtraPoints{ get; set; }
    public bool IsForFirstDeath { get; set; }


}
