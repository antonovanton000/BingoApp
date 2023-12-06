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
using System.Threading;
using System.Threading.Tasks;

namespace BingoApp.ViewModels
{
    public partial class MainViewModel : MyBaseViewModel
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanConnect))]
        string roomUrl;
        
        public bool CanConnect => !string.IsNullOrEmpty(RoomUrl);

        [ObservableProperty]
        bool isModalVisible;

        [ObservableProperty]
        bool isConnectModalVisible;

        [ObservableProperty]
        bool isNewRoomModalVisible;

        [ObservableProperty]
        bool isBoardConnecting;

        [ObservableProperty]
        bool isConnectingToBoardStarted;

        [ObservableProperty]
        RoomConnectionInfo roomInfo;

        [ObservableProperty]
        NewBoardModel newRoom = new NewBoardModel();

        [ObservableProperty]
        PlayerCredentials credentials = new PlayerCredentials();

        [ObservableProperty]
        bool isIncorrectPassword = false;

        [ObservableProperty]
        bool isEmptyPassword = false;

        [ObservableProperty]
        bool isEmptyNickName = false;

        [ObservableProperty]
        bool isUsingPresets = true;

        [ObservableProperty]
        bool isBoardCreating = false;

        [ObservableProperty]
        bool isBoardWithAutoReveal = false;

        [ObservableProperty]
        bool isFromStartupArgs = false;

        [ObservableProperty]
        bool isActiveRoomsVisible = false;

        [ObservableProperty]
        ObservableCollection<RoomLockoutMode> roomLockoutModes = new ObservableCollection<RoomLockoutMode>();

        CancellationTokenSource tokenSource = new CancellationTokenSource();

        public string HelloMessage
        {
            get
            {
                var userName = BingoApp.Properties.Settings.Default.NickName;
                var message = "";
                if (DateTime.Now.Hour < 12)
                {
                    message = "Good Morning";
                }
                else if (DateTime.Now.Hour < 17)
                {
                    message = "Good Afternoon";
                }
                else
                {
                    message = "Good Evening";
                }

                return message + (string.IsNullOrEmpty(userName) ? "!" : $", {userName}!");
            }
        }

        [ObservableProperty]
        ObservableCollection<ActiveRoomModel> activeRooms = new ObservableCollection<ActiveRoomModel>();
        
        [RelayCommand]
        async Task Appearing()
        {
            MainWindow.ShowSettingsButton();
            Credentials.NickName = BingoApp.Properties.Settings.Default.NickName;
            GetActiveBoards();
            await CheckStartupArgs();
        }

        async Task CheckStartupArgs()
        {
            IsBoardWithAutoReveal = false;
            var startupArgs = App.StartupArgs;
            if (!string.IsNullOrEmpty(startupArgs))
            {
                try
                {
                    startupArgs = startupArgs.Replace("bingoapp://", "").Replace("/", "");
                    var json = Encoding.UTF8.GetString(Convert.FromBase64String(startupArgs));
                    var jobj = JObject.Parse(json);
                    var roomId = jobj["r"].Value<string>();
                    var password = jobj["p"].Value<string>();
                    var isAutoReveal = jobj["i"].Value<int>();

                    var nickName = BingoApp.Properties.Settings.Default.NickName;


                    IsModalVisible = true;
                    IsConnectModalVisible = true;
                    IsConnectingToBoardStarted = false;

                    RoomInfo = await (App.Current as App).BingoEngine.GetRoomInfoAsync($"https://bingosync.com/room/{roomId}");
                    IsBoardWithAutoReveal = isAutoReveal == 1;
                    Credentials.NickName = nickName;
                    Credentials.Password = password;                                        
                    IsFromStartupArgs = true;

                }
                catch (Exception ex)
                {
                    MainWindow.ShowErrorMessage(ex.Message, "SplashPage", "Appearing");
                    IsModalVisible = false;
                    IsConnectModalVisible = false;                

                }
                App.StartupArgs = string.Empty;
            }
        }


        [RelayCommand]
        async Task GetRoomConnectionInfo()
        {
            IsBusy = true;
            IsBoardConnecting = true;
            IsFromStartupArgs = false;
            try
            {
                if (!RoomUrl.ToLower().Contains("https://bingosync.com/room/"))
                {
                    MainWindow.ShowToast(new ToastInfo("Error", "Wrong link!", ToastType.Warning));
                    IsBoardConnecting = IsBusy = false;

                    return;
                }

                RoomInfo = await (App.Current as App).BingoEngine.GetRoomInfoAsync(RoomUrl);
                IsModalVisible = true;
                IsConnectModalVisible = true;
            }
            catch (Exception ex)
            {
                if (ex.Message == "Board not found")
                    MainWindow.ShowToast(new ToastInfo("Error", "Board not found!", ToastType.Warning));
                else if (ex.Message == "Wrong link")
                    MainWindow.ShowToast(new ToastInfo("Error", "Wrong link!", ToastType.Warning));
                else
                    MainWindow.ShowToast(new ToastInfo("Error", ex.Message, ToastType.Error));
            }
            IsBoardConnecting = false;
            IsBusy = false;
        }

        [ObservableProperty]
        ObservableCollection<BoardPreset> presets = new ObservableCollection<BoardPreset>();

        async Task LoadPresets()
        {
            var folderPath = System.IO.Path.Combine(App.Location, "GameJsons");
            var files = System.IO.Directory.GetFiles(folderPath, "*.json");
            Presets.Clear();
            foreach (var file in files)
            {
                try
                {
                    var json = await System.IO.File.ReadAllTextAsync(file);
                    var jarr = JArray.Parse(json);
                    var preset = new BoardPreset()
                    {
                        PresetName = System.IO.Path.GetFileNameWithoutExtension(file),
                        FilePath = file,
                        Json = json,
                        SquareCount = jarr.Count,
                    };
                    Presets.Add(preset);
                }
                catch (Exception)
                {

                }

            }
        }

        [RelayCommand]
        async Task CreateNewRoom()
        {

            IsBoardCreating = false;
            await LoadPresets();
            RoomLockoutModes.Clear();
            RoomLockoutModes.Add(new RoomLockoutMode() { Id = 2, Name = "Lockout" });
            RoomLockoutModes.Add(new RoomLockoutMode() { Id = 1, Name = "Non-Lockout" });
            
            var defaultRoomName = "New Room";
            if (!string.IsNullOrEmpty(BingoApp.Properties.Settings.Default.DefaultRoomName))
            {
                defaultRoomName = BingoApp.Properties.Settings.Default.DefaultRoomName;
            }
            else if (!string.IsNullOrEmpty(BingoApp.Properties.Settings.Default.NickName))
            {
                defaultRoomName = BingoApp.Properties.Settings.Default.NickName + "'s Board";
            }
            NewRoom = new NewBoardModel();
            NewRoom.NickName = BingoApp.Properties.Settings.Default.NickName;
            NewRoom.RoomName = defaultRoomName;
            NewRoom.Password = BingoApp.Properties.Settings.Default.DefaultPassword;
            NewRoom.RoomLockoutMode = RoomLockoutModes.FirstOrDefault(i => i.Id == 2);
            NewRoom.BoardPreset = null;
            NewRoom.IsAutoBoardReveal = false;
            
            IsNewRoomModalVisible = true;            
        }

        [RelayCommand]
        void UsingPresetSwitch()
        {
            IsUsingPresets = !IsUsingPresets;
        }

        [RelayCommand]
        async Task CreateNewRoomFinaly()
        {
            NewRoom.IsRoomNameError = string.IsNullOrEmpty(NewRoom.RoomName);
            NewRoom.IsPasswordError = string.IsNullOrEmpty(NewRoom.Password);
            NewRoom.IsNickNameError = string.IsNullOrEmpty(NewRoom.NickName);
            if (IsUsingPresets)
            {
                NewRoom.IsBoardJsonError = false;
                NewRoom.IsBoardPresetError = NewRoom.BoardPreset == null;
            }
            else
            {
                NewRoom.IsBoardPresetError = false;
                NewRoom.IsBoardJsonError = string.IsNullOrEmpty(NewRoom.BoardJSON);
            }

            if (!NewRoom.IsRoomNameError && !NewRoom.IsPasswordError && !NewRoom.IsNickNameError &&
                !(IsUsingPresets ? NewRoom.IsBoardPresetError : NewRoom.IsBoardJsonError))
            {
                IsBoardCreating = true;
                if (IsUsingPresets)
                {
                    NewRoom.BoardJSON = await System.IO.File.ReadAllTextAsync(NewRoom.BoardPreset.FilePath);
                }

                try
                {
                    var room = await (App.Current as App).BingoEngine.CreateRoomAsync(NewRoom);
                    room.IsAutoBoardReveal = NewRoom.IsAutoBoardReveal;
                    room.IsCreatorMode = true;
                    if (!room.CurrentPlayer.IsSpectator)
                    {
                        if (!string.IsNullOrEmpty(BingoApp.Properties.Settings.Default.PreferedColor))
                            await room.ChangeCollor((BingoColor)Enum.Parse(typeof(BingoColor), BingoApp.Properties.Settings.Default.PreferedColor));
                    }

                    await room.SaveAsync();

                    var vm = new RoomPageViewModel() { Room = room };

                    var page = new RoomPage() { DataContext = vm };
                    MainWindow.NavigateTo(page);
                }
                catch (Exception ex)
                {
                    if (ex.Message == "Wrong Json")
                        MainWindow.ShowMessage("You passed wrong json!", MessageNotificationType.Ok);
                }
            }
        }

        [RelayCommand]
        void CloseModals()
        {
            IsConnectModalVisible = IsModalVisible = IsNewRoomModalVisible = false;
        }

        [RelayCommand]
        async Task ConnectToRoom()
        {
            try
            {
                tokenSource = new CancellationTokenSource();
                IsIncorrectPassword = false;
                IsEmptyPassword = string.IsNullOrEmpty(Credentials.Password);
                IsEmptyNickName = string.IsNullOrEmpty(Credentials.NickName);

                if (string.IsNullOrEmpty(Credentials.NickName) || string.IsNullOrEmpty(Credentials.Password))
                {
                    return;
                }

                IsConnectingToBoardStarted = true;
                var room = await (App.Current as App).BingoEngine.ConnectToRoomAsync(RoomInfo, Credentials, tokenSource.Token, true);
                var vm = new RoomPageViewModel() { Room = room };
                if (!room.CurrentPlayer.IsSpectator)
                {
                    if (!string.IsNullOrEmpty(BingoApp.Properties.Settings.Default.PreferedColor))
                        await room.ChangeCollor((BingoColor)Enum.Parse(typeof(BingoColor), BingoApp.Properties.Settings.Default.PreferedColor));
                }

                if (IsFromStartupArgs)
                {
                    room.IsAutoBoardReveal = IsBoardWithAutoReveal;
                    room.IsCreatorMode = false;
                }

                var page = new RoomPage() { DataContext = vm };
                IsConnectingToBoardStarted = false;
                MainWindow.NavigateTo(page);
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
                }
            }
        }

        [RelayCommand]
        void ConnectToRoomCancel()
        {
            tokenSource.Cancel(false);
            IsConnectModalVisible = false;
        }

        [RelayCommand]
        void OpenBoardManager()
        {
            MainWindow.NavigateTo(new BoardManagerPage());
        }

        [RelayCommand]
        void OpenHistory()
        {
            MainWindow.NavigateTo(new HistoryPage());
        }

        [RelayCommand]
        void OpenSettings()
        {
            MainWindow.NavigateTo(new SettingsPage());
        }

        void GetActiveBoards()
        {
            ActiveRooms.Clear();
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
                        ActiveRooms.Add(new ActiveRoomModel()
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
            }

            IsActiveRoomsVisible = ActiveRooms.Count > 0;
        }

        [RelayCommand]
        async Task ContinueRoom(ActiveRoomModel model)
        {

            MainPage.CloseActiveRoomsPopup();
            RoomInfo = new RoomConnectionInfo()
            {
                RoomName = model.RoomName
            };
            IsModalVisible = true;
            IsConnectModalVisible = true;
            IsConnectingToBoardStarted = true;

            var room = await (App.Current as App).BingoEngine.LoadRoomFromFile(model.FilePath);
            var vm = new RoomPageViewModel() { Room = room };

            var page = new RoomPage() { DataContext = vm };
            IsConnectingToBoardStarted = false;
            IsConnectModalVisible = false;
            MainWindow.NavigateTo(page);
        }

        [RelayCommand]
        async Task EndGame(ActiveRoomModel model)
        {
            MainPage.CloseActiveRoomsPopup();
            MainWindow.ShowMessage($"Do you realy want to finish\r\n{model.RoomName} game?", MessageNotificationType.YesNo, async () =>
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
    }
}
