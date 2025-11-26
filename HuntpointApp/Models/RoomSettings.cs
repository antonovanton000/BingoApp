using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuntpointApp.Models
{
    public class RoomSettings
    {
        public bool HideCard { get; set; }         
        public bool IsAutoBoardReveal { get; set; }
        public bool IsSameSeedGeneration { get; set; } = false;
        public string GameName { get; set; }
        public string PresetName { get; set; }
        public ExtraGameMode ExtraGameMode { get; set; } = ExtraGameMode.None;
        
        public bool IsLegendaryExtraPoints { get; set; }
        public bool IsForFirstDeath { get; set; }
    }
}
