using BingoApp.Classes;
using BingoApp.ModalPopups;
using BingoApp.Models;
using BingoApp.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Windows.AI.MachineLearning;

namespace BingoApp.ViewModels
{
    public partial class MainViewModel : MyBaseViewModel
    {

        public MainViewModel()
        {
            App.SignalRHub.OnInvitePlayerRecieved += SignalRHub_OnInvitePlayerRecieved;
            App.SignalRHub.OnCancelInviteRecieved += SignalRHub_OnCancelInviteRecieved;
            App.SignalRHub.OnGameInviteRecieved += SignalRHub_OnGameInviteRecieved;
            App.SignalRHub.OnSharePresetRecieved += SignalRHub_OnSharePresetRecieved;
        }

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
        bool isPracticeModalVisible;

        [ObservableProperty]
        bool isBoardConnecting;

        [ObservableProperty]
        bool isConnectingToBoardStarted;

        [ObservableProperty]
        RoomConnectionInfo roomInfo;

        [ObservableProperty]
        NewBoardModel newRoom = new NewBoardModel();

        [ObservableProperty]
        NewBoardModel practiceRoom = new NewBoardModel();

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
        bool isDynamicCreateVisible = false;

        [ObservableProperty]
        bool isDynamicPresetCreated;

        [ObservableProperty]
        bool isUsingDynamicPreset;

        GameMode connectRoomGameMode;

        ExtraGameMode connectRoomExtraGameMode;

        string connectRoomPresetName;

        string connectRoomGameName;

        [ObservableProperty]
        ObservableCollection<RoomLockoutMode> roomLockoutModes = RoomLockoutMode.All;

        [ObservableProperty]
        ObservableCollection<RoomExtraMode> roomExtraModes = RoomExtraMode.All;

        CancellationTokenSource tokenSource = new CancellationTokenSource();

        [ObservableProperty]
        ObservableCollection<BoardPreset> presets = new ObservableCollection<BoardPreset>();

        public string HelloMessage
        {
            get
            {
                var userName = BingoApp.Properties.Settings.Default.NickName;
                var message = "";
                if (DateTime.Now.Hour < 12)
                {
                    message = App.Current.FindResource("mp_goodmorning").ToString(); //"Good Morning";
                }
                else if (DateTime.Now.Hour < 17)
                {
                    message = App.Current.FindResource("mp_goodafternoon").ToString();//"Good Afternoon";
                }
                else
                {
                    message = App.Current.FindResource("mp_goodevening").ToString();//"Good Evening";
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
            try
            {
                var resp = await App.RestClient.UpdateUserInfoAsync(App.CurrentPlayer);
                await ConnectToHub();
                await App.SignalRHub.ConnectAsync(App.CurrentPlayer.Id);
                var timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(40) };
                timer.Tick += async (s, e) =>
                {
                    var resp = await App.RestClient.UpdateUserInfoAsync(App.CurrentPlayer);
                };
                timer.Start();
                await CheckForUpdates();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
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
                    var gameModeStr = jobj["gm"].Value<string>();
                    var gameExtraModeStr = jobj["egm"].Value<string>();
                    var gamePresetStr = jobj["pr"].Value<string>();
                    var gameNameStr = jobj["g"].Value<string>();
                    connectRoomGameMode = GameMode.Other;
                    connectRoomPresetName = "Unknown";
                    connectRoomGameName = "Unknown";

                    if (gameModeStr != null)
                    {
                        connectRoomGameMode = Enum.Parse<GameMode>(gameModeStr);
                    }

                    if (gamePresetStr != null)
                    {
                        connectRoomPresetName = gamePresetStr;
                    }

                    if (gameNameStr != null)
                    {
                        connectRoomGameName = gameNameStr;
                    }
                    if (gameExtraModeStr != null)
                    {
                        connectRoomExtraGameMode = Enum.Parse<ExtraGameMode>(gameExtraModeStr);
                    }


                    var nickName = BingoApp.Properties.Settings.Default.NickName;

                    IsModalVisible = true;
                    IsConnectModalVisible = true;
                    IsConnectingToBoardStarted = false;

                    RoomInfo = await (App.Current as App).BingoEngine.GetRoomInfoAsync($"https://bingosync.com/room/{roomId}");
                    RoomInfo.GameName = gameNameStr;
                    RoomInfo.PresetName = gamePresetStr;
                    RoomInfo.GameMode = connectRoomGameMode;
                    RoomInfo.GameExtraMode = connectRoomExtraGameMode;
                    RoomInfo.IsFromBingoApp = true;

                    IsBoardWithAutoReveal = isAutoReveal == 1;
                    Credentials.NickName = nickName;
                    Credentials.Password = password;
                    IsFromStartupArgs = true;

                }
                catch (Exception ex)
                {
                    MainWindow.ShowErrorMessage(ex.Message, nameof(MainPage), nameof(CheckStartupArgs));
                    IsModalVisible = false;
                    IsConnectModalVisible = false;
                    Logger.Error(ex);
                }
                App.StartupArgs = string.Empty;
            }

            if (App.NewBoardModel != null && !string.IsNullOrEmpty(App.DynamicPresetJSON))
            {
                IsUsingDynamicPreset = true;
                NewRoom = App.NewBoardModel;
                MainWindow.ShowModal(new CreateNewRoomModal(), this);
                IsNewRoomModalVisible = true;
                App.NewBoardModel = null;
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
                    MainWindow.ShowToast(new ToastInfo(App.Current.FindResource("mes_error").ToString(), App.Current.FindResource("mes_wronglink").ToString(), ToastType.Warning));
                    IsBoardConnecting = IsBusy = false;

                    return;
                }

                RoomInfo = await (App.Current as App).BingoEngine.GetRoomInfoAsync(RoomUrl);
                MainWindow.ShowModal(new ConnectingToRoomModal(), this);
                IsModalVisible = true;
                IsConnectModalVisible = true;
            }
            catch (Exception ex)
            {
                if (ex.Message == "Board not found")
                    MainWindow.ShowToast(new ToastInfo(App.Current.FindResource("mes_error").ToString(), App.Current.FindResource("mes_boardnotfound").ToString(), ToastType.Warning));
                else if (ex.Message == "Wrong link")
                    MainWindow.ShowToast(new ToastInfo(App.Current.FindResource("mes_error").ToString(), App.Current.FindResource("mes_wronglink").ToString(), ToastType.Warning));
                else
                    MainWindow.ShowToast(new ToastInfo(App.Current.FindResource("mes_error").ToString(), ex.Message, ToastType.Error));

                Logger.Error(ex);
            }
            IsBoardConnecting = false;
            IsBusy = false;
        }

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

