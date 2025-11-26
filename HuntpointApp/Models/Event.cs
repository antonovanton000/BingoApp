using Newtonsoft.Json;
using System.Windows.Media;

namespace HuntpointApp.Models;

public class Event
{
    public EventSubType EventType { get; set; }

    public EventType Type { get; set; }

    public DateTime Timestamp { get; set; }

    public string Message { get; set; }

    public Player Player { get; set; }

    public Objective Objective { get; set; }

    public HuntpointColor Color { get; set; }

    public HuntpointColor PlayerColor { get; set; }

    public bool Remove { get; set; }

    public string ColorName => Color.ToString();
    public string PlayerColorName => PlayerColor.ToString();

    public string ChangeColorName => Type == Models.EventType.color ? ColorName : "";

    [JsonIgnore]
    public Brush PlayerColorBrush
    {
        get
        {
            switch (PlayerColor)
            {
                case HuntpointColor.orange:
                    return App.Current.FindResource("HuntpointColorOrrange") as LinearGradientBrush;
                case HuntpointColor.red:
                    return App.Current.FindResource("HuntpointColorRed") as LinearGradientBrush;
                case HuntpointColor.blue:
                    return App.Current.FindResource("HuntpointColorBlue") as LinearGradientBrush;
                case HuntpointColor.green:
                    return App.Current.FindResource("HuntpointColorGreen") as LinearGradientBrush;
                case HuntpointColor.purple:
                    return App.Current.FindResource("HuntpointColorPurple") as LinearGradientBrush;
                case HuntpointColor.navy:
                    return App.Current.FindResource("HuntpointColorNavy") as LinearGradientBrush;
                case HuntpointColor.teal:
                    return App.Current.FindResource("HuntpointColorTeal") as LinearGradientBrush;
                case HuntpointColor.brown:
                    return App.Current.FindResource("HuntpointColorBrown") as LinearGradientBrush;
                case HuntpointColor.pink:
                    return App.Current.FindResource("HuntpointColorPink") as LinearGradientBrush;
                case HuntpointColor.yellow:
                    return App.Current.FindResource("HuntpointColorYellow") as LinearGradientBrush;
            }
            return null;
        }
    }

    [JsonIgnore]
    public Brush ColorBrush
    {
        get
        {
            switch (Color)
            {
                case HuntpointColor.orange:
                    return App.Current.FindResource("HuntpointColorOrrange") as LinearGradientBrush;
                case HuntpointColor.red:
                    return App.Current.FindResource("HuntpointColorRed") as LinearGradientBrush;
                case HuntpointColor.blue:
                    return App.Current.FindResource("HuntpointColorBlue") as LinearGradientBrush;
                case HuntpointColor.green:
                    return App.Current.FindResource("HuntpointColorGreen") as LinearGradientBrush;
                case HuntpointColor.purple:
                    return App.Current.FindResource("HuntpointColorPurple") as LinearGradientBrush;
                case HuntpointColor.navy:
                    return App.Current.FindResource("HuntpointColorNavy") as LinearGradientBrush;
                case HuntpointColor.teal:
                    return App.Current.FindResource("HuntpointColorTeal") as LinearGradientBrush;
                case HuntpointColor.brown:
                    return App.Current.FindResource("HuntpointColorBrown") as LinearGradientBrush;
                case HuntpointColor.pink:
                    return App.Current.FindResource("HuntpointColorPink") as LinearGradientBrush;
                case HuntpointColor.yellow:
                    return App.Current.FindResource("HuntpointColorYellow") as LinearGradientBrush;
            }
            return null;
        }
    }

    [JsonIgnore]
    public Brush MessageColor
    {
        get
        {
            switch (Type)
            {
                case Models.EventType.connection:
                    return App.Current.FindResource("AccentColorBrush") as SolidColorBrush;
                case Models.EventType.chat:
                    return (PlayerColorBrush ?? App.Current.FindResource("AccentColorBrush") as SolidColorBrush);
                case Models.EventType.revealed:
                    return App.Current.FindResource("AccentColorBrush") as SolidColorBrush;
                case Models.EventType.goal:
                    return PlayerColorBrush;
                case Models.EventType.Huntpoint:
                    return App.Current.FindResource("WinBrush") as LinearGradientBrush;
                case Models.EventType.win:
                    return App.Current.FindResource("WinBrush") as LinearGradientBrush;
                case Models.EventType.color:
                    return App.Current.FindResource("AccentColorBrush") as SolidColorBrush;
                case Models.EventType.newsquare:
                    return App.Current.FindResource("AccentColorBrush") as SolidColorBrush;
                case Models.EventType.unhudesquare:
                    return App.Current.FindResource("AccentColorBrush") as SolidColorBrush;
                case Models.EventType.newcard:
                    return App.Current.FindResource("AccentColorBrush") as SolidColorBrush;
                case Models.EventType.finish:
                    return PlayerColorBrush;
            }

            return null;
        }
    }

