using BingoApp.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BingoApp.Classes
{
    public class WsClient : IDisposable
    {
        public int ReceiveBufferSize { get; set; } = 8192;

        private ClientWebSocket WS;
        private CancellationTokenSource CTS;

        private Room ActiveRoom { get; set; }

        public event EventHandler NewCardEvent;

        public WsClient()
        {
            ActiveRoom = null;
        }
        public WsClient(Room room)
        {
            ActiveRoom = room;
        }

        public async Task ConnectAsync(string url)
        {
            if (WS != null)
            {
                if (WS.State == WebSocketState.Open) return;
                else WS.Dispose();
            }
            WS = new ClientWebSocket();
            if (CTS != null) CTS.Dispose();
            CTS = new CancellationTokenSource();
            await WS.ConnectAsync(new Uri(url), CTS.Token);
            ActiveRoom.IsConnectedToServer = true;
            await Task.Factory.StartNew(ReceiveLoop, CTS.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public async Task DisconnectAsync()
        {
            if (WS is null) return;
            // TODO: requests cleanup code, sub-protocol dependent.
            if (WS.State == WebSocketState.Open)
            {
                CTS.CancelAfter(TimeSpan.FromSeconds(2));
                await WS.CloseOutputAsync(WebSocketCloseStatus.Empty, "", CancellationToken.None);
                await WS.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            }
            ActiveRoom.IsConnectedToServer = false;
            WS.Dispose();
            WS = null;
            CTS.Dispose();
            CTS = null;
        }

        private async Task ReceiveLoop()
        {
            var loopToken = CTS.Token;
            MemoryStream outputStream = null;
            WebSocketReceiveResult receiveResult = null;
            var buffer = new byte[ReceiveBufferSize];
            try
            {
                while (!loopToken.IsCancellationRequested)
                {
                    outputStream = new MemoryStream(ReceiveBufferSize);
                    do
                    {
                        receiveResult = await WS.ReceiveAsync(buffer, CTS.Token);
                        if (receiveResult.MessageType != WebSocketMessageType.Close)
                            outputStream.Write(buffer, 0, receiveResult.Count);
                    }
                    while (!receiveResult.EndOfMessage);
                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        ActiveRoom.IsConnectedToServer = false;
                    }
                    outputStream.Position = 0;
                    await ResponseReceived(outputStream);
                }
            }
            catch (TaskCanceledException) { }
            finally
            {
                outputStream?.Dispose();
            }
        }

        public async Task SendTempSocketKeyAsync(string socketKey)
        {
            var jobj = new JObject();
            jobj["socket_key"] = socketKey;
            var socketJson = Encoding.UTF8.GetBytes(jobj.ToString());
            var message = new ReadOnlyMemory<byte>(socketJson);
            await WS.SendAsync(message, WebSocketMessageType.Text, true, CancellationToken.None);

        }

        private async Task ResponseReceived(MemoryStream inputStream)
        {
            var message = Encoding.UTF8.GetString(inputStream.ToArray());
            var jobj = JObject.Parse(message);

            if (jobj["type"]?.Value<string>() == "error")
            {
                //console.log("Got error message from socket: ", json);
                MainWindow.ShowMessage("Error happened :(", MessageNotificationType.Ok);
                return;
            }

            await App.Current.Dispatcher.InvokeAsync(async () =>
            {
                var chatEvent = new Event()
                {
                    Type = (EventType)Enum.Parse(typeof(EventType), jobj["type"].Value<string>().Replace("-", "")),
                    Timestamp = App.UnixTimeStampToDateTime(jobj["timestamp"].Value<double>()),

                    Player = new Player()
                    {
                        IsSpectator = jobj["player"]["is_spectator"].Value<bool>(),
                        Uuid = jobj["player"]["uuid"].Value<string>(),
                        NickName = jobj["player"]["name"].Value<string>(),
                        Color = (BingoColor)Enum.Parse(typeof(BingoColor), jobj["player"]["color"].Value<string>())
                    },
                    PlayerColor = (BingoColor)Enum.Parse(typeof(BingoColor), jobj["player_color"].Value<string>())
                };

                if (jobj.ContainsKey("event_type"))
                {
                    chatEvent.EventType = (EventSubType)Enum.Parse(typeof(EventSubType), jobj["event_type"].Value<string>());
                }
                if (jobj.ContainsKey("remove"))
                {
                    chatEvent.Remove = jobj["remove"].Value<bool>();
                }
                if (jobj.ContainsKey("square"))
                {
                    var arr = jobj["square"]["colors"].Value<string>().Split(" ");
                    var colors = new ObservableCollection<BingoColor>();
                    foreach (var cl in arr)
                    {
                        colors.Add((BingoColor)Enum.Parse(typeof(BingoColor), cl));
                    }
                    chatEvent.Square = new Square()
                    {
                        Slot = jobj["square"]["slot"].Value<string>(),
                        Name = jobj["square"]["name"].Value<string>(),
                        SquareColors = colors
                    };
                }
                if (chatEvent.Type == EventType.chat)
                {
                    chatEvent.Message = jobj["text"].Value<string>();
                }
                if (jobj.ContainsKey("color"))
                {
                    chatEvent.Color = (BingoColor)Enum.Parse(typeof(BingoColor), jobj["color"].Value<string>());
                }

                ActiveRoom.ChatMessages.Add(chatEvent);


                if (jobj["type"]?.Value<string>() == "error")
                {
                    //console.log("Got error message from socket: ", json);
                    return;
                }
                else if (jobj["type"]?.Value<string>() == "goal")
                {
                    var squareobj = jobj["square"];
                    var square = ActiveRoom.Board.GetSquare(squareobj["slot"].Value<string>());
                    square.IsMarking = false;
                    var arr = squareobj["colors"].Value<string>().Split(" ");
                    square.SquareColors.Clear();
                    foreach (var item in arr)
                    {
                        square.SquareColors.Add((BingoColor)Enum.Parse(typeof(BingoColor), item));
                    }
                    ActiveRoom.UpdatePlayersGoals();
                }
                else if (jobj["type"]?.Value<string>() == "color")
                {
                    ActiveRoom.AddNewPlayer(jobj["player"] as JObject);
                    ActiveRoom.UpdatePlayersGoals();
                }
                else if (jobj["type"]?.Value<string>() == "connection")
                {
                    var jplayer = jobj["player"];
                    if (jobj["event_type"]?.Value<string>() == "connected")
                    {
                        ActiveRoom.AddNewPlayer(jobj["player"] as JObject);
                        ActiveRoom.UpdatePlayersGoals();
                    }
                    else if (jobj["event_type"]?.Value<string>() == "disconnected")
                    {
                        var playerJobj = jobj["player"] as JObject;
                        ActiveRoom.RemovePlayer(playerJobj["uuid"].Value<string>());
                    }
                }
                else if (jobj["type"]?.Value<string>() == "new-card")
                {
                    // TODO: remove this external dependency
                    // if the card was never revealed show what the seed was in the chat anyway
                    //$("#bingo-chat .new-card-message .seed-hidden").text(ROOM_SETTINGS.seed).removeClass('seed-hidden').addClass('seed');
                    //    refreshBoard();
                    NewCardEvent?.Invoke(this, EventArgs.Empty);                    
                }
                else if (jobj["type"]?.Value<string>() == "chat")
                {
                    // no special effects for chat, it just gets written to the panel
                }
                else
                {
                    //console.log("unrecognized event type: ", json);
                }

            });
        }

        public void Dispose() => DisconnectAsync().Wait();



    }

}

//CONNECTED RESPONSE JSON
//{ "timestamp": 1698613451.356494, "player": { "name": "Test3", "uuid": "fPFuBghRSIiI-TiOGK0lCw", "color": "red", "is_spectator": false}, "type": "connection", "player_color": "red", "event_type": "connected", "room": "HPpMsW2lRHOdOSW-evJHQQ"}
//REVEAL RESPONSE JSON
//{"timestamp": 1698637367.387235, "player_color": "red", "player": {"name": "DirefulLemur0", "uuid": "76uF1s91SHyrJf3kDDXOIA", "color": "red", "is_spectator": false}, "type": "revealed", "room": "HPpMsW2lRHOdOSW-evJHQQ"}
