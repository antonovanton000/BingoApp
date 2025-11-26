using HuntpointApp.Models;
using HuntpointApp.ViewModels;
using Microsoft.Win32;
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

namespace HuntpointApp.Views
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

        public static void SetPreferedColor(HuntpointColor color)
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

            (this.DataContext as SettingsViewModel).HuntpointColor = (HuntpointColor)Enum.Parse(typeof(HuntpointColor), (sender as ToggleButton).Tag.ToString());
        }
    }
}
