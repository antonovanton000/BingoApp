using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuntpointApp.Models
{
    public class SendPresetModel
    {
        public string FromPlayerId { get; set; } = default!;
        public string ToPlayerId { get; set; } = default!;
        public string GameName { get; set; } = default!;

        public string PresetName { get; set; } = default!;

        public string SquaresJson { get; set; } = default!;

        public string NotesJson { get; set; } = default!;
    }
}
