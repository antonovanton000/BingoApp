namespace SocketServer.Models.Bingo;

public class PlayerScore
{
    public string PlayerId { get; set; } = default!;

    public int Score { get; set; }

    public int LinesCount { get; set; }
}
