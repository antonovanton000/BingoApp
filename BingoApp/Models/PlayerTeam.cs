using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingoApp.Models
{
    public class PlayerTeam
    {
        public string Name { get; set; } = default!;
        public BingoColor Color { get; set; } = default!;
        public List<Player> Players { get; set; } = [];
        

    }
}
