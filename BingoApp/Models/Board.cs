using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingoApp.Models
{
    public partial class Board : ObservableObject
    {
        [ObservableProperty]
        ObservableCollection<Square> squares = new ObservableCollection<Square>();

        public Square GetSquare(string slotid)
        {
            return Squares.FirstOrDefault(i => i.Slot == slotid);
        }

        public int GetColorCount(BingoColor color)
        {
            return Squares.Count(i => i.SquareColors.Any(j => j == color));
        }

        public int GetLinesCount(BingoColor color)
        {
            var linesCount = 0;
            for (int col = 0; col < 5; col++)
            {
                if (Squares.Where(i => i.Column == col).Count(i => i.SquareColors.Any(j => j == color)) == 5)
                    linesCount++;
            }
            for (int row = 0; row < 5; row++)
            {
                if (Squares.Where(i => i.Row == row).Count(i => i.SquareColors.Any(j => j == color)) == 5)
                    linesCount++;
            }

            if (Squares.Where(i => i.Row == i.Column).Count(i => i.SquareColors.Any(j => j == color)) == 5)
                    linesCount++;

            var rt_tb = new List<Square>();

            for (int col = 4; col >= 0; col--)
            {
                for (int row = 0; row< 5; row++)
                {
                    if (row==col)
                        rt_tb.Add(Squares.FirstOrDefault(i => i.Row == row && i.Column == 4 - col));
                }                
            }

            if (rt_tb.Count(i => i.SquareColors.Any(j => j == color)) == 5)
                linesCount++;


            return linesCount;
        }
    }
}
