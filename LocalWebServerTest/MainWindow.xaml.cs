using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LocalWebServerTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            new LocalWebServer();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LocalWebServer.Current.Start();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            LocalWebServer.Current.Stop();
        }
    }
}