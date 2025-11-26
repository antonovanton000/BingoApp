using BingoApp.Classes;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json.Linq;
using System.Net.WebSockets;
using System.Text;


await TestBingoAppMainHub();

Console.ReadLine();

static async Task TestBingoAppMainHub()
{
    var playerId = "6057b32a-da4f-45de-9fe8-86603c7a0fc0";
    var nickName = "ConsolePlayer";

    bool IsHubConnected;
    string baseUrl = "http://localhost:5054/";

    IsHubConnected = false;
    var connection = new HubConnectionBuilder()
           .WithUrl($"{baseUrl}bingohub")
           .Build();

    connection.On<string, string, string, string, string>("InvitePlayer",  async (creatorId, nickname, presetId, presetName, presetJSON) =>
    {
        Console.WriteLine($"InvitePlayer: {creatorId}, {nickname}, {presetId}, {presetName}\r\n{presetJSON}");

        await Task.Delay(1000);
        await connection.InvokeAsync("AcceptInvite", presetId, playerId);
    });


    connection.On<string, string>("AcceptInvite", (playerId, nickName) => 
    {
        Console.WriteLine($"AcceptInvite: {playerId}, {nickName}");

    });

    connection.On<string>("RejectInvite", (nickName) =>
    {
        Console.WriteLine($"RejectInvite: {nickName}");
    });

    connection.On<string, string>("CheckSquare", (playerId, squareId) =>
    {
        Console.WriteLine($"CheckSquare: {playerId}, {squareId}");
    });

    connection.On<string, string>("UncheckSquare", (playerId, squareId) =>
    {
        Console.WriteLine($"UncheckSquare: {playerId}, {squareId}");
    });

    connection.On<string>("Connected", (playerId) =>
    {
        Console.WriteLine($"Connected: {playerId}");
    });


    var restClient = new RestClient();

    var resp = await restClient.UpdateUserInfoAsync(new BingoApp.Models.BingoAppPlayer() { Id = playerId, NickName = nickName });
    await connection.StartAsync();
    Console.WriteLine(connection.State);
    await Task.Delay(100);
    await connection.InvokeAsync("Connect", playerId);
}




static async Task TestGameItemTrackerSocket()
{
    var ws = new ClientWebSocket();
    string userId = Guid.NewGuid().ToString();
    string roomId = "TESTROOMID";

    Console.WriteLine("Connecting to server");
    await ws.ConnectAsync(new Uri($"ws://172.17.0.1:4991/ws?userid={userId}&roomid={roomId}"), CancellationToken.None);
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
}
