namespace SocketServer.Models.Huntpoint
{
    public class ConnectRoomModel
    {
        public string RoomId { get; set; } = default!;

        public string PlayerId { get; set; } = default!;

        public bool AsSpectator { get; set; } = false;

        public string? Password { get; set; } = null;
    }
}
