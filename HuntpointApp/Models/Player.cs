using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.RegularExpressions;

namespace HuntpointApp.Models
{
    public partial class Player : ObservableObject
    {

        [ObservableProperty]
        string id;

        [ObservableProperty]
        string nickName;

        [ObservableProperty]
        HuntpointColor color;

        [ObservableProperty]
        int score;                

        [ObservableProperty]
        bool isSpectator;

        [ObservableProperty]
        bool isCurrentPlayer;

        [ObservableProperty]
        bool isFinished;

        [ObservableProperty]
        bool isFirstLegendaryMarked;

        [ObservableProperty]
        [property: Newtonsoft.Json.JsonIgnore]
        bool isCaster;

        public async Task AnimateCaster()
        {
            IsCaster = true;
            await Task.Delay(10000);
            IsCaster = false;
        }
    }
}
