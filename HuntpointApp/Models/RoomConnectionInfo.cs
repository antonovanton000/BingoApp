using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuntpointApp.Models
{    
    public partial class RoomConnectionInfo : ObservableObject
    {
        [ObservableProperty]
        string roomId;
        
        [ObservableProperty]
        string roomName;
        
        [ObservableProperty]
        string creatorName;
                               
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(GameAndPreset))]
        [NotifyPropertyChangedFor(nameof(IsSekiro))]
        [NotifyPropertyChangedFor(nameof(IsNightreign))]
        string gameName;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(GameAndPreset))]
        string presetName;
        
        [ObservableProperty]
        ExtraGameMode gameExtraMode;

        [ObservableProperty]
        bool isLegendaryExtraPoints = true;

        public string GameAndPreset => $"{GameName} - {PresetName}";

        public bool IsNightreign => GameName?.Contains("Nightreign", StringComparison.OrdinalIgnoreCase) ?? false;
        public bool IsSekiro => GameName?.Contains("Sekiro", StringComparison.OrdinalIgnoreCase) ?? false;

        [ObservableProperty]
        bool isFromBingoApp;

        [ObservableProperty]
        string? legendaryObjective;

        [ObservableProperty]
        bool isForFirstDeath = true;

        [ObservableProperty]
        bool isGenerateSameSeeds = true;
    }
}
