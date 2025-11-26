using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingoApp.Models
{
    [ObservableObject]
    public partial class RoomConnectionInfo
    {
        [ObservableProperty]
        string roomId;
        
        [ObservableProperty]
        string roomName;
        
        [ObservableProperty]
        string creator;
        
        [ObservableProperty]
        string game;
        
        [ObservableProperty]
        string encodedRoomUUID;
        
        [ObservableProperty]
        string csrfMiddlewareToken;
        
        [ObservableProperty]
        string roomActionUrl;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(GameAndPreset))]
        string gameName;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(GameAndPreset))]
        string presetName;

        [ObservableProperty]
        GameMode gameMode;

        [ObservableProperty]
        ExtraGameMode gameExtraMode;

        public string GameAndPreset => $"{GameName} - {PresetName}";

        [ObservableProperty]
        bool isFromBingoApp;
    }
}
