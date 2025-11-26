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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HuntpointApp.Views
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
        
        private void RoomPage_EventAdded(object? sender, EventArgs e)
        {
            sv.ScrollToBottom();            
        }

        private void page_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel = (DataContext as RoomPageViewModel);
            if (_viewModel == null)
                return;

            _viewModel.EventAdded += RoomPage_EventAdded;
            _viewModel.BingoHappened += (s,ee) => {
                bingoAnimation.PlayAnimation();
            };
            sv.ScrollToBottom();

            if (_viewModel.IsNightreignGame)
            {
                if (_viewModel.Room.NightreignMapPattern != null)
                {
                    mps.SetSpecificPattern((int)_viewModel.Room.NightreignMapPattern);
                }
            }

            App.Current.MainWindow.SizeChanged += (s,e)=> {
                _viewModel.CloseMapPatternSelectCommand.Execute(null);  
            };

            App.Current.MainWindow.LocationChanged += (s, e) => {
                _viewModel.CloseMapPatternSelectCommand.Execute(null);
            };
            
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void tbxChat_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _viewModel.SendChatMessageCommand.Execute(_viewModel);
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
            var tooltip = (minutes == 0 ? App.Current.FindResource("mes_lessthanminutes").ToString() : minutes == 1 ?
                string.Format(App.Current.FindResource("mes_0minuteago").ToString(), minutes) :
                string.Format(App.Current.FindResource("mes_0minutesago").ToString(), minutes));

            grid.ToolTip = tooltip;
        }

        private void CloseInvitePopup_Click(object sender, RoutedEventArgs e)
        {            
            _viewModel.IsInvitePopupVisible = false;
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _viewModel.ChangeTeamViewCommand.Execute(null);
        }
        
        private async void ChatMessage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //var chatEvent = ((sender as Grid).DataContext as Event);
            //if (chatEvent!=null)
            //{
            //    if (chatEvent.Type == EventType.newsquare || chatEvent.Type == EventType.unhudesquare)
            //    {
            //        if (chatEvent.Square!=null)
            //        {
            //            chatEvent.Square.IsReplaceNewAnimate = true;
            //            await Task.Delay(1000);
            //            chatEvent.Square.IsReplaceNewAnimate = false;
            //        }
            //    }
            //}
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CreateBoardShot_Click(object sender, RoutedEventArgs e)
        {
            btnShare.IsChecked = false;
            //VisualHelper.CopyElementToClipboard(board);
            MainWindow.ShowToast(new ToastInfo() { Title = App.Current.FindResource("mes_success").ToString(), Detail = App.Current.FindResource("mes_screenshotsaved").ToString() });
        }

        private void CLoseFinishRunPopup_Click(object sender, RoutedEventArgs e)
        {
            btnFinishRun.IsChecked = false;
        }

        private void TestAnimation_Click(object sender, RoutedEventArgs e)
        {
            var sb = FindResource("WelcomeAnimation") as Storyboard;
            if (sb!=null)
            {
                sb.Begin();
            }
        }

        private void MapPatternClose_Clicked(object sender, EventArgs e)
        {
            //btnMapPattern.IsChecked = false;
            _viewModel.CloseMapPatternSelectCommand.Execute(null);
        }
    }
}
