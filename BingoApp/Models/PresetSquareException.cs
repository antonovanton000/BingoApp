using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingoApp.Models
{
    public partial class PresetSquareException
    {
        public ObservableCollection<string> SquareNames { get; set; } = new ObservableCollection<string>();
    }
}
