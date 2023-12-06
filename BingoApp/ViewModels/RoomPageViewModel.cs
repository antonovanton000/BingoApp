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
    public partial class RoomPageViewModel : MyBaseViewModel
    {

        public DispatcherTimer _timer;
        Stopwatch sw = new Stopwatch();

        private TimerSocketClient _timerSocket;
        
        MediaPlayer dingPlayer;

        private Uri chatSound = new Uri(System.Environment.CurrentDirectory + "/Sounds/chat.wav", UriKind.Relative);
        private Uri connectedSound = new Uri(System.Environment.CurrentDirectory + "/Sounds/connected.wav", UriKind.Relative);
        private Uri disconnectedSound = new Uri(System.Environment.CurrentDirectory + "/Sounds/disconnected.wav", UriKind.Relative);
        private Uri goalSound = new Uri(System.Environment.CurrentDirectory + "/Sounds/goal.wav", UriKind.Relative);
        private Uri revealSound = new Uri(System.Environment.CurrentDirectory + "/Sounds/reveal.wav", UriKind.Relative);

        public event EventHandler EventAdded;
        public RoomPageViewModel()
        {
            _timerSocket = new TimerSocketClient();
            _timer = new DispatcherTimer(DispatcherPriority.Input) { Interval = new TimeSpan(0, 0, 0, 0, 250) };
            _timer.Tick += async (s, e) =>
            {
                //if (IsTimerStarted)
                //    TimerCounter += 1;

                TimerCounter = sw.Elapsed;

                if (IsTimerStarted)
                {                    
                    if (IsStartTimerStarted)
                    {
                        IsAutoRevealBoardVisible = false;
                        if (((int)TimerCounter.TotalSeconds) == StartTimeSeconds)
                        {
                            sw.Stop();
                            IsStartTimerStarted = false;
                            IsAfterRevealTimerStarted = true;
                            await Room.RevealTheBoard();
                            Room.IsRevealed = true;
                            sw.Reset();
                            sw.Start();
                        }
                    }
                    else if (IsAfterRevealTimerStarted)
                    {
                        IsAutoRevealBoardVisible = false;
                        if (((int)TimerCounter.TotalSeconds) == AfterRevealSeconds)
                        {
                            sw.Stop();
                            IsAfterRevealTimerStarted = false;
                            IsGameTimerStarted = true;
                            Room.IsGameStarted = true;
                            Room.StartDate = DateTime.Now;
                            IsStarted = true;                            
                            IsTimerButtonsVisible = true;
                            await _timerSocket.DisconnectAsync();
                            sw.Reset();
                            StartTimer();
                        }
                    }

                }
            };
                        


            IsPlayerChat = BingoApp.Properties.Settings.Default.IsPlayerChat;
            IsGoalActions = BingoApp.Properties.Settings.Default.IsGoalActions;
            IsColorChanged = BingoApp.Properties.Settings.Default.IsColorChanged;
            IsConnections = BingoApp.Properties.Settings.Default.IsConnections;
            IsSoundsOn = BingoApp.Properties.Settings.Default.IsSoundsOn;
            SoundsVolume = BingoApp.Properties.Settings.Default.SoundsVolume;

            dingPlayer = new MediaPlayer();
            dingPlayer.Volume = SoundsVolume * 0.01d;
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

        [RelayCommand]
        async Task Appearing()
        {
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

            TimerCounter = TimeSpan.FromSeconds(Room.CurrentTimerTime);

            if (!Room.IsGameStarted)
            {
                if(Room.IsAutoBoardReveal)
                {
                    IsTimerButtonsVisible = false;
                    RevealPanelText = "Waiting for game started";
                    _timerSocket.ConnectionChangedEvent += _timerSocket_ConnectionChangedEvent;
                    _timerSocket.SettingsRecievedEvent += _timerSocket_SettingsRecievedEvent;
                    _timerSocket.StartRecievedEvent += _timerSocket_StartRecievedEvent;
                    IsTimerStarted = false;
                    await _timerSocket.InitAsync(Room.RoomId);                    
                    if (Room.IsCreatorMode)
                    {
                        IsAutoRevealBoardVisible = true;
                        StartTimeSeconds = BingoApp.Properties.Settings.Default.BeforeStartTime;
                        AfterRevealSeconds = BingoApp.Properties.Settings.Default.BoardAnalyzeTime;
                        await _timerSocket.SendSettingsToServerAsync(StartTimeSeconds, AfterRevealSeconds);
                    }
                }
                else
                {
                    IsStarted = true;
                }
            }
            else
            {
                IsStarted = true;
            }
        }

        [ObservableProperty]
        bool isFinishGameVisible;

        [ObservableProperty]
        bool isAutoRevealBoardVisible;

        [ObservableProperty]
        bool isRefreshing;

        private void _timerSocket_StartRecievedEvent(object? sender, StartRecievedEventArgs e)
        {
            //var dtNow = DateTime.Now;
            //var difference = dtNow - e.StartTime;

            //var startTime = TimeSpan.FromSeconds(StartTimeSeconds) - difference;
            TimerCounter = TimeSpan.FromSeconds((StartTimeSeconds) * -1);//(int)(Math.Round(startTime.TotalSeconds, 0) * -1);

            IsGameTimerStarted = false;
            IsStartTimerStarted = true;
            IsTimerStarted = true;
            sw.Start();
            _timer.Start();
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
            EventAdded?.Invoke(this, new EventArgs());
            if (IsSoundsOn)
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems!= null)
                {
                    var newEvent = (Event)e.NewItems[0];
                    if (newEvent != null)
                    {
                        switch (newEvent.Type)
                        {
                            case EventType.connection:
                                if (newEvent.EventType == EventSubType.connected)
                                    dingPlayer.Open(connectedSound);
                                else if (newEvent.EventType == EventSubType.disconnected)
                                    dingPlayer.Open(disconnectedSound);
                                break;
                            case EventType.chat:
                                dingPlayer.Open(chatSound);
                                break;
                            case EventType.revealed:
                                dingPlayer.Open(revealSound);
                                break;
                            case EventType.goal:
                                dingPlayer.Open(goalSound);
                                break;
                            case EventType.color:
                                break;
                            case EventType.none:
                                break;
                            default:
                                break;
                        }

                        dingPlayer.Volume = SoundsVolume * 0.01d;
                        dingPlayer.Play();
                        }
                }

            }
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
            if (!Room.IsAutoBoardReveal)
            {
                await Room.RevealTheBoard();
                Room.IsRevealed = true;
            }
        }

        [RelayCommand]
        async Task SendChatMessage()
        {
            await Room.SendChatMessage(ChatMessage);
            ChatMessage = "";
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
    }
}
