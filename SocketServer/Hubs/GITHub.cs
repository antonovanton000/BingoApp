using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using Player = SocketServer.Models.GIT.Player;
using Room = SocketServer.Models.GIT.Room;

namespace SocketServer.Hubs
{
    public class GITHub : Hub
    {
        #region Fields

        HashSet<Player> _players;

        HashSet<Room> _rooms;

        #endregion

        #region Constructor

        public GITHub(HashSet<Player> players, HashSet<Room> rooms)
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
                    await Groups.AddToGroupAsync(connectionId, room.RoomId);
                    var json = JsonConvert.SerializeObject(player);
                    await Clients.Group(roomId).SendAsync("PlayerConnected", json);
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
                    }
                    await Clients.Group(roomId).SendAsync("PlayerDisconnected", player.Id);
                }
            }

            CleanUpRooms();
        }
        #endregion

        #region Actions
        public async Task CheckItem(string roomId, string playerId, string itemId)
        {

            if (Clients == null) return;            
            var room = _rooms.FirstOrDefault(r => r.RoomId == roomId);
            if (room != null)
            {
                var player = room.Players.FirstOrDefault(p => p.Id == playerId);

                var curItem = room.ItemsStates.FirstOrDefault(i => i.ItemId == itemId);
                if (curItem != null && player != null)
                {
                    curItem.SelectByPlayers.Add(player.Id);
                    await Clients.Group(roomId).SendAsync("ItemChecked", itemId, player.Id);
                }                
            }
        }

        public async Task UncheckItem(string roomId, string playerId, string itemId)
        {

            if (Clients == null) return;
            
            var room = _rooms.FirstOrDefault(r => r.RoomId == roomId);
            if (room != null)
            {
                var player = room.Players.FirstOrDefault(p => p.Id == playerId);

                var curItem = room.ItemsStates.FirstOrDefault(i => i.ItemId == itemId);
                if (curItem != null && player != null)
                {
                    curItem.SelectByPlayers.Remove(player.Id);
                    await Clients.Group(roomId).SendAsync("ItemUnchecked", itemId, player.Id);
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
                await Clients.GroupExcept(roomId, new string[] {connectionId}).SendAsync("SpEffectApplied", playerId, flagId);
            }
        }

        public async Task ApplySpEffectToPlayer(string roomId, string playerId, int flagId)
        {
            if (Clients == null) return;           
            var room = _rooms.FirstOrDefault(r => r.RoomId == roomId);
            if (room != null)
            {
                var player = room.Players.FirstOrDefault(p => p.Id == playerId);
                if (player!= null && !string.IsNullOrEmpty(player.ConnectionId))
                    await Clients.Client(player.ConnectionId).SendAsync("SpEffectApplied", playerId, flagId);
            }
        }


        public async Task SubmitTime(string roomId, string playerId, TimeSpan time)
        {
            if (Clients == null) return;
            var room = _rooms.FirstOrDefault(r => r.RoomId == roomId);
            if (room != null)
            {
                var player = room.Players.FirstOrDefault(p => p.Id == playerId);
                if (player != null)
                {
                    player.RunTime = time;
                    await Clients.Group(roomId).SendAsync("PlayerRunTime", playerId, time);
                }
            }

        }
        #endregion

        #region Methods
        private void CleanUpRooms()
        {
            var now = DateTime.Now;
            var expiredRooms = _rooms.Where(i => (now - i.CreationDate).TotalHours > 24).ToList();
            foreach (var room in expiredRooms)
            {
                _rooms.Remove(room);
            }
        }

        #endregion
    }
}
