
using Newtonsoft.Json;

namespace SocketServer.Models.GIT;

public class Player
{

    public Player() { }

    public Player(string id, string nickName, string avatar)
    {
        Id = id;
        NickName = nickName;                               
        Picture = avatar;
    }

    [JsonIgnore]
    public string? ActiveRoomId { get; set; }
    public string Id { get; set; } = default!;
    public string NickName { get; set; } = default!;
    public string Picture { get; set; }

    [JsonIgnore]
    public string? ConnectionId { get; set; }

    [JsonIgnore]
    public bool IsOnline { get; set;  } = false;

    [JsonIgnore]
    public DateTime LastActionTime { get; set; }

    [JsonIgnore]
    public TimeSpan RunTime { get; set; }
}
