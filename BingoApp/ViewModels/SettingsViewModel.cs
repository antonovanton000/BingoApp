using BingoApp.Models;
using BingoApp.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace BingoApp.ViewModels
{
    public partial class SettingsViewModel : MyBaseViewModel
    {
        MediaPlayer dingPlayer;


        [ObservableProperty]
        string nickName;

        [ObservableProperty]
        BingoColor bingoColor;

        [ObservableProperty]
        string defaultRoomName;

        [ObservableProperty]
        string defaultPassword;

        [ObservableProperty]
        int startingTime;

        [ObservableProperty]
        int afterRevealTime;

        [ObservableProperty]
        bool feedPlayerChat;

        [ObservableProperty]
        bool feedGoalActions;

        [ObservableProperty]
        bool feedColorChanged;

        [ObservableProperty]
        bool feedConnections;

        [ObservableProperty]
        bool isSoundsOn;

        [ObservableProperty]
        int soundsVolume;

        [ObservableProperty]
        bool isDebug;

        private Uri revealSound = new Uri(System.Environment.CurrentDirectory + "\\Sounds\\reveal.wav", UriKind.Absolute);


        [RelayCommand]
        async Task Appearing()
        {
            MainWindow.HideSettingsButton();
            (App.Current.MainWindow as MainWindow).frame.Navigating += Frame_Navigating;
            GetSettings();
            SettingsPage.SetPreferedColor(BingoColor);

            dingPlayer = new MediaPlayer();
            dingPlayer.Volume = SoundsVolume * 0.01d;
            dingPlayer.MediaEnded += (s, e) =>
            {
                dingPlayer.Stop();
                dingPlayer.Close();
                dingPlayer.Position = TimeSpan.Zero;
            };

        }

        [RelayCommand]
        void TestSound()
        {
            try
            {
                dingPlayer.Open(revealSound);
                dingPlayer.Volume = SoundsVolume * 0.01d;
                dingPlayer.Play();

                if (IsDebug)
                {
                    var debugMessage = $"Reveal sound path: {revealSound}\r\nSoundsVolume: {SoundsVolume}\r\nPlayer.Volume: {dingPlayer.Volume}\r\n----------------------\r\n";
                    System.IO.File.AppendAllText(System.IO.Path.Combine(App.Location, "logs.txt"), debugMessage);
                }
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText(System.IO.Path.Combine(App.Location, "errors.txt"), $"Error: {ex.Message}\r\n-------------------\r\n");
            }
        }

        private void Frame_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == System.Windows.Navigation.NavigationMode.Back)
            {
                SaveSettings();
                (App.Current.MainWindow as MainWindow).frame.Navigating -= Frame_Navigating;
            }
        }

        void GetSettings()
        {
            NickName = BingoApp.Properties.Settings.Default.NickName;
            BingoColor = (BingoColor)Enum.Parse(typeof(BingoColor), BingoApp.Properties.Settings.Default.PreferedColor);
            DefaultRoomName = BingoApp.Properties.Settings.Default.DefaultRoomName;
            DefaultPassword = BingoApp.Properties.Settings.Default.DefaultPassword;
            StartingTime = BingoApp.Properties.Settings.Default.BeforeStartTime;
            AfterRevealTime = BingoApp.Properties.Settings.Default.BoardAnalyzeTime;
            FeedPlayerChat = BingoApp.Properties.Settings.Default.IsPlayerChat;
            FeedGoalActions = BingoApp.Properties.Settings.Default.IsGoalActions;
            FeedColorChanged = BingoApp.Properties.Settings.Default.IsColorChanged;
            FeedConnections = BingoApp.Properties.Settings.Default.IsConnections;
            IsSoundsOn = BingoApp.Properties.Settings.Default.IsSoundsOn;
            IsDebug = BingoApp.Properties.Settings.Default.IsDebug;
            SoundsVolume = BingoApp.Properties.Settings.Default.SoundsVolume;
        }

        void SaveSettings()
        {
            BingoApp.Properties.Settings.Default.NickName = NickName;
            BingoApp.Properties.Settings.Default.PreferedColor = BingoColor.ToString();
            BingoApp.Properties.Settings.Default.DefaultRoomName = DefaultRoomName;
            BingoApp.Properties.Settings.Default.DefaultPassword = DefaultPassword;
            BingoApp.Properties.Settings.Default.BeforeStartTime = StartingTime;
            BingoApp.Properties.Settings.Default.BoardAnalyzeTime = AfterRevealTime;
            BingoApp.Properties.Settings.Default.IsPlayerChat = FeedPlayerChat;
            BingoApp.Properties.Settings.Default.IsGoalActions = FeedGoalActions;
            BingoApp.Properties.Settings.Default.IsColorChanged = FeedColorChanged;
            BingoApp.Properties.Settings.Default.IsConnections = FeedConnections;
            BingoApp.Properties.Settings.Default.SoundsVolume = SoundsVolume;
            BingoApp.Properties.Settings.Default.IsSoundsOn = IsSoundsOn;
            BingoApp.Properties.Settings.Default.IsDebug = IsDebug;
            BingoApp.Properties.Settings.Default.Save();
        }

    }
}