        [RelayCommand]
        async Task CreateNewRoom()
        {
            try
            {
                IsUsingDynamicPreset = false;
                IsUsingPresets = true;
                IsBoardCreating = false;
                await LoadPresets();
                RoomLockoutModes = RoomLockoutMode.All;
                RoomExtraModes = RoomExtraMode.All;

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
                NewRoom.RoomLockoutMode = RoomLockoutModes.First(i => i.Id == 2);
                NewRoom.RoomExtraMode = RoomExtraModes.First(i => i.Id == 1);
                NewRoom.BoardPreset = null;
                NewRoom.IsAutoBoardReveal = true;

                if (App.SignalRHub.IsHubConnected)
                {
                    IsDynamicCreateVisible = true;
                }

                MainWindow.ShowModal(new CreateNewRoomModal(), this);
                IsNewRoomModalVisible = true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        [RelayCommand]
        void UsingPresetSwitch()
        {
            IsUsingPresets = !IsUsingPresets;
        }

        [RelayCommand]
        void UsePreset()
        {
            IsUsingDynamicPreset = false;
            IsUsingPresets = true;
        }

        [RelayCommand]
        void AutoRevealSwitch()
        {
            try
            {
                if (!NewRoom.IsAutoBoardReveal)
                {
                    NewRoom.RoomExtraMode = RoomExtraModes.First(i => i.Id == 1);
                }
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
            NewRoom.IsPasswordError = string.IsNullOrEmpty(NewRoom.Password);
            NewRoom.IsNickNameError = string.IsNullOrEmpty(NewRoom.NickName);

            if (!IsUsingDynamicPreset)
            {
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
            }

            if (!NewRoom.IsRoomNameError && !NewRoom.IsPasswordError && !NewRoom.IsNickNameError &&
                !(IsUsingPresets ? NewRoom.IsBoardPresetError : NewRoom.IsBoardJsonError))
            {
                IsBoardCreating = true;
                if (IsUsingDynamicPreset)
                {
                    if (App.DynamicPresetJSON != null)
                    {
                        NewRoom.BoardJSON = App.DynamicPresetJSON;
                    }
                    App.DynamicPresetJSON = null;
                    NewRoom.RoomExtraMode = RoomExtraModes.First(i => i.Id == 1);
                }
                else
                {
                    if (IsUsingPresets)
                    {
                        NewRoom.BoardPreset.LoadSquares();
                        var squares = NewRoom.BoardPreset.GetSquares();
                        var json = JsonConvert.SerializeObject(squares);
                        NewRoom.BoardJSON = json;//await System.IO.File.ReadAllTextAsync(NewRoom.BoardPreset.FilePath);
                    }
                }

                try
                {
                    var room = await (App.Current as App).BingoEngine.CreateRoomAsync(NewRoom);
                    if (room == null)
                    {
                        IsBoardCreating = false;
                        IsModalVisible = false;
                        MainWindow.ShowErrorMessage("Нет связи с bingosync.com", nameof(MainPage), nameof(CreateNewRoomFinaly));
                        return;
                    }

                    room.IsAutoBoardReveal = NewRoom.IsAutoBoardReveal;
                    room.IsCreatorMode = true;
                    room.GameMode = RoomLockoutMode.ToGameMode(NewRoom.RoomLockoutMode.Id);
                    room.GameExtraMode = RoomExtraMode.ToExtraGameMode(NewRoom.RoomExtraMode.Id);
                    room.PresetName = IsUsingDynamicPreset ? NewRoom.DynamicPresetName : (NewRoom.BoardPreset?.PresetName ?? "Unknown");
                    room.GameName = IsUsingDynamicPreset ? NewRoom.DynamicGameName : (NewRoom.BoardPreset?.Game ?? "Unknown");

                    if (IsUsingPresets)
                    {
                        room.PresetSquares = NewRoom.BoardPreset.GetSquares();
                    }

                    var notesPath = System.IO.Path.Combine(App.Location, "notes.json");
                    var squareAppearingPath = System.IO.Path.Combine(App.Location, "squareAppearingCount.json");
                    if (System.IO.File.Exists(notesPath))
                    {
                        var notesJson = await System.IO.File.ReadAllTextAsync(notesPath);
                        var notes = JsonConvert.DeserializeObject<List<SquareNote>>(notesJson);
                        if (notes != null)
                        {
                            foreach (var item in room.Board.Squares)
                            {
                                var note = notes.FirstOrDefault(i => i.SquareName.ToLower() == item.Name.ToLower());
                                if (note != null)
                                {
                                    item.Notes = note.Note;
                                }
                            }
                        }
                    }

                    var squareAppearingInfo = new List<SquareAppearingInfo>();
                    if (System.IO.File.Exists(squareAppearingPath))
                    {
                        squareAppearingInfo = JsonConvert.DeserializeObject<List<SquareAppearingInfo>>(await System.IO.File.ReadAllTextAsync(squareAppearingPath));
                        if (squareAppearingInfo == null)
                            squareAppearingInfo = new List<SquareAppearingInfo>();
                    }
                    foreach (var item in room.Board.Squares)
                    {
                        var info = squareAppearingInfo.FirstOrDefault(i => i.Name == item.Name.ToLower());
                        if (info != null)
                        {
                            info.Count += 1;
                        }
                        else
                        {
                            squareAppearingInfo.Add(new SquareAppearingInfo() { Name = item.Name, Count = 1 });
                        }
                    }
                    await System.IO.File.WriteAllTextAsync(squareAppearingPath, JsonConvert.SerializeObject(squareAppearingInfo));

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
                        MainWindow.ShowMessage(App.Current.FindResource("mes_youpassedwrongj").ToString(), MessageNotificationType.Ok);
                    else
                        MainWindow.ShowErrorMessage(ex.Message, nameof(MainPage), nameof(CreateNewRoomFinaly));
                    IsBoardCreating = false;
                    Logger.Error(ex);
                }
            }
        }

        [RelayCommand]
        void CloseModals()
        {
            IsConnectModalVisible = IsModalVisible = IsNewRoomModalVisible = false;
            MainWindow.CloseModal();
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
                room.GameMode = connectRoomGameMode;
                room.PresetName = connectRoomPresetName;
                room.GameName = connectRoomGameName;
                room.GameExtraMode = connectRoomExtraGameMode;

                var notesPath = System.IO.Path.Combine(App.Location, "notes.json");
                if (System.IO.File.Exists(notesPath))
                {
                    var notesJson = await System.IO.File.ReadAllTextAsync(notesPath);
                    var notes = JsonConvert.DeserializeObject<List<SquareNote>>(notesJson);
                    if (notes != null)
                    {
                        foreach (var item in room.Board.Squares)
                        {
                            var note = notes.FirstOrDefault(i => i.SquareName.ToLower() == item.Name.ToLower());
                            if (note != null)
                            {
                                item.Notes = note.Note;
                            }
                        }
                    }
                }

                var squareAppearingPath = System.IO.Path.Combine(App.Location, "squareAppearingCount.json");
                var squareAppearingInfo = new List<SquareAppearingInfo>();
                if (System.IO.File.Exists(squareAppearingPath))
                {
                    squareAppearingInfo = JsonConvert.DeserializeObject<List<SquareAppearingInfo>>(await System.IO.File.ReadAllTextAsync(squareAppearingPath));
                    if (squareAppearingInfo == null)
                        squareAppearingInfo = new List<SquareAppearingInfo>();
                }
                foreach (var item in room.Board.Squares)
                {
                    var info = squareAppearingInfo.FirstOrDefault(i => i.Name == item.Name.ToLower());
                    if (info != null)
                    {
                        info.Count += 1;
                    }
                    else
                    {
                        squareAppearingInfo.Add(new SquareAppearingInfo() { Name = item.Name, Count = 1 });
                    }
                }
                await System.IO.File.WriteAllTextAsync(squareAppearingPath, JsonConvert.SerializeObject(squareAppearingInfo));

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
                    Logger.Error(ex);
                }
            }
        }

        [RelayCommand]
        void ConnectToRoomCancel()
        {
            tokenSource.Cancel(false);
            IsConnectModalVisible = false;
            MainWindow.CloseModal();
        }

        [RelayCommand]
        void CreateNewRoomCancel()
        {
            tokenSource.Cancel(false);
            IsConnectModalVisible = false;
            MainWindow.CloseModal();
        }

        [RelayCommand]
        void OpenBoardManager()
        {
            MainWindow.NavigateTo(new GameManagerPage());
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

                IsActiveRoomsVisible = ActiveRooms.Count > 0;
            }
            catch (Exception ex) { Logger.Error(ex); }
        }

