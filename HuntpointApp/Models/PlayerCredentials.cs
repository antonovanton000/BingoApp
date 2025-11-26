using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuntpointApp.Models
{
    public partial class PlayerCredentials : ObservableObject
    {
        [ObservableProperty]
        string nickName;

        [ObservableProperty]
        string password;

        [ObservableProperty]
        bool isSpectator;
    }
}
