using BingoApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BingoApp.Classes
{

    public partial class BingoAppSignalRHub : ObservableObject
    {
        [ObservableProperty]
        bool isHubConnected;

        private string _playerId = "";

        public event EventHandler<InvitePlayerEventArgs>? OnInvitePlayerRecieved;
        public event EventHandler<AcceptInviteEventArgs>? OnAcceptInviteRecieved;
        public event EventHandler<RejectInviteEventArgs>? OnRejectInviteRecieved;
        public event EventHandler<CheckSquareEventArgs>? OnCheckSquareRecieved;
        public event EventHandler<UncheckSquareEventArgs>? OnUncheckSquareRecieved;
        public event EventHandler<ChangeSquareEventArgs>? OnChangeSquareRecieved;
        public event EventHandler<UnhideSquareEventArgs>? OnUnhideSquareRecieved;
        public event EventHandler<string>? OnStartPresetCreationRecieved;
        public event EventHandler<string>? OnPlayerReadyRecieved;
        public event EventHandler<string>? OnPlayerNotReadyRecieved;
        public event EventHandler<string>? OnPresetCreatedRecieved;
        public event EventHandler<string>? OnPresetCreationCanceledRecieved;
        public event EventHandler<string>? OnCancelInviteRecieved;
        public event EventHandler<GameInviteEventArgs>? OnGameInviteRecieved;
        public event EventHandler<string>? OnStartGameRecieved;
        public event EventHandler<RoomTimerEventArgs>? OnPauseGameRecieved;
        public event EventHandler<RoomTimerEventArgs>? OnResumeGameRecieved;
        public event EventHandler<RoomTimerEventArgs>? OnSyncRoomTimerRecieved;
        public event EventHandler<string>? OnStopGameRecieved;
        public event EventHandler<string>? OnResetRoomTimerRecieved;
        public event EventHandler<string>? OnPlayerDisconnected;
        public event EventHandler<string>? OnPlayerReconnected;
        public event EventHandler<Player>? OnPlayerConnected;        
        public event EventHandler<SharePresetEventArgs>? OnSharePresetRecieved;
        public event EventHandler<RoomBoardEventArgs>? OnRoomBoardRecieved;
        public event EventHandler<string>? OnReconnecting;
        public event EventHandler<string>? OnReconnected;
        public event EventHandler<string>? OnDisconnected;
        public event EventHandler<string>? OnConnected;
        public event EventHandler<PlayerColorChangedEventArgs>? OnPlayerColorChangedRecieved;        
        public event EventHandler<Board>? OnNewBoardRecieved;
        public event EventHandler<Event>? OnNewEventRecieved;
        public event EventHandler<MarkSquareEventArgs>? OnMarkSquareRecieved;
        public event EventHandler<MarkSquareEventArgs>? OnUnmarkSquareRecieved;
        public event EventHandler<string>? OnBingoLineSelected;

        HubConnection connection;
        public BingoAppSignalRHub()
        {
            connection = new HubConnectionBuilder()
                           .WithUrl($"{App.TimerSocketScheme}://{App.TimerSocketAddress}/bingohub")
                           .Build();            

            connection.HandshakeTimeout = TimeSpan.FromSeconds(3);
            connection.KeepAliveInterval = TimeSpan.FromSeconds(3);

            InitHubEndpoints();
        }

        private void InitHubEndpoints()
        {
            connection.On<string, string, string, string, string>("InvitePlayer", (creatorId, nickname, presetId, presetName, presetJSON) =>
            {
                OnInvitePlayerRecieved?.Invoke(this, new InvitePlayerEventArgs()
                {
                    CreatorId = creatorId,
                    NickName = nickname,
                    PresetId = presetId,
                    PresetName = presetName,
                    PresetJSON = presetJSON
                });
            });

            connection.On<string, string>("AcceptInvite", (playerId, nickName) =>
            {
                OnAcceptInviteRecieved?.Invoke(this, new AcceptInviteEventArgs()
                {
                    PlayerId = playerId,
                    NickName = nickName,
                });

            });

            connection.On<string>("CancelInvitePlayer", (presetId) =>
            {
                OnCancelInviteRecieved?.Invoke(this, presetId);
            });

            connection.On<string>("RejectInvite", (nickName) =>
            {
                OnRejectInviteRecieved?.Invoke(this, new RejectInviteEventArgs()
                {
                    NickName = nickName
                });
            });

            connection.On<string, string>("CheckSquare", (playerId, squareId) =>
            {
                OnCheckSquareRecieved?.Invoke(this, new CheckSquareEventArgs()
                {
                    PlayerId = playerId,
                    SquareId = squareId
                });
            });

            connection.On<string, string>("UncheckSquare", (playerId, squareId) =>
            {
                OnUncheckSquareRecieved?.Invoke(this, new UncheckSquareEventArgs()
                {
                    PlayerId = playerId,
                    SquareId = squareId
                });
            });

            connection.On<string>("StartPresetCreation", (presetId) =>
            {
                OnStartPresetCreationRecieved?.Invoke(this, presetId);
            });

            connection.On<string>("PlayerReady", (playerId) =>
            {
                OnPlayerReadyRecieved?.Invoke(this, playerId);
            });

            connection.On<string>("PlayerNotReady", (playerId) =>
            {
                OnPlayerNotReadyRecieved?.Invoke(this, playerId);
            });

            connection.On<string>("PresetCreated", (presetId) =>
            {
                OnPresetCreatedRecieved?.Invoke(this, presetId);
            });

            connection.On<string>("PresetCreated", (presetId) =>
            {
                OnPresetCreatedRecieved?.Invoke(this, presetId);
            });

            connection.On<string>("PresetCreationCanceled", (playerId) =>
            {
                OnPresetCreationCanceledRecieved?.Invoke(this, playerId);
            });

            connection.On<string, string>("GameInvite", (nickName, data) =>
            {
                OnGameInviteRecieved?.Invoke(this, new GameInviteEventArgs() { NickName = nickName, Data = data });
            });

            connection.On<string, string, string>("ChangeSquare", (roomId, slotId, newName) =>
            {
                OnChangeSquareRecieved?.Invoke(this, new ChangeSquareEventArgs() { RoomId = roomId, SlotId = slotId, NewName = newName });
            });

            connection.On<string, string>("UnhideSquare", (roomId, slotId) =>
            {
                OnUnhideSquareRecieved?.Invoke(this, new UnhideSquareEventArgs() { RoomId = roomId, SlotId = slotId });
            });

            connection.On<string>("StartRoomGame", (roomId) =>
            {
                OnStartGameRecieved?.Invoke(this, roomId);
            });

            connection.On<string, TimeSpan>("PauseRoomGame", (roomId, currentTime) =>
            {
                OnPauseGameRecieved?.Invoke(this, new RoomTimerEventArgs() { RoomId = roomId, CurrentTime = currentTime});
            });

            connection.On<string, TimeSpan>("ResumeRoomGame", (roomId, currentTime) =>
            {
                OnResumeGameRecieved?.Invoke(this, new RoomTimerEventArgs() { RoomId = roomId, CurrentTime = currentTime });
            });

            connection.On<string>("StopRoomGame", (roomId) =>
            {
                OnStopGameRecieved?.Invoke(this, roomId);
            });

            connection.On<string>("ResetRoomTimer", (roomId) =>
            {
                OnResetRoomTimerRecieved?.Invoke(this, roomId);
            });

            connection.On<string, TimeSpan>("SyncRoomTimer", (roomId, currentTime) =>
            {
                OnSyncRoomTimerRecieved?.Invoke(this, new RoomTimerEventArgs() { RoomId = roomId, CurrentTime = currentTime });
            });

            connection.On<string>("PlayerConnected", (json) =>
            {
                var roomPlayer = JsonConvert.DeserializeObject<Player>(json);
                if (roomPlayer != null)
                {
                    OnPlayerConnected?.Invoke(this, roomPlayer);
                }
            });

            connection.On<string>("PlayerDisconnected", (playerId) =>
            {
                OnPlayerDisconnected?.Invoke(this, playerId);
            });

            connection.On<string, string, string, string, string>("PresetReceived", (fromPlayerName, gameName, presetName, squaresFileName, notesFileName) =>
            {
                OnSharePresetRecieved?.Invoke(this, new SharePresetEventArgs()
                {
                    FromPlayerName = fromPlayerName,
                    GameName = gameName,
                    PresetName = presetName,
                    SquaresFileName = squaresFileName,
                    NotesFileName = notesFileName
                });
            });

            connection.On<string>("RoomBoard", (json) => { 
            
                var squares = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<Square>>(json);

                OnRoomBoardRecieved?.Invoke(this, new RoomBoardEventArgs()
                {
                    Squares = squares ?? new List<Square>()
                });
            });

            connection.On<string>("NewBoard", (json) => {
                var board = Newtonsoft.Json.JsonConvert.DeserializeObject<Board>(json);
                if (board!=null)
                {
                    OnNewBoardRecieved?.Invoke(this, board);
                }                
            });

            connection.On<string>("NewEvent", json =>
            {
                var newEvent = JsonConvert.DeserializeObject<Event>(json);
                if (newEvent != null)
                {
                    newEvent.Timestamp = newEvent.Timestamp.ToLocalTime();
                    OnNewEventRecieved?.Invoke(this, newEvent);
                }
            });

            connection.On<string, BingoColor>("PlayerColorChanged", (playerId, color) =>
            {
                OnPlayerColorChangedRecieved?.Invoke(this, new PlayerColorChangedEventArgs() { PlayerId = playerId, Color = color });
            });

            connection.On<string, BingoColor>("MarkSquare", (slot, color) =>
            {
                OnMarkSquareRecieved?.Invoke(this, new MarkSquareEventArgs() { Slot = slot, Color = color });
            });

            connection.On<string, BingoColor>("UnmarkSquare", (slot, color) =>
            {
                OnUnmarkSquareRecieved?.Invoke(this, new MarkSquareEventArgs() { Slot = slot, Color = color });
            });

            connection.On<string>("SelectBingoLine", bingoLine => {
                OnBingoLineSelected?.Invoke(this, bingoLine);
            });

            connection.On<string>("Connected", playerId =>
            {
                OnConnected?.Invoke(this, playerId);
            });

            connection.Reconnecting += Connection_Reconnecting;

            connection.Reconnected += Connection_Reconnected;

            connection.Closed += Connection_Closed;
        }

        private async Task Connection_Closed(Exception? arg)
        {
            IsHubConnected = connection.State == HubConnectionState.Connected;
            OnDisconnected?.Invoke(this, arg?.Message ?? "");
        }

        private async Task Connection_Reconnected(string? arg)
        {
            IsHubConnected = connection.State == HubConnectionState.Connected;
            OnReconnected?.Invoke(this, arg ?? "");
        }

        private async Task Connection_Reconnecting(Exception? arg)
        {
            IsHubConnected = connection.State == HubConnectionState.Connected;
            OnReconnected?.Invoke(this, arg?.Message ?? "");
        }

        public async Task ConnectAsync(string playerId)
        {
            _playerId = playerId;
            try
            {
                if (connection.State == HubConnectionState.Disconnected)
                {
                    await connection.StartAsync();
                    IsHubConnected = connection.State == HubConnectionState.Connected;

                    await connection.SendAsync("Connect", _playerId);
                }
            }
            catch (Exception ex)
            {
                IsHubConnected = false;
            }
        }

        public async Task ConnectToRoomHub(string roomId)
        {
            try
            {
                if (connection.State != HubConnectionState.Connected)
                    await connection.StartAsync();

                await connection.SendAsync("ConnectToRoom", roomId);
            }
            catch (Exception)
            {
            }
        }

        public async Task DisconnectFromRoomHub(string roomId)
        {
            try
            {
                if (connection.State != HubConnectionState.Connected)
                    await connection.StartAsync();
                await connection.SendAsync("DisconnectFromRoom", roomId);
            }
            catch (Exception)
            {

            }
        }

        public async Task SendRevealBoard(string roomId, string playerId)
        {
            try
            {
                if (connection.State != HubConnectionState.Connected)
                    await connection.StartAsync();

                await connection.InvokeAsync("RevealBoard", roomId, playerId);
            }
            catch (Exception) { }
        }

        public async Task SendMarkSquare(string roomId, string slotId, BingoColor color)
        {
            try
            {
                if (connection.State != HubConnectionState.Connected)
                    await connection.StartAsync();
                await connection.InvokeAsync("MarkSquare", roomId, slotId, color);
            }
            catch (Exception) { }
        }

        public async Task SendSelectBingoLine(string roomId, string playerId, string bingoLine)
        {
            try
            {
                if (connection.State != HubConnectionState.Connected)
                    await connection.StartAsync();
                await connection.InvokeAsync("SelectBingoLine", roomId, playerId, bingoLine);
            }
            catch (Exception) { }
        }

        public async Task SendPlayerChangeColor(string roomId, BingoColor color)
        {
            try
            {
                if (connection.State != HubConnectionState.Connected)
                    await connection.StartAsync();
                await connection.InvokeAsync("PlayerChangeColor", roomId, color);
            }
            catch (Exception) { }
        }

        public async Task SendChatMessage(string roomId, string message)
        {
            try
            {
                if (connection.State != HubConnectionState.Connected)
                    await connection.StartAsync();
                await connection.InvokeAsync("SendChatMessage", roomId, message);
            }
            catch (Exception) { }
        }

        public async Task InvitePlayerAsync(BingoAppPlayer creator, BingoAppPlayer player, string presetId, string presetName, string presetJSON)
        {
            try
            {
                if (connection.State != HubConnectionState.Connected)
                    await connection.StartAsync();

                await connection.InvokeAsync("InvitePlayer", creator.Id, player.Id, presetId, presetName, presetJSON);
            }
            catch (Exception)
            {
            }
        }

        public async Task CancelInvitePlayerAsync(BingoAppPlayer player, string presetId)
        {
            try
            {
                if (connection.State != HubConnectionState.Connected)
                    await connection.StartAsync();

                await connection.InvokeAsync("CancelInvitePlayer", presetId, player.Id);
            }
            catch (Exception)
            { }
        }

        public async Task AcceptInviteAsync(string presetId, string playerId)
        {
            try
            {
                if (connection.State != HubConnectionState.Connected)
                    await connection.StartAsync();

                await connection.InvokeAsync("AcceptInvite", presetId, playerId);
            }
            catch (Exception) { }
        }

        public async Task RejectInviteAsync(string presetId, string playerId)
        {
            try
            {
                if (connection.State != HubConnectionState.Connected)
                    await connection.StartAsync();

                await connection.InvokeAsync("RejectInvite", presetId, playerId);
            }
            catch (Exception) { }
        }

        public async Task CheckSquareAsync(string presetId, string playerId, string squareId)
        {
            try
            {
                if (connection.State != HubConnectionState.Connected)
                    await connection.StartAsync();

                await connection.InvokeAsync("CheckSquare", presetId, playerId, squareId);
            }
            catch (Exception) { }
        }

        public async Task UncheckSquareAsync(string presetId, string playerId, string squareId)
        {
            try
            {
                if (connection.State != HubConnectionState.Connected)
                    await connection.StartAsync();

                await connection.InvokeAsync("UncheckSquare", presetId, playerId, squareId);
            }
            catch (Exception) { }
        }

        public async Task SendStartPresetCreationAsync(string presetId)
        {
            try
            {
                if (connection.State != HubConnectionState.Connected)
                    await connection.StartAsync();

                await connection.InvokeAsync("StartPresetCreation", presetId);
            }
            catch (Exception) { }
        }

        public async Task SendPlayerReadyAsync(string presetId, string playerId)
        {
            try
            {
                if (connection.State != HubConnectionState.Connected)
                    await connection.StartAsync();

                await connection.InvokeAsync("PlayerReady", presetId, playerId);
            }
            catch (Exception) { }
        }

        public async Task SendPlayerNotReadyAsync(string presetId, string playerId)
        {
            try
            {
                if (connection.State != HubConnectionState.Connected)
                    await connection.StartAsync();

                await connection.InvokeAsync("PlayerNotReady", presetId, playerId);
            }
            catch (Exception) { }
        }

        public async Task SendPresetCreatedAsync(string presetId)
        {
            try
            {
                if (connection.State != HubConnectionState.Connected)
                    await connection.StartAsync();

                await connection.InvokeAsync("PresetCreated", presetId);
            }
            catch (Exception) { }
        }

        public async Task SendPresetCreationCanceledAsync(string presetId, string playerId)
        {
            try
            {
                if (connection.State != HubConnectionState.Connected)
                    await connection.StartAsync();

                await connection.InvokeAsync("PresetCreationCanceled", presetId, playerId);
            }
            catch (Exception) { }
        }

        public async Task SendChangeSquareAsync(string roomId, int minute)
        {
            try
            {
                if (connection.State != HubConnectionState.Connected)
                    await connection.StartAsync();

                await connection.InvokeAsync("ChangeSquare", roomId, minute);
            }
            catch (Exception) { }
        }

        public async Task SendUnhideSquareAsync(string roomId, int step)
        {
            try
            {
                if (connection.State != HubConnectionState.Connected)
                    await connection.StartAsync();

                await connection.InvokeAsync("UnhideSquare", roomId, step);
            }
            catch (Exception) { }
        }
        
        public async Task SendStartGame(string roomId)
        {
            try
            {
                if (connection.State != HubConnectionState.Connected)
                    await connection.StartAsync();

                await connection.InvokeAsync("StartRoomGame", roomId);
            }
            catch (Exception) { }
        }

        public async Task SendPauseGame(string roomId, TimeSpan currentTime)
        {
            try
            {
                if (connection.State != HubConnectionState.Connected)
                    await connection.StartAsync();

                await connection.InvokeAsync("PauseRoomGame", roomId, currentTime);
            }
            catch (Exception) { }
        }

        public async Task SendStopGame(string roomId)
        {
            try
            {
                if (connection.State != HubConnectionState.Connected)
                    await connection.StartAsync();

                await connection.InvokeAsync("StopRoomGame", roomId);
            }
            catch (Exception) { }
        }

        public async Task SendResumeGame(string roomId, TimeSpan currentTime)
        {
            try
            {
                if (connection.State != HubConnectionState.Connected)
                    await connection.StartAsync();

                await connection.InvokeAsync("ResumeRoomGame", roomId, currentTime);
            }
            catch (Exception) { }
        }

        public async Task SendResetRoomTimer(string roomId)
        {
            try
            {
                if (connection.State != HubConnectionState.Connected)
                    await connection.StartAsync();

                await connection.InvokeAsync("ResetRoomTimer", roomId);
            }
            catch (Exception) { }
        }

        public async Task SendSyncRoomTimer(string roomId, TimeSpan currentTime)
        {
            try
            {
                if (connection.State != HubConnectionState.Connected)
                    await connection.StartAsync();

                await connection.InvokeAsync("SyncRoomTimer", roomId, currentTime);
            }
            catch (Exception) { }
        }



        public async Task SendClearSharedPreset(string presetFileName, string notesFileName)
        {
            try
            {
                if (connection.State != HubConnectionState.Connected)
                    await connection.StartAsync();
                await connection.InvokeAsync("ClearSharedPreset", presetFileName, notesFileName);
            }
            catch (Exception) { }
        }

        public class InvitePlayerEventArgs : EventArgs
        {
            public string CreatorId { get; set; } = default!;
            public string NickName { get; set; } = default!;
            public string PresetId { get; set; } = default!;
            public string PresetName { get; set; } = default!;
            public string PresetJSON { get; set; } = default!;
        }

        public class AcceptInviteEventArgs : EventArgs
        {
            public string PlayerId { get; set; } = default!;

            public string NickName { get; set; } = default!;
        }

        public class RejectInviteEventArgs : EventArgs
        {
            public string NickName { get; set; } = default!;
        }

        public class CheckSquareEventArgs : EventArgs
        {
            public string PlayerId { get; set; } = default!;
            public string SquareId { get; set; } = default!;
        }

        public class UncheckSquareEventArgs : EventArgs
        {
            public string PlayerId { get; set; } = default!;
            public string SquareId { get; set; } = default!;
        }

        public class GameInviteEventArgs : EventArgs
        {
            public string NickName { get; set; } = default!;
            public string Data { get; set; } = default!;
        }

        public class ChangeSquareEventArgs : EventArgs
        {
            public string RoomId { get; set; } = default!;
            public string SlotId { get; set; } = default!;
            public string NewName { get; set; } = default!;
        }

        public class UnhideSquareEventArgs : EventArgs
        {
            public string RoomId { get; set; } = default!;
            public string SlotId { get; set; } = default!;            
        }

        public class RoomTimerSettingsEventArgs : EventArgs
        {
            public string RoomId { get; set; } = default!;
            public int StartTime { get; set; }
            public int AfterRevealTime { get; set; }
            public int UnhideTime { get; set; }
            public int ChangeTime { get; set; }
        }

        public class RoomTimeSyncEventArgs : EventArgs
        {
            public string RoomId { get; set; } = default!;

            public int CurrentTime { get; set; }
        }

        public class SharePresetEventArgs : EventArgs
        {
            public string FromPlayerName { get; set; } = default!;
            public string GameName { get; set; } = default!;
            public string PresetName { get; set; } = default!;
            public string SquaresFileName { get; set; } = default!;
            public string NotesFileName { get; set; } = default!;

        }

        public class RoomBoardEventArgs : EventArgs
        {            
            public IEnumerable<Square> Squares { get; set; } = default!;

        }

        public class PlayerColorChangedEventArgs : EventArgs
        {
            public string PlayerId { get; set; } = default!;
            public BingoColor Color { get; set; } = default!;
        }

        public class RoomTimerEventArgs : EventArgs
        {
            public string RoomId { get; set; } = default!;
            public TimeSpan CurrentTime { get; set; } = default!;
        }

        public class MarkSquareEventArgs : EventArgs
        {
            public string Slot { get; set; } = default!;
            public BingoColor Color { get; set; } = default!;
        }
    }
}
