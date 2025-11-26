using HuntpointApp.Classes;
using HuntpointApp.Models;
using HuntpointApp.Properties;
using HuntpointApp.Popups;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace HuntpointApp.ViewModels
{
    public partial class RoomHistoryViewModel : MyBaseViewModel
    {               
        #region Properties

        [ObservableProperty]
        Room room = default!;

        [ObservableProperty]
        bool isTeamView;
        #endregion

        #region MiscCommands

        [RelayCommand]
        async Task SendResults()
        {
            var gameResult = new GameResult()
            {
                RoomId = Room.RoomId,                
                PlayersNames = Room.RoomPlayers.Select(i => i.NickName).ToArray(),
                Score = Room.RoomPlayers.Select(i => i.Score).ToArray(),                
                GameDate = Room.StartDate ?? DateTime.MinValue,
                PresetName = Room.RoomSettings.PresetName,
                GameName = Room.RoomSettings.GameName
            };

            var res = await App.RestClient.SendGameResult(gameResult);
            if (res.IsSuccess)
            {
                MainWindow.ShowToast(new ToastInfo() { Title = App.Current.FindResource("mes_success").ToString(), Detail = App.Current.FindResource("mes_resultsend").ToString() });
            }
        }

        #endregion
    }
}
