using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace WheelGame.Models;

public partial class ObjectiveModifier : ObservableObject
{
    public ObjectiveModifier()
    {
        Players = new();
        Players.CollectionChanged += (s, e) => { OnPropertyChanged(nameof(PlayersOrdered)); };
    }

    public string Slot { get; set; } = default!;

    public string Name { get; set; } = default!;

    public int Points { get; set; } = 0;

    public List<string> PlayerIds { get; set; } = new();
    public ObservableCollection<Player> Players { get; set; } = new();
    public IEnumerable<Player> PlayersOrdered => Players.OrderBy(i => i.PlayerIndex);

    [ObservableProperty]
    bool isMarked;
}
