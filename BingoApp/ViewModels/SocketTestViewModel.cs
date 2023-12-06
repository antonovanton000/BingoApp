using BingoApp.Classes;
using BingoApp.Models;
using BingoApp.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace BingoApp.ViewModels
{
    public partial class SocketTestViewModel : MyBaseViewModel
    {

        private TimerSocketClient _timerSocket;

        public SocketTestViewModel()
        {            
            _timerSocket = new TimerSocketClient();
            
            _timer = new DispatcherTimer(DispatcherPriority.Input) { Interval = TimeSpan.FromSeconds(1) };            
            _timer.Tick += (s, e) =>
            {
                if (IsTimerStarted)
                {
                    if (IsStartTimerStarted)
                    {
                        if (TimerCounter == 0)
                        {
                            IsStartTimerStarted = false;
                            IsAfterRevealTimerStarted = true;
                            TimerCounter = AfterRevealSeconds * -1;                            
                            IsRevealed = true;
                        }
                    }
                    else if (IsAfterRevealTimerStarted) 
                    {
                        if (TimerCounter == 0) 
                        {
                            IsAfterRevealTimerStarted = false;
                            IsGameTimerStarted = true;
                            IsStarted = true;
                        }
                    }

                    TimerCounter += 1;
                }
            };            
        }

        public DispatcherTimer _timer;

        [ObservableProperty]
        bool isTimerStarted = false;

        [ObservableProperty]
        bool isStartTimerStarted = false;

        [ObservableProperty]
        bool isAfterRevealTimerStarted = false;

        [ObservableProperty]
        bool isGameTimerStarted = false;

        [ObservableProperty]
        bool isConnected;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TimerString))]
        int timerCounter;
                
        public string TimerString => TimeSpan.FromSeconds(TimerCounter).ToString(@"hh\:mm\:ss");

        [ObservableProperty]
        int startTimeSeconds;

        [ObservableProperty]
        int afterRevealSeconds;

        [ObservableProperty]
        bool isRevealed = false;

        [ObservableProperty]
        bool isStarted = false;

        [RelayCommand]
        async Task Appearing()
        {
            _timerSocket.ConnectionChangedEvent += _timerSocket_ConnectionChangedEvent;
            _timerSocket.SettingsRecievedEvent += _timerSocket_SettingsRecievedEvent;
            _timerSocket.StartRecievedEvent += _timerSocket_StartRecievedEvent;
            IsTimerStarted = false;
            await _timerSocket.InitAsync("TESTROOMID");
        }

        private void _timerSocket_StartRecievedEvent(object? sender, StartRecievedEventArgs e)
        {
            var dtNow = DateTime.Now;
            var difference = dtNow - e.StartTime;

            var startTime = TimeSpan.FromSeconds(StartTimeSeconds) - difference;
            TimerCounter = (int)(Math.Round(startTime.TotalSeconds ,0) * -1);
            
            IsStartTimerStarted = true;
            IsTimerStarted = true;
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

        [RelayCommand]
        async Task Connect()
        {
            await _timerSocket.SendSettingsToServerAsync(10, 30);
            StartTimeSeconds = 10;
            AfterRevealSeconds = 30;
        }

        [RelayCommand]
        async Task Start()
        {
            await _timerSocket.SendStartToServerAsync();
            
        }
        
    }
}
