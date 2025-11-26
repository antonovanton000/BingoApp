using BingoApp.Classes;
using CommunityToolkit.Mvvm.ComponentModel;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
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

namespace BingoApp.Models
{
    public partial class Room : ObservableObject
    {
        #region Properties

        [ObservableProperty]
        string roomName = default!;

        [ObservableProperty]
        string roomId = default!;

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
        [NotifyPropertyChangedFor(nameof(TimerString))]
        [NotifyPropertyChangedFor(nameof(StartingTimerString))]
        [NotifyPropertyChangedFor(nameof(AfterRevealTimerString))]
        TimeSpan timerCounter;
        public string TimerString => TimerCounter.ToString(@"hh\:mm\:ss");
        public string StartingTimerString => (TimeSpan.FromSeconds(RoomSettings.StartTimeSeconds) - TimerCounter).ToString(@"hh\:mm\:ss");
        public string AfterRevealTimerString => (TimeSpan.FromSeconds(RoomSettings.AfterRevealSeconds) - TimerCounter).ToString(@"hh\:mm\:ss");

        [ObservableProperty]
        bool isCreatorMode;

        [ObservableProperty]
        bool isConnectedToServer;

        [ObservableProperty]
        RoomSettings roomSettings = new();

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
        List<PresetSquare> presetSquares = [];

        public IEnumerable<Player> ActualPlayers => Players.Where(i => !i.IsSpectator).OrderBy(i => i.Color);

        public string Spectators => Players.Where(i => i.IsSpectator).Count().ToString();

        public bool HasSpectators => Players?.Any(i => i.IsSpectator) ?? false;

        public string PlayersNames => string.Join(", ", Players.Select(i => i.NickName));

        public string TimeString => TimerCounter.ToString(@"hh\:mm\:ss");

        [JsonIgnore]
        public List<PlayerTeam> PlayerTeams => Players.GroupBy(i => i.Color).Select(i => new PlayerTeam() { Color = i.First().Color, Players = i.ToList() }).OrderBy(i => i.Color).ToList();

        [ObservableProperty]
        string? password;

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

        #endregion

        #region Constructors

        public Room()
        {
            Board = new();
            Players = new ObservableCollection<Player>();
        }

        public Room(string roomName, string roomId)
        {
            Players = new ObservableCollection<Player>();
            ChatMessages = new ObservableCollection<Event>();
            RoomName = roomName;
            RoomId = roomId;
        }

        #endregion

        #region Events

        public event EventHandler? NewCardEvent;

        public event EventHandler<Square>? OnMarkSquareRecieved;

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
            OnPropertyChanged(nameof(ActualPlayers));
            OnPropertyChanged(nameof(Spectators));
            OnPropertyChanged(nameof(HasSpectators));
            OnPropertyChanged(nameof(RoomPlayers));
            OnPropertyChanged(nameof(PlayerTeams));
        }

        public async Task ConnectToSocketAsync()
        {
            await App.SignalRHub.ConnectToRoomHub(RoomId);
        }

        public void UpdatePlayersGoals(Square? markedSquare = null)
        {
            if (Board != null)
            {



                foreach (var item in Players)
                {
                    var lines = item.LinesCount;
                    item.SquaresCount = Board.GetColorCount(item.Color);
                    item.LinesCount = Board.GetLinesCount(item.Color);
                    item.PotentialBingos = Board.GetPotentialBingos(item);
                    item.PotentialBingosCount = Board.GetPotentialBingosCount(item);
                    

                    if (IsPractice)
                    {

                        if (item.LinesCount > lines && markedSquare != null)
                        {
                            Board.AnimateBingoLine(item.Color, markedSquare);

                            if (RoomSettings.GameMode == GameMode.Triple)
                            {
                                if (item.LinesCount == 3)
                                {
                                    ChatMessages.Add(new Event()
                                    {
                                        Type = EventType.bingo,
                                        Player = item,
                                        Timestamp = DateTime.Now,
                                    });
                                    IsGameEnded = true;
                                }
                            }
                            if (RoomSettings.GameMode == GameMode.Lockout)
                            {
                                ChatMessages.Add(new Event()
                                {
                                    Type = EventType.bingo,
                                    Player = item,
                                    Timestamp = DateTime.Now,
                                });
                                IsGameEnded = true;
                            }
                            if (RoomSettings.GameMode == GameMode.Blackout)
                            {
                                if (item.SquaresCount == 25)
                                {
                                    ChatMessages.Add(new Event()
                                    {
                                        Type = EventType.bingo,
                                        Player = item,
                                        Timestamp = DateTime.Now,
                                    });
                                    IsGameEnded = true;
                                }
                            }

                        }
                    }
                    else
                    {
                        if (item.LinesCount > lines && markedSquare != null)
                        {
                            Board.AnimateBingoLine(item.Color, markedSquare);
                        }
                    }
                }

                foreach (var item in RoomPlayers)
                {
                    var lines = item.LinesCount;
                    item.SquaresCount = Board.GetColorCount(item.Color);
                    item.LinesCount = Board.GetLinesCount(item.Color);
                    item.PotentialBingos = Board.GetPotentialBingos(item);
                    item.PotentialBingosCount = Board.GetPotentialBingosCount(item);
                }

                var playersScore = Players.Select(i => new PlayerScore()
                {
                    PlayerId = i.Id,
                    Score = i.SquaresCount,
                    LinesCount = i.LinesCount
                }).ToArray();

                OnUpdatePlayerGoals?.Invoke(this, playersScore);
            }
        }

