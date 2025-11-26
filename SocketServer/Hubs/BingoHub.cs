using SocketServer.Models;
using SocketServer.Models.Bingo;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http.HttpResults;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Newtonsoft.Json;
using Player = SocketServer.Models.Player;

namespace SocketServer.Hubs
{
    public class BingoHub : Hub
    {

        #region Fields

        HashSet<Player> _players;

        HashSet<Room> _rooms;

        #endregion

        #region Constructor

        public BingoHub(HashSet<Player> players, HashSet<Room> rooms)
        {
            _players = players;
            _rooms = rooms;
        }

        #endregion

        #region Connect And Disconnect

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            var player = _players.FirstOrDefault(i => i.ConnectionId == connectionId);
            if (player != null)
            {
                player.IsOnline = false;
                if (!string.IsNullOrEmpty(player.ActiveRoomId))
                {
                    var room = _rooms.FirstOrDefault(r => r.RoomId == player.ActiveRoomId);
                    if (room != null)
                    {
                        await Groups.RemoveFromGroupAsync(connectionId, room.RoomId);
                        var roomPlayer = room.Players.FirstOrDefault(i => i.Id == player.Id);
                        if (roomPlayer != null)
                        {
                            room.Players.Remove(roomPlayer);
                            await AddNewEvent(room, new Event() { EventType = EventSubType.disconnected, Type = EventType.connection, Player = roomPlayer });
                        }
                        await Clients.Group(room.RoomId).SendAsync("PlayerDisconnected", player.Id);
                    }
                }
                player.ActiveRoomId = null;
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async void Connect(string playerId)
        {
            var connectionId = Context.ConnectionId;
            var player = _players.FirstOrDefault(p => p.Id == playerId);
            if (player != null)
            {
                player.IsOnline = true;
                player.ConnectionId = connectionId;
            }
            await Clients.Client(connectionId).SendAsync("Connected", playerId);
        }

        #endregion

        #region Room Management

        public async Task ConnectToRoom(string roomId)
        {
            if (Clients == null) return;

            var connectionId = Context.ConnectionId;
            var room = _rooms.FirstOrDefault(r => r.RoomId == roomId);
            if (room != null)
            {
                var player = _players.FirstOrDefault(p => p.ConnectionId == connectionId);
                if (player != null)
                {
                    player.ActiveRoomId = room.RoomId;
                    var roomPlayer = room.Players.FirstOrDefault(i => i.Id == player.Id);
                    if (roomPlayer == null)
                    {
                        roomPlayer = new Models.Bingo.Player(player.Id, player.NickName);
                        room.Players.Add(roomPlayer);
                        await Groups.AddToGroupAsync(connectionId, room.RoomId);
                        var json = JsonConvert.SerializeObject(roomPlayer);
                        await Clients.Group(roomId).SendAsync("PlayerConnected", json);
                    }
                    else
                    {
                        await Groups.AddToGroupAsync(connectionId, room.RoomId);
                        var json = JsonConvert.SerializeObject(roomPlayer);
                        await Clients.Group(roomId).SendAsync("PlayerConnected", json);
                    }
                    await AddNewEvent(room, new Event() { EventType = EventSubType.connected, Type = EventType.connection, Player = roomPlayer });
                }
            }

            CleanUpRooms();
        }

        public async Task DisconnectFromRoom(string roomId)
        {
            var connectionId = Context.ConnectionId;
            var room = _rooms.FirstOrDefault(r => r.RoomId == roomId);
            if (room != null)
            {
                await Groups.RemoveFromGroupAsync(connectionId, roomId);
                var player = _players.FirstOrDefault(p => p.ConnectionId == connectionId);
                if (player != null)
                {
                    player.ActiveRoomId = null;
                    var roomPlayer = room.Players.FirstOrDefault(i => i.Id == player.Id);
                    if (roomPlayer != null)
                    {
                        room.Players.Remove(roomPlayer);
                        await AddNewEvent(room, new Event() { EventType = EventSubType.disconnected, Type = EventType.connection, Player = roomPlayer });
                    }
                    await Clients.Group(roomId).SendAsync("PlayerDisconnected", player.Id);
                }
            }

            CleanUpRooms();
        }

        public async Task SendChatMessage(string roomId, string message)
        {
            if (Clients == null) return;

            var connectionId = Context.ConnectionId;
            var room = _rooms.FirstOrDefault(r => r.RoomId == roomId);
            if (room != null)
            {
                var player = _players.FirstOrDefault(p => p.ConnectionId == connectionId);
                if (player != null)
                {
                    var roomPlayer = room.Players.FirstOrDefault(i => i.Id == player.Id);
                    if (roomPlayer != null)
                    {
                        await AddNewEvent(room, new Event() { Type = EventType.chat, Player = roomPlayer, Message = message, PlayerColor = roomPlayer.Color });
                    }
                }
            }
        }

        public async Task PlayerChangeColor(string roomId, BingoColor color)
        {
            if (Clients == null) return;

            var connectionId = Context.ConnectionId;
            var room = _rooms.FirstOrDefault(r => r.RoomId == roomId);
            if (room != null)
            {
                var player = _players.FirstOrDefault(p => p.ConnectionId == connectionId);
                if (player != null)
                {
                    var roomPlayer = room.Players.FirstOrDefault(i => i.Id == player.Id);
                    if (roomPlayer != null)
                    {
                        roomPlayer.Color = color;
                        await Clients.Group(roomId).SendAsync("PlayerColorChanged", roomPlayer.Id, roomPlayer.Color);
                        await AddNewEvent(room, new Event() { Type = EventType.color, Player = roomPlayer, Color = color, PlayerColor = color });
                        await UpdatePlayersGoals(room);
                    }
                }
            }
        }

        public async Task MarkSquare(string roomId, string slotId, BingoColor color)
        {
            if (Clients == null) return;

            var connectionId = Context.ConnectionId;
            var room = _rooms.FirstOrDefault(r => r.RoomId == roomId);
            if (room != null)
            {
                var player = _players.FirstOrDefault(p => p.ConnectionId == connectionId);
                if (player != null)
                {
                    var roomPlayer = room.Players.FirstOrDefault(i => i.Id == player.Id);
                    var square = room.Board.Squares.FirstOrDefault(i => i.Slot == slotId);
                    if (square != null)
                    {
                        if (square.SquareColors.Any(i => i == color))
                        {
                            // If the color is already set, remove it
                            square.SquareColors.Remove(color);
                            await Clients.Group(roomId).SendAsync("UnmarkSquare", slotId, color);
                            await AddNewEvent(room, new Event() { Type = EventType.goal, Player = roomPlayer, Square = square, PlayerColor = roomPlayer.Color, Remove = true });
                        }
                        else
                        {
                            if (room.RoomSettings.GameMode == GameMode.Lockout)
                            {
                                if (square.SquareColors.Count == 0)
                                {
                                    // If the color is not set, add it
                                    square.SquareColors.Add(color);
                                    await Clients.Group(roomId).SendAsync("MarkSquare", slotId, color);
                                    await AddNewEvent(room, new Event() { Type = EventType.goal, Player = roomPlayer, Square = square, PlayerColor = roomPlayer.Color });
                                }
                            }
                            else
                            {
                                // If the color is not set, add it
                                square.SquareColors.Add(color);
                                await Clients.Group(roomId).SendAsync("MarkSquare", slotId, color);
                                await AddNewEvent(room, new Event() { Type = EventType.goal, Player = roomPlayer, Square = square, PlayerColor = roomPlayer.Color });
                            }

                        }

                        await UpdatePlayersGoals(room);
                    }
                }
            }
        }

        public async Task RevealBoard(string roomId, string playerId)
        {
            if (Clients == null) return;

            var connectionId = Context.ConnectionId;
            var room = _rooms.FirstOrDefault(r => r.RoomId == roomId);
            if (room != null)
            {
                var roomPlayer = room.Players.FirstOrDefault(i => i.Id == playerId);
                if (roomPlayer != null)
                {
                    roomPlayer.IsBoardRevealed = true;
                    await AddNewEvent(room, new Event() { Type = EventType.revealed, Player = roomPlayer });
                }

            }
        }

        public async Task SendNewRoomBoard(Room room)
        {
            if (Clients == null) return;
            if (room.RoomSettings.HideCard || room.RoomSettings.IsAutoBoardReveal)
            {
                foreach (var item in room.Players)
                {
                    item.IsBoardRevealed = false;
                    item.SquaresCount = 0;
                    item.LinesCount = 0;
                }
            }
            room.IsGameStarted = false;
            room.IsGameEnded = false;
            room.IsTimerOnPause = false;
            room.IsTimerStarted = false;
            room.LastChangeMinute = 0;
            room.TimerCounter = TimeSpan.Zero;
            await UpdatePlayersGoals(room);

            var json = JsonConvert.SerializeObject(room.Board);
            var creator = room.Players.FirstOrDefault(i => i.Id == room.CreatorId);
            await AddNewEvent(room, new Event() { Type = EventType.newcard, Player = creator });
            await Clients.Group(room.RoomId).SendAsync("NewBoard", json);
        }

        public async Task SelectBingoLine(string roomId, string playerId, string lineName)
        {
            if (Clients == null) return;

            var connectionId = Context.ConnectionId;
            var room = _rooms.FirstOrDefault(r => r.RoomId == roomId);
            if (room != null)
            {
                var roomPlayer = room.Players.FirstOrDefault(i => i.Id == playerId);
                if (roomPlayer != null)
                { 
                    roomPlayer.SelectedBingoLine = lineName;
                    var selectedPlayer = _players.FirstOrDefault(i => i.Id == playerId);
                    if (selectedPlayer != null && !string.IsNullOrEmpty(selectedPlayer.ConnectionId)) {
                        await Clients.Client(selectedPlayer.ConnectionId).SendAsync("SelectBingoLine", lineName);
                    }
                }
            }

        }

        #endregion

        #region Timer

        public async Task StartRoomGame(string roomId)
        {
            var room = _rooms.FirstOrDefault(r => r.RoomId == roomId);
            if (room != null)
            {
                room.IsGameStarted = true;
                await Clients.Group(roomId).SendAsync("StartRoomGame", roomId);
            }
        }

        public async Task PauseRoomGame(string roomId, TimeSpan currentTime)
        {
            var room = _rooms.FirstOrDefault(r => r.RoomId == roomId);
            if (room != null)
            {
                room.IsTimerOnPause = true;
                room.IsTimerStarted = false;
                room.TimerCounter = currentTime;
                await Clients.Group(roomId).SendAsync("PauseRoomGame", roomId, room.TimerCounter);
            }
        }

        public async Task ResumeRoomGame(string roomId, TimeSpan currentTime)
        {
            var room = _rooms.FirstOrDefault(r => r.RoomId == roomId);
            if (room != null)
            {
                room.IsTimerOnPause = false;
                room.IsTimerStarted = true;
                room.TimerCounter = currentTime;
                await Clients.Group(roomId).SendAsync("ResumeRoomGame", roomId, room.TimerCounter);
            }
        }

        public async Task StopRoomGame(string roomId)
        {
            var room = _rooms.FirstOrDefault(r => r.RoomId == roomId);
            if (room != null)
            {
                room.IsGameStarted = false;
                room.IsTimerStarted = false;
                room.IsTimerOnPause = false;
                await Clients.Group(roomId).SendAsync("StopRoomGame", roomId);
            }
        }

        public async Task ResetRoomTimer(string roomId)
        {
            var room = _rooms.FirstOrDefault(r => r.RoomId == roomId);
            if (room != null)
            {
                room.IsTimerOnPause = false;
                room.IsTimerStarted = false;
                room.TimerCounter = TimeSpan.Zero;
                await Clients.Group(roomId).SendAsync("ResetRoomTimer", roomId);
            }
        }

        public async Task SyncRoomTimer(string roomId, TimeSpan currentTime)
        {
            var room = _rooms.FirstOrDefault(r => r.RoomId == roomId);
            if (room != null)
            {
                room.TimerCounter = currentTime;
                await Clients.Group(roomId).SendAsync("SyncRoomTimer", roomId, currentTime);
            }
        }

        #endregion

        #region Changing Mode
        public async Task ChangeSquare(string roomId, int lastChangeMinute)
        {
            var room = _rooms.FirstOrDefault(i => i.RoomId == roomId);
            if (room != null)
            {
                room.LastChangeMinute = lastChangeMinute;
                var notMarkedSquares = room.Board.Squares.Where(i => !i.IsMarked).ToList();
                if (notMarkedSquares.Count == 0) return;
                var rnd = new Random();
                var randomSquare = notMarkedSquares.OrderBy(x => rnd.NextDouble()).First();
                var presetSquares = JsonConvert.DeserializeObject<List<PresetSquare>>(room.PresetJson);
                var newrandomSquare = presetSquares.Where(i => !room.Board.Squares.Any(j => j.Name == i.Name)).OrderBy(x => rnd.NextDouble()).First();

                var oldSqareName = randomSquare.Name;
                randomSquare.Name = newrandomSquare.Name;
                await AddNewEvent(room, new Event()
                {
                    Type = EventType.newsquare,
                    Message = oldSqareName,
                    Timestamp = DateTime.Now,
                    Square = randomSquare
                });
                await Clients.Group(roomId).SendAsync("ChangeSquare", roomId, randomSquare.Slot, randomSquare.Name);
            }

        }
        #endregion

        #region Unhide Mode

        public async Task UnhideSquare(string roomId, int step)
        {
            var room = _rooms.FirstOrDefault(i => i.RoomId == roomId);
            if (room != null)
            {
                room.CurrentHiddenStep = step;
                Square? square = null;
                if (room.CurrentHiddenStep == 1)
                {
                    square = room.Board.Squares[6];
                }
                else if (room.CurrentHiddenStep == 2)
                {
                    square = room.Board.Squares[18];
                }
                else if (room.CurrentHiddenStep == 3)
                {
                    square = room.Board.Squares[2];
                }
                else if (room.CurrentHiddenStep == 4)
                {
                    square = room.Board.Squares[22];
                }
                else if (room.CurrentHiddenStep == 5)
                {
                    square = room.Board.Squares[10];
                }
                else if (room.CurrentHiddenStep == 6)
                {
                    square = room.Board.Squares[14];
                }
                else if (room.CurrentHiddenStep == 7)
                {
                    square = room.Board.Squares[8];
                }
                else if (room.CurrentHiddenStep == 8)
                {
                    square = room.Board.Squares[16];
                }
                else if (room.CurrentHiddenStep == 9)
                {
                    square = room.Board.Squares[0];
                }
                else if (room.CurrentHiddenStep == 10)
                {
                    square = room.Board.Squares[4];
                }
                else if (room.CurrentHiddenStep == 11)
                {
                    square = room.Board.Squares[20];
                }
                else if (room.CurrentHiddenStep == 12)
                {
                    square = room.Board.Squares[24];
                }
                else if (room.CurrentHiddenStep == 13)
                {
                    square = room.Board.Squares[12];
                }
                if (square != null)
                {
                    square.IsHidden = false;
                    await Clients.Group(roomId).SendAsync("UnhideSquare", roomId, square.Slot);
                }
            }
        }

        #endregion

        #region Preset Creation
        public async Task InvitePlayer(string creatorId, string playerId, string presetId, string presetName, string presetJSON)
        {
            var creator = _players.FirstOrDefault(i => i.Id == creatorId);
            if (creator != null)
            {
                creator.IsOnline = true;
                if (string.IsNullOrEmpty(creator.ConnectionId))
                    creator.ConnectionId = Context.ConnectionId;

                var player = _players.FirstOrDefault(i => i.Id == playerId);
                if (player != null && !string.IsNullOrEmpty(player.ConnectionId))
                {
                    await Groups.AddToGroupAsync(creator.ConnectionId, presetId);
                    await Clients.Client(player.ConnectionId).SendAsync("InvitePlayer", creatorId, creator.NickName, presetId, presetName, presetJSON);
                }
            }
        }

        public async Task CancelInvitePlayer(string presetId, string playerId)
        {
            var player = _players.FirstOrDefault(i => i.Id == playerId);
            if (player != null && !string.IsNullOrEmpty(player.ConnectionId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, presetId);
                await Clients.Client(player.ConnectionId).SendAsync("CancelInvitePlayer", presetId);
            }
        }

        public async Task AcceptInvite(string presetId, string playerId)
        {
            var connectionId = Context.ConnectionId;
            var player = _players.FirstOrDefault(i => i.Id == playerId);
            if (player != null)
            {
                player.IsOnline = true;
                if (string.IsNullOrEmpty(player.ConnectionId))
                    player.ConnectionId = Context.ConnectionId;

                await Groups.AddToGroupAsync(connectionId, presetId);
                await Clients.Group(presetId).SendAsync("AcceptInvite", player.Id, player.NickName);
            }
        }

        public async Task RejectInvite(string presetId, string playerId)
        {
            var connectionId = Context.ConnectionId;
            var player = _players.FirstOrDefault(i => i.Id == playerId);
            if (player != null)
            {
                player.IsOnline = true;
                if (string.IsNullOrEmpty(player.ConnectionId))
                    player.ConnectionId = Context.ConnectionId;

                await Clients.Group(presetId).SendAsync("RejectInvite", player.NickName);
            }
        }

        public async Task StartPresetCreation(string presetId)
        {
            await Clients.Group(presetId).SendAsync("StartPresetCreation", presetId);

        }

        public async Task CheckSquare(string presetId, string playerId, string squareId)
        {
            await Clients.Group(presetId).SendAsync("CheckSquare", playerId, squareId);
        }

        public async Task UncheckSquare(string presetId, string playerId, string squareId)
        {
            await Clients.Group(presetId).SendAsync("UncheckSquare", playerId, squareId);
        }

        public async Task PlayerReady(string presetId, string playerId)
        {
            await Clients.Group(presetId).SendAsync("PlayerReady", playerId);
        }

        public async Task PlayerNotReady(string presetId, string playerId)
        {
            await Clients.Group(presetId).SendAsync("PlayerNotReady", playerId);
        }

        public async Task PresetCreated(string presetId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, presetId);
            await Clients.Group(presetId).SendAsync("PresetCreated", presetId);
        }

