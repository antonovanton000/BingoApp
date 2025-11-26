namespace SocketServer.Models.Bingo;

public class Board
{
    public Board() { }
    public List<Square> Squares { get; set; } = new List<Square>();

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
        var listOfColors = new List<BingoColor>() { color, BingoColor.blank };
        foreach (var square in squares)
        {
            foreach (var item in square.SquareColors)
            {
                if (!listOfColors.Contains(item)) return false;
            }
        }
        return true;
    }

    bool ContainsOrBlank(IEnumerable<Square> squares, BingoColor color)
    {
        var listOfColors = new List<BingoColor>() { color, BingoColor.blank };
        foreach (var square in squares)
        {
            foreach (var item in square.SquareColors)
            {
                if (!listOfColors.Contains(item)) return false;
            }
        }
        return true;
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
                {
                    var sq = Squares.FirstOrDefault(i => i.Row == row && i.Column == 4 - col);
                    if (sq != null)
                        rt_lb.Add(sq);
                }
            }
        }

        if (rt_lb.Count(i => i.SquareColors.Any(j => j == color)) == 5)
            linesCount++;


        return linesCount;
    }
    
    public int GetPotentialBingosCount(Player player)
    {
        var count = 0;
        for (int col = 0; col < 5; col++)
        {
            var csquares = Squares.Where(i => i.Column == col);
            if (ContainsOrBlank(csquares, player.Color))
            {
                count++;
            }

        }

        for (int row = 0; row < 5; row++)
        {
            var rsquares = Squares.Where(i => i.Row == row);
            if (ContainsOrBlank(rsquares, player.Color))
            {
                count++;
            }
        }

        var dsquares = Squares.Where(i => i.Row == i.Column);

        if (ContainsOrBlank(dsquares, player.Color))
        {
            count++;
        }


        var bl_rt = new List<Square>();
        for (int col = 4; col >= 0; col--)
        {
            for (int row = 0; row < 5; row++)
            {
                if (row == col)
                {
                    var square = Squares.FirstOrDefault(i => i.Row == row && i.Column == 4 - col);
                    if (square != null)
                        bl_rt.Add(square);
                }
            }
        }

        if (ContainsOrBlank(bl_rt, player.Color))
        {
            count++;
        }


        return count;
    }

}