        public void UpdatePlayerTeams()
        {
            OnPropertyChanged(nameof(ActualPlayers));
            OnPropertyChanged(nameof(PlayerTeams));
        }

        public async Task RevealTheBoard()
        {
            if (CurrentPlayer.IsBoardRevealed)
                return;

            await App.SignalRHub.SendRevealBoard(RoomId, CurrentPlayer.Id);
        }

        public async Task ChangeColor(BingoColor color)
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

        public async Task SendSelectBingoLine(string playerId, string lineName)
        {
            await App.SignalRHub.SendSelectBingoLine(RoomId, playerId, lineName);
        }

        public async Task MarkSquare(Square square)
        {
            if (IsPractice)
            {
                square.IsMarking = true;
                await Task.Delay(200);
                if (square.SquareColors.Contains(CurrentPlayer.Color))
                {
                    square.SquareColors.Remove(CurrentPlayer.Color);
                    ChatMessages.Add(new Event() { PlayerColor = CurrentPlayer.Color, Player = CurrentPlayer, Square = square, Type = EventType.goal, Remove = true, Timestamp = DateTime.Now });
                }
                else
                {
                    square.SquareColors.Add(CurrentPlayer.Color);
                    ChatMessages.Add(new Event() { PlayerColor = CurrentPlayer.Color, Player = CurrentPlayer, Square = square, Type = EventType.goal, Remove = false, Timestamp = DateTime.Now });

                }
                UpdatePlayersGoals(square);
                square.IsMarking = false;
            }
            else
            {
                if (RoomSettings.GameMode == GameMode.Lockout)
                {
                    if (square.SquareColors.Contains(CurrentPlayer.Color) || square.SquareColors.Count == 0)
                    {
                        square.IsMarking = true;
                        await App.SignalRHub.SendMarkSquare(RoomId, square.Slot, CurrentPlayer.Color);
                    }
                }
                else
                {
                    await App.SignalRHub.SendMarkSquare(RoomId, square.Slot, CurrentPlayer.Color);
                }
            }
        }

        public void TriggerMarkSquareEvent(Square square)
        {
            OnMarkSquareRecieved?.Invoke(this, square);
        }

        public void TriggerRevealBoard()
        {
            OnBoardReveal?.Invoke(this, EventArgs.Empty);
        }

        public void TriggerNewBoardGenerated()
        {
            OnNewBoardGenerated?.Invoke(this, EventArgs.Empty);
        }
        public void TriggerPlayerTeamsUpdate()
        {
            OnPropertyChanged(nameof(PlayerTeams));
            OnPropertyChanged(nameof(ActualPlayers));
            OnPropertyChanged(nameof(Spectators));
            OnPropertyChanged(nameof(HasSpectators));
        }

        public async Task DisconnectAsync()
        {
            await App.SignalRHub.DisconnectFromRoomHub(RoomId);
        }

        public async Task SaveAsync()
        {
            var dirName = System.IO.Path.Combine(App.Location, "ActiveRooms");
            if (!System.IO.Directory.Exists(dirName))
                System.IO.Directory.CreateDirectory(dirName);

            var fileName = System.IO.Path.Combine(dirName, RoomName + "_" + RoomId + ".json");
            if (IsPractice)
            {
                var json = JsonConvert.SerializeObject(this, Formatting.Indented);
                await System.IO.File.WriteAllTextAsync(fileName, json);
            }
            else
            {
                var password = !string.IsNullOrEmpty(Password) ? Convert.ToBase64String(Encoding.UTF8.GetBytes(this.Password)) : Password;
                var json = JsonConvert.SerializeObject(new
                {
                    RoomId,
                    RoomName,
                    Password = password,
                    CurrentPlayer
                }, Formatting.Indented);

                await System.IO.File.WriteAllTextAsync(fileName, json);
            }
        }

