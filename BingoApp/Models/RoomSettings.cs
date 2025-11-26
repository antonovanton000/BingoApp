using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingoApp.Models
{
    public class RoomSettings
    {
        public bool HideCard { get; set; }
        public bool IsAutoBoardReveal { get; set; }
        public bool IsAutoFogWall { get; set; }
        public string GameName { get; set; }
        public string PresetName { get; set; }
        public int StartTimeSeconds { get; set; }
        public int AfterRevealSeconds { get; set; }
        public int UnhideTimeMinutes { get; set; }
        public int ChangeTimeMinutes { get; set; }
        public GameMode GameMode { get; set; } = GameMode.Lockout;
        public ExtraGameMode ExtraGameMode { get; set; } = ExtraGameMode.None;
        public bool IsTripleBingoSelect { get; set; }
    }
}
