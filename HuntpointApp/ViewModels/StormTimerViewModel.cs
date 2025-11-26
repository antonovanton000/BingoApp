using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HuntpointApp.Classes;
using HuntpointApp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Threading;

namespace HuntpointApp.ViewModels
{
    public partial class StormTimerViewModel:MyBaseViewModel
    {

        [RelayCommand]
        void Appearing()
        {
            InitStormTimer();
        }

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
    }
}
