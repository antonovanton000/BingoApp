using ABI.System;
using BingoApp.Models;
using BingoApp.ViewModels;
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
using System.Windows.Media.Effects;
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
        private EditBoardViewModel _viewModel = default!;

        public EditBoardPage()
        {
            InitializeComponent();
            Loaded += EditBoardPage_Loaded;
        }

        private void EditBoardPage_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel = (DataContext as EditBoardViewModel);
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
            btnaddons.IsChecked = false;
            flyoutPlace.Visibility = Visibility.Visible;
            var flyout = presetSelectFlyout;
            var button = (sender as Button);
            var point = button.TransformToAncestor(this)
                              .Transform(new Point(0, 0));

            flyout.Margin = new Thickness(point.X - flyout.Width + button.Width, point.Y + button.ActualHeight + 5, 0, 0);

            flyout.Visibility = Visibility.Visible;
        }

        private void CloseInvitePopup_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.IsSendToPlayerPopupVisible = false;
        }

        private void ClosePopup_Click(object sender, RoutedEventArgs e)
        {
            btnShare.IsChecked = false;
        }

        private void HideFlyout_Click(object sender, RoutedEventArgs e)
        {
            flyoutPlace.Visibility = Visibility.Collapsed;
            presetSelectFlyout.Visibility = Visibility.Collapsed;
        }

        private void SaveNote_Click(object sender, RoutedEventArgs e)
        {
            ((((((sender as Button).Parent as Grid).Parent as Border).Parent as Popup).Parent as Grid).Children[0] as ToggleButton).IsChecked = false;
        }

        private void CloseAddonPopup_Click(object sender, RoutedEventArgs e)
        {
            btnaddons.IsChecked = false;
        }

        private void tbxSearch2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var vm = (DataContext as EditBoardViewModel);
                vm.EnterSearch2();
            }
        }

        private void DeleteSquareFromException_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                var squareName = btn.DataContext as string;
                var exception = btn.Tag as PresetSquareException;
                if (exception != null && !string.IsNullOrEmpty(squareName))
                {
                    _viewModel.DeleteSquareFromException(exception, squareName);
                }                
            }
        }

        private void squareName_Changed(object sender, TextChangedEventArgs e)
        {
            if (!(sender is TextBox tb)) return;
            if (!(_viewModel?.BoardPreset?.Exceptions is System.Collections.ObjectModel.ObservableCollection<PresetSquareException> exceptions)) return;
            if (!(tb.DataContext is PresetSquare presetSquare)) return;

            var oldName = presetSquare.Name.Trim() ?? string.Empty;
            var newName = tb.Text.Trim() ?? string.Empty;
            if (oldName == newName) return;

            // Replace entries in each ObservableCollection<string> by index so indices are preserved
            foreach (var ex in exceptions)
            {
                var names = ex.SquareNames;
                if (names == null) continue;

                for (int i = 0; i < names.Count; i++)
                {
                    if (names[i].Trim() == oldName.Trim())
                    {
                        names[i] = newName.Trim();
                    }
                }
            }
        }

        string oldName;

        private void tbxName_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!(sender is TextBox tb)) return;
            if (!(tb.DataContext is PresetSquare presetSquare)) return;
            oldName = presetSquare.Name.Trim() ?? string.Empty;

        }

        private void tbxName_KeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!(sender is TextBox tb)) return;
                if (!(_viewModel?.BoardPreset?.Exceptions is System.Collections.ObjectModel.ObservableCollection<PresetSquareException> exceptions)) return;
                if (!(tb.DataContext is PresetSquare presetSquare)) return;
                var newName = tb.Text.Trim() ?? string.Empty;
                if (oldName == newName) return;

                // Replace entries in each ObservableCollection<string> by index so indices are preserved
                foreach (var ex in exceptions)
                {
                    var names = ex.SquareNames;
                    if (names == null) continue;

                    for (int i = 0; i < names.Count; i++)
                    {
                        if (names[i].Trim() == oldName.Trim())
                        {
                            names[i] = newName.Trim();
                        }
                    }
                }
            }
        }
    }
}
