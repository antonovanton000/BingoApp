using System.Text.Json.Serialization;

namespace SocketServer.Models
{
    public class Player
    {
        public string Id { get; set; } = null!;
        public string NickName { get; set; } = null!;
        
        public string AvatarBase64 { get; set; } = null!;

        [Newtonsoft.Json.JsonIgnore]
        public bool IsOnline { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public bool IsAvailable { get; set; }

        [JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string? ConnectionId { get; set; }
        
        public DateTime LastActionTime { get; set; }

        [JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string? ActiveRoomId { get; set; }
    }
}
