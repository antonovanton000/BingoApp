using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingoApp.Models
{
    public partial class Player : ObservableObject
    {
        [ObservableProperty]
        string id;
        
        [ObservableProperty]
        string nickName;
        
        [ObservableProperty]
        BingoColor color;
        
        [ObservableProperty]
        bool isSpectator;
        
        [ObservableProperty]
        int squaresCount;
        
        [ObservableProperty]
        int linesCount;

        [ObservableProperty]
        string potentialBingos;

        [ObservableProperty]
        int potentialBingosCount;

        [ObservableProperty]
        bool isAvailable;

        [ObservableProperty]
        bool isCurrentPlayer;

        [ObservableProperty]
        bool isBoardRevealed;

        [ObservableProperty]
        string? selectedBingoLine; 

    }
}
