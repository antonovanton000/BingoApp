using BingoApp.Models;
using BingoApp.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingoApp.ViewModels
{
    public partial class FirstLaunchViewModel : MyBaseViewModel
    {
        [RelayCommand]
        async Task Appearing()
        {
            MainWindow.HideSettingsButton();
        }

        [ObservableProperty]
        BingoColor bingoColor;

        [ObservableProperty]
        string? nickName;

        [RelayCommand]
        async Task Continue()
        {
            BingoApp.Properties.Settings.Default.NickName = NickName;
            BingoApp.Properties.Settings.Default.PreferedColor = BingoColor.ToString();
            BingoApp.Properties.Settings.Default.IsFirstOpen = false;
            BingoApp.Properties.Settings.Default.Save();

            var frame = (App.Current.MainWindow as MainWindow).frame;
            frame.Source = new Uri($"/Views/MainPage.xaml", UriKind.Relative);
            (App.Current.MainWindow as MainWindow).NeedClean = true;
            MainWindow.ClearHistory();
        }

        [RelayCommand]
        async Task Skip()
        {
            BingoApp.Properties.Settings.Default.IsFirstOpen = false;
            BingoApp.Properties.Settings.Default.Save();

            var frame = (App.Current.MainWindow as MainWindow).frame;
            frame.Source = new Uri($"/Views/MainPage.xaml", UriKind.Relative);
            (App.Current.MainWindow as MainWindow).NeedClean = true;
            MainWindow.ClearHistory();
        }
        
    }
}
