using CommunityToolkit.Mvvm.ComponentModel;
using HuntpointApp.Classes;
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

namespace HuntpointApp.Models;

public partial class Room : ObservableObject
{

    #region Properties

    [ObservableProperty]
    HuntpointColor chosenColor;

    [ObservableProperty]
    string roomName = default!;

    [ObservableProperty]
    string roomId = default!;

    [ObservableProperty]
    string roomSeed = default!;

    [ObservableProperty]
    Board board = new();

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
    [NotifyPropertyChangedFor(nameof(IsBoardHided))]
    RoomSettings roomSettings = new();

    [ObservableProperty]
    bool isCreatorMode;

    [ObservableProperty]
    bool isConnectedToServer;

    [ObservableProperty]
    Player currentPlayer = new();

    [ObservableProperty]
    ObservableCollection<Event> chatMessages = [];

    [ObservableProperty]
    ObservableCollection<Player> players = [];

    [ObservableProperty]
    List<Player> roomPlayers = [];

    [ObservableProperty]
    [JsonIgnore]
    List<PresetObjective> presetObjectives = [];

    [ObservableProperty]
    string? password;

    public IEnumerable<Player> ActualPlayers => Players.Where(i => !i.IsSpectator).OrderBy(i => i.Color);
    public string Spectators => string.Join(", ", Players.Where(i => i.IsSpectator).Select(i => i.NickName));

    public bool HasSpectators => Players?.Any(i => i.IsSpectator) ?? false;

    public string PlayersNames => string.Join(", ", Players.Select(i => i.NickName));

    public string TimeString => TimeSpan.FromSeconds(CurrentTimerTime).ToString(@"hh\:mm\:ss");

