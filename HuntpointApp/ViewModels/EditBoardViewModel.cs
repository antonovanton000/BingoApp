using HuntpointApp.Classes;
using HuntpointApp.Models;
using HuntpointApp.Views;
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

namespace HuntpointApp.ViewModels
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
            var newjson = JsonConvert.SerializeObject(BoardPreset.Objectives, Formatting.None);
            return firstJson != newjson || firstName != BoardPreset.PresetName;
        }

        public Game Game { get; set; }

        [ObservableProperty]
        ICollectionView boardObjectivesCollection;

        [ObservableProperty]
        string searchQueue;

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
        HuntpointAppPlayer selectedPlayer;

        [ObservableProperty]
        Objective selectedObjective;
        public ObservableCollection<HuntpointAppPlayer> AvailablePlayers { get; set; } = [];

        [RelayCommand]
        async Task Appearing()
        {
            MainWindow.HideSettingsButton();
            await LoadPresets();
            await LoadNotes();
            BoardObjectivesCollection = CollectionViewSource.GetDefaultView(BoardPreset.Objectives);
            BoardObjectivesCollection.SortDescriptions.Add(new SortDescription("Type", ListSortDirection.Ascending));
            BoardObjectivesCollection.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            firstJson = JsonConvert.SerializeObject(BoardPreset.Objectives, Formatting.None);
            firstName = BoardPreset.PresetName;
            (App.Current.MainWindow as MainWindow).frame.Navigating += Frame_Navigating;
        }

        List<ObjectiveNote> notes = [];

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
                        notes = JsonConvert.DeserializeObject<List<ObjectiveNote>>(json);
                    }
                    else
                        return;
                }

                if (notes == null)
                    notes = []; 

                foreach (var item in notes)
                {
                    var square = BoardPreset.Objectives.FirstOrDefault(i => i.Name.ToLower() == item.ObjectiveName.ToLower());
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

        [ObservableProperty]
        bool isAnySelected;

        [RelayCommand]
        async Task CopySelectedTo(BoardPreset preset)
        {
            var json = await System.IO.File.ReadAllTextAsync(preset.FilePath);
            var jarr = JArray.Parse(json);
            preset.Json = json;
            preset.ObjectivesCount = jarr.Count;

            foreach (var item in jarr)
            {
                preset.Objectives.Add(new PresetObjective() { Name = item["name"].Value<string>() });
            }

            foreach (var item in BoardPreset.Objectives.Where(i => i.IsChecked))
            {
                preset.Objectives.Add(item);
            }

            var newjson = JsonConvert.SerializeObject(preset.Objectives, Formatting.Indented);
            await System.IO.File.WriteAllTextAsync(preset.FilePath, newjson);
            
            MainWindow.ShowToast(new ToastInfo() { Title = App.Current.FindResource("mes_success").ToString(), Detail = App.Current.FindResource("mes_squarescopied").ToString() });
            await LoadPresets();

        }

        [RelayCommand]
        async Task CopySelectedToNewBoard()
        {
            MainWindow.InputDialog("New preset name", new Action<string>(async (s) =>
            {

                var newjson = JsonConvert.SerializeObject(BoardPreset.Objectives.Where(i => i.IsChecked).ToArray(), Formatting.Indented);
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
        void DeleteSquare(PresetObjective square)
        {
            BoardPreset.Objectives.Remove(square);
        }

        [RelayCommand]
        void DuplicateSquare(PresetObjective square)
        {
            var index = BoardPreset.Objectives.IndexOf(square);
            var newsquare = new PresetObjective()
            {
                Group = square.Group,
                Name = square.Name,
                Type = square.Type,
                Score = square.Score,
                ShiftingEath = square.ShiftingEath,
                Notes = square.Notes,                
            };
            BoardPreset.Objectives.Insert(index + 1, newsquare);
        }

        [RelayCommand]
        void RemoveDuplicates()
        {
            var noduplicates = new List<PresetObjective>();
            foreach (var item in BoardPreset.Objectives)
            {
                if (!noduplicates.Any(i => i.Name == item.Name))
                    noduplicates.Add(item);
            }

            var difference = BoardPreset.Objectives.Count - noduplicates.Count;

            if (difference > 0)
            {
                MainWindow.ShowMessage(string.Format(App.Current.FindResource("mes_0duplicatesqare").ToString(), difference), MessageNotificationType.YesNo, () =>
                {
                    BoardPreset.Objectives.Clear();
                    foreach (var item in noduplicates)
                    {
                        BoardPreset.Objectives.Add(item);
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
            BoardPreset.Objectives.Add(new PresetObjective() { Name = " ", Type = ObjectiveType.Regular, Score=1 });
        }

        [RelayCommand]
        void Search()
        {
            EnterSearch();
        }

        public void EnterSearch()
        {
            if (SearchQueue == null)
                return;

            BoardObjectivesCollection.Filter = item =>
            {
                PresetObjective obj = item as PresetObjective;
                return obj.Name.ToLower().Contains(SearchQueue.ToLower());
            };
        }

        [RelayCommand]
        void ClearSearch()
        {
            SearchQueue = "";
            BoardObjectivesCollection.Filter = null;
        }

        [ObservableProperty]
        bool isAllSelected;

        [RelayCommand]
        void SelectUnselectAll(bool isChecked)
        {
            foreach (var item in BoardPreset.Objectives)
            {
                item.IsChecked = isChecked;
            }
            IsAllSelected = isChecked;
            IsAnySelected = isChecked;
        }

        [RelayCommand]
        void CheckSquare()
        {
            IsAllSelected = !BoardPreset.Objectives.Any(i => !i.IsChecked);
            IsAnySelected = BoardPreset.Objectives.Any(i => i.IsChecked);
        }

        [RelayCommand]
        async Task Save()
        {
            var newjson = JsonConvert.SerializeObject(BoardPreset.Objectives, Formatting.Indented);
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
                await PresetCollection.RefreshGamePresets(Game);
            }
            else
            {
                await System.IO.File.WriteAllTextAsync(BoardPreset.FilePath, newjson);
            }

            BoardPreset.Json = JsonConvert.SerializeObject(BoardPreset.Objectives, Formatting.None);
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
                    var newjson = JsonConvert.SerializeObject(BoardPreset.Objectives, Formatting.Indented);
                    
                    var directoryPath = System.IO.Path.Combine(App.Location, "Presets", BoardPreset.Game);
                    var newPath = System.IO.Path.Combine(directoryPath, s + ".json");
                    await System.IO.File.WriteAllTextAsync(newPath, newjson);
                    
                    await PresetCollection.RefreshGamePresets(Game);
                    MainWindow.ShowToast(new ToastInfo() { Title = $"{App.Current.FindResource("mes_success")}", Detail =  string.Format(App.Current.FindResource("mes_0presetcreated").ToString(), s) });                    
                }
            }), BoardPreset.PresetName + " - Copy", App.Current.FindResource("mes_typenewpresetna").ToString());
        }

        [RelayCommand]
        async Task SaveNotes(PresetObjective square)
        {
            try
            {
                var note = notes.FirstOrDefault(i => i.ObjectiveName.ToLower() == square.Name.ToLower());
                if (note == null)
                {
                    note = new();
                    note.ObjectiveName = square.Name;
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
        public void IncreaseScore(PresetObjective obj)
        {
            if (obj.Score<999)
            {
                obj.Score += 1;
            }
        }

        [RelayCommand]
        public void DecreaseScore(PresetObjective obj)
        {
            if (obj.Score > 0)
            {
                obj.Score -= 1;
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

            var presetNotes = BoardPreset.Objectives
                .Where(i => !string.IsNullOrEmpty(i.Notes))
                .Select(i => new ObjectiveNote() { ObjectiveName = i.Name, Note = i.Notes })
                .ToArray();

            var presetJson = JsonConvert.SerializeObject(BoardPreset.Objectives, Formatting.None);
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

        PresetObjective _dragedSquare;

        public void StartDrag(IDragInfo dragInfo)
        {
            var square = (PresetObjective)dragInfo.SourceItem;
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
            if (dropInfo.Data is PresetObjective)
            {
                _dragedSquare = dropInfo.Data as PresetObjective;
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
