using BingoApp.Classes;
using BingoApp.Models;
using BingoApp.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

namespace BingoApp.ViewModels
{
    public partial class SettingsViewModel : MyBaseViewModel
    {
        MediaPlayer dingPlayer;


        [ObservableProperty]
        string nickName;

        [ObservableProperty]
        BingoColor bingoColor;

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
        [NotifyPropertyChangedFor(nameof(IsSekiroExeSetted))]
        string sekiroExePath = "";

        [ObservableProperty]
        bool isModCopied = false;

        [ObservableProperty]
        bool isModUpToDate = false;

        [ObservableProperty]
        bool isModDownloading = false;

        [ObservableProperty]
        string modVersion = "";

        public bool IsSekiroExeSetted => !string.IsNullOrEmpty(SekiroExePath);

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


        [RelayCommand]
        async Task Appearing()
        {
            MainWindow.HideSettingsButton();
            (App.Current.MainWindow as MainWindow).frame.Navigating += Frame_Navigating;

            this.Version = App.AppVersion;

            GetSettings();
            SettingsPage.SetPreferedColor(BingoColor);

            CheckModCopied();
            await CheckModUpToDate();

            dingPlayer = new MediaPlayer();
            dingPlayer.Volume = SoundsVolume * 0.01d;
            dingPlayer.MediaEnded += (s, e) =>
            {
                dingPlayer.Stop();
                dingPlayer.Close();
                dingPlayer.Position = TimeSpan.Zero;
            };

            this.PropertyChanged += SettingsViewModel_PropertyChanged;

            LocalServerLinks = "";
            foreach (var ip in LocalWebServer.GetIPAddresses())
            {
                LocalServerLinks += $"http://{ip}:{LocalServerPort}\r\n";
            }
        }

