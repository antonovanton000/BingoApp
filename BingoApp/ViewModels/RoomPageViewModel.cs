using BingoApp.Classes;
using BingoApp.Models;
using BingoApp.Properties;
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
    public partial class RoomPageViewModel : MyBaseViewModel
    {

        public DispatcherTimer _timer;
        Stopwatch sw = new Stopwatch();

        private TimerSocketClient _timerSocket;

        MediaPlayer dingPlayer;

        private Uri chatSound = new Uri(App.Location + "\\Sounds\\chat.wav", UriKind.Absolute);
        private Uri connectedSound = new Uri(App.Location + "\\Sounds\\connected.wav", UriKind.Absolute);
        private Uri disconnectedSound = new Uri(App.Location + "\\Sounds\\disconnected.wav", UriKind.Absolute);
        private Uri goalSound = new Uri(App.Location + "\\Sounds\\goal.wav", UriKind.Absolute);
        private Uri revealSound = new Uri(App.Location + "\\Sounds\\reveal.wav", UriKind.Absolute);
        private TimeSpan savedTime = TimeSpan.Zero;
        public event EventHandler EventAdded;

        public RoomPageViewModel()
        {
            _timerSocket = new TimerSocketClient();
            _timer = new DispatcherTimer(DispatcherPriority.Input) { Interval = new TimeSpan(0, 0, 0, 0, 250) };
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
        }



        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsAutoRevealBoardVisible))]
        Room? room;

        [ObservableProperty]
        bool isPotentialBingoVisible = false;

        [ObservableProperty]
        BingoColor potentialBingoColor = BingoColor.blank;

        [ObservableProperty]
        string chatMessage;

        [ObservableProperty]
        bool isTimerStarted = false;

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

        [RelayCommand]
        void StartTimer()
        {
            IsStartVisible = false;
            IsPauseVisible = true;
            IsResetVisible = true;
            IsTimerStarted = true;
            sw.Start();
            _timer.Start();
        }

        [RelayCommand]
        void PauseTimer()
        {
            IsStartVisible = true;
            IsPauseVisible = false;
            IsTimerStarted = false;
            sw.Stop();
            _timer.Stop();
        }

        [RelayCommand]
        void ResetTimer()
        {
            sw.Stop();
            sw.Reset();
            _timer.Stop();
            IsPauseVisible = false;
            IsResetVisible = false;
            IsTimerStarted = false;
            IsStartVisible = true;
            TimerCounter = TimeSpan.FromSeconds(0);
        }

        [RelayCommand]
        async Task StopTimer()
        {
            await _timerSocket.SendStopToServerAsync();
        }

        private async void _timer_Tick(object? sender, EventArgs e)
        {
            //if (IsTimerStarted)
            //    TimerCounter += 1;

            TimerCounter = (sw.Elapsed + savedTime);

            if (IsTimerStarted)
            {
                if (IsStartTimerStarted)
                {
                    IsAutoRevealBoardVisible = false;
                    if (((int)TimerCounter.TotalSeconds) == StartTimeSeconds)
                    {
                        sw.Stop();
                        sw.Reset();
                        IsStartTimerStarted = false;
                        IsAfterRevealTimerStarted = true;
                        await Room.RevealTheBoard();
                        Room.IsRevealed = true;
                        sw.Start();
                    }
                }
                if (IsAfterRevealTimerStarted)
                {
                    IsAutoRevealBoardVisible = false;
                    if (((int)TimerCounter.TotalSeconds) == AfterRevealSeconds)
                    {
                        sw.Stop();
                        sw.Reset();
                        IsAfterRevealTimerStarted = false;
                        IsGameTimerStarted = true;
                        Room.IsGameStarted = true;
                        Room.StartDate = DateTime.Now;
                        IsStarted = true;
                        IsTimerButtonsVisible = true;
                        IsStopVisible = false;
                        StartTimer();
                    }
                }

            }
        }


        [ObservableProperty]
        ICollectionView chatEventsMessages;

        [ObservableProperty]
        int startTimeSeconds;

        [ObservableProperty]
        int afterRevealSeconds;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsAutoRevealBoardVisible))]
        bool isStartTimerStarted = false;

        [ObservableProperty]
        bool isAfterRevealTimerStarted = false;

        [ObservableProperty]
        bool isGameTimerStarted = true;

        [ObservableProperty]
        bool isConnected;

        [ObservableProperty]
        bool isStarted = false;

        [ObservableProperty]
        string revealPanelText = "Click to reveal";

        [ObservableProperty]
        bool isSoundsOn;

        [ObservableProperty]
        int soundsVolume;

        [ObservableProperty]
        bool isDebug;

        [RelayCommand]
        async Task Appearing()
        {
            IsDebug = Settings.Default.IsDebug;

            MainWindow.HideSettingsButton();
            (App.Current.MainWindow as MainWindow).frame.Navigating += Frame_Navigating;
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
                        @event.Type == EventType.revealed);
            };

            if (Room.StartDate == null)
                Room.StartDate = DateTime.Now;
            if (Room.RoomSettings.HideCard)
                Room.IsRevealed = false;


            Room.NewCardEvent += Room_NewCardEvent;
            if (Room.IsAutoBoardReveal)
                await AutoBoardRevealProcess();
            else
                IsTimerVisible = true;
        }

        private async void Room_NewCardEvent(object? sender, EventArgs e)
        {
            if (Room.IsAutoBoardReveal)
            {
                await AutoBoardRevealProcess(true);
            }
        }

        private async Task AutoBoardRevealProcess(bool isRefresh = false)
        {
            _timer.Stop();
            sw.Stop();
            sw.Reset();

            IsTimerStarted = false;
            IsStartVisible = false;
            IsPauseVisible = false;
            IsResetVisible = false;
            IsStopVisible = false;
            IsTimerButtonsVisible = false;
            IsStartTimerStarted = false;
            IsAfterRevealTimerStarted = false;
            IsGameTimerStarted = false;

            TimerCounter = TimeSpan.FromSeconds(isRefresh ? 0 : Room.CurrentTimerTime);
            savedTime = TimeSpan.FromSeconds(isRefresh ? 0 : Room.CurrentTimerTime);

            _timerSocket.ConnectionChangedEvent += _timerSocket_ConnectionChangedEvent;
            _timerSocket.SettingsRecievedEvent += _timerSocket_SettingsRecievedEvent;
            _timerSocket.StartRecievedEvent += _timerSocket_StartRecievedEvent;
            _timerSocket.StopRecievedEvent += _timerSocket_StopRecievedEvent;

            Room.IsGameStarted = false;
            IsTimerVisible = true;
            IsTimerButtonsVisible = false;
            RevealPanelText = "Waiting for game started";
            IsTimerStarted = false;
            await _timerSocket.InitAsync(Room.RoomId);
            if (Room.IsCreatorMode)
            {
                IsAutoRevealBoardVisible = true;
                StartTimeSeconds = BingoApp.Properties.Settings.Default.BeforeStartTime;
                AfterRevealSeconds = BingoApp.Properties.Settings.Default.BoardAnalyzeTime;
                await _timerSocket.SendSettingsToServerAsync(StartTimeSeconds, AfterRevealSeconds);
            }
            else
            {
                if (Room.CurrentPlayer.IsSpectator)
                {
                    IsAutoRevealBoardVisible = false;
                    IsTimerVisible = true;
                }
                else
                {
                    IsTimerVisible = true;
                    IsTimerButtonsVisible = false;
                    RevealPanelText = "Waiting for game started";
                    IsTimerStarted = false;
                }
            }
        }

        [ObservableProperty]
        bool isFinishGameVisible;

        [ObservableProperty]
        bool isAutoRevealBoardVisible;

        [ObservableProperty]
        bool isRefreshing;

        [ObservableProperty]
        bool isTimerVisible;

        private void _timerSocket_StartRecievedEvent(object? sender, StartRecievedEventArgs e)
        {
            //var dtNow = DateTime.Now;
            //var difference = dtNow - e.StartTime;

            //var startTime = TimeSpan.FromSeconds(StartTimeSeconds) - difference;
            TimerCounter = TimeSpan.FromSeconds(0); //TimeSpan.FromSeconds((StartTimeSeconds) * -1);//(int)(Math.Round(startTime.TotalSeconds, 0) * -1);
            IsAfterRevealTimerStarted = false;
            IsGameTimerStarted = false;
            IsStartTimerStarted = true;
            IsTimerStarted = true;
            IsStopVisible = true;
            IsTimerVisible = true;            
            sw.Start();
            _timer.Start();
        }

        private void _timerSocket_StopRecievedEvent(object? sender, StartRecievedEventArgs e)
        {
            _timer.Stop();
            sw.Stop();
            sw.Reset();
            IsStartTimerStarted = false;
            IsGameTimerStarted = false;
            IsAfterRevealTimerStarted = false;
            IsPauseVisible = false;
            IsResetVisible = false;
            IsTimerStarted = false;
            IsStartVisible = false;
            IsStopVisible = false;
            TimerCounter = TimeSpan.FromSeconds(0);
            IsTimerVisible = false;
            Room.IsRevealed = false;
            savedTime = TimeSpan.FromSeconds(0);
            if (Room.IsCreatorMode)
            {
                IsAutoRevealBoardVisible = true;
            }
        }

        private void _timerSocket_SettingsRecievedEvent(object? sender, SettingsRecievedEventArgs e)
        {
            StartTimeSeconds = e.StartTimeSeconds;
            AfterRevealSeconds = e.AfterRevealSeconds;
        }

        private void _timerSocket_ConnectionChangedEvent(object? sender, ConnectionChangedEventArgs e)
        {
            IsConnected = e.State == System.Net.WebSockets.WebSocketState.Open;
        }


        bool leaveunsaved = false;

        private async void Frame_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
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
                            MainWindow.ShowMessage("Your game not finished!\r\n\r\nAre you sure you want to leave?", MessageNotificationType.YesNo,
                                new Action(async () =>
                                {
                                    await Room.DisconnectAsync();
                                    await _timerSocket.DisconnectAsync();
                                    Room.CurrentTimerTime = (int)TimerCounter.TotalSeconds;
                                    await Room.SaveAsync();
                                    leaveunsaved = true;
                                    MainWindow.CloseMessage();
                                    (App.Current.MainWindow as MainWindow).frame.Navigating -= Frame_Navigating;
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
                                MainWindow.ShowMessage("Are you sure you want to leave?", MessageNotificationType.YesNo,
                                    new Action(async () =>
                                    {
                                        leaveunsaved = true;
                                        MainWindow.CloseMessage();
                                        (App.Current.MainWindow as MainWindow).frame.Navigating -= Frame_Navigating;
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
                                MainWindow.ShowMessage("Your game not finished!\r\n\r\nAre you sure you want to leave?", MessageNotificationType.YesNo,
                                    new Action(async () =>
                                    {
                                        await Room.DisconnectAsync();
                                        await _timerSocket.DisconnectAsync();
                                        Room.CurrentTimerTime = (int)TimerCounter.TotalSeconds;
                                        await Room.SaveAsync();
                                        leaveunsaved = true;
                                        MainWindow.CloseMessage();
                                        (App.Current.MainWindow as MainWindow).frame.Navigating -= Frame_Navigating;
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

        private void ChatMessages_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
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
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText(System.IO.Path.Combine(App.Location, "errors.log"), ex.Message + "\r\n");
            }
        }

        [RelayCommand]
        void MuteUnmute()
        {
            BingoApp.Properties.Settings.Default.IsSoundsOn = IsSoundsOn;
            BingoApp.Properties.Settings.Default.Save();
        }

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
            BingoApp.Properties.Settings.Default.IsPlayerChat = IsPlayerChat;
            BingoApp.Properties.Settings.Default.IsGoalActions = IsGoalActions;
            BingoApp.Properties.Settings.Default.IsColorChanged = IsColorChanged;
            BingoApp.Properties.Settings.Default.IsConnections = IsConnections;
            BingoApp.Properties.Settings.Default.Save();

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

        [RelayCommand]
        async Task StartGame()
        {
            await _timerSocket.SendStartToServerAsync();

        }

        [RelayCommand]
        void CopyBingoAppLink()
        {
            var jobj = new JObject();
            jobj["r"] = Room.RoomId;
            jobj["p"] = IsPasswordShare ? Room.PlayerCredentials.Password : "";
            jobj["i"] = Room.IsAutoBoardReveal ? 1 : 0;

            var json = jobj.ToString();
            var data = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

            Clipboard.SetText($"http://{App.TimerSocketAddress}/?d={data}");

            MainWindow.ShowToast(new ToastInfo() { Title = "Success", Detail = "Link copied!" });
        }

        [RelayCommand]
        void CopyBingosyncLink()
        {
            Clipboard.SetText($"https://bingosync.com/room/{Room.RoomId}");
            MainWindow.ShowToast(new ToastInfo() { Title = "Success", Detail = "Link copied!" });

        }

        [RelayCommand]
        async Task EndGame()
        {
            MainWindow.ShowMessage("Do you realy want to end this game?", MessageNotificationType.YesNo, async () =>
            {
                Room.IsGameEnded = true;
                Room.EndDate = DateTime.Now;
                IsTimerStarted = false;
                IsStartVisible = true;
                IsPauseVisible = false;
                IsTimerStarted = false;
                _timer.Stop();
                Room.CurrentTimerTime = (int)TimerCounter.TotalSeconds;
                await Room.SaveToHistoryAsync();
                Room.RemoveFromActiveRooms();

                MainWindow.GoBack();
            });
        }

        [RelayCommand]
        async Task RefreshRoom()
        {
            IsRefreshing = true;
            await Room.RefreshRoomAsync();
            IsRefreshing = false;
        }

        [RelayCommand]
        void PlayerClick(Player player)
        {
            Room.UpdatePlayersGoals();
            PotentialBingoColor = player.Color;
            IsPotentialBingoVisible = !IsPotentialBingoVisible;
        }

        [RelayCommand]
        async void NewBoard()
        {
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
}
