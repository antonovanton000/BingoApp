using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for AllNewsPage.xaml
    /// </summary>
    public partial class NewsDetailPage : Page
    {
        public NewsDetailPage()
        {
            InitializeComponent();         
        }

        private void wb_NavigationStarting(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            if (!e.Uri.Contains("data:text/html"))
            {
                e.Cancel = true;
                System.Diagnostics.Process.Start(new ProcessStartInfo
                {
                    FileName = e.Uri.ToString(),
                    UseShellExecute = true
                });
            }
        }
    }
}