    [JsonIgnore]
    public List<PlayerTeam> PlayerTeams => Players.GroupBy(i => i.Color).Select(i => new PlayerTeam() { Color = i.First().Color, Players = i.ToList() }).ToList();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsBoardHided))]
    bool isRevealed = false;

    [JsonIgnore]
    public bool IsBoardHided => RoomSettings.HideCard && !IsRevealed;

    public string? BoardJSON { get; set; }
    public string? PresetJSON { get; set; }

    [ObservableProperty]
    bool isPractice = false;

    [ObservableProperty]
    bool isHiddenGameInited = false;

    [ObservableProperty]
    int currentHiddenStep = 1;

    [ObservableProperty]
    int lastChangeMinute = 0;

    [ObservableProperty]
    bool isTimerStarted;

    [ObservableProperty]
    bool isTimerOnPause;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAutoRevealBoardVisible))]
    bool isStartTimerStarted = false;

    [ObservableProperty]
    bool isAfterRevealTimerStarted = false;

    [ObservableProperty]
    bool isGameTimerStarted = true;

    [ObservableProperty]
    bool isAutoRevealBoardVisible;

    [ObservableProperty]
    string? legendaryObjective;
    public uint? NightreignSeed { get; set; }
    public int? NightreignMapPattern { get; set; }
    public string? PatternDescribe { get; set; } = default!;

    #endregion

    #region Constructors

    public Room()
    {
        Players = new ObservableCollection<Player>();
    }
    public Room(string roomName, string roomId, string? boardJSON = null)
    {
        Players = new ObservableCollection<Player>();
        ChatMessages = new ObservableCollection<Event>();
        RoomName = roomName;
        RoomId = roomId;

        BoardJSON = boardJSON;
    }


    #endregion

    #region Events

    public event EventHandler? NewCardEvent;

    public event EventHandler<Objective>? OnMarkObjectiveRecieved;

    public event EventHandler<UnhideGameSquareEventArgs>? OnUnhideGameStep;

    public event EventHandler<ChangeGameSquareeEventArgs> OnChangeGameStep;

    public event EventHandler? OnBoardReveal;

    public event EventHandler<Player>? OnPlayerConnected;

    public event EventHandler<Player>? OnPlayerDisconnected;

    public event EventHandler<Player>? OnPlayerChangeColor;

    public event EventHandler<PlayerScore[]>? OnUpdatePlayerGoals;

    public event EventHandler<Player>? OnRoomClosed;

    public event EventHandler? OnNewBoardGenerated;

    public event EventHandler? OnRefreshRoom;

    #endregion

    #region Methods

    public async Task InitRoom()
    {
        CurrentPlayer = Players.FirstOrDefault(i => i.Id == App.CurrentPlayer.Id) ?? new();
        Players.CollectionChanged += (s, e) =>
        {
            OnPropertyChanged(nameof(ActualPlayers));
            OnPropertyChanged(nameof(Spectators));
            OnPropertyChanged(nameof(HasSpectators));
            OnPropertyChanged(nameof(RoomPlayers));
            OnPropertyChanged(nameof(PlayerTeams));
        };

        foreach (var item in Players)
        {
            item.IsCurrentPlayer = item.Id == CurrentPlayer.Id;
        }
        RoomPlayers = Players.ToList();
        if (!IsPractice)
        {
            await ConnectToSocketAsync();
        }
    }

    public async Task ConnectToSocketAsync()
    {
        await App.SignalRHub.ConnectToRoomHub(RoomId);
    }

    public void UpdatePlayersGoals()
    {
        foreach (var item in ActualPlayers)
        {
            item.Score = Board.Objectives.Where(i => i.ObjectiveColors.Any(j => j == item.Color)).Sum(i => i.Score);
            if (RoomSettings.IsLegendaryExtraPoints)
            {
                if (item.IsFirstLegendaryMarked)
                {
                    item.Score += 10;
                }
            }
        }
    }

    public void UpdatePlayerTeams()
    {
        OnPropertyChanged(nameof(PlayerTeams));
    }

    public async Task GenerateNewBoardAsync()
    {
        if (!IsCreatorMode) return;

        if (RoomSettings.HideCard)
            IsRevealed = false;

        await App.RestClient.GenerateNewBoard(RoomId);
    }

    public async Task RefreshRoomAsync()
    {
        var serverRoomResp = await App.RestClient.ConnectToRoom(new ConnectRoomModel()
        {
            RoomId = RoomId,
            Password = Password,
            AsSpectator = CurrentPlayer.IsSpectator,
            PlayerId = CurrentPlayer.Id
        });
        if (serverRoomResp.IsSuccess)
        {
            var serverRoom = serverRoomResp.Data;
            Board = serverRoom.Board;

            ChatMessages.Clear();
            foreach (var item in serverRoom.ChatMessages)
            {
                ChatMessages.Add(item);
            }

            Players.Clear();
            foreach (var item in serverRoom.Players)
            {
                item.IsCurrentPlayer = item.Id == CurrentPlayer.Id;
                Players.Add(item);
            }
            RoomPlayers = Players.ToList();
        }
    }

    public async Task MarkObjective(Objective objective)
    {
        objective.IsMarking = true;
        if (IsPractice)
        {
            await Task.Delay(200);
            if (objective.ObjectiveColors.Contains(CurrentPlayer.Color))
            {
                objective.ObjectiveColors.Remove(CurrentPlayer.Color);
                ChatMessages.Add(new Event() { PlayerColor = CurrentPlayer.Color, Player = CurrentPlayer, Objective = objective, Type = EventType.goal, Remove = true, Timestamp = DateTime.Now });
            }
            else
            {
                objective.ObjectiveColors.Add(CurrentPlayer.Color);
                ChatMessages.Add(new Event() { PlayerColor = CurrentPlayer.Color, Player = CurrentPlayer, Objective = objective, Type = EventType.goal, Remove = false, Timestamp = DateTime.Now });

            }
            objective.IsMarking = false;
        }
        else
        {
            await App.SignalRHub.SendMarkObjective(RoomId, objective.Slot, CurrentPlayer.Color);
        }
    }

    public async Task ChangeColor(HuntpointColor color)
    {
        CurrentPlayer.Color = color;
        if (!IsPractice)
        {
            await App.SignalRHub.SendPlayerChangeColor(RoomId, CurrentPlayer.Color);
        }
    }

    public async Task SendChatMessage(string message)
    {
        await App.SignalRHub.SendChatMessage(RoomId, message);
    }

    public async Task RevealTheBoard()
    {
        if (IsRevealed)
            return;
        IsRevealed = true;
        await App.SignalRHub.SendRevealBoard(RoomId, CurrentPlayer.Id);
    }

    public void TriggerRevealBoard()
    {
        OnBoardReveal?.Invoke(this, EventArgs.Empty);
    }

    public void TriggerNewBoardGenerated()
    {
        OnNewBoardGenerated?.Invoke(this, EventArgs.Empty);
    }

    public async Task DisconnectAsync()
    {
        await App.SignalRHub.DisconnectFromRoomHub(RoomId);
    }

    public async Task SaveAsync()
    {
        var json = JsonConvert.SerializeObject(this, Formatting.Indented);

        var dirName = System.IO.Path.Combine(App.Location, "ActiveRooms");
        if (!System.IO.Directory.Exists(dirName))
            System.IO.Directory.CreateDirectory(dirName);

        var fileName = System.IO.Path.Combine(dirName, RoomName + "_" + RoomId + ".json");

        await System.IO.File.WriteAllTextAsync(fileName, json);
    }

    public async Task SaveToHistoryAsync()
    {
        this.BoardJSON = "";

        foreach (var item in RoomPlayers)
        {
            if (!Players.Any(i => i.Id == item.Id))
            {
                if (!item.IsSpectator)
                {
                    Players.Add(item);
                }
            }
        }

        var json = JsonConvert.SerializeObject(this, Formatting.Indented);

        var dirName = System.IO.Path.Combine(App.Location, "HistoryRooms");
        if (!System.IO.Directory.Exists(dirName))
            System.IO.Directory.CreateDirectory(dirName);

        var fileName = System.IO.Path.Combine(dirName, RoomName + "_" + RoomId + ".json");

        await System.IO.File.WriteAllTextAsync(fileName, json);
    }

    public void RemoveFromActiveRooms()
    {
        var dirName = System.IO.Path.Combine(App.Location, "ActiveRooms");
        var fileName = System.IO.Path.Combine(dirName, RoomName + "_" + RoomId + ".json");
        if (System.IO.File.Exists(fileName))
            System.IO.File.Delete(fileName);
    }

    public void RemoveFromHistory()
    {
        var dirName = System.IO.Path.Combine(App.Location, "HistoryRooms");
        var fileName = System.IO.Path.Combine(dirName, RoomName + "_" + RoomId + ".json");
        if (System.IO.File.Exists(fileName))
            System.IO.File.Delete(fileName);
    }

    public static async Task<Room?> LoadRoomFromFile(string roomPath)
    {
        var roomJson = await System.IO.File.ReadAllTextAsync(roomPath);
        var oldroom = JsonConvert.DeserializeObject<Room>(roomJson);
        return oldroom;
    }

    public void GeneratePracticeBoard()
    {
        if (IsPractice)
        {
            IsRevealed = false;

            var presetObjectives = JsonConvert.DeserializeObject<List<PresetObjective>>(PresetJSON);
            if (presetObjectives == null || presetObjectives.Count == 0)
            {
                throw new ArgumentException("Invalid JSON format or empty preset objectives.");
            }

            var ram = new Random();
            // Shuffle the preset objectives
            var regularObjectives = presetObjectives?.Where(i => i.Type == ObjectiveType.Regular).OrderBy(i => ram.NextDouble()).Take(10).ToList();
            var rareObjectives = presetObjectives?.Where(i => i.Type == ObjectiveType.Rare).OrderBy(i => ram.NextDouble()).Take(6).ToList();
            var uniquObjectives = presetObjectives?.Where(i => i.Type == ObjectiveType.Unique).OrderBy(i => ram.NextDouble()).Take(3).ToList();

            PresetObjective? legendaryObjective;
            if (this.LegendaryObjective == null)
            {
                legendaryObjective = presetObjectives?.Where(i => i.Type == ObjectiveType.Legendary).OrderBy(i => ram.NextDouble()).FirstOrDefault();
            }
            else
            {
                legendaryObjective = presetObjectives?.FirstOrDefault(i => i.Name.Equals(this.LegendaryObjective, StringComparison.OrdinalIgnoreCase) && i.Type == ObjectiveType.Legendary);
                if (legendaryObjective == null)
                {
                    throw new ArgumentException($"Legendary objective '{this.LegendaryObjective}' not found in the provided JSON.");
                }
            }

            Board.Objectives.Clear();
            var slot = 1;
            if (regularObjectives != null && regularObjectives.Count == 10)
            {
                foreach (var item in regularObjectives)
                {
                    Board.Objectives.Add(new Objective
                    {
                        Slot = $"slot{slot++}",
                        Name = item.Name,
                        Score = item.Score,
                        Type = item.Type,
                        ObjectiveColors = new()
                    });
                }
            }
            else
            {
                throw new ArgumentException("Invalid JSON format or not enough regular objectives.");
            }

            if (rareObjectives != null && rareObjectives.Count == 6)
            {
                foreach (var item in rareObjectives)
                {
                    Board.Objectives.Add(new Objective
                    {
                        Slot = $"slot{slot++}",
                        Name = item.Name,
                        Score = item.Score,
                        Type = item.Type
                    });
                }
            }
            else
            {
                throw new ArgumentException("Invalid JSON format or not enough unique objectives.");
            }

            if (uniquObjectives != null && uniquObjectives.Count == 3)
            {
                foreach (var item in uniquObjectives)
                {
                    Board.Objectives.Add(new Objective
                    {
                        Slot = $"slot{slot++}",
                        Name = item.Name,
                        Score = item.Score,
                        Type = item.Type
                    });
                }
            }
            else
            {
                throw new ArgumentException("Invalid JSON format or not enough unique objectives.");
            }

            if (legendaryObjective != null)
            {
                Board.Objectives.Add(new Objective
                {
                    Slot = $"slot{slot++}",
                    Name = legendaryObjective.Name,
                    Score = legendaryObjective.Score,
                    Type = legendaryObjective.Type
                });
            }
            else
            {
                throw new ArgumentException("Invalid JSON format or not enough legendary objectives.");
            }
        }
    }

    #endregion
}

public class UnhideGameSquareEventArgs : EventArgs
{
    public string SlotId { get; set; } = default!;
}

public class ChangeGameSquareeEventArgs : EventArgs
{
    public string SlotId { get; set; } = default!;

    public string NewSquareName { get; set; } = default!;
}

