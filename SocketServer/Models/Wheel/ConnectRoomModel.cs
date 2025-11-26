namespace SocketServer.Models.Wheel
{
    public class ConnectRoomModel
    {
        public string RoomId { get; set; } = default!;

        public string PlayerId { get; set; } = default!;
        
        public string? Password { get; set; } = null;
    }
}
