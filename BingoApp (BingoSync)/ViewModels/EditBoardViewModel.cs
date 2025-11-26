using BingoApp.Classes;
using BingoApp.Models;
using BingoApp.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GongSolutions.Wpf.DragDrop;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
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
    public partial class EditBoardViewModel : MyBaseViewModel, IDropTarget
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
                        MainWindow.ShowMessage($"{App.Current.FindResource("mes_youhaveunsavedc")}", MessageNotificationType.YesNo,
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

        [ObservableProperty]
        bool isSendToPlayerPopupVisible;

        [ObservableProperty]
        BingoAppPlayer selectedPlayer;

        public ObservableCollection<BingoAppPlayer> AvailablePlayers { get; set; } = [];

        [RelayCommand]
        async Task Appearing()
        {
            MainWindow.HideSettingsButton();
            await LoadPresets();
            await LoadNotes();
            BoardSqaresCollection = CollectionViewSource.GetDefaultView(BoardPreset.Squares);
            firstJson = JsonConvert.SerializeObject(BoardPreset.Squares, Formatting.None);
            firstName = BoardPreset.PresetName;
            (App.Current.MainWindow as MainWindow).frame.Navigating += Frame_Navigating;
        }

        List<SquareNote> notes = [];

        async Task LoadNotes()
        {
            try
            {
                var notesPath = System.IO.Path.Combine(App.Location, "notes.json");
                if (System.IO.File.Exists(notesPath))
                {
                    var json = await System.IO.File.ReadAllTextAsync(notesPath);
                    if (json != null)
                    {
                        notes = JsonConvert.DeserializeObject<List<SquareNote>>(json);
                    }
                    else
                        return;
                }

                if (notes == null)
                    notes = []; 

                foreach (var item in notes)
                {
                    var square = BoardPreset.Squares.FirstOrDefault(i => i.Name.ToLower() == item.SquareName.ToLower());
                    if (square != null)
                    {
                        square.Notes = item.Note;
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        async Task LoadPresets()
        {
            Presets.Clear();
            var presets = await PresetCollection.GetPresetsAsync();
            foreach (var item in presets)
            {
                Presets.Add(item);
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
                //BoardPreset.CoverFilePath = fo.FileName;
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
            
            IsImageChanging = false;
            IsModalOpen = false;
        }

        [ObservableProperty]
        bool isAnySelected;

        [RelayCommand]
        async Task CopySelectedTo(BoardPreset preset)
        {
            var json = await System.IO.File.ReadAllTextAsync(preset.FilePath);
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
            
            MainWindow.ShowToast(new ToastInfo() { Title = App.Current.FindResource("mes_success").ToString(), Detail = App.Current.FindResource("mes_squarescopied").ToString() });
            await LoadPresets();

        }

        [RelayCommand]
        async Task CopySelectedToNewBoard()
        {
            MainWindow.InputDialog("New preset name", new Action<string>(async (s) =>
            {

                var newjson = JsonConvert.SerializeObject(BoardPreset.Squares.Where(i => i.IsChecked).ToArray(), Formatting.Indented);
                var directoryPath = System.IO.Path.GetDirectoryName(BoardPreset.FilePath);
                var newPath = System.IO.Path.Combine(directoryPath, s + ".json");
                if (System.IO.File.Exists(newPath))
                {
                    MainWindow.ShowMessage(string.Format(App.Current.FindResource("mes_presetwithname0").ToString(), s), MessageNotificationType.YesNo, async () =>
                    {
                        var imgPath = System.IO.Path.Combine(App.Location, "PresetImages", BoardPreset.PresetName + ".jpg");
                        if (System.IO.File.Exists(imgPath))
                        {
                            var imgDirectoryPath = System.IO.Path.GetDirectoryName(imgPath);
                            var imgNewPath = System.IO.Path.Combine(imgDirectoryPath, s + ".jpg");
                            System.IO.File.Copy(imgPath, imgNewPath, true);
                        }

                        await System.IO.File.WriteAllTextAsync(newPath, newjson);
                        MainWindow.ShowToast(new ToastInfo() { Title = $"{App.Current.FindResource("mes_success")}", Detail =  string.Format(App.Current.FindResource("mes_0presetcreated").ToString(), s) });
                        await LoadPresets();
                    });
                }
                else
                {

                    var imgPath = System.IO.Path.Combine(App.Location, "PresetImages", BoardPreset.PresetName + ".jpg");
                    if (System.IO.File.Exists(imgPath))
                    {
                        var imgDirectoryPath = System.IO.Path.GetDirectoryName(imgPath);
                        var imgNewPath = System.IO.Path.Combine(imgDirectoryPath, s + ".jpg");
                        System.IO.File.Copy(imgPath, imgNewPath, true);
                    }

                    await System.IO.File.WriteAllTextAsync(newPath, newjson);

                    MainWindow.ShowToast(new ToastInfo() { Title = $"{App.Current.FindResource("mes_success")}", Detail =  string.Format(App.Current.FindResource("mes_0presetcreated").ToString(), s) });                    
                    await LoadPresets();
                }
            }), "", App.Current.FindResource("mes_typenewpresetna").ToString());
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
        void RemoveDuplicates()
        {
            var noduplicates = new List<PresetSquare>();
            foreach (var item in BoardPreset.Squares)
            {
                if (!noduplicates.Any(i => i.Name == item.Name))
                    noduplicates.Add(item);
            }

            var difference = BoardPreset.Squares.Count - noduplicates.Count;

            if (difference > 0)
            {
                MainWindow.ShowMessage(string.Format(App.Current.FindResource("mes_0duplicatesqare").ToString(), difference), MessageNotificationType.YesNo, () =>
                {
                    BoardPreset.Squares.Clear();
                    foreach (var item in noduplicates)
                    {
                        BoardPreset.Squares.Add(item);
                    }

                    MainWindow.ShowToast(new ToastInfo() { Title = App.Current.FindResource("mes_allduplicatesqa").ToString() });
                });
            }
            else
            {
                MainWindow.ShowToast(new ToastInfo() { Title = App.Current.FindResource("mes_noduplicatesqua").ToString() });
            }

        }

        [RelayCommand]
        void AddNewSquare()
        {
            BoardPreset.Squares.Add(new PresetSquare() { Name = App.Current.FindResource("mes_newsqare").ToString() });
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
            MainWindow.ShowToast(new ToastInfo() { Title = $"{App.Current.FindResource("mes_success")}", Detail = App.Current.FindResource("mes_presetsaved").ToString() });
        }

        [RelayCommand]
        async Task SaveAsCopy()
        {
            MainWindow.InputDialog(App.Current.FindResource("mes_newpresetname").ToString(), new Action<string>(async (s) =>
            {
                if (!string.IsNullOrEmpty(s))
                {
                    var newjson = JsonConvert.SerializeObject(BoardPreset.Squares, Formatting.Indented);
                    
                    var directoryPath = System.IO.Path.Combine(App.Location, "CustomPresets");
                    var newPath = System.IO.Path.Combine(directoryPath, s + ".json");
                    await System.IO.File.WriteAllTextAsync(newPath, newjson);

                    MainWindow.ShowToast(new ToastInfo() { Title = $"{App.Current.FindResource("mes_success")}", Detail =  string.Format(App.Current.FindResource("mes_0presetcreated").ToString(), s) });                    
                }
            }), BoardPreset.PresetName + " - Copy", App.Current.FindResource("mes_typenewpresetna").ToString());
        }

        [RelayCommand]
        async Task SaveNotes(PresetSquare square)
        {
            try
            {
                var note = notes.FirstOrDefault(i => i.SquareName.ToLower() == square.Name.ToLower());
                if (note == null)
                {
                    note = new();
                    note.SquareName = square.Name;
                    note.Note = square.Notes;
                    notes.Add(note);
                }
                else
                {
                    note.Note = square.Notes;
                }
                var json = JsonConvert.SerializeObject(notes);
                var notesPath = System.IO.Path.Combine(App.Location, "notes.json");
                await System.IO.File.WriteAllTextAsync(notesPath, json);
            }
            catch (Exception ex)
            {


            }
        }

        #region SharePreset

        [RelayCommand]
        void CopyAsJson()
        {
            Clipboard.SetText(firstJson);
            MainWindow.ShowToast(new ToastInfo() { Title = $"{App.Current.FindResource("mes_success")}", Detail = App.Current.FindResource("mes_jsoncopiedsucce").ToString() });
        }

        [RelayCommand]
        async Task SendToPlayer()
        {
            IsSendToPlayerPopupVisible = true;
            await GetAvailablePlayers();
        }

        [RelayCommand]
        async Task RefreshAvailablePlayers()
        {
            await GetAvailablePlayers();
        }

        async Task GetAvailablePlayers()
        {
            var resp = await App.RestClient.GetAvailablePlayersAsync(App.CurrentPlayer.Id);
            if (resp.IsSuccess)
            {
                AvailablePlayers.Clear();
                foreach (var item in resp.Data)
                {
                    AvailablePlayers.Add(item);
                }
            }
        }

        [RelayCommand]
        async Task SendPreset()
        {
            if (SelectedPlayer == null) return;

            var presetNotes = BoardPreset.Squares
                .Where(i => !string.IsNullOrEmpty(i.Notes))
                .Select(i => new SquareNote() { SquareName = i.Name, Note = i.Notes })
                .ToArray();

            var presetJson = JsonConvert.SerializeObject(BoardPreset.Squares, Formatting.None);
            var presetNotesJson = JsonConvert.SerializeObject(presetNotes, Formatting.None);

            var sendPreset = new SendPresetModel()
            {
                FromPlayerId = App.CurrentPlayer.Id,
                ToPlayerId = SelectedPlayer.Id,
                SquaresJson = presetJson,
                NotesJson = presetNotesJson,
                GameName = BoardPreset.Game,
                PresetName = BoardPreset.PresetName
            };

            IsSendToPlayerPopupVisible = false;
            var resp = await App.RestClient.SharePreset(sendPreset);
            if (resp.IsSuccess)
            {
                MainWindow.ShowToast(new ToastInfo() { Title = App.Current.FindResource("mes_success").ToString(), Detail = App.Current.FindResource("mes_invitesent").ToString() });
            }
        }


        #endregion


        #region Drag and Drop

        PresetSquare _dragedSquare;

        public void StartDrag(IDragInfo dragInfo)
        {
            var square = (PresetSquare)dragInfo.SourceItem;
            if (square != null)
            {
                _dragedSquare = square;
                _dragedSquare.IsDraging = true;
                dragInfo.Effects = DragDropEffects.Move;
            }
        }

        public bool CanStartDrag(IDragInfo dragInfo)
        {
            return true;
        }

        public void Dropped(IDropInfo dropInfo)
        {
            //throw new NotImplementedException();
        }

        public void DragDropOperationFinished(DragDropEffects operationResult, IDragInfo dragInfo)
        {
            _dragedSquare.IsDraging = false;
            //throw new NotImplementedException();
        }

        public void DragCancelled()
        {
            _dragedSquare.IsDraging = false;
            //throw new NotImplementedException();
        }

        public bool TryCatchOccurredException(Exception exception)
        {
            return true;
            //throw new NotImplementedException();
        }

        public IEnumerable SortDropTargetItems(IEnumerable items)
        {
            //throw new NotImplementedException();
            return items;
        }

        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data is PresetSquare)
            {
                _dragedSquare = dropInfo.Data as PresetSquare;
                if (_dragedSquare != null)
                    _dragedSquare.IsDraging = true;

                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Move;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (_dragedSquare != null)
            {
                _dragedSquare.IsDraging = false;
            }
            //throw new NotImplementedException();
        }

        #endregion
    }
}
