using BingoApp.Models;
using BingoApp.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BingoApp.ViewModels
{
    public partial class BoardManagerViewModel : MyBaseViewModel
    {
        [ObservableProperty]
        ObservableCollection<BoardPreset> presets = new ObservableCollection<BoardPreset>();

        [ObservableProperty]
        bool isModalOpen = false;

        [ObservableProperty]
        bool isPresetCreating = false;

        [ObservableProperty]
        BoardPreset newPreset;

        async Task LoadPresets()
        {
            var folderPath = System.IO.Path.Combine(App.Location, "GameJsons");
            var files = System.IO.Directory.GetFiles(folderPath, "*.json");
            Presets.Clear();
            foreach (var file in files)
            {
                try
                {
                    var json = await System.IO.File.ReadAllTextAsync(file);
                    var jarr = JArray.Parse(json);
                    var preset = new BoardPreset()
                    {
                        PresetName = System.IO.Path.GetFileNameWithoutExtension(file),
                        FilePath = file,
                        Json = json,
                        SquareCount = jarr.Count,
                    };
                    foreach (var item in jarr)
                    {
                        preset.Squares.Add(new PresetSquare() { Name = item["name"].Value<string>() });
                    }
                    Presets.Add(preset);
                }
                catch (Exception)
                {

                }

            }
        }

        [RelayCommand]
        async Task Appearing()
        {
            MainWindow.HideSettingsButton();
            await LoadPresets();
        }

        [RelayCommand]
        void EditPreset(BoardPreset preset)
        {
            MainWindow.NavigateTo(new EditBoardPage() { DataContext = new EditBoardViewModel() { BoardPreset = preset } });
        }

        [RelayCommand]
        void DuplicatePreset(BoardPreset preset)
        {
            MainWindow.InputDialog("New preset name", new Action<string>(async (s) =>
            {
                if (!string.IsNullOrEmpty(s))
                {
                    var directoryPath = System.IO.Path.GetDirectoryName(preset.FilePath);
                    var newPath = System.IO.Path.Combine(directoryPath, s + ".json");
                    System.IO.File.Copy(preset.FilePath, newPath, true);

                    var imgPath = System.IO.Path.Combine(App.Location, "PresetImages", preset.PresetName + ".jpg");
                    if (System.IO.File.Exists(imgPath))
                    {
                        directoryPath = System.IO.Path.GetDirectoryName(imgPath);
                        newPath = System.IO.Path.Combine(directoryPath, s + ".jpg");
                        System.IO.File.Copy(imgPath, newPath, true);
                    }

                    MainWindow.ShowToast(new ToastInfo() { Title = "Success", Detail = $"{s} preset created!" });
                    await LoadPresets();
                }
            }), preset.PresetName + " - Copy", "Type new preset name");
        }

        [RelayCommand]
        void DeletePreset(BoardPreset preset)
        {
            MainWindow.ShowMessage($"Do you realy want to delete \"{preset.PresetName}\" preset?", MessageNotificationType.YesNo,
                new Action(async () =>
                {
                    System.IO.File.Delete(preset.FilePath);
                    await LoadPresets();
                    await Task.Delay(1000);
                    try
                    {
                        var imgPath = System.IO.Path.Combine(App.Location, "PresetImages", preset.PresetName + ".jpg");
                        if (System.IO.File.Exists(imgPath))
                        {
                            System.IO.File.Delete(imgPath);
                        }
                    }
                    catch (Exception)
                    {
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
            MainWindow.ShowToast(new ToastInfo() { Title = "Success", Detail = "JSON copied successfully!", ToastType = ToastType.Success });

        }

        [RelayCommand]
        async Task OpenFile()
        {
            var fo = new OpenFileDialog();
            fo.DefaultExt = ".jpg";
            fo.Filter = "Image Files|*.jpg;*.jpeg;*.png;";

            if (fo.ShowDialog() == true)
            {
                NewPreset.CoverFilePath = fo.FileName;
            }            
        }


        [RelayCommand]
        async Task CreatePresetFinally()
        {            
            NewPreset.IsPresetNameError = false;
            NewPreset.IsJsonEmpty = false;
            NewPreset.IsJsonError = false;
            NewPreset.IsWebLinkBad = false;
            NewPreset.IsDownloadError = false;

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

            if (!string.IsNullOrEmpty(NewPreset.CoverFilePath))
            {
                var imgPath = System.IO.Path.Combine(App.Location, "PresetImages", NewPreset.PresetName + ".jpg");
                System.IO.File.Copy(NewPreset.CoverFilePath, imgPath, true);                
            }

            if (!string.IsNullOrEmpty(NewPreset.CoverWebLink))
            {

                if (NewPreset.CoverWebLink.Contains(".jpg") || NewPreset.CoverWebLink.Contains(".jpeg")
                    || NewPreset.CoverWebLink.Contains(".png"))
                {
                    IsPresetCreating = true;
                    var client = new HttpClient();
                    try
                    {
                        var bytes = await client.GetByteArrayAsync(NewPreset.CoverWebLink);
                        var imgPath = System.IO.Path.Combine(App.Location, "PresetImages", NewPreset.PresetName + ".jpg");
                        await System.IO.File.WriteAllBytesAsync(imgPath, bytes);
                        IsPresetCreating = false;
                    }
                    catch (Exception)
                    {
                        IsPresetCreating = false;
                        NewPreset.IsDownloadError = true;
                        return;
                    }
                }
                else
                {
                    NewPreset.IsWebLinkBad = true;
                    return;
                }
            }

            var presetFilePath = System.IO.Path.Combine(App.Location, "GameJsons", NewPreset.PresetName + ".json");
            await System.IO.File.WriteAllTextAsync(presetFilePath, NewPreset.Json);
            
            IsModalOpen = false;
            MainWindow.ShowToast(new ToastInfo() { Title = "Success", Detail="Preset created successfully!", ToastType= ToastType.Success });
            await LoadPresets();

        }
    }
}
