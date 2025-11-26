using HuntpointApp.Models;
using HuntpointApp.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace HuntpointApp.ViewModels
{
    public partial class HistoryViewModel : MyBaseViewModel
    {
        [ObservableProperty]
        ObservableCollection<Room> historyRooms;

        async Task LoadHistory()
        {
            HistoryRooms = new ObservableCollection<Room>();

            var dirName = System.IO.Path.Combine(App.Location, "HistoryRooms");
            var historyItems = new List<Room>();
            if (System.IO.Directory.Exists(dirName))
            {
                var files = System.IO.Directory.GetFiles(dirName, "*.json");
                foreach (var file in files)
                {
                    try
                    {
                        var roomJson = await System.IO.File.ReadAllTextAsync(file);
                        var room = JsonConvert.DeserializeObject<Room>(roomJson);
                        historyItems.Add(room);
                    }
                    catch (Exception)
                    {
                    }
                }
                foreach (var item in historyItems.OrderByDescending(i => i.StartDate))
                {
                    HistoryRooms.Add(item);
                }
            }
        }


        [RelayCommand]
        async Task Appearing()
        {
            MainWindow.HideSettingsButton();
            IsBusy = true;
            await LoadHistory();
            IsBusy = false;
        }


        [RelayCommand]
        void RemoveFromHistory(Room room)
        {
            MainWindow.ShowMessage(string.Format(App.Current.Resources["mes_deletefromhistory"].ToString(), room.RoomName), MessageNotificationType.YesNo,
                new Action(async () =>
                {
                    room.RemoveFromHistory();
                    await LoadHistory();
                }
            ));
        }

        [RelayCommand]
        void ViewRoom(Room room)
        {
            var vm = new RoomHistoryViewModel() { Room = room };
            var page = new RoomHistoryPage() { DataContext = vm };

            MainWindow.NavigateTo(page);
        }

    }
}
