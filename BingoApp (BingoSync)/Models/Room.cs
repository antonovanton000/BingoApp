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
        #region Constants

        private const string baseUrl = "https://bingosync.com";
        private const string socketUrl = "wss://sockets.bingosync.com/broadcast";
        private const string boardUrl = baseUrl + "/room/{0}/board";
        private const string chatHistoryUrl = baseUrl + "/room/{0}/feed";
        private const string chatUrl = baseUrl + "/api/chat";
        private const string colorSelectedUrl = baseUrl + "/api/color";
        private const string goalSelectedUrl = baseUrl + "/api/select";
        private const string boardRevealedUrl = baseUrl + "/api/revealed";
        private const string roomSettingsUrl = baseUrl + "/room/{0}/room-settings";
        private const string newBoardUrl = baseUrl + "/api/new-card";

        #endregion

        #region Properties

        [ObservableProperty]
        BingoColor chosenColor;

        [ObservableProperty]
        string socketTempId = default!;

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
        bool isAutoBoardReveal;

        [ObservableProperty]
        bool isCreatorMode;

        [ObservableProperty]
        bool isConnectedToServer;

        [ObservableProperty]
        GameMode gameMode = GameMode.Other;

        [ObservableProperty]
        ExtraGameMode gameExtraMode = ExtraGameMode.None;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsBoardHided))]
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
       
        [ObservableProperty]
        PlayerCredentials playerCredentials = default!;

        public string CsrfMiddlewareToken { get; set; } = "";

        public string? BoardJSON { get; set; }

        [ObservableProperty]
        bool isPractice = false;

        [ObservableProperty]
        string presetName = default!;

        [ObservableProperty]
        string gameName = default!;

        [ObservableProperty]
        bool isHiddenGameInited = false;

        [ObservableProperty]
        int currentHiddenStep = 1;

        [ObservableProperty]
        int lastChangeMinute = 0;


        [JsonIgnore]
        WsClient wsClient = default!;

        [JsonIgnore]
        HttpClient client = default!;

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
            wsClient = new WsClient(this);
            wsClient.NewCardEvent += WsClient_NewCardEvent;
            Players = new ObservableCollection<Player>();
        }
        public Room(string roomName, string roomId, string csrfMiddlewareToken, string? boardJSON = null)
        {
            Players = new ObservableCollection<Player>();
            wsClient = new WsClient(this);
            wsClient.NewCardEvent += WsClient_NewCardEvent;
            wsClient.Connected += WsClient_Connected;
            wsClient.Disconnected += WsClient_Disconnected;
            ChatMessages = new ObservableCollection<Event>();
            RoomName = roomName;
            RoomId = roomId;

            Players.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(ActualPlayers));
                OnPropertyChanged(nameof(Spectators));
                OnPropertyChanged(nameof(HasSpectators));
                OnPropertyChanged(nameof(RoomPlayers));
                OnPropertyChanged(nameof(PlayerTeams));
            };
            CsrfMiddlewareToken = csrfMiddlewareToken;
            BoardJSON = boardJSON;
        }


        #endregion

        #region EventHandlers
        private async void WsClient_NewCardEvent(object? sender, EventArgs e)
        {
            if (RoomSettings.HideCard)
            {
                IsRevealed = false;
            }

            await Task.Delay(2000);
            var newboard = await GetBoardAsync();
            if (newboard != null)
            {
                MainWindow.ShowToast(new ToastInfo() { Title = App.Current.FindResource("mes_newboardcreated").ToString() });
                Board = newboard;
            }
            else
            {
                MainWindow.ShowToast(new ToastInfo() { Title = App.Current.FindResource("mes_errorhappendwhe").ToString(), ToastType = ToastType.Error });
            }

            NewCardEvent?.Invoke(sender, e);
        }
        private void WsClient_Disconnected(object? sender, EventArgs e)
        {
            IsConnectedToServer = false;
        }
        private void WsClient_Connected(object? sender, EventArgs e)
        {
            IsConnectedToServer = true;
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

        public async Task InitRoom(HttpClient httpClient)
        {
            client = httpClient;
            if (IsPractice) return;
            await ConnectToSocketAsync();
            Board = await GetBoardAsync();
            await GetChatFeed(true);
        }

        public async Task ConnectToSocketAsync()
        {
            await wsClient.ConnectAsync(socketUrl);
            await wsClient.SendTempSocketKeyAsync(SocketTempId);
        }

        public async Task<Board> GetBoardAsync()
        {
            var board = new Board();
            try
            {
                var boardJson = await client.GetStringAsync(string.Format(boardUrl, RoomId));
                var jarray = JArray.Parse(boardJson);
                var rowindex = 0;
                var columnindex = 0;
                foreach (var item in jarray)
                {
                    var cell = new Square()
                    {
                        Slot = item["slot"].Value<string>(),
                        Name = item["name"].Value<string>(),
                        Row = rowindex,
                        Column = columnindex++,
                    };

                    var arr = item["colors"].Value<string>().Split(" ");
                    foreach (var cl in arr)
                    {
                        cell.SquareColors.Add((BingoColor)Enum.Parse(typeof(BingoColor), cl));
                    }
                    if (columnindex > 4)
                    {
                        rowindex++;
                        columnindex = 0;
                    }

                    board.Squares.Add(cell);
                }

            }
            catch (Exception ex)
            {
                return null;
            }
            return board;
        }

        public void AddNewPlayer(JObject playerObj)
        {

            if (!Players.Any(i => i.Uuid == playerObj["uuid"].Value<string>()))
            {
                // insert if the uuid is not already listed
                var player = new Player()
                {
                    IsSpectator = playerObj["is_spectator"].Value<bool>(),
                    Uuid = playerObj["uuid"].Value<string>(),
                    NickName = playerObj["name"].Value<string>(),
                    Color = (BingoColor)Enum.Parse(typeof(BingoColor), playerObj["color"].Value<string>()),
                };
                Players.Add(player);
                RoomPlayers.Add(player);
                OnPlayerConnected?.Invoke(this, player);
            }
            else
            {
                // otherwise update the player's color
                var player = Players.FirstOrDefault(i => i.Uuid == playerObj["uuid"].Value<string>());
                var roomPlayer = RoomPlayers.FirstOrDefault(i => i.Uuid == playerObj["uuid"].Value<string>());
                if (player != null && roomPlayer != null)
                {
                    var newColor = (BingoColor)Enum.Parse(typeof(BingoColor), playerObj["color"].Value<string>());
                    if (CurrentPlayer.Uuid == playerObj["uuid"].Value<string>())
                        CurrentPlayer.Color = newColor;

                    player.Color = newColor;
                    roomPlayer.Color = newColor;
                    TriggerPlayerTeamsUpdate();
                    OnPlayerChangeColor?.Invoke(this, player);
                }
            }
        }

        public void RemovePlayer(string uuid)
        {
            var player = Players.FirstOrDefault(i => i.Uuid == uuid);
            if (player != null)
            {
                Players.Remove(player);
                OnPlayerDisconnected?.Invoke(this, player);
            }
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

                    if (item.LinesCount > lines && markedSquare != null)
                    {
                        Board.AnimateBingoLine(item.Color, markedSquare);

                        if (GameMode == GameMode.TripleBingo)
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
                        if (GameMode == GameMode.Lockout)
                        {
                            ChatMessages.Add(new Event()
                            {
                                Type = EventType.bingo,
                                Player = item,
                                Timestamp = DateTime.Now,
                            });
                            IsGameEnded = true;
                        }
                        if (GameMode == GameMode.Blackout)
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

                foreach (var item in RoomPlayers)
                {
                    var lines = item.LinesCount;
                    item.SquaresCount = Board.GetColorCount(item.Color);
                    item.LinesCount = Board.GetLinesCount(item.Color);
                    item.PotentialBingos = Board.GetPotentialBingos(item);
                    item.PotentialBingosCount = Board.GetPotentialBingosCount(item);
                }

                if (GameMode == GameMode.Lockout)
                {                    
                    foreach (var item in Players)
                    {
                        if (item.LinesCount == 0 && item.SquaresCount >=13 && Players.Except([item]).Sum(i => i.PotentialBingosCount) == 0)
                        {
                            ChatMessages.Add(new Event()
                            {
                                Type = EventType.win,
                                Player = item,
                                Timestamp = DateTime.Now,
                            });
                            IsGameEnded = true;
                        }
                    }
                }

                var playersScore = Players.Select(i => new PlayerScore()
                {
                    PlayerId = i.Uuid,
                    Score = i.SquaresCount,
                    LinesCount = i.LinesCount
                }).ToArray();

                OnUpdatePlayerGoals?.Invoke(this, playersScore);
            }
        }

        public async Task GetChatFeed(bool isFull)
        {
            var response = await client.GetAsync(string.Format(chatHistoryUrl, RoomId) + (isFull ? "?full=true" : ""));
            if (response.IsSuccessStatusCode)
            {
                var chatJson = await response.Content.ReadAsStringAsync();
                var jobj = JObject.Parse(chatJson);
                var eventsArray = jobj["events"] as JArray;
                if (eventsArray != null)
                {
                    foreach (JObject item in eventsArray)
                    {
                        var chatEvent = new Event()
                        {
                            Type = (EventType)Enum.Parse(typeof(EventType), item["type"].Value<string>().Replace("-", "")),
                            Timestamp = App.UnixTimeStampToDateTime(item["timestamp"].Value<double>()),

                            Player = new Player()
                            {
                                IsSpectator = item["player"]["is_spectator"].Value<bool>(),
                                Uuid = item["player"]["uuid"].Value<string>(),
                                NickName = item["player"]["name"].Value<string>(),
                                Color = (BingoColor)Enum.Parse(typeof(BingoColor), item["player"]["color"].Value<string>())
                            },
                            PlayerColor = (BingoColor)Enum.Parse(typeof(BingoColor), item["player_color"].Value<string>())
                        };

                        if (item.ContainsKey("event_type"))
                        {
                            chatEvent.EventType = (EventSubType)Enum.Parse(typeof(EventSubType), item["event_type"].Value<string>());
                        }
                        if (item.ContainsKey("remove"))
                        {
                            chatEvent.Remove = item["remove"].Value<bool>();
                        }
                        if (item.ContainsKey("square"))
                        {
                            var arr = item["square"]["colors"].Value<string>().Split(" ");
                            var colors = new ObservableCollection<BingoColor>();
                            foreach (var cl in arr)
                            {
                                colors.Add((BingoColor)Enum.Parse(typeof(BingoColor), cl));
                            }
                            chatEvent.Square = new Square()
                            {
                                Slot = item["square"]["slot"].Value<string>(),
                                Name = item["square"]["name"].Value<string>(),
                                SquareColors = colors
                            };
                        }
                        if (chatEvent.Type == EventType.chat)
                        {
                            chatEvent.Message = item["text"].Value<string>();
                        }
                        if (chatEvent.Type == EventType.color)
                        {
                            chatEvent.Color = (BingoColor)Enum.Parse(typeof(BingoColor), item["color"].Value<string>());
                        }

                        ChatMessages.Add(chatEvent);
                    }
                }
            }

        }

        public async Task<bool> RevealTheBoard()
        {
            var json = new JObject();
            json["room"] = RoomId;
            var stringContent = new StringContent(json.ToString());
            var resp = await client.PostAsync(boardRevealedUrl, stringContent);
            if (resp.IsSuccessStatusCode)
            {
                OnBoardReveal?.Invoke(this, EventArgs.Empty);
                return true;
            }
            return false;
        }

        public async Task<bool> ChangeCollor(BingoColor color)
        {
            var json = new JObject();
            json["room"] = RoomId;
            json["color"] = color.ToString();
            var stringContent = new StringContent(json.ToString());
            var resp = await client.PostAsync(colorSelectedUrl, stringContent);
            if (resp.IsSuccessStatusCode)
            {
                ChosenColor = color;
                return true;
            }
            return false;
        }

        public async Task<bool> SendChatMessage(string message)
        {
            var json = new JObject();
            json["room"] = RoomId;
            json["text"] = message;
            var stringContent = new StringContent(json.ToString());
            var resp = await client.PostAsync(chatUrl, stringContent);
            if (resp.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> MarkSquare(Square square)
        {

            square.IsMarking = true;
            if (IsPractice)
            {
                await Task.Delay(500);
                square.IsMarking = false;
                if (square.IsMarked)
                {                 
                    square.SquareColors.Clear();
                    square.SquareColors.Add(BingoColor.blank);
                    var chatEvent = new Event()
                    {
                        Type = EventType.goal,
                        Timestamp = DateTime.Now,
                        Player = CurrentPlayer,
                        PlayerColor = CurrentPlayer.Color,
                        Square = square,
                        Remove = true
                    };
                    ChatMessages.Add(chatEvent);
                }
                else
                {
                    square.SquareColors.Clear();
                    square.SquareColors.Add(CurrentPlayer.Color);

                    var chatEvent = new Event()
                    {
                        Type = EventType.goal,
                        Timestamp = DateTime.Now,
                        Player = CurrentPlayer,
                        PlayerColor = CurrentPlayer.Color,
                        Square = square
                    };
                    ChatMessages.Add(chatEvent);
                }
                UpdatePlayersGoals(square);
                TriggerMarkSquareEvent(square);
                //ChatMessages.Add(new Event() { });
                return true;
            }

            var removeColor = false;
            // the square is blank and we're painting it
            if (!square.IsMarked)
            {
                removeColor = false;
            }
            // the square is colored the same as the chosen color so we're clearing it (or just removing the chosen color from the square's colors)
            else if (square.SquareColors.Any(i => i == ChosenColor))
            {
                removeColor = true;
            }
            // the square is a different color, but we allow multiple colors, so add it
            else if (RoomSettings.LockoutMode != "Lockout")
            {
                removeColor = false;
            }
            // the square is colored a different color and we don't allow multiple colors, so don't do anything
            else
            {
                return false;
            }

            var json = new JObject();
            json["room"] = RoomId;
            json["slot"] = square.Slot.Replace("slot", "");
            json["color"] = ChosenColor.ToString();
            json["remove_color"] = removeColor;
            var stringContent = new StringContent(json.ToString());
            try
            {
                var resp = await client.PutAsync(goalSelectedUrl, stringContent);                
                return resp.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                IsConnectedToServer = false;
            }
            return false;
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
            await wsClient.DisconnectAsync();
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
                if (!Players.Any(i => i.Uuid == item.Uuid))
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

        public async Task GenerateNewBoardAsync()
        {
            if (!IsCreatorMode || string.IsNullOrEmpty(BoardJSON))
                return;

            var jobj = new JObject();
            jobj["hide_card"] = RoomSettings.HideCard;
            jobj["csrfmiddlewaretoken"] = CsrfMiddlewareToken;
            jobj["variant_type"] = RoomSettings.VariantId;
            jobj["game_type"] = RoomSettings.GameId;
            jobj["lockout_mode"] = RoomSettings.LockoutMode == "Lockout" ? 1 : 2;
            jobj["custom_json"] = BoardJSON;
            jobj["seed"] = "";
            jobj["room"] = RoomId;


            var stringContent = new StringContent(jobj.ToString());
            var response = await client.PostAsync(newBoardUrl, stringContent);
            if (!response.IsSuccessStatusCode)
            {
                MainWindow.ShowToast(new ToastInfo() { ToastType = ToastType.Error, Title = "Error", Detail = App.Current.FindResource("mes_errorhappendwhe").ToString() });
            }
            OnNewBoardGenerated?.Invoke(this, EventArgs.Empty);
        }

        public async Task RefreshRoomAsync()
        {
            var response = await client.GetAsync(baseUrl + "/room/" + this.RoomId);
            if (response.IsSuccessStatusCode)
            {
                var responseMessage = await response.Content.ReadAsStringAsync();
                var stringToFind = "var temporarySocketKey = \"";
                if (responseMessage.IndexOf(stringToFind) != -1)
                {
                    var startPosition = responseMessage.IndexOf(stringToFind) + stringToFind.Length;
                    var endPosition = responseMessage.IndexOf("\";", startPosition);
                    var length = endPosition - startPosition;
                    var socketTempId = responseMessage.Substring(startPosition, length);
                    SocketTempId = socketTempId;
                }

                stringToFind = "ROOM_SETTINGS = JSON.parse('";
                if (responseMessage.IndexOf(stringToFind) != -1)
                {
                    var startPosition = responseMessage.IndexOf(stringToFind) + stringToFind.Length;
                    var endPosition = responseMessage.IndexOf("');", startPosition);
                    var length = endPosition - startPosition;
                    var roomSettingsJson = responseMessage.Substring(startPosition, length).Replace("\\u0022", "\"");
                    var roomjObj = JObject.Parse(roomSettingsJson);
                    RoomSettings = new RoomSettings()
                    {
                        HideCard = roomjObj["hide_card"].Value<bool>(),
                        Variant = roomjObj["variant"].Value<string>(),
                        LockoutMode = roomjObj["lockout_mode"].Value<string>(),
                        Seed = roomjObj["seed"].Value<long>(),
                        Game = roomjObj["game"].Value<string>(),
                        GameId = roomjObj["game_id"].Value<int>(),
                        VariantId = roomjObj["variant_id"].Value<int>()
                    };
                }

                stringToFind = "var player = JSON.parse('";
                if (responseMessage.IndexOf(stringToFind) != -1)
                {
                    var startPosition = responseMessage.IndexOf(stringToFind) + stringToFind.Length;
                    var endPosition = responseMessage.IndexOf("');", startPosition);
                    var length = endPosition - startPosition;
                    var playerJson = responseMessage.Substring(startPosition, length).Replace("\\u0022", "\"");
                    var playerjObj = JObject.Parse(playerJson);
                    CurrentPlayer = new Player()
                    {
                        IsSpectator = playerjObj["is_spectator"].Value<bool>(),
                        Uuid = playerjObj["uuid"].Value<string>(),
                        Color = (BingoColor)Enum.Parse(typeof(BingoColor), playerjObj["color"].Value<string>()),
                        NickName = playerjObj["name"].Value<string>()
                    };

                    ChosenColor = CurrentPlayer.Color;
                }

                var doc = new HtmlDocument();
                doc.LoadHtml(responseMessage);
                var playerPanel = doc.GetElementbyId("players-panel");
                var playersEntrys = playerPanel.Elements("div");

                Players.Clear();
                RoomPlayers.Clear();
                foreach (var entry in playersEntrys)
                {
                    var player = new Player();
                    player.Uuid = entry.Attributes["id"].Value;
                    player.NickName = entry.Elements("span").FirstOrDefault(i => i.Attributes["class"].Value == "playername").InnerText.Trim();
                    var color = entry.Elements("span").FirstOrDefault(i => i.Attributes["class"].Value.Contains("goalcounter")).Attributes["class"].Value;
                    color = color.Replace("goalcounter", "").Replace("square", "").Trim();
                    player.Color = (BingoColor)Enum.Parse(typeof(BingoColor), color);

                    Players.Add(player);
                    RoomPlayers.Add(player);
                }
                await ConnectToSocketAsync();
                var newboard = await GetBoardAsync();
                if (newboard != null)
                    Board = newboard;

                await GetChatFeed(true);
                OnRefreshRoom?.Invoke(this, EventArgs.Empty);
            }
        }

        public void ProcessHiddenGame()
        {
            if (Board == null) return;
            if (IsHiddenGameInited) return;

            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    if (y % 2 == 0)
                    {
                        Board.Squares[y * 5 + x].IsHidden = x % 2 == 0;                        
                    }
                    else
                    {
                        Board.Squares[y * 5 + x].IsHidden = x % 2 != 0;
                    }
                }

            }
            IsHiddenGameInited = true;
        }

        public void UnhideSquares()
        {            
            if (CurrentHiddenStep == 1)
            {
                Board.Squares[6].IsHidden = false;
                AddUnhideChatMessage(Board.Squares[6]);
            }
            else if (CurrentHiddenStep == 2)
            {
                Board.Squares[18].IsHidden = false;
                AddUnhideChatMessage(Board.Squares[18]);
            }
            else if (CurrentHiddenStep == 3)
            {
                Board.Squares[2].IsHidden = false;
                AddUnhideChatMessage(Board.Squares[2]);
            }
            else if (CurrentHiddenStep  == 4)
            {
                Board.Squares[22].IsHidden = false;
                AddUnhideChatMessage(Board.Squares[22]);
            }
            else if (CurrentHiddenStep == 5)
            {
                Board.Squares[10].IsHidden = false;
                AddUnhideChatMessage(Board.Squares[10]);
            }
            else if (CurrentHiddenStep == 6)
            {
                Board.Squares[14].IsHidden = false;
                AddUnhideChatMessage(Board.Squares[14]);
            }
            else if (CurrentHiddenStep == 7)
            {
                Board.Squares[8].IsHidden = false;
                AddUnhideChatMessage(Board.Squares[8]);
            }
            else if (CurrentHiddenStep == 8)
            {
                Board.Squares[16].IsHidden = false;
                AddUnhideChatMessage(Board.Squares[16]);
            }
            else if (CurrentHiddenStep == 9)
            {
                Board.Squares[0].IsHidden = false;
                AddUnhideChatMessage(Board.Squares[0]);
            }
            else if (CurrentHiddenStep == 10)
            {
                Board.Squares[4].IsHidden = false;
                AddUnhideChatMessage(Board.Squares[4]);
            }
            else if (CurrentHiddenStep == 11)
            {
                Board.Squares[20].IsHidden = false;
                AddUnhideChatMessage(Board.Squares[20]);
            }
            else if (CurrentHiddenStep == 12)
            {
                Board.Squares[24].IsHidden = false;
                AddUnhideChatMessage(Board.Squares[24]);
            }
            else if (CurrentHiddenStep == 13)
            {
                Board.Squares[12].IsHidden = false;
                AddUnhideChatMessage(Board.Squares[12]);
            }
            CurrentHiddenStep++;
        }

        private void AddUnhideChatMessage(Square square)
        {
            OnUnhideGameStep?.Invoke(this, new UnhideGameSquareEventArgs() { SlotId = square.Slot });
            ChatMessages.Add(new Event()
            {
                Type = EventType.unhudesquare,
                Message = string.Format(App.Current.Resources["mes_squareunhide"].ToString(), square.Name),
                Timestamp = DateTime.Now,
                Square = square
            });
        }

        public async Task ProcessChangingGame(int minute)
        {
            if (Board == null) return;
            if (PresetSquares == null) return;
            if (!IsCreatorMode) return;

            LastChangeMinute = minute;
            var notMarkedSquares = Board.Squares.Where(i => !i.IsMarked).ToList();
            if (notMarkedSquares.Count == 0) return;
            var randomSquare = notMarkedSquares.OrderBy(x => Guid.NewGuid()).First();
            var newrandomSquare = PresetSquares.Where(i => !Board.Squares.Any(j => j.Name == i.Name)).OrderBy(x => Guid.NewGuid()).First();

            var oldSqareName = randomSquare.Name;
            randomSquare.Name = newrandomSquare.Name;
            randomSquare.IsReplaceNewAnimate = true;
            ChatMessages.Add(new Event()
            {
                Type = EventType.newsquare,
                Message = string.Format(App.Current.Resources["mes_squarechanged"].ToString(), oldSqareName),
                Timestamp = DateTime.Now,
                Square = randomSquare
            });
            OnChangeGameStep?.Invoke(this, new ChangeGameSquareeEventArgs() { SlotId = randomSquare.Slot, NewSquareName = randomSquare.Name });
            await App.SignalRHub.SendChangeSquareAsync(RoomId, randomSquare.Slot, newrandomSquare.Name);

            await Task.Delay(1000);
            randomSquare.IsReplaceNewAnimate = false;
        }

        public async Task ChangeSquare(string slot, string name)
        {
            var square = Board.Squares.FirstOrDefault(i => i.Slot == slot);
            if (square != null)
            {
                ChatMessages.Add(new Event()
                {
                    Type = EventType.newsquare,
                    Message = string.Format(App.Current.Resources["mes_squarechanged"].ToString(), square.Name),
                    Timestamp = DateTime.Now,
                    Square = square
                });
                square.Name = name;
                square.IsReplaceNewAnimate = true;
                await Task.Delay(1000);
                square.IsReplaceNewAnimate = false;
            }
        }

        public RoomServerSync GetRoomServerSyncData()
        {
            return new RoomServerSync()
            {
                RoomId = RoomId,
                PresetName = PresetName,
                GameName = GameName,                
                GameMode = GameMode,
                GameExtraMode = GameExtraMode,
                IsGameStarted = IsGameStarted,
                IsGameEnded = IsGameEnded,
                StartDate = StartDate,
                EndDate = EndDate,
                CurrentTimerTime = CurrentTimerTime,
                IsAutoBoardReveal = IsAutoBoardReveal,
                IsRevealed = IsRevealed,
                CurrentHiddenStep = CurrentHiddenStep,
                IsHiddenGameInited = IsHiddenGameInited,
                LastChangeMinute = LastChangeMinute,                
            };
        }

        #endregion
    }

    public enum GameMode
    {
        Lockout,
        Blackout,
        TripleBingo,
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
