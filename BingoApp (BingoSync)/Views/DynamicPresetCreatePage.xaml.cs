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
    /// Interaction logic for DynamicPresetCreatePage.xaml
    /// </summary>
    public partial class DynamicPresetCreatePage : Page
    {
        public DynamicPresetCreatePage()
        {
            InitializeComponent();
        }

        private void tbxSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var vm = (DataContext as DynamicPresetCreateViewModel);
                vm.EnterSearch();
            }
        }
    }
}
