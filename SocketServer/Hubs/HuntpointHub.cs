using SocketServer.Models;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http.HttpResults;
using static System.Runtime.InteropServices.JavaScript.JSType;
using SocketServer.Models.Huntpoint;
using Player = SocketServer.Models.Player;
using Newtonsoft.Json;
using System.Drawing;

namespace SocketServer.Hubs
{
    public class HuntpointHub : Hub
    {

        HashSet<Room> _rooms;

        HashSet<Player> _players;

        HashSet<RoomTimerSettings> _settings;

        public HuntpointHub(HashSet<Player> players, HashSet<Room> rooms)
        {
            _players = players;
            _rooms = rooms;
            _settings = new();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (Clients == null) return;

            var connectionId = Context.ConnectionId;
            var player = _players.FirstOrDefault(i => i.ConnectionId == connectionId);
            if (player != null)
            {
                player.IsOnline = false;
                if (!string.IsNullOrEmpty(player.ActiveRoomId))
                {
                    await Groups.RemoveFromGroupAsync(connectionId, player.ActiveRoomId);
                    await Clients.Group(player.ActiveRoomId).SendAsync("PlayerDisconnected", player.NickName);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async void Connect(string playerId)
        {
            if (Clients == null) return;

            var connectionId = Context.ConnectionId;
            var player = _players.FirstOrDefault(p => p.Id == playerId);
            if (player != null)
            {
                player.IsOnline = true;
                player.ConnectionId = connectionId;
            }
            await Clients.Client(connectionId).SendAsync("Connected", playerId);
        }

        public async Task ConnectToRoom(string roomId)
        {
            if (Clients == null) return;

            var connectionId = Context.ConnectionId;
            var room = _rooms.FirstOrDefault(r => r.RoomId == roomId);
            if (room != null)
            {
                if (_settings.Any(i => i.RoomId == roomId))
                {
                    var settings = _settings.FirstOrDefault(i => i.RoomId == roomId);
                    if (settings != null)
                    {
                        await Clients.Client(connectionId).SendAsync("AutoBoardRevealSettings", roomId, settings.StartTime, settings.AfterRevealTime, settings.UnhideTime, settings.ChangeTime);
                    }
                }

                var player = _players.FirstOrDefault(p => p.ConnectionId == connectionId);
                if (player != null)
                {
                    var roomPlayer = room.Players.FirstOrDefault(i => i.Id == player.Id);
                    if (roomPlayer == null)
                    {
                        player.ActiveRoomId = room.RoomId;
                        roomPlayer = new Models.Huntpoint.Player(player.Id, player.NickName);
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
                    if (roomPlayer!=null)
                    {
                        await AddNewEvent(room, new Event() { Type = EventType.chat, Player = roomPlayer, Message = message, PlayerColor = roomPlayer.Color });
                    }
                }
            }
        }         

        public async Task PlayerChangeColor(string roomId, HuntpointColor color)
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
                    }
                }
            }
        }

        public async Task SendGameInvite(string playerId, string invitePlayerId, string data)
        {
            if (Clients == null) return;

            var player = _players.FirstOrDefault(i => i.Id == playerId);
            var invitePlayer = _players.FirstOrDefault(i => i.Id == invitePlayerId);

            if (player != null && invitePlayer != null && !string.IsNullOrEmpty(invitePlayer.ConnectionId))
            {
                await Clients.Client(invitePlayer.ConnectionId).SendAsync("GameInvite", player.NickName, data);
            }
        }

