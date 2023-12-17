using BingoApp.Models;
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
using System.Windows.Threading;

namespace BingoApp.Controls
{
    /// <summary>
    /// Interaction logic for BingoSquare.xaml
    /// </summary>
    public partial class BingoSquare : UserControl
    {
        private DispatcherTimer _timer;

        private bool canChangeValue = true;

        public BingoSquare()
        {
            InitializeComponent();

            _timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(400) };
            _timer.Tick += _timer_Tick;
        }

        private void _timer_Tick(object? sender, EventArgs e)
        {
            _timer.Stop();
            canChangeValue = true;
        }

        public BingoColor CurrentPlayerColor
        {
            get { return (BingoColor)GetValue(CurrentPlayerColorProperty); }
            set { SetValue(CurrentPlayerColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentPlayerColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentPlayerColorProperty =
            DependencyProperty.Register("CurrentPlayerColor", typeof(BingoColor), typeof(BingoSquare), new PropertyMetadata(BingoColor.red));



        public Square Square
        {
            get { return (Square)GetValue(SquareProperty); }
            set { SetValue(SquareProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Square.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SquareProperty =
            DependencyProperty.Register("Square", typeof(Square), typeof(BingoSquare), new PropertyMetadata(null));




        public ICommand MarkCommand
        {
            get { return (ICommand)GetValue(MarkCommandProperty); }
            set { SetValue(MarkCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MarkCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MarkCommandProperty =
            DependencyProperty.Register("MarkCommand", typeof(ICommand), typeof(BingoSquare), new PropertyMetadata(null));



        public object MarkCommandParameter
        {
            get { return (object)GetValue(MarkCommandParameterProperty); }
            set { SetValue(MarkCommandParameterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MarkCommandParameter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MarkCommandParameterProperty =
            DependencyProperty.Register("MarkCommandParameter", typeof(object), typeof(BingoSquare), new PropertyMetadata(null));

                        
        public bool IsScoreVisible => (Square?.Score ?? 0 ) > 0;

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            (sender as Border).Focus();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {            
            if (MarkCommand != null)
            {
                if (MarkCommand.CanExecute(Square))
                    MarkCommand.Execute(Square);
            }
        }

        private void Border_KeyDown(object sender, KeyEventArgs e)
        {
            if (CurrentPlayerColor == BingoColor.blank)
            {
                tblScore.Foreground = App.Current.FindResource("AccentColorBrush") as SolidColorBrush;
            }

            if (e.Key == Key.Space && e.KeyboardDevice.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;
                if (MarkCommand != null)
                {
                    if (MarkCommand.CanExecute(Square))
                        MarkCommand.Execute(Square);
                }
            }
            if (e.Key == Key.Space && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                Square.IsGoal = !Square.IsGoal;
                e.Handled = true;
                
            }
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.Up)
            {
                if (canChangeValue)
                {
                    Square.Score += 1;
                }
                canChangeValue = false;
                _timer.Start();
                e.Handled = true;
            }
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.Down)
            {
                if (canChangeValue)
                {
                    if (Square.Score>0)
                        Square.Score -= 1;
                }
                canChangeValue = false;
                _timer.Start();
                e.Handled = true;
            }
        }

        private void Border_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (CurrentPlayerColor == BingoColor.blank)
            {
                tblScore.Foreground = App.Current.FindResource("AccentColorBrush") as SolidColorBrush;
            }

            if (canChangeValue)
            {
                if (e.Delta > 0)
                {
                    if (Square.Score>0)
                        Square.Score -= 1;
                }
                else if (e.Delta < 0)
                {
                    Square.Score += 1;
                }
            }
            canChangeValue = false;
            _timer.Start();
        }

        private void Border_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled= true;
            Square.IsGoal = !Square.IsGoal;
        }

        private void control_Initialized(object sender, EventArgs e)
        {
            if (CurrentPlayerColor == BingoColor.blank)
            {
                tblScore.Foreground = App.Current.FindResource("AccentColorBrush") as SolidColorBrush;
            }
        }
    }
}
