using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingoApp.Models
{
    public class PlayerScore
    {
        public string PlayerId { get; set; } = default!;

        public int Score { get; set; }

        public int LinesCount { get; set; }
    }
}
