using HuntpointApp.Classes;
using HuntpointApp.Models;
using HuntpointApp.Views;
using ColorThiefDotNet;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace HuntpointApp.ViewModels
{
    public partial class BoardManagerViewModel : MyBaseViewModel
    {
        [ObservableProperty]
        Game game = new();

        [ObservableProperty]
        bool isModalOpen = false;

        [ObservableProperty]
        bool isPresetCreating = false;

        [ObservableProperty]
        BoardPreset newPreset = new();

        [ObservableProperty]
        List<System.Windows.Media.Color> gameColors = [];

        [RelayCommand]
        async Task Appearing()
        {
            MainWindow.HideSettingsButton();
            var imgCoverPath = Game.ImageCoverPath;
            if (!string.IsNullOrEmpty(imgCoverPath))
            {
                Bitmap? bMap = Bitmap.FromFile(imgCoverPath) as Bitmap;
                var colorThief = new ColorThief();
                var colors = colorThief.GetPalette(bMap, 6);
                GameColors = colors.Select(i => System.Windows.Media.Color.FromArgb(i.Color.A, i.Color.R, i.Color.G, i.Color.B)).ToList();                
            }            
        }

        [RelayCommand]
        void EditPreset(BoardPreset preset)
        {
            MainWindow.NavigateTo(new EditBoardPage() { DataContext = new EditBoardViewModel() { BoardPreset = preset, Game = Game } });
        }

        [RelayCommand]
        void DuplicatePreset(BoardPreset preset)
        {
            MainWindow.InputDialog(App.Current.Resources["mes_newpresetname"].ToString(), new Action<string>(async (s) =>
            {
                if (!string.IsNullOrEmpty(s))
                {
                    var directoryPath = System.IO.Path.Combine(App.Location, "Presets", Game.Name);
                    var newPath = System.IO.Path.Combine(directoryPath, s + ".json");
                    System.IO.File.Copy(preset.FilePath, newPath, true);
                    
                    await PresetCollection.RefreshGamePresets(Game);
                    MainWindow.ShowToast(new ToastInfo() { 
                        Title = App.Current.Resources["mes_success"].ToString(), 
                        Detail = string.Format(App.Current.Resources["mes_0presetcreated"].ToString(), s) 
                    });
                }
            }), preset.PresetName + $" - {App.Current.Resources["mes_copy"]}", App.Current.Resources["mes_typenewpresetna"].ToString());
        }

        [RelayCommand]
        void DeletePreset(BoardPreset preset)
        {
            MainWindow.ShowMessage(string.Format(App.Current.Resources["mes_realywantdelete"].ToString(), preset.PresetName), MessageNotificationType.YesNo,
                new Action(async () =>
                {
                    System.IO.File.Delete(preset.FilePath);
                    Game.Presets.Remove(preset);
                }
            ));
        }

        [RelayCommand]
        void DeleteGame()
        {
            MainWindow.ShowMessage(string.Format(App.Current.Resources["mes_realywantdeletegame"].ToString(), Game.Name), MessageNotificationType.YesNo,
                new Action(async () =>
                {                    
                    System.IO.Directory.Delete(System.IO.Path.Combine(App.Location, "Presets", Game.Name), true);
                    MainWindow.GoBack();
                    await Task.Delay(1000);
                    if (!string.IsNullOrEmpty(Game.ImageCoverPath))
                    {
                        if (System.IO.File.Exists(Game.ImageCoverPath))
                        {
                            System.IO.File.Delete(Game.ImageCoverPath);
                        }
                    }
                }
            ));
        }

        [RelayCommand]
        void CreateNewPreset()
        {
            NewPreset = new BoardPreset();
            IsPresetCreating = false;
            IsModalOpen = true;
        }

        [RelayCommand]
        void CopyJSON(BoardPreset preset)
        {
            Clipboard.SetText(preset.Json);
            MainWindow.ShowToast(new ToastInfo() { 
                Title = App.Current.Resources["mes_success"].ToString(), 
                Detail = App.Current.Resources["mes_jsoncopiedsucce"].ToString(), 
                ToastType = ToastType.Success 
            });

        }

        [RelayCommand]
        async Task CreatePresetFinally()
        {
            NewPreset.IsPresetNameError = false;
            NewPreset.IsJsonEmpty = false;
            NewPreset.IsJsonError = false;            

            if (string.IsNullOrEmpty(NewPreset.PresetName))
            {
                NewPreset.IsPresetNameError = true;
                return;
            }

            if (string.IsNullOrEmpty(NewPreset.Json))
            {
                NewPreset.Json = "[]";
            }

            try
            {
                var jobj = JArray.Parse(NewPreset.Json);
            }
            catch (Exception)
            {
                NewPreset.IsJsonError = true;
                return;
            }            

            var presetFilePath = System.IO.Path.Combine(App.Location, "Presets", Game.Name, NewPreset.PresetName + ".json");
            await System.IO.File.WriteAllTextAsync(presetFilePath, NewPreset.Json);
            await PresetCollection.RefreshGamePresets(Game);
            IsModalOpen = false;
            MainWindow.ShowToast(new ToastInfo() { 
                Title = App.Current.Resources["mes_success"].ToString(), 
                Detail = App.Current.Resources["mes_presetcreatesuccess"].ToString(), 
                ToastType = ToastType.Success });

        }
    }
}
