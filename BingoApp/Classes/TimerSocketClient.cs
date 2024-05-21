using BingoApp.Properties;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BingoApp.Classes
{
    public class TimerSocketClient
    {
        private ClientWebSocket ws;

        private string _roomId;
        private string _userId;

        private int _sts;
        private int _ars;


        public event EventHandler<ConnectionChangedEventArgs> ConnectionChangedEvent;
        public event EventHandler<SettingsRecievedEventArgs> SettingsRecievedEvent;
        public event EventHandler<StartRecievedEventArgs> StartRecievedEvent;
        public event EventHandler<StartRecievedEventArgs> StopRecievedEvent;

        private bool keepListening = true;

        public async Task InitAsync(string roomId)
        {
            ws = new ClientWebSocket();
            _roomId = roomId;
            _userId = Guid.NewGuid().ToString();


            await ws.ConnectAsync(new Uri($"ws://{App.TimerSocketAddress}/ws?userid={_userId}&roomid={_roomId}"), CancellationToken.None)
                .ContinueWith((t) =>
                {
                    ConnectionChangedEvent?.Invoke(this, new ConnectionChangedEventArgs()
                    {
                        State = ws.State,
                        CloseStatus = ws.CloseStatus
                    });
                });



            await Task.Factory.StartNew(recieveTask, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private async Task recieveTask()
        {
            var buffer = new byte[1024 * 4];
            while (keepListening)
            {
                var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    ConnectionChangedEvent?.Invoke(this, new ConnectionChangedEventArgs()
                    {
                        State = ws.State,
                        CloseStatus = ws.CloseStatus
                    });
                    break;
                }

                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                var jobj = JObject.Parse(message);
                if (jobj["type"].Value<string>() == "init")
                {
                    SettingsRecievedEvent?.Invoke(this, new SettingsRecievedEventArgs()
                    {
                        StartTimeSeconds = jobj["settings"]["sts"].Value<int>(),
                        AfterRevealSeconds = jobj["settings"]["ars"].Value<int>()
                    });
                }
                if (jobj["type"].Value<string>() == "start")
                {
                    var timeString = jobj["time"].Value<string>();
                    var utcDateTime = DateTime.ParseExact(timeString, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
                    var localTime = utcDateTime.ToLocalTime();
                    StartRecievedEvent?.Invoke(this, new StartRecievedEventArgs()
                    {
                        StartTime = localTime
                    });
                }
                if (jobj["type"].Value<string>() == "stop")
                {
                    var timeString = jobj["time"].Value<string>();
                    var utcDateTime = DateTime.ParseExact(timeString, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
                    var localTime = utcDateTime.ToLocalTime();
                    StopRecievedEvent?.Invoke(this, new StartRecievedEventArgs()
                    {
                        StartTime = localTime
                    });
                }

            }
        }

        public async Task SendSettingsToServerAsync(int sts, int ars)
        {
            _sts = sts;
            _ars = ars;
            var jobj = new JObject();
            jobj["type"] = "init";
            var setJobj = new JObject();
            setJobj["sts"] = _sts;
            setJobj["ars"] = _ars;
            jobj["settings"] = setJobj;
            var bytes = Encoding.UTF8.GetBytes(jobj.ToString());
            await ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task SendStartToServerAsync()
        {
            var jobj = new JObject();
            jobj["type"] = "start";
            jobj["time"] = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss");
            var bytes = Encoding.UTF8.GetBytes(jobj.ToString());
            await ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task SendStopToServerAsync()
        {
            var jobj = new JObject();
            jobj["type"] = "stop";
            jobj["time"] = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss");
            var bytes = Encoding.UTF8.GetBytes(jobj.ToString());
            await ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }


        public async Task DisconnectAsync()
        {
            keepListening = false;
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "End", CancellationToken.None);
            ws.Dispose();
        }

    }

    public class SettingsRecievedEventArgs : EventArgs
    {
        public int StartTimeSeconds { get; set; }

        public int AfterRevealSeconds { get; set; }
    }

    public class StartRecievedEventArgs : EventArgs
    {
        public DateTime StartTime { get; set; }
    }

    public class ConnectionChangedEventArgs : EventArgs
    {
        public WebSocketState State { get; set; }

        public WebSocketCloseStatus? CloseStatus { get; set; }
    }
}