    public bool IsItalic
    {
        get
        {
            return Type == Models.EventType.connection || Type == Models.EventType.color ? true : false;
        }
    }

    public string MessageHeader
    {
        get
        {
            switch (Type)
            {
                case Models.EventType.connection:
                    if (EventType == EventSubType.connected)
                        return $"{(Player.IsSpectator ? App.Current.FindResource("mes_spectator").ToString() : App.Current.FindResource("mes_player").ToString())} {App.Current.FindResource("mes_connected")}";
                    else
                        return $"{App.Current.FindResource("mes_playerdisconnec")}";
                case Models.EventType.chat:
                    return Player.NickName;                        
                case Models.EventType.revealed:
                    return $"{App.Current.FindResource("mes_boardrevealed")}";                    
                case Models.EventType.goal:
                    return Player.NickName;
                case Models.EventType.color:
                    return Player.NickName;                    
                case Models.EventType.Huntpoint:
                    return $"{App.Current.FindResource("mes_Huntpoint")}";
                case Models.EventType.win:
                    return $"{App.Current.FindResource("mes_win")}";
                case Models.EventType.newsquare:
                    return $"{App.Current.FindResource("mes_squarechangedheader")}";
                case Models.EventType.unhudesquare:
                    return $"{App.Current.FindResource("mes_squareunhideheader")}";
                case Models.EventType.newcard:
                    return $"{App.Current.FindResource("mes_newboard")}";
                case Models.EventType.finish:
                    return $"{App.Current.FindResource("mes_playerfinish")}";
                default:
                    break;
            }
            return "";
        }
    }

    public string MessageText
    {
        get
        {
            switch (Type)
            {
                case Models.EventType.connection:
                    if (EventType == EventSubType.connected)
                        return $"{(!Player.IsSpectator ? $"{App.Current.FindResource("mes_player")} " : "")}{Player.NickName} {App.Current.FindResource("mes_connected")} {(Player.IsSpectator ? App.Current.FindResource("mes_asspectator") : "")}";
                    else
                        return $"{App.Current.FindResource("mes_player")} {Player.NickName} {App.Current.FindResource("mes_disconnected")}";
                case Models.EventType.chat:
                    return Message;
                case Models.EventType.revealed:
                    return $"{App.Current.FindResource("mes_player")} {Player.NickName} {App.Current.FindResource("mes_revealedtheboar")}";
                case Models.EventType.goal:
                    {
                        if (Remove)
                            return $"{App.Current.FindResource("mes_cleared")}";
                        else
                            return $"{App.Current.FindResource("mes_marked")}";
                    }
                case Models.EventType.color:
                    return $"{App.Current.FindResource("mes_changedcolorto")}";
                case Models.EventType.Huntpoint:
                    return $"{App.Current.FindResource("mes_player")} {Player.NickName} {App.Current.FindResource("mes_gotHuntpoint")}";
                case Models.EventType.win:
                    return $"{App.Current.FindResource("mes_player")} {Player.NickName} {App.Current.FindResource("mes_gotmajoritywin")}";
                case Models.EventType.newsquare:
                    return Message;
                case Models.EventType.unhudesquare:
                    return Message;
                case Models.EventType.newcard:
                    return $"{App.Current.FindResource("mes_player")} {Player.NickName} {App.Current.FindResource("mes_newboardtext")}";
                case Models.EventType.finish:
                    return $"{App.Current.FindResource("mes_player")} {Player.NickName} {App.Current.FindResource("mes_playerfinishtext")}";

            }
            return "";
        }
    }

}

public enum EventSubType
{
    connected,
    disconnected,
}

public enum EventType
{
    connection,
    chat,
    revealed,
    goal,
    color,
    newcard,
    none,
    Huntpoint,
    newsquare,
    unhudesquare,
    win,
    finish
}
