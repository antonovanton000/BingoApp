using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuntpointApp.Models
{
    public class PlayerTeam
    {
        public string Name { get; set; } = default!;
        public HuntpointColor Color { get; set; } = default!;
        public List<Player> Players { get; set; } = [];
        

    }
}
