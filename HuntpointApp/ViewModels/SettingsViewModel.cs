using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HuntpointApp.Classes;
using HuntpointApp.Models;
using HuntpointApp.Views;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace HuntpointApp.ViewModels
{
    public partial class SettingsViewModel : MyBaseViewModel
    {
        MediaPlayer dingPlayer;


        [ObservableProperty]
        string nickName;

        [ObservableProperty]
        HuntpointColor huntpointColor;

        [ObservableProperty]
        string defaultRoomName;

        [ObservableProperty]
        string defaultPassword;

        [ObservableProperty]
        int startingTime;

        [ObservableProperty]
        int afterRevealTime;

        [ObservableProperty]
        int afterRevealTimeChanging;

        [ObservableProperty]
        int afterRevealTimeHidden;

        [ObservableProperty]
        int changingSquareTime;

        [ObservableProperty]
        int unhideSquareTime;

        [ObservableProperty]
        bool feedPlayerChat;

        [ObservableProperty]
        bool feedGoalActions;

        [ObservableProperty]
        bool feedColorChanged;

        [ObservableProperty]
        bool feedConnections;

        [ObservableProperty]
        bool isSoundsOn;

        [ObservableProperty]
        int soundsVolume;

        [ObservableProperty]
        int localServerPort;

        [ObservableProperty]
        bool isStartLocalServer;

        [ObservableProperty]
        string version;

        [ObservableProperty]
        string localServerLinks = "";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNightreignExeSetted))]
        string nightreignPath = "";

        [ObservableProperty]
        bool isModCopied = false;

        [ObservableProperty]
        bool isModEngineInstalled = false;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsSteamIdSetted))]
        string steamPlayerId = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsSteamIdSetted))]
        string steamPlayerName = string.Empty;

        public bool IsNightreignExeSetted => !string.IsNullOrEmpty(NightreignPath);
        public bool IsSteamIdSetted => !string.IsNullOrEmpty(SteamPlayerId);

        AppLanguage curLanguage;
        public AppLanguage CurLanguage { get => curLanguage; set { curLanguage = value; OnPropertyChanged(); LanguageChanged(); } }

        [ObservableProperty]
        List<AppLanguage> languages = new() {
            new AppLanguage() { LanguageName = "English", Culture = new CultureInfo("en-US")},
            new AppLanguage() { LanguageName = "Русский", Culture = new CultureInfo("ru-RU")}
        };

        [ObservableProperty]
        bool isDebug;

        private Uri revealSound = new Uri(System.Environment.CurrentDirectory + "\\Sounds\\reveal.wav", UriKind.Absolute);

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsSekiroExeSetted))]
        string sekiroExePath = "";

        [ObservableProperty]
        bool isSekiroModCopied = false;

        [ObservableProperty]
        bool isSekiroModUpToDate = false;

        [ObservableProperty]
        bool isSekiroModDownloading = false;

        [ObservableProperty]
        string sekiroModVersion = "";

        [ObservableProperty]
        string sekiroServerModVersion = "";

        public bool IsSekiroExeSetted => !string.IsNullOrEmpty(SekiroExePath);

        [RelayCommand]
        async Task Appearing()
        {
            MainWindow.HideSettingsButton();
            (App.Current.MainWindow as MainWindow).frame.Navigating += Frame_Navigating;

            this.Version = App.AppVersion;

            GetSettings();
            SettingsPage.SetPreferedColor(HuntpointColor);
            CheckModCopied();
            CheckSekiroModCopied();
            CheckModEngineInstalled();
            await CheckSekiroModUpToDate();

            dingPlayer = new MediaPlayer();
            dingPlayer.Volume = SoundsVolume * 0.01d;
            dingPlayer.MediaEnded += (s, e) =>
            {
                dingPlayer.Stop();
                dingPlayer.Close();
                dingPlayer.Position = TimeSpan.Zero;
            };

            this.PropertyChanged += SettingsViewModel_PropertyChanged;

            //LocalServerLinks = "";
            //foreach (var ip in LocalWebServer.GetIPAddresses())
            //{
            //    LocalServerLinks += $"http://{ip}:{LocalServerPort}\r\n";
            //}
        }

        private void SettingsViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LocalServerPort) || e.PropertyName == nameof(IsStartLocalServer))
            {
                LocalServerLinks = "";
                //foreach (var ip in LocalWebServer.GetIPAddresses())
                //{
                //    LocalServerLinks += $"http://{ip}:{LocalServerPort}\r\n";
                //}

                //if (IsStartLocalServer)
                //{
                //    WindowsFirewallHelper.OpenPortNetsh(LocalServerPort);
                //}

            }
        }

        [RelayCommand]
        void TestSound()
        {
            try
            {
                dingPlayer.Open(revealSound);
                dingPlayer.Volume = SoundsVolume * 0.01d;
                dingPlayer.Play();

                if (IsDebug)
                {
                    var debugMessage = $"Reveal sound path: {revealSound}\r\nSoundsVolume: {SoundsVolume}\r\nPlayer.Volume: {dingPlayer.Volume}\r\n----------------------\r\n";
                    System.IO.File.AppendAllText(System.IO.Path.Combine(App.Location, "logs.txt"), debugMessage);
                }
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText(System.IO.Path.Combine(App.Location, "errors.txt"), $"Error: {ex.Message}\r\n-------------------\r\n");
            }
        }

        private void Frame_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == System.Windows.Navigation.NavigationMode.Back)
            {
                SaveSettings();
                (App.Current.MainWindow as MainWindow).frame.Navigating -= Frame_Navigating;
            }
        }

        void GetSettings()
        {
            NickName = Properties.Settings.Default.NickName;
            HuntpointColor = (HuntpointColor)Enum.Parse(typeof(HuntpointColor), Properties.Settings.Default.PreferedColor);
            DefaultRoomName = Properties.Settings.Default.DefaultRoomName;
            DefaultPassword = Properties.Settings.Default.DefaultPassword;
            StartingTime = Properties.Settings.Default.BeforeStartTime;
            AfterRevealTime = Properties.Settings.Default.BoardAnalyzeTime;
            FeedPlayerChat = Properties.Settings.Default.IsPlayerChat;
            FeedGoalActions = Properties.Settings.Default.IsGoalActions;
            FeedColorChanged = Properties.Settings.Default.IsColorChanged;
            FeedConnections = Properties.Settings.Default.IsConnections;
            IsSoundsOn = Properties.Settings.Default.IsSoundsOn;
            IsDebug = Properties.Settings.Default.IsDebug;
            SoundsVolume = Properties.Settings.Default.SoundsVolume;
            CurLanguage = Languages.First(i => i.Culture.Name == HuntpointApp.Properties.Settings.Default.AppLanguage);
            AfterRevealTimeChanging = Properties.Settings.Default.BoardAnalyzeTimeChanging;
            AfterRevealTimeHidden = Properties.Settings.Default.BoardAnalyzeTimeHidden;
            ChangingSquareTime = Properties.Settings.Default.BoardChangeSqaureTime;
            UnhideSquareTime = Properties.Settings.Default.BoardUnhideSqaresTime;
            LocalServerPort = Properties.Settings.Default.LocalServerPort;
            IsStartLocalServer = Properties.Settings.Default.IsStartLocalServer;
            NightreignPath = Properties.Settings.Default.NightreignExePath;
            SteamPlayerId = Properties.Settings.Default.SteamPlayerId;
            SteamPlayerName = Properties.Settings.Default.SteamPlayerName;
            SekiroExePath = Properties.Settings.Default.SekiroExePath;
        }

        void SaveSettings()
        {
            Properties.Settings.Default.NickName = NickName;
            Properties.Settings.Default.PreferedColor = HuntpointColor.ToString();
            Properties.Settings.Default.DefaultRoomName = DefaultRoomName;
            Properties.Settings.Default.DefaultPassword = DefaultPassword;
            Properties.Settings.Default.BeforeStartTime = StartingTime;
            Properties.Settings.Default.BoardAnalyzeTime = AfterRevealTime;
            Properties.Settings.Default.IsPlayerChat = FeedPlayerChat;
            Properties.Settings.Default.IsGoalActions = FeedGoalActions;
            Properties.Settings.Default.IsColorChanged = FeedColorChanged;
            Properties.Settings.Default.IsConnections = FeedConnections;
            Properties.Settings.Default.SoundsVolume = SoundsVolume;
            Properties.Settings.Default.IsSoundsOn = IsSoundsOn;
            Properties.Settings.Default.IsDebug = IsDebug;
            Properties.Settings.Default.AppLanguage = CurLanguage.Culture.Name;
            Properties.Settings.Default.BoardAnalyzeTimeChanging = AfterRevealTimeChanging;
            Properties.Settings.Default.BoardAnalyzeTimeHidden = AfterRevealTimeHidden;
            Properties.Settings.Default.BoardChangeSqaureTime = ChangingSquareTime;
            Properties.Settings.Default.BoardUnhideSqaresTime = UnhideSquareTime;
            Properties.Settings.Default.LocalServerPort = LocalServerPort;
            Properties.Settings.Default.IsStartLocalServer = IsStartLocalServer;
            Properties.Settings.Default.NightreignExePath = NightreignPath;
            Properties.Settings.Default.SteamPlayerId = SteamPlayerId;
            Properties.Settings.Default.SteamPlayerName = SteamPlayerName;
            Properties.Settings.Default.SekiroExePath = SekiroExePath;
            HuntpointApp.Properties.Settings.Default.Save();
        }

        void LanguageChanged()
        {
            App.Language = CurLanguage.Culture;
        }

        #region Nightreign

        [RelayCommand]
        void SelectNightreignExe()
        {
            var fo = new OpenFileDialog() { Filter = "Nightreign|nightreign.exe", FileName = @"C:\Program Files (x86)\Steam\steamapps\common\ELDEN RING NIGHTREIGN\Game\nightreign.exe" };
            if (fo.ShowDialog() == true)
            {
                NightreignPath = fo.FileName;
            }
        }

        [RelayCommand]
        void OpenModEngineDownload()
        {
            Process.Start(
               new ProcessStartInfo
               {
                   FileName = "https://me3.help/en/latest/",
                   UseShellExecute = true
               });            
        }

        [RelayCommand]
        void OpenSaveFolder()
        {
            try
            {
                var saveFolderPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nightreign");
                Process.Start("explorer.exe", saveFolderPath);
            }
            catch (Exception ex)
            {

            }
        }

        void CheckModCopied()
        {
            if (IsNightreignExeSetted)
            {
                if (string.IsNullOrEmpty(NightreignPath))
                    return;

                var nightreignFolderPath = System.IO.Path.GetDirectoryName(NightreignPath);
                if (string.IsNullOrEmpty(nightreignFolderPath))
                    return;

                var me3Path = System.IO.Path.Combine(nightreignFolderPath, "nightreign-with-helper.me3");
                var dllPath = System.IO.Path.Combine(nightreignFolderPath, "nighreign-helper", "NightreignRandomizerHelper.dll");
                var iniPath = System.IO.Path.Combine(nightreignFolderPath, "nighreign-helper", "NightreignRandomizerHelper_config.ini");

                if (System.IO.File.Exists(me3Path) && System.IO.File.Exists(dllPath) && System.IO.File.Exists(iniPath))
                {
                    IsModCopied = true;
                }
                else
                {
                    IsModCopied = false;
                }
            }
        }

        void CheckModEngineInstalled()
        {
            if (IsNightreignExeSetted)
            {
                try
                {
                    IsModEngineInstalled = false;
                    Process process = new Process();
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    startInfo.RedirectStandardOutput = true;
                    startInfo.FileName = "cmd.exe";
                    startInfo.Arguments = $"/C me3 info -V";
                    process.StartInfo = startInfo;
                    process.Start();
                    var output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    
                    if (output.Contains("me3-info"))
                    {
                        IsModEngineInstalled = true;
                    }
                    else
                    {
                        IsModEngineInstalled = false;
                    }
                }
                catch (Exception ex)
                {
                    IsModEngineInstalled = false;
                }
            }
        }

        [RelayCommand]
        void RemoveMod()
        {
            try
            {
                if (string.IsNullOrEmpty(NightreignPath))
                    return;
                var nightreignFolderPath = System.IO.Path.GetDirectoryName(NightreignPath);
                if (string.IsNullOrEmpty(nightreignFolderPath))
                    return;

                var me3Path = System.IO.Path.Combine(nightreignFolderPath, "nightreign-with-helper.me3");
                if (System.IO.File.Exists(me3Path))
                    System.IO.File.Delete(me3Path);

                var modFolderPath = System.IO.Path.Combine(nightreignFolderPath, "nighreign-helper");
                if (System.IO.Directory.Exists(modFolderPath))
                    System.IO.Directory.Delete(modFolderPath, true);
                IsModCopied = false;
            }
            catch (Exception ex)
            {
                MainWindow.ShowToast(new ToastInfo() { Title = App.Current.FindResource("mes_error").ToString(), Detail = ex.Message });
            }
        }


        [RelayCommand]
        void CopyMod()
        {
            try
            {
                if (string.IsNullOrEmpty(NightreignPath))
                    return;

                var nightreignFolderPath = System.IO.Path.GetDirectoryName(NightreignPath);
                if (string.IsNullOrEmpty(nightreignFolderPath))
                    return;

                System.IO.File.Copy(System.IO.Path.Combine(App.Location, "NightreignHelper", "nightreign-with-helper.me3"), System.IO.Path.Combine(nightreignFolderPath, "nightreign-with-helper.me3"), true);

                var modFolderPath = System.IO.Path.Combine(nightreignFolderPath, "nighreign-helper");
                if (!System.IO.Directory.Exists(modFolderPath))
                    System.IO.Directory.CreateDirectory(modFolderPath);

                System.IO.File.Copy(System.IO.Path.Combine(App.Location, "NightreignHelper", "nighreign-helper", "NightreignRandomizerHelper.dll"),
                    System.IO.Path.Combine(modFolderPath, "NightreignRandomizerHelper.dll"), true);

                System.IO.File.Copy(System.IO.Path.Combine(App.Location, "NightreignHelper", "nighreign-helper", "NightreignRandomizerHelper_config.ini"),
                    System.IO.Path.Combine(modFolderPath, "NightreignRandomizerHelper_config.ini"), true);

                IsModCopied = true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowToast(new ToastInfo() { Title = App.Current.FindResource("mes_error").ToString(), Detail = App.Current.FindResource("mes_errorhappend").ToString() });
            }
        }


        [RelayCommand]
        async Task CreateSaveBackup()
        {
            var saveFolderPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nightreign");
            if (!System.IO.Directory.Exists(saveFolderPath))
            {
                MainWindow.ShowToast(new ToastInfo() { Title = App.Current.FindResource("mes_warning").ToString(), Detail = App.Current.FindResource("mes_nosavefilefolder").ToString(), ToastType=ToastType.Warning });                
                return;
            }

            if (string.IsNullOrEmpty(Properties.Settings.Default.SteamPlayerId))
            {
                var directories = System.IO.Directory.GetDirectories(saveFolderPath);
                if (directories.Length == 0)
                {
                    MainWindow.ShowToast(new ToastInfo() { Title = App.Current.FindResource("mes_warning").ToString(), Detail = App.Current.FindResource("mes_nosavefilesinfolder").ToString(), ToastType = ToastType.Warning });                    
                    return;
                }
                if (directories.Length == 1)
                {
                    SteamPlayerId = System.IO.Path.GetFileName(directories[0]);
                    SteamPlayerName = await SteamHelper.GetUserNameById(SteamPlayerId);
                   
                    CopySaveFile(SteamPlayerId);
                }
                else
                {
                    var message = App.Current.FindResource("mes_morethanoneprofile").ToString();
                    var listUsers = new List<KeyValuePair<string, string>>();
                    foreach (var item in directories)
                    {
                        var key = System.IO.Path.GetFileName(item);
                        var value = await SteamHelper.GetUserNameById(key);
                        listUsers.Add(new KeyValuePair<string, string>(key, value));
                    }
                    message += "\r\n" + string.Join("\r\n", listUsers.Select(i => $"{i.Key} - {i.Value}"));
                    MainWindow.InputDialog(message, async (steamUserId) => {
                        steamUserId = steamUserId.Trim();
                        var steamUserName = await SteamHelper.GetUserNameById(steamUserId);
                        SteamPlayerId = steamUserId;
                        SteamPlayerName = steamUserName;
                        
                        CopySaveFile(steamUserId);
                    }, placeholder: "Steam ID");
                }
            }
            else
            {
                var steamUserId = Properties.Settings.Default.SteamPlayerId;
                CopySaveFile(steamUserId);
            }
        }

        void CopySaveFile(string steamUserId)
        {
            var saveFolderPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nightreign");
            var saveFileName = System.IO.Path.Combine(saveFolderPath, steamUserId, "NR0000.sl2");
            var newsaveFileName = System.IO.Path.Combine(saveFolderPath, steamUserId, "NR0000.co2");
            if (!System.IO.File.Exists(saveFileName))
            {
                MainWindow.ShowToast(new ToastInfo() { Title = App.Current.FindResource("mes_warning").ToString(), Detail = App.Current.FindResource("mes_nosavefilesinfolder").ToString(), ToastType = ToastType.Warning });                
                return;
            }
            System.IO.File.Copy(saveFileName, newsaveFileName, true);
            MainWindow.ShowToast(new ToastInfo() { Title = App.Current.FindResource("mes_success").ToString(), Detail = App.Current.FindResource("mes_savefilecopied").ToString() });
        }

        [RelayCommand]
        void CleanSteamPlayer()
        {
            SteamPlayerId = string.Empty;
            SteamPlayerName = string.Empty;
            Properties.Settings.Default.SteamPlayerId = string.Empty;
            Properties.Settings.Default.SteamPlayerName = string.Empty;
            Properties.Settings.Default.Save();
        }

        #endregion

        #region Sekiro
        [RelayCommand]
        void SelectSekiroExe()
        {
            var fo = new OpenFileDialog() { 
                Filter = "Sekiro|sekiro.exe", 
                InitialDirectory = @"C:\Program Files (x86)\Steam\steamapps\common\Sekiro", 
                FileName = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Sekiro\\sekiro.exe"
            };
            fo.RestoreDirectory = true;
            if (fo.ShowDialog() == true)
            {
                SekiroExePath = fo.FileName;
            }
            CheckSekiroModCopied();
        }


        [RelayCommand]
        async Task DownloadSekiroMod()
        {
            var tf = new TaskFactory();
            await tf.StartNew(async () =>
            {
                IsSekiroModDownloading = true;
                try
                {
                    var serverModVersionResp = await App.RestClient.GetModLastVersionAsync();
                    if (serverModVersionResp.IsSuccess)
                    {
                        var httpClient = new HttpClient();
                        var link = RestClient.baseUri + serverModVersionResp.Data.Link;
                        using var s = await httpClient.GetStreamAsync(link);
                        using var fs = new FileStream($"_huntpointmod.download", FileMode.Create);
                        await s.CopyToAsync(fs);
                        fs.Close();
                        s.Close();
                        var sekiroFolderPath = System.IO.Path.GetDirectoryName(SekiroExePath);

                        using var archive = System.IO.Compression.ZipFile.Open($"_huntpointmod.download", System.IO.Compression.ZipArchiveMode.Read);
                        foreach (var item in archive.Entries)
                        {
                            var folderName = System.IO.Path.GetDirectoryName(item.FullName);
                            var destFolderPath = Path.Combine(sekiroFolderPath, folderName);
                            if (item.FullName.Contains(@"/"))
                            {
                                if (!Directory.Exists(destFolderPath))
                                    Directory.CreateDirectory(destFolderPath);
                            }
                            if (item.Length > 0)
                                item.ExtractToFile(Path.Combine(sekiroFolderPath, item.FullName), true);
                        }
                        archive.Dispose();
                        File.WriteAllText(Path.Combine(sekiroFolderPath, "huntpointmod", "version.txt"), serverModVersionResp.Data.Version);
                        File.Delete($"_huntpointmod.download");
                        await CheckSekiroModUpToDate();
                        IsSekiroModDownloading = false;
                        IsSekiroModCopied = true;
                    }
                }
                catch (Exception)
                {
                    MainWindow.ShowToast(new ToastInfo() { Title = App.Current.FindResource("mes_error").ToString(), Detail = App.Current.FindResource("mes_errorhappend").ToString() });
                }
            });
        }

        [RelayCommand]
        async Task CopySekiroMod()
        {
            try
            {
                if (string.IsNullOrEmpty(SekiroExePath))
                    return;

                var sekiroFolderPath = System.IO.Path.GetDirectoryName(SekiroExePath);
                if (string.IsNullOrEmpty(sekiroFolderPath))
                    return;

                var sourcePath = System.IO.Path.Combine(App.Location, "Sekiro", "huntpointmod");
                System.IO.File.Copy(System.IO.Path.Combine(App.Location, "Sekiro", "modengine.ini"), System.IO.Path.Combine(sekiroFolderPath, "modengine.ini"));
                System.IO.File.Copy(System.IO.Path.Combine(App.Location, "Sekiro", "dinput8.dll"), System.IO.Path.Combine(sekiroFolderPath, "dinput8.dll"));
                await DownloadSekiroMod();

                IsSekiroModCopied = true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowToast(new ToastInfo() { Title = App.Current.FindResource("mes_error").ToString(), Detail = App.Current.FindResource("mes_errorhappend").ToString() });
            }
        }

        void CheckSekiroModCopied()
        {
            if (IsSekiroExeSetted)
            {
                if (string.IsNullOrEmpty(SekiroExePath))
                    return;

                var sekiroFolderPath = System.IO.Path.GetDirectoryName(SekiroExePath);
                if (string.IsNullOrEmpty(sekiroFolderPath))
                    return;

                var modPath = System.IO.Path.Combine(sekiroFolderPath, "huntpointmod");
                var dllPath = System.IO.Path.Combine(sekiroFolderPath, "dinput8.dll");
                var iniPath = System.IO.Path.Combine(sekiroFolderPath, "modengine.ini");

                if (System.IO.Directory.Exists(modPath) && System.IO.File.Exists(dllPath) && System.IO.File.Exists(iniPath))
                {
                    IsSekiroModCopied = true;
                    SekiroModVersion = "???";
                    var modVersionPath = System.IO.Path.Combine(modPath, "version.txt");
                    if (!System.IO.File.Exists(modVersionPath))
                        return;

                    SekiroModVersion = System.IO.File.ReadAllText(modVersionPath).Trim();

                }
                else
                {
                    IsSekiroModCopied = false;
                }
            }
        }

        async Task CheckSekiroModUpToDate()
        {
            if (!IsSekiroExeSetted)
                return;

            try
            {
                IsSekiroModUpToDate = false;
                SekiroModVersion = "???";
                var sekiroFolderPath = System.IO.Path.GetDirectoryName(SekiroExePath);
                if (string.IsNullOrEmpty(sekiroFolderPath))
                    return;

                var modPath = System.IO.Path.Combine(sekiroFolderPath, "huntpointmod");
                if (!System.IO.Directory.Exists(modPath))
                    return;

                var modVersionPath = System.IO.Path.Combine(modPath, "version.txt");
                if (!System.IO.File.Exists(modVersionPath))
                    return;

                SekiroModVersion = System.IO.File.ReadAllText(modVersionPath).Trim();
                var serverModVersionResp = await App.RestClient.GetModLastVersionAsync();
                if (serverModVersionResp.IsSuccess)
                {
                    SekiroServerModVersion = serverModVersionResp.Data.Version;
                    if (serverModVersionResp.Data.Version.Trim() == SekiroModVersion)
                    {
                        IsSekiroModUpToDate = true;
                    }
                }
            }
            catch (Exception)
            {
                //MainWindow.ShowToast(new ToastInfo() { Title = App.Current.FindResource("mes_error").ToString(), Detail = App.Current.FindResource("mes_errorhappend").ToString() });
            }
        }

        [RelayCommand]
        void RemoveSekiroMod()
        {
            try
            {
                if (string.IsNullOrEmpty(SekiroExePath))
                    return;

                var sekiroFolderPath = System.IO.Path.GetDirectoryName(SekiroExePath);
                if (string.IsNullOrEmpty(sekiroFolderPath))
                    return;

                var dllPath = System.IO.Path.Combine(sekiroFolderPath, "dinput8.dll");
                if (System.IO.File.Exists(dllPath))
                    System.IO.File.Delete(dllPath);

                var iniPath = System.IO.Path.Combine(sekiroFolderPath, "modengine.ini");
                if (System.IO.File.Exists(iniPath))
                    System.IO.File.Delete(iniPath);

                var modFolderPath = System.IO.Path.Combine(sekiroFolderPath, "huntpointmod");
                if (System.IO.Directory.Exists(modFolderPath))
                    System.IO.Directory.Delete(modFolderPath, true);

                IsSekiroModCopied = false;
            }
            catch (Exception ex)
            {
                MainWindow.ShowToast(new ToastInfo() { Title = App.Current.FindResource("mes_error").ToString(), Detail = ex.Message });
            }
        }

        #endregion
    }
}