        [RelayCommand]
        async Task ContinueRoom(ActiveRoomModel model)
        {
            try
            {
                MainPage.CloseActiveRoomsPopup();
                RoomInfo = new RoomConnectionInfo()
                {
                    RoomName = model.RoomName
                };
                IsModalVisible = true;
                MainWindow.ShowModal(new ConnectingToRoomModal(), this);
                IsConnectModalVisible = true;
                IsConnectingToBoardStarted = true;

                var room = await (App.Current as App).BingoEngine.LoadRoomFromFile(model.FilePath);
                var notesPath = System.IO.Path.Combine(App.Location, "notes.json");
                if (System.IO.File.Exists(notesPath))
                {
                    var notesJson = await System.IO.File.ReadAllTextAsync(notesPath);
                    var notes = JsonConvert.DeserializeObject<List<SquareNote>>(notesJson);
                    if (notes != null)
                    {
                        foreach (var item in room.Board.Squares)
                        {
                            var note = notes.FirstOrDefault(i => i.SquareName.ToLower() == item.Name.ToLower());
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
                IsConnectModalVisible = false;
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
        async Task OpenPracticeModal()
        {
            try
            {
                await LoadPresets();
                MainWindow.ShowModal(new PracticeModal(), this);
                IsPracticeModalVisible = true;
            }
            catch (Exception ex) { Logger.Error(ex); }
        }

        [RelayCommand]
        void ClosePracticeModal()
        {
            IsPracticeModalVisible = false;
        }

        [RelayCommand]
        async Task StartPractice()
        {
            try
            {
                if (PracticeRoom.BoardPreset == null)
                {
                    return;
                }

                MainPage.CloseActiveRoomsPopup();
                RoomInfo = new RoomConnectionInfo()
                {
                    RoomName = App.Current.FindResource("practice").ToString()
                };
                MainWindow.ShowModal(new ConnectingToRoomModal(), this);
                IsModalVisible = true;
                IsConnectModalVisible = true;
                IsConnectingToBoardStarted = true;
                PracticeRoom.BoardJSON = await System.IO.File.ReadAllTextAsync(PracticeRoom.BoardPreset.FilePath);
                PracticeRoom.BoardPreset.Squares = JsonConvert.DeserializeObject<ObservableCollection<PresetSquare>>(PracticeRoom.BoardJSON);
                var squares = PracticeRoom.BoardPreset.GetSquares();
                var rndSquares = squares.OrderBy(x => Guid.NewGuid()).Take(25);
                var boardSqares = new List<Square>();

                var row = 0;
                var col = 0;
                var slot = 1;
                foreach (var square in rndSquares)
                {
                    var s = new Square()
                    {
                        Name = square.Name,
                        Row = row,
                        Column = col,
                        IsMarking = false,
                        Slot = $"slot{slot++}"
                    };
                    s.SquareColors.Add(BingoColor.blank);
                    boardSqares.Add(s);
                    col++;
                    if (col == 5)
                    {
                        col = 0;
                        row++;
                    }
                }
                var nickName = BingoApp.Properties.Settings.Default.NickName ?? App.Current.FindResource("practice").ToString();
                var settingsColor = BingoApp.Properties.Settings.Default.PreferedColor;
                settingsColor = string.IsNullOrEmpty(settingsColor) ? "blue" : settingsColor;
                var defaultColor = Enum.Parse<BingoColor>(settingsColor);
                var player = new Player()
                {
                    IsSpectator = false,
                    Color = defaultColor,
                    NickName = nickName,
                    Uuid = "practice"
                };

                var room = new Room()
                {
                    RoomName = App.Current.FindResource("practice").ToString(),
                    IsPractice = true,
                    IsAutoBoardReveal = false,
                    IsCreatorMode = true,
                    IsRevealed = false,
                    Board = new Board() { Squares = new ObservableCollection<Square>(boardSqares) },
                    RoomSettings = new RoomSettings() { Game = App.Current.FindResource("practice").ToString(), HideCard = true },
                    GameMode = RoomLockoutMode.ToGameMode(PracticeRoom.RoomLockoutMode.Id),
                    CurrentPlayer = player,
                    ChosenColor = defaultColor,
                    BoardJSON = PracticeRoom.BoardJSON
                };
                room.Players.Add(player);

                var notesPath = System.IO.Path.Combine(App.Location, "notes.json");
                if (System.IO.File.Exists(notesPath))
                {
                    var notesJson = await System.IO.File.ReadAllTextAsync(notesPath);
                    var notes = JsonConvert.DeserializeObject<List<SquareNote>>(notesJson);
                    if (notes != null)
                    {
                        foreach (var item in room.Board.Squares)
                        {
                            var note = notes.FirstOrDefault(i => i.SquareName.ToLower() == item.Name.ToLower());
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
                IsConnectModalVisible = false;
                MainWindow.NavigateTo(page);
            }
            catch (Exception ex)
            {
                MainWindow.ShowErrorMessage(ex.Message, nameof(MainPage), nameof(StartPractice));
                Logger.Error(ex);
            }
        }

        async Task ConnectToHub()
        {
            try
            {
                var resp = await App.RestClient.UpdateUserInfoAsync(App.CurrentPlayer);
                await App.SignalRHub.ConnectAsync(BingoApp.Properties.Settings.Default.PlayerId);
                IsDynamicCreateVisible = App.SignalRHub.IsHubConnected;
            }
            catch (Exception ex) { Logger.Error(ex); }
        }

        [RelayCommand]
        void DynamicPresetCreate()
        {
            try
            {
                if (IsUsingPresets)
                {
                    NewRoom.IsBoardJsonError = false;
                    NewRoom.IsBoardPresetError = NewRoom.BoardPreset == null;
                }
                else
                    return;

                if (NewRoom.IsBoardPresetError) return;
                if (NewRoom.BoardPreset == null) return;

                NewRoom.BoardPreset.LoadSquares();
                var squares = NewRoom.BoardPreset.GetSquares();
                var presetSquares = new List<DynamicPresetSquare>();
                var squareId = 0;
                foreach (var item in squares)
                {
                    presetSquares.Add(new() { Id = "sq_" + squareId.ToString(), Name = item.Name });
                    squareId++;
                }

                var json = JsonConvert.SerializeObject(presetSquares, Formatting.None);

                NewRoom.DynamicPresetName = string.Format(App.Current.FindResource("mp_dynamicbasedon").ToString(), NewRoom.BoardPreset.PresetAndGame);
                NewRoom.DynamicGameName = NewRoom.BoardPreset.Game;
                App.NewBoardModel = NewRoom;

                MainWindow.NavigateTo(new DynamicPresetCreatePage()
                {
                    DataContext = new DynamicPresetCreateViewModel()
                    {
                        PresetSquares = presetSquares,
                        Creator = App.CurrentPlayer,
                        IsCreator = true,
                        PresetId = Guid.NewGuid().ToString(),
                        PresetName = NewRoom.BoardPreset.PresetAndGame,
                        PresetJSON = json
                    }
                });
            }
            catch (Exception ex) { Logger.Error(ex); }
        }

        private void SignalRHub_OnInvitePlayerRecieved(object? sender, Classes.BingoAppSignalRHub.InvitePlayerEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                MainWindow.ShowMessage(string.Format(App.Current.FindResource("mes_invitemessage").ToString(), e.NickName, e.PresetName), MessageNotificationType.YesNo,
                async () =>
                {
                    await App.SignalRHub.AcceptInviteAsync(e.PresetId, App.CurrentPlayer.Id);
                    var presetSquares = JsonConvert.DeserializeObject<List<DynamicPresetSquare>>(e.PresetJSON);
                    if (presetSquares != null)
                    {
                        MainWindow.NavigateTo(new DynamicPresetCreatePage()
                        {
                            DataContext = new DynamicPresetCreateViewModel()
                            {
                                PresetSquares = presetSquares,
                                Creator = new BingoAppPlayer() { Id = e.CreatorId, NickName = e.NickName },
                                SecondPlayer = App.CurrentPlayer,
                                IsCreator = false,
                                PresetId = e.PresetId,
                                PresetName = e.PresetName,
                                PresetJSON = e.PresetJSON
                            }
                        });
                    }
                },
                async () =>
                {
                    await App.SignalRHub.RejectInviteAsync(e.PresetId, App.CurrentPlayer.Id);
                });
            });
        }

        private void SignalRHub_OnCancelInviteRecieved(object? sender, string e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                MainWindow.CloseMessage();
            });
        }

        private void SignalRHub_OnGameInviteRecieved(object? sender, Classes.BingoAppSignalRHub.GameInviteEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                MainWindow.ShowMessage(string.Format(App.Current.FindResource("mp_gameinvite").ToString(), e.NickName), MessageNotificationType.YesNo,
                    async () =>
                    {
                        try
                        {
                            var json = Encoding.UTF8.GetString(Convert.FromBase64String(e.Data));
                            var jobj = JObject.Parse(json);
                            var roomId = jobj["r"].Value<string>();
                            var password = jobj["p"].Value<string>();
                            var isAutoReveal = jobj["i"].Value<int>();
                            var gameModeStr = jobj["gm"].Value<string>();
                            var gameExtraModeStr = jobj["egm"].Value<string>();
                            var gamePresetStr = jobj["pr"].Value<string>();
                            var gameStr = jobj["g"].Value<string>();
                            connectRoomGameMode = GameMode.Other;
                            connectRoomExtraGameMode = ExtraGameMode.None;
                            connectRoomPresetName = "Unknown";
                            connectRoomGameName = "Unknown";

                            if (gameModeStr != null)
                            {
                                connectRoomGameMode = Enum.Parse<GameMode>(gameModeStr);
                            }

                            if (gamePresetStr != null)
                            {
                                connectRoomPresetName = gamePresetStr;
                            }
                            if (gameStr != null)
                            {
                                connectRoomGameName = gameStr;
                            }
                            if (gameExtraModeStr != null)
                            {
                                connectRoomExtraGameMode = Enum.Parse<ExtraGameMode>(gameExtraModeStr);
                            }

                            var nickName = BingoApp.Properties.Settings.Default.NickName;

                            MainWindow.ShowModal(new ConnectingToRoomModal(), this);
                            IsModalVisible = true;
                            IsConnectModalVisible = true;
                            IsConnectingToBoardStarted = false;

                            RoomInfo = await (App.Current as App).BingoEngine.GetRoomInfoAsync($"https://bingosync.com/room/{roomId}");
                            RoomInfo.IsFromBingoApp = true;
                            RoomInfo.GameName = connectRoomGameName;
                            RoomInfo.PresetName = connectRoomPresetName;
                            RoomInfo.GameMode = connectRoomGameMode;
                            RoomInfo.GameExtraMode = connectRoomExtraGameMode;
                            RoomInfo.IsFromBingoApp = true;
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
                    });
            });
        }

        private void SignalRHub_OnSharePresetRecieved(object? sender, BingoAppSignalRHub.SharePresetEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                MainWindow.ShowMessage(string.Format(App.Current.FindResource("mes_sharepresetmes").ToString(), e.FromPlayerName, $"{e.GameName} - {e.PresetName}"), MessageNotificationType.YesNo,
                    async () =>
                    {
                        try
                        {
                            MainWindow.CloseMessage();
                            await Task.Delay(500);

                            if (File.Exists(Path.Combine(App.Location, "Presets", e.GameName, e.PresetName + ".json")))
                            {

                                MainWindow.ShowMessage(string.Format(App.Current.FindResource("mes_presetalreadyexist").ToString(), $"{e.GameName} - {e.PresetName}"), MessageNotificationType.YesNo, 
                                async () => {
                                    await DownloadAndSavePresetAsync(e);
                                }, 
                                async () => {
                                    await App.SignalRHub.SendClearSharedPreset(e.SquaresFileName, e.NotesFileName);
                                });
                            }
                            else
                            {
                                await DownloadAndSavePresetAsync(e);
                            }
                            
                        }
                        catch (Exception ex)
                        {

                        }
                    },
                    async () =>
                    {
                        await App.SignalRHub.SendClearSharedPreset(e.SquaresFileName, e.NotesFileName);
                    });
            });
        }

        async Task DownloadAndSavePresetAsync(BingoAppSignalRHub.SharePresetEventArgs e)
        {
            try
            {
                MainWindow.ShowModal(new DownloadPresetModal(), this);
                await Task.Delay(500);

                await (new TaskFactory()).StartNew(async () =>
                {

                    var presetResponse = await App.RestClient.GetSharedPresetSquares(e.SquaresFileName);
                    if (presetResponse.IsSuccess)
                    {
                        if (!Directory.Exists(Path.Combine(App.Location, "Presets", e.GameName)))
                        {
                            Directory.CreateDirectory(Path.Combine(App.Location, "Presets", e.GameName));
                        }
                        await File.WriteAllTextAsync(Path.Combine(App.Location, "Presets", e.GameName, e.PresetName + ".json"), presetResponse.Data.ToString());
                    }

                    var notesResponse = await App.RestClient.GetSharedPresetNotes(e.NotesFileName);
                    if (notesResponse.IsSuccess)
                    {
                        var newNotes = JsonConvert.DeserializeObject<List<SquareNote>>(notesResponse.Data.ToString());
                        if (newNotes != null)
                        {
                            var oldNotes = new List<SquareNote>();
                            var notesPath = System.IO.Path.Combine(App.Location, "notes.json");
                            if (System.IO.File.Exists(notesPath))
                            {
                                var json = await System.IO.File.ReadAllTextAsync(notesPath);
                                if (json != null)
                                {
                                    oldNotes = JsonConvert.DeserializeObject<List<SquareNote>>(json);
                                }
                            }

                            foreach (var item in newNotes)
                            {
                                var note = oldNotes.FirstOrDefault(i => i.SquareName == item.SquareName);
                                if (note == null)
                                {
                                    oldNotes.Add(item);
                                }
                                else
                                {
                                    note.Note = item.Note; // Update existing note
                                }
                            }
                            await System.IO.File.WriteAllTextAsync(notesPath, JsonConvert.SerializeObject(oldNotes, Formatting.Indented));
                        }
                    }
                    await App.Current.Dispatcher.InvokeAsync(async () => {
                        MainWindow.CloseModal();
                        await App.SignalRHub.SendClearSharedPreset(e.SquaresFileName, e.NotesFileName);
                        MainWindow.ShowToast(new ToastInfo() { Title = $"{App.Current.FindResource("mes_success")}", Detail = App.Current.FindResource("mes_presetsharedsuccess").ToString() });

                    });
                });
            }
            catch(Exception ex)
            { }
        }

        private async Task CheckForUpdates()
        {
            try
            {
                var resp = await App.RestClient.GetLastVersionAsync();
                if (resp.IsSuccess)
                {
                    var updateInfo = resp.Data as UpdateInfo;
                    if (updateInfo != null)
                    {
                        if (updateInfo.Version != App.AppVersion)
                        {
                            MainWindow.ShowMessage(App.Current.FindResource("mes_updateapp").ToString(), MessageNotificationType.YesNo, () =>
                            {
                                Process.Start("AppUpdater.exe");
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}
