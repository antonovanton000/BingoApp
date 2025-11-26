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
            var squaresJson = JsonConvert.SerializeObject(BoardPreset.Squares, Formatting.None);
            var exceptionJson = JsonConvert.SerializeObject(BoardPreset.Exceptions, Formatting.None);

            var newjson = squaresJson + "\r\n//---EXCEPTIONS---//" + "\r\n" + exceptionJson;
            return firstJson != newjson || firstName != BoardPreset.PresetName;
        }

        string firstJson;

        string firstName;

        [ObservableProperty]
        bool isSquaresVisible = true;

        [ObservableProperty]
        bool isExceptionsVisible = false;

        [ObservableProperty]
        ObservableCollection<BoardPreset> presets = new ObservableCollection<BoardPreset>();

        [ObservableProperty]
        BoardPreset boardPreset;

        [ObservableProperty]
        bool isModalOpen = false;
        
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
            BoardSqaresCollection2 = CollectionViewSource.GetDefaultView(BoardPreset.Squares);

            var squaresJson = JsonConvert.SerializeObject(BoardPreset.Squares, Formatting.None);
            var exceptionJson = JsonConvert.SerializeObject(BoardPreset.Exceptions, Formatting.None);

            firstJson = squaresJson + "\r\n//---EXCEPTIONS---//" + "\r\n" + exceptionJson;            
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
                        MainWindow.ShowToast(new ToastInfo() { Title = $"{App.Current.FindResource("mes_success")}", Detail = string.Format(App.Current.FindResource("mes_0presetcreated").ToString(), s) });
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

                    MainWindow.ShowToast(new ToastInfo() { Title = $"{App.Current.FindResource("mes_success")}", Detail = string.Format(App.Current.FindResource("mes_0presetcreated").ToString(), s) });
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
            var newSquare = new PresetSquare()
            {
                Name = square.Name,
                Group = square.Group,
            };

            BoardPreset.Squares.Insert(index + 1, newSquare);
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

        [RelayCommand]
        void AddNewBellow(PresetSquare square)
        {
            var index = BoardPreset.Squares.IndexOf(square);
            BoardPreset.Squares.Insert(index + 1, new PresetSquare() { Name = App.Current.FindResource("mes_newsqare").ToString() });
        }

        [RelayCommand]
        void SortSquares()
        {
            var sorted = BoardPreset.Squares.OrderByDescending(i => i.Group).ToList();
            BoardPreset.Squares.Clear();
            foreach (var item in sorted)
            {
                BoardPreset.Squares.Add(item);
            }
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
            foreach (var item in BoardPreset.Squares)
            {
                item.Name = item.Name.Trim();
                item.Group = item.Group?.Trim();
            }

            var squaresJson = JsonConvert.SerializeObject(BoardPreset.Squares, Formatting.None);
            var exceptionJson = JsonConvert.SerializeObject(BoardPreset.Exceptions, Formatting.None);

            var newjson = squaresJson + "\r\n//---EXCEPTIONS---//" + "\r\n" + exceptionJson;
            if (firstName != BoardPreset.PresetName)
            {
                System.IO.File.Delete(BoardPreset.FilePath);
                var directoryPath = System.IO.Path.GetDirectoryName(BoardPreset.FilePath);
                var newPath = System.IO.Path.Combine(directoryPath, BoardPreset.PresetName + ".json");
                await System.IO.File.WriteAllTextAsync(newPath, newjson);
                
                BoardPreset.FilePath = newPath;
            }
            else
            {
                await System.IO.File.WriteAllTextAsync(BoardPreset.FilePath, newjson);
            }

            BoardPreset.Json = newjson;
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

                    MainWindow.ShowToast(new ToastInfo() { Title = $"{App.Current.FindResource("mes_success")}", Detail = string.Format(App.Current.FindResource("mes_0presetcreated").ToString(), s) });
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

        [RelayCommand]
        void ShowSquares()
        {
            IsSquaresVisible = true;
            IsExceptionsVisible = false;
        }

        [RelayCommand]
        void ShowExceptions()
        {
            IsSquaresVisible = false;
            IsExceptionsVisible = true;
        }

        [RelayCommand]
        void AddNewExtension()
        {
            BoardPreset.Exceptions.Add(new PresetSquareException());
        }

        [ObservableProperty]
        PresetSquareException? currentException;

        [RelayCommand]
        void AddExceptionOpenModal(PresetSquareException exception)
        {
            if (exception == null)
            {
                exception = new PresetSquareException();
                BoardPreset.Exceptions.Add(exception);
            }
            
            CurrentException = exception;
            BoardSqaresCollection2.Filter = item =>
            {
                PresetSquare square = item as PresetSquare;
                return !(CurrentException.SquareNames.Contains(square.Name)) && SearchQueue2!= null && square.Name.ToLower().Contains(SearchQueue2?.ToLower());
            };
            IsModalOpen = true;
        }
        
        [RelayCommand]
        void CloseAddSquareModal()
        {
            SearchQueue2 = null;
            IsModalOpen = false;
            CurrentException = null;
        }

        [RelayCommand]
        void AddSquareToException(PresetSquare square)
        {
            if (CurrentException!=null && square!=null)
            {
                CurrentException.SquareNames.Add(square.Name);
                BoardSqaresCollection2.Filter = item =>
                {
                    PresetSquare square = item as PresetSquare;
                    return !(CurrentException.SquareNames.Contains(square.Name)) && SearchQueue2 != null && square.Name.ToLower().Contains(SearchQueue2?.ToLower());
                };
            }
        }

        [ObservableProperty]
        ICollectionView boardSqaresCollection2;

        [ObservableProperty]
        string searchQueue2;

        [RelayCommand]
        void Search2()
        {
            EnterSearch2();
        }

        public void EnterSearch2()
        {
            if (SearchQueue2 == null)
                return;

            BoardSqaresCollection2.Filter = item =>
            {
                PresetSquare square = item as PresetSquare;
                return square.Name.ToLower().Contains(SearchQueue2.ToLower());
            };
        }

        [RelayCommand]
        void ClearSearch2()
        {
            SearchQueue2 = "";
            BoardSqaresCollection2.Filter = item =>
            {
                PresetSquare square = item as PresetSquare;
                return !(CurrentException.SquareNames.Contains(square.Name)) && SearchQueue2 != null && square.Name.ToLower().Contains(SearchQueue2?.ToLower());
            };
        }

        [RelayCommand]
        void DeleteException(PresetSquareException exception)
        {
            BoardPreset.Exceptions.Remove(exception);
        }

        
        public void DeleteSquareFromException(PresetSquareException exception, string squareName)
        {
            exception.SquareNames.Remove(squareName);
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
