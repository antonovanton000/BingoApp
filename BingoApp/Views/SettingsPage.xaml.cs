using BingoApp.Models;
using BingoApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        private static SettingsPage instance;
        public SettingsPage()
        {
            InitializeComponent();
            instance = this;
        }

        public static void SetPreferedColor(BingoColor color)
        {
            foreach (ToggleButton button in instance.wrPanel.Children.Cast<ToggleButton>())
            {
                if (button.Tag.ToString() == color.ToString())
                {
                    button.IsChecked = true;
                }
            }
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (ToggleButton button in wrPanel.Children.Cast<ToggleButton>())
            {
                if (button != (sender as ToggleButton))
                {
                    button.IsChecked = false;
                }
            }

            (this.DataContext as SettingsViewModel).BingoColor = (BingoColor)Enum.Parse(typeof(BingoColor), (sender as ToggleButton).Tag.ToString());
        }
    }
}
