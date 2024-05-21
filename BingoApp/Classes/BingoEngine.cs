using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using BingoApp.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Threading;
using System.Text.Encodings.Web;

namespace BingoApp.Classes
{
    public class BingoEngine
    {
        private const string baseUrl = "https://bingosync.com";
        private const string boardUrl = "https://bingosync.com/room/{0}/board";
        private const string feedUrl = "https://bingosync.com/room/{0}/feed";


        public CookieContainer cookieContainer;
        public HttpClientHandler httpClientHandler;
        public HttpClient httpClient;

        private string currentBingoUrl = "";
        public BingoEngine()
        {
            cookieContainer = new CookieContainer();
            httpClientHandler = new HttpClientHandler() { CookieContainer = cookieContainer, UseCookies = true, AllowAutoRedirect = true };
            httpClient = new HttpClient(httpClientHandler);
        }

        public async Task<RoomConnectionInfo> GetRoomInfoAsync(string url)
        {
            try
            {
                cookieContainer = new CookieContainer();
                httpClientHandler = new HttpClientHandler() { CookieContainer = cookieContainer, UseCookies = true, AllowAutoRedirect = true };
                httpClient = new HttpClient(httpClientHandler);

                var html = await httpClient.GetStringAsync(url);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                var roomName = doc.GetElementbyId("id_room_name");
                var creatorName = doc.GetElementbyId("id_creator_name");
                var gameName = doc.GetElementbyId("id_game_name");
                var roomUuid = doc.GetElementbyId("id_encoded_room_uuid");
                var form = doc.DocumentNode.Descendants("form").FirstOrDefault();
                var csrfToken = form?.ChildNodes.Where(i => i.Name == "input" && i.Attributes.Any(j => j.Name == "name")).FirstOrDefault(i => i.Attributes["name"].Value == "csrfmiddlewaretoken");
                if (roomName == null || creatorName == null || gameName == null || roomUuid == null || csrfToken == null)
                {
                    throw new Exception("Wrong link");
                }

                var roomInfo = new RoomConnectionInfo()
                {
                    RoomId = url.Replace("https://bingosync.com/room/", ""),
                    RoomName = System.Web.HttpUtility.HtmlDecode(roomName.Attributes["value"].Value),
                    Creator = System.Web.HttpUtility.HtmlDecode(creatorName.Attributes["value"].Value),
                    Game = System.Web.HttpUtility.HtmlDecode(gameName.Attributes["value"].Value),
                    EncodedRoomUUID = roomUuid.Attributes["value"].Value,
                    CsrfMiddlewareToken = csrfToken?.Attributes["value"].Value ?? "",
                    RoomActionUrl = form?.Attributes["action"].Value ?? ""
                };

                return roomInfo;
            }
            catch (Exception ex)
            {
                throw new Exception("Board not found");
            }
        }

