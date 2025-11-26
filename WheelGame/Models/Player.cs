using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.RegularExpressions;

namespace WheelGame.Models
{
    public partial class Player : ObservableObject
    {

        [ObservableProperty]
        string id;

        [ObservableProperty]
        string nickName;
        
        [ObservableProperty]
        int score;                
        
        [ObservableProperty]
        bool isCurrentPlayer;

        [ObservableProperty]
        bool isPlayerTurn;

        [ObservableProperty]
        bool isFinished = false;

        [ObservableProperty]
        int playerIndex;
    }
}
