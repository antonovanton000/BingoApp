using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WheelGame.ViewModels
{
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

            var isFirstLoaded = WheelGame.Properties.Settings.Default.IsFirstOpen;
            if (!isFirstLoaded)
            {
                if (string.IsNullOrEmpty(WheelGame.Properties.Settings.Default.PlayerId))
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
}
