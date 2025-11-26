using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HuntpointApp.Models;
using HuntpointApp.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuntpointApp.ViewModels
{
    public partial class WelcomeAboutAppViewModel : MyBaseViewModel
    {

        [ObservableProperty]
        bool isAppUpdateVisible = false;

        [ObservableProperty]
        bool isLaunchGameVisible = false;

        [RelayCommand]
        async Task Appearing()
        {
            IsAppUpdateVisible = false;
            IsLaunchGameVisible = !string.IsNullOrEmpty(Properties.Settings.Default.NightreignExePath);
            await CheckForUpdates();
        }

        [RelayCommand]
        void UpdateApp()
        {
            Process.Start("AppUpdaterHuntpoint.exe");
        }

        [RelayCommand]
        void UpdateAppCancel()
        {
            IsAppUpdateVisible = false;
        }

        [RelayCommand]
        void ShowMapSelector()
        {
            MainViewModel.ShowMapSelector();
        }


        [RelayCommand]
        void LaunchGame()
        {
            if (string.IsNullOrEmpty(Properties.Settings.Default.NightreignExePath))
                return;

            var nightreignFolderPath = System.IO.Path.GetDirectoryName(Properties.Settings.Default.NightreignExePath);
            if (string.IsNullOrEmpty(nightreignFolderPath))
                return;
            try
            {
                var me3Path = System.IO.Path.Combine(nightreignFolderPath, "nightreign-with-helper.me3");
                Process.Start(new ProcessStartInfo() { FileName = me3Path, UseShellExecute = true });
                //Process process = new Process();
                //ProcessStartInfo startInfo = new ProcessStartInfo();
                //startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                //startInfo.FileName = "cmd.exe";
                //startInfo.Arguments = $"/C me3 launch -p \"{me3Path}\" --auto-detect";
                //process.StartInfo = startInfo;
                //process.Start();                
            }
            catch (Exception ex)
            {
                if (Properties.Settings.Default.IsDebug)
                {
                    MainWindow.ShowErrorMessage(ex.Message, nameof(WelcomeAboutAppViewModel), nameof(LaunchGame));
                }
            }
        }

        private async Task CheckForUpdates()
        {
            try
            {
                if (!App.IsUpdateAppDialogShown)
                {
                    await Task.Delay(2000);
                    var resp = await App.RestClient.GetLastVersionAsync();
                    if (resp.IsSuccess)
                    {
                        var updateInfo = resp.Data as UpdateInfo;
                        if (updateInfo != null)
                        {
                            if (updateInfo.Version != App.AppVersion)
                            {
                                App.IsUpdateAppDialogShown = true;
                                IsAppUpdateVisible = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}
