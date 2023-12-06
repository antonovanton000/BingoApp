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
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace BingoApp.ViewModels
{
    public partial class EditBoardViewModel : MyBaseViewModel
    {

        public EditBoardViewModel()
        {

        }

        bool leaveunsaved = false;

        private async void Frame_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == System.Windows.Navigation.NavigationMode.Back)
            {
                if (IsPresetChanged())
                {
                    e.Cancel = !leaveunsaved;
                    if (!leaveunsaved)
                    {
                        MainWindow.ShowMessage("You have unsaved changes!\r\n\r\nAre you sure you want to leave?", MessageNotificationType.YesNo,
                            new Action(async () =>
                            {
                                leaveunsaved = true;
                                MainWindow.CloseMessage();
                                (App.Current.MainWindow as MainWindow).frame.Navigating -= Frame_Navigating;
                                await Task.Delay(300);
                                MainWindow.GoBack();

                            }),
                            new Action(() =>
                            {
                                leaveunsaved = false;
                            }));
                    }
                }
            }
        }

        bool IsPresetChanged()
        {
            var newjson = JsonConvert.SerializeObject(BoardPreset.Squares, Formatting.None);
            return firstJson != newjson || firstName != BoardPreset.PresetName;
        }

        string firstJson;

        string firstName;

        [ObservableProperty]
        ObservableCollection<BoardPreset> presets = new ObservableCollection<BoardPreset>();

        [ObservableProperty]
        BoardPreset boardPreset;

        [ObservableProperty]
        bool isModalOpen = false;

        [ObservableProperty]
        bool isImageChanging = false;

        [RelayCommand]
        async Task Appearing()
        {
            MainWindow.HideSettingsButton();
            await LoadPresets();
            BoardSqaresCollection = CollectionViewSource.GetDefaultView(BoardPreset.Squares);
            firstJson = JsonConvert.SerializeObject(BoardPreset.Squares, Formatting.None);
            firstName = BoardPreset.PresetName;
            (App.Current.MainWindow as MainWindow).frame.Navigating += Frame_Navigating;
        }


        async Task LoadPresets()
        {
            var folderPath = System.IO.Path.Combine(App.Location, "GameJsons");
            var files = System.IO.Directory.GetFiles(folderPath, "*.json");
            Presets.Clear();
            foreach (var file in files)
            {
                var preset = new BoardPreset()
                {
                    PresetName = System.IO.Path.GetFileNameWithoutExtension(file),
                    FilePath = file,
                };

                Presets.Add(preset);
            }
        }


        [RelayCommand]
        void OpenFile()
        {
            var fo = new OpenFileDialog();
            fo.DefaultExt = ".jpg";
            fo.Filter = "Image Files|*.jpg;*.jpeg;*.png;";

            if (fo.ShowDialog() == true)
            {
                BoardPreset.CoverFilePath = fo.FileName;
            }        
        }

        [RelayCommand]
        void ChangeImage()
        {            
            IsImageChanging = false;
            IsModalOpen = true;
        }


        [RelayCommand]
        async Task ChangeImageFinally()
        {
            var imgPath = System.IO.Path.Combine(App.Location, "PresetImages", BoardPreset.PresetName + ".jpg");

            var needDelete = false;
            if (System.IO.File.Exists(imgPath))
            {
                needDelete = true;
            }

            if (!string.IsNullOrEmpty(BoardPreset.CoverFilePath))
            {                
                if (needDelete)
                {
                    System.IO.File.Delete(imgPath);
                }
                System.IO.File.Copy(BoardPreset.CoverFilePath, imgPath, true);
            }

            if (!string.IsNullOrEmpty(BoardPreset.CoverWebLink))
            {

                if (BoardPreset.CoverWebLink.Contains(".jpg") || BoardPreset.CoverWebLink.Contains(".jpeg")
                    || BoardPreset.CoverWebLink.Contains(".png"))
                {
                    IsImageChanging = true;
                    var client = new HttpClient();
                    try
                    {
                        var bytes = await client.GetByteArrayAsync(BoardPreset.CoverWebLink);
                        if (needDelete)
                        {
                            System.IO.File.Delete(imgPath);
                        }
                        await System.IO.File.WriteAllBytesAsync(imgPath, bytes);
                        IsImageChanging = false;
                    }
                    catch (Exception)
                    {
                        IsImageChanging = false;
                        BoardPreset.IsDownloadError = true;
                        return;
                    }
                }
                else
                {
                    BoardPreset.IsWebLinkBad = true;
                    return;
                }
            }
            BoardPreset.RefreshImageCover();
            IsImageChanging = false;
            IsModalOpen = false;
        }

        [ObservableProperty]
        bool isAnySelected;

        [RelayCommand]
        async Task CopySelectedTo(BoardPreset preset)
        {
            var json = await  System.IO.File.ReadAllTextAsync(preset.FilePath);
            var jarr = JArray.Parse(json);
            preset.Json = json;
            preset.SquareCount = jarr.Count;
            
            foreach (var item in jarr)
            {
                preset.Squares.Add(new PresetSquare() { Name = item["name"].Value<string>() });
            }

            foreach (var item in BoardPreset.Squares.Where(i => i.IsChecked))
            {
                preset.Squares.Add(item);
            }

            var newjson = JsonConvert.SerializeObject(preset.Squares, Formatting.Indented);
            await System.IO.File.WriteAllTextAsync(preset.FilePath, newjson);

            MainWindow.ShowToast(new ToastInfo() { Title = "Success", Detail = "Squares copied!" });
            await LoadPresets();

        }

        [RelayCommand]
        async Task CopySelectedToNewBoard()
        {
            MainWindow.InputDialog("New preset name", new Action<string>(async (s) => {

                var newjson = JsonConvert.SerializeObject(BoardPreset.Squares.Where(i => i.IsChecked).ToArray(), Formatting.Indented);
                var directoryPath = System.IO.Path.GetDirectoryName(BoardPreset.FilePath);
                var newPath = System.IO.Path.Combine(directoryPath, s + ".json");
                await System.IO.File.WriteAllTextAsync(newPath, newjson);

                MainWindow.ShowToast(new ToastInfo() { Title = "Success", Detail = $"{s} preset created!" });
                await LoadPresets();
            }), "", "Type new preset name");
        }

        [RelayCommand]
        void DeleteSquare(PresetSquare square)
        {
            BoardPreset.Squares.Remove(square);
        }

        [RelayCommand]
        void DuplicateSquare(PresetSquare square)
        {
            var index = BoardPreset.Squares.IndexOf(square);
            BoardPreset.Squares.Insert(index + 1, square);
        }

        [RelayCommand]
        void AddNewSquare()
        {
            BoardPreset.Squares.Add(new PresetSquare() { Name = "New Sqare" });
        }


        [RelayCommand]
        void CopyAsJson()
        {
            Clipboard.SetText(firstJson);
            MainWindow.ShowToast(new ToastInfo() { Title = "Success", Detail = $"JSON copied successfully!" });
        }

        [ObservableProperty]
        ICollectionView boardSqaresCollection;

        [ObservableProperty]
        string searchQueue;

        [RelayCommand]
        void Search()
        {
            EnterSearch();
        }

        public void EnterSearch()
        {
            if (SearchQueue == null)
                return;

            BoardSqaresCollection.Filter = item =>
            {
                PresetSquare square = item as PresetSquare;
                return square.Name.ToLower().Contains(SearchQueue.ToLower());
            };
        }

        [RelayCommand]
        void ClearSearch()
        {
            SearchQueue = "";
            BoardSqaresCollection.Filter = null;
        }

        [ObservableProperty]
        bool isAllSelected;

        [RelayCommand]
        void SelectUnselectAll(bool isChecked)
        {
            foreach (var item in BoardPreset.Squares)
            {
                item.IsChecked = isChecked;
            }
            IsAllSelected = isChecked;
            IsAnySelected = isChecked;
        }

        [RelayCommand]
        void CheckSquare()
        {
            IsAllSelected = !BoardPreset.Squares.Any(i => !i.IsChecked);
            IsAnySelected = BoardPreset.Squares.Any(i => i.IsChecked);
        }

        [RelayCommand]
        async Task Save()
        {
            var newjson = JsonConvert.SerializeObject(BoardPreset.Squares, Formatting.Indented);
            if (firstName != BoardPreset.PresetName)
            {
                System.IO.File.Delete(BoardPreset.FilePath);
                var directoryPath = System.IO.Path.GetDirectoryName(BoardPreset.FilePath);
                var newPath = System.IO.Path.Combine(directoryPath, BoardPreset.PresetName + ".json");
                await System.IO.File.WriteAllTextAsync(newPath, newjson);
                var imgPath = System.IO.Path.Combine(App.Location, "PresetImages", firstName + ".jpg");
                if (System.IO.File.Exists(imgPath))
                {
                    directoryPath = System.IO.Path.GetDirectoryName(imgPath);
                    newPath = System.IO.Path.Combine(directoryPath, BoardPreset.PresetName + ".jpg");
                    System.IO.File.Copy(imgPath, newPath, true);
                    System.IO.File.Delete(imgPath);
                }
                BoardPreset.FilePath = newPath;
            }
            else
            {
                await System.IO.File.WriteAllTextAsync(BoardPreset.FilePath, newjson);
            }

            BoardPreset.Json = JsonConvert.SerializeObject(BoardPreset.Squares, Formatting.None);
            firstJson = BoardPreset.Json;
            firstName = BoardPreset.PresetName;
            MainWindow.ShowToast(new ToastInfo() { Title = "Success", Detail = $"Preset saved!" });
        }

        [RelayCommand]
        async Task SaveAsCopy()
        {
            MainWindow.InputDialog("New preset name", new Action<string>(async (s) =>
            {
                if (!string.IsNullOrEmpty(s))
                {
                    var newjson = JsonConvert.SerializeObject(BoardPreset.Squares, Formatting.Indented);
                    var directoryPath = System.IO.Path.GetDirectoryName(BoardPreset.FilePath);
                    var newPath = System.IO.Path.Combine(directoryPath, s + ".json");
                    await System.IO.File.WriteAllTextAsync(newPath, newjson);

                    MainWindow.ShowToast(new ToastInfo() { Title = "Success", Detail = $"{s} preset created!" });
                }
            }), BoardPreset.PresetName + " - Copy", "Type new preset name");
        }

    }
}
