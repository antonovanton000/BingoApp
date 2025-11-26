using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace WheelGame.Models;

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
    string debufsJSON;

    [ObservableProperty]
    string? password;

    [ObservableProperty]
    int timerSeconds;

    [ObservableProperty]
    [property: JsonIgnore]
    bool isRoomNameError = false;
}
