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
        public string Variant { get; set; }
        public string LockoutMode { get; set; }
        public long Seed { get; set; }
        public string Game { get; set; }
        public int GameId { get; set; }
        public int VariantId { get; set; }
    }
}
