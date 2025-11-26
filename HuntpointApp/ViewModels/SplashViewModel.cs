using HuntpointApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuntpointApp.ViewModels;

public partial class SplashViewModel : MyBaseViewModel
{

    [ObservableProperty]
    string version;

    [RelayCommand]
    async Task Appearing()
    {
        MainWindow.MakeTransparent();
        Version = App.AppVersion;
        MainWindow.HideSettingsButton();
        await Task.Delay(2000);

        var isFirstLoaded = HuntpointApp.Properties.Settings.Default.IsFirstOpen;
        if (!isFirstLoaded)
        {
            if (string.IsNullOrEmpty(HuntpointApp.Properties.Settings.Default.PlayerId))
            {
                GoToFirstLaunch();
                return;
            }
            GoToMainPage();
        }
        else
        {
            GoToFirstLaunch();
        }
    }

    private void GoToMainPage()
    {
        var frame = (App.Current.MainWindow as MainWindow).frame;            
        MainWindow.RemoveTransparent();            
        frame.Source = new Uri($"/Views/MainPage.xaml", UriKind.Relative);
        (App.Current.MainWindow as MainWindow).NeedClean = true;
        MainWindow.ClearHistory();
    }

    private void GoToFirstLaunch()
    {
        var frame = (App.Current.MainWindow as MainWindow).frame;
        MainWindow.RemoveTransparent();
        frame.Source = new Uri($"/Views/FirstLaunchPage.xaml", UriKind.Relative);
        (App.Current.MainWindow as MainWindow).NeedClean = true;
        MainWindow.ClearHistory();
    }
    
}
