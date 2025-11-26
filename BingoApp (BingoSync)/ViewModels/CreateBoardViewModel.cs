using BingoApp.Models;
using BingoApp.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingoApp.ViewModels
{
    public partial class CreateBoardViewModel : MyBaseViewModel
    {

        
        [RelayCommand]
        async Task Appearing()
        {
            MainWindow.HideSettingsButton();
        }

    }
}
