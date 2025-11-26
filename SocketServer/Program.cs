using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocketServer;
using SocketServer.Controllers;
using SocketServer.Hubs;
using SocketServer.Models;
using SocketServer.Models.Bingo;
using SocketServer.Models.GIT;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

var builder = WebApplication.CreateBuilder(args);

#if !DEBUG
builder.WebHost.UseUrls("http://0.0.0.0:4999");
#endif

builder.Services.AddSignalR(o =>
{
    o.EnableDetailedErrors = true;
});

builder.Services.AddControllers();
builder.Services.AddMvc();

HashSet<SocketServer.Models.Player> Players = new();
HashSet<SocketServer.Models.Player> HPPlayers = new();
HashSet<SocketServer.Models.Player> WPlayers = new();
HashSet<SocketServer.Models.GIT.Player> GPlayers = new();

HashSet<GameResult> GamesResults = new HashSet<GameResult>();
HashSet<SocketServer.Models.Huntpoint.GameResult> HPGamesResults = new HashSet<SocketServer.Models.Huntpoint.GameResult>();

HashSet<SocketServer.Models.Bingo.Room> Rooms = new HashSet<SocketServer.Models.Bingo.Room>();
HashSet<SocketServer.Models.GIT.Room> GRooms = new HashSet<SocketServer.Models.GIT.Room>();
HashSet<SocketServer.Models.Huntpoint.Room> HPRooms = new HashSet<SocketServer.Models.Huntpoint.Room>();
HashSet<SocketServer.Models.Wheel.Room> WRooms = new HashSet<SocketServer.Models.Wheel.Room>();

