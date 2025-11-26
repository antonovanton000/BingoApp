using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HuntpointApp.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace HuntpointApp.ViewModels
{
    public partial class FirstLaunchViewModel : MyBaseViewModel
    {
        [RelayCommand]
        async Task Appearing()
        {
            CurLanguage = Languages.First();
            MainWindow.HideSettingsButton();
        }

        [ObservableProperty]
        HuntpointColor huntpointColor;

        [ObservableProperty]
        string? nickName;

        AppLanguage curLanguage = null!;
        public AppLanguage CurLanguage { get => curLanguage; set { curLanguage = value; OnPropertyChanged(); LanguageChanged(); } }

        [ObservableProperty]
        List<AppLanguage> languages = new() {
            new AppLanguage() { LanguageName = "Русский", Culture = new CultureInfo("ru-RU")},
            new AppLanguage() { LanguageName = "English", Culture = new CultureInfo("en-US")}
        };
        void LanguageChanged()
        {
            App.Language = CurLanguage.Culture;
        }

        [RelayCommand]
        async Task Continue()
        {
            var playerId = Guid.NewGuid().ToString();
            HuntpointApp.Properties.Settings.Default.PlayerId = playerId;
            HuntpointApp.Properties.Settings.Default.NickName = NickName;
            HuntpointApp.Properties.Settings.Default.PreferedColor = HuntpointColor.ToString();
            HuntpointApp.Properties.Settings.Default.IsFirstOpen = false;
            HuntpointApp.Properties.Settings.Default.AppLanguage = CurLanguage.Culture.Name;
            HuntpointApp.Properties.Settings.Default.Save();
            
            var frame = (App.Current.MainWindow as MainWindow).frame;
            frame.Source = new Uri($"/Views/MainPage.xaml", UriKind.Relative);
            (App.Current.MainWindow as MainWindow).NeedClean = true;
            MainWindow.ClearHistory();
        }

        [RelayCommand]
        void Skip()
        {
            var playerId = Guid.NewGuid().ToString();
            HuntpointApp.Properties.Settings.Default.IsFirstOpen = false;
            HuntpointApp.Properties.Settings.Default.PlayerId = playerId;
            HuntpointApp.Properties.Settings.Default.NickName = CurLanguage.LanguageName == "English" ? "Player" : "Игрок";
            HuntpointApp.Properties.Settings.Default.AppLanguage = CurLanguage.Culture.Name;
            HuntpointApp.Properties.Settings.Default.Save();

            var frame = (App.Current.MainWindow as MainWindow)?.frame;
            if (frame != null)
            {
                frame.Source = new Uri($"/Views/MainPage.xaml", UriKind.Relative);
                var mainWindow = (App.Current.MainWindow as MainWindow);
                if (mainWindow != null)
                {
                    mainWindow.NeedClean = true;
                }
                MainWindow.ClearHistory();
            }            
        }
        
    }
}
