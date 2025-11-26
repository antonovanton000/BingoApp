using System.Collections.ObjectModel;
namespace SocketServer.Models.Wheel;

public class Objective
{

    public Objective() { }
    public Objective(string slot, string name)
    {
        Slot = slot;
        Name = name;            
    }

    public string Slot { get; set; } = default!;
    public string Name { get; set; } = default!;
    public int Score { get; set; } = 0;
    public List<ObjectiveModifier> Modifiers { get; set; } = new List<ObjectiveModifier>();
    public ObjectiveType Type { get; set; } = ObjectiveType.EarlyGame;
    public List<string> PlayerIds { get; set; } = new();

    public bool IsCompleted { get; set; } = false;
}
