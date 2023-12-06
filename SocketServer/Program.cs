using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocketServer;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:4991");
var app = builder.Build();
app.UseWebSockets();

var connections = new List<SocketClient>();

var settings = new List<RoomTimeSettings>();

app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var roomId = context.Request.Query["roomid"];
        var userId = context.Request.Query["userid"];

        using var ws = await context.WebSockets.AcceptWebSocketAsync();

        var newClient = new SocketClient()
        {
            RoomId = roomId,
            UserId = userId,
            Socket = ws
        };
        connections.Add(newClient);

        var roomSettings = settings.FirstOrDefault(i => i.RoomId == newClient.RoomId);
        if (roomSettings!=null)
        {
            await SendMessageToUser(newClient.UserId, roomSettings.Settings);
        }

        await ReceiveMessage(newClient,
            async (result, buffer) =>
            {
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    try
                    {
                        var jobj = JObject.Parse(message);
                        if (jobj["type"].Value<string>() == "init")
                        {
                            var roomsettings = settings.FirstOrDefault(i => i.RoomId == newClient.RoomId);
                            if (roomsettings == null)
                            {
                                roomsettings = new RoomTimeSettings() { RoomId = newClient.RoomId, Settings = jobj.ToString() };
                                settings.Add(roomsettings);
                            }
                        }
                        await SendMessageToRoom(newClient.RoomId, jobj.ToString(Formatting.None));
                    }
                    catch (Exception)
                    {

                    }

                }
                else if (result.MessageType == WebSocketMessageType.Close || ws.State == WebSocketState.Aborted)
                {
                    connections.Remove(newClient);
                    await ws.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                }
            });
    }
    else
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
    }
});

var html = System.IO.File.ReadAllText("RedirectPage.html");

app.MapGet("/", () => Results.Extensions.Html(html));

async Task ReceiveMessage(SocketClient client, Action<WebSocketReceiveResult, byte[]> handleMessage)
{
    var buffer = new byte[1024 * 4];
    while (client.Socket.State == WebSocketState.Open)
    {
        var result = await client.Socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        handleMessage(result, buffer);
    }
}

async Task SendMessageToUser(string userId, string message)
{
    var client = connections.FirstOrDefault(c => c.UserId == userId);
    if (client == null)
        return;

    var bytes = Encoding.UTF8.GetBytes(message);
    if (client.Socket.State == WebSocketState.Open)
    {
        var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
        await client.Socket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
    }
}

async Task SendMessageToRoom( string roomId, string message)
{
    var clients = connections.Where(c => c.RoomId == roomId);
    var bytes = Encoding.UTF8.GetBytes(message);
    foreach (var client in clients)
    {
        if (client.Socket.State == WebSocketState.Open)
        {
            var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
            await client.Socket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}

await app.RunAsync();


public class RoomTimeSettings
{
    public string RoomId { get; set; }

    public string Settings { get; set; }
}