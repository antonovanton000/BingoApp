using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WheelGame.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WheelGame.ViewModels
{
    public partial class FirstLaunchViewModel : MyBaseViewModel
    {
        [RelayCommand]
        async Task Appearing()
        {            
            MainWindow.HideSettingsButton();
        }
        

        [ObservableProperty]
        string? nickName;


        [RelayCommand]
        async Task Continue()
        {
            var playerId = Guid.NewGuid().ToString();
            WheelGame.Properties.Settings.Default.PlayerId = playerId;
            WheelGame.Properties.Settings.Default.NickName = NickName;            
            WheelGame.Properties.Settings.Default.IsFirstOpen = false;            
            WheelGame.Properties.Settings.Default.Save();
            
            var frame = (App.Current.MainWindow as MainWindow).frame;
            frame.Source = new Uri($"/Views/MainPage.xaml", UriKind.Relative);
            (App.Current.MainWindow as MainWindow).NeedClean = true;
            MainWindow.ClearHistory();
        }        
    }
}
