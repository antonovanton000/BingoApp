using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
namespace WheelGame.Models;

public partial class Objective : ObservableObject
{

    public Objective() { }
    public Objective(string slot, string name)
    {
        Slot = slot;
        Name = name;
        Players = new();
        Players.CollectionChanged += (s, e) => { OnPropertyChanged(nameof(PlayersOrdered)); };
    }

    public string Slot { get; set; } = default!;
    public string Name { get; set; } = default!;
    public int Score { get; set; } = 0;
    public List<ObjectiveModifier> Modifiers { get; set; } = new List<ObjectiveModifier>();
    public ObjectiveType Type { get; set; } = ObjectiveType.EarlyGame;
    public List<string> PlayerIds { get; set; } = new();
    public ObservableCollection<Player> Players { get; set; } = new();

    public IEnumerable<Player> PlayersOrdered => Players.OrderBy(i => i.PlayerIndex);

    public int FromAngle { get; set; } = 0;
    public int ToAngle { get; set; } = 0;

    [ObservableProperty]
    bool isMarked;

    [ObservableProperty]
    bool isCompleted;
    
}
