using BingoApp.Classes;
using BingoApp.Models;
using BingoApp.Properties;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Identity.UI.Services;
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

namespace BingoApp.ViewModels
{
    public partial class RoomPageViewModel : MyBaseViewModel
    {
        #region Construcror
        public RoomPageViewModel()
        {
            _timerSocket = new TimerSocketClient();
            _timer = new DispatcherTimer(DispatcherPriority.Input) { Interval = new TimeSpan(0, 0, 0, 0, 100) };
            _timer.Tick += _timer_Tick;

            IsPlayerChat = BingoApp.Properties.Settings.Default.IsPlayerChat;
            IsGoalActions = BingoApp.Properties.Settings.Default.IsGoalActions;
            IsColorChanged = BingoApp.Properties.Settings.Default.IsColorChanged;
            IsConnections = BingoApp.Properties.Settings.Default.IsConnections;
            IsSoundsOn = BingoApp.Properties.Settings.Default.IsSoundsOn;
            SoundsVolume = BingoApp.Properties.Settings.Default.SoundsVolume;

            dingPlayer = new MediaPlayer
            {
                Volume = SoundsVolume * 0.01d
            };

            dingPlayer.MediaEnded += (s, e) =>
            {
                dingPlayer.Stop();
                dingPlayer.Close();
                dingPlayer.Position = TimeSpan.Zero;
            };
            newEventTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(7) };
            newEventTimer.Tick += (s, e) =>
            {
                newEventTimer.Stop();
                IsNewEventAdded = false;
            };
        }
        #endregion

        #region Fields

        public DispatcherTimer _timer;
        Stopwatch sw = new Stopwatch();
        private TimerSocketClient _timerSocket;
        MediaPlayer dingPlayer;
        private Uri chatSound = new Uri(App.Location + "\\Sounds\\chat.wav", UriKind.Absolute);
        private Uri connectedSound = new Uri(App.Location + "\\Sounds\\connected.wav", UriKind.Absolute);
        private Uri disconnectedSound = new Uri(App.Location + "\\Sounds\\disconnected.wav", UriKind.Absolute);
        private Uri goalSound = new Uri(App.Location + "\\Sounds\\goal.wav", UriKind.Absolute);
        private Uri revealSound = new Uri(App.Location + "\\Sounds\\reveal.wav", UriKind.Absolute);
        private Uri bingoSound = new Uri(App.Location + "\\Sounds\\bingo.wav", UriKind.Absolute);
        private Uri beepSound = new Uri(App.Location + "\\Sounds\\beep-timer.wav", UriKind.Absolute);
        private Uri newsquareSound = new Uri(App.Location + "\\Sounds\\new-square.wav", UriKind.Absolute);
        private TimeSpan savedTime = TimeSpan.Zero;
        bool leaveunsaved = false;
        bool localServerStarted = false;
        DispatcherTimer newEventTimer;
        private BoardWindow? boardWindow = null;
        #endregion

        #region Events
        public event EventHandler EventAdded;
        public event EventHandler BingoHappened;
        #endregion

        #region Properties

        [ObservableProperty]
        Room room = default!;

        [ObservableProperty]
        bool isPotentialBingoVisible = false;

        [ObservableProperty]
        BingoColor potentialBingoColor = BingoColor.blank;

        [ObservableProperty]
        string chatMessage;

        [ObservableProperty]
        bool isStartVisible = true;

        [ObservableProperty]
        bool isPauseVisible = false;

        [ObservableProperty]
        bool isResetVisible = false;

        [ObservableProperty]
        bool isStopVisible = false;

