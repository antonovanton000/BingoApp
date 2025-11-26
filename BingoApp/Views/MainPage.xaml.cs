using BingoApp.Classes;
using BingoApp.ModalPopups;
using BingoApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BingoApp.Views
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        private static MainPage instance;

        public MainPage()
        {
            InitializeComponent();
            instance = this;            
        }

        private void Run_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start(
                new ProcessStartInfo
                {
                    FileName = "https://bingosync.com",
                    UseShellExecute = true
                });
        }

        
        public static void CloseActiveRoomsPopup()
        {
            instance.btnActiveRooms.IsChecked = false;
        }

        private void Discord_Click(object sender, MouseButtonEventArgs e)
        {
            Process.Start(
                new ProcessStartInfo
                {
                    FileName = "https://discordapp.com/users/742384872584118434",
                    UseShellExecute = true
                });
        }

        private void Twitch_Click(object sender, MouseButtonEventArgs e)
        {
            Process.Start(
                new ProcessStartInfo
                {
                    FileName = "https://www.twitch.tv/direfullemur0",
                    UseShellExecute = true
                });
        }

        private void Freepik_Click(object sender, MouseButtonEventArgs e)
        {
            Process.Start(
                new ProcessStartInfo
                {
                    FileName = "https://www.freepik.com",
                    UseShellExecute = true
                });
        }

        private void page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!(App.Current as App).IsFirstTimeAppear)
            {
                var sb = FindResource("ActiveRoomAnimation") as Storyboard;
                sb?.Begin();
                (App.Current as App).IsFirstTimeAppear = true;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
