using HuntpointApp.Models;
using HuntpointApp.ViewModels;
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

namespace HuntpointApp.Views;

/// <summary>
/// Interaction logic for FirstLaunchPage.xaml
/// </summary>
public partial class FirstLaunchPage : Page
{
    public FirstLaunchPage()
    {
        InitializeComponent();
    }

    private void ToggleButton_Click(object sender, RoutedEventArgs e)
    {
        foreach(ToggleButton button in wrPanel.Children.Cast<ToggleButton>())
        {
            if (button != (sender as ToggleButton))
            {
                button.IsChecked = false;
            }
        }

        (this.DataContext as FirstLaunchViewModel).HuntpointColor = (HuntpointColor)Enum.Parse(typeof(HuntpointColor), (sender as ToggleButton).Tag.ToString());
    }

    private void Continue_Click(object sender, RoutedEventArgs e)
    {
        MainWindow.NavigateTo(new MainPage());
    }
}
