using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer.Models.Bingo;

[Flags]
public enum BingoColor
{
    orange,
    red,
    blue,
    green,
    purple,
    navy,
    teal,
    brown,
    pink,
    yellow,
    blank     
}

public static class FlagExtensions
{
    public static BingoColor Add(this BingoColor me, BingoColor toAdd)
    {
        return me | toAdd;
    }
}
