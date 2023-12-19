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

        bool ContainsOnly(IEnumerable<Square> squares, BingoColor color)
        {
            var listOfColors = new List<BingoColor>() { color, BingoColor.blank};            
            foreach (var square in squares)
            {
                foreach (var item in square.SquareColors)
                {
                    if (!listOfColors.Contains(item)) return false;
                }
            }
            return true;
        }

        public string GetPotentialBingos(Player player)
        {
            foreach (var item in Squares)
            {
                item.IsPotentialBingo = false;
            }
            var res = new List<string>();

            for (int col = 0; col < 5; col++)
            {
                var csquares = Squares.Where(i => i.Column == col);
                if (csquares.Any(i => i.SquareColors.Contains(player.Color)))
                {
                    if (ContainsOnly(csquares, player.Color))
                    {
                        foreach (var item in csquares)
                        {
                            item.IsPotentialBingo = true;
                        }
                        res.Add($"Col {col + 1}");
                    }
                }    
            }

            for (int row = 0; row < 5; row++)
            {
                var rsquares = Squares.Where(i => i.Row == row);
                if (rsquares.Any(i => i.SquareColors.Contains(player.Color)))
                {
                    if (ContainsOnly(rsquares, player.Color))
                    {
                        foreach (var item in rsquares)
                        {
                            item.IsPotentialBingo = true;
                        }
                        res.Add($"Row {row + 1}");
                    }
                }
            }

            var dsquares = Squares.Where(i => i.Row == i.Column);
            if (dsquares.Any(i => i.SquareColors.Contains(player.Color)))
            {
                if (ContainsOnly(dsquares, player.Color))
                {
                    foreach (var item in dsquares)
                    {
                        item.IsPotentialBingo = true;
                    }
                    res.Add($"TL-RB");
                }
            }

            var bl_rt = new List<Square>();

            for (int col = 4; col >= 0; col--)
            {
                for (int row = 0; row < 5; row++)
                {
                    if (row == col)
                        bl_rt.Add(Squares.FirstOrDefault(i => i.Row == row && i.Column == 4 - col));
                }
            }

            if (bl_rt.Any(i => i.SquareColors.Contains(player.Color)))
            {
                if (ContainsOnly(bl_rt, player.Color))
                {
                    foreach (var item in bl_rt)
                    {
                        item.IsPotentialBingo = true;
                    }
                    res.Add($"BL-RT");
                }
            }

            return string.Join("\r\n", res);
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

            var rt_lb = new List<Square>();

            for (int col = 4; col >= 0; col--)
            {
                for (int row = 0; row < 5; row++)
                {
                    if (row == col)
                        rt_lb.Add(Squares.FirstOrDefault(i => i.Row == row && i.Column == 4 - col));
                }
            }

            if (rt_lb.Count(i => i.SquareColors.Any(j => j == color)) == 5)
                linesCount++;


            return linesCount;
        }
    }
}
