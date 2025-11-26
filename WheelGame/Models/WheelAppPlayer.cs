using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WheelGame.Models;

public class WheelAppPlayer
{
    public string Id { get; set; } = null!;
    public string NickName { get; set; } = null!;
    public string AvatarBase64 { get; set; } = null!;        
    public bool IsOnline { get; set; }
}
