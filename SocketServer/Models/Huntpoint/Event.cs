namespace SocketServer.Models.Huntpoint;

public class Event
{
    public EventSubType EventType { get; set; }

    public EventType Type { get; set; }

    public DateTime Timestamp { get; set; }

    public string Message { get; set; }

    public Player Player { get; set; }

    public Objective Objective { get; set; }

    public HuntpointColor Color { get; set; }

    public HuntpointColor PlayerColor { get; set; }

    public bool Remove { get; set; }        
}

public enum EventSubType
{
    connected,
    disconnected,
}

public enum EventType
{
    connection,
    chat,
    revealed,
    goal,
    color,
    newcard,
    none,
    bingo,
    newsquare,
    unhudesquare,
    win,
    finish
}
