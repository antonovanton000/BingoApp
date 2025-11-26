using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingoApp.Models
{
    public partial class NewRoomModel : ObservableObject
    {
        public string CreatorId { get; set; } = default!;

        [ObservableProperty]
        string roomName;
        
        [ObservableProperty]
        string password;
        
        [ObservableProperty]
        string nickName;
        
        [ObservableProperty]
        string boardJSON;

        [ObservableProperty]
        [property: JsonIgnore]
        RoomMode roomLockoutMode;

        [ObservableProperty]
        [property: JsonIgnore]
        RoomExtraMode roomExtraMode = BingoApp.Models.RoomExtraMode.All.First();
        public GameMode GameMode { get; set; }
        public ExtraGameMode GameExtraMode { get; set; }

        [ObservableProperty]
        bool hideCard = true;
        
        [ObservableProperty]
        bool asSpectator = false;
        
        [ObservableProperty]
        bool isAutoBoardReveal = true;

        [ObservableProperty]
        bool isAutoFogWall = true;

        [JsonIgnore]
        public bool IsGameSekiro => BoardPreset?.Game == "Sekiro";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsGameSekiro))]
        [property: JsonIgnore]
        BoardPreset boardPreset;

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
        [property: JsonIgnore]
        string dynamicPresetName;

        [ObservableProperty]
        [property: JsonIgnore]
        string dynamicGameName;

        [ObservableProperty]
        string gameName = default!;

        [ObservableProperty]
        string presetName = default!;
        public int StartTimeSeconds { get; set; }
        public int AfterRevealSeconds { get; set; }
        public int UnhideTimeMinutes { get; set; }
        public int ChangeTimeMinutes { get; set; }

        [ObservableProperty]
        bool isTripleBingoSelect;

        public void UpdatePropertyChanged()
        {
            OnPropertyChanged(nameof(RoomLockoutMode));
            OnPropertyChanged(nameof(RoomExtraMode));
        }

    }
}
