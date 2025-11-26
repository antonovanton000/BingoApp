
using Newtonsoft.Json;

namespace SocketServer.Models.Huntpoint;

public partial class PresetObjective
{                
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonProperty("score")]
    public int Score { get; set; } = 0;

    [JsonProperty("type")]
    public ObjectiveType Type { get; set; } = ObjectiveType.Regular;

    [JsonProperty("shiftingEarth")]
    public ShiftingEarhType ShiftingEath { get; set; } = ShiftingEarhType.Default;

}
