using HuntpointApp.Classes;
using HuntpointApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using HuntpointApp.Views;
using HuntpointApp.Popups;

namespace HuntpointApp.ViewModels
{
    public partial class MainViewModel : MyBaseViewModel
    {
        #region Constructor
        private static MainViewModel instance;
        public MainViewModel()
        {
            AppVersion = App.AppVersion;
            //App.SignalRHub.OnInvitePlayerRecieved += SignalRHub_OnInvitePlayerRecieved;
            //App.SignalRHub.OnCancelInviteRecieved += SignalRHub_OnCancelInviteRecieved;
            App.SignalRHub.OnGameInviteRecieved += SignalRHub_OnGameInviteRecieved;
            App.SignalRHub.OnSharePresetRecieved += SignalRHub_OnSharePresetRecieved;
            instance = this;
        }
        #endregion

        #region Properties

        [ObservableProperty]
        string appVersion;

        [ObservableProperty]
        bool isFromStartupArgs = false;

        public string HelloMessage
        {
            get
            {
                var userName = HuntpointApp.Properties.Settings.Default.NickName;
                var message = "";
                if (DateTime.Now.Hour < 12)
                {
                    message = App.Current.FindResource("mp_goodmorning").ToString(); //"Good Morning";
                }
                else if (DateTime.Now.Hour < 17)
                {
                    message = App.Current.FindResource("mp_goodafternoon").ToString();//"Good Afternoon";
                }
                else
                {
                    message = App.Current.FindResource("mp_goodevening").ToString();//"Good Evening";
                }

                return message + (string.IsNullOrEmpty(userName) ? "!" : $", {userName}!");
            }
        }

        [ObservableProperty]
        int activeRoomsCount;

        [ObservableProperty]
        Uri currentPageUri;

        [ObservableProperty]
        bool isNewRoomPageActive;

        [ObservableProperty]
        bool isConnectToRoomPageActive;

        [ObservableProperty]
        bool isPracticePageActive;
        #endregion

        #region Fields

        ExtraGameMode connectRoomExtraGameMode;

        string connectRoomPresetName;

        string connectRoomGameName;

        #endregion

        #region Appearing

