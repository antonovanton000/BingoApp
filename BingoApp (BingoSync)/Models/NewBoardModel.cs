using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingoApp.Models
{
    public partial class NewBoardModel : ObservableObject
    {
        [ObservableProperty]
        string roomName;
        
        [ObservableProperty]
        string password;
        
        [ObservableProperty]
        string nickName;
        
        [ObservableProperty]
        string boardJSON;

        [ObservableProperty]
        RoomLockoutMode roomLockoutMode;

        [ObservableProperty]
        RoomExtraMode roomExtraMode = RoomExtraMode.All.First();

        [ObservableProperty]
        bool hideCard = true;
        
        [ObservableProperty]
        bool asSpectator = false;
        
        [ObservableProperty]
        bool isAutoBoardReveal = true;
        
        [ObservableProperty]
        BoardPreset boardPreset;

        [ObservableProperty]
        bool isRoomNameError = false;

        [ObservableProperty]
        bool isPasswordError = false;

        [ObservableProperty]
        bool isNickNameError = false;

        [ObservableProperty]
        bool isBoardPresetError = false;

        [ObservableProperty]
        bool isBoardJsonError = false;

        [ObservableProperty]
        string dynamicPresetName;

        [ObservableProperty]
        string dynamicGameName;

        public int Game { get; set; } = 18;
        public int? Seed { get; set; }
        public int Variant { get; set; } = 172;

    }
}
