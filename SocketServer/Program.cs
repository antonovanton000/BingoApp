using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocketServer;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json.Serialization;
using static System.Collections.Specialized.BitVector32;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:4991");
var app = builder.Build();
app.UseWebSockets();

var connections = new List<SocketClient>();
var connectionsGIT = new List<GITSocketClient>();
var playersGIT = new List<GITPlayer>();


var settings = new List<RoomTimeSettings>();
var settingsGIT = new List<GITRoomSettings>();

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
        if (roomSettings != null)
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

app.Map("/wsGIT", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var userId = context.Request.Query["userid"];
        var roomId = context.Request.Query["roomid"];

        using var ws = await context.WebSockets.AcceptWebSocketAsync();

        var oldConnection = connectionsGIT.FirstOrDefault(i => i.UserId == userId && i.RoomId == roomId);
        if (oldConnection != null)
            connectionsGIT.Remove(oldConnection);

        var newClient = new GITSocketClient()
        {
            RoomId = roomId,
            UserId = userId,
            Socket = ws
        };
        connectionsGIT.Add(newClient);

        var roomSettings = settingsGIT.FirstOrDefault(i => i.RoomId == newClient.RoomId);
        if (roomSettings != null)
        {
            var jobj = new JObject();
            jobj["type"] = "init";
            var jsettings = new JObject();
            jsettings["roomId"] = roomSettings.RoomId;
            jsettings["gameName"] = roomSettings.GameName;
            jsettings["itemsIds"] = roomSettings.ItemsIds;
            jsettings["itemsState"] = JArray.Parse(JsonConvert.SerializeObject(roomSettings.ItemsStates));
            jsettings["players"] = JArray.Parse(JsonConvert.SerializeObject(playersGIT.Where(i => i.RoomId == newClient.RoomId && i.Id != newClient.UserId)));

            jobj["settings"] = jsettings;

            await SendMessageToGITUser(newClient.UserId, jobj.ToString());
        }

        await ReceiveGITMessage(newClient,
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
                           var gameName = jobj["settings"]["gameName"].ToString();
                           var itemsIds = jobj["settings"]["itemsIds"].ToString();

                           var roomsettings = settingsGIT.FirstOrDefault(i => i.RoomId == newClient.RoomId);
                           if (roomsettings == null)
                           {
                               roomsettings = new GITRoomSettings() { RoomId = newClient.RoomId, ItemsIds = itemsIds, GameName = gameName };
                               var arr = roomsettings.ItemsIds.Split(",");
                               roomsettings.ItemsStates = new List<ItemState>();
                               foreach (var itemId in arr)
                               {
                                   roomsettings.ItemsStates.Add(new ItemState() { ItemId = itemId, SelectByPlayers = new List<string>() });
                               }
                               settingsGIT.Add(roomsettings);
                           }
                       }
                       if (jobj["type"].Value<string>() == "playerInfo")
                       {
                           var playerInfo = JsonConvert.DeserializeObject<GITPlayer>(jobj["player"].ToString());
                           if (playerInfo != null)
                           {
                               playerInfo.RoomId = newClient.RoomId;
                               var oldPlayer = playersGIT.FirstOrDefault(i => i.RoomId == playerInfo.RoomId && i.Id == playerInfo.Id);
                               if (oldPlayer != null)
                                   playersGIT.Remove(oldPlayer);

                               playersGIT.Add(playerInfo);
                           }
                       }
                       if (jobj["type"].Value<string>() == "action")
                       {
                           var playerId = jobj["playerId"].Value<string>();
                           var roomId = jobj["roomId"].Value<string>();
                           var itemId = jobj["itemId"].Value<string>();
                           var action = jobj["action"].Value<int>();
                           var roomSettings = settingsGIT.FirstOrDefault(i => i.RoomId == roomId);

                           if (roomSettings != null)
                           {
                               var curItem = roomSettings.ItemsStates.FirstOrDefault(i => i.ItemId == itemId);
                               if (curItem != null)
                               {
                                   if (action == 1)
                                   {
                                       curItem.SelectByPlayers.Add(playerId);
                                   }
                                   if (action == 0)
                                   {
                                       curItem.SelectByPlayers.Remove(playerId);
                                   }
                               }
                           }

                       }
                       if (jobj["type"].Value<string>() == "disconnect")
                       {
                           connectionsGIT.Remove(newClient);
                           playersGIT.Remove(playersGIT.FirstOrDefault(i => i.Id == newClient.UserId));                           
                       }
                       await SendMessageToGITRoom(newClient.UserId, newClient.RoomId, jobj.ToString(Formatting.None));
                   }
                   catch (Exception)
                   {

                   }

               }
               else if (result.MessageType == WebSocketMessageType.Close || ws.State == WebSocketState.Aborted)
               {
                   try
                   {
                       connectionsGIT.Remove(newClient);
                       playersGIT.Remove(playersGIT.FirstOrDefault(i => i.Id == newClient.UserId));
                       var jobj = new JObject();
                       jobj["type"] = "disconect";
                       jobj["playerId"] = newClient.UserId;
                       await SendMessageToGITRoom(newClient.UserId, newClient.RoomId, jobj.ToString());
                       await ws.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                   }
                   catch (Exception)
                   {
                   }
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

#region BingoTimer
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

async Task SendMessageToRoom(string roomId, string message)
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

#endregion

#region GIT

async Task ReceiveGITMessage(GITSocketClient client, Action<WebSocketReceiveResult, byte[]> handleMessage)
{
    try
    {
        var buffer = new byte[1024 * 12];
        while (client.Socket.State == WebSocketState.Open)
        {
            var result = await client.Socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            handleMessage(result, buffer);
        }
    }
    catch (Exception)
    {
    }
}

async Task SendMessageToGITUser(string userId, string message)
{
    try
    {

        var client = connectionsGIT.FirstOrDefault(c => c.UserId == userId);
        if (client == null)
            return;

        var bytes = Encoding.UTF8.GetBytes(message);
        if (client.Socket.State == WebSocketState.Open)
        {
            var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
            await client.Socket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
    catch (Exception) { }
}

async Task SendMessageToGITRoom(string senderId, string roomId, string message)
{
    try
    {

        var clients = connectionsGIT.Where(c => c.RoomId == roomId && c.UserId != senderId);
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
    catch (Exception) { }

}

#endregion

await app.RunAsync();


public class RoomTimeSettings
{
    public string RoomId { get; set; }

    public string Settings { get; set; }
}

public class GITRoomSettings
{
    public string RoomId { get; set; }
    public string GameName { get; set; }
    public string ItemsIds { get; set; }

    public List<ItemState> ItemsStates { get; set; } = new List<ItemState>();
}

public class ItemState
{
    public string ItemId { get; set; }

    public List<string> SelectByPlayers { get; set; } = new List<string>();

}

public class GITPlayer
{
    public string RoomId { get; set; }
    public string Id { get; set; }
    public string NickName { get; set; }
    public string Picture { get; set; }
}