        public async Task PresetCreationCanceled(string presetId, string playerId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, presetId);
            await Clients.Group(presetId).SendAsync("PresetCreationCanceled", playerId);
        }
        #endregion

        #region Game Invite
        public async Task SendGameInvite(string playerId, string invitePlayerId, string data)
        {
            var player = _players.FirstOrDefault(i => i.Id == playerId);
            var invitePlayer = _players.FirstOrDefault(i => i.Id == invitePlayerId);

            if (player != null && invitePlayer != null && !string.IsNullOrEmpty(invitePlayer.ConnectionId))
            {
                await Clients.Client(invitePlayer.ConnectionId).SendAsync("GameInvite", player.NickName, data);
            }
        }
        #endregion

        #region Preset Sharing
        public async Task SendPresetToPlayer(string playerId, string fromPlayerName, string gameName, string presetName, string squaresFileName, string notesFileName)
        {
            var player = _players.FirstOrDefault(i => i.Id == playerId);
            if (player != null && !string.IsNullOrEmpty(player.ConnectionId))
            {
                await Clients.Client(player.ConnectionId).SendAsync("PresetReceived", fromPlayerName, gameName, presetName, squaresFileName, notesFileName);
            }
        }

        #endregion

        #region Test
        public async Task Test()
        {
            await Clients.All.SendAsync("test", "test");
        }

