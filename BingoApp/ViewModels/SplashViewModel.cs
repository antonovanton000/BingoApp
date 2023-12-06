using BingoApp.Models;
using BingoApp.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingoApp.ViewModels
{
    public partial class SplashViewModel : MyBaseViewModel
    {
        [RelayCommand]
        async Task Appearing()
        {
            MainWindow.HideSettingsButton();
            await Task.Delay(1000);
            
            var isFirstLoaded = BingoApp.Properties.Settings.Default.IsFirstOpen;
            if (!isFirstLoaded)
            {
                var frame = (App.Current.MainWindow as MainWindow).frame;
                //frame.Source = new Uri($"/Views/FirstLaunchPage.xaml", UriKind.Relative);
                frame.Source = new Uri($"/Views/MainPage.xaml", UriKind.Relative);
                (App.Current.MainWindow as MainWindow).NeedClean = true;
                MainWindow.ClearHistory();
            }
            else
            {
                var frame = (App.Current.MainWindow as MainWindow).frame;
                frame.Source = new Uri($"/Views/FirstLaunchPage.xaml", UriKind.Relative);
                (App.Current.MainWindow as MainWindow).NeedClean = true;
                MainWindow.ClearHistory();
            }
        }
        
    }
}
