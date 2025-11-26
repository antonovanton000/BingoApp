using HuntpointApp.Models;
using HuntpointApp.ViewModels;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HuntpointApp.Views
{
    /// <summary>
    /// Interaction logic for RoomPage.xaml
    /// </summary>
    public partial class RoomHistoryPage : Page
    {
        private RoomHistoryViewModel _viewModel = default!;
        
        public RoomHistoryPage()
        {
            InitializeComponent();               
        }

        private void CreateBoardShot_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CloseInvitePopup_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ClosePopup_Click(object sender, RoutedEventArgs e)
        {
            btnShare.IsChecked = false;
        }
    }
}
