using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuntpointApp.Models;

public class HuntpointAppPlayer
{
    public string Id { get; set; } = null!;
    public string NickName { get; set; } = null!;
    public string AvatarBase64 { get; set; } = null!;        
    public bool IsOnline { get; set; }
}
