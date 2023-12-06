using BingoApp.ViewModels;
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
    /// Interaction logic for EditBoardPage.xaml
    /// </summary>
    public partial class EditBoardPage : Page
    {
        public EditBoardPage()
        {
            InitializeComponent();
        }

        private void tbxSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var vm = (DataContext as EditBoardViewModel);
                vm.EnterSearch();
            }
        }

        private void flyoutPlace_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            flyoutPlace.Visibility = Visibility.Collapsed;
            presetSelectFlyout.Visibility = Visibility.Collapsed;            
        }

        private void presetSelectFlyout_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void CopyTo_Click(object sender, RoutedEventArgs e)
        {
            flyoutPlace.Visibility = Visibility.Visible;
            var flyout = presetSelectFlyout;
            var button = (sender as Button);
            var point = button.TransformToAncestor(this)
                              .Transform(new Point(0, 0));

            flyout.Margin = new Thickness(point.X - flyout.Width + button.Width, point.Y + button.ActualHeight + 5, 0, 0);

            flyout.Visibility = Visibility.Visible;
        }

        private void HideFlyout_Click(object sender, RoutedEventArgs e)
        {
            flyoutPlace.Visibility = Visibility.Collapsed;
            presetSelectFlyout.Visibility = Visibility.Collapsed;
        }
    }
}
