using Newtonsoft.Json.Linq;
using System.Net.WebSockets;
using System.Text;

var ws = new ClientWebSocket();
string userId = Guid.NewGuid().ToString();
string roomId = "TESTROOMID";

Console.WriteLine("Connecting to server");
await ws.ConnectAsync(new Uri($"ws://localhost:4991/ws?userid={userId}&roomid={roomId}"), CancellationToken.None);
Console.WriteLine("Connected!");

var receiveTask = Task.Run(async () =>
{
    var buffer = new byte[1024 * 4];
    while (true)
    {
        var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        if (result.MessageType == WebSocketMessageType.Close)
        {
            break;
        }

        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
        Console.WriteLine(message);
    }
});


var sendTask = Task.Run(async () =>
{
    while (true)
    {
        var input = Console.ReadLine();

        if (input == "init")
        {
            var jobj = new JObject();
            jobj["type"] = "init";
            var setJobj = new JObject();
            setJobj["sts"] = 10;
            setJobj["ars"] = 60;
            jobj["settings"] = setJobj;
            var bytes = Encoding.UTF8.GetBytes(jobj.ToString());
            await ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        if (input == "start")
        {
            var jobj = new JObject();
            jobj["type"] = "start";
            jobj["time"] = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss");
            var bytes = Encoding.UTF8.GetBytes(jobj.ToString());
            await ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        if (input == "stop")
        {
            var jobj = new JObject();
            jobj["type"] = "stop";
            jobj["time"] = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss");
            var bytes = Encoding.UTF8.GetBytes(jobj.ToString());
            await ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
});

await Task.WhenAny(sendTask, receiveTask);

if (ws.State != WebSocketState.Closed)
{
    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
}

await Task.WhenAll(sendTask, receiveTask);
