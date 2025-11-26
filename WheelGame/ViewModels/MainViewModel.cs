using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using WheelGame.Models;
using WheelGame.Views;

namespace WheelGame.ViewModels
{
    public partial class MainViewModel : MyBaseViewModel
    {

        [ObservableProperty]
        string nickname;

        [ObservableProperty]
        int timerSeconds = 180;

        [RelayCommand]
        async Task Appearing()
        {
            try
            {
                var resp = await App.RestClient.UpdateUserInfoAsync(App.CurrentPlayer);
                await ConnectToHub();
                var timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(40) };
                timer.Tick += async (s, e) =>
                {
                    var resp = await App.RestClient.UpdateUserInfoAsync(App.CurrentPlayer);
                };
                timer.Start();

                Nickname = Properties.Settings.Default.NickName;
                TimerSeconds = Properties.Settings.Default.TimerSeconds;

            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        async Task ConnectToHub()
        {
            try
            {
                var resp = await App.RestClient.UpdateUserInfoAsync(App.CurrentPlayer);
                await App.SignalRHub.ConnectAsync(WheelGame.Properties.Settings.Default.PlayerId);
            }
            catch (Exception ex) { Logger.Error(ex); }
        }

        [RelayCommand]
        async Task CreateRoom()
        {
            var newRoom = new NewRoomModel();

            MainWindow.InputDialog("Введите название комнаты", async (roomName)=> {
                if (!string.IsNullOrEmpty(roomName))
                {
                    var json = System.IO.File.ReadAllText("Presets/preset.json");
                    var debufs = System.IO.File.ReadAllText("Presets/debuffs.json");
                    newRoom.RoomName = roomName;
                    newRoom.BoardJSON = json;
                    newRoom.DebufsJSON = debufs;
                    newRoom.CreatorId = App.CurrentPlayer.Id;
                    newRoom.TimerSeconds = TimerSeconds;
                    try
                    {
                        var roomResponse = await App.RestClient.CreateRoomAsync(newRoom);
                        if (!roomResponse.IsSuccess)
                        {                            
                            MainWindow.ShowErrorMessage("Нет связи с сервером", nameof(MainPage), nameof(CreateRoom));
                            return;
                        }
                        
                        var room = roomResponse.Data;

                        room.IsCreatorMode = true;                                                
                        
                        var vm = new RoomPageViewModel() { Room = room };

                        var page = new RoomPage() { DataContext = vm };

                        MainWindow.NavigateTo(page);                        
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message == "Wrong Json")
                            MainWindow.ShowMessage(App.Current.FindResource("mes_youpassedwrongj").ToString(), MessageNotificationType.Ok);
                        else
                            MainWindow.ShowErrorMessage(ex.Message, nameof(MainPage), nameof(CreateRoom));                        
                        Logger.Error(ex);
                    }

                }
            }, inputValue: "TestRoom");                                       
        }

        [RelayCommand]
        async Task ConnectRoom()
        {
            MainWindow.InputDialog("Введите RoomId", async (roomId) =>
            {
                if (!string.IsNullOrEmpty(roomId))
                {
                    var roomInfoResp = await App.RestClient.GetRoomInfo(roomId);
                    if (roomInfoResp.IsSuccess)
                    {
                        var roomInfo = roomInfoResp.Data;
                        var connectModel = new ConnectRoomModel()
                        {
                            RoomId = roomInfo.RoomId,
                            PlayerId = App.CurrentPlayer.Id,
                        };
                        var roomRespond = await App.RestClient.ConnectToRoom(connectModel);
                        if (roomRespond.IsSuccess)
                        {
                            var room = roomRespond.Data;
                            var vm = new RoomPageViewModel() { Room = room };
                            room.IsCreatorMode = false;
                            var page = new RoomPage() { DataContext = vm };
                            MainWindow.NavigateTo(page);
                        }
                    }
                }
            });
        }

        [RelayCommand]
        void TestRoom()
        {
            var room = new Room()
            {
                RoomId = "TestRoomId",
                RoomName = "TestRoom",
                EarlyWheel = new Wheel()
                {
                    Objectives = new ObservableCollection<Objective>()
                    {
                        new Objective() { Name = "Objective 1", Slot = "slot1" },
                        new Objective() { Name = "Objective 2", Slot = "slot2" },
                        new Objective() { Name = "Objective 3", Slot = "slot3" },
                        new Objective() { Name = "Objective 4", Slot = "slot4" },
                        new Objective() { Name = "Objective 5", Slot = "slot5" },
                        new Objective() { Name = "Objective 6", Slot = "slot6" }
                    }
                },
                MiddleWheel = new Wheel()
                {
                    Objectives = new ObservableCollection<Objective>()
                    {
                        new Objective() { Name = "Objective 7", Slot = "slot7" },
                        new Objective() { Name = "Objective 8", Slot = "slot8" },
                        new Objective() { Name = "Objective 9", Slot = "slot9" },
                        new Objective() { Name = "Objective 10", Slot = "slot10" },
                        new Objective() { Name = "Objective 11", Slot = "slot11" },
                        new Objective() { Name = "Objective 12", Slot = "slot12" }
                    }
                },
            };
            var vm = new RoomPageViewModel() { Room = room };

            var page = new RoomPage() { DataContext = vm };

            MainWindow.NavigateTo(page);
        }

        [RelayCommand]
        void SaveSettings()
        {
            Properties.Settings.Default.NickName = Nickname;
            Properties.Settings.Default.TimerSeconds = TimerSeconds;
            Properties.Settings.Default.Save(); 
        }
    }
}
