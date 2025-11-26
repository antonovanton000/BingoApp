using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HuntpointApp.Classes;
using HuntpointApp.Models;
using HuntpointApp.Popups;
using HuntpointApp.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using static HuntpointApp.Classes.HuntpointSignalRHub;

namespace HuntpointApp.ViewModels
{
    public partial class RoomPageViewModel : MyBaseViewModel
    {
        #region Construcror
        public RoomPageViewModel()
        {
            _timer = new DispatcherTimer(DispatcherPriority.Input) { Interval = new TimeSpan(0, 0, 0, 0, 100) };
            _timer.Tick += _timer_Tick;

            IsPlayerChat = HuntpointApp.Properties.Settings.Default.IsPlayerChat;
            IsGoalActions = HuntpointApp.Properties.Settings.Default.IsGoalActions;
            IsColorChanged = HuntpointApp.Properties.Settings.Default.IsColorChanged;
            IsConnections = HuntpointApp.Properties.Settings.Default.IsConnections;
            IsSoundsOn = HuntpointApp.Properties.Settings.Default.IsSoundsOn;
            SoundsVolume = HuntpointApp.Properties.Settings.Default.SoundsVolume;

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
        MediaPlayer dingPlayer;
        private Uri chatSound = new Uri(App.Location + "\\Sounds\\chat.wav", UriKind.Absolute);
        private Uri connectedSound = new Uri(App.Location + "\\Sounds\\connected.wav", UriKind.Absolute);
        private Uri disconnectedSound = new Uri(App.Location + "\\Sounds\\disconnected.wav", UriKind.Absolute);
        private Uri goalSound = new Uri(App.Location + "\\Sounds\\goal.wav", UriKind.Absolute);
        private Uri revealSound = new Uri(App.Location + "\\Sounds\\reveal.wav", UriKind.Absolute);
        private Uri bingoSound = new Uri(App.Location + "\\Sounds\\bingo.wav", UriKind.Absolute);
        private Uri beepSound = new Uri(App.Location + "\\Sounds\\beep-timer.wav", UriKind.Absolute);
        private Uri newsquareSound = new Uri(App.Location + "\\Sounds\\new-square.wav", UriKind.Absolute);
        private Uri finishSound = new Uri(App.Location + "\\Sounds\\finish.wav", UriKind.Absolute);
        private TimeSpan savedTime = TimeSpan.Zero;
        bool leaveunsaved = false;
        bool localServerStarted = false;
        DispatcherTimer newEventTimer;
        private BoardWindow? boardWindow = null;
        private SekiroProcessWatcher processWatcher;
        private MemoryReader mem;
        private SekiroEventFlags eventFlags;
        bool isSekiroInitialized = false;
        bool isFogWallOn = false;
        #endregion

        #region Events
        public event EventHandler EventAdded;
        public event EventHandler BingoHappened;
        #endregion

        #region Properties

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNightreignGame))]
        Room room = default!;

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
        public ObservableCollection<HuntpointAppPlayer> AvailablePlayers { get; set; } = [];

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
        HuntpointAppPlayer selectedPlayer;

        [ObservableProperty]
        bool isReconnecting = false;

        [ObservableProperty]
        bool isNewEventAdded = false;

        [ObservableProperty]
        Event newFeedEvent;

        [ObservableProperty]
        bool isExternalWindowOpened = false;

        [ObservableProperty]
        bool isMapSelectorVisible = false;
        public bool IsNightreignGame => Room?.RoomSettings.GameName.Contains("Nightreign", StringComparison.OrdinalIgnoreCase) ?? false;

        public bool IsSekiroGame => Room?.RoomSettings.GameName.Contains("Sekiro", StringComparison.OrdinalIgnoreCase) ?? false;

        [ObservableProperty]
        bool isSekiroAttached = false;

        #endregion

        #region TimerControlCommands

        [RelayCommand]
        async Task StartTimer()
        {
            IsStartVisible = false;
            IsPauseVisible = true;
            IsResetVisible = true;
            Room.IsTimerStarted = true;
            Room.IsTimerOnPause = false;
            Room.IsGameEnded = false;
            sw.Start();
            _timer.Start();
            if (Room.RoomSettings.IsAutoBoardReveal)
            {
                await App.SignalRHub.SendResumeGame(Room.RoomId);
            }
        }

        [RelayCommand]
        async Task PauseTimer()
        {
            IsStartVisible = true;
            IsPauseVisible = false;
            Room.IsTimerStarted = false;
            Room.IsTimerOnPause = true;
            sw.Stop();
            _timer.Stop();
            if (Room.RoomSettings.IsAutoBoardReveal)
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

        #region SignalREvents
        private void AddSignalRHubEventHandlers()
        {
            App.SignalRHub.OnPlayerConnected += SignalRHub_OnPlayerConnected;
            App.SignalRHub.OnRoomTimerSettingsRecieved += SignalRHub_OnRoomTimerSettingsRecieved;
            App.SignalRHub.OnRoomTimeSyncRecieved += SignalRHub_OnRoomTimeSyncRecieved;
            App.SignalRHub.OnStartGameRecieved += SignalRHub_OnStartGameRecieved;
            App.SignalRHub.OnPauseGameRecieved += SignalRHub_OnPauseGameRecieved;
            App.SignalRHub.OnStopGameRecieved += SignalRHub_OnStopGameRecieved;
            App.SignalRHub.OnResumeGameRecieved += SignalRHub_OnResumeGameRecieved;
            App.SignalRHub.OnDisconnected += SignalRHub_OnDisconnected;
            App.SignalRHub.OnPlayerDisconnected += SignalRHub_OnPlayerDisconnected;
            App.SignalRHub.OnPlayerReconnected += SignalRHub_OnPlayerReconnected;
            App.SignalRHub.OnReconnecting += SignalRHub_OnReconnecting;
            App.SignalRHub.OnReconnected += SignalRHub_OnReconnected;
            App.SignalRHub.OnNewEventRecieved += SignalRHub_OnNewEventRecieved;
            App.SignalRHub.OnPlayerColorChangedRecieved += SignalRHub_OnPlayerColorChangedRecieved;
            App.SignalRHub.OnMarkObjectiveRecieved += SignalRHub_OnMarkObjectiveRecieved;
            App.SignalRHub.OnUnmarkObjectiveRecieved += SignalRHub_OnUnmarkObjectiveRecieved;
            App.SignalRHub.OnNewBoardRecieved += SignalRHub_OnNewBoardRecieved;
            App.SignalRHub.OnPlayerFinishedRecieved += SignalRHub_OnPlayerFinishedRecieved;
            App.SignalRHub.OnApplySpEffectRecieved += SignalRHub_OnApplySpEffectRecieved;
        }

        private void RemoveSignalRHubEventHandlers()
        {
            App.SignalRHub.OnPlayerConnected -= SignalRHub_OnPlayerConnected;
            App.SignalRHub.OnRoomTimerSettingsRecieved -= SignalRHub_OnRoomTimerSettingsRecieved;
            App.SignalRHub.OnRoomTimeSyncRecieved -= SignalRHub_OnRoomTimeSyncRecieved;
            App.SignalRHub.OnStartGameRecieved -= SignalRHub_OnStartGameRecieved;
            App.SignalRHub.OnPauseGameRecieved -= SignalRHub_OnPauseGameRecieved;
            App.SignalRHub.OnStopGameRecieved -= SignalRHub_OnStopGameRecieved;
            App.SignalRHub.OnResumeGameRecieved -= SignalRHub_OnResumeGameRecieved;
            App.SignalRHub.OnDisconnected -= SignalRHub_OnDisconnected;
            App.SignalRHub.OnPlayerDisconnected -= SignalRHub_OnPlayerDisconnected;
            App.SignalRHub.OnPlayerReconnected -= SignalRHub_OnPlayerReconnected;

            App.SignalRHub.OnReconnecting -= SignalRHub_OnReconnecting;
            App.SignalRHub.OnReconnected -= SignalRHub_OnReconnected;
            App.SignalRHub.OnNewEventRecieved -= SignalRHub_OnNewEventRecieved;
            App.SignalRHub.OnPlayerColorChangedRecieved -= SignalRHub_OnPlayerColorChangedRecieved;
            App.SignalRHub.OnMarkObjectiveRecieved -= SignalRHub_OnMarkObjectiveRecieved;
            App.SignalRHub.OnUnmarkObjectiveRecieved -= SignalRHub_OnUnmarkObjectiveRecieved;
            App.SignalRHub.OnNewBoardRecieved -= SignalRHub_OnNewBoardRecieved;
            App.SignalRHub.OnApplySpEffectRecieved -= SignalRHub_OnApplySpEffectRecieved;
        }

        private void SignalRHub_OnApplySpEffectRecieved(object? sender, ApplySpEffectEventArgs e)
        {
            bool ok = eventFlags.WriteFlag(e.FlagId, true, 0);
            App.Logger.Info($"ApplySpEffectRecieved: flagId={e.FlagId}, Player={e.PlayerId}");

            var player = Room.Players.FirstOrDefault(i => i.Id == e.PlayerId);
            if (player != null)
            {
                App.Current.Dispatcher.Invoke(async () =>
                {
                    await player.AnimateCaster();
                });
            }
        }

        private void SignalRHub_OnPlayerFinishedRecieved(object? sender, string e)
        {
            var player = Room.Players.FirstOrDefault(i => i.Id == e);
            if (player != null)
            {
                player.IsFinished = true;
            }
            if (Room.CurrentPlayer.Id == e)
            {
                Room.CurrentPlayer.IsFinished = true;
            }

            if (!Room.ActualPlayers.Any(i => !i.IsFinished))
            {
                Room.IsGameEnded = true;
                Room.IsTimerOnPause = true;
                sw.Stop();
                _timer.Stop();
            }
        }

        private async void SignalRHub_OnNewBoardRecieved(object? sender, Board e)
        {
            Room.Board = e;
            if (Room.RoomSettings.IsAutoBoardReveal)
            {
                Room.IsGameStarted = false;
                await AutoBoardRevealProcess(true);
            }
            await Room.SaveAsync();
        }

        private void SignalRHub_OnUnmarkObjectiveRecieved(object? sender, HuntpointSignalRHub.MarkObjectiveEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var obj = Room.Board.Objectives.FirstOrDefault(i => i.Slot == e.Slot);
                if (obj != null)
                {
                    obj.IsMarking = false;
                    obj.ObjectiveColors.Remove(e.Color);
                    if (obj.ObjectiveColors.Count == 0 && obj.Type == ObjectiveType.Legendary)
                    {
                        var players = Room.Players.Where(i => i.Color == e.Color);
                        foreach (var item in players)
                        {
                            item.IsFirstLegendaryMarked = false;
                        }
                    }
                }
                Room.UpdatePlayersGoals();
            });
        }

        private void SignalRHub_OnMarkObjectiveRecieved(object? sender, HuntpointSignalRHub.MarkObjectiveEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var obj = Room.Board.Objectives.FirstOrDefault(i => i.Slot == e.Slot);
                if (obj != null)
                {
                    obj.IsMarking = false;
                    obj.ObjectiveColors.Add(e.Color);
                    if (obj.ObjectiveColors.Count == 1 && obj.Type == ObjectiveType.Legendary)
                    {
                        var players = Room.Players.Where(i => i.Color == e.Color);
                        foreach (var item in players)
                        {
                            item.IsFirstLegendaryMarked = true;
                        }
                    }

                }
                Room.UpdatePlayersGoals();
            });
        }

        private void SignalRHub_OnPlayerColorChangedRecieved(object? sender, HuntpointSignalRHub.PlayerColorChangedEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var player = Room.Players.FirstOrDefault(i => i.Id == e.PlayerId);
                if (player != null)
                {
                    player.Color = e.Color;
                }
                if (Room.CurrentPlayer.Id == e.PlayerId)
                {
                    Room.CurrentPlayer.Color = e.Color;
                }
                Room.UpdatePlayerTeams();
                Room.UpdatePlayersGoals();
            });
        }

        private void SignalRHub_OnPlayerConnected(object? sender, Player e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                if (!Room.Players.Any(i => i.Id == e.Id))
                {
                    e.IsCurrentPlayer = e.Id == Room.CurrentPlayer.Id;
                    Room.Players.Add(e);
                    Room.RoomPlayers.Add(e);
                }

                if (e.Id == App.CurrentPlayer.Id)
                {
                    Room.IsConnectedToServer = true;
                    IsConnected = true;
                }
                Room.UpdatePlayersGoals();
            });
        }

        private void SignalRHub_OnNewEventRecieved(object? sender, Event e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                Room.ChatMessages.Add(e);
            });
        }

        private void SignalRHub_OnReconnected(object? sender, string e)
        {
            IsReconnecting = false;
        }

        private void SignalRHub_OnReconnecting(object? sender, string e)
        {
            IsReconnecting = true;
        }

        private void SignalRHub_OnPlayerDisconnected(object? sender, string playerId)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var player = Room.Players.FirstOrDefault(i => i.Id == playerId);
                if (player != null)
                {
                    Room.Players.Remove(player);
                    sw.Stop();
                    _timer.Stop();
                    IsStartVisible = true;
                    IsPauseVisible = false;
                    Room.IsTimerStarted = false;
                    dingPlayer.Open(disconnectedSound);
                    dingPlayer.Volume = SoundsVolume * 0.01d;
                    dingPlayer.Play();
                }
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

        #endregion

        #region TimerEvents
        private async void _timer_Tick(object? sender, EventArgs e)
        {
            TimerCounter = (sw.Elapsed + savedTime);
            Room.CurrentTimerTime = (int)TimerCounter.TotalSeconds;
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
                    if (IsSekiroGame)
                    {
                        if (AfterRevealSeconds - (int)TimerCounter.TotalSeconds <= 2)
                        {
                            await RemoveFogWall();
                        }
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
                        if (Room.RoomSettings.ExtraGameMode == ExtraGameMode.Hidden || Room.RoomSettings.ExtraGameMode == ExtraGameMode.Changing)
                        {
                            if (Room.IsCreatorMode)
                                IsTimerButtonsVisible = true;
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

            if (Room.IsGameStarted)
            {
                TimerCounter = TimeSpan.FromSeconds(Room.CurrentTimerTime);
                savedTime = TimeSpan.FromSeconds(Room.CurrentTimerTime);
                Room.IsTimerStarted = false;
                IsTimerVisible = true;

                IsTimerButtonsVisible = true;
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
                    switch (Room.RoomSettings.ExtraGameMode)
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

        private async void SignalRHub_OnResumeGameRecieved(object? sender, string e)
        {
            if (e != Room.RoomId) return;
            Room.IsTimerOnPause = false;
            Room.IsGameEnded = false;
            sw.Start();
            _timer.Start();
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
        }

        private async void SignalRHub_OnPauseGameRecieved(object? sender, string e)
        {
            if (e != Room.RoomId) return;

            Room.IsTimerOnPause = true;
            sw.Stop();
            _timer.Stop();
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

            if (IsSekiroGame)
                await AddFogWall();
        }

        private void SignalRHub_OnRoomTimerSettingsRecieved(object? sender, HuntpointSignalRHub.RoomTimerSettingsEventArgs e)
        {
            if (e.RoomId != Room.RoomId) return;

            StartTimeSeconds = e.StartTime;
            AfterRevealSeconds = e.AfterRevealTime;
            UnhideTimeMinutes = e.UnhideTime;
            ChangeTimeMinutes = e.ChangeTime;
        }

        private void SignalRHub_OnRoomTimeSyncRecieved(object? sender, HuntpointSignalRHub.RoomTimeSyncEventArgs e)
        {
            if (e.RoomId != Room.RoomId) return;

            Room.CurrentTimerTime = e.CurrentTime;
            sw.Reset();
            savedTime = TimeSpan.FromSeconds(e.CurrentTime);
            TimerCounter = savedTime;
        }

        private async void SignalRHub_OnPlayerReconnected(object? sender, Player player)
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
            IsMapSelectorVisible = false;
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
                                        RemoveSignalRHubEventHandlers();
                                        await Room.DisconnectAsync();
                                    }
                                    Room.CurrentTimerTime = (int)TimerCounter.TotalSeconds;
                                    await Room.SaveAsync();

                                    //if (App.LocalServer.IsServerStarted)
                                    //    await App.LocalServer.StopAsync();                                    
                                    if (IsNightreignGame)
                                    {
                                        RemoveHotKeys();
                                    }
                                    Room.ChatMessages.CollectionChanged -= ChatMessages_CollectionChanged;
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
                                        //if (boardWindow != null)
                                        //{
                                        //    boardWindow.Close();
                                        //    boardWindow = null;
                                        //    IsExternalWindowOpened = false;
                                        //}
                                        leaveunsaved = true;
                                        if (Room.RoomSettings.ExtraGameMode == ExtraGameMode.Changing)
                                        {
                                            //App.SignalRHub.OnChangeSquareRecieved -= SignalRHub_OnChangeSquareRecieved;
                                            await App.SignalRHub.DisconnectFromRoomHub(Room.RoomId);
                                        }
                                        if (IsNightreignGame)
                                        {
                                            RemoveHotKeys();
                                        }
                                        Room.ChatMessages.CollectionChanged -= ChatMessages_CollectionChanged;

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
                                        Room.CurrentTimerTime = (int)TimerCounter.TotalSeconds;
                                        await Room.SaveAsync();
                                        leaveunsaved = true;
                                        MainWindow.CloseMessage();
                                        if (IsNightreignGame)
                                        {
                                            RemoveHotKeys();
                                        }
                                        Room.ChatMessages.CollectionChanged -= ChatMessages_CollectionChanged;
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
                else
                {
                    if (IsNightreignGame)
                    {
                        RemoveHotKeys();
                    }
                    Room.ChatMessages.CollectionChanged -= ChatMessages_CollectionChanged;
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
                if (IsRefreshing) return;
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
                                        soundPath = chatSound;
                                        break;
                                    case EventType.none:
                                        break;
                                    case EventType.newsquare:
                                        soundPath = newsquareSound;
                                        break;
                                    case EventType.unhudesquare:
                                        soundPath = revealSound;
                                        break;
                                    case EventType.finish:
                                        soundPath = finishSound;
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
                            var filteredEventTypes = new List<EventType>() { EventType.Huntpoint, EventType.win, EventType.newsquare, EventType.unhudesquare };

                            if (IsPlayerChat)
                                filteredEventTypes.Add(EventType.chat);

                            if (IsColorChanged)
                                filteredEventTypes.Add(EventType.color);

                            if (IsGoalActions)
                                filteredEventTypes.Add(EventType.goal);

                            if (IsConnections)
                                filteredEventTypes.Add(EventType.connection);

                            if (filteredEventTypes.Any(i => i == newEvent.Type))
                            {

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
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText(System.IO.Path.Combine(App.Location, "errors.log"), ex.Message + "\r\n");
            }
        }

        private void RoomPageViewModel_Closing(object? sender, CancelEventArgs e)
        {
            IsMapSelectorVisible = false;
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
                                RemoveSignalRHubEventHandlers();
                                await Room.DisconnectAsync();
                            }
                            Room.CurrentTimerTime = (int)TimerCounter.TotalSeconds;
                            await Room.SaveAsync();
                            if (Room.RoomSettings.GameName.Contains("Nightreign", StringComparison.OrdinalIgnoreCase))
                            {
                                RemoveHotKeys();
                            }
                            //if (App.LocalServer.IsServerStarted)
                            //    await App.LocalServer.StopAsync();
                            Room.ChatMessages.CollectionChanged -= ChatMessages_CollectionChanged;
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
                                await Room.DisconnectAsync();
                                if (Room.RoomSettings.GameName.Contains("Nightreign", StringComparison.OrdinalIgnoreCase))
                                {
                                    RemoveHotKeys();
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
                                if (Room.RoomSettings.ExtraGameMode == ExtraGameMode.Changing)
                                {
                                    //App.SignalRHub.OnChangeSquareRecieved -= SignalRHub_OnChangeSquareRecieved;
                                }
                                if (Room.RoomSettings.GameName.Contains("Nightreign", StringComparison.OrdinalIgnoreCase))
                                {
                                    RemoveHotKeys();
                                }
                                //App.SignalRHub.OnRoomTimerSettingsRecieved -= SignalRHub_OnRoomTimerSettingsRecieved;                                                                
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
        async Task MarkObjective(Objective objective)
        {
            if (Room.CurrentPlayer.IsFinished) return;
            if (Room.CurrentPlayer.IsSpectator) return;

            if (objective != null)
            {
                await Room.MarkObjective(objective);
            }
        }

        [RelayCommand]
        async Task ChangeCollor(string color)
        {
            var bcolor = (HuntpointColor)Enum.Parse(typeof(HuntpointColor), color);
            await Room.ChangeColor(bcolor);
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
                    if (!Room.RoomSettings.IsAutoBoardReveal)
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
                Room.ChatMessages.Add(new Event() { Type = EventType.newcard, Player = Room.CurrentPlayer });
                Room.GeneratePracticeBoard();
            }
            else
            {
                Room.IsGameStarted = false;
                IsRefreshing = true;
                await Room.GenerateNewBoardAsync();
                IsRefreshing = false;

                if (Room.RoomSettings.IsAutoBoardReveal)
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
            if (!Room.IsPractice)
            {
                AddSignalRHubEventHandlers();
            }

            await Room.InitRoom();
            var preferedColor = HuntpointApp.Properties.Settings.Default.PreferedColor;
            if (!string.IsNullOrEmpty(preferedColor))
            {
                var color = (HuntpointColor)Enum.Parse(typeof(HuntpointColor), preferedColor);
                await Room.ChangeColor(color);
            }

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
                        @event.Type == EventType.finish ||
                        @event.Type == EventType.newcard ||
                        @event.Type == EventType.win ||
                        @event.Type == EventType.newsquare ||
                        @event.Type == EventType.unhudesquare ||
                        @event.Type == EventType.revealed);
            };

            if (Room.StartDate == null)
                Room.StartDate = DateTime.Now;

            if (Room.RoomSettings.HideCard && !Room.IsGameStarted)
                Room.IsRevealed = false;

            if (Room.RoomSettings.IsAutoBoardReveal)
            {
                IsTimerVisible = false;
                Room.IsStartTimerStarted = false;
                Room.IsAfterRevealTimerStarted = false;
                await AutoBoardRevealProcess();
            }
            else
                IsTimerVisible = true;

            if (IsNightreignGame)
            {
                InitStormTimer();
                if (Room.RoomSettings.IsSameSeedGeneration)
                    SetSeed();
            }
            if (IsSekiroGame)
            {
                InitSekiroProcessing();
            }

        }
        #endregion

        #region ChatAndNotificationsCommands

        [RelayCommand]
        void MuteUnmute()
        {
            HuntpointApp.Properties.Settings.Default.IsSoundsOn = IsSoundsOn;
            HuntpointApp.Properties.Settings.Default.Save();
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
                        @event.Type == EventType.finish ||
                        @event.Type == EventType.newcard ||
                        @event.Type == EventType.revealed ||
                        @event.Type == EventType.win ||
                        @event.Type == EventType.newsquare ||
                        @event.Type == EventType.unhudesquare ||
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
            jobj["p"] = Room.Password;
            jobj["i"] = Room.RoomSettings.IsAutoBoardReveal ? 1 : 0;
            jobj["egm"] = Room.RoomSettings.ExtraGameMode.ToString();
            jobj["pr"] = Room.RoomSettings.PresetName?.ToString();
            jobj["g"] = Room.RoomSettings.GameName?.ToString();

            var json = jobj.ToString();
            await App.RestClient.SendGameInvite(App.CurrentPlayer, SelectedPlayer, json);

            IsInvitePopupVisible = false;

            MainWindow.ShowToast(new ToastInfo() { Title = App.Current.FindResource("mes_success").ToString(), Detail = App.Current.FindResource("mes_invitesent").ToString() });

        }

        #endregion

        #region PlayerStufCommands

        [RelayCommand]
        void PlayerClick(Player player)
        {
            Room.UpdatePlayersGoals();
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
                //if (boardWindow != null)
                //{
                //    boardWindow.Close();
                //    boardWindow = null;
                //    IsExternalWindowOpened = false;
                //}

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

                //if (App.LocalServer.IsServerStarted)
                //    await App.LocalServer.StopAsync();

                MainWindow.GoBack();
            });
        }

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

        [RelayCommand]
        async Task ReconnectCancel()
        {

            await Room.DisconnectAsync();
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
                boardWindow.Closed += (s, e) =>
                {
                    boardWindow = null;
                    IsExternalWindowOpened = false;
                };
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

        [RelayCommand]
        void ShowMapPatternSelect()
        {
            IsMapSelectorVisible = !IsMapSelectorVisible;
        }

        [RelayCommand]
        void CloseMapPatternSelect()
        {
            IsMapSelectorVisible = false;
        }

        [RelayCommand]
        void CopyRoomId()
        {
            Clipboard.SetText(Room.RoomId);
            MainWindow.ShowToast(new ToastInfo() { Title = App.Current.FindResource("mes_success").ToString(), Detail = App.Current.FindResource("mes_roomidcopysuccess").ToString() });
        }

        [RelayCommand]
        async Task FinishRun()
        {
            await App.SignalRHub.SendFinishRun(Room.RoomId, Room.CurrentPlayer.Id);
        }

        #endregion

        #region StormTimer

        [ObservableProperty]
        bool isStormTimerVisible;

        [ObservableProperty]
        StormState currentStormState = 0;

        public string StormTimer
        {
            get
            {
                var ts = TimeSpan.Zero;
                var setTime = TimeSpan.FromSeconds(0);
                switch (CurrentStormState)
                {
                    case StormState.Day1Start:
                        setTime = TimeSpan.FromSeconds(270);
                        ts = setTime - (stormTimerStopwatch?.Elapsed ?? TimeSpan.Zero);
                        break;
                    case StormState.Day1Shrink1:
                        setTime = TimeSpan.FromSeconds(180);
                        ts = setTime - (stormTimerStopwatch?.Elapsed ?? TimeSpan.Zero);
                        break;
                    case StormState.Day1AfterShrink:
                        setTime = TimeSpan.FromSeconds(210);
                        ts = setTime - (stormTimerStopwatch?.Elapsed ?? TimeSpan.Zero);
                        break;
                    case StormState.Day1Shrink2:
                        setTime = TimeSpan.FromSeconds(180);
                        ts = setTime - (stormTimerStopwatch?.Elapsed ?? TimeSpan.Zero);
                        break;
                    case StormState.BossFight1:
                        ts = TimeSpan.Zero;
                        break;
                    case StormState.Day2Start:
                        setTime = TimeSpan.FromSeconds(270);
                        ts = setTime - (stormTimerStopwatch?.Elapsed ?? TimeSpan.Zero);
                        break;
                    case StormState.Day2Shrink1:
                        setTime = TimeSpan.FromSeconds(180);
                        ts = setTime - (stormTimerStopwatch?.Elapsed ?? TimeSpan.Zero);
                        break;
                    case StormState.Day2AfterShrink:
                        setTime = TimeSpan.FromSeconds(210);
                        ts = setTime - (stormTimerStopwatch?.Elapsed ?? TimeSpan.Zero);
                        break;
                    case StormState.Day2Shrink2:
                        setTime = TimeSpan.FromSeconds(180);
                        ts = setTime - (stormTimerStopwatch?.Elapsed ?? TimeSpan.Zero);
                        break;
                    case StormState.BossFight2:
                        ts = TimeSpan.Zero;
                        break;
                    default:
                        break;
                }
                return $"{ts.Minutes:D2}:{ts.Seconds:D2}";
            }
        }

        Stopwatch stormTimerStopwatch;
        DispatcherTimer stormTimer;

        void ProcessStormState(TimeSpan timeSpan)
        {
            switch (CurrentStormState)
            {
                case StormState.Day1Start:
                    {
                        if (timeSpan == TimeSpan.Zero)
                        {
                            IsStormTimerVisible = true;
                        }
                        else if (timeSpan.TotalSeconds >= 270) // 04:30
                        {
                            CurrentStormState = StormState.Day1Shrink1;
                            stormTimerStopwatch.Restart();

                        }
                    }
                    break;
                case StormState.Day1Shrink1:
                    {
                        if (timeSpan.TotalSeconds >= 180) // 03:00
                        {
                            CurrentStormState = StormState.Day1AfterShrink;
                            stormTimerStopwatch.Restart();
                        }
                    }
                    break;
                case StormState.Day1AfterShrink:
                    {
                        if (timeSpan.TotalSeconds >= 210) // 03:30
                        {
                            CurrentStormState = StormState.Day1Shrink2;
                            stormTimerStopwatch.Restart();
                        }
                    }
                    break;
                case StormState.Day1Shrink2:
                    {
                        if (timeSpan.TotalSeconds >= 180) // 03:00
                        {
                            CurrentStormState = StormState.BossFight1;
                            IsStormTimerVisible = false;
                            stormTimer.Stop();
                            stormTimerStopwatch.Reset();
                            stormTimerStopwatch.Stop();
                        }
                    }
                    break;
                case StormState.BossFight1:
                    {
                        if (timeSpan == TimeSpan.Zero)
                        {
                            CurrentStormState = StormState.Day2Start;
                            stormTimerStopwatch.Restart();
                            stormTimer.Start();
                        }
                    }
                    break;
                case StormState.Day2Start:
                    if (timeSpan == TimeSpan.Zero)
                    {
                        IsStormTimerVisible = true;
                    }
                    else if (timeSpan.TotalSeconds >= 270) // 04:30
                    {
                        CurrentStormState = StormState.Day2Shrink1;
                        stormTimerStopwatch.Restart();
                    }
                    break;
                case StormState.Day2Shrink1:
                    if (timeSpan.TotalSeconds >= 180) // 03:00
                    {
                        CurrentStormState = StormState.Day2AfterShrink;
                        stormTimerStopwatch.Restart();
                    }
                    break;
                case StormState.Day2AfterShrink:
                    if (timeSpan.TotalSeconds >= 210) // 03:30
                    {
                        CurrentStormState = StormState.Day2Shrink2;
                        stormTimerStopwatch.Restart();
                    }
                    break;
                case StormState.Day2Shrink2:
                    if (timeSpan.TotalSeconds >= 180) // 03:00
                    {
                        CurrentStormState = StormState.BossFight2;
                        stormTimerStopwatch.Restart();
                    }
                    break;
                case StormState.BossFight2:
                    if (timeSpan.TotalSeconds >= 0) // Boss fight duration is not defined, so we just keep it as is.
                    {
                        // You can add logic here for what happens after the second boss fight.
                        IsStormTimerVisible = false; // Hide the storm timer after the second boss fight.
                    }
                    break;
                default:
                    break;
            }
        }

        void InitStormTimer()
        {
            InitHotKeys();
            stormTimerStopwatch = new Stopwatch();
            stormTimer = new DispatcherTimer(DispatcherPriority.Background) { Interval = TimeSpan.FromMicroseconds(200) };
            stormTimer.Tick += (s, e) =>
            {
                OnPropertyChanged(nameof(StormTimer));
                ProcessStormState(stormTimerStopwatch.Elapsed);
            };
        }

        #endregion

        #region HotKeys
        private HotKeyHost _hotKeyHost;
        private HotKey startStormTimerHotKey;
        private HotKey skipStepStormTimerHotKey;
        private HotKey stopStormTimerHotKey;

        private void InitHotKeys()
        {
            _hotKeyHost = new HotKeyHost((HwndSource)HwndSource.FromVisual(App.Current.MainWindow));
            startStormTimerHotKey = new HotKey(System.Windows.Input.Key.F1, System.Windows.Input.ModifierKeys.Shift);
            startStormTimerHotKey.HotKeyPressed += (s, e) =>
            {
                stormTimer.Start();
                if (CurrentStormState == StormState.Day1Start || CurrentStormState == StormState.BossFight1)
                {
                    IsStormTimerVisible = true;
                    ProcessStormState(TimeSpan.Zero);
                }
                stormTimerStopwatch.Start();
            };
            skipStepStormTimerHotKey = new HotKey(System.Windows.Input.Key.F2, System.Windows.Input.ModifierKeys.Shift);
            skipStepStormTimerHotKey.HotKeyPressed += (s, e) =>
            {
                CurrentStormState = (StormState)(((int)CurrentStormState + 1) % Enum.GetValues(typeof(StormState)).Length);
                if (CurrentStormState == StormState.BossFight1 || CurrentStormState == StormState.BossFight2)
                {
                    IsStormTimerVisible = false;
                    stormTimer.Stop();
                    stormTimerStopwatch.Reset();
                    stormTimerStopwatch.Stop();
                }
                else
                {
                    stormTimerStopwatch.Restart();
                    ProcessStormState(stormTimerStopwatch.Elapsed);
                }
            };

            stopStormTimerHotKey = new HotKey(System.Windows.Input.Key.F3, System.Windows.Input.ModifierKeys.Shift);
            stopStormTimerHotKey.HotKeyPressed += (s, e) =>
            {
                IsStormTimerVisible = false;
                stormTimer.Stop();
                stormTimerStopwatch.Stop();
                stormTimerStopwatch.Reset();
            };

            _hotKeyHost.AddHotKey(startStormTimerHotKey);
            _hotKeyHost.AddHotKey(skipStepStormTimerHotKey);
            _hotKeyHost.AddHotKey(stopStormTimerHotKey);
        }

        private void RemoveHotKeys()
        {
            _hotKeyHost.RemoveHotKey(startStormTimerHotKey);
            _hotKeyHost.RemoveHotKey(skipStepStormTimerHotKey);
            _hotKeyHost.RemoveHotKey(stopStormTimerHotKey);
            _hotKeyHost.Dispose();
        }
        #endregion

        #region NightreignSeedSetter

        public void SetSeed()
        {
            var nightreignPath = Settings.Default.NightreignExePath;
            if (string.IsNullOrEmpty(nightreignPath))
            {
                MainWindow.ShowToast(new ToastInfo(App.Current.FindResource("mes_error").ToString(), App.Current.FindResource("mes_nonightreignpath").ToString(), ToastType.Error));
                return;
            }

            var nightreignFolderPath = System.IO.Path.GetDirectoryName(nightreignPath);
            if (string.IsNullOrEmpty(nightreignFolderPath))
                return;

            var iniPath = System.IO.Path.Combine(nightreignFolderPath, "nighreign-helper", "NightreignRandomizerHelper_config.ini");
            var logPath = System.IO.Path.Combine(nightreignFolderPath, "nighreign-helper", "log.txt");

            if (Room.NightreignSeed == null)
            {
                MainWindow.ShowToast(new ToastInfo(App.Current.FindResource("mes_error").ToString(), App.Current.FindResource("mes_sharedseedretrieveerror").ToString(), ToastType.Error));
                return;
            }

            var seed = Room.NightreignSeed;
            var hexSeed = $"0x{seed:X8}";
            var configFileContent = System.IO.File.ReadAllLines(iniPath).ToList();
            var patchSeedLineString = configFileContent.FirstOrDefault(i => i.StartsWith("patchSeed"));
            var seedLineString = configFileContent.FirstOrDefault(i => i.StartsWith("seed"));
            if (patchSeedLineString == null || seedLineString == null)
            {
                MainWindow.ShowToast(new ToastInfo(App.Current.FindResource("mes_error").ToString(), App.Current.FindResource("mes_configfilerror").ToString(), ToastType.Error));
                return;
            }
            var patchSeedLineIndex = configFileContent.IndexOf(patchSeedLineString);
            var seedLineIndex = configFileContent.IndexOf(seedLineString);
            configFileContent[patchSeedLineIndex] = $"patchSeed = true";
            configFileContent[seedLineIndex] = $"seed = {hexSeed}";

            System.IO.File.WriteAllLines(iniPath, configFileContent);
            System.IO.File.AppendAllText(logPath, $"Seed setted to {hexSeed} at {DateTime.Now}\r\n");
        }

        #endregion

        #region Sekiro
        void InitSekiroProcessing()
        {
            mem = new MemoryReader();
            processWatcher = new SekiroProcessWatcher(mem);
            processWatcher.OnAttach += async () =>
            {
                eventFlags = new SekiroEventFlags(mem);
                while (!eventFlags.IsReady)
                {
                    if (!eventFlags.InitByAbsolutePointer())
                    {
                        eventFlags.InitBySignature();
                    }
                    await Task.Delay(1000);
                }
                await StartEventFlagsWatcher();
                App.Current.Dispatcher.Invoke(() =>
                {
                    IsSekiroAttached = true;
                });
            };

            processWatcher.OnDetach += () =>
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    cts?.Cancel();
                    IsSekiroAttached = false;
                });
            };

            processWatcher.Start();

        }

        private CancellationTokenSource? cts;
        async Task StartEventFlagsWatcher()
        {
            cts = new CancellationTokenSource();
            var tf = new TaskFactory();
            await tf.StartNew(() => FlagsWatcher(cts.Token));
        }

        bool f101state;
        bool prev_f101state;
        bool f103state;
        bool prev_f103state;
        bool f105state;
        bool prev_f105state;
        bool f107state;
        bool prev_f107state;
        bool f109state;
        bool prev_f109state;
        bool f111state;
        bool prev_f111state;
        bool f113state;
        bool prev_f113state;
        bool f115state;
        bool prev_f115state;
        bool f125state;
        bool prev_f125state;
        bool f117state;
        bool prev_f117state;

        async Task FlagsWatcher(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    f101state = eventFlags.ReadFlag(101);
                    f103state = eventFlags.ReadFlag(103);
                    f105state = eventFlags.ReadFlag(105);
                    f107state = eventFlags.ReadFlag(107);
                    f109state = eventFlags.ReadFlag(109);
                    f111state = eventFlags.ReadFlag(111);
                    f113state = eventFlags.ReadFlag(113);
                    f115state = eventFlags.ReadFlag(115);
                    f125state = eventFlags.ReadFlag(125);
                    f117state = eventFlags.ReadFlag(117);

                    //if (Settings.Default.IsDebug)
                    //{
                    //    await System.IO.File.AppendAllTextAsync("flags.log", $"{DateTime.Now:HH:mm:ss.fff} | 101={f101state}, 103={f103state}, 105={f105state}, 107={f107state}, 109={f109state}, 111={f111state}, 113={f113state}, 115={f115state}{Environment.NewLine}");
                    //    await System.IO.File.AppendAllTextAsync("flags.log", $"{DateTime.Now:HH:mm:ss.fff} | pre={prev_f101state}, pre={prev_f103state}, pre={prev_f105state}, pre={prev_f107state}, pre={prev_f109state}, pre={prev_f111state}, pre={prev_f113state}, pre={prev_f115state}{Environment.NewLine}");
                    //}
                    if (f101state == true && prev_f101state == false)
                    {
                        App.Logger.Info($"Flag 101 = ON Send ApplySpEffectAsync: roomId:{Room.RoomId}, PlayerId: {Room.CurrentPlayer.Id}, FlagId: 102");
                        await App.Current.Dispatcher.Invoke(async () =>
                        {
                            await App.SignalRHub.ApplySpEffectAsync(Room.RoomId, Room.CurrentPlayer.Id, 102);
                            await Room.CurrentPlayer.AnimateCaster();
                        });

                    }
                    if (f103state == true && prev_f103state == false)
                    {
                        App.Logger.Info($"Flag 103 = ON Send ApplySpEffectAsync: roomId:{Room.RoomId}, PlayerId: {Room.CurrentPlayer.Id}, FlagId: 103");
                        await App.Current.Dispatcher.Invoke(async () =>
                        {
                            await App.SignalRHub.ApplySpEffectAsync(Room.RoomId, Room.CurrentPlayer.Id, 104);
                            await Room.CurrentPlayer.AnimateCaster();
                        });
                    }
                    if (f105state == true && prev_f105state == false)
                    {
                        App.Logger.Info($"Flag 105 = ON Send ApplySpEffectAsync: roomId:{Room.RoomId}, PlayerId: {Room.CurrentPlayer.Id}, FlagId: 105");
                        await App.Current.Dispatcher.Invoke(async () =>
                        {
                            await App.SignalRHub.ApplySpEffectAsync(Room.RoomId, Room.CurrentPlayer.Id, 106);
                            await Room.CurrentPlayer.AnimateCaster();
                        });
                    }
                    if (f107state == true && prev_f107state == false)
                    {
                        App.Logger.Info($"Flag 107 = ON Send ApplySpEffectAsync: roomId:{Room.RoomId}, PlayerId: {Room.CurrentPlayer.Id}, FlagId: 107");
                        await App.Current.Dispatcher.Invoke(async () =>
                        {
                            await App.SignalRHub.ApplySpEffectAsync(Room.RoomId, Room.CurrentPlayer.Id, 108);
                            await Room.CurrentPlayer.AnimateCaster();
                        });
                    }
                    if (f109state == true && prev_f109state == false)
                    {
                        App.Logger.Info($"Flag 109 = ON Send ApplySpEffectAsync: roomId:{Room.RoomId}, PlayerId: {Room.CurrentPlayer.Id}, FlagId: 109");
                        await App.Current.Dispatcher.Invoke(async () =>
                        {
                            await App.SignalRHub.ApplySpEffectAsync(Room.RoomId, Room.CurrentPlayer.Id, 110);
                            await Room.CurrentPlayer.AnimateCaster();
                        });
                    }
                    if (f111state == true && prev_f111state == false)
                    {
                        App.Logger.Info($"Flag 111 = ON Send ApplySpEffectAsync: roomId:{Room.RoomId}, PlayerId: {Room.CurrentPlayer.Id}, FlagId: 111");
                        await App.Current.Dispatcher.Invoke(async () =>
                        {
                            await App.SignalRHub.ApplySpEffectAsync(Room.RoomId, Room.CurrentPlayer.Id, 112);
                            await Room.CurrentPlayer.AnimateCaster();
                        });
                    }
                    if (f113state == true && prev_f113state == false)
                    {
                        App.Logger.Info($"Flag 113 = ON Send ApplySpEffectAsync: roomId:{Room.RoomId}, PlayerId: {Room.CurrentPlayer.Id}, FlagId: 113");
                        await App.Current.Dispatcher.Invoke(async () =>
                        {
                            await App.SignalRHub.ApplySpEffectAsync(Room.RoomId, Room.CurrentPlayer.Id, 114);
                            await Room.CurrentPlayer.AnimateCaster();
                        });
                    }
                    if (f115state == true && prev_f115state == false)
                    {
                        App.Logger.Info($"Flag 115 = ON Send ApplySpEffectAsync: roomId:{Room.RoomId}, PlayerId: {Room.CurrentPlayer.Id}, FlagId: 115");
                        await App.Current.Dispatcher.Invoke(async () =>
                        {
                            await App.SignalRHub.ApplySpEffectAsync(Room.RoomId, Room.CurrentPlayer.Id, 116);
                            await Room.CurrentPlayer.AnimateCaster();
                        });
                    }
                    if (f125state == true && prev_f125state == false)
                    {
                        App.Logger.Info($"Flag 125 = ON Send Finish Run");
                        await App.Current.Dispatcher.Invoke(async () =>
                        {
                            await App.SignalRHub.SendFinishRun(Room.RoomId, Room.CurrentPlayer.Id);
                        });
                    }

                    if (Room.RoomSettings.IsForFirstDeath)
                    {
                        if (f117state == false)
                        {
                            eventFlags.WriteFlag(117, true, 0);
                        }
                    }
                    else
                    {
                        if (f117state == true)
                        {
                            eventFlags.WriteFlag(117, false, 0);
                        }

                    }

                    prev_f101state = f101state;
                    prev_f103state = f103state;
                    prev_f105state = f105state;
                    prev_f107state = f107state;
                    prev_f109state = f109state;
                    prev_f111state = f111state;
                    prev_f113state = f113state;
                    prev_f115state = f115state;
                    prev_f125state = f125state;
                    prev_f117state = f117state;

                }
                catch (Exception ex)
                {
                    App.Logger.Error($"❌ FlagsWatchLoop error: {ex.Message}");
                }
                try
                {

                    await Task.Delay(500, token); // проверка два раза в секунду
                }
                catch { }
            }
        }


        #endregion

        #region FogWall

        async Task AddFogWall()
        {
            try
            {
                var sekiroExePath = Properties.Settings.Default.SekiroExePath;
                if (string.IsNullOrEmpty(sekiroExePath) || !System.IO.File.Exists(sekiroExePath))
                {
                    return;
                }
                var sekiroFolder = System.IO.Path.GetDirectoryName(sekiroExePath);
                if (string.IsNullOrEmpty(sekiroFolder) || !System.IO.Directory.Exists(sekiroFolder))
                {
                    return;
                }
                var fogWallFilePath = System.IO.Path.Combine(sekiroFolder, "huntpointmod", "fogwall.cfg");
                await System.IO.File.WriteAllTextAsync(fogWallFilePath, "FogWall=ON");
                isFogWallOn = true;
            }
            catch (Exception ex)
            {

            }
        }

        async Task RemoveFogWall()
        {
            try
            {
                if (isFogWallOn == false)
                    return;
                var sekiroExePath = Properties.Settings.Default.SekiroExePath;
                if (string.IsNullOrEmpty(sekiroExePath) || !System.IO.File.Exists(sekiroExePath))
                {
                    return;
                }
                var sekiroFolder = System.IO.Path.GetDirectoryName(sekiroExePath);
                if (string.IsNullOrEmpty(sekiroFolder) || !System.IO.Directory.Exists(sekiroFolder))
                {
                    return;
                }
                var fogWallFilePath = System.IO.Path.Combine(sekiroFolder, "huntpointmod", "fogwall.cfg");
                await System.IO.File.WriteAllTextAsync(fogWallFilePath, "FogWall=OFF");
                isFogWallOn = false;
            }
            catch (Exception ex)
            {
            }
        }
        #endregion
    }
}
