using WheelGame.Models;
using WheelGame.ViewModels;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WheelGame.Views
{
    /// <summary>
    /// Interaction logic for RoomPage.xaml
    /// </summary>
    public partial class RoomPage : Page
    {
        private RoomPageViewModel _viewModel = default!;
        
        public RoomPage()
        {
            InitializeComponent();               
        }
        
       
        private void page_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel = (DataContext as RoomPageViewModel);
            if (_viewModel == null)
                return;
            _viewModel.OnRotateWheelRecieved += _viewModel_OnRotateWheelRecieved;
            _viewModel.OnWheelSectorDelete += _viewModel_OnWheelSectorDelete;
        }

        private void _viewModel_OnWheelSectorDelete(object? sender, EventArgs e)
        {
            if (_viewModel.Room.IsEarlyWheelVisible)
                earlyWheel.UpdateWheel();
            if (_viewModel.Room.IsMiddleWheelVisible)
                middleWheel.UpdateWheel();
            if (_viewModel.Room.IsEndWheelVisible)
                endWheel.UpdateWheel();
        }

        private void _viewModel_OnRotateWheelRecieved(object? sender, int angle)
        {
            if (_viewModel.Room.IsEarlyWheelVisible)
                earlyWheel.RotateWheel(angle);
            if (_viewModel.Room.IsMiddleWheelVisible)
                middleWheel.RotateWheel(angle);
            if (_viewModel.Room.IsEndWheelVisible)
                endWheel.RotateWheel(angle);
        }        
    }
}
