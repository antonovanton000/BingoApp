using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace BingoApp.Models
{
    public class Event
    {
        public EventSubType EventType { get; set; }

        public EventType Type { get; set; }

        public DateTime Timestamp { get; set; }

        public string Message { get; set; }

        public Player Player { get; set; }

        public Square Square { get; set; }

        public BingoColor Color { get; set; }

        public BingoColor PlayerColor { get; set; }

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
                    case BingoColor.orange:
                        return App.Current.FindResource("BingoColorOrrange") as LinearGradientBrush;
                    case BingoColor.red:
                        return App.Current.FindResource("BingoColorRed") as LinearGradientBrush;
                    case BingoColor.blue:
                        return App.Current.FindResource("BingoColorBlue") as LinearGradientBrush;
                    case BingoColor.green:
                        return App.Current.FindResource("BingoColorGreen") as LinearGradientBrush;
                    case BingoColor.purple:
                        return App.Current.FindResource("BingoColorPurple") as LinearGradientBrush;
                    case BingoColor.navy:
                        return App.Current.FindResource("BingoColorNavy") as LinearGradientBrush;
                    case BingoColor.teal:
                        return App.Current.FindResource("BingoColorTeal") as LinearGradientBrush;
                    case BingoColor.brown:
                        return App.Current.FindResource("BingoColorBrown") as LinearGradientBrush;
                    case BingoColor.pink:
                        return App.Current.FindResource("BingoColorPink") as LinearGradientBrush;
                    case BingoColor.yellow:
                        return App.Current.FindResource("BingoColorYellow") as LinearGradientBrush;
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
                    case BingoColor.orange:
                        return App.Current.FindResource("BingoColorOrrange") as LinearGradientBrush;
                    case BingoColor.red:
                        return App.Current.FindResource("BingoColorRed") as LinearGradientBrush;
                    case BingoColor.blue:
                        return App.Current.FindResource("BingoColorBlue") as LinearGradientBrush;
                    case BingoColor.green:
                        return App.Current.FindResource("BingoColorGreen") as LinearGradientBrush;
                    case BingoColor.purple:
                        return App.Current.FindResource("BingoColorPurple") as LinearGradientBrush;
                    case BingoColor.navy:
                        return App.Current.FindResource("BingoColorNavy") as LinearGradientBrush;
                    case BingoColor.teal:
                        return App.Current.FindResource("BingoColorTeal") as LinearGradientBrush;
                    case BingoColor.brown:
                        return App.Current.FindResource("BingoColorBrown") as LinearGradientBrush;
                    case BingoColor.pink:
                        return App.Current.FindResource("BingoColorPink") as LinearGradientBrush;
                    case BingoColor.yellow:
                        return App.Current.FindResource("BingoColorYellow") as LinearGradientBrush;
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
                        return PlayerColorBrush;
                    case Models.EventType.revealed:
                        return App.Current.FindResource("AccentColorBrush") as SolidColorBrush;
                    case Models.EventType.goal:
                        return PlayerColorBrush;
                    case Models.EventType.color:
                        return App.Current.FindResource("AccentColorBrush") as SolidColorBrush;
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
                            return $"{(Player.IsSpectator ? "Spectator" : "Player")} connected!";
                        else
                            return "Player dicconnected!";
                    case Models.EventType.chat:
                        return Player.NickName;                        
                    case Models.EventType.revealed:
                        return "Board revealed";                    
                    case Models.EventType.goal:
                        return Player.NickName;
                    case Models.EventType.color:
                        return Player.NickName;                    
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
                            return $"{(!Player.IsSpectator ? "Player " : "")}{Player.NickName} connected {(Player.IsSpectator ? "as spectator" : "")}";
                        else
                            return $"Player {Player.NickName} disconnected";
                    case Models.EventType.chat:
                        return Message;
                    case Models.EventType.revealed:
                        return $"Player {Player.NickName} revealed the board";
                    case Models.EventType.goal:
                        {
                            if (Remove)
                                return $"cleared";
                            else
                                return $"marked";
                        }
                    case Models.EventType.color:
                        return "changed color to";
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
        none
    }
}
