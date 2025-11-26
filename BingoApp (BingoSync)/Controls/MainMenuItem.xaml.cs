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

namespace BingoApp.Controls
{
    /// <summary>
    /// Interaction logic for MainMenuItem.xaml
    /// </summary>
    public partial class MainMenuItem : UserControl
    {
        public MainMenuItem()
        {
            InitializeComponent();
        }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(MainMenuItem), new PropertyMetadata(string.Empty));



        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Icon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(ImageSource), typeof(MainMenuItem), new PropertyMetadata(null));



        public string RoutePage
        {
            get { return (string)GetValue(RoutePageProperty); }
            set { SetValue(RoutePageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RoutePage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RoutePageProperty =
            DependencyProperty.Register("RoutePage", typeof(string), typeof(MainMenuItem), new PropertyMetadata(string.Empty));



        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSelected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(MainMenuItem), new PropertyMetadata(false));


        public event EventHandler Clicked;

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Clicked?.Invoke(this, new EventArgs());
        }
    }
}
