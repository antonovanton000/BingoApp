using Newtonsoft.Json;

namespace SocketServer.Models.Wheel;

public partial class PresetObjective
{                
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonProperty("score")]
    public int Score { get; set; } = 1;

    [JsonProperty("type")]
    public ObjectiveType Type { get; set; } = ObjectiveType.EarlyGame;
    
    [JsonProperty("modifiers")]
    public PresetModifier[] Modifiers { get; set; } = new PresetModifier[0];

}

public class PresetModifier
{
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonProperty("points")]
    public int Points { get; set; } = 0; 
}