        #endregion

        #region Methods
        private async Task AddNewEvent(Room room, Event @event)
        {
            if (Clients == null) return;
            @event.Timestamp = DateTime.UtcNow;
            room.ChatMessages.Add(@event);
            var json = JsonConvert.SerializeObject(@event);
            await Clients.Group(room.RoomId).SendAsync("NewEvent", json);
        }

        private void CleanUpRooms()
        {
            var now = DateTime.Now;
            var expiredRooms = _rooms.Where(i => (now - i.CreaionDate).TotalHours > 24).ToList();
            foreach (var room in expiredRooms)
            {
                _rooms.Remove(room);
            }
        }

        public void ClearSharedPreset(string squaresFileName, string notesFileName)
        {
            if (File.Exists($"wwwroot/uploads/presets/{squaresFileName}"))
            {
                File.Delete($"wwwroot/uploads/presets/{squaresFileName}");
            }
            if (File.Exists($"wwwroot/uploads/presets/{notesFileName}"))
            {
                File.Delete($"wwwroot/uploads/presets/{notesFileName}");
            }
        }

        public async Task UpdatePlayersGoals(Room room)
        {
            if (room.Board != null)
            {
                foreach (var item in room.Players)
                {
                    var lines = item.LinesCount;
                    item.SquaresCount = room.Board.GetColorCount(item.Color);
                    item.LinesCount = room.Board.GetLinesCount(item.Color);
                    item.PotentialBingosCount = room.Board.GetPotentialBingosCount(item);

                    if (item.LinesCount > lines)
                    {
                        if (room.RoomSettings.GameMode == GameMode.Triple)
                        {
                            if (item.LinesCount == 3)
                            {
                                await AddNewEvent(room, new Event()
                                {
                                    Type = EventType.bingo,
                                    Player = item,
                                    Timestamp = DateTime.Now,
                                });
                                room.IsGameEnded = true;
                            }
                        }
                        if (room.RoomSettings.GameMode == GameMode.Lockout)
                        {
                            await AddNewEvent(room, new Event()
                            {
                                Type = EventType.bingo,
                                Player = item,
                                Timestamp = DateTime.Now,
                            });
                            room.IsGameEnded = true;
                        }
                        if (room.RoomSettings.GameMode == GameMode.Blackout)
                        {
                            if (item.SquaresCount == 25)
                            {
                                await AddNewEvent(room, new Event()
                                {
                                    Type = EventType.bingo,
                                    Player = item,
                                    Timestamp = DateTime.Now,
                                });
                                room.IsGameEnded = true;
                            }
                        }
                    }
                }

                if (room.RoomSettings.GameMode == GameMode.Lockout)
                {
                    foreach (var item in room.Players)
                    {
                        if (item.LinesCount == 0 && item.SquaresCount >= 13 && room.Players.Except(new SocketServer.Models.Bingo.Player[] { item }).Sum(i => i.PotentialBingosCount) == 0)
                        {
                            await AddNewEvent(room, new Event()
                            {
                                Type = EventType.win,
                                Player = item,
                                Timestamp = DateTime.Now,
                            });
                            room.IsGameEnded = true;
                        }
                    }
                }

                var playersScore = room.Players.Select(i => new PlayerScore()
                {
                    PlayerId = i.Id,
                    Score = i.SquaresCount,
                    LinesCount = i.LinesCount
                }).ToArray();
                var json = JsonConvert.SerializeObject(playersScore, Formatting.Indented);

                await Clients.Group(room.RoomId).SendAsync("UpdatePlayerScores", json);
            }
        }

