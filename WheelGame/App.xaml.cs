using System.Configuration;
using System.Data;
using System.Windows;
using WheelGame.Classes;
using WheelGame.Models;

namespace WheelGame
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

        public static string AppVersion = "v0.0.1";
        
        public static bool IsUpdateAppShown = false;

        public static RestClient RestClient { get; set; } = new();

        public static WheelSignalRHub SignalRHub { get; set; } = new();

        public static string Location => System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        public static WheelAppPlayer CurrentPlayer => new WheelAppPlayer() { Id = WheelGame.Properties.Settings.Default.PlayerId, NickName = WheelGame.Properties.Settings.Default.NickName };
    }

}
