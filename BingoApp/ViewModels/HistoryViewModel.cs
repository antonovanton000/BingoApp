using BingoApp.Models;
using BingoApp.Views;
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

namespace BingoApp.ViewModels
{
    public partial class HistoryViewModel : MyBaseViewModel
    {

        [ObservableProperty]
        ObservableCollection<Room> historyRooms = new ObservableCollection<Room>();

        async Task LoadHistory()
        {
            HistoryRooms.Clear();

            var dirName = System.IO.Path.Combine(App.Location, "HistoryRooms");
            if (System.IO.Directory.Exists(dirName))
            {
                var files = System.IO.Directory.GetFiles(dirName, "*.json");
                foreach (var file in files)
                {
                    try
                    {
                        var roomJson = await System.IO.File.ReadAllTextAsync(file);
                        var room = JsonConvert.DeserializeObject<Room>(roomJson);
                        HistoryRooms.Add(room);
                    }
                    catch (Exception)
                    {
                    }
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
            MainWindow.ShowMessage($"Do you realy want to delete \"{room.RoomName}\" from history?", MessageNotificationType.YesNo,
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
