using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HuntpointApp.Classes;
using HuntpointApp.Models;
using HuntpointApp.Views;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HuntpointApp.ViewModels
{
    public partial class PracticeViewModel : MyBaseViewModel
    {
        public PracticeViewModel()
        {
            PracticeRoom = new ();            
        }

        #region Properties

        [ObservableProperty]
        NewRoomModel practiceRoom;

        [ObservableProperty]
        ObservableCollection<BoardPreset> presets = new ObservableCollection<BoardPreset>();
        #endregion

        #region Commands

        [RelayCommand]
        async Task Appearing()
        {
            await LoadPresets();
            PracticeRoom.BoardPreset = null;
            PracticeRoom.LegendaryObjective = null;
            PracticeRoom.LegendaryObjectiveItem = PracticeRoom.LegendaryObjectives.First();
            PracticeRoom.IsAutoBoardReveal = true;
            PracticeRoom.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(PracticeRoom.BoardPreset))
                {
                    if (s is NewRoomModel m)
                    {
                        m.LegendaryObjectives.Clear();
                        m.LegendaryObjectives.Add(new LegendaryObjectiveSelect() { Name = App.Current.Resources["mp_random"].ToString() });
                        if (m.BoardPreset != null)
                        {
                            foreach (var item in m.BoardPreset.Objectives.Where(i => i.Type == ObjectiveType.Legendary))
                            {
                                m.LegendaryObjectives.Add(new LegendaryObjectiveSelect() { Name = item.Name, Value = item.Name });
                            }
                        }
                        m.LegendaryObjectiveItem = PracticeRoom.LegendaryObjectives.First();
                    }
                }
            };
        }

        [RelayCommand]
        async Task StartPractice()
        {
            try
            {
                if (PracticeRoom.BoardPreset == null)
                {
                    PracticeRoom.IsBoardPresetError = true;
                    return;
                }
                
                var roomInfo = new RoomConnectionInfo()
                {
                    RoomName = App.Current.FindResource("practice").ToString()
                };
                
                var json = await System.IO.File.ReadAllTextAsync(PracticeRoom.BoardPreset.FilePath);

                var nickName = HuntpointApp.Properties.Settings.Default.NickName ?? App.Current.FindResource("practice").ToString();
                var settingsColor = HuntpointApp.Properties.Settings.Default.PreferedColor;
                settingsColor = string.IsNullOrEmpty(settingsColor) ? "blue" : settingsColor;
                var defaultColor = Enum.Parse<HuntpointColor>(settingsColor);
                var player = new Player()
                {
                    IsSpectator = false,
                    Color = defaultColor,
                    NickName = nickName,
                    Id = "practice"
                };

                var room = new Room()
                {
                    RoomName = App.Current.FindResource("practice").ToString(),
                    IsPractice = true,
                    IsCreatorMode = true,
                    IsRevealed = false,
                    Board = new(),
                    RoomSettings = new RoomSettings() { GameName = PracticeRoom.BoardPreset.Game, PresetName=PracticeRoom.BoardPreset.PresetName, HideCard = true },                 
                    CurrentPlayer = player,
                    ChosenColor = defaultColor,
                    BoardJSON = PracticeRoom.BoardJSON,
                    LegendaryObjective = PracticeRoom.LegendaryObjectiveItem?.Value,
                    PresetJSON = json
                };
                room.Players.Add(player);
                room.CurrentPlayer = player;

                room.GeneratePracticeBoard();
                var notesPath = System.IO.Path.Combine(App.Location, "notes.json");
                if (System.IO.File.Exists(notesPath))
                {
                    var notesJson = await System.IO.File.ReadAllTextAsync(notesPath);
                    var notes = JsonConvert.DeserializeObject<List<ObjectiveNote>>(notesJson);
                    if (notes != null)
                    {
                        foreach (var item in room.Board.Objectives)
                        {
                            var note = notes.FirstOrDefault(i => i.ObjectiveName.ToLower() == item.Name.ToLower());
                            if (note != null)
                            {
                                item.Notes = note.Note;
                            }
                        }
                    }
                }
                var vm = new RoomPageViewModel() { Room = room };

                var page = new RoomPage() { DataContext = vm };               
                MainWindow.NavigateTo(page);
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage(ex.Message, nameof(MainPage), nameof(StartPractice));
                Logger.Error(ex);
            }
        }

        [RelayCommand]
        void ClosePage()
        {
            MainViewModel.ShowWelcomeText();
        }

        #endregion

        #region Methods
        async Task LoadPresets()
        {
            try
            {
                Presets.Clear();
                var presets = await PresetCollection.GetPresetsAsync();
                foreach (var item in presets)
                {
                    Presets.Add(item);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        #endregion
    }
}
