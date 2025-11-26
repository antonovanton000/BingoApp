using SocketServer.Models;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http.HttpResults;
using static System.Runtime.InteropServices.JavaScript.JSType;
using SocketServer.Models.Wheel;
using Player = SocketServer.Models.Player;
using Newtonsoft.Json;
using System.Drawing;

namespace SocketServer.Hubs
{
    public class WheelHub : Hub
    {

        HashSet<Room> _rooms;

        HashSet<Player> _players;

        public WheelHub(HashSet<Player> players, HashSet<Room> rooms)
        {
            _players = players;
            _rooms = rooms;
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

                var player = _players.FirstOrDefault(p => p.ConnectionId == connectionId);
                if (player != null)
                {
                    var roomPlayer = room.Players.FirstOrDefault(i => i.Id == player.Id);
                    if (roomPlayer == null)
                    {
                        player.ActiveRoomId = room.RoomId;
                        roomPlayer = new Models.Wheel.Player(player.Id, player.NickName);
                        roomPlayer.PlayerIndex = room.Players.Count + 1;
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
                    }
                    await Clients.Group(roomId).SendAsync("PlayerDisconnected", player.Id);
                }
            }

            CleanUpRooms();
        }

        public async Task MarkObjective(string roomId, string slotId)
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
                    var objective = room.FindObjectiveBySlot(slotId);
                    if (objective != null && roomPlayer != null)
                    {
                        if (objective.PlayerIds.Any(i => i == roomPlayer.Id))
                        {
                            // If the color is already set, remove it
                            objective.PlayerIds.Remove(roomPlayer.Id);
                            await Clients.Group(roomId).SendAsync("UnmarkObjective", slotId, roomPlayer.Id);
                        }
                        else
                        {
                            // If the color is not set, add it
                            objective.PlayerIds.Add(roomPlayer.Id);
                            await Clients.Group(roomId).SendAsync("MarkObjective", slotId, roomPlayer.Id);
                        }

                    }
                }
            }
        }

        public async Task MarkObjectiveModifier(string roomId, string slotId, string slotModifierId)
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
                    var objective = room.FindObjectiveBySlot(slotId);
                    if (objective != null && roomPlayer != null)
                    {
                        var modifier = objective.Modifiers.FirstOrDefault(i => i.Slot == slotModifierId);
                        if (modifier == null) return;
                        if (modifier.PlayerIds.Any(i => i == roomPlayer.Id))
                        {
                            // If the color is already set, remove it
                            modifier.PlayerIds.Remove(roomPlayer.Id);
                            await Clients.Group(roomId).SendAsync("UnmarkObjectiveModifier", slotId, slotModifierId, roomPlayer.Id);
                        }
                        else
                        {
                            // If the color is not set, add it
                            modifier.PlayerIds.Add(roomPlayer.Id);
                            await Clients.Group(roomId).SendAsync("MarkObjectiveModifier", slotId, slotModifierId, roomPlayer.Id);
                        }

                    }
                }
            }
        }

        public async Task RotateWheel(string roomId, int angle)
        {
            if (Clients == null) return;
            
            await Clients.Group(roomId).SendAsync("RotateWheel", angle);                        
        }

        public async Task StartRoomGame(string roomId)
        {
            if (Clients == null) return;

            await Clients.Group(roomId).SendAsync("StartRoomGame", roomId);
            var room = _rooms.FirstOrDefault(r => r.RoomId == roomId);
            if (room == null) return;
            room.IsGameStarted = true;
            var firstPlayer = room.Players.FirstOrDefault();
            if (firstPlayer == null) return;
            firstPlayer.IsPlayerTurn = true;
            await Clients.Group(roomId).SendAsync("ChangePlayerTurn", firstPlayer.Id);
        }

        public async Task FinishCurrentObjective(string roomId, string slotId, string playerId)
        {
            if (Clients == null) return;
            var room = _rooms.FirstOrDefault(r => r.RoomId == roomId);
            if (room != null)
            {
                var player = room.Players.FirstOrDefault(i => i.Id == playerId);
                if (player != null)
                {
                    player.IsFinished = true; 
                    await Clients.Group(roomId).SendAsync("FinishCurrentObjective", playerId);
                    
                    var isAllFinished = !room.Players.Any(i => !i.IsFinished);
                    if (isAllFinished)
                    {
                        await Clients.Group(roomId).SendAsync("StopTimer");
                        room.RemoveObjectiveBySlot(slotId);
                        await Clients.Group(roomId).SendAsync("RemoveWheelSector", slotId);
                        // If all players finished their objectives, change the turn
                        var currentPlayer = room.Players.FirstOrDefault(i => i.IsPlayerTurn);
                        if (currentPlayer == null) return;
                        var currentPlayerIndex = room.Players.IndexOf(currentPlayer);
                        var nextPlayerIndex = currentPlayerIndex == room.Players.Count - 1 ? 0 : currentPlayerIndex + 1;
                        currentPlayer.IsPlayerTurn = false;
                        room.Players[nextPlayerIndex].IsPlayerTurn = true;
                        foreach (var item in room.Players)
                        {
                            item.IsFinished = false;
                        }
                        await Clients.Group(roomId).SendAsync("ChangePlayerTurn", room.Players[nextPlayerIndex].Id);
                    }
                    else
                    {
                        await Clients.Group(roomId).SendAsync("StartTimer", room.RoomSettings.TimerSeconds);
                    }
                }
            }
        }

        public async Task GetNewRandomDebuff(string roomId)
        {
            if (Clients == null) return;
            var room = _rooms.FirstOrDefault(r => r.RoomId == roomId);
            if (room != null)
            {
                room.SetRandomDebuff();
                await Clients.Group(roomId).SendAsync("NewDebuff", room.CurrentDebuff);
            }
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

        public async Task Test()
        {
            if (Clients == null) return;

            await Clients.All.SendAsync("test", "test");
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
    }
}
