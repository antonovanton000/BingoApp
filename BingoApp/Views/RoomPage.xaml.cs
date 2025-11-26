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
using System.Windows.Controls.Primitives;
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
            _viewModel.BingoHappened += (s, ee) =>
            {
                bingoAnimation.PlayAnimation();
            };
            sv.ScrollToBottom();
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
            var chatEvent = ((sender as Grid).DataContext as Event);
            if (chatEvent != null)
            {
                if (chatEvent.Type == EventType.newsquare || chatEvent.Type == EventType.unhudesquare)
                {
                    if (chatEvent.Square != null)
                    {
                        chatEvent.Square.IsReplaceNewAnimate = true;
                        await Task.Delay(1000);
                        chatEvent.Square.IsReplaceNewAnimate = false;
                    }
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void CreateBoardShot_Click(object sender, RoutedEventArgs e)
        {
            btnShare.IsChecked = false;
            var boardWindow = new BoardWindow();
            boardWindow.NotSavePosition = true;
            boardWindow.DataContext = _viewModel;
            boardWindow.Show();
            boardWindow.Left = 3000;
            boardWindow.Top = 3000;
            boardWindow.Width = 720;
            boardWindow.Height = 1000;
            await Task.Delay(500);
            VisualHelper.CopyElementToClipboard(boardWindow.mainViewBox);
            boardWindow.mainViewBox.Visibility = Visibility.Collapsed;
            await Task.Delay(200);
            boardWindow.Close();
            MainWindow.ShowToast(new ToastInfo() { Title = App.Current.FindResource("mes_success").ToString(), Detail = App.Current.FindResource("mes_screenshotsaved").ToString() });
        }

        private void LineName_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //e.Handled = true;
            if (sender is TextBlock tbl)
            {
                var popup = FindResource("SelectPlayerPopup") as Popup;
                if (popup != null)
                {
                    popup.PlacementTarget = tbl;
                    popup.Placement = PlacementMode.Bottom;
                    popup.StaysOpen = false;
                    popup.IsOpen = true;
                }
            }
        }

        private void LineName_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                foreach(var item in _viewModel.Room.Board.Squares)
                {
                    item.IsInSelectedLine = false;
                }

                if (!_viewModel.CanSelectBingoLine) 
                    return;

                var lineName = btn.Tag.ToString();
                if (lineName == _viewModel.SelectedBingoLine)
                {
                    _viewModel.SelectedBingoLine = null;
                    return;
                }

                _viewModel.SelectedBingoLine = lineName;
                if (lineName.Contains("row"))
                {
                    var rowNum = int.Parse(lineName.Replace("row", ""));
                    var squares = _viewModel.Room.Board.Squares.Where(i => i.Row == rowNum);
                    foreach (var item in squares)
                    {
                        item.IsInSelectedLine = true;
                    }
                }
                else if (lineName.Contains("col"))
                {
                    var colNum = int.Parse(lineName.Replace("col", ""));
                    var squares = _viewModel.Room.Board.Squares.Where(i => i.Column == colNum);
                    foreach (var item in squares)
                    {
                        item.IsInSelectedLine = true;
                    }
                }
                else if (lineName == "tr_bl")
                {
                    var squares = _viewModel.Room.Board.GetTRLBDiagonal();
                    foreach (var item in squares)
                    {
                        item.IsInSelectedLine = true;
                    }
                }
                else if (lineName == "tl_br")
                {
                    var squares = _viewModel.Room.Board.GetTLRBDiagonal();
                    foreach (var item in squares)
                    {
                        item.IsInSelectedLine = true;
                    }
                }
            }
        }
    }
}