#region Players
if (File.Exists("players.json"))
{
    var jsonPlayers = await File.ReadAllTextAsync("players.json");
    if (!string.IsNullOrEmpty(jsonPlayers))
    {
        try
        {
            var playersList = JsonConvert.DeserializeObject<HashSet<SocketServer.Models.Player>>(jsonPlayers);
            if (playersList != null)
                Players = playersList;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

    }
}

if (File.Exists("hpplayers.json"))
{
    var jsonPlayers = await File.ReadAllTextAsync("hpplayers.json");
    if (!string.IsNullOrEmpty(jsonPlayers))
    {
        try
        {
            var hpplayersList = JsonConvert.DeserializeObject<HashSet<SocketServer.Models.Player>>(jsonPlayers);
            if (hpplayersList != null)
                HPPlayers = hpplayersList;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

    }
}

if (File.Exists("wplayers.json"))
{
    var jsonPlayers = await File.ReadAllTextAsync("wplayers.json");
    if (!string.IsNullOrEmpty(jsonPlayers))
    {
        try
        {
            var wplayersList = JsonConvert.DeserializeObject<HashSet<SocketServer.Models.Player>>(jsonPlayers);
            if (wplayersList != null)
                WPlayers = wplayersList;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

    }
}


if (File.Exists("gplayers.json"))
{
    var jsonPlayers = await File.ReadAllTextAsync("gplayers.json");
    if (!string.IsNullOrEmpty(jsonPlayers))
    {
        try
        {
            var gplayersList = JsonConvert.DeserializeObject<HashSet<SocketServer.Models.GIT.Player>>(jsonPlayers);
            if (gplayersList != null)
                GPlayers = gplayersList;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

    }
}

#endregion

#region Results

if (File.Exists("wwwroot/uploads/results/gamesResults.json"))
{
    var jsonGamesResult = await File.ReadAllTextAsync("wwwroot/uploads/results/gamesResults.json");
    if (!string.IsNullOrEmpty(jsonGamesResult))
    {
        try
        {
            var gamesResultList = JsonConvert.DeserializeObject<HashSet<GameResult>>(jsonGamesResult);
            if (gamesResultList != null)
                GamesResults = gamesResultList;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

    }
}

if (File.Exists("wwwroot/uploads/results/hpgamesResults.json"))
{
    var jsonGamesResult = await File.ReadAllTextAsync("wwwroot/uploads/results/hpgamesResults.json");
    if (!string.IsNullOrEmpty(jsonGamesResult))
    {
        try
        {
            var hpgamesResultList = JsonConvert.DeserializeObject<HashSet<SocketServer.Models.Huntpoint.GameResult>>(jsonGamesResult);
            if (hpgamesResultList != null)
                HPGamesResults = hpgamesResultList;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}

#endregion

BingoHub bingoHub = new(Players, Rooms);
builder.Services.AddSingleton(bingoHub);

HuntpointHub huntpointHub = new(HPPlayers, HPRooms);
builder.Services.AddSingleton(huntpointHub);

WheelHub wheelHub = new(WPlayers, WRooms);
builder.Services.AddSingleton(wheelHub);

GITHub gitHub = new(GPlayers, GRooms);
builder.Services.AddSingleton(gitHub);

var taskFactory = new TaskFactory();
await taskFactory.StartNew(async () =>
{
    while (true)
    {
        foreach (var player in Players.Where(i => (DateTime.Now - i.LastActionTime).TotalMinutes > 5))
        {
            player.IsOnline = false;
            player.IsAvailable = false;
        }

        foreach (var hpplayer in HPPlayers.Where(i => (DateTime.Now - i.LastActionTime).TotalMinutes > 5))
        {
            hpplayer.IsOnline = false;
            hpplayer.IsAvailable = false;
        }
        foreach (var wplayer in WPlayers.Where(i => (DateTime.Now - i.LastActionTime).TotalMinutes > 5))
        {
            wplayer.IsOnline = false;
            wplayer.IsAvailable = false;
        }

        foreach (var gplayer in GPlayers.Where(i => (DateTime.Now - i.LastActionTime).TotalMinutes > 5))
        {
            gplayer.IsOnline = false;            
        }

        await Task.Delay(60000);
    }
});

var app = builder.Build();

app.UseWebSockets();

app.MapGet("/test", () => "SocketServer running...");

#region GIT
app.MapHub<GITHub>("/gitHub");
app.MapPost("/git/ping/{playerId}", (string playerId) =>
{
    var player = GPlayers.FirstOrDefault(i => i.Id == playerId);
    if (player != null)
    {
        player.IsOnline = true;
        player.LastActionTime = DateTime.Now;
    }
    return "pong";
});

app.MapPost("/git/players/signup", async (SocketServer.Models.GIT.Player player) =>
{
    var existPlayer = GPlayers.FirstOrDefault(i => i.Id == player.Id);
    if (existPlayer != null)
    {
        existPlayer.IsOnline = true;        
        existPlayer.NickName = player.NickName;
        existPlayer.Picture = player.Picture;        
        existPlayer.LastActionTime = DateTime.Now;
    }
    else
    {
        player.IsOnline = true;        
        player.LastActionTime = DateTime.Now;
        GPlayers.Add(player);
    }
    var json = JsonConvert.SerializeObject(GPlayers, Formatting.None);
    await File.WriteAllTextAsync("gplayers.json", json);
    return "success";
});


app.MapPost("/git/room/create", (SocketServer.Models.GIT.NewRoomModel newroomModel) =>
{
    var room = new SocketServer.Models.GIT.Room();
    try
    {
        room.CreatorId = newroomModel.CreatorId;
        room.GameName = newroomModel.GameName;
        room.ItemsIds = newroomModel.ItemsIds.ToList();
        room.ItemsStates = new List<SocketServer.Models.GIT.ItemState>();
        foreach (var itemId in newroomModel.ItemsIds)
        {
            room.ItemsStates.Add(new SocketServer.Models.GIT.ItemState() { ItemId = itemId, SelectByPlayers = new List<string>() });
        }
        var player = GPlayers.FirstOrDefault(i => i.Id == newroomModel.CreatorId);
        if (player != null)
        {
            room.CreatorName = player.NickName;
            var newroomPlayer = new SocketServer.Models.GIT.Player()
            {
                Id = player.Id,
                NickName = player.NickName,
                Picture = player.Picture
            };
            room.Players.Add(newroomPlayer);
        }
    }
    catch (Exception ex)
    {
        return JsonConvert.SerializeObject(new { error = "Error happend.", message = ex.Message });
    }

    GRooms.Add(room);
    return JsonConvert.SerializeObject(room);
});

app.MapPost("/git/room/connect", (ConnectToRoomModel model) =>
{
    var room = GRooms.FirstOrDefault(i => i.RoomConnectNumber == model.ConnectNumber);
    if (room != null)
    {
        var player = GPlayers.FirstOrDefault(i => i.Id == model.PlayerId);
        if (player != null)
        {
            if (!room.Players.Any(i => i.Id == player.Id))
            {
                var newroomPlayer = new SocketServer.Models.GIT.Player()
                {
                    Id = player.Id,
                    NickName = player.NickName,
                    Picture = player.Picture
                };
                room.Players.Add(newroomPlayer);
            }
        }
        var json = JsonConvert.SerializeObject(room);
        return json;
    }
    else
        return "notfound";
});

#endregion

#region BingoServer
app.MapHub<BingoHub>("/bingohub");
var html = System.IO.File.ReadAllText("RedirectPage.html");
app.MapGet("/openapp", () => Results.Extensions.Html(html));

app.MapPost("/ping/{playerId}", (string playerId) =>
{
    var player = Players.FirstOrDefault(i => i.Id == playerId);
    if (player != null)
    {
        player.IsOnline = true;
        player.LastActionTime = DateTime.Now;
    }
    return "pong";
});

app.MapPost("/players/signup", async (SocketServer.Models.Player player) =>
{
    var existPlayer = Players.FirstOrDefault(i => i.Id == player.Id);
    if (existPlayer != null)
    {
        existPlayer.IsOnline = true;
        existPlayer.IsAvailable = true;
        existPlayer.NickName = player.NickName;
        //existPlayer.AvatarBase64 = player.AvatarBase64;        
        existPlayer.LastActionTime = DateTime.Now;
    }
    else
    {
        player.IsOnline = true;
        player.IsAvailable = true;
        player.LastActionTime = DateTime.Now;
        Players.Add(player);
    }
    var json = JsonConvert.SerializeObject(Players, Formatting.None);
    await File.WriteAllTextAsync("players.json", json);
    return "success";
});

app.MapGet("/players/info/{playerId}", (string playerId) =>
{
    var player = Players.FirstOrDefault(i => i.Id == playerId);
    if (player != null)
    {
        return new SocketServer.Models.Player() { Id = player.Id, IsOnline = player.IsOnline, NickName = player.NickName, IsAvailable = player.IsAvailable };
    }
    return null;
});

app.MapGet("/players/available/{playerId}", (string playerId) =>
{
    var player = Players.FirstOrDefault(i => i.Id == playerId);
    if (player != null)
    {
        player.IsOnline = true;
        player.LastActionTime = DateTime.Now;
    }

    return Players.Where(i => i.IsOnline && i.Id != playerId);
});

app.MapGet("/players/all/{playerId}", (string playerId) =>
{
    var player = Players.FirstOrDefault(i => i.Id == playerId);
    if (player != null)
    {
        player.IsOnline = true;
        player.LastActionTime = DateTime.Now;
    }

    return Players.Where(i => i.Id != playerId).OrderByDescending(i => i.LastActionTime);
});

app.MapPost("/room/create", (SocketServer.Models.Bingo.NewRoomModel newroomModel) =>
{
    var room = new SocketServer.Models.Bingo.Room();
    try
    {
        room.RoomName = newroomModel.RoomName;
        room.CreaionDate = DateTime.UtcNow;
        room.CreatorId = newroomModel.CreatorId;

        if (!string.IsNullOrEmpty(newroomModel.Password))
            room.RoomPassword = Convert.ToBase64String(Encoding.UTF8.GetBytes(newroomModel.Password));

        room.RoomSettings = new SocketServer.Models.Bingo.RoomSettings()
        {
            ExtraGameMode = newroomModel.GameExtraMode,
            GameMode = newroomModel.GameMode,
            GameName = newroomModel.GameName,
            PresetName = newroomModel.PresetName,
            HideCard = newroomModel.HideCard,
            IsAutoBoardReveal = newroomModel.IsAutoBoardReveal,
            IsAutoFogWall = newroomModel.IsAutoFogWall,
            UnhideTimeMinutes = newroomModel.UnhideTimeMinutes,
            ChangeTimeMinutes = newroomModel.ChangeTimeMinutes,
            StartTimeSeconds = newroomModel.StartTimeSeconds,
            AfterRevealSeconds = newroomModel.AfterRevealSeconds,
            IsTripleBingoSelect = newroomModel.IsTripleBingoSelect
        };
        var player = Players.FirstOrDefault(i => i.Id == newroomModel.CreatorId);
        if (player != null)
        {
            room.CreatorName = player.NickName;
            var newroomPlayer = new SocketServer.Models.Bingo.Player()
            {
                Id = player.Id,
                NickName = player.NickName,
                Color = BingoColor.red,
                IsSpectator = newroomModel.AsSpectator,
                IsBoardRevealed = !(newroomModel.IsAutoBoardReveal || newroomModel.HideCard)
            };
            room.Players.Add(newroomPlayer);
        }
        room.PresetJson = newroomModel.BoardJSON;
        room.GenerateBoardFromJson();
    }
    catch (Exception ex)
    {
        return JsonConvert.SerializeObject(new { error = "Invalid preset JSON format.", message = ex.Message });
    }

    Rooms.Add(room);
    return JsonConvert.SerializeObject(room);
});

app.MapGet("/room/info/{roomId}", (string roomId) =>
{
    var room = Rooms.FirstOrDefault(i => i.RoomId == roomId);
    if (room != null)
    {
        return JsonConvert.SerializeObject(new
        {
            room.RoomId,
            room.RoomName,
            room.RoomSettings.GameName,
            room.RoomSettings.PresetName,
            room.CreatorName,
            room.RoomSettings.ExtraGameMode,
            room.RoomSettings.GameMode
        });
    }
    else
        return "notfound";
});

app.MapPost("/room/connect", (SocketServer.Models.Bingo.ConnectRoomModel data) =>
{
    var room = Rooms.FirstOrDefault(i => i.RoomId == data.RoomId);
    if (room != null)
    {
        if (!data.AsSpectator)
        {
            if (!string.IsNullOrEmpty(room.RoomPassword))
            {
                if (string.IsNullOrEmpty(data.Password) || Convert.ToBase64String(Encoding.UTF8.GetBytes(data.Password)) != room.RoomPassword)
                {
                    return "wrongpassword";
                }
            }
        }

        var player = Players.FirstOrDefault(i => i.Id == data.PlayerId);
        if (player != null)
        {
            if (!room.Players.Any(i => i.Id == player.Id))
            {
                var newroomPlayer = new SocketServer.Models.Bingo.Player()
                {
                    Id = player.Id,
                    NickName = player.NickName,
                    Color = SocketServer.Models.Bingo.BingoColor.red,
                    IsSpectator = data.AsSpectator,
                    IsBoardRevealed = !(room.RoomSettings.IsAutoBoardReveal || room.RoomSettings.HideCard)
                };

                room.Players.Add(newroomPlayer);
            }
        }

        room.IsCreatorMode = room.CreatorId == data.PlayerId;
        var json = JsonConvert.SerializeObject(room);
        return json;
    }
    else
        return "notfound";
});

app.MapPost("/room/newboard/{roomId}", async (string roomId) =>
{
    var room = Rooms.FirstOrDefault(i => i.RoomId == roomId);
    if (room != null)
    {
        room.GenerateBoardFromJson();
        await bingoHub.SendNewRoomBoard(room);
        return "success";
    }
    else
        return "notfound";
});

app.MapPost("/game/invite", async (GameInviteData data) =>
{
    await bingoHub.SendGameInvite(data.PlayerId, data.InvitePlayerId, data.Data);
    return "success";
});

app.MapPost("/preset/share", async (SendPresetModel data) =>
{
    var fromPlayer = Players.FirstOrDefault(i => i.Id == data.FromPlayerId);
    if (fromPlayer != null)
    {
        var newFileName = Guid.NewGuid().ToString();
        var squaresFileName = $"{newFileName}_squares.json";
        var notesFileName = $"{newFileName}_notes.json";

        await File.WriteAllTextAsync($"wwwroot/uploads/presets/{squaresFileName}", data.SquaresJson);
        await File.WriteAllTextAsync($"wwwroot/uploads/presets/{notesFileName}", data.NotesJson);
        await bingoHub.SendPresetToPlayer(data.ToPlayerId, fromPlayer.NickName, data.GameName, data.PresetName, squaresFileName, notesFileName);
        return "success";
    }
    return "error";
});

app.MapPost("/game/submitResult", async (GameResult data) =>
{
    if (!GamesResults.Any(i => i.RoomId == data.RoomId))
    {
        GamesResults.Add(data);
        var json = JsonConvert.SerializeObject(GamesResults, Formatting.None);
        await File.WriteAllTextAsync("wwwroot/uploads/results/gamesResults.json", json);
    }
    return "success";

});

app.MapGet("/app/lastversion", () =>
{
    var files = Directory.GetFiles("wwwroot/uploads/apps", "*.zip");
    var link = "";
    var version = "";
    var updatelink = "";
    foreach (var item in files)
    {
        if (item.Contains("BingoApp"))
        {
            var fileName = Path.GetFileNameWithoutExtension(item);
            var arr = fileName.Split("_");
            version = arr[1];
            link = item.Replace("wwwroot", "");
            updatelink = $"/uploads/updates/{version}.zip";
        }
    }
    var jobj = new JObject();
    jobj["version"] = version;
    jobj["link"] = link;
    jobj["updatelink"] = updatelink;

    return jobj.ToString();
});

app.MapGet("/app/mod/lastversion", () =>
{
    var files = Directory.GetFiles("wwwroot/uploads/apps", "*.zip");
    var link = "";
    var version = "";
    var fileBingoMod = files.FirstOrDefault(i => i.Contains("BingoMod"));
    if (fileBingoMod != null)
    {
        var fileName = Path.GetFileNameWithoutExtension(fileBingoMod);
        var arr = fileName.Split("_");
        version = arr[1];
        link = fileBingoMod.Replace("wwwroot", "");
    }
    var jobj = new JObject();
    jobj["version"] = version;
    jobj["link"] = link;

    return jobj.ToString();
});

app.MapGet("/news", async () =>
{
    try
    {
        var newsFiles = Directory.GetFiles("wwwroot/uploads/news", "*.json");
        var news = new List<NewsItem>();
        foreach (var item in newsFiles)
        {
            try
            {
                var newsjson = await System.IO.File.ReadAllTextAsync(item, System.Text.Encoding.UTF8);
                var newsItem = JsonConvert.DeserializeObject<NewsItem>(newsjson);
                if (newsItem != null)
                {
                    newsItem.Content = "";
                    newsItem.FileName = Path.GetFileNameWithoutExtension(item);
                    news.Add(newsItem);
                }

            }
            catch (Exception)
            {
            }

        }
        var json = JsonConvert.SerializeObject(news, Formatting.None);
        return json;
    }
    catch (Exception ex)
    {
        return "[]";
    }
});

app.MapGet("/news/detail/{newsId}", async (string newsId) =>
{
    try
    {
        var json = await System.IO.File.ReadAllTextAsync(Path.Combine("wwwroot/uploads/news", $"{newsId}.json"));
        var newsItem = JsonConvert.DeserializeObject<NewsItem>(json);
        if (newsItem != null)
        {
            newsItem.FileName = newsId;
            json = JsonConvert.SerializeObject(newsItem, Formatting.None);
            return json;
        }
    }
    catch (Exception)
    {
    }
    return "{}";
});

#endregion

#region HuntpointServer

app.MapHub<HuntpointHub>("/huntpointHub");

app.MapPost("/huntpoint/ping/{playerId}", (string playerId) =>
{
    var player = HPPlayers.FirstOrDefault(i => i.Id == playerId);
    if (player != null)
    {
        player.IsOnline = true;
        player.LastActionTime = DateTime.Now;
    }
    return "pong";
});

app.MapPost("/huntpoint/players/signup", async (SocketServer.Models.Player player) =>
{
    var existPlayer = HPPlayers.FirstOrDefault(i => i.Id == player.Id);
    if (existPlayer != null)
    {
        existPlayer.IsOnline = true;
        existPlayer.IsAvailable = true;
        existPlayer.NickName = player.NickName;
        //existPlayer.AvatarBase64 = player.AvatarBase64;        
        existPlayer.LastActionTime = DateTime.Now;
    }
    else
    {
        player.IsOnline = true;
        player.IsAvailable = true;
        player.LastActionTime = DateTime.Now;
        HPPlayers.Add(player);
    }
    var json = JsonConvert.SerializeObject(HPPlayers, Formatting.None);
    await File.WriteAllTextAsync("hpplayers.json", json);
    return "success";
});

app.MapGet("/huntpoint/players/info/{playerId}", (string playerId) =>
{
    var player = HPPlayers.FirstOrDefault(i => i.Id == playerId);
    if (player != null)
    {
        return new SocketServer.Models.Player() { Id = player.Id, IsOnline = player.IsOnline, NickName = player.NickName, IsAvailable = player.IsAvailable };
    }
    return null;
});

app.MapGet("/huntpoint/players/available/{playerId}", (string playerId) =>
{
    var player = HPPlayers.FirstOrDefault(i => i.Id == playerId);
    if (player != null)
    {
        player.IsOnline = true;
        player.LastActionTime = DateTime.Now;
    }

    return HPPlayers.Where(i => i.IsOnline && i.Id != playerId);
});

app.MapGet("/huntpoint/players/all/{playerId}", (string playerId) =>
{
    var player = HPPlayers.FirstOrDefault(i => i.Id == playerId);
    if (player != null)
    {
        player.IsOnline = true;
        player.LastActionTime = DateTime.Now;
    }

    return HPPlayers.Where(i => i.Id != playerId).OrderByDescending(i => i.LastActionTime);
});

app.MapPost("/huntpoint/room/create", (SocketServer.Models.Huntpoint.NewRoomModel newroomModel) =>
{
    var room = new SocketServer.Models.Huntpoint.Room();
    try
    {
        room.RoomName = newroomModel.RoomName;
        room.CreaionDate = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(newroomModel.Password))
            room.RoomPassword = Convert.ToBase64String(Encoding.UTF8.GetBytes(newroomModel.Password));

        room.RoomSettings = new SocketServer.Models.Huntpoint.RoomSettings()
        {
            ExtraGameMode = newroomModel.RoomExtraMode,
            GameName = newroomModel.GameName,
            PresetName = newroomModel.PresetName,
            HideCard = newroomModel.HideCard,
            IsAutoBoardReveal = newroomModel.IsAutoBoardReveal,
            IsSameSeedGeneration = newroomModel.IsGenerateSameSeeds,
            IsLegendaryExtraPoints = newroomModel.IsLegendaryExtraPoints,
            IsForFirstDeath = newroomModel.IsForFirstDeath
        };
        var player = HPPlayers.FirstOrDefault(i => i.Id == newroomModel.CreatorId);
        if (player != null)
        {
            var newroomPlayer = new SocketServer.Models.Huntpoint.Player()
            {
                Id = player.Id,
                NickName = player.NickName,
                Color = SocketServer.Models.Huntpoint.HuntpointColor.red,
                IsSpectator = newroomModel.AsSpectator,
                Score = 0
            };
            room.Players.Add(newroomPlayer);
            room.CreatorId = newroomModel.CreatorId;
            room.CreatorName = player.NickName;
        }
        room.PresetJson = newroomModel.BoardJSON;
        room.GenerateBoardFromJson(newroomModel.BoardJSON, newroomModel.LegendaryObjective);
    }
    catch (Exception ex)
    {
        return JsonConvert.SerializeObject(new { error = "Invalid preset JSON format.", message = ex.Message });
    }
    HPRooms.Add(room);
    return JsonConvert.SerializeObject(room);
});

app.MapGet("/huntpoint/room/info/{roomId}", (string roomId) =>  
{
    var room = HPRooms.FirstOrDefault(i => i.RoomId == roomId);
    if (room != null)
    {
        return JsonConvert.SerializeObject(new
        {
            room.RoomId,
            room.RoomName,
            room.RoomSettings.GameName,
            room.RoomSettings.PresetName,
            room.CreatorName,
            room.RoomSettings.ExtraGameMode,
            room.LegendaryObjective,
            room.RoomSettings.IsLegendaryExtraPoints,
            room.RoomSettings.IsForFirstDeath,
            IsGenerateSameSeeds = room.RoomSettings.IsSameSeedGeneration
        });
    }
    else
        return "notfound";
});

app.MapPost("/huntpoint/room/connect", (SocketServer.Models.Huntpoint.ConnectRoomModel data) =>
{
    var room = HPRooms.FirstOrDefault(i => i.RoomId == data.RoomId);
    if (room != null)
    {
        if (!string.IsNullOrEmpty(room.RoomPassword))
        {
            if (string.IsNullOrEmpty(data.Password) || Convert.ToBase64String(Encoding.UTF8.GetBytes(data.Password)) != room.RoomPassword)
            {
                return "wrongpassword";
            }
        }

        var player = HPPlayers.FirstOrDefault(i => i.Id == data.PlayerId);
        if (player != null)
        {
            if (!room.Players.Any(i => i.Id == player.Id))
            {
                var newroomPlayer = new SocketServer.Models.Huntpoint.Player()
                {
                    Id = player.Id,
                    NickName = player.NickName,
                    Color = SocketServer.Models.Huntpoint.HuntpointColor.red,
                    IsSpectator = data.AsSpectator,
                    Score = 0
                };

                room.Players.Add(newroomPlayer);
            }
        }
        return JsonConvert.SerializeObject(room);
    }
    else
        return "notfound";
});

app.MapPost("/huntpoint/room/newboard/{roomId}", async (string roomId) =>
{
    var room = HPRooms.FirstOrDefault(i => i.RoomId == roomId);
    if (room != null)
    {
        room.GenerateBoardFromJson(room.PresetJson, room.LegendaryObjective);
        await huntpointHub.SendNewRoomBoard(room);
        return "success";
    }
    else
        return "notfound";
});

app.MapPost("/huntpoint/game/invite", async (GameInviteData data) =>
{
    await huntpointHub.SendGameInvite(data.PlayerId, data.InvitePlayerId, data.Data);
    return "success";
});

app.MapPost("/huntpoint/preset/share", async (SendPresetModel data) =>
{
    var fromPlayer = HPPlayers.FirstOrDefault(i => i.Id == data.FromPlayerId);
    if (fromPlayer != null)
    {
        var newFileName = Guid.NewGuid().ToString();
        var squaresFileName = $"{newFileName}_squares.json";
        var notesFileName = $"{newFileName}_notes.json";

        await File.WriteAllTextAsync($"wwwroot/uploads/presets/{squaresFileName}", data.SquaresJson);
        await File.WriteAllTextAsync($"wwwroot/uploads/presets/{notesFileName}", data.NotesJson);
        await huntpointHub.SendPresetToPlayer(data.ToPlayerId, fromPlayer.NickName, data.GameName, data.PresetName, squaresFileName, notesFileName);
        return "success";
    }
    return "error";
});

app.MapPost("/huntpoint/game/submitResult", async (SocketServer.Models.Huntpoint.GameResult data) =>
{
    if (!HPGamesResults.Any(i => i.RoomId == data.RoomId))
    {
        HPGamesResults.Add(data);
        var json = JsonConvert.SerializeObject(HPGamesResults, Formatting.None);
        await File.WriteAllTextAsync("wwwroot/uploads/results/hpgamesResults.json", json);
    }
    return "success";

});

app.MapGet("/huntpoint/app/lastversion", () =>
{
    var files = Directory.GetFiles("wwwroot/uploads/apps", "*.zip");
    var link = "";
    var version = "";
    var updatelink = "";
    foreach (var item in files)
    {
        if (item.Contains("HuntpointApp"))
        {
            var fileName = Path.GetFileNameWithoutExtension(item);
            var arr = fileName.Split("_");
            version = arr[1];
            link = item.Replace("wwwroot", "");
            updatelink = $"/uploads/updates/{version}.zip";
        }
    }

    var jobj = new JObject();
    jobj["version"] = version;
    jobj["link"] = link;
    jobj["updatelink"] = updatelink;

    return jobj.ToString();
});

app.MapGet("/huntpoint/test/roomcreate", () =>
{
    var room = new SocketServer.Models.Huntpoint.Room();
    try
    {
        room.RoomName = "Test";
        room.CreaionDate = DateTime.UtcNow;

        room.RoomSettings = new SocketServer.Models.Huntpoint.RoomSettings()
        {
            ExtraGameMode = SocketServer.Models.Huntpoint.ExtraGameMode.None,
            GameName = "Elden Ring Nightreign",
            PresetName = "Test",
            HideCard = true,
            IsAutoBoardReveal = true,
            IsSameSeedGeneration = true
        };
        var json = File.ReadAllText("testPreset.json");
        room.PresetJson = json;
        room.GenerateBoardFromJson(json, null);//"Победить Гладиуса, Зверя Ночи (Трицефал)");
    }
    catch (Exception ex)
    {
        return JsonConvert.SerializeObject(new { error = "Invalid preset JSON format.", message = ex.Message });
    }
    HPRooms.Add(room);
    return JsonConvert.SerializeObject(room);
});

app.MapGet("/huntpoint/mod/lastversion", () =>
{
    var files = Directory.GetFiles("wwwroot/uploads/apps", "*.zip");
    var link = "";
    var version = "";
    var fileBingoMod = files.FirstOrDefault(i => i.Contains("HuntpointMod"));
    if (fileBingoMod != null)
    {
        var fileName = Path.GetFileNameWithoutExtension(fileBingoMod);
        var arr = fileName.Split("_");
        version = arr[1];
        link = fileBingoMod.Replace("wwwroot", "");
    }
    var jobj = new JObject();
    jobj["version"] = version;
    jobj["link"] = link;

    return jobj.ToString();
});

#endregion

#region WheelServer

app.MapHub<WheelHub>("/wheelHub");

app.MapPost("/wheel/ping/{playerId}", (string playerId) =>
{
    var player = WPlayers.FirstOrDefault(i => i.Id == playerId);
    if (player != null)
    {
        player.IsOnline = true;
        player.LastActionTime = DateTime.Now;
    }
    return "pong";
});

app.MapPost("/wheel/players/signup", async (SocketServer.Models.Player player) =>
{
    var existPlayer = WPlayers.FirstOrDefault(i => i.Id == player.Id);
    if (existPlayer != null)
    {
        existPlayer.IsOnline = true;
        existPlayer.IsAvailable = true;
        existPlayer.NickName = player.NickName;
        //existPlayer.AvatarBase64 = player.AvatarBase64;        
        existPlayer.LastActionTime = DateTime.Now;
    }
    else
    {
        player.IsOnline = true;
        player.IsAvailable = true;
        player.LastActionTime = DateTime.Now;
        WPlayers.Add(player);
    }
    var json = JsonConvert.SerializeObject(WPlayers, Formatting.None);
    await File.WriteAllTextAsync("wplayers.json", json);
    return "success";
});

app.MapGet("/wheel/players/info/{playerId}", (string playerId) =>
{
    var player = WPlayers.FirstOrDefault(i => i.Id == playerId);
    if (player != null)
    {
        return new SocketServer.Models.Player() { Id = player.Id, IsOnline = player.IsOnline, NickName = player.NickName, IsAvailable = player.IsAvailable };
    }
    return null;
});

app.MapGet("/wheel/players/available/{playerId}", (string playerId) =>
{
    var player = WPlayers.FirstOrDefault(i => i.Id == playerId);
    if (player != null)
    {
        player.IsOnline = true;
        player.LastActionTime = DateTime.Now;
    }

    return WPlayers.Where(i => i.IsOnline && i.Id != playerId);
});

app.MapGet("/wheel/players/all/{playerId}", (string playerId) =>
{
    var player = WPlayers.FirstOrDefault(i => i.Id == playerId);
    if (player != null)
    {
        player.IsOnline = true;
        player.LastActionTime = DateTime.Now;
    }

    return WPlayers.Where(i => i.Id != playerId).OrderByDescending(i => i.LastActionTime);
});

app.MapPost("/wheel/room/create", (SocketServer.Models.Wheel.NewRoomModel newroomModel) =>
{
    var room = new SocketServer.Models.Wheel.Room();
    try
    {
        room.RoomName = newroomModel.RoomName;
        room.CreaionDate = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(newroomModel.Password))
            room.RoomPassword = Convert.ToBase64String(Encoding.UTF8.GetBytes(newroomModel.Password));

        room.RoomSettings = new SocketServer.Models.Wheel.RoomSettings()
        {
            TimerSeconds = newroomModel.TimerSeconds,
        };
        var player = WPlayers.FirstOrDefault(i => i.Id == newroomModel.CreatorId);
        if (player != null)
        {
            var newroomPlayer = new SocketServer.Models.Wheel.Player()
            {
                Id = player.Id,
                NickName = player.NickName,
                Score = 0,
                PlayerIndex = 1
            };
            room.Players.Add(newroomPlayer);
            room.CreatorId = newroomModel.CreatorId;
            room.CreatorName = player.NickName;
        }
        room.PresetJson = newroomModel.BoardJSON;
        room.DebufsJson = newroomModel.DebufsJSON;
        room.SetRandomDebuff();
        room.GenerateWheelsFromJson(newroomModel.BoardJSON);
    }
    catch (Exception ex)
    {
        return JsonConvert.SerializeObject(new { error = "Invalid preset JSON format.", message = ex.Message });
    }
    WRooms.Add(room);
    return JsonConvert.SerializeObject(room);
});

app.MapGet("/wheel/room/info/{roomId}", (string roomId) =>
{
    var room = WRooms.FirstOrDefault(i => i.RoomId == roomId);
    if (room != null)
    {
        return JsonConvert.SerializeObject(new
        {
            room.RoomId,
            room.RoomName,
            room.RoomSettings.TimerSeconds,
            room.CreatorName
        });
    }
    else
        return "notfound";
});

app.MapPost("/wheel/room/connect", (SocketServer.Models.Wheel.ConnectRoomModel data) =>
{
    var room = WRooms.FirstOrDefault(i => i.RoomId == data.RoomId);
    if (room != null)
    {
        if (!string.IsNullOrEmpty(room.RoomPassword))
        {
            if (string.IsNullOrEmpty(data.Password) || Convert.ToBase64String(Encoding.UTF8.GetBytes(data.Password)) != room.RoomPassword)
            {
                return "wrongpassword";
            }
        }

        var player = WPlayers.FirstOrDefault(i => i.Id == data.PlayerId);
        if (player != null)
        {
            if (!room.Players.Any(i => i.Id == player.Id))
            {
                var newroomPlayer = new SocketServer.Models.Wheel.Player()
                {
                    Id = player.Id,
                    NickName = player.NickName,
                    Score = 0,
                    PlayerIndex = room.Players.Count + 1
                };

                room.Players.Add(newroomPlayer);
            }
        }
        return JsonConvert.SerializeObject(room);
    }
    else
        return "notfound";
});

#endregion

//app.UseStatusCodePages();
app.UseStaticFiles();

//app.UseMvcWithDefaultRoute();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseErrorPages("404.html");

await app.RunAsync();



public class GameInviteData
{
    public string PlayerId { get; set; }
    public string InvitePlayerId { get; set; }

    public string Data { get; set; }
}

public class RoomTimeSettings
{
    public string RoomId { get; set; }

    public string Settings { get; set; }
}

public class SendPresetModel
{
    public string FromPlayerId { get; set; } = default!;
    public string ToPlayerId { get; set; } = default!;
    public string GameName { get; set; } = default!;

    public string PresetName { get; set; } = default!;

    public string SquaresJson { get; set; } = default!;

    public string NotesJson { get; set; } = default!;
}