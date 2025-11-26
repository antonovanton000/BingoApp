using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;

namespace WheelGame.Models;

public partial class Room : ObservableObject
{

    #region Properties

    [ObservableProperty]
    string roomName = default!;

    [ObservableProperty]
    string roomId = default!;

    [ObservableProperty]
    string roomSeed = default!;

    [ObservableProperty]
    Wheel earlyWheel = new();

    [ObservableProperty]
    Wheel middleWheel = new();

    [ObservableProperty]
    Wheel endWheel = new();

    [ObservableProperty]
    DateTime? startDate;

    [ObservableProperty]
    DateTime? endDate;

    [ObservableProperty]
    bool isGameStarted = false;

    [ObservableProperty]
    bool isGameEnded = false;

    [ObservableProperty]
    int currentTimerTime;

    [ObservableProperty]
    RoomSettings roomSettings = new();

    [ObservableProperty]
    bool isCreatorMode;

    [ObservableProperty]
    bool isConnectedToServer;
    
    public Player CurrentPlayer => 
        Players.FirstOrDefault(i => i.Id == App.CurrentPlayer.Id) ?? new Player
        {
            Id = App.CurrentPlayer.Id,            
        };

    [ObservableProperty]
    ObservableCollection<Player> players = [];

    [ObservableProperty]
    List<Player> roomPlayers = [];

    [ObservableProperty]
    string? password;

    public string TimeString => TimeSpan.FromSeconds(RoomSettings.TimerSeconds - TimerCounter.TotalSeconds).ToString(@"hh\:mm\:ss");

    [ObservableProperty]
    bool isTimerStarted;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TimeString))]
    TimeSpan timerCounter = TimeSpan.Zero;

    [ObservableProperty]
    bool isEarlyWheelVisible = true;
    
    [ObservableProperty]
    bool isMiddleWheelVisible = false;
    
    [ObservableProperty]
    bool isEndWheelVisible = false;

    [ObservableProperty]
    string currentDebuff;

    #endregion

    #region Constructors

    public Room()
    {
        Players = new ObservableCollection<Player>();
    }
    public Room(string roomName, string roomId)
    {
        Players = new ObservableCollection<Player>();
        RoomName = roomName;
        RoomId = roomId;
    }


    #endregion

    #region Methods

    public async Task InitRoom()
    {
        //CurrentPlayer = Players.FirstOrDefault(i => i.Id == App.CurrentPlayer.Id) ?? new();
        Players.CollectionChanged += (s, e) =>
        {
            OnPropertyChanged(nameof(RoomPlayers));
            OnPropertyChanged(nameof(CurrentPlayer));
        };

        foreach (var item in Players)
        {
            item.IsCurrentPlayer = item.Id == CurrentPlayer.Id;
        }

        RoomPlayers = Players.ToList();
        await ConnectToSocketAsync();
    }

    public async Task ConnectToSocketAsync()
    {
        await App.SignalRHub.ConnectToRoomHub(RoomId);
    }

    public async Task StartGame()
    {
        if (IsGameStarted)
            return;

        await App.SignalRHub.SendStartGame(RoomId);
    }

    public void UpdatePlayersGoals()
    {
        foreach (var item in Players)
        {
            var score = 0;
            foreach (var obj in EarlyWheel.Objectives.Where(i => i.PlayerIds.Any(j => j == item.Id)))
            {
                score++;
                score += obj.Modifiers.Where(i => i.PlayerIds.Any(j => j == item.Id)).Sum(i => i.Points);
            }

            foreach (var obj in MiddleWheel.Objectives.Where(i => i.PlayerIds.Any(j => j == item.Id)))
            {
                score++;
                score += obj.Modifiers.Where(i => i.PlayerIds.Any(j => j == item.Id)).Sum(i => i.Points);
            }

            foreach (var obj in EndWheel.Objectives.Where(i => i.PlayerIds.Any(j => j == item.Id)))
            {
                score++;
                score += obj.Modifiers.Where(i => i.PlayerIds.Any(j => j == item.Id)).Sum(i => i.Points);
            }

            item.Score = score;
        }
    }

    public async Task RotateWheel(int angle)
    {
        await App.SignalRHub.SendRotateWheel(RoomId, angle);

    }

    public async Task MarkObjective(Objective objective)
    {
        await App.SignalRHub.SendMarkObjective(RoomId, objective.Slot);
    }

    public async Task MarkObjectiveModifier(ObjectiveModifier modifier)
    {
        var slot = modifier.Slot.Split("_")[0];
        await App.SignalRHub.SendMarkObjectiveModifier(RoomId, slot, modifier.Slot);
    }

    public async Task FinishCurrentObjective(Objective objective)
    {
        await App.SignalRHub.SendFinishCurrentObjective(RoomId, objective.Slot, CurrentPlayer.Id);
    }

    public async Task GetNewRandomDebuff()
    {
        await App.SignalRHub.SendGetNewRandomDebuff(RoomId);
    }

    public async Task DisconnectAsync()
    {
        await App.SignalRHub.DisconnectFromRoomHub(RoomId);
    }

    public Objective? FindObjectiveBySlot(string slot)
    {
        var objective = EarlyWheel.Objectives.FirstOrDefault(o => o.Slot == slot) ??
                        MiddleWheel.Objectives.FirstOrDefault(o => o.Slot == slot) ??
                        EndWheel.Objectives.FirstOrDefault(o => o.Slot == slot);

        return objective;
    }

    public void RemoveObjectiveBySlot(string slot)
    {
        var obj = EarlyWheel.Objectives.FirstOrDefault(o => o.Slot == slot);
        if (obj!=null)
        {
            obj.IsCompleted = true;
            if (!EarlyWheel.Objectives.Any(i => !i.IsCompleted))
            {
                IsEarlyWheelVisible = false;
                IsMiddleWheelVisible = true;
            }
            return;
        }
        obj = MiddleWheel.Objectives.FirstOrDefault(o => o.Slot == slot);
        if (obj != null)
        {
            obj.IsCompleted = true;
            if (!MiddleWheel.Objectives.Any(i => !i.IsCompleted))
            {
                IsMiddleWheelVisible = false;
                IsEndWheelVisible = true;
            }
            return;
        }
        obj = EndWheel.Objectives.FirstOrDefault(o => o.Slot == slot);
        if (obj != null)
        {
            obj.IsCompleted = true;
            if (!EndWheel.Objectives.Any(i => !i.IsCompleted))
            {                
                IsEndWheelVisible = false;
            }
        }
    }

    #endregion
}