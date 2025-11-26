using Newtonsoft.Json;
using System;
using System.Net.WebSockets;
using System.Text.RegularExpressions;

namespace SocketServer.Models.GIT;

public class Room
{
    public Room()
    {
        RoomId = Guid.NewGuid().ToString();
        RoomConnectNumber = GenerateRoomConnectNumber();
        CreationDate = DateTime.Now;
    }

    public string CreatorId { get; set; } = default!;
    
    public DateTime CreationDate { get; set; }

    public string GameName { get; set; } = default!;

    public string CreatorName { get; set; } = default!;

    public string RoomId { get; set; } = default!;

    public string RoomConnectNumber { get; set; }

    public List<Player> Players { get; set; } = new();

    public List<string> ItemsIds { get; set; } = new();    
    public List<ItemState> ItemsStates { get; set; } = new();

    public static string GenerateRoomConnectNumber()
    {
        var number = System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, 1000000);
        return number.ToString("D6");
    }
}
