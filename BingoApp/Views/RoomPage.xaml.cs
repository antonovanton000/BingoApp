using BingoApp.Models;
using BingoApp.ViewModels;
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

namespace BingoApp.Views
{
    /// <summary>
    /// Interaction logic for RoomPage.xaml
    /// </summary>
    public partial class RoomPage : Page
    {
        public RoomPage()
        {
            InitializeComponent();            
        }

        private void RoomPage_EventAdded(object? sender, EventArgs e)
        {
            sv.ScrollToBottom();            
        }

        private void page_Loaded(object sender, RoutedEventArgs e)
        {
            (DataContext as RoomPageViewModel).EventAdded += RoomPage_EventAdded;
            sv.ScrollToBottom();
        }

       
        private void tbxChat_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var vm = this.DataContext as RoomPageViewModel;
                vm.SendChatMessageCommand.Execute(vm);
            }
        }

        private void CopyLink_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            btnChangeColor.IsChecked = false;
        }

        private void ClosePopup_Click(object sender, RoutedEventArgs e)
        {
            btnShare.IsChecked = false;
        }

        private void Grid_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            var grid = (sender as Grid);
            var @event = (grid.DataContext as Event);
            var minutes = Math.Round((DateTime.Now - @event.Timestamp).TotalMinutes, 0);
            var tooltip = (minutes == 0 ? "less than minute ago" :  minutes == 1 ? $"{minutes} minute ago" : $"{minutes} minutes ago");
            grid.ToolTip = tooltip;
        }
    }
}
