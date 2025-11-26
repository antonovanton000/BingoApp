using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace HuntpointApp.Models;

public partial class Objective : ObservableObject
{
    public Objective()
    {
        this.ObjectiveColors = new ObservableCollection<HuntpointColor>();
        this.ObjectiveColors.CollectionChanged += Colors_CollectionChanged;
    }

    private void Colors_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasOnlyOneColor));
        OnPropertyChanged(nameof(IsMarked));
        OnPropertyChanged(nameof(ObjectiveColorsSorted));
        OnPropertyChanged(nameof(FirstColor));
    }

    [ObservableProperty]
    string slot;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMarked))]
    [NotifyPropertyChangedFor(nameof(IsPlayerScoreVisible))]
    [NotifyPropertyChangedFor(nameof(IsGoalVisible))]
    [NotifyPropertyChangedFor(nameof(ObjectiveColorsSorted))]
    ObservableCollection<HuntpointColor> objectiveColors;
    
    public IEnumerable<HuntpointColor> ObjectiveColorsSorted => ObjectiveColors.OrderBy(i=> (int)i);

    public HuntpointColor FirstColor => (ObjectiveColors.Count > 0 ? ObjectiveColors.FirstOrDefault() : HuntpointColor.blank);

    [ObservableProperty]
    string name;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsGoalVisible))]
    bool isGoal;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPlayerScoreVisible))]
    int score = 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPlayerScoreVisible))]
    int playerScore = 0;

    [ObservableProperty]
    ObjectiveType type;

    [ObservableProperty]
    bool isMarking = false;
    
    [ObservableProperty]
    string notes;

    public bool IsPlayerScoreVisible => PlayerScore > 0;
    public bool IsGoalVisible => !IsMarked && IsGoal;

    public bool HasOnlyOneColor => ObjectiveColors.Where(i => i != HuntpointColor.blank).Count() == 1;

    public bool IsMarked { get => ObjectiveColors.Count > 0 && ObjectiveColors.Any(i => i != HuntpointColor.blank); }

    [ObservableProperty]
    bool isBingoAnimate;

    [ObservableProperty]
    bool isHidden = false;

    [ObservableProperty]
    bool isReplaceNewAnimate;

}
