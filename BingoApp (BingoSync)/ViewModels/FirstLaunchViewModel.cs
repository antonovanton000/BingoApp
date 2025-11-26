using BingoApp.Models;
using BingoApp.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using static BingoApp.ViewModels.SettingsViewModel;

namespace BingoApp.ViewModels
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
        BingoColor bingoColor;

        [ObservableProperty]
        string? nickName;

        AppLanguage curLanguage = null!;
        public AppLanguage CurLanguage { get => curLanguage; set { curLanguage = value; OnPropertyChanged(); LanguageChanged(); } }

        [ObservableProperty]
        List<AppLanguage> languages = new() {
            new AppLanguage() { LanguageName = "English", Culture = new CultureInfo("en-US")},
            new AppLanguage() { LanguageName = "Русский", Culture = new CultureInfo("ru-RU")}
        };
        void LanguageChanged()
        {
            App.Language = CurLanguage.Culture;
        }

        [RelayCommand]
        async Task Continue()
        {
            var playerId = Guid.NewGuid().ToString();
            BingoApp.Properties.Settings.Default.PlayerId = playerId;
            BingoApp.Properties.Settings.Default.NickName = NickName;
            BingoApp.Properties.Settings.Default.PreferedColor = BingoColor.ToString();
            BingoApp.Properties.Settings.Default.IsFirstOpen = false;
            BingoApp.Properties.Settings.Default.AppLanguage = CurLanguage.Culture.Name;
            BingoApp.Properties.Settings.Default.Save();
            
            var frame = (App.Current.MainWindow as MainWindow).frame;
            frame.Source = new Uri($"/Views/MainPage.xaml", UriKind.Relative);
            (App.Current.MainWindow as MainWindow).NeedClean = true;
            MainWindow.ClearHistory();
        }

        [RelayCommand]
        void Skip()
        {
            var playerId = Guid.NewGuid().ToString();
            BingoApp.Properties.Settings.Default.IsFirstOpen = false;
            BingoApp.Properties.Settings.Default.PlayerId = playerId;
            BingoApp.Properties.Settings.Default.NickName = CurLanguage.LanguageName == "English" ? "Player" : "Игрок";
            BingoApp.Properties.Settings.Default.AppLanguage = CurLanguage.Culture.Name;
            BingoApp.Properties.Settings.Default.Save();

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
