using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuntpointApp.Models
{    
    public partial class RoomConnectionInfo : ObservableObject
    {
        [ObservableProperty]
        string roomId;
        
        [ObservableProperty]
        string roomName;
        
        [ObservableProperty]
        string creatorName;
                                       
    }
}