        public async Task<Room> ConnectToRoomAsync(RoomConnectionInfo roomInfo, PlayerCredentials credentials, CancellationToken token, bool needSafe = true)
        {
            var formData = new List<KeyValuePair<string, string>>();
            formData.Add(new KeyValuePair<string, string>("csrfmiddlewaretoken", roomInfo.CsrfMiddlewareToken));
            formData.Add(new KeyValuePair<string, string>("encoded_room_uuid", roomInfo.EncodedRoomUUID));
            formData.Add(new KeyValuePair<string, string>("room_name", roomInfo.RoomName));
            formData.Add(new KeyValuePair<string, string>("creator_name", roomInfo.Creator));
            formData.Add(new KeyValuePair<string, string>("game_name", roomInfo.Game));
            formData.Add(new KeyValuePair<string, string>("player_name", credentials.NickName));
            formData.Add(new KeyValuePair<string, string>("passphrase", credentials.Password));
            if (credentials.IsSpectator)
            {
                formData.Add(new KeyValuePair<string, string>("is_spectator", "on"));
            }

            var response = await httpClient.PostAsync(baseUrl + roomInfo.RoomActionUrl, new FormUrlEncodedContent(formData), token);
            if (response.IsSuccessStatusCode)
            {
                var room = await Task.Run<Room>(async () =>
                {
                    var room = new Room(roomInfo.RoomName, roomInfo.RoomId, roomInfo.CsrfMiddlewareToken);
                    var responseMessage = await response.Content.ReadAsStringAsync(token);
                    if (responseMessage.Contains("Incorrect Password"))
                    {
                        throw new Exception("Incorrect Password!");
                    }
                    var stringToFind = "var temporarySocketKey = \"";
                    if (responseMessage.IndexOf(stringToFind) != -1)
                    {
                        var startPosition = responseMessage.IndexOf(stringToFind) + stringToFind.Length;
                        var endPosition = responseMessage.IndexOf("\";", startPosition);
                        var length = endPosition - startPosition;
                        var socketTempId = responseMessage.Substring(startPosition, length);
                        room.SocketTempId = socketTempId;
                    }

                    stringToFind = "ROOM_SETTINGS = JSON.parse('";
                    if (responseMessage.IndexOf(stringToFind) != -1)
                    {
                        var startPosition = responseMessage.IndexOf(stringToFind) + stringToFind.Length;
                        var endPosition = responseMessage.IndexOf("');", startPosition);
                        var length = endPosition - startPosition;
                        var roomSettingsJson = responseMessage.Substring(startPosition, length).Replace("\\u0022", "\"");
                        var roomjObj = JObject.Parse(roomSettingsJson);
                        room.RoomSettings = new RoomSettings()
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
                        room.CurrentPlayer = new Player()
                        {
                            IsSpectator = playerjObj["is_spectator"].Value<bool>(),
                            Uuid = playerjObj["uuid"].Value<string>(),
                            Color = (BingoColor)Enum.Parse(typeof(BingoColor), playerjObj["color"].Value<string>()),
                            NickName = playerjObj["name"].Value<string>()
                        };
                        
                        room.ChosenColor = room.CurrentPlayer.Color;
                    }

                    var doc = new HtmlDocument();
                    doc.LoadHtml(responseMessage);

                    var playerPanel = doc.GetElementbyId("players-panel");
                    var playersEntrys = playerPanel.Elements("div");
                    foreach (var entry in playersEntrys)
                    {
                        var player = new Player();
                        player.Uuid = entry.Attributes["id"].Value;
                        player.NickName = entry.Elements("span").FirstOrDefault(i => i.Attributes["class"].Value == "playername").InnerText.Trim();
                        var color = entry.Elements("span").FirstOrDefault(i => i.Attributes["class"].Value.Contains("goalcounter")).Attributes["class"].Value;
                        color = color.Replace("goalcounter", "").Replace("square", "").Trim();
                        player.Color = (BingoColor)Enum.Parse(typeof(BingoColor), color);

                        room.Players.Add(player);
                    }
                    room.PlayerCredentials = credentials;
                    await room.InitRoom(httpClient);
                    
                    if (needSafe && !room.CurrentPlayer.IsSpectator)
                        await room.SaveAsync();

                    return room;

                }, token);
                return room;
            }
            return null;
        }

