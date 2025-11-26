using BingoApp.Classes;
using BingoApp.Models;
using BingoApp.Properties;
using BingoApp.ViewModels;
using Microsoft.Win32;
using Newtonsoft.Json.Bson;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace BingoApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public const string TimerSocketAddress = "bingoapp.injusteam.kz";
        public const string TimerSocketScheme = "https";

        //public const string TimerSocketAddress = "localhost";
        //public const string TimerSocketScheme = "http";

        public static string AppVersion = "v1.4.8";

        public static bool IsUpdateAppShown = false;
        public static RestClient RestClient { get; set; } = new();

        public static BingoAppSignalRHub SignalRHub { get; set; } = new();
        public static BingoAppLocalSignalRHub? LocalSignalRHub { get; set; }
        public static Room? CurrentRoom { get; set; }
        public static LocalWebServer LocalServer { get; set; } = new();

        public static BingoAppPlayer CurrentPlayer => new BingoAppPlayer() { Id = BingoApp.Properties.Settings.Default.PlayerId, NickName = BingoApp.Properties.Settings.Default.NickName };

        private static Logger Logger = default!;

        public static string Location => System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        public static string StartupArgs { get; set; }
        
        public static string? DynamicPresetJSON { get; set; }
        public static NewRoomModel? NewBoardModel { get; set; }
        public bool IsFirstTimeAppear { get; set; } = false;

        private static List<CultureInfo> languages = new List<CultureInfo>();

        public static List<CultureInfo> Languages
        {
            get
            {
                return languages;
            }
        }

        public static event EventHandler LanguageChanged;

        public static CultureInfo Language
        {
            get
            {
                return System.Threading.Thread.CurrentThread.CurrentUICulture;
            }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                if (value == System.Threading.Thread.CurrentThread.CurrentUICulture) return;

                //1. Меняем язык приложения:
                System.Threading.Thread.CurrentThread.CurrentUICulture = value;

                //2. Создаём ResourceDictionary для новой культуры
                ResourceDictionary dict = new ResourceDictionary();
                switch (value.Name)
                {
                    case "ru-RU":
                        dict.Source = new Uri(String.Format("Resources/lang.{0}.xaml", value.Name), UriKind.Relative);
                        break;
                    default:
                        dict.Source = new Uri("Resources/lang.xaml", UriKind.Relative);
                        break;
                }

                //3. Находим старую ResourceDictionary и удаляем его и добавляем новую ResourceDictionary
                ResourceDictionary oldDict = (from d in Application.Current.Resources.MergedDictionaries
                                              where d.Source != null && d.Source.OriginalString.StartsWith("Resources/lang.")
                                              select d).First();
                if (oldDict != null)
                {
                    int ind = Application.Current.Resources.MergedDictionaries.IndexOf(oldDict);
                    Application.Current.Resources.MergedDictionaries.Remove(oldDict);
                    Application.Current.Resources.MergedDictionaries.Insert(ind, dict);
                }
                else
                {
                    Application.Current.Resources.MergedDictionaries.Add(dict);
                }

                //4. Вызываем евент для оповещения всех окон.
                LanguageChanged?.Invoke(Application.Current, new EventArgs());
            }
        }


        public App()
        {
            languages.Clear();
            languages.Add(new CultureInfo("en-US")); 
            languages.Add(new CultureInfo("ru-RU"));
            InitNLog();

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Logger.Fatal(args.ExceptionObject as Exception, "Необработанное исключение!");
            };

            DispatcherUnhandledException += (sender, args) =>
            {
                Logger.Fatal(args.Exception, "Ошибка в UI-потоке!");
                args.Handled = true;
            };
        }

        private void InitNLog()
        {
            var config = new NLog.Config.LoggingConfiguration();
            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = $"errorlog_{DateTime.Now:yyyyMMdd}.log" };
            if (Settings.Default.IsDebug)
            {
                config.AddRule(LogLevel.Info, LogLevel.Info, logfile);
                config.AddRule(LogLevel.Warn, LogLevel.Warn, logfile);
            }
            config.AddRule(LogLevel.Error, LogLevel.Error, logfile);
            config.AddRule(LogLevel.Fatal, LogLevel.Fatal, logfile);

            // Apply config           
            NLog.LogManager.Configuration = config;
            Logger = LogManager.GetCurrentClassLogger();
        }


        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }

        protected override void OnStartup(StartupEventArgs e)
        {            
            base.OnStartup(e);
            Language = new CultureInfo(BingoApp.Properties.Settings.Default.AppLanguage);
            var args = string.Join("; ", e.Args);
            StartupArgs = args;
            RegisterUriScheme();
        }

        const string UriScheme = "bingoapp";
        const string FriendlyName = "BingoApp Protocol";

        public static void RegisterUriScheme()
        {
            using (var key = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Classes\\" + UriScheme))
            {
                // Replace typeof(App) by the class that contains the Main method or any class located in the project that produces the exe.
                // or replace typeof(App).Assembly.Location by anything that gives the full path to the exe
                string applicationLocation = typeof(App).Assembly.Location.Replace(".dll",".exe");

                key.SetValue("", "URL:" + FriendlyName);
                key.SetValue("URL Protocol", "");

                using (var defaultIcon = key.CreateSubKey("DefaultIcon"))
                {
                    defaultIcon.SetValue("", applicationLocation + ",1");
                }

                using (var commandKey = key.CreateSubKey(@"shell\open\command"))
                {
                    commandKey.SetValue("", "\"" + applicationLocation + "\" \"%1\"");
                }
            }
        }
    }
}
