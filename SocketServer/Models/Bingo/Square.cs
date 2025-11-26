using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace SocketServer.Models.Bingo;

public class Square
{

    public Square() { }
    public Square(string slot, string name)
    {
        Slot = slot;
        Name = name;            
    }

    public string Slot { get; set; } = default!;
    public string Name { get; set; } = default!;    
    public int Row { get; set; }    
    public int Column { get; set; }
    public bool IsHidden { get; set;} = false;
    public List<BingoColor> SquareColors { get; set; } = new List<BingoColor>();

    [JsonIgnore]
    public bool IsMarked { get => SquareColors.Count > 0 && SquareColors.Any(i => i != BingoColor.blank); }
    
}
