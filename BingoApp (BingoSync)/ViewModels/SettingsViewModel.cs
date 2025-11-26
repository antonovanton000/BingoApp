using BingoApp.Classes;
using BingoApp.Models;
using BingoApp.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
        int afterRevealTimeChanging;

        [ObservableProperty]
        int afterRevealTimeHidden;

        [ObservableProperty]
        int changingSquareTime;

        [ObservableProperty]
        int unhideSquareTime;

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
        int localServerPort;

        [ObservableProperty]
        bool isStartLocalServer;

        [ObservableProperty]
        string version;

        [ObservableProperty]
        string localServerLinks = "";

        AppLanguage curLanguage;
        public AppLanguage CurLanguage { get => curLanguage; set {  curLanguage = value; OnPropertyChanged(); LanguageChanged(); } }

        [ObservableProperty]
        List<AppLanguage> languages = new() { 
            new AppLanguage() { LanguageName = "English", Culture = new CultureInfo("en-US")},
            new AppLanguage() { LanguageName = "Русский", Culture = new CultureInfo("ru-RU")}
        };

        [ObservableProperty]
        bool isDebug;

        private Uri revealSound = new Uri(System.Environment.CurrentDirectory + "\\Sounds\\reveal.wav", UriKind.Absolute);


        [RelayCommand]
        async Task Appearing()
        {
            MainWindow.HideSettingsButton();
            (App.Current.MainWindow as MainWindow).frame.Navigating += Frame_Navigating;
            
            this.Version = App.AppVersion;

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

            this.PropertyChanged += SettingsViewModel_PropertyChanged;

            LocalServerLinks = "";
            foreach (var ip in LocalWebServer.GetIPAddresses())
            {
                LocalServerLinks += $"http://{ip}:{LocalServerPort}\r\n";
            }
        }

        private void SettingsViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LocalServerPort) || e.PropertyName == nameof(IsStartLocalServer))
            {
                LocalServerLinks = "";
                foreach (var ip in LocalWebServer.GetIPAddresses())
                {
                    LocalServerLinks += $"http://{ip}:{LocalServerPort}\r\n";
                }

                if (IsStartLocalServer)
                {
                    WindowsFirewallHelper.OpenPortNetsh(LocalServerPort);
                }

            }
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
            NickName = Properties.Settings.Default.NickName;
            BingoColor = (BingoColor)Enum.Parse(typeof(BingoColor), Properties.Settings.Default.PreferedColor);
            DefaultRoomName = Properties.Settings.Default.DefaultRoomName;
            DefaultPassword = Properties.Settings.Default.DefaultPassword;
            StartingTime = Properties.Settings.Default.BeforeStartTime;
            AfterRevealTime = Properties.Settings.Default.BoardAnalyzeTime;
            FeedPlayerChat = Properties.Settings.Default.IsPlayerChat;
            FeedGoalActions = Properties.Settings.Default.IsGoalActions;
            FeedColorChanged = Properties.Settings.Default.IsColorChanged;
            FeedConnections = Properties.Settings.Default.IsConnections;
            IsSoundsOn = Properties.Settings.Default.IsSoundsOn;
            IsDebug = Properties.Settings.Default.IsDebug;
            SoundsVolume = Properties.Settings.Default.SoundsVolume;
            CurLanguage = Languages.First(i => i.Culture.Name == BingoApp.Properties.Settings.Default.AppLanguage);
            AfterRevealTimeChanging = Properties.Settings.Default.BoardAnalyzeTimeChanging;
            AfterRevealTimeHidden = Properties.Settings.Default.BoardAnalyzeTimeHidden;
            ChangingSquareTime = Properties.Settings.Default.BoardChangeSqaureTime;
            UnhideSquareTime = Properties.Settings.Default.BoardUnhideSqaresTime;
            LocalServerPort = Properties.Settings.Default.LocalServerPort;
            IsStartLocalServer = Properties.Settings.Default.IsStartLocalServer;
        }

        void SaveSettings()
        {
            Properties.Settings.Default.NickName = NickName;
            Properties.Settings.Default.PreferedColor = BingoColor.ToString();
            Properties.Settings.Default.DefaultRoomName = DefaultRoomName;
            Properties.Settings.Default.DefaultPassword = DefaultPassword;
            Properties.Settings.Default.BeforeStartTime = StartingTime;
            Properties.Settings.Default.BoardAnalyzeTime = AfterRevealTime;
            Properties.Settings.Default.IsPlayerChat = FeedPlayerChat;
            Properties.Settings.Default.IsGoalActions = FeedGoalActions;
            Properties.Settings.Default.IsColorChanged = FeedColorChanged;
            Properties.Settings.Default.IsConnections = FeedConnections;
            Properties.Settings.Default.SoundsVolume = SoundsVolume;
            Properties.Settings.Default.IsSoundsOn = IsSoundsOn;
            Properties.Settings.Default.IsDebug = IsDebug;
            Properties.Settings.Default.AppLanguage = CurLanguage.Culture.Name;
            Properties.Settings.Default.BoardAnalyzeTimeChanging = AfterRevealTimeChanging;
            Properties.Settings.Default.BoardAnalyzeTimeHidden = AfterRevealTimeHidden;
            Properties.Settings.Default.BoardChangeSqaureTime = ChangingSquareTime;
            Properties.Settings.Default.BoardUnhideSqaresTime = UnhideSquareTime;
            Properties.Settings.Default.LocalServerPort = LocalServerPort;
            Properties.Settings.Default.IsStartLocalServer = IsStartLocalServer;

            BingoApp.Properties.Settings.Default.Save();
        }

        void LanguageChanged()
        {
            App.Language = CurLanguage.Culture;
        }

        public class AppLanguage
        {
            public string LanguageName { get; set; } = null!;

            public CultureInfo Culture { get; set; } = null!;
        }
    }
}
