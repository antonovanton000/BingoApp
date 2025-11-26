using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuntpointApp.Models
{
    public enum StormState
    {
        Day1Start = 0, //04:30
        Day1Shrink1 = 1, //03:00
        Day1AfterShrink = 2, //03:30
        Day1Shrink2 = 3, //03:00
        BossFight1 = 4, //-----
        Day2Start = 5, //04:30
        Day2Shrink1 = 6, //03:00
        Day2AfterShrink = 7, //03:30
        Day2Shrink2 = 8, //03:00
        BossFight2 = 9 //-----
    }
}