        #endregion

        #region ForSpectators
        public async Task InitRoom(string roomId, string playerId)
        {
            if (Clients == null) return;
            var connectionId = Context.ConnectionId;

            var room = _rooms.FirstOrDefault(r => r.RoomId == roomId);
            if (room == null)
            {
                await Clients.All.SendAsync("NoRoom");
            }
            else
            {

                var roomPlayer = room.Players.FirstOrDefault(i => i.Id == playerId);
                if (roomPlayer == null)
                {
                    roomPlayer = new Models.Bingo.Player(playerId, "Spectator");
                    roomPlayer.IsSpectator = true;
                    room.Players.Add(roomPlayer);
                    await Groups.AddToGroupAsync(connectionId, room.RoomId);
                    var playerJson = JsonConvert.SerializeObject(roomPlayer);
                    await Clients.Group(roomId).SendAsync("PlayerConnected", playerJson);
                }
                else
                {
                    await Groups.AddToGroupAsync(connectionId, room.RoomId);
                    var playerJson = JsonConvert.SerializeObject(roomPlayer);
                    await Clients.Group(roomId).SendAsync("PlayerConnected", playerJson);
                }

                var json = JsonConvert.SerializeObject(room, Formatting.Indented);
                await Clients.All.SendAsync("RecieveRoomInfo", json);
            }
        }

        #endregion
    }

}
