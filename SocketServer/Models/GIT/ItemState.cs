namespace SocketServer.Models.GIT;

public class ItemState
{
    public string ItemId { get; set; } = default!;

    public List<string> SelectByPlayers { get; set; } = new List<string>();

}
