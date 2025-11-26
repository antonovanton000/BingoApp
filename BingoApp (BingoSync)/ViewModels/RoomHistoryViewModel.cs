using BingoApp.Classes;
using BingoApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace BingoApp.ViewModels
{
    public partial class RoomHistoryViewModel : MyBaseViewModel
    {

        public RoomHistoryViewModel()
        {
                                              
        }

        [ObservableProperty]
        Room? room;
                
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TimerString))]
        TimeSpan timerCounter;

        public string TimerString => TimerCounter.ToString(@"hh\:mm\:ss");

        [ObservableProperty]
        ICollectionView chatEventsMessages;

        [RelayCommand]
        async Task Appearing()
        {
            MainWindow.HideSettingsButton();
            ChatEventsMessages = CollectionViewSource.GetDefaultView(Room.ChatMessages);             
            TimerCounter = TimeSpan.FromSeconds(Room.CurrentTimerTime);

        }

        [RelayCommand]
        async Task SendResults()
        {
            var gameResult = new GameResult()
            {
                RoomId = Room.RoomId,
                BingoType = Room.GameMode.ToString(),
                PlayersNames = Room.Players.Select(i => i.NickName).ToArray(),
                Score = Room.Players.Select(i => i.SquaresCount).ToArray(),
                LinesCount = Room.Players.Select(i => i.LinesCount).ToArray(),
                GameDate = Room.StartDate ?? DateTime.MinValue,
                PresetName = Room.PresetName,
                GameName = Room.GameName
            };

            var res = await App.RestClient.SendGameResult(gameResult);
            if (res.IsSuccess)
            {
                MainWindow.ShowToast(new ToastInfo() { Title = App.Current.FindResource("mes_success").ToString(), Detail = App.Current.FindResource("mes_resultsend").ToString() });
            }
        }
    }
}
