using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    public partial class ConnectToRoomViewModel : MyBaseViewModel
    {
        public ConnectToRoomViewModel()
        {

        }

        #region Properties

        [ObservableProperty]
        string roomId;

        [ObservableProperty]
        RoomConnectionInfo roomInfo;

        [ObservableProperty]
        ObservableCollection<ActiveRoomModel> activeRooms = new ObservableCollection<ActiveRoomModel>();

        [ObservableProperty]
        bool isActiveRoomsVisible = false;

        [ObservableProperty]
        PlayerCredentials credentials;

        [ObservableProperty]
        bool isConnectingModalVisible;

        [ObservableProperty]
        bool isIncorrectPassword;

        [ObservableProperty]
        bool isEmptyNickName;

        [ObservableProperty]
        bool isConnectingToBoardStarted;

        [ObservableProperty]
        bool isRoomIdError;

        private CancellationTokenSource tokenSource;

        #endregion

        #region Commands

        [RelayCommand]
        async Task Appearing()
        {
            await CheckStartupArgs();
            GetActiveBoards();
        }

        [RelayCommand]
        async Task GetRoomConnectionInfo()
        {
            IsRoomIdError = string.IsNullOrEmpty(RoomId);
            if (IsRoomIdError) return;

            IsBusy = true;
            try
            {
                if (!string.IsNullOrEmpty(RoomId))
                {
                    var roomInfoResp = await App.RestClient.GetRoomInfo(RoomId);
                    if (roomInfoResp.IsSuccess)
                    {
                        RoomInfo = roomInfoResp.Data;
                        if (RoomInfo.LegendaryObjective== null)
                        {
                            RoomInfo.LegendaryObjective = App.Current.Resources["mp_random"].ToString();
                        }

                        Credentials = new PlayerCredentials() { NickName = HuntpointApp.Properties.Settings.Default.NickName };
                        IsConnectingModalVisible = true;
                    }
                    else
                    {
                        if (roomInfoResp.ErrorMessage == "Not Found")
                            MainWindow.ShowToast(new ToastInfo(App.Current.FindResource("mes_error").ToString(), App.Current.FindResource("mes_boardnotfound").ToString(), ToastType.Warning));
                    }
                }
            }
            catch (Exception ex)
            {
                MainWindow.ShowToast(new ToastInfo(App.Current.FindResource("mes_error").ToString(), ex.Message, ToastType.Error));
                Logger.Error(ex);
            }
            IsBusy = false;
        }

        [RelayCommand]
        void ConnectingToRoomCancel()
        {
            tokenSource?.Cancel();
            IsConnectingModalVisible = false;
        }

        [RelayCommand]
        async Task ConnectToRoom()
        {
            try
            {
                tokenSource = new CancellationTokenSource();
                IsIncorrectPassword = false;
                IsEmptyNickName = false;
                if (string.IsNullOrEmpty(Credentials.NickName))
                {
                    IsEmptyNickName = true;
                    return;
                }

                IsConnectingToBoardStarted = true;
                var connectModel = new ConnectRoomModel()
                {
                    RoomId = RoomInfo.RoomId,
                    PlayerId = App.CurrentPlayer.Id,
                    AsSpectator = Credentials.IsSpectator,
                    Password = Credentials.Password
                };

                var roomRespond = await App.RestClient.ConnectToRoom(connectModel);
                if (roomRespond.IsSuccess)
                {
                    var room = roomRespond.Data;

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
                    //if (!room.CurrentPlayer.IsSpectator)
                    //{
                    //    if (!string.IsNullOrEmpty(BingoApp.Properties.Settings.Default.PreferedColor))
                    //        await room.ChangeCollor((BingoColor)Enum.Parse(typeof(BingoColor), BingoApp.Properties.Settings.Default.PreferedColor));
                    //}
                    room.Password = Credentials.Password;
                    room.IsCreatorMode = false;
                    await room.SaveAsync();
                    var page = new RoomPage() { DataContext = vm };
                    IsConnectingToBoardStarted = false;
                    MainWindow.NavigateTo(page);
                }
                else
                {
                    IsConnectingToBoardStarted = false;
                    if (roomRespond.ErrorMessage == "Wrong Password")
                    {
                        IsIncorrectPassword = true;
                    }
                }

            }
            catch (Exception ex)
            {
                IsConnectingToBoardStarted = false;
                if (ex.Message == "Incorrect Password!")
                {
                    IsIncorrectPassword = true;
                }
                else
                {
                    MainWindow.ShowErrorMessage(ex.Message, "MainPage", "ConnectToRoom");
                    Logger.Error(ex);
                }
            }
        }

        [RelayCommand]
        void ConnectToRoomCancel()
        {

        }

        [RelayCommand]
        async Task ContinueRoom(ActiveRoomModel model)
        {
            try
            {
                IsConnectingToBoardStarted = true;

                var room = await Room.LoadRoomFromFile(model.FilePath);
                if (room == null)
                {

                    MainWindow.ShowMessage(App.Current.FindResource("mes_roomloaderror").ToString(), MessageNotificationType.Ok, okCallback: () =>
                    {
                        File.Delete(model.FilePath);
                        GetActiveBoards();
                    });
                    IsConnectingToBoardStarted = false;
                    return;
                }
                if (!room.IsPractice)
                {
                    var roomInfoResp = await App.RestClient.GetRoomInfo(room.RoomId);
                    if (!roomInfoResp.IsSuccess)
                    {
                        MainWindow.ShowMessage(App.Current.FindResource("mes_roomnotfound").ToString(), MessageNotificationType.Ok, okCallback: () =>
                        {
                            File.Delete(model.FilePath);
                            GetActiveBoards();
                        });
                        IsConnectingToBoardStarted = false;
                        return;
                    }
                    var serverRoomResp = await App.RestClient.ConnectToRoom(new ConnectRoomModel()
                    {
                        RoomId = room.RoomId,
                        Password = room.Password,
                        AsSpectator = room.CurrentPlayer.IsSpectator,
                        PlayerId = room.CurrentPlayer.Id
                    });
                    if (serverRoomResp.IsSuccess)
                    {
                        var serverRoom = serverRoomResp.Data;
                        room.ChatMessages = serverRoom.ChatMessages;
                        room.Board = serverRoom.Board;
                        room.Players = serverRoom.Players;
                    }
                    else
                    {
                        MainWindow.ShowMessage(App.Current.FindResource("mes_roomloaderror").ToString(), MessageNotificationType.Ok, okCallback: () =>
                        {
                            File.Delete(model.FilePath);
                            GetActiveBoards();
                        });
                        IsConnectingToBoardStarted = false;
                        return;
                    }
                }


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
                IsConnectingToBoardStarted = false;
                MainWindow.NavigateTo(page);
            }
            catch (Exception ex) { Logger.Error(ex); }
        }

        [RelayCommand]
        void EndGame(ActiveRoomModel model)
        {
            try
            {
                MainPage.CloseActiveRoomsPopup();
                MainWindow.ShowMessage(string.Format(App.Current.FindResource("mes_doyourealywantt").ToString(), model.RoomName), MessageNotificationType.YesNo, async () =>
                {

                    var roomJson = await System.IO.File.ReadAllTextAsync(model.FilePath);
                    var room = JsonConvert.DeserializeObject<Room>(roomJson);

                    room.IsGameEnded = true;
                    room.EndDate = DateTime.Now;
                    await room.SaveToHistoryAsync();
                    room.RemoveFromActiveRooms();

                    GetActiveBoards();
                });
            }
            catch (Exception ex) { Logger.Error(ex); }
        }

        [RelayCommand]
        void ClosePage()
        {
            MainViewModel.ShowWelcomeText();
        }

        #endregion

        #region Methods
        void GetActiveBoards()
        {
            try
            {
                ActiveRooms.Clear();
                var lst = new List<ActiveRoomModel>();
                var dirName = System.IO.Path.Combine(App.Location, "ActiveRooms");
                if (System.IO.Directory.Exists(dirName))
                {
                    var files = System.IO.Directory.GetFiles(dirName, "*.json");
                    foreach (var file in files)
                    {
                        try
                        {
                            var fname = System.IO.Path.GetFileNameWithoutExtension(file);
                            var arr = fname.Split("_");
                            var crDate = System.IO.File.GetCreationTime(file);
                            lst.Add(new ActiveRoomModel()
                            {
                                RoomId = arr[1],
                                RoomName = arr[0],
                                FilePath = file,
                                CreationDate = crDate
                            });
                        }
                        catch (Exception)
                        {
                        }
                    }
                    foreach (var item in lst.OrderByDescending(i => i.CreationDate))
                    {
                        ActiveRooms.Add(item);
                    }
                }
                MainViewModel.UpdateActiveRoomsCount(ActiveRooms.Count);
                IsActiveRoomsVisible = ActiveRooms.Count > 0;
            }
            catch (Exception ex) { Logger.Error(ex); }
        }

        async Task CheckStartupArgs()
        {
            try
            {
                if (!string.IsNullOrEmpty(App.StartupArgs))
                {
                    var jobj = JObject.Parse(App.StartupArgs);
                    App.StartupArgs = null;
                    var roomId = jobj["r"].Value<string>();
                    var password = jobj["p"].Value<string>();
                    var isAutoReveal = jobj["i"].Value<int>();
                    var gameExtraModeStr = jobj["egm"].Value<string>();
                    var gamePresetStr = jobj["pr"].Value<string>();
                    var gameStr = jobj["g"].Value<string>();
                    var nickName = HuntpointApp.Properties.Settings.Default.NickName;
                    var roomInfoResp = await App.RestClient.GetRoomInfo(roomId);
                    if (roomInfoResp.IsSuccess)
                    {
                        RoomInfo = roomInfoResp.Data;
                        RoomInfo.IsFromBingoApp = true;
                        if (RoomInfo.LegendaryObjective == null)
                        {
                            RoomInfo.LegendaryObjective = App.Current.Resources["mp_random"].ToString();
                        }
                        Credentials = new PlayerCredentials() { NickName = nickName, Password = password };
                        RoomInfo.IsFromBingoApp = true;
                        IsConnectingModalVisible = true;
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        #endregion
    }
}
