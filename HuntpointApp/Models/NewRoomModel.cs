using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace HuntpointApp.Models;

public partial class NewRoomModel : ObservableObject
{
    public NewRoomModel()
    {
        
    }

    [ObservableProperty]
    string creatorId;

    [ObservableProperty]
    string roomName;
                            
    [ObservableProperty]
    string boardJSON;

    [ObservableProperty]
    ExtraGameMode extraGameMode;

    [ObservableProperty]
    bool hideCard = true;
    
    [ObservableProperty]
    bool asSpectator = false;
    
    [ObservableProperty]
    bool isAutoBoardReveal = true;

    [ObservableProperty]
    string gameName = default!;

    [ObservableProperty]
    string presetName = default!;

    [ObservableProperty]
    [property: JsonIgnore]
    RoomExtraMode roomExtraMode = RoomExtraMode.All.First();

    [ObservableProperty]
    string? password;

    [ObservableProperty]
    string? legendaryObjective;

    [ObservableProperty]
    [property: JsonIgnore]
    LegendaryObjectiveSelect? legendaryObjectiveItem;

    [ObservableProperty]
    bool isLegendaryExtraPoints = true;

    [ObservableProperty]
    bool isForFirstDeath = true;

    [ObservableProperty]
    [property: JsonIgnore]
    [NotifyPropertyChangedFor(nameof(LegendaryObjectives))]
    [NotifyPropertyChangedFor(nameof(LegendaryObjectiveItem))]
    [NotifyPropertyChangedFor(nameof(IsNightreign))]
    [NotifyPropertyChangedFor(nameof(IsSekiro))]
    BoardPreset boardPreset;

    [JsonIgnore]
    public ObservableCollection<LegendaryObjectiveSelect> LegendaryObjectives { get; set; } = new() { new LegendaryObjectiveSelect() { Name = App.Current.Resources["mp_random"].ToString() } };
                        
    [ObservableProperty]
    [property: JsonIgnore]
    bool isRoomNameError = false;

    [ObservableProperty]
    [property: JsonIgnore]
    bool isPasswordError = false;

    [ObservableProperty]
    [property: JsonIgnore]
    bool isNickNameError = false;

    [ObservableProperty]
    [property: JsonIgnore]
    bool isBoardPresetError = false;

    [ObservableProperty]
    [property: JsonIgnore]
    bool isBoardJsonError = false;

    [ObservableProperty]
    bool isGenerateSameSeeds = false;

    [JsonIgnore]
    public bool IsNightreign => BoardPreset?.Game.Contains("Nightreign", StringComparison.OrdinalIgnoreCase) ?? false; 
    public bool IsSekiro => BoardPreset?.Game.Contains("Sekiro", StringComparison.OrdinalIgnoreCase) ?? false;

}

public class LegendaryObjectiveSelect
{
    public string Name { get; set; } = default!;
    public string? Value { get; set; } = null;
}
