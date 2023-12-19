using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingoApp.Models
{
    public partial class Square : ObservableObject
    {

        public Square() 
        { 
            this.SquareColors = new ObservableCollection<BingoColor>();
            this.SquareColors.CollectionChanged += Colors_CollectionChanged;
        }

        private void Colors_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(HasOnlyOneColor));
        }

        [ObservableProperty]           
        string slot;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsMarked))]
        [NotifyPropertyChangedFor(nameof(IsScoreVisible))]
        [NotifyPropertyChangedFor(nameof(IsGoalVisible))]
        ObservableCollection<BingoColor> squareColors;

        [ObservableProperty]   
        string name;
        
        [ObservableProperty]   
        int row;
        
        [ObservableProperty]   
        int column;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsGoalVisible))]
        bool isGoal;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsScoreVisible))]
        int score = 0;

        [ObservableProperty]
        bool isMarking = false;

        [ObservableProperty]
        bool isPotentialBingo = false;

        public bool IsScoreVisible => !IsMarked && Score > 0;
        public bool IsGoalVisible => !IsMarked && IsGoal;

        public bool HasOnlyOneColor => SquareColors.Where(i => i != BingoColor.blank).Count() == 1;

        public bool IsMarked { get => SquareColors.Count > 0 && SquareColors.Any(i => i != BingoColor.blank); }
        
    }
}