        public async Task MarkObjective(string roomId, string slotId, HuntpointColor color)
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
                    var objective = room.Board.Objectives.FirstOrDefault(i => i.Slot == slotId);
                    if (objective != null)
                    {
                        if (objective.ObjectiveColors.Any(i => i == color))
                        {
                            // If the color is already set, remove it
                            objective.ObjectiveColors.Remove(color);
                            await Clients.Group(roomId).SendAsync("UnmarkObjective", slotId, color);
                            await AddNewEvent(room, new Event() {Type = EventType.goal, Player = roomPlayer, Objective = objective, PlayerColor = roomPlayer.Color, Remove = true });
                        }
                        else
                        {
                            // If the color is not set, add it
                            objective.ObjectiveColors.Add(color);
                            await Clients.Group(roomId).SendAsync("MarkObjective", slotId, color);
                            await AddNewEvent(room, new Event() { Type = EventType.goal, Player = roomPlayer, Objective = objective, PlayerColor = roomPlayer.Color });
                        }

                    }
                }
            }
        }

        public async Task ApplySpEffect(string roomId, string playerId, int flagId)
        {
            if (Clients == null) return;
            var connectionId = Context.ConnectionId;

            var room = _rooms.FirstOrDefault(r => r.RoomId == roomId);
            if (room != null)
            {
                await Clients.GroupExcept(roomId, new string[] { connectionId }).SendAsync("SpEffectApplied", playerId, flagId);
            }
        }

        public async Task RevealBoard(string roomId, string playerId)
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
                        await AddNewEvent(room, new Event() { Type = EventType.revealed, Player = roomPlayer });
                    }
                }
            }
        }

        public async Task AutoBoardRevealSettings(string roomId, int startTimeSettings, int afterRevealSettings, int unhideTime, int changeTime)
        {
            if (Clients == null) return;

            var settings = _settings.FirstOrDefault(i => i.RoomId == roomId);
            if (settings == null)
            {
                settings = new RoomTimerSettings
                {
                    RoomId = roomId,
                    StartTime = startTimeSettings,
                    AfterRevealTime = afterRevealSettings,
                    UnhideTime = unhideTime,
                    ChangeTime = changeTime,
                    Timestamp = DateTime.Now
                };
                _settings.Add(settings);
            }
            else
            {
                settings.Timestamp = DateTime.Now;
                settings.StartTime = startTimeSettings;
                settings.AfterRevealTime = afterRevealSettings;
                settings.UnhideTime = unhideTime;
                settings.ChangeTime = changeTime;
            }
            CleanUpRoomSettings();
            await Clients.Group(roomId).SendAsync("AutoBoardRevealSettings", roomId, startTimeSettings, afterRevealSettings, unhideTime, changeTime);
        }

        public async Task ChangeSquare(string roomId, string slotId, string newName)
        {
            if (Clients == null) return;

            await Clients.Group(roomId).SendAsync("ChangeSquare", roomId, slotId, newName);
        }

        public async Task StartRoomGame(string roomId)
        {
            if (Clients == null) return;

            await Clients.Group(roomId).SendAsync("StartRoomGame", roomId);
        }

        public async Task PauseRoomGame(string roomId)
        {
            if (Clients == null) return;

            await Clients.Group(roomId).SendAsync("PauseRoomGame", roomId);
        }

        public async Task ResumeRoomGame(string roomId)
        {
            if (Clients == null) return;

            await Clients.Group(roomId).SendAsync("ResumeRoomGame", roomId);
        }

        public async Task StopRoomGame(string roomId)
        {
            if (Clients == null) return;

            await Clients.Group(roomId).SendAsync("StopRoomGame", roomId);
        }

        public async Task RoomCurrentTimeSync(string roomId, int currentTime)
        {
            if (Clients == null) return;

            await Clients.Group(roomId).SendAsync("RoomCurrentTimeSync", roomId, currentTime);
        }

        private async Task AddNewEvent(Room room, Event @event)
        {
            if (Clients == null) return;
            @event.Timestamp = DateTime.UtcNow;
            room.ChatMessages.Add(@event);
            var json = JsonConvert.SerializeObject(@event);
            await Clients.Group(room.RoomId).SendAsync("NewEvent", json);
        }

        public async Task SendNewRoomBoard(Room room)
        {
            if (Clients == null) return;

            var json = JsonConvert.SerializeObject(room.Board);
            var creator = room.Players.FirstOrDefault(i => i.Id == room.CreatorId);
            await AddNewEvent(room, new Event() { Type = EventType.newcard, Player = creator });
            await Clients.Group(room.RoomId).SendAsync("NewBoard", json);
        }

        public async Task FinishRun(string roomId, string playerId)
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
                        roomPlayer.IsFinished = true;
                        await Clients.Group(room.RoomId).SendAsync("PlayerFinished", playerId);
                        await AddNewEvent(room, new Event() { Type = EventType.finish, Player = roomPlayer, PlayerColor = roomPlayer.Color });
                    }
                }
            }
        }

        public async Task Test()
        {
            if (Clients == null) return;

            await Clients.All.SendAsync("test", "test");
        }

        private void CleanUpRoomSettings()
        {
            var now = DateTime.Now;
            var expiredSettings = _settings.Where(i => (now - i.Timestamp).TotalHours > 10).ToList();
            foreach (var setting in expiredSettings)
            {
                _settings.Remove(setting);
            }
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


        public async Task SendPresetToPlayer(string playerId, string fromPlayerName, string gameName, string presetName, string squaresFileName, string notesFileName)
        {
            if (Clients == null) return;

            var player = _players.FirstOrDefault(i => i.Id == playerId);
            if (player != null && !string.IsNullOrEmpty(player.ConnectionId))
            {
                await Clients.Client(player.ConnectionId).SendAsync("PresetReceived", fromPlayerName, gameName, presetName, squaresFileName, notesFileName);
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
    }

}
