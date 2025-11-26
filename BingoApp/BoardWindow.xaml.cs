using BingoApp.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BingoApp
{
    /// <summary>
    /// Interaction logic for BoardWindow.xaml
    /// </summary>
    public partial class BoardWindow : Window
    {

        WindowSinker sinker;

        public bool NotSavePosition { get; set; } = false;

        public BoardWindow()
        {
            InitializeComponent();
            SourceInitialized += (s, e) =>
            {
                if (Properties.Settings.Default.BWIsPositionSaved)
                {
                    this.Top = Properties.Settings.Default.BWTop;
                    this.Left = Properties.Settings.Default.BWLeft;
                    this.Height = Properties.Settings.Default.BWHeight;
                    this.Width = Properties.Settings.Default.BWWidth;
                }
            };
            Closing += (s, e) =>
            {
                if (NotSavePosition)
                    return;

                Properties.Settings.Default.BWTop = this.Top;
                Properties.Settings.Default.BWLeft = this.Left;
                Properties.Settings.Default.BWHeight = this.Height;
                Properties.Settings.Default.BWWidth = this.Width;                
                Properties.Settings.Default.BWIsPositionSaved = true;
                Properties.Settings.Default.Save();
            };
            sinker = new WindowSinker(this);
            sinker.Sink();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