        public async Task SaveToHistoryAsync()
        {

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
            try
            {
                var roomJson = await System.IO.File.ReadAllTextAsync(roomPath);
                var oldroom = JsonConvert.DeserializeObject<Room>(roomJson);
                if (oldroom == null) return null;
                if (!oldroom.IsPractice)
                {
                    oldroom.Password = !string.IsNullOrEmpty(oldroom.Password) ? Encoding.UTF8.GetString(Convert.FromBase64String(oldroom.Password)) : null;
                }
                return oldroom;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task GenerateNewBoardAsync()
        {
            if (!IsCreatorMode) return;

            if (RoomSettings.HideCard)
                CurrentPlayer.IsBoardRevealed = false;

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

                TimerCounter = serverRoom.TimerCounter;
                IsTimerOnPause = serverRoom.IsTimerOnPause;
                IsTimerStarted = serverRoom.IsTimerStarted;
                IsGameStarted = serverRoom.IsGameStarted;
                IsGameEnded = serverRoom.IsGameEnded;
                LastChangeMinute = serverRoom.LastChangeMinute;
                CurrentHiddenStep = serverRoom.CurrentHiddenStep;
            }
        }

        public async Task ProcessHiddenGame()
        {
            if (Board == null) return;
            if (PresetSquares == null) return;
            if (!IsCreatorMode) return;

            await App.SignalRHub.SendUnhideSquareAsync(RoomId, CurrentHiddenStep);
            CurrentHiddenStep++;
        }

        public async Task UnhideSquares(string slot)
        {
            var square = Board.Squares.FirstOrDefault(i => i.Slot == slot);
            if (square != null)
            {
                square.IsHidden = false;
                square.IsReplaceNewAnimate = true;
                await Task.Delay(1000);
                square.IsReplaceNewAnimate = false;
            }
        }

        public async Task ProcessChangingGame(int minute)
        {
            if (Board == null) return;
            if (PresetSquares == null) return;
            if (!IsCreatorMode) return;

            LastChangeMinute = minute;

            await App.SignalRHub.SendChangeSquareAsync(RoomId, minute);
        }

        public async Task ChangeSquare(string slot, string name)
        {
            var square = Board.Squares.FirstOrDefault(i => i.Slot == slot);
            if (square != null)
            {
                square.Name = name;
                square.IsReplaceNewAnimate = true;
                OnChangeGameStep?.Invoke(this, new ChangeGameSquareeEventArgs() { SlotId = slot, NewSquareName = name });
                await Task.Delay(1000);
                square.IsReplaceNewAnimate = false;
            }
        }

        public RoomServerSync GetRoomServerSyncData()
        {
            return new RoomServerSync()
            {
                RoomId = RoomId,
                PresetName = RoomSettings.PresetName,
                GameName = RoomSettings.GameName,
                GameMode = RoomSettings.GameMode,
                GameExtraMode = RoomSettings.ExtraGameMode,
                IsGameStarted = IsGameStarted,
                IsGameEnded = IsGameEnded,
                StartDate = StartDate,
                EndDate = EndDate,
                CurrentTimerTime = (int)TimerCounter.TotalSeconds,
                IsAutoBoardReveal = RoomSettings.IsAutoBoardReveal,
                CurrentHiddenStep = CurrentHiddenStep,
                IsHiddenGameInited = IsHiddenGameInited,
                LastChangeMinute = LastChangeMinute,
            };
        }

        public void GeneratePracticeBoard()
        {
            if (IsPractice)
            {
                var rnd = new Random();
                var rndSquares = PresetSquares.OrderBy(x => rnd.NextDouble()).Take(25);
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
                    boardSqares.Add(s);
                    col++;
                    if (col == 5)
                    {
                        col = 0;
                        row++;
                    }
                }
                Board = new Board() { Squares = new ObservableCollection<Square>(boardSqares) };
                TriggerNewBoardGenerated();
            }
        }

        #endregion
    }

    public enum GameMode
    {
        Lockout,
        Triple,
        Blackout,
        Other
    }

    public enum ExtraGameMode
    {
        None,
        Hidden,
        Changing
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

}
