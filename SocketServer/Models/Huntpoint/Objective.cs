using System.Collections.ObjectModel;

namespace SocketServer.Models.Huntpoint;

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

    public List<HuntpointColor> ObjectiveColors { get; set; } = new List<HuntpointColor>();

    public ObjectiveType Type { get; set; } = ObjectiveType.Regular;
}
