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
        bool isFogWallOn = false;
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

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsBingoLineSelected))]
        string? selectedBingoLine;

        public bool IsBingoLineSelected => !string.IsNullOrEmpty(selectedBingoLine);

        [ObservableProperty]
        bool canSelectBingoLine;

        public bool IsChangingMode => Room?.RoomSettings.ExtraGameMode == ExtraGameMode.Changing;
        public bool IsUnhideMode => Room?.RoomSettings.ExtraGameMode == ExtraGameMode.Hidden;

        #endregion

        #region TimerControlCommands

        [RelayCommand]
        async Task StartTimer()
        {
            if (localServerStarted && App.LocalSignalRHub != null)
            {
                if (Room.IsTimerOnPause)
                {
                    await App.LocalSignalRHub.SendTimerResume((int)Room.TimerCounter.TotalSeconds);
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
            if (Room.RoomSettings.IsAutoBoardReveal)
            {
                await App.SignalRHub.SendResumeGame(Room.RoomId, Room.TimerCounter);
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
            savedTime = Room.TimerCounter;
            sw.Stop();
            sw.Reset();
            _timer.Stop();

            if (Room.RoomSettings.IsAutoBoardReveal)
            {
                await App.SignalRHub.SendPauseGame(Room.RoomId, Room.TimerCounter);
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
            Room.IsGameStarted = false;
            Room.TimerCounter = TimeSpan.FromSeconds(0);

            if (!Room.IsPractice)
                await App.SignalRHub.SendResetRoomTimer(Room.RoomId);

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

        [RelayCommand]
        async Task SyncTimer()
        {
            await App.SignalRHub.SendSyncRoomTimer(Room.RoomId, Room.TimerCounter);
        }

        #endregion

        #region TimerEvents
        private async void _timer_Tick(object? sender, EventArgs e)
        {
            Room.TimerCounter = (sw.Elapsed + savedTime);

            if (Room.IsCreatorMode)
            {
                if (Room.IsGameTimerStarted && Room.RoomSettings.ExtraGameMode == ExtraGameMode.Hidden && Room.CurrentHiddenStep <= 13)
                {
                    if (Room.TimerCounter.TotalMinutes >= (Room.RoomSettings.UnhideTimeMinutes * Room.CurrentHiddenStep))
                    {
                        await Room.ProcessHiddenGame();
                    }
                }

                if (Room.IsGameTimerStarted && Room.RoomSettings.ExtraGameMode == ExtraGameMode.Changing)
                {
                    int totalMinutes = (int)Room.TimerCounter.TotalMinutes;
                    if ((totalMinutes % Room.RoomSettings.ChangeTimeMinutes) == 0 && Room.LastChangeMinute != totalMinutes)
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
                    if (((int)Room.TimerCounter.TotalSeconds) == Room.RoomSettings.StartTimeSeconds)
                    {
                        sw.Stop();
                        sw.Reset();
                        Room.IsStartTimerStarted = false;
                        Room.IsAfterRevealTimerStarted = true;
                        await Room.RevealTheBoard();
                        Room.CurrentPlayer.IsBoardRevealed = true;
                        sw.Start();

                        if (localServerStarted && App.LocalSignalRHub != null)
                        {
                            await App.Current.Dispatcher.InvokeAsync(async () =>
                            {
                                await App.LocalSignalRHub.SendAfterRevealTimeStarted(Room.RoomSettings.AfterRevealSeconds);
                            });
                        }
                    }
                }
                if (Room.IsAfterRevealTimerStarted)
                {
                    Room.IsAutoRevealBoardVisible = false;
                    if ((int)Room.TimerCounter.TotalSeconds == (Room.RoomSettings.AfterRevealSeconds - 6))
                    {
                        dingPlayer.Open(beepSound);
                        dingPlayer.Volume = SoundsVolume * 0.01d;
                        dingPlayer.Play();
                    }

                    if (Room.RoomSettings.IsAutoFogWall)
                    {
                        if (Room.RoomSettings.AfterRevealSeconds - (int)Room.TimerCounter.TotalSeconds <= 2)
                        {
                            await RemoveFogWall();
                        }
                    }

                    if (((int)Room.TimerCounter.TotalSeconds) == Room.RoomSettings.AfterRevealSeconds)
                    {
                        sw.Stop();
                        sw.Reset();
                        Room.IsAfterRevealTimerStarted = false;
                        Room.IsGameTimerStarted = true;
                        Room.IsGameStarted = true;
                        Room.StartDate = DateTime.Now;
                        Room.TimerCounter = TimeSpan.FromSeconds(0);
                        IsStarted = true;
                        IsStopVisible = false;
                        if (Room.RoomSettings.ExtraGameMode == ExtraGameMode.Hidden || Room.RoomSettings.ExtraGameMode == ExtraGameMode.Changing)
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
            }
        }

        private void AutoBoardRevealProcess(bool isRefresh = false)
        {
            _timer.Stop();
            sw.Stop();
            sw.Reset();

            if (Room.IsGameStarted)
            {
                savedTime = Room.TimerCounter;
                Room.IsTimerStarted = false;
                IsTimerVisible = true;

                if (Room.IsCreatorMode)
                    IsTimerButtonsVisible = true;
                else
                    IsTimerButtonsVisible = false;
            }
            else
            {
                if (isRefresh)
                {
                    Room.TimerCounter = TimeSpan.FromSeconds(0);
                    savedTime = TimeSpan.FromSeconds(0);
                }
                IsTimerVisible = true;
                IsStartVisible = false;
                IsPauseVisible = false;
                IsResetVisible = false;
                IsStopVisible = false;
                IsTimerButtonsVisible = false;
                Room.IsTimerStarted = false;
                Room.IsStartTimerStarted = false;
                Room.IsAfterRevealTimerStarted = false;
                Room.IsGameTimerStarted = false;
                RevealPanelText = App.Current.FindResource("mes_waitingforgames").ToString();
                if (Room.IsCreatorMode)
                {
                    Room.IsAutoRevealBoardVisible = true;
                }
            }
        }

        #endregion

        #region SignalRHubEventHandlers
        private void AddSignalRHubEventHandlers()
        {
            App.SignalRHub.OnPlayerConnected += SignalRHub_OnPlayerConnected;
            App.SignalRHub.OnSyncRoomTimerRecieved += SignalRHub_OnSyncRoomTimerRecieved;
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
            App.SignalRHub.OnNewEventRecieved += SignalRHub_OnNewEventRecieved;
            App.SignalRHub.OnPlayerColorChangedRecieved += SignalRHub_OnPlayerColorChangedRecieved;
            App.SignalRHub.OnMarkSquareRecieved += SignalRHub_OnMarkSquareRecieved;
            App.SignalRHub.OnUnmarkSquareRecieved += SignalRHub_OnUnmarkSquareRecieved;
            App.SignalRHub.OnNewBoardRecieved += SignalRHub_OnNewBoardRecieved;
            App.SignalRHub.OnResetRoomTimerRecieved += SignalRHub_OnResetRoomTimerRecieved;
            App.SignalRHub.OnUnhideSquareRecieved += SignalRHub_OnUnhideSquareRecieved;
            App.SignalRHub.OnBingoLineSelected += SignalRHub_OnBingoLineSelected;
        }

        private void RemoveSignalRHubEventHandlers()
        {
            App.SignalRHub.OnPlayerConnected -= SignalRHub_OnPlayerConnected;
            App.SignalRHub.OnSyncRoomTimerRecieved -= SignalRHub_OnSyncRoomTimerRecieved;
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
            App.SignalRHub.OnNewEventRecieved -= SignalRHub_OnNewEventRecieved;
            App.SignalRHub.OnPlayerColorChangedRecieved -= SignalRHub_OnPlayerColorChangedRecieved;
            App.SignalRHub.OnMarkSquareRecieved -= SignalRHub_OnMarkSquareRecieved;
            App.SignalRHub.OnUnmarkSquareRecieved -= SignalRHub_OnUnmarkSquareRecieved;
            App.SignalRHub.OnNewBoardRecieved -= SignalRHub_OnNewBoardRecieved;
            App.SignalRHub.OnResetRoomTimerRecieved -= SignalRHub_OnResetRoomTimerRecieved;
            App.SignalRHub.OnUnhideSquareRecieved -= SignalRHub_OnUnhideSquareRecieved;
            App.SignalRHub.OnBingoLineSelected -= SignalRHub_OnBingoLineSelected;

        }

        private async void SignalRHub_OnUnhideSquareRecieved(object? sender, BingoAppSignalRHub.UnhideSquareEventArgs e)
        {
            if (e.RoomId != Room.RoomId) return;
            await Room.UnhideSquares(e.SlotId);

            if (localServerStarted && App.LocalSignalRHub != null)
            {
                await App.LocalSignalRHub.SendUnhideSquare(e.SlotId);
            }
        }

        private async void SignalRHub_OnSyncRoomTimerRecieved(object? sender, BingoAppSignalRHub.RoomTimerEventArgs e)
        {
            if (e.RoomId != Room.RoomId) return;
            Room.IsTimerOnPause = true;
            Room.IsTimerStarted = false;
            sw.Stop();
            sw.Reset();
            _timer.Stop();
            IsPauseVisible = false;
            IsStartVisible = true;
            Room.TimerCounter = e.CurrentTime;
            savedTime = e.CurrentTime;

            if (localServerStarted && App.LocalSignalRHub != null)
            {
                await App.LocalSignalRHub.SendTimerPause();
            }
        }

        private async void SignalRHub_OnNewEventRecieved(object? sender, Event e)
        {
            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                Room.ChatMessages.Add(e);
            });
        }

        private async void SignalRHub_OnNewBoardRecieved(object? sender, Board e)
        {
            Room.Board = e;
            if (Room.RoomSettings.IsAutoBoardReveal)
            {
                Room.IsGameEnded = false;
                Room.IsGameStarted = false;
                Room.CurrentPlayer.IsBoardRevealed = false;
                AutoBoardRevealProcess(true);
            }
            Room.UpdatePlayersGoals();
            await Room.SaveAsync();
        }

        private void SignalRHub_OnUnmarkSquareRecieved(object? sender, BingoAppSignalRHub.MarkSquareEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var obj = Room.Board.Squares.FirstOrDefault(i => i.Slot == e.Slot);
                if (obj != null)
                {
                    obj.IsMarking = false;
                    obj.SquareColors.Remove(e.Color);
                    Room.TriggerMarkSquareEvent(obj);
                }
                Room.UpdatePlayersGoals(obj);
            });
        }

        private void SignalRHub_OnMarkSquareRecieved(object? sender, BingoAppSignalRHub.MarkSquareEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var obj = Room.Board.Squares.FirstOrDefault(i => i.Slot == e.Slot);
                if (obj != null)
                {
                    obj.IsMarking = false;
                    obj.SquareColors.Add(e.Color);
                    Room.TriggerMarkSquareEvent(obj);
                }
                Room.UpdatePlayersGoals(obj);
            });
        }

        private async void SignalRHub_OnPlayerColorChangedRecieved(object? sender, BingoAppSignalRHub.PlayerColorChangedEventArgs e)
        {
            await App.Current.Dispatcher.InvokeAsync(async () =>
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
                if (localServerStarted && App.LocalSignalRHub != null)
                {
                    await App.LocalSignalRHub.SendPlayerChangeColor(e.PlayerId, e.Color);
                }
            });
        }

        private async void SignalRHub_OnPlayerConnected(object? sender, Player e)
        {
            await App.Current.Dispatcher.InvokeAsync(async () =>
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
                if (localServerStarted && App.LocalSignalRHub != null)
                {
                    await App.LocalSignalRHub.SendPlayerConnected(e);
                }
            });
        }

        private void SignalRHub_OnRoomBoardRecieved(object? sender, BingoAppSignalRHub.RoomBoardEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                foreach (var item in e.Squares)
                {
                    var square = Room.Board.Squares.FirstOrDefault(i => i.Slot == item.Slot);
                    if (square != null)
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
                if (e.RoomId != Room.RoomId) return;
                await Room.ChangeSquare(e.SlotId, e.NewName);
                if (localServerStarted && App.LocalSignalRHub != null)
                {
                    await App.LocalSignalRHub.SendChangeSquare(e.SlotId, e.NewName);
                }
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

        private async void SignalRHub_OnPlayerDisconnected(object? sender, string playerId)
        {
            await App.Current.Dispatcher.InvokeAsync(async () =>
             {
                 var player = Room.Players.FirstOrDefault(i => i.Id == playerId);
                 if (player != null)
                 {
                     savedTime = Room.TimerCounter;
                     sw.Stop();
                     sw.Reset();
                     _timer.Stop();
                     Room.Players.Remove(player);
                     IsStartVisible = true;
                     IsPauseVisible = false;
                     Room.IsTimerStarted = false;
                     Room.IsTimerOnPause = true;
                     dingPlayer.Open(disconnectedSound);
                     dingPlayer.Volume = SoundsVolume * 0.01d;
                     dingPlayer.Play();
                     if (localServerStarted && App.LocalSignalRHub != null)
                     {
                         await App.LocalSignalRHub.SendPlayerDisconnected(playerId);
                     }
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
                        await App.SignalRHub.ConnectToRoomHub(Room.RoomId);
                        await Room.InitRoom();

                        var preferedColor = BingoApp.Properties.Settings.Default.PreferedColor;
                        if (!string.IsNullOrEmpty(preferedColor))
                        {
                            var color = (BingoColor)Enum.Parse(typeof(BingoColor), preferedColor);
                            await Room.ChangeColor(color);
                        }

                        if (Room.IsCreatorMode)
                        {
                            savedTime = Room.TimerCounter;
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

        private async void SignalRHub_OnResumeGameRecieved(object? sender, BingoAppSignalRHub.RoomTimerEventArgs e)
        {
            if (e.RoomId != Room.RoomId) return;
            sw.Start();
            _timer.Start();

            Room.IsTimerOnPause = false;
            Room.IsGameEnded = false;
            IsPauseVisible = true;
            IsResetVisible = true;
            IsStartVisible = false;
            savedTime = e.CurrentTime;
            Room.TimerCounter = e.CurrentTime;

            if (localServerStarted && App.LocalSignalRHub != null)
            {
                await App.LocalSignalRHub.SendTimerResume((int)Room.TimerCounter.TotalSeconds);
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
            Room.TimerCounter = TimeSpan.FromSeconds(0);
            IsTimerVisible = false;
            Room.CurrentPlayer.IsBoardRevealed = false;
            savedTime = TimeSpan.FromSeconds(0);
            if (Room.IsCreatorMode)
            {
                Room.IsAutoRevealBoardVisible = true;
            }

            if (localServerStarted && App.LocalSignalRHub != null)
            {
                await App.LocalSignalRHub.SendGameStoped();
            }
        }

        private async void SignalRHub_OnResetRoomTimerRecieved(object? sender, string e)
        {
            if (e != Room.RoomId) return;

            sw.Stop();
            sw.Reset();
            _timer.Stop();
            IsPauseVisible = false;
            IsResetVisible = false;
            Room.IsTimerStarted = false;
            IsStartVisible = true;
            Room.IsTimerOnPause = false;
            Room.IsGameEnded = false;
            Room.IsGameStarted = false;
            Room.TimerCounter = TimeSpan.FromSeconds(0);

            if (localServerStarted && App.LocalSignalRHub != null)
            {
                await App.LocalSignalRHub.SendTimerReset();
            }
        }

        private async void SignalRHub_OnPauseGameRecieved(object? sender, BingoAppSignalRHub.RoomTimerEventArgs e)
        {
            if (e.RoomId != Room.RoomId) return;
            sw.Stop();
            sw.Reset();
            _timer.Stop();

            Room.IsTimerOnPause = true;
            IsPauseVisible = false;
            IsStartVisible = true;
            savedTime = e.CurrentTime;
            Room.TimerCounter = e.CurrentTime;

            if (localServerStarted && App.LocalSignalRHub != null)
            {
                await App.LocalSignalRHub.SendTimerPause();
            }
        }

        private async void SignalRHub_OnStartGameRecieved(object? sender, string e)
        {
            if (e != Room.RoomId) return;

            Room.TimerCounter = TimeSpan.FromSeconds(0);
            Room.IsAfterRevealTimerStarted = false;
            Room.IsGameTimerStarted = false;
            Room.IsStartTimerStarted = true;
            Room.IsTimerStarted = true;
            Room.IsTimerOnPause = false;
            Room.IsGameEnded = false;
            IsStopVisible = true;
            IsPauseVisible = false;
            IsStartVisible = true;
            IsTimerVisible = true;
            sw.Start();
            _timer.Start();

            if (Room.RoomSettings.IsAutoFogWall)
            {
                await AddFogWall();
            }

            if (localServerStarted && App.LocalSignalRHub != null)
            {
                await App.LocalSignalRHub.SendStartTimeStarted(Room.RoomSettings.StartTimeSeconds);
            }

        }

        private void SignalRHub_OnRoomTimeSyncRecieved(object? sender, BingoAppSignalRHub.RoomTimeSyncEventArgs e)
        {
            if (e.RoomId != Room.RoomId) return;

            sw.Reset();
            Room.TimerCounter = TimeSpan.FromSeconds(e.CurrentTime);
            savedTime = TimeSpan.FromSeconds(e.CurrentTime);
        }

        private void SignalRHub_OnPlayerReconnected(object? sender, string e)
        {
            if (Room.IsCreatorMode)
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    dingPlayer.Open(connectedSound);
                    dingPlayer.Volume = SoundsVolume * 0.01d;
                    dingPlayer.Play();
                });
            }
        }

        private void SignalRHub_OnBingoLineSelected(object? sender, string e)
        {
            Room.CurrentPlayer.SelectedBingoLine = e;
            foreach (var item in Room.Board.Squares)
            {
                item.IsSelectedBingo = false;
            }

            if (e.Contains("row"))
            {
                var rowNum = int.Parse(e.Replace("row", ""));
                var squares = Room.Board.Squares.Where(i => i.Row == rowNum);
                foreach (var item in squares)
                {
                    item.IsSelectedBingo = true;
                }
            }
            else if (e.Contains("col"))
            {
                var colNum = int.Parse(e.Replace("col", ""));
                var squares = Room.Board.Squares.Where(i => i.Column == colNum);
                foreach (var item in squares)
                {
                    item.IsSelectedBingo = true;
                }
            }
            else if (e == "tr_bl")
            {
                var squares = Room.Board.GetTRLBDiagonal();
                foreach (var item in squares)
                {
                    item.IsSelectedBingo = true;
                }
            }
            else if (e == "tl_br")
            {
                var squares = Room.Board.GetTLRBDiagonal();
                foreach (var item in squares)
                {
                    item.IsSelectedBingo = true;
                }
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
                                    if (Room.IsGameStarted)
                                    {
                                        await App.SignalRHub.SendPauseGame(Room.RoomId, Room.TimerCounter);
                                    }

                                    await Room.DisconnectAsync();
                                    RemoveSignalRHubEventHandlers();
                                    await App.SignalRHub.DisconnectFromRoomHub(Room.RoomId);
                                }

                                if (Room.CurrentPlayer.IsSpectator)
                                {
                                    if (Room.IsCreatorMode)
                                        await Room.SaveAsync();
                                }
                                else
                                {
                                    await Room.SaveAsync();
                                }

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
                    (App.Current.MainWindow as MainWindow).frame.Navigating -= Frame_Navigating;
                    (App.Current.MainWindow as MainWindow).Closing -= RoomPageViewModel_Closing;
                }
            }
        }

        private void RoomPageViewModel_Closing(object? sender, CancelEventArgs e)
        {
            e.Cancel = true;
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
                        RemoveSignalRHubEventHandlers();
                        await App.SignalRHub.DisconnectFromRoomHub(Room.RoomId);
                    }

                    if (Room.CurrentPlayer.IsSpectator)
                    {
                        if (Room.IsCreatorMode)
                            await Room.SaveAsync();
                    }
                    else
                    {
                        await Room.SaveAsync();
                    }

                    if (App.LocalServer.IsServerStarted)
                        await App.LocalServer.StopAsync();

                    await Task.Delay(1000);
                    (App.Current.MainWindow as MainWindow).frame.Navigating -= Frame_Navigating;
                    (App.Current.MainWindow as MainWindow).Closing -= RoomPageViewModel_Closing;
                    App.Current.Shutdown();
                }));
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
                            var filteredEventTypes = new List<EventType>() { EventType.bingo, EventType.win, EventType.newsquare, EventType.unhudesquare };

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
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText(System.IO.Path.Combine(App.Location, "errors.log"), ex.Message + "\r\n");
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
            await Room.ChangeColor(bcolor);
        }

        [RelayCommand]
        async Task RevealBoard()
        {
            if (Room.IsPractice)
            {
                Room.CurrentPlayer.IsBoardRevealed = true;
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
                if (Room.RoomSettings.IsAutoBoardReveal && (Room.IsStartTimerStarted || !Room.IsGameStarted))
                {
                    return;
                }

                await Room.RevealTheBoard();
                Room.CurrentPlayer.IsBoardRevealed = true;
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
                Room.IsGameEnded = false;
                Room.TimerCounter = TimeSpan.Zero;
                savedTime = TimeSpan.Zero;
                Room.CurrentPlayer.IsBoardRevealed = false;
                Room.GeneratePracticeBoard();
                Room.UpdatePlayersGoals();
                Room.TriggerNewBoardGenerated();
            }
            else
            {
                Room.IsGameStarted = false;
                IsRefreshing = true;
                await Room.GenerateNewBoardAsync();
                IsRefreshing = false;
            }
        }

        public async Task SelectBingoLine(string lineName, string playerId)
        {
            if (Room.RoomSettings.IsTripleBingoSelect)
            {
                await Room.SendSelectBingoLine(playerId, lineName);
                CanSelectBingoLine = false;
                SelectedBingoLine = null;
                foreach (var item in Room.Board.Squares)
                {
                    item.IsInSelectedLine = false;
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
            else
            {
                IsConnected = true;
            }

            await Room.InitRoom();

            var preferedColor = BingoApp.Properties.Settings.Default.PreferedColor;
            if (!string.IsNullOrEmpty(preferedColor))
            {
                var color = (BingoColor)Enum.Parse(typeof(BingoColor), preferedColor);
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
                        @event.Type == EventType.bingo ||
                        @event.Type == EventType.win ||
                        @event.Type == EventType.newsquare ||
                        @event.Type == EventType.unhudesquare ||
                        @event.Type == EventType.revealed);
            };

            if (Room.RoomSettings.IsAutoBoardReveal)
            {
                AutoBoardRevealProcess();
            }
            else
            {
                IsTimerVisible = true;
            }

            if (Room.RoomSettings.GameMode == GameMode.Triple && Room.RoomSettings.IsTripleBingoSelect)
                CanSelectBingoLine = true;

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

                        Room.OnUpdatePlayerGoals += async (sender, e) =>
                        {
                            await App.LocalSignalRHub.SendPlayersScore(e);
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
                        @event.Type == EventType.revealed ||
                        @event.Type == EventType.bingo ||
                        @event.Type == EventType.win);
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
            jobj["gm"] = Room.RoomSettings.GameMode.ToString();
            jobj["egm"] = Room.RoomSettings.ExtraGameMode.ToString();
            jobj["pr"] = Room.RoomSettings.PresetName?.ToString();
            jobj["g"] = Room.RoomSettings.GameName?.ToString();

            var json = jobj.ToString();
            var data = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

            var resp = await App.RestClient.SendGameInvite(App.CurrentPlayer, SelectedPlayer, data);
            IsInvitePopupVisible = false;

            MainWindow.ShowToast(new ToastInfo() { Title = App.Current.FindResource("mes_success").ToString(), Detail = App.Current.FindResource("mes_invitesent").ToString() });

        }

        #endregion

        #region CopyLinksCommands
        [RelayCommand]
        void CopyRoomId()
        {
            Clipboard.SetText(Room.RoomId);

            MainWindow.ShowToast(new ToastInfo() { Title = App.Current.FindResource("mes_success").ToString(), Detail = App.Current.FindResource("mes_roomidcopied").ToString() });
        }

        [RelayCommand]
        void CopyRoomSpectatorLink()
        {
            Clipboard.SetText(RestClient.baseUri + $"bingo/spectate/{Room.RoomId}");

            MainWindow.ShowToast(new ToastInfo() { Title = App.Current.FindResource("mes_success").ToString(), Detail = App.Current.FindResource("rp_linkcopiedsuccess").ToString() });
        }

        #endregion

        #region PlayerStufCommands

        [RelayCommand]
        async Task PlayerClick(Player player)
        {
            if (CanSelectBingoLine && IsBingoLineSelected && SelectedBingoLine != null)
            {
                if (player.Id != Room.CurrentPlayer.Id)
                {
                    await SelectBingoLine(SelectedBingoLine, player.Id);
                    return;
                }
            }

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
                BingoType = Room.RoomSettings.GameMode.ToString(),
                PlayersNames = Room.RoomPlayers.Select(i => i.NickName).ToArray(),
                Score = Room.RoomPlayers.Select(i => i.SquaresCount).ToArray(),
                LinesCount = Room.RoomPlayers.Select(i => i.LinesCount).ToArray(),
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
                var fogWallFilePath = System.IO.Path.Combine(sekiroFolder, "bingomod", "fogwall.cfg");
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
                var fogWallFilePath = System.IO.Path.Combine(sekiroFolder, "bingomod", "fogwall.cfg");
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