        [ObservableProperty]
        bool isTimerButtonsVisible = true;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TimerString))]
        [NotifyPropertyChangedFor(nameof(StartingTimerString))]
        [NotifyPropertyChangedFor(nameof(AfterRevealTimerString))]
        TimeSpan timerCounter;

        public string TimerString => TimerCounter.ToString(@"hh\:mm\:ss");
        public string StartingTimerString => (TimeSpan.FromSeconds(StartTimeSeconds) - TimerCounter).ToString(@"hh\:mm\:ss");
        public string AfterRevealTimerString => (TimeSpan.FromSeconds(AfterRevealSeconds) - TimerCounter).ToString(@"hh\:mm\:ss");

        [ObservableProperty]
        bool isPlayerChat;

        [ObservableProperty]
        bool isGoalActions;

        [ObservableProperty]
        bool isColorChanged;

        [ObservableProperty]
        bool isConnections;

        [ObservableProperty]
        bool isPasswordShare;
        public ObservableCollection<BingoAppPlayer> AvailablePlayers { get; set; } = [];

        [ObservableProperty]
        ICollectionView chatEventsMessages;

        [ObservableProperty]
        int startTimeSeconds;

        [ObservableProperty]
        int afterRevealSeconds;

        [ObservableProperty]
        int unhideTimeMinutes;

        [ObservableProperty]
        int changeTimeMinutes;

        [ObservableProperty]
        bool isConnected;

        [ObservableProperty]
        bool isStarted = false;

        [ObservableProperty]
        string revealPanelText = App.Current.FindResource("mes_clicktoreveal").ToString();

        [ObservableProperty]
        bool isSoundsOn;

        [ObservableProperty]
        int soundsVolume;

        [ObservableProperty]
        bool isDebug;

        [ObservableProperty]
        bool isTeamView = false;

        [ObservableProperty]
        bool isFinishGameVisible;

        [ObservableProperty]
        bool isRefreshing;

        [ObservableProperty]
        bool isTimerVisible;

        [ObservableProperty]
        bool isInvitePopupVisible;

        [ObservableProperty]
        BingoAppPlayer selectedPlayer;

        [ObservableProperty]
        bool isReconnecting = false;

        [ObservableProperty]
        bool isNewEventAdded = false;

        [ObservableProperty]
        Event newFeedEvent;

        [ObservableProperty]
        bool isExternalWindowOpened = false;

        #endregion

        #region TimerControlCommands

        [RelayCommand]
        async Task StartTimer()
        {
            if (localServerStarted && App.LocalSignalRHub != null)
            {
                if (Room.IsTimerOnPause)
                {
                    await App.LocalSignalRHub.SendTimerResume(Room.CurrentTimerTime);
                }
                else
                {
                    await App.LocalSignalRHub.SendTimerStart();
                }
            }
            IsStartVisible = false;
            IsPauseVisible = true;
            IsResetVisible = true;
            Room.IsTimerStarted = true;
            Room.IsTimerOnPause = false;
            Room.IsGameEnded = false;
            sw.Start();
            _timer.Start();
            if (Room.IsAutoBoardReveal)
            {
                await App.SignalRHub.SendResumeGame(Room.RoomId);
            }
        }

        [RelayCommand]
        async Task PauseTimer()
        {
            if (localServerStarted && App.LocalSignalRHub != null)
            {
                await App.LocalSignalRHub.SendTimerPause();
            }
            IsStartVisible = true;
            IsPauseVisible = false;
            Room.IsTimerStarted = false;
            Room.IsTimerOnPause = true;
            sw.Stop();
            _timer.Stop();
            if (Room.IsAutoBoardReveal)
            {
                await App.SignalRHub.SendPauseGame(Room.RoomId);
            }


        }

        [RelayCommand]
        async Task ResetTimer()
        {
            sw.Stop();
            sw.Reset();
            _timer.Stop();
            IsPauseVisible = false;
            IsResetVisible = false;
            Room.IsTimerStarted = false;
            IsStartVisible = true;
            Room.IsTimerOnPause = false;
            Room.IsGameEnded = false;
            TimerCounter = TimeSpan.FromSeconds(0);

            if (localServerStarted && App.LocalSignalRHub != null)
            {
                await App.LocalSignalRHub.SendTimerReset();
            }
        }

        [RelayCommand]
        async Task StopTimer()
        {
            await App.SignalRHub.SendStopGame(Room.RoomId);
        }

        [RelayCommand]
        async Task StartGame()
        {
            await App.SignalRHub.SendStartGame(Room.RoomId);
        }

        #endregion

        #region TimerEvents
        private async void _timer_Tick(object? sender, EventArgs e)
        {
            TimerCounter = (sw.Elapsed + savedTime);
            Room.CurrentTimerTime = (int)TimerCounter.TotalSeconds;

            if (Room.IsGameTimerStarted && Room.GameExtraMode == ExtraGameMode.Hidden && Room.CurrentHiddenStep <= 13)
            {
                if (TimerCounter.TotalMinutes >= (UnhideTimeMinutes * Room.CurrentHiddenStep))
                {
                    Room.UnhideSquares();
                }
            }

            if (Room.IsGameTimerStarted && Room.GameExtraMode == ExtraGameMode.Changing)
            {
                if (Room.IsCreatorMode)
                {
                    int totalMinutes = (int)TimerCounter.TotalMinutes;
                    if ((totalMinutes % ChangeTimeMinutes) == 0 && Room.LastChangeMinute != totalMinutes)
                    {
                        await Room.ProcessChangingGame(totalMinutes);
                    }
                }
            }

            if (Room.IsTimerStarted)
            {
                if (Room.IsStartTimerStarted)
                {
                    Room.IsAutoRevealBoardVisible = false;
                    if (((int)TimerCounter.TotalSeconds) == StartTimeSeconds)
                    {
                        sw.Stop();
                        sw.Reset();
                        Room.IsStartTimerStarted = false;
                        Room.IsAfterRevealTimerStarted = true;
                        await Room.RevealTheBoard();
                        Room.IsRevealed = true;
                        sw.Start();

                        if (localServerStarted && App.LocalSignalRHub != null)
                        {
                            await App.Current.Dispatcher.InvokeAsync(async () =>
                            {
                                await App.LocalSignalRHub.SendAfterRevealTimeStarted(AfterRevealSeconds);
                            });
                        }
                    }
                }
                if (Room.IsAfterRevealTimerStarted)
                {
                    Room.IsAutoRevealBoardVisible = false;
                    if ((int)TimerCounter.TotalSeconds == (AfterRevealSeconds - 6))
                    {
                        dingPlayer.Open(beepSound);
                        dingPlayer.Volume = SoundsVolume * 0.01d;
                        dingPlayer.Play();
                    }
                    if (((int)TimerCounter.TotalSeconds) == AfterRevealSeconds)
                    {
                        sw.Stop();
                        sw.Reset();
                        Room.IsAfterRevealTimerStarted = false;
                        Room.IsGameTimerStarted = true;
                        Room.IsGameStarted = true;
                        Room.StartDate = DateTime.Now;
                        Room.CurrentTimerTime = 0;
                        IsStarted = true;
                        IsStopVisible = false;
                        if (Room.GameExtraMode == ExtraGameMode.Hidden || Room.GameExtraMode == ExtraGameMode.Changing)
                        {
                            if (Room.IsCreatorMode)
                                IsTimerButtonsVisible = true;
                            else
                                IsTimerButtonsVisible = false;
                        }
                        else
                        {
                            IsTimerButtonsVisible = true;
                        }
                        await Room.SaveAsync();
                        await StartTimer();
                    }
                }
                if (Room.IsGameTimerStarted)
                {
                    if ((Room.CurrentTimerTime % 10) == 0)
                    {
                        await Room.SaveAsync();
                    }
                }
            }
        }

        private async Task AutoBoardRevealProcess(bool isRefresh = false)
        {
            _timer.Stop();
            sw.Stop();
            sw.Reset();
            AddSignalRHubEventHandlers();
            await App.SignalRHub.ConnectToRoomHub(Room.RoomId);

            if (Room.IsGameStarted)
            {
                TimerCounter = TimeSpan.FromSeconds(Room.CurrentTimerTime);
                savedTime = TimeSpan.FromSeconds(Room.CurrentTimerTime);
                Room.IsTimerStarted = false;
                IsTimerVisible = true;

                if (Room.IsCreatorMode)
                    IsTimerButtonsVisible = true;
                else
                    IsTimerButtonsVisible = false;
            }
            else
            {
                TimerCounter = TimeSpan.FromSeconds(isRefresh ? 0 : Room.CurrentTimerTime);
                savedTime = TimeSpan.FromSeconds(isRefresh ? 0 : Room.CurrentTimerTime);
                Room.IsTimerStarted = false;
                IsStartVisible = false;
                IsPauseVisible = false;
                IsResetVisible = false;
                IsStopVisible = false;
                IsTimerButtonsVisible = false;
                Room.IsStartTimerStarted = false;
                Room.IsAfterRevealTimerStarted = false;
                Room.IsGameTimerStarted = false;
                IsTimerVisible = true;
                RevealPanelText = App.Current.FindResource("mes_waitingforgames").ToString();

                if (Room.IsCreatorMode)
                {
                    Room.IsAutoRevealBoardVisible = true;

                    StartTimeSeconds = Settings.Default.BeforeStartTime;
                    UnhideTimeMinutes = Settings.Default.BoardUnhideSqaresTime;
                    ChangeTimeMinutes = Settings.Default.BoardChangeSqaureTime;
                    switch (Room.GameExtraMode)
                    {
                        case ExtraGameMode.None:
                            AfterRevealSeconds = Settings.Default.BoardAnalyzeTime;
                            break;
                        case ExtraGameMode.Hidden:
                            AfterRevealSeconds = Settings.Default.BoardAnalyzeTimeHidden;
                            break;
                        case ExtraGameMode.Changing:
                            AfterRevealSeconds = Settings.Default.BoardAnalyzeTimeChanging;
                            break;
                    }

                    await App.SignalRHub.SendRoomTimerSettings(Room.RoomId, StartTimeSeconds, AfterRevealSeconds, UnhideTimeMinutes, ChangeTimeMinutes);
                }
                else
                {
                    if (Room.CurrentPlayer.IsSpectator)
                    {
                        Room.IsAutoRevealBoardVisible = false;
                        IsTimerVisible = true;
                    }
                    else
                    {
                        IsTimerVisible = true;
                        IsTimerButtonsVisible = false;
                        RevealPanelText = App.Current.FindResource("mes_waitingforgames").ToString();
                        Room.IsTimerStarted = false;
                    }
                }
            }
        }

        #endregion

        #region SignalRHubEventHandlers
        private void AddSignalRHubEventHandlers()
        {
            App.SignalRHub.OnRoomTimerSettingsRecieved += SignalRHub_OnRoomTimerSettingsRecieved;
            App.SignalRHub.OnRoomTimeSyncRecieved += SignalRHub_OnRoomTimeSyncRecieved;
            App.SignalRHub.OnStartGameRecieved += SignalRHub_OnStartGameRecieved;
            App.SignalRHub.OnPauseGameRecieved += SignalRHub_OnPauseGameRecieved;
            App.SignalRHub.OnStopGameRecieved += SignalRHub_OnStopGameRecieved;
            App.SignalRHub.OnResumeGameRecieved += SignalRHub_OnResumeGameRecieved;
            App.SignalRHub.OnDisconnected += SignalRHub_OnDisconnected;
            App.SignalRHub.OnPlayerDisconnected += SignalRHub_OnPlayerDisconnected;
            App.SignalRHub.OnPlayerReconnected += SignalRHub_OnPlayerReconnected;
            App.SignalRHub.OnChangeSquareRecieved += SignalRHub_OnChangeSquareRecieved;
            App.SignalRHub.OnRoomBoardRecieved += SignalRHub_OnRoomBoardRecieved;
            App.SignalRHub.OnReconnecting += SignalRHub_OnReconnecting;
            App.SignalRHub.OnReconnected += SignalRHub_OnReconnected;
        }

        private void RemoveSignalRHubEventHandlers()
        {
            App.SignalRHub.OnRoomTimerSettingsRecieved -= SignalRHub_OnRoomTimerSettingsRecieved;
            App.SignalRHub.OnRoomTimeSyncRecieved -= SignalRHub_OnRoomTimeSyncRecieved;
            App.SignalRHub.OnStartGameRecieved -= SignalRHub_OnStartGameRecieved;
            App.SignalRHub.OnPauseGameRecieved -= SignalRHub_OnPauseGameRecieved;
            App.SignalRHub.OnStopGameRecieved -= SignalRHub_OnStopGameRecieved;
            App.SignalRHub.OnResumeGameRecieved -= SignalRHub_OnResumeGameRecieved;
            App.SignalRHub.OnDisconnected -= SignalRHub_OnDisconnected;
            App.SignalRHub.OnPlayerDisconnected -= SignalRHub_OnPlayerDisconnected;
            App.SignalRHub.OnPlayerReconnected -= SignalRHub_OnPlayerReconnected;
            App.SignalRHub.OnChangeSquareRecieved -= SignalRHub_OnChangeSquareRecieved;
            App.SignalRHub.OnRoomBoardRecieved -= SignalRHub_OnRoomBoardRecieved;
            App.SignalRHub.OnReconnecting -= SignalRHub_OnReconnecting;
            App.SignalRHub.OnReconnected -= SignalRHub_OnReconnected;
        }

        private void SignalRHub_OnRoomBoardRecieved(object? sender, BingoAppSignalRHub.RoomBoardEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() => {
                foreach (var item in e.Squares)
                {
                    var square = Room.Board.Squares.FirstOrDefault(i => i.Slot == item.Slot);
                    if (square!=null)
                    {
                        square.Name = item.Name;
                    }
                }
            });
        }

        private async void SignalRHub_OnChangeSquareRecieved(object? sender, BingoAppSignalRHub.ChangeSquareEventArgs e)
        {
            await App.Current.Dispatcher.InvokeAsync(async () =>
            {
                if (e.RoomId == Room.RoomId && !Room.IsCreatorMode)
                {
                    await Room.ChangeSquare(e.SlotId, e.NewName);
                }
            });
        }

        private async void Room_NewCardEvent(object? sender, EventArgs e)
        {
            if (Room.IsAutoBoardReveal)
            {
                Room.IsGameStarted = false;
                await AutoBoardRevealProcess(true);
            }
        }

        private void SignalRHub_OnReconnected(object? sender, string e)
        {
            IsReconnecting = false;
        }

        private void SignalRHub_OnReconnecting(object? sender, string e)
        {
            IsReconnecting = true;
        }

        private void SignalRHub_OnPlayerDisconnected(object? sender, string e)
        {
            sw.Stop();
            _timer.Stop();
            IsStartVisible = true;
            IsPauseVisible = false;
            Room.IsTimerStarted = false;
            App.Current.Dispatcher.Invoke(() =>
            {
                dingPlayer.Open(disconnectedSound);
                dingPlayer.Volume = SoundsVolume * 0.01d;
                dingPlayer.Play();
            });
        }

        private async void SignalRHub_OnDisconnected(object? sender, string e)
        {
            IsReconnecting = true;
            sw.Stop();
            _timer.Stop();
            IsStartVisible = true;
            IsPauseVisible = false;
            Room.IsTimerStarted = false;
            var tf = new TaskFactory();
            await tf.StartNew(async () =>
            {
                while (!App.SignalRHub.IsHubConnected)
                {
                    await App.SignalRHub.ConnectAsync(App.CurrentPlayer.Id);
                    if (App.SignalRHub.IsHubConnected)
                    {
                        await App.SignalRHub.ConnectToRoomHub(Room.RoomId);
                        await Room.ConnectToSocketAsync();

                        if (Room.IsCreatorMode)
                        {
                            var currentTime = (int)TimerCounter.TotalSeconds;
                            await App.SignalRHub.SendCurrentRoomTime(Room.RoomId, currentTime);
                            savedTime = TimeSpan.FromSeconds(currentTime);
                            sw.Reset();
                        }

                        IsReconnecting = false;
                    }
                    else
                    {
                        IsReconnecting = true;
                        await Task.Delay(3000);
                    }
                }
            });
        }

        private async void SignalRHub_OnResumeGameRecieved(object? sender, string e)
        {
            if (e != Room.RoomId) return;
            Room.IsTimerOnPause = false;
            Room.IsGameEnded = false;
            sw.Start();
            _timer.Start();
            if (localServerStarted && App.LocalSignalRHub != null)
            {
                await App.LocalSignalRHub.SendTimerResume(Room.CurrentTimerTime);
            }
        }

        private async void SignalRHub_OnStopGameRecieved(object? sender, string e)
        {
            if (e != Room.RoomId) return;

            _timer.Stop();
            sw.Stop();
            sw.Reset();
            Room.IsStartTimerStarted = false;
            Room.IsGameTimerStarted = false;
            Room.IsAfterRevealTimerStarted = false;
            IsPauseVisible = false;
            IsResetVisible = false;
            IsStartVisible = false;
            IsStopVisible = false;
            Room.IsTimerOnPause = false;
            Room.IsTimerStarted = false;
            TimerCounter = TimeSpan.FromSeconds(0);
            IsTimerVisible = false;
            Room.IsRevealed = false;
            savedTime = TimeSpan.FromSeconds(0);
            Room.CurrentTimerTime = 0;
            if (Room.IsCreatorMode)
            {
                Room.IsAutoRevealBoardVisible = true;
            }

            if (localServerStarted && App.LocalSignalRHub != null)
            {
                await App.LocalSignalRHub.SendGameStoped();
            }
        }

        private async void SignalRHub_OnPauseGameRecieved(object? sender, string e)
        {
            if (e != Room.RoomId) return;

            Room.IsTimerOnPause = true;
            sw.Stop();
            _timer.Stop();

            if (localServerStarted && App.LocalSignalRHub != null)
            {
                await App.LocalSignalRHub.SendTimerPause();
            }
        }

        private async void SignalRHub_OnStartGameRecieved(object? sender, string e)
        {
            if (e != Room.RoomId) return;

            TimerCounter = TimeSpan.FromSeconds(0);
            Room.IsAfterRevealTimerStarted = false;
            Room.IsGameTimerStarted = false;
            Room.IsStartTimerStarted = true;
            Room.IsTimerStarted = true;
            Room.IsGameEnded = false;
            IsStopVisible = true;
            IsTimerVisible = true;
            Room.IsTimerOnPause = false;
            sw.Start();
            _timer.Start();

            if (localServerStarted && App.LocalSignalRHub != null)
            {
                await App.LocalSignalRHub.SendStartTimeStarted(StartTimeSeconds);
            }

        }

        private void SignalRHub_OnRoomTimerSettingsRecieved(object? sender, BingoAppSignalRHub.RoomTimerSettingsEventArgs e)
        {
            if (e.RoomId != Room.RoomId) return;

            StartTimeSeconds = e.StartTime;
            AfterRevealSeconds = e.AfterRevealTime;
            UnhideTimeMinutes = e.UnhideTime;
            ChangeTimeMinutes = e.ChangeTime;
        }

        private void SignalRHub_OnRoomTimeSyncRecieved(object? sender, BingoAppSignalRHub.RoomTimeSyncEventArgs e)
        {
            if (e.RoomId != Room.RoomId) return;

            Room.CurrentTimerTime = e.CurrentTime;
            sw.Reset();
            savedTime = TimeSpan.FromSeconds(e.CurrentTime);
            TimerCounter = savedTime;
        }

        private async void SignalRHub_OnPlayerReconnected(object? sender, string e)
        {
            if (Room.IsCreatorMode)
            {
                await App.SignalRHub.SendCurrentRoomTime(Room.RoomId, Room.CurrentTimerTime);
                App.Current.Dispatcher.Invoke(() =>
                {
                    dingPlayer.Open(connectedSound);
                    dingPlayer.Volume = SoundsVolume * 0.01d;
                    dingPlayer.Play();
                });
            }
        }

        #endregion

        #region EventHandlers
        private void Frame_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == System.Windows.Navigation.NavigationMode.Back)
            {
                if (!Room.IsGameEnded)
                {
                    e.Cancel = !leaveunsaved;

                    if (Room.IsCreatorMode)
                    {
                        if (!leaveunsaved)
                        {
                            MainWindow.ShowMessage(App.Current.FindResource("mes_yourgamenotfini").ToString(), MessageNotificationType.YesNo,
                                new Action(async () =>
                                {
                                    if (boardWindow != null)
                                    {
                                        boardWindow.Close();
                                        boardWindow = null;
                                        IsExternalWindowOpened = false;
                                    }

                                    if (!Room.IsPractice)
                                    {
                                        await Room.DisconnectAsync();
                                        await _timerSocket.DisconnectAsync();
                                        RemoveSignalRHubEventHandlers();
                                        await App.SignalRHub.DisconnectFromRoomHub(Room.RoomId);
                                    }
                                    Room.CurrentTimerTime = (int)TimerCounter.TotalSeconds;
                                    await Room.SaveAsync();

                                    if (App.LocalServer.IsServerStarted)
                                        await App.LocalServer.StopAsync();

                                    leaveunsaved = true;
                                    MainWindow.CloseMessage();
                                    (App.Current.MainWindow as MainWindow).frame.Navigating -= Frame_Navigating;
                                    (App.Current.MainWindow as MainWindow).Closing -= RoomPageViewModel_Closing;
                                    await Task.Delay(300);
                                    MainWindow.GoBack();

                                }),
                                new Action(() =>
                                {
                                    leaveunsaved = false;
                                }));
                        }
                    }
                    else
                    {
                        if (Room.CurrentPlayer.IsSpectator)
                        {
                            if (!leaveunsaved)
                            {
                                MainWindow.ShowMessage(App.Current.FindResource("mes_areyousureyouwa\r\n").ToString(), MessageNotificationType.YesNo,
                                    new Action(async () =>
                                    {
                                        if (boardWindow != null)
                                        {
                                            boardWindow.Close();
                                            boardWindow = null;
                                            IsExternalWindowOpened = false;
                                        }
                                        leaveunsaved = true;
                                        if (Room.GameExtraMode == ExtraGameMode.Changing)
                                        {
                                            App.SignalRHub.OnChangeSquareRecieved -= SignalRHub_OnChangeSquareRecieved;
                                            await App.SignalRHub.DisconnectFromRoomHub(Room.RoomId);
                                        }
                                        if (App.LocalServer.IsServerStarted)
                                            await App.LocalServer.StopAsync();

                                        MainWindow.CloseMessage();
                                        (App.Current.MainWindow as MainWindow).frame.Navigating -= Frame_Navigating;
                                        (App.Current.MainWindow as MainWindow).Closing -= RoomPageViewModel_Closing;
                                        await Task.Delay(300);
                                        MainWindow.GoBack();

                                    }),
                                    new Action(() =>
                                    {
                                        leaveunsaved = false;
                                    }));
                            }
                        }
                        else
                        {
                            if (!leaveunsaved)
                            {
                                MainWindow.ShowMessage(App.Current.FindResource("mes_yourgamenotfini").ToString(), MessageNotificationType.YesNo,
                                    new Action(async () =>
                                    {
                                        await Room.DisconnectAsync();
                                        await _timerSocket.DisconnectAsync();
                                        Room.CurrentTimerTime = (int)TimerCounter.TotalSeconds;
                                        await Room.SaveAsync();
                                        leaveunsaved = true;
                                        MainWindow.CloseMessage();
                                        if (App.LocalServer.IsServerStarted)
                                            await App.LocalServer.StopAsync();

                                        (App.Current.MainWindow as MainWindow).frame.Navigating -= Frame_Navigating;
                                        (App.Current.MainWindow as MainWindow).Closing -= RoomPageViewModel_Closing;
                                        await Task.Delay(300);
                                        MainWindow.GoBack();

                                    }),
                                    new Action(() =>
                                    {
                                        leaveunsaved = false;
                                    }));
                            }
                        }
                    }
                }
            }
        }

        private async void ChatMessages_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                EventAdded?.Invoke(this, new EventArgs());
                Uri soundPath = null;
                var eventType = "";
                if (IsSoundsOn)
                {
                    if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                    {
                        if (e.NewItems != null)
                        {
                            var newEvent = (Event)e.NewItems[0];
                            if (newEvent != null)
                            {
                                eventType = newEvent.Type.ToString();
                                switch (newEvent.Type)
                                {
                                    case EventType.connection:
                                        if (newEvent.EventType == EventSubType.connected)
                                            soundPath = connectedSound;
                                        else if (newEvent.EventType == EventSubType.disconnected)
                                            soundPath = disconnectedSound;
                                        break;
                                    case EventType.chat:
                                        soundPath = chatSound;
                                        break;
                                    case EventType.revealed:
                                        soundPath = revealSound;
                                        break;
                                    case EventType.goal:
                                        soundPath = goalSound;
                                        break;
                                    case EventType.color:
                                        break;
                                    case EventType.none:
                                        break;
                                    case EventType.newsquare:
                                        soundPath = newsquareSound;
                                        break;
                                    case EventType.unhudesquare:
                                        soundPath = revealSound;
                                        break;
                                    case EventType.bingo:
                                        {
                                            soundPath = bingoSound;
                                            BingoHappened?.Invoke(this, new EventArgs());
                                            Room.IsGameEnded = true;
                                            await PauseTimer();
                                        }
                                        break;
                                    case EventType.win:
                                        {
                                            soundPath = bingoSound;
                                            BingoHappened?.Invoke(this, new EventArgs());
                                            Room.IsGameEnded = true;
                                            await PauseTimer();
                                        }
                                        break;
                                    default:
                                        break;
                                }
                                dingPlayer.Open(soundPath);
                                dingPlayer.Volume = SoundsVolume * 0.01d;
                                dingPlayer.Play();
                            }
                        }

                    }
                }
                if (IsDebug)
                {
                    var debugMessage = $"IsSoundsOn: {IsSoundsOn}\r\nAction: {e.Action}\r\nEventType: {eventType}\r\nSound path: {soundPath}\r\nSoundsVolume: {SoundsVolume}\r\nPlayer.Volume: {dingPlayer.Volume}\r\n----------------------\r\n";
                    System.IO.File.AppendAllText(System.IO.Path.Combine(App.Location, "logs.txt"), debugMessage);
                }
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    if (e.NewItems != null)
                    {
                        if (e.NewItems[0] is Event newEvent)
                        {
                            if (localServerStarted && App.LocalSignalRHub != null)
                            {
                                await App.LocalSignalRHub.SendNewChatEvent(newEvent);
                            }
                            newEventTimer.Stop();
                            NewFeedEvent = newEvent;
                            IsNewEventAdded = false;
                            await Task.Delay(200);
                            IsNewEventAdded = true;
                            newEventTimer.Start();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText(System.IO.Path.Combine(App.Location, "errors.log"), ex.Message + "\r\n");
            }
        }

        private void RoomPageViewModel_Closing(object? sender, CancelEventArgs e)
        {
            if (!Room.IsGameEnded)
            {
                e.Cancel = true;
                if (Room.IsCreatorMode)
                {
                    MainWindow.ShowMessage(App.Current.FindResource("mes_yourgamenotfini").ToString(), MessageNotificationType.YesNo,
                        new Action(async () =>
                        {
                            if (boardWindow != null)
                            {
                                boardWindow.Close();
                                boardWindow = null;
                                IsExternalWindowOpened = false;
                            }
                            if (!Room.IsPractice)
                            {
                                await Room.DisconnectAsync();
                                await _timerSocket.DisconnectAsync();
                                RemoveSignalRHubEventHandlers();
                                await App.SignalRHub.DisconnectFromRoomHub(Room.RoomId);
                            }
                            Room.CurrentTimerTime = (int)TimerCounter.TotalSeconds;
                            await Room.SaveAsync();

                            if (App.LocalServer.IsServerStarted)
                                await App.LocalServer.StopAsync();

                            await Task.Delay(1000);
                            App.Current.Shutdown();
                        }),
                        new Action(() =>
                        {

                        }));

                }
                else
                {
                    if (Room.CurrentPlayer.IsSpectator)
                    {
                        MainWindow.ShowMessage(App.Current.FindResource("mes_areyousureyouwa\r\n").ToString(), MessageNotificationType.YesNo,
                            new Action(async () =>
                            {
                                if (boardWindow != null)
                                {
                                    boardWindow.Close();
                                    boardWindow = null;
                                    IsExternalWindowOpened = false;
                                }
                                if (Room.GameExtraMode == ExtraGameMode.Changing)
                                {
                                    App.SignalRHub.OnChangeSquareRecieved -= SignalRHub_OnChangeSquareRecieved;
                                    await App.SignalRHub.DisconnectFromRoomHub(Room.RoomId);
                                }
                                App.Current.Shutdown();
                            }),
                            new Action(() =>
                            {

                            }));
                    }
                    else
                    {

                        MainWindow.ShowMessage(App.Current.FindResource("mes_yourgamenotfini").ToString(), MessageNotificationType.YesNo,
                            new Action(async () =>
                            {
                                if (boardWindow != null)
                                {
                                    boardWindow.Close();
                                    boardWindow = null;
                                    IsExternalWindowOpened = false;
                                }
                                await Room.DisconnectAsync();
                                await _timerSocket.DisconnectAsync();
                                if (Room.GameExtraMode == ExtraGameMode.Changing)
                                {
                                    App.SignalRHub.OnChangeSquareRecieved -= SignalRHub_OnChangeSquareRecieved;
                                }
                                App.SignalRHub.OnRoomTimerSettingsRecieved -= SignalRHub_OnRoomTimerSettingsRecieved;
                                App.SignalRHub.OnStartGameRecieved -= SignalRHub_OnStartGameRecieved;
                                App.SignalRHub.OnPauseGameRecieved -= SignalRHub_OnPauseGameRecieved;
                                App.SignalRHub.OnStopGameRecieved -= SignalRHub_OnStopGameRecieved;
                                App.SignalRHub.OnResumeGameRecieved -= SignalRHub_OnResumeGameRecieved;
                                await App.SignalRHub.DisconnectFromRoomHub(Room.RoomId);
                                Room.CurrentTimerTime = (int)TimerCounter.TotalSeconds;
                                await Room.SaveAsync();
                                App.Current.Shutdown();

                            }),
                            new Action(() =>
                            {

                            }));
                    }
                }
            }

        }

        #endregion

        #region BoardManipulationCommands

        [RelayCommand]
        async Task MarkSquare(Square square)
        {
            if (Room?.CurrentPlayer.IsSpectator == false)
            {
                if (square != null)
                {
                    await Room.MarkSquare(square);
                }
            }
        }

        [RelayCommand]
        async Task ChangeCollor(string color)
        {
            var bcolor = (BingoColor)Enum.Parse(typeof(BingoColor), color);
            await Room.ChangeCollor(bcolor);
        }

        [RelayCommand]
        async Task RevealBoard()
        {
            if (Room.IsPractice)
            {
                Room.IsRevealed = true;
                Room.ChatMessages.Add(new Event()
                {
                    Player = Room.CurrentPlayer,
                    Type = EventType.revealed,
                    Timestamp = DateTime.Now
                });
                Room.TriggerRevealBoard();
            }
            else
            {

                if (Room.CurrentPlayer.IsSpectator)
                {
                    await Room.RevealTheBoard();
                    Room.IsRevealed = true;
                }
                else
                {
                    if (!Room.IsAutoBoardReveal)
                    {
                        await Room.RevealTheBoard();
                        Room.IsRevealed = true;
                    }
                }
            }
        }

        [RelayCommand]
        async Task RefreshRoom()
        {
            IsRefreshing = true;
            await Room.RefreshRoomAsync();
            IsRefreshing = false;
        }

        [RelayCommand]
        async Task NewBoard()
        {
            if (Room.IsPractice)
            {
                Room.IsRevealed = false;
                var presetSquares = JsonConvert.DeserializeObject<ObservableCollection<PresetSquare>>(Room.BoardJSON);

                var rndSquares = presetSquares.OrderBy(x => Guid.NewGuid()).Take(25);
                var boardSqares = new List<Square>();

                var row = 0;
                var col = 0;
                var slot = 1;
                foreach (var square in rndSquares)
                {
                    var s = new Square()
                    {
                        Name = square.Name,
                        Row = row,
                        Column = col,
                        IsMarking = false,
                        Slot = $"slot{slot++}"
                    };
                    s.SquareColors.Add(BingoColor.blank);
                    boardSqares.Add(s);
                    col++;
                    if (col == 5)
                    {
                        col = 0;
                        row++;
                    }
                }
                Room.Board = new Board() { Squares = new ObservableCollection<Square>(boardSqares) };
                Room.TriggerNewBoardGenerated();
            }
            else
            {
                Room.IsGameStarted = false;
                IsRefreshing = true;
                await Room.GenerateNewBoardAsync();
                IsRefreshing = false;

                if (Room.IsAutoBoardReveal)
                {
                    await StopTimer();
                    await AutoBoardRevealProcess(true);
                }
            }
        }
        #endregion

        #region AppearingCommand

        [RelayCommand]
        async Task Appearing()
        {
            IsDebug = Settings.Default.IsDebug;

            MainWindow.HideSettingsButton();
            (App.Current.MainWindow as MainWindow).frame.Navigating += Frame_Navigating;
            (App.Current.MainWindow as MainWindow).Closing += RoomPageViewModel_Closing;
            Room.ChatMessages.CollectionChanged += ChatMessages_CollectionChanged;
            ChatEventsMessages = CollectionViewSource.GetDefaultView(Room.ChatMessages);
            IsFinishGameVisible = Room.IsCreatorMode || !Room.CurrentPlayer.IsSpectator;

            ChatEventsMessages.Filter = item =>
            {
                Event @event = item as Event;
                if (@event == null)
                    return false;

                return (@event.Type == (IsPlayerChat ? EventType.chat : EventType.none) ||
                        @event.Type == (IsColorChanged ? EventType.color : EventType.none) ||
                        @event.Type == (IsGoalActions ? EventType.goal : EventType.none) ||
                        @event.Type == (IsConnections ? EventType.connection : EventType.none) ||
                        @event.Type == EventType.bingo ||
                        @event.Type == EventType.win ||
                        @event.Type == EventType.newsquare ||
                        @event.Type == EventType.unhudesquare ||
                        @event.Type == EventType.revealed);
            };

            if (Room.StartDate == null)
                Room.StartDate = DateTime.Now;

            if (Room.RoomSettings.HideCard && !Room.IsGameStarted)
                Room.IsRevealed = false;


            Room.NewCardEvent += Room_NewCardEvent;
            if (Room.IsAutoBoardReveal)
            {
                IsTimerVisible = false;
                Room.IsStartTimerStarted = false;
                Room.IsAfterRevealTimerStarted = false;
                await AutoBoardRevealProcess();
            }
            else
                IsTimerVisible = true;

            if (Room.GameExtraMode == ExtraGameMode.Hidden)
            {
                Room.ProcessHiddenGame();
            }
            if (Room.GameExtraMode == ExtraGameMode.Changing)
            {
                if (!Room.IsGameStarted)
                {
                    if (Room.IsCreatorMode)
                    {
                        await App.SignalRHub.SaveRoomBoard(Room.RoomId, Room.Board.Squares);
                    }
                }
                else
                {
                    await App.SignalRHub.GetRoomBoard(Room.RoomId);
                }
            }


            localServerStarted = Properties.Settings.Default.IsStartLocalServer;
            if (localServerStarted)
            {
                App.CurrentRoom = Room;
                App.LocalServer.Start();
                App.LocalServer.OnServerStarted += (s, args) =>
                {
                    if (App.LocalSignalRHub != null)
                    {
                        App.LocalSignalRHub.OnMarkSquareRecieved += (sender, slotId) =>
                        {
                            var square = Room.Board.Squares.FirstOrDefault(s => s.Slot == slotId);
                            if (square != null)
                            {
                                App.Current.Dispatcher.Invoke(async () =>
                                {
                                    await MarkSquare(square);
                                });
                            }
                        };

                        App.LocalSignalRHub.OnRevealBoardRecieved += (sender, e) =>
                        {
                            App.Current.Dispatcher.Invoke(async () =>
                            {
                                await RevealBoard();
                            });
                        };
                    }
                };
            }
        }
        #endregion

        #region ChatAndNotificationsCommands

        [RelayCommand]
        void MuteUnmute()
        {
            BingoApp.Properties.Settings.Default.IsSoundsOn = IsSoundsOn;
            BingoApp.Properties.Settings.Default.Save();
        }

        [RelayCommand]
        async Task SendChatMessage()
        {
            if (!string.IsNullOrEmpty(ChatMessage))
            {
                await Room.SendChatMessage(ChatMessage);
                ChatMessage = "";
            }
        }

        [RelayCommand]
        void SaveSettings()
        {
            Settings.Default.IsPlayerChat = IsPlayerChat;
            Settings.Default.IsGoalActions = IsGoalActions;
            Settings.Default.IsColorChanged = IsColorChanged;
            Settings.Default.IsConnections = IsConnections;
            Settings.Default.Save();

            ChatEventsMessages.Filter = item =>
            {
                Event @event = item as Event;

                return (@event.Type == (IsPlayerChat ? EventType.chat : EventType.none) ||
                        @event.Type == (IsColorChanged ? EventType.color : EventType.none) ||
                        @event.Type == (IsGoalActions ? EventType.goal : EventType.none) ||
                        @event.Type == (IsConnections ? EventType.connection : EventType.none) ||
                        @event.Type == EventType.revealed);
            };
        }
        #endregion

        #region InviteCommands
        [RelayCommand]
        async Task InvitePlayer()
        {
            IsInvitePopupVisible = true;
            await GetAvailablePlayers();
        }

        [RelayCommand]
        async Task RefreshAvailablePlayers()
        {
            await GetAvailablePlayers();
        }

        async Task GetAvailablePlayers()
        {
            var resp = await App.RestClient.GetAvailablePlayersAsync(App.CurrentPlayer.Id);
            if (resp.IsSuccess)
            {
                AvailablePlayers.Clear();
                foreach (var item in resp.Data)
                {
                    AvailablePlayers.Add(item);
                }
            }
        }

        [RelayCommand]
        async Task SendInvite()
        {
            var jobj = new JObject();
            jobj["r"] = Room.RoomId;
            jobj["p"] = Room.PlayerCredentials.Password;
            jobj["i"] = Room.IsAutoBoardReveal ? 1 : 0;
            jobj["gm"] = Room.GameMode.ToString();
            jobj["egm"] = Room.GameExtraMode.ToString();
            jobj["pr"] = Room.PresetName?.ToString();
            jobj["g"] = Room.GameName?.ToString();

            var json = jobj.ToString();
            var data = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

            await App.RestClient.SendGameInvite(App.CurrentPlayer, SelectedPlayer, data);
            IsInvitePopupVisible = false;

            MainWindow.ShowToast(new ToastInfo() { Title = App.Current.FindResource("mes_success").ToString(), Detail = App.Current.FindResource("mes_invitesent").ToString() });

        }

        #endregion

        #region CopyLinksCommands
        [RelayCommand]
        void CopyBingoAppLink()
        {
            var jobj = new JObject();
            jobj["r"] = Room.RoomId;
            jobj["p"] = IsPasswordShare ? Room.PlayerCredentials.Password : "";
            jobj["i"] = Room.IsAutoBoardReveal ? 1 : 0;
            jobj["gm"] = Room.GameMode.ToString();
            jobj["pr"] = Room.PresetName.ToString();
            jobj["g"] = Room.GameName.ToString();

            var json = jobj.ToString();
            var data = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

            Clipboard.SetText($"http://{App.TimerSocketAddress}/openapp?d={data}");

            MainWindow.ShowToast(new ToastInfo() { Title = App.Current.FindResource("mes_success").ToString(), Detail = App.Current.FindResource("mes_linkcopied").ToString() });
        }

        [RelayCommand]
        void CopyBingosyncLink()
        {
            Clipboard.SetText($"https://bingosync.com/room/{Room.RoomId}");
            MainWindow.ShowToast(new ToastInfo() { Title = App.Current.FindResource("mes_success").ToString(), Detail = App.Current.FindResource("mes_linkcopied").ToString() });

        }

        #endregion

        #region PlayerStufCommands

        [RelayCommand]
        void PlayerClick(Player player)
        {
            Room.UpdatePlayersGoals();
            PotentialBingoColor = player.Color;
            IsPotentialBingoVisible = !IsPotentialBingoVisible;
        }
        [RelayCommand]
        void ChangeTeamView()
        {
            IsTeamView = !IsTeamView;
        }

        #endregion

        #region MiscCommands

        [RelayCommand]
        void EndGame()
        {
            MainWindow.ShowMessage(App.Current.FindResource("mes_doyourealywanttoend").ToString(), MessageNotificationType.YesNo, async () =>
            {
                if (boardWindow != null)
                {
                    boardWindow.Close();
                    boardWindow = null;
                    IsExternalWindowOpened = false;
                }

                Room.IsGameEnded = true;
                Room.EndDate = DateTime.Now;
                Room.IsTimerStarted = false;
                IsStartVisible = true;
                IsPauseVisible = false;
                Room.IsTimerStarted = false;
                _timer.Stop();
                await Room.DisconnectAsync();
                Room.CurrentTimerTime = (int)TimerCounter.TotalSeconds;
                await Room.SaveToHistoryAsync();
                Room.RemoveFromActiveRooms();

                if (App.LocalServer.IsServerStarted)
                    await App.LocalServer.StopAsync();

                MainWindow.GoBack();
            });
        }

        [RelayCommand]
        async Task SendResults()
        {
            var gameResult = new GameResult()
            {
                RoomId = Room.RoomId,
                BingoType = Room.GameMode.ToString(),
                PlayersNames = Room.RoomPlayers.Select(i => i.NickName).ToArray(),
                Score = Room.RoomPlayers.Select(i => i.SquaresCount).ToArray(),
                LinesCount = Room.RoomPlayers.Select(i => i.LinesCount).ToArray(),
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

        [RelayCommand]
        async Task ReconnectCancel()
        {

            await Room.DisconnectAsync();
            await _timerSocket.DisconnectAsync();
            RemoveSignalRHubEventHandlers();
            Room.CurrentTimerTime = (int)TimerCounter.TotalSeconds;
            await Room.SaveAsync();
            leaveunsaved = true;
            MainWindow.CloseMessage();
            (App.Current.MainWindow as MainWindow).frame.Navigating -= Frame_Navigating;
            await Task.Delay(300);
            MainWindow.GoBack();
        }

        [RelayCommand]
        void OpenExternalWindow()
        {
            if (!IsExternalWindowOpened)
            {
                IsExternalWindowOpened = true;
                boardWindow = new BoardWindow();
                boardWindow.DataContext = this;
                boardWindow.Topmost = true;
                boardWindow.Show();
            }
            else
            {
                if (boardWindow != null)
                {
                    boardWindow.Close();
                    boardWindow = null;
                    IsExternalWindowOpened = false;
                }
            }
        }

        #endregion
    }
}
