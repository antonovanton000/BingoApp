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
using System.Windows.Media;
using static System.Net.WebRequestMethods;

namespace BingoApp.Models
{
    public partial class Room : ObservableObject
    {
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

        [ObservableProperty]
        BingoColor chosenColor;

        [ObservableProperty]
        string socketTempId;

        [ObservableProperty]
        string roomName;

        [ObservableProperty]
        string roomId;

        [ObservableProperty]
        string roomSeed;

        [ObservableProperty]
        Board board;

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
        [NotifyPropertyChangedFor(nameof(IsBoardHided))]
        RoomSettings roomSettings;

        [ObservableProperty]
        Player currentPlayer;

        [ObservableProperty]
        ObservableCollection<Event> chatMessages = new ObservableCollection<Event>();

        [ObservableProperty]
        ObservableCollection<Player> players = new ObservableCollection<Player>();

        public IEnumerable<Player> ActualPlayers => Players.Where(i => !i.IsSpectator);
        public string Spectators => string.Join(", ", Players.Where(i => i.IsSpectator).Select(i => i.NickName));

        public bool HasSpectators => Players?.Any(i => i.IsSpectator) ?? false;

        public string PlayersNames => string.Join(", ", Players.Select(i => i.NickName));

        public string TimeString => TimeSpan.FromSeconds(CurrentTimerTime).ToString(@"hh\:mm\:ss");

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsBoardHided))]
        bool isRevealed = false;

        [JsonIgnore]
        public bool IsBoardHided => RoomSettings.HideCard && !IsRevealed;

        [JsonIgnore]
        WsClient wsClient;

        [JsonIgnore]
        HttpClient client;

        [ObservableProperty]
        PlayerCredentials playerCredentials;
        
        public event EventHandler NewCardEvent;
        
        public string CsrfMiddlewareToken { get; set; }

        public string? BoardJSON { get; set; }

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
            ChatMessages = new ObservableCollection<Event>();
            RoomName = roomName;
            RoomId = roomId;

            Players.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(ActualPlayers));
                OnPropertyChanged(nameof(Spectators));
                OnPropertyChanged(nameof(HasSpectators));
            };
            CsrfMiddlewareToken = csrfMiddlewareToken;
            BoardJSON = boardJSON;
        }

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
                MainWindow.ShowToast(new ToastInfo() { Title = "New board created!" });
                Board = newboard;
            }
            else
            {
                MainWindow.ShowToast(new ToastInfo() { Title = "Error happend when new board created!", ToastType = ToastType.Error });
            }

            NewCardEvent?.Invoke(sender, e);
        }

        public async Task InitRoom(HttpClient httpClient)
        {
            client = httpClient;
            await ConnectToSocketAsync(SocketTempId);
            Board = await GetBoardAsync();
            await GetChatFeed(true);
        }

        private async Task ConnectToSocketAsync(string tempSocketId)
        {
            await wsClient.ConnectAsync(socketUrl);
            await wsClient.SendTempSocketKeyAsync(tempSocketId);
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
            }
            else
            {
                // otherwise update the player's color
                var player = Players.FirstOrDefault(i => i.Uuid == playerObj["uuid"].Value<string>());
                var newColor = (BingoColor)Enum.Parse(typeof(BingoColor), playerObj["color"].Value<string>());
                //foreach (var cell in Board.Squares.Where(i => i.Colors.Any(j => j == player.Color)))
                //{
                //    cell.Colors.Remove(player.Color);
                //    cell.Colors.Add(newColor);
                //}
                if (CurrentPlayer.Uuid == playerObj["uuid"].Value<string>())
                    CurrentPlayer.Color = newColor;

                player.Color = newColor;
            }
        }

        public void RemovePlayer(string uuid)
        {
            var player = Players.FirstOrDefault(i => i.Uuid == uuid);
            if (player != null)
            {
                Players.Remove(player);
            }
        }

        public void UpdatePlayersGoals()
        {
            if (Board != null)
            {
                foreach (var item in Players)
                {
                    item.SquaresCount = Board.GetColorCount(item.Color);
                    item.LinesCount = Board.GetLinesCount(item.Color);
                    item.PotentialBingos = Board.GetPotentialBingos(item);
                }
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
                            Type = (EventType)Enum.Parse(typeof(EventType), item["type"].Value<string>().Replace("-","")),
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
                MainWindow.ShowToast(new ToastInfo() { ToastType = ToastType.Error, Title="Error", Detail="Error happend when new board created!" });
            }            
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
                foreach (var entry in playersEntrys)
                {
                    var player = new Player();
                    player.Uuid = entry.Attributes["id"].Value;
                    player.NickName = entry.Elements("span").FirstOrDefault(i => i.Attributes["class"].Value == "playername").InnerText.Trim();
                    var color = entry.Elements("span").FirstOrDefault(i => i.Attributes["class"].Value.Contains("goalcounter")).Attributes["class"].Value;
                    color = color.Replace("goalcounter", "").Replace("square", "").Trim();
                    player.Color = (BingoColor)Enum.Parse(typeof(BingoColor), color);

                    Players.Add(player);
                }
                await ConnectToSocketAsync(SocketTempId);
                var newboard = await GetBoardAsync();
                if (newboard!=null)
                    Board = newboard;

                await GetChatFeed(true);

            }
        }
    }
}
