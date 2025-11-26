using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HuntpointApp.Classes;
using HuntpointApp.Models;
using HuntpointApp.Views;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HuntpointApp.ViewModels
{
    public partial class NewRoomViewModel : MyBaseViewModel
    {
        public NewRoomViewModel()
        {
            var defaultRoomName = "New Room";
            if (!string.IsNullOrEmpty(HuntpointApp.Properties.Settings.Default.DefaultRoomName))
            {
                defaultRoomName = HuntpointApp.Properties.Settings.Default.DefaultRoomName;
            }
            else if (!string.IsNullOrEmpty(HuntpointApp.Properties.Settings.Default.NickName))
            {
                defaultRoomName = HuntpointApp.Properties.Settings.Default.NickName + "'s Board";
            }
            NewRoom = new NewRoomModel() { Password = null };
            NewRoom.CreatorId = App.CurrentPlayer.Id;
            NewRoom.RoomName = defaultRoomName;
            NewRoom.BoardPreset = null;
            NewRoom.LegendaryObjective = null;
            NewRoom.LegendaryObjectiveItem = NewRoom.LegendaryObjectives.First();
            NewRoom.IsAutoBoardReveal = true;
            NewRoom.IsLegendaryExtraPoints = true;
            NewRoom.IsForFirstDeath = true;
            NewRoom.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(NewRoom.BoardPreset))
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
                        m.LegendaryObjectiveItem = NewRoom.LegendaryObjectives.First();
                    }
                }
            };

            Credentials = new PlayerCredentials();
        }

        #region Properties

        [ObservableProperty]
        NewRoomModel newRoom = new();

        [ObservableProperty]
        bool isBoardCreating = false;

        [ObservableProperty]
        ObservableCollection<BoardPreset> presets = new ObservableCollection<BoardPreset>();

        [ObservableProperty]
        PlayerCredentials credentials = new();

        #endregion

        #region Commands

        [RelayCommand]
        async Task Appearing()
        {
            try
            {
                IsBoardCreating = false;
                await LoadPresets();
                NewRoom.BoardPreset = null;
                
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        [RelayCommand]
        async Task CreateNewRoomFinaly()
        {
            NewRoom.IsRoomNameError = string.IsNullOrEmpty(NewRoom.RoomName);
            NewRoom.IsBoardPresetError = NewRoom.BoardPreset == null;

            if (!NewRoom.IsRoomNameError && NewRoom.BoardPreset != null)
            {
                IsBoardCreating = true;
                NewRoom.LegendaryObjective = NewRoom.LegendaryObjectiveItem?.Value;
                NewRoom.BoardPreset.LoadObjectives();
                var objectives = NewRoom.BoardPreset.GetObjectives();
                var json = JsonConvert.SerializeObject(objectives);
                NewRoom.BoardJSON = json;
                NewRoom.GameName = NewRoom.BoardPreset.Game;
                NewRoom.PresetName = NewRoom.BoardPreset.PresetName;
                NewRoom.ExtraGameMode = RoomExtraMode.ToExtraGameMode(NewRoom.RoomExtraMode.Id);
                try
                {
                    var roomResponse = await App.RestClient.CreateRoomAsync(NewRoom);
                    if (!roomResponse.IsSuccess)
                    {
                        IsBoardCreating = false;
                        MainWindow.ShowErrorMessage("Нет связи с сервером", nameof(NewRoomPage), nameof(CreateNewRoomFinaly));
                        return;
                    }
                    var room = roomResponse.Data;

                    room.IsCreatorMode = true;
                    room.PresetObjectives = objectives;
                    room.Password = NewRoom.Password;

                    var notesPath = System.IO.Path.Combine(App.Location, "notes.json");
                    var squareAppearingPath = System.IO.Path.Combine(App.Location, "squareAppearingCount.json");
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

                    //if (!room.CurrentPlayer.IsSpectator)
                    //{
                    //    if (!string.IsNullOrEmpty(HuntpointApp.Properties.Settings.Default.PreferedColor))
                    //        await room.ChangeCollor((BingoColor)Enum.Parse(typeof(BingoColor), BingoApp.Properties.Settings.Default.PreferedColor));
                    //}

                    await room.SaveAsync();

                    var vm = new RoomPageViewModel() { Room = room };

                    var page = new RoomPage() { DataContext = vm };

                    MainWindow.NavigateTo(page);
                    IsBoardCreating = false;                    
                }
                catch (Exception ex)
                {
                    if (ex.Message == "Wrong Json")
                        MainWindow.ShowMessage(App.Current.FindResource("mes_youpassedwrongj").ToString(), MessageNotificationType.Ok);
                    else
                        MainWindow.ShowErrorMessage(ex.Message, nameof(NewRoomPage), nameof(CreateNewRoomFinaly));
                    IsBoardCreating = false;
                    Logger.Error(ex);
                }
            }
        }

        [RelayCommand]
        void CloseModals()
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
