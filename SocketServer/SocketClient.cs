using System.Net.WebSockets;

namespace SocketServer
{
    public class SocketClient
    {
        public string UserId { get; set; }

        public string RoomId { get; set; }

        public WebSocket Socket { get; set; }
    }
}
