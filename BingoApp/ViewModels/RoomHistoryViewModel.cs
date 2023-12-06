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
    }
}