        public async Task<Room> CreateRoomAsync(NewBoardModel boardModel)
        {
            try
            {
                var html = await httpClient.GetStringAsync(baseUrl);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                var form = doc.DocumentNode.Descendants("form").FirstOrDefault();
                var csrfToken = form?.ChildNodes.Where(i => i.Name == "input" && i.Attributes.Any(j => j.Name == "name")).FirstOrDefault(i => i.Attributes["name"].Value == "csrfmiddlewaretoken")?.Attributes["value"].Value;

                var formData = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("csrfmiddlewaretoken", csrfToken ?? ""),
                    new KeyValuePair<string, string>("room_name", boardModel.RoomName),
                    new KeyValuePair<string, string>("passphrase", boardModel.Password),
                    new KeyValuePair<string, string>("nickname", boardModel.NickName),
                    new KeyValuePair<string, string>("game_type", boardModel.Game.ToString()),
                    new KeyValuePair<string, string>("variant_type", boardModel.Variant.ToString()),
                    new KeyValuePair<string, string>("custom_json", boardModel.BoardJSON),
                    new KeyValuePair<string, string>("lockout_mode", boardModel.RoomLockoutMode.Id.ToString()),
                    new KeyValuePair<string, string>("seed", boardModel.Seed?.ToString() ?? "")
                };

                if (boardModel.HideCard)
                    formData.Add(new KeyValuePair<string, string>("hide_card", "on"));
                if (boardModel.AsSpectator)
                    formData.Add(new KeyValuePair<string, string>("is_spectator", "on"));


                var response = await httpClient.PostAsync(baseUrl, new FormUrlEncodedContent(formData));
                if (response.IsSuccessStatusCode)
                {
                    var responseMessage = await response.Content.ReadAsStringAsync();

                    if (responseMessage.Contains("Couldn't parse board json"))
                    {
                        throw new Exception("Wrong Json");
                    }

                    var roomId = "";
                    var stringToFind = "window.sessionStorage.setItem(\"room\", \"";
                    if (responseMessage.IndexOf(stringToFind) != -1)
                    {
                        var startPosition = responseMessage.IndexOf(stringToFind) + stringToFind.Length;
                        var endPosition = responseMessage.IndexOf("\");", startPosition);
                        var length = endPosition - startPosition;
                        roomId = responseMessage.Substring(startPosition, length);
                    }

                    var room = new Room(boardModel.RoomName, roomId, csrfToken, boardModel.BoardJSON);
                    stringToFind = "var temporarySocketKey = \"";
                    if (responseMessage.IndexOf(stringToFind) != -1)
                    {
                        var startPosition = responseMessage.IndexOf(stringToFind) + stringToFind.Length;
                        var endPosition = responseMessage.IndexOf("\";", startPosition);
                        var length = endPosition - startPosition;
                        var socketTempId = responseMessage.Substring(startPosition, length);
                        room.SocketTempId = socketTempId;
                    }

                    stringToFind = "ROOM_SETTINGS = JSON.parse('";
                    if (responseMessage.IndexOf(stringToFind) != -1)
                    {
                        var startPosition = responseMessage.IndexOf(stringToFind) + stringToFind.Length;
                        var endPosition = responseMessage.IndexOf("');", startPosition);
                        var length = endPosition - startPosition;
                        var roomSettingsJson = responseMessage.Substring(startPosition, length).Replace("\\u0022", "\"");
                        var roomjObj = JObject.Parse(roomSettingsJson);
                        room.RoomSettings = new RoomSettings()
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
                        room.CurrentPlayer = new Player()
                        {
                            IsSpectator = playerjObj["is_spectator"].Value<bool>(),
                            Uuid = playerjObj["uuid"].Value<string>(),
                            Color = (BingoColor)Enum.Parse(typeof(BingoColor), playerjObj["color"].Value<string>()),
                            NickName = playerjObj["name"].Value<string>()
                        };                        
                        room.ChosenColor = room.CurrentPlayer.Color;
                    }

                    room.PlayerCredentials = new PlayerCredentials()
                    {
                        Password = boardModel.Password,
                        NickName = boardModel.NickName,
                        IsSpectator = boardModel.AsSpectator
                    };
                    await room.InitRoom(httpClient);
                    room.IsCreatorMode = true;

                    return room;
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Room> LoadRoomFromFile(string roomPath)
        {
            var roomJson = await System.IO.File.ReadAllTextAsync(roomPath);
            var oldroom = JsonConvert.DeserializeObject<Room>(roomJson);
            var roomInfo = await GetRoomInfoAsync(baseUrl + $"/room/{oldroom.RoomId}");
            var room = await ConnectToRoomAsync(roomInfo, oldroom.PlayerCredentials, CancellationToken.None, false);
            
            if (!oldroom.CurrentPlayer.IsSpectator)
                await room.ChangeCollor(oldroom.ChosenColor);

            room.StartDate = oldroom.StartDate;
            room.IsRevealed = oldroom.IsRevealed;
            room.IsGameStarted = oldroom.IsGameStarted;
            room.IsGameEnded = oldroom.IsGameEnded;
            room.CurrentTimerTime = oldroom.CurrentTimerTime;
            room.ChosenColor = oldroom.ChosenColor;
            room.IsCreatorMode = oldroom.IsCreatorMode;
            room.IsAutoBoardReveal = oldroom.IsAutoBoardReveal;
            room.BoardJSON = oldroom.BoardJSON;

            for (int i = 0; i < oldroom.Board.Squares.Count; i++)
            {
                var oldsqaure = oldroom.Board.Squares[i];
                var newsqaure = room.Board.Squares[i];

                newsqaure.IsGoal = oldsqaure.IsGoal;
                newsqaure.Score = oldsqaure.Score;
            }

            return room;

        }

    }
}