        [RelayCommand]
        async Task Appearing()
        {
            CurrentPageUri = new Uri("/Views/WelcomeAboutAppPage.xaml", UriKind.Relative);
            MainWindow.ShowSettingsButton();
            GetActiveBoards();
            SetSekiro();
            //await CheckStartupArgs();
            try
            {
                var resp = await App.RestClient.UpdateUserInfoAsync(App.CurrentPlayer);
                await ConnectToHub();
                var timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(40) };
                timer.Tick += async (s, e) =>
                {
                    var resp = await App.RestClient.UpdateUserInfoAsync(App.CurrentPlayer);
                };
                timer.Start();               
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        #endregion

        #region Commands
        [RelayCommand]
        void CreateNewRoom()
        {
            IsPracticePageActive = IsConnectToRoomPageActive = false;
            CurrentPageUri = new Uri("/Views/NewRoomPage.xaml", UriKind.Relative);
            IsNewRoomPageActive = true;
        }

        [RelayCommand]
        void ShowConnectModal()
        {
            IsNewRoomPageActive = IsPracticePageActive = false;
            CurrentPageUri = new Uri("/Views/ConnectToRoomPage.xaml", UriKind.Relative);
            IsConnectToRoomPageActive = true;
        }

        [RelayCommand]
        void OpenBoardManager()
        {
            MainWindow.NavigateTo(new GameManagerPage());
        }

        [RelayCommand]
        void OpenHistory()
        {
            MainWindow.NavigateTo(new HistoryPage());
        }

        [RelayCommand]
        void OpenPractice()
        {
            IsNewRoomPageActive = IsConnectToRoomPageActive = false;
            CurrentPageUri = new Uri("/Views/PracticePage.xaml", UriKind.Relative);
            IsPracticePageActive = true;
        }

        [RelayCommand]
        void OpenSettings()
        {
            MainWindow.NavigateTo(new SettingsPage());
        }

        #endregion

        #region Methods

        public static void ShowMapSelector()
        {
            instance.IsNewRoomPageActive = false;
            instance.IsPracticePageActive = false;
            instance.IsConnectToRoomPageActive = false;
            instance.CurrentPageUri = new Uri("/Views/MapSelectorPage.xaml", UriKind.Relative);
        }
        public static void ShowStormTimer()
        {
            instance.IsNewRoomPageActive = false;
            instance.IsPracticePageActive = false;
            instance.IsConnectToRoomPageActive = false;
            instance.CurrentPageUri = new Uri("/Views/StormTimerPage.xaml", UriKind.Relative);
        }

        public static void ShowWelcomeText()
        {
            instance.IsPracticePageActive = instance.IsConnectToRoomPageActive = false;
            instance.IsNewRoomPageActive = false;
            instance.CurrentPageUri = new Uri("/Views/WelcomeAboutAppPage.xaml", UriKind.Relative);
        }

        public static void UpdateActiveRoomsCount(int count)
        {
            instance.ActiveRoomsCount = count;
        }
        async Task ConnectToHub()
        {
            try
            {
                var resp = await App.RestClient.UpdateUserInfoAsync(App.CurrentPlayer);
                await App.SignalRHub.ConnectAsync(HuntpointApp.Properties.Settings.Default.PlayerId);
            }
            catch (Exception ex) { Logger.Error(ex); }
        }

        void GetActiveBoards()
        {
            try
            {
                var dirName = System.IO.Path.Combine(App.Location, "ActiveRooms");
                if (System.IO.Directory.Exists(dirName))
                {
                    var files = System.IO.Directory.GetFiles(dirName, "*.json");
                    ActiveRoomsCount = files.Length;
                }
                else
                    ActiveRoomsCount = 0;
            }
            catch (Exception ex) { Logger.Error(ex); }
        }

        async Task DownloadAndSavePresetAsync(HuntpointSignalRHub.SharePresetEventArgs e)
        {
            try
            {
                MainWindow.ShowModal(new DownloadPresetModal(), this);
                await Task.Delay(500);

                await (new TaskFactory()).StartNew(async () =>
                {

                    var presetResponse = await App.RestClient.GetSharedPresetSquares(e.SquaresFileName);
                    if (presetResponse.IsSuccess)
                    {
                        if (!Directory.Exists(Path.Combine(App.Location, "Presets", e.GameName)))
                        {
                            Directory.CreateDirectory(Path.Combine(App.Location, "Presets", e.GameName));
                        }
                        await File.WriteAllTextAsync(Path.Combine(App.Location, "Presets", e.GameName, e.PresetName + ".json"), presetResponse.Data.ToString());
                    }

                    var notesResponse = await App.RestClient.GetSharedPresetNotes(e.NotesFileName);
                    if (notesResponse.IsSuccess)
                    {
                        var newNotes = JsonConvert.DeserializeObject<List<ObjectiveNote>>(notesResponse.Data.ToString());
                        if (newNotes != null)
                        {
                            var oldNotes = new List<ObjectiveNote>();
                            var notesPath = System.IO.Path.Combine(App.Location, "notes.json");
                            if (System.IO.File.Exists(notesPath))
                            {
                                var json = await System.IO.File.ReadAllTextAsync(notesPath);
                                if (json != null)
                                {
                                    oldNotes = JsonConvert.DeserializeObject<List<ObjectiveNote>>(json);
                                }
                            }

                            foreach (var item in newNotes)
                            {
                                var note = oldNotes.FirstOrDefault(i => i.ObjectiveName == item.ObjectiveName);
                                if (note == null)
                                {
                                    oldNotes.Add(item);
                                }
                                else
                                {
                                    note.Note = item.Note; // Update existing note
                                }
                            }
                            await System.IO.File.WriteAllTextAsync(notesPath, JsonConvert.SerializeObject(oldNotes, Formatting.Indented));
                        }
                    }
                    await App.Current.Dispatcher.InvokeAsync(async () =>
                    {
                        MainWindow.CloseModal();
                        await App.SignalRHub.SendClearSharedPreset(e.SquaresFileName, e.NotesFileName);
                        MainWindow.ShowToast(new ToastInfo() { Title = $"{App.Current.FindResource("mes_success")}", Detail = App.Current.FindResource("mes_presetsharedsuccess").ToString() });

                    });
                });
            }
            catch (Exception ex)
            { }
        }

        void SetSekiro()
        {
            try
            {
                var sekiroPath = HuntpointApp.Properties.Settings.Default.SekiroExePath;
                if (!string.IsNullOrEmpty(sekiroPath) && File.Exists(sekiroPath))
                {
                    var modEngineIniPath = Path.Combine(Path.GetDirectoryName(sekiroPath), "modengine.ini");
                    if (File.Exists(modEngineIniPath))
                    {
                        ModEngineIniHelper.UpdateModEngineIni(
                            modEngineIniPath,
                            "\\huntpointmod\\SekiroHelper.dll",
                            "\\huntpointmod");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        #endregion

        #region HubEventHandlers

        private void SignalRHub_OnGameInviteRecieved(object? sender, HuntpointSignalRHub.GameInviteEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                MainWindow.ShowMessage(string.Format(App.Current.FindResource("mp_gameinvite").ToString(), e.NickName), MessageNotificationType.YesNo,
                    async () =>
                    {
                        try
                        {
                            App.StartupArgs = e.Data;
                            CurrentPageUri = new Uri("/Views/WelcomeAboutAppPage.xaml", UriKind.Relative);
                            await Task.Delay(200);
                            CurrentPageUri = new Uri("/Views/ConnectToRoomPage.xaml", UriKind.Relative);                            
                            IsConnectToRoomPageActive = true;

                        }
                        catch (Exception ex)
                        {
                            MainWindow.ShowErrorMessage(ex.Message, "SplashPage", "Appearing");                            
                        }
                    });
            });
        }

        private void SignalRHub_OnSharePresetRecieved(object? sender, HuntpointSignalRHub.SharePresetEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                MainWindow.ShowMessage(string.Format(App.Current.FindResource("mes_sharepresetmes").ToString(), e.FromPlayerName, $"{e.GameName} - {e.PresetName}"), MessageNotificationType.YesNo,
                    async () =>
                    {
                        try
                        {
                            MainWindow.CloseMessage();
                            await Task.Delay(500);

                            if (File.Exists(Path.Combine(App.Location, "Presets", e.GameName, e.PresetName + ".json")))
                            {

                                MainWindow.ShowMessage(string.Format(App.Current.FindResource("mes_presetalreadyexist").ToString(), $"{e.GameName} - {e.PresetName}"), MessageNotificationType.YesNo,
                                async () =>
                                {
                                    await DownloadAndSavePresetAsync(e);
                                },
                                async () =>
                                {
                                    await App.SignalRHub.SendClearSharedPreset(e.SquaresFileName, e.NotesFileName);
                                });
                            }
                            else
                            {
                                await DownloadAndSavePresetAsync(e);
                            }

                        }
                        catch (Exception ex)
                        {

                        }
                    },
                    async () =>
                    {
                        await App.SignalRHub.SendClearSharedPreset(e.SquaresFileName, e.NotesFileName);
                    });
            });
        }
        #endregion

    }
}
