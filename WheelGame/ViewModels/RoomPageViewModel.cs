using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WheelGame.Classes;
using WheelGame.Models;
using WheelGame.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace WheelGame.ViewModels
{
    public partial class RoomPageViewModel : MyBaseViewModel
    {
        #region Construcror
        public RoomPageViewModel()
        {
            _timer = new DispatcherTimer(DispatcherPriority.Input) { Interval = new TimeSpan(0, 0, 0, 0, 100) };
            _timer.Tick += TimerTick;
        }

        #endregion

        #region Fields
        public DispatcherTimer _timer;
        Stopwatch sw = new Stopwatch();
        MediaPlayer dingPlayer;        
        private TimeSpan savedTime = TimeSpan.Zero;
        bool leaveunsaved = false;
        bool localServerStarted = false;        
        #endregion

        #region Properties

        [ObservableProperty]        
        Room room = default!;
              
        [ObservableProperty]
        bool isConnected;

        [ObservableProperty]
        bool isStarted = false;
       
        [ObservableProperty]
        bool isRefreshing;

        [ObservableProperty]
        bool isTimerVisible;

        [ObservableProperty]
        bool isReconnecting = false;

        [ObservableProperty]
        Objective currentObjective;
        
        #endregion

        #region Events
        public event EventHandler<int>? OnRotateWheelRecieved;

        public event EventHandler? OnWheelSectorDelete;
        #endregion

        #region SignalREvents
        private void AddSignalRHubEventHandlers()
        {
            App.SignalRHub.OnPlayerConnected += SignalRHub_OnPlayerConnected;            
            App.SignalRHub.OnDisconnected += SignalRHub_OnDisconnected;
            App.SignalRHub.OnPlayerDisconnected += SignalRHub_OnPlayerDisconnected;            
            App.SignalRHub.OnReconnecting += SignalRHub_OnReconnecting;
            App.SignalRHub.OnReconnected += SignalRHub_OnReconnected;            
            App.SignalRHub.OnMarkObjectiveRecieved += SignalRHub_OnMarkObjectiveRecieved;
            App.SignalRHub.OnUnmarkObjectiveRecieved += SignalRHub_OnUnmarkObjectiveRecieved;
            App.SignalRHub.OnMarkObjectiveModifierRecieved += SignalRHub_OnMarkObjectiveModifierRecieved;
            App.SignalRHub.OnUnmarkObjectiveModifierRecieved += SignalRHub_OnUnmarkObjectiveModifierRecieved;
            App.SignalRHub.OnRotateWheel += SignalRHub_OnRotateWheel;
            App.SignalRHub.OnStartGameRecieved += SignalRHub_OnStartGameRecieved;
            App.SignalRHub.OnChangePlayerTurn += SignalRHub_OnChangePlayerTurn;
            App.SignalRHub.OnFinishCurrentObjective += SignalRHub_OnFinishCurrentObjective;
            App.SignalRHub.OnRemoveWheelSector += SignalRHub_OnRemoveWheelSector;
            App.SignalRHub.OnStartTimer += SignalRHub_OnStartTimer;
            App.SignalRHub.OnStopTimer += SignalRHub_OnStopTimer;
            App.SignalRHub.OnNewDebuffRecieved += SignalRHub_OnNewDebuffRecieved;
        }


        private void RemoveSignalRHubEventHandlers()
        {
            App.SignalRHub.OnPlayerConnected -= SignalRHub_OnPlayerConnected;
            App.SignalRHub.OnDisconnected -= SignalRHub_OnDisconnected;
            App.SignalRHub.OnPlayerDisconnected -= SignalRHub_OnPlayerDisconnected;            
            App.SignalRHub.OnReconnecting -= SignalRHub_OnReconnecting;
            App.SignalRHub.OnReconnected -= SignalRHub_OnReconnected;
            App.SignalRHub.OnMarkObjectiveRecieved -= SignalRHub_OnMarkObjectiveRecieved;
            App.SignalRHub.OnUnmarkObjectiveRecieved -= SignalRHub_OnUnmarkObjectiveRecieved;
            App.SignalRHub.OnMarkObjectiveModifierRecieved -= SignalRHub_OnMarkObjectiveModifierRecieved;
            App.SignalRHub.OnUnmarkObjectiveModifierRecieved -= SignalRHub_OnUnmarkObjectiveModifierRecieved;
            App.SignalRHub.OnRotateWheel -= SignalRHub_OnRotateWheel;
            App.SignalRHub.OnStartGameRecieved -= SignalRHub_OnStartGameRecieved;
            App.SignalRHub.OnChangePlayerTurn -= SignalRHub_OnChangePlayerTurn;
            App.SignalRHub.OnFinishCurrentObjective -= SignalRHub_OnFinishCurrentObjective;
            App.SignalRHub.OnRemoveWheelSector -= SignalRHub_OnRemoveWheelSector;
            App.SignalRHub.OnStartTimer -= SignalRHub_OnStartTimer;
            App.SignalRHub.OnStopTimer -= SignalRHub_OnStopTimer;
            App.SignalRHub.OnNewDebuffRecieved -= SignalRHub_OnNewDebuffRecieved;
        }

        private void SignalRHub_OnNewDebuffRecieved(object? sender, string e)
        {
            App.Current.Dispatcher.Invoke(() => { 
                Room.CurrentDebuff = e; 
            });
        }

        private void SignalRHub_OnStopTimer(object? sender, EventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                _timer.Stop();
                sw.Stop();
                sw.Reset();
                Room.IsTimerStarted = false;
            });
        }

        private void SignalRHub_OnStartTimer(object? sender, int seconds)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                Room.IsTimerStarted = true;
                sw.Reset();
                sw.Start();
                _timer.Start();
            });

        }

        private void SignalRHub_OnRemoveWheelSector(object? sender, string e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                Room.RemoveObjectiveBySlot(e);
                CurrentObjective = null;
                foreach (var item in Room.Players)
                {
                    item.IsFinished = false;
                }
                OnWheelSectorDelete?.Invoke(this, EventArgs.Empty);
            });
        }

        private void SignalRHub_OnFinishCurrentObjective(object? sender, string playerId)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var player = Room.Players.FirstOrDefault(i => i.Id == playerId);
                if (player!=null)
                    player.IsFinished = true;                
            });
        }

        private void SignalRHub_OnChangePlayerTurn(object? sender, string playerId)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                foreach (var item in Room.Players)
                {
                    item.IsPlayerTurn = item.Id == playerId;
                }
            });
        }

        private void SignalRHub_OnRotateWheel(object? sender, int e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                foreach (var item in Room.Players)
                {
                    item.IsFinished = false;
                }
                OnRotateWheelRecieved?.Invoke(this, e);
            });
        }

        private void SignalRHub_OnStartGameRecieved(object? sender, string e)
        {
            App.Current.Dispatcher.Invoke(() => { Room.IsGameStarted = true; });
        }

        private void SignalRHub_OnUnmarkObjectiveModifierRecieved(object? sender, WheelSignalRHub.MarkObjectiveModifierEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var obj = Room.FindObjectiveBySlot(e.Slot);
                if (obj != null)
                {
                    var objModifier = obj.Modifiers.FirstOrDefault(i => i.Slot == e.ModifierSlot);
                    if (objModifier!= null)
                    {
                        objModifier.PlayerIds.Remove(e.PlayerId);
                        var player = Room.Players.FirstOrDefault(i => i.Id == e.PlayerId);
                        if (player != null)
                        {
                            objModifier.Players.Remove(player);
                        }
                        objModifier.IsMarked = objModifier.PlayerIds.Any(i => i == Room.CurrentPlayer.Id);
                    }
                }
                Room.UpdatePlayersGoals();
            });
        }

        private void SignalRHub_OnMarkObjectiveModifierRecieved(object? sender, WheelSignalRHub.MarkObjectiveModifierEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var obj = Room.FindObjectiveBySlot(e.Slot);
                if (obj != null)
                {
                    var objModifier = obj.Modifiers.FirstOrDefault(i => i.Slot == e.ModifierSlot);
                    if (objModifier != null)
                    {
                        objModifier.PlayerIds.Add(e.PlayerId);
                        var player = Room.Players.FirstOrDefault(i => i.Id == e.PlayerId);
                        if (player != null)
                        {
                            objModifier.Players.Add(player);
                        }
                        objModifier.IsMarked = objModifier.PlayerIds.Any(i => i == Room.CurrentPlayer.Id);
                    }
                }
                Room.UpdatePlayersGoals();
            });
        }
       
        private void SignalRHub_OnUnmarkObjectiveRecieved(object? sender, WheelSignalRHub.MarkObjectiveEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var obj = Room.FindObjectiveBySlot(e.Slot);
                if (obj != null)
                {
                    obj.PlayerIds.Remove(e.PlayerId);
                    var player = Room.Players.FirstOrDefault(i => i.Id == e.PlayerId);
                    if (player != null)
                    {
                        obj.Players.Remove(player);
                    }
                    obj.IsMarked = obj.PlayerIds.Any(i => i == Room.CurrentPlayer.Id);
                }
                Room.UpdatePlayersGoals();
            });
        }

        private void SignalRHub_OnMarkObjectiveRecieved(object? sender, WheelSignalRHub.MarkObjectiveEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var obj = Room.FindObjectiveBySlot(e.Slot);
                if (obj != null)
                {
                    obj.PlayerIds.Add(e.PlayerId);
                    var player = Room.Players.FirstOrDefault(i => i.Id == e.PlayerId);
                    if (player != null)
                    {
                        obj.Players.Add(player);
                    }
                    obj.IsMarked = obj.PlayerIds.Any(i => i == Room.CurrentPlayer.Id);
                }
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
                }
            });
        }

        private async void SignalRHub_OnDisconnected(object? sender, string e)
        {
            IsReconnecting = true;
            sw.Stop();
            _timer.Stop();
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

        #region Timer
        private async void TimerTick(object? sender, EventArgs e)
        {
            if (Room.IsTimerStarted)
            {
                if (sw.IsRunning)
                {
                    Room.TimerCounter = sw.Elapsed;                    
                    if (Room.TimerCounter.TotalSeconds >= Room.RoomSettings.TimerSeconds)
                    {
                        _timer.Stop();
                        sw.Stop();
                        await FinishObjective();
                    }
                }
            }
        }
        #endregion

        #region BoardManipulationCommands

        [RelayCommand(AllowConcurrentExecutions = true)]
        async Task MarkObjective()
        {
            
            if (CurrentObjective != null)
            {
                await Room.MarkObjective(CurrentObjective);
            }
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        async Task MarkObjectiveModifier(ObjectiveModifier modifier)
        {
            if (modifier != null)
            {
                await Room.MarkObjectiveModifier(modifier);
            }
        }

        [RelayCommand]
        async Task RotateWheel()
        {
            var angle = 1080 + new Random().Next(0, 360);
            await Room.RotateWheel(angle);
        }

        [RelayCommand]
        async Task FinishObjective()
        {
            if (CurrentObjective != null)
            {
                await Room.FinishCurrentObjective(CurrentObjective);
            }
        }

        [RelayCommand]
        async Task StartGame()
        {
            await Room.StartGame();
        }

        [RelayCommand]
        async Task GetNewRandomDebuff()
        {
            await Room.GetNewRandomDebuff();
        }

        #endregion

        #region EventHandlers
        private async void Frame_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            RemoveSignalRHubEventHandlers();
            await Room.DisconnectAsync();
        }
        
        private async void RoomPageViewModel_Closing(object? sender, CancelEventArgs e)
        {
            RemoveSignalRHubEventHandlers();
            await Room.DisconnectAsync();            
        }

        #endregion

        #region AppearingCommand

        [RelayCommand]
        async Task Appearing()
        {
            AddSignalRHubEventHandlers();
            (App.Current.MainWindow as MainWindow).frame.Navigating += Frame_Navigating;
            (App.Current.MainWindow as MainWindow).Closing += RoomPageViewModel_Closing;
            await Room.InitRoom();
            MainWindow.HideSettingsButton();                        
        }
        #endregion

        #region Misc

        [RelayCommand]
        void CopyRoomId()
        {
            Clipboard.SetText(Room.RoomId);
            MainWindow.ShowToast(new ToastInfo("Успех!", "RoomId успешно скопирован!"));
        }
        #endregion

    }
}
