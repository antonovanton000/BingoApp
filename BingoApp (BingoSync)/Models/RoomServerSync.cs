using System;

namespace BingoApp.Models
{
    public class RoomServerSync
    {
        public string RoomId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsGameStarted { get; set; } = false;
        public bool IsGameEnded { get; set; } = false;
        public int CurrentTimerTime { get; set; }
        public bool IsAutoBoardReveal { get; set; }
        public GameMode GameMode { get; set; } = GameMode.Other;
        public ExtraGameMode GameExtraMode { get; set; }    
        public bool IsRevealed { get; set; } 
        public string PresetName { get; set; } = default!;
        public string GameName { get; set; } = default!;
        public bool IsHiddenGameInited { get; set; } = false;
        public int CurrentHiddenStep { get; set; } = 1;
        public int LastChangeMinute { get; set; } = 0;

    }
}