        private void SettingsViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LocalServerPort) || e.PropertyName == nameof(IsStartLocalServer))
            {
                LocalServerLinks = "";
                foreach (var ip in LocalWebServer.GetIPAddresses())
                {
                    LocalServerLinks += $"http://{ip}:{LocalServerPort}\r\n";
                }

                if (IsStartLocalServer)
                {
                    WindowsFirewallHelper.OpenPortNetsh(LocalServerPort);
                }

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
            BingoColor = (BingoColor)Enum.Parse(typeof(BingoColor), Properties.Settings.Default.PreferedColor);
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
            CurLanguage = Languages.First(i => i.Culture.Name == BingoApp.Properties.Settings.Default.AppLanguage);
            AfterRevealTimeChanging = Properties.Settings.Default.BoardAnalyzeTimeChanging;
            AfterRevealTimeHidden = Properties.Settings.Default.BoardAnalyzeTimeHidden;
            ChangingSquareTime = Properties.Settings.Default.BoardChangeSqaureTime;
            UnhideSquareTime = Properties.Settings.Default.BoardUnhideSqaresTime;
            LocalServerPort = Properties.Settings.Default.LocalServerPort;
            IsStartLocalServer = Properties.Settings.Default.IsStartLocalServer;
            SekiroExePath = Properties.Settings.Default.SekiroExePath;
        }

        void SaveSettings()
        {
            Properties.Settings.Default.NickName = NickName;
            Properties.Settings.Default.PreferedColor = BingoColor.ToString();
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
            Properties.Settings.Default.SekiroExePath = SekiroExePath;

            BingoApp.Properties.Settings.Default.Save();
        }

        void LanguageChanged()
        {
            App.Language = CurLanguage.Culture;
        }

        public class AppLanguage
        {
            public string LanguageName { get; set; } = null!;

            public CultureInfo Culture { get; set; } = null!;
        }


        [RelayCommand]
        void SelectSekiroExe()
        {
            var fo = new OpenFileDialog() { Filter = "Sekiro|sekiro.exe", DefaultDirectory = @"C:\Program Files (x86)\Steam\steamapps\common\Sekiro", FileName = "sekiro.exe" };
            if (fo.ShowDialog() == true)
            {
                SekiroExePath = fo.FileName;
            }
        }


        [RelayCommand]
        async Task DownloadMod()
        {
            var tf = new TaskFactory();
            await tf.StartNew(async () =>
            {
                IsModDownloading = true;
                try
                {
                    var serverModVersionResp = await App.RestClient.GetModLastVersionAsync();
                    if (serverModVersionResp.IsSuccess)
                    {
                        var httpClient = new HttpClient();
                        var link = RestClient.baseUri + serverModVersionResp.Data.Link;
                        using var s = await httpClient.GetStreamAsync(link);
                        using var fs = new FileStream($"_bingomod.download", FileMode.Create);
                        await s.CopyToAsync(fs);
                        fs.Close();
                        s.Close();
                        var sekiroFolderPath = System.IO.Path.GetDirectoryName(SekiroExePath);

                        using var archive = System.IO.Compression.ZipFile.Open($"_bingomod.download", System.IO.Compression.ZipArchiveMode.Read);
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
                        File.WriteAllText(Path.Combine(sekiroFolderPath, "bingomod", "version.txt"), serverModVersionResp.Data.Version);
                        File.Delete($"_bingomod.download");
                        await CheckModUpToDate();
                        IsModDownloading = false;
                        IsModCopied = true;
                    }
                }
                catch (Exception)
                {
                    MainWindow.ShowToast(new ToastInfo() { Title = App.Current.FindResource("mes_error").ToString(), Detail = App.Current.FindResource("mes_errorhappend").ToString() });
                }
            });
        }

        [RelayCommand]
        async Task CopyMod()
        {
            try
            {
                if (string.IsNullOrEmpty(SekiroExePath))
                    return;

                var sekiroFolderPath = System.IO.Path.GetDirectoryName(SekiroExePath);
                if (string.IsNullOrEmpty(sekiroFolderPath))
                    return;

                var sourcePath = System.IO.Path.Combine(App.Location, "Sekiro", "bingomod");
                System.IO.File.Copy(System.IO.Path.Combine(App.Location, "Sekiro", "modengine.ini"), System.IO.Path.Combine(sekiroFolderPath, "modengine.ini"));
                System.IO.File.Copy(System.IO.Path.Combine(App.Location, "Sekiro", "dinput8.dll"), System.IO.Path.Combine(sekiroFolderPath, "dinput8.dll"));
                await DownloadMod();

                IsModCopied = true;
            }
            catch (Exception ex)
            {
                MainWindow.ShowToast(new ToastInfo() { Title = App.Current.FindResource("mes_error").ToString(), Detail = App.Current.FindResource("mes_errorhappend").ToString() });
            }
        }

        void CheckModCopied()
        {
            if (IsSekiroExeSetted)
            {
                if (string.IsNullOrEmpty(SekiroExePath))
                    return;

                var sekiroFolderPath = System.IO.Path.GetDirectoryName(SekiroExePath);
                if (string.IsNullOrEmpty(sekiroFolderPath))
                    return;

                var modPath = System.IO.Path.Combine(sekiroFolderPath, "bingomod");
                var dllPath = System.IO.Path.Combine(sekiroFolderPath, "dinput8.dll");
                var iniPath = System.IO.Path.Combine(sekiroFolderPath, "modengine.ini");

                if (System.IO.Directory.Exists(modPath) && System.IO.File.Exists(dllPath) && System.IO.File.Exists(iniPath))
                {
                    IsModCopied = true;
                }
                else
                {
                    IsModCopied = false;
                }
            }
        }

        async Task CheckModUpToDate()
        {
            if (!IsSekiroExeSetted)
                return;

            try
            {
                IsModUpToDate = false;
                ModVersion = "???";
                var sekiroFolderPath = System.IO.Path.GetDirectoryName(SekiroExePath);
                if (string.IsNullOrEmpty(sekiroFolderPath))
                    return;

                var modPath = System.IO.Path.Combine(sekiroFolderPath, "bingomod");
                if (!System.IO.Directory.Exists(modPath))
                    return;

                var modVersionPath = System.IO.Path.Combine(modPath, "version.txt");
                if (!System.IO.File.Exists(modVersionPath))
                    return;

                ModVersion = System.IO.File.ReadAllText(modVersionPath).Trim();
                var serverModVersionResp = await App.RestClient.GetModLastVersionAsync();
                if (serverModVersionResp.IsSuccess)
                {
                    if (serverModVersionResp.Data.Version.Trim() == ModVersion)
                    {
                        IsModUpToDate = true;
                    }
                }
            }
            catch (Exception)
            {
                //MainWindow.ShowToast(new ToastInfo() { Title = App.Current.FindResource("mes_error").ToString(), Detail = App.Current.FindResource("mes_errorhappend").ToString() });
            }
        }

        [RelayCommand]
        void RemoveMod()
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

                var modFolderPath = System.IO.Path.Combine(sekiroFolderPath, "bingomod");
                if (System.IO.Directory.Exists(modFolderPath))
                    System.IO.Directory.Delete(modFolderPath, true);

                IsModCopied = false;
            }
            catch (Exception ex)
            {
                MainWindow.ShowToast(new ToastInfo() { Title = App.Current.FindResource("mes_error").ToString(), Detail = ex.Message });
            }
        }


        private static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }
    }
}
