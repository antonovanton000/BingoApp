using WheelGame.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WheelGame;

namespace WheelGame.Classes
{
    public partial class WheelSignalRHub : ObservableObject
    {
        [ObservableProperty]
        bool isHubConnected;

        private string _playerId = "";

        public event EventHandler<string>? OnStartGameRecieved;
        public event EventHandler<string>? OnPauseGameRecieved;
        public event EventHandler<string>? OnResumeGameRecieved;
        public event EventHandler<string>? OnStopGameRecieved;
        public event EventHandler<string>? OnPlayerDisconnected;
        public event EventHandler<Player>? OnPlayerConnected;
        public event EventHandler<Player>? OnPlayerReconnected;
        public event EventHandler<string>? OnReconnecting;
        public event EventHandler<string>? OnReconnected;
        public event EventHandler<string>? OnDisconnected;
        public event EventHandler<string>? OnConnected;
        public event EventHandler<int>? OnRotateWheel;
        public event EventHandler<int>? OnStartTimer;
        public event EventHandler? OnStopTimer;
        public event EventHandler<MarkObjectiveEventArgs>? OnMarkObjectiveRecieved;
        public event EventHandler<MarkObjectiveModifierEventArgs>? OnMarkObjectiveModifierRecieved;
        public event EventHandler<MarkObjectiveEventArgs>? OnUnmarkObjectiveRecieved;
        public event EventHandler<MarkObjectiveModifierEventArgs>? OnUnmarkObjectiveModifierRecieved;
        public event EventHandler<string> OnChangePlayerTurn;
        public event EventHandler<string> OnFinishCurrentObjective;
        public event EventHandler<string> OnRemoveWheelSector;
        public event EventHandler<string> OnNewDebuffRecieved;

        HubConnection connection;
        public WheelSignalRHub()
        {
            connection = new HubConnectionBuilder()
                           .WithUrl($"{App.TimerSocketScheme}://{App.TimerSocketAddress}/wheelHub")
                           .Build();

            connection.HandshakeTimeout = TimeSpan.FromSeconds(3);
            connection.KeepAliveInterval = TimeSpan.FromSeconds(3);

            InitHubEndpoints();
        }

        private void InitHubEndpoints()
        {            
            connection.On<string>("StartRoomGame", (roomId) =>
            {
                OnStartGameRecieved?.Invoke(this, roomId);
            });

            connection.On<string>("PauseRoomGame", (roomId) =>
            {
                OnPauseGameRecieved?.Invoke(this, roomId);
            });

            connection.On<string>("ResumeRoomGame", (roomId) =>
            {
                OnResumeGameRecieved?.Invoke(this, roomId);
            });

            connection.On<string>("StopRoomGame", (roomId) =>
            {
                OnStopGameRecieved?.Invoke(this, roomId);
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

            connection.On<string>("PlayerReconnected", json =>
            {
                var roomPlayer = JsonConvert.DeserializeObject<Player>(json);
                if (roomPlayer != null)
                {
                    OnPlayerReconnected?.Invoke(this, roomPlayer);
                }
            });
            
            connection.On<string, string>("MarkObjective", (slot, playerId) =>
            {
                OnMarkObjectiveRecieved?.Invoke(this, new MarkObjectiveEventArgs() { Slot = slot, PlayerId = playerId });
            });

            connection.On<string, string>("UnmarkObjective", (slot, playerId) =>
            {
                OnUnmarkObjectiveRecieved?.Invoke(this, new MarkObjectiveEventArgs() { Slot = slot, PlayerId = playerId });
            });

            connection.On<string, string, string>("MarkObjectiveModifier", (slot, slotModifier, playerId) =>
            {
                OnMarkObjectiveModifierRecieved?.Invoke(this, new MarkObjectiveModifierEventArgs() { Slot = slot, PlayerId = playerId, ModifierSlot = slotModifier });
            });

            connection.On<string, string, string>("UnmarkObjectiveModifier", (slot, slotModifier, playerId) =>
            {
                OnUnmarkObjectiveModifierRecieved?.Invoke(this, new MarkObjectiveModifierEventArgs() { Slot = slot, PlayerId = playerId, ModifierSlot = slotModifier });
            });

            connection.On<string>("FinishCurrentObjective", (playerId) => {
                OnFinishCurrentObjective?.Invoke(this, playerId);
            });

            connection.On<string>("RemoveWheelSector", (slotId) =>
            {
                OnRemoveWheelSector?.Invoke(this, slotId);
            });

            connection.On<string>("Connected", playerId =>
            {
                OnConnected?.Invoke(this, playerId);
            });

            connection.On<int>("RotateWheel", angle =>
            {
                OnRotateWheel?.Invoke(this, angle);
            });

            connection.On<string>("ChangePlayerTurn", playerId =>
            {
                OnChangePlayerTurn?.Invoke(this, playerId);
            });

            connection.On<int>("StartTimer", seconds => 
            { 
                OnStartTimer?.Invoke(this, seconds);
            });

            connection.On("StopTimer", () =>
            {
                OnStopTimer?.Invoke(this, EventArgs.Empty);
            });

            connection.On<string>("NewDebuff", newDebuff =>
            {
                OnNewDebuffRecieved?.Invoke(this, newDebuff);
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

        public async Task SendPauseGame(string roomId)
        {
            try
            {
                if (connection.State != HubConnectionState.Connected)
                    await connection.StartAsync();

                await connection.InvokeAsync("PauseRoomGame", roomId);
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

        public async Task SendResumeGame(string roomId)
        {
            try
            {
                if (connection.State != HubConnectionState.Connected)
                    await connection.StartAsync();

                await connection.InvokeAsync("ResumeRoomGame", roomId);
            }
            catch (Exception) { }
        }
        public async Task SendRotateWheel(string roomId, int angle)
        {
            try
            {
                if (connection.State != HubConnectionState.Connected)
                    await connection.StartAsync();
                await connection.InvokeAsync("RotateWheel", roomId, angle);
            }
            catch (Exception) { }
        }

        public async Task SendMarkObjective(string roomId, string slotId)
        {
            try
            {
                if (connection.State != HubConnectionState.Connected)
                    await connection.StartAsync();
                await connection.InvokeAsync("MarkObjective", roomId, slotId);
            }
            catch (Exception) { }
        }

        public async Task SendMarkObjectiveModifier(string roomId, string slotId, string modifierId)
        {
            try
            {
                if (connection.State != HubConnectionState.Connected)
                    await connection.StartAsync();
                await connection.InvokeAsync("MarkObjectiveModifier", roomId, slotId, modifierId);
            }
            catch (Exception) { }
        }

        public async Task SendFinishCurrentObjective(string roomId, string slotId, string playerId)
        {
            try
            {
                if (connection.State != HubConnectionState.Connected)
                    await connection.StartAsync();
                await connection.InvokeAsync("FinishCurrentObjective", roomId, slotId, playerId);
            }
            catch (Exception) { }
        }
        
        public async Task SendGetNewRandomDebuff(string roomId)
        {
            try
            {
                if (connection.State != HubConnectionState.Connected)
                    await connection.StartAsync();
                await connection.InvokeAsync("GetNewRandomDebuff", roomId);
            }
            catch (Exception) { }
        }

        public class MarkObjectiveEventArgs : EventArgs
        {
            public string Slot { get; set; } = default!;
            public string PlayerId { get; set; } = default!;
        }
        public class MarkObjectiveModifierEventArgs : EventArgs
        {
            public string Slot { get; set; } = default!;
            public string ModifierSlot { get; set; } = default!;
            public string PlayerId { get; set; } = default!;
        }
    }
}
