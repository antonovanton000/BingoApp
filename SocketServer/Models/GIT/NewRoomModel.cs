namespace SocketServer.Models.GIT;

public class NewRoomModel
{
    public string CreatorId { get; set; } = default!;
    
    public string GameName { get; set; } = default!;

    public string[] ItemsIds { get; set; } = default!;

}
