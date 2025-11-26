
namespace SocketServer.Models.Wheel;

public class NewRoomModel
{
    public string CreatorId { get; set; } = default!;
    public string RoomName { get; set; } = default!;
    public string BoardJSON { get; set; } = default!; 
    public string DebufsJSON { get; set; } = default!;
    public string? Password { get; set; }    
    public int TimerSeconds { get; set; }

}
