namespace HuntpointApp.Models;

[Flags]
public enum HuntpointColor
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
    public static HuntpointColor Add(this HuntpointColor me, HuntpointColor toAdd)
    {
        return me | toAdd;
    }
}

