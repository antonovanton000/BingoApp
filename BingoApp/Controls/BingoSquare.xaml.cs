using BingoApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BingoApp.Controls
{
    /// <summary>
    /// Interaction logic for BingoSquare.xaml
    /// </summary>
    public partial class BingoSquare : UserControl, INotifyPropertyChanged
    {
        private DispatcherTimer _timer;

        private bool canChangeValue = true;

        public BingoSquare()
        {
            InitializeComponent();
            glowBorder.Visibility = Visibility.Collapsed;
            IsSynergyVisible = true;
            SynergyBackground = new SolidColorBrush(synergyBackgroundsList[synergyBackgroundIndex]);
            _timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(300) };
            _timer.Tick += _timer_Tick;
            Loaded += BingoSquare_Loaded;
        }

        private async void BingoSquare_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(4500);
            glowBorder.Visibility = Visibility.Visible;
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



        public BingoColor PotentialBingoColor
        {
            get { return (BingoColor)GetValue(PotentialBingoColorProperty); }
            set { SetValue(PotentialBingoColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PotentialBingoColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PotentialBingoColorProperty =
            DependencyProperty.Register("PotentialBingoColor", typeof(BingoColor), typeof(BingoSquare), new PropertyMetadata(BingoColor.blank));



        public bool IsPotentialBingoVisible
        {
            get { return (bool)GetValue(IsPotentialBingoVisibleProperty); }
            set { SetValue(IsPotentialBingoVisibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsPotentialBingoVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsPotentialBingoVisibleProperty =
            DependencyProperty.Register("IsPotentialBingoVisible", typeof(bool), typeof(BingoSquare), new PropertyMetadata(false));



        public bool IsTransparent
        {
            get { return (bool)GetValue(IsTransparentProperty); }
            set { SetValue(IsTransparentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsTransparent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsTransparentProperty =
            DependencyProperty.Register("IsTransparent", typeof(bool), typeof(BingoSquare), new PropertyMetadata(false));

        public bool IsScoreVisible => (Square?.Score ?? 0) > 0;

        bool isSynergyVisible;

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsSynergyVisible { get => isSynergyVisible; set { isSynergyVisible = value; OnPropertyChanged(nameof(IsSynergyVisible)); } }

        SolidColorBrush synergyBackground;
        public SolidColorBrush SynergyBackground { get => synergyBackground; set { synergyBackground = value; OnPropertyChanged(nameof(SynergyBackground)); } }

        int synergyBackgroundIndex = 0;

        Color[] synergyBackgroundsList = new Color[]
        {
            Color.FromArgb(0xFF,0xFE,0xFE,0xFE),
            Color.FromArgb(0xFF,0x7E, 0xD4,0xAD),
            Color.FromArgb(0xFF,0xFC, 0xF5,0x96),
            Color.FromArgb(0xFF,0x00, 0x96,0xFF),
            Color.FromArgb(0xFF,0xAF, 0x17,0x40)
        };

        public void AnimateBackground()
        {
            var sb = FindResource("bingoAnimate") as Storyboard;
            if (sb != null)
            {
                sb.Begin();
            }
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            (sender as Border).Focus();
        }

        private async void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Square.IsHidden)
                return;

            if (MarkCommand != null)
            {
                if (MarkCommand.CanExecute(Square))
                    MarkCommand.Execute(Square);                
            }
        }

        private void Border_KeyDown(object sender, KeyEventArgs e)
        {
            if (Square.IsHidden)
                return;

            if (CurrentPlayerColor == BingoColor.blank)
            {
                //tblScore.Foreground = App.Current.FindResource("AccentColorBrush") as SolidColorBrush;
            }

            if (e.Key == Key.Space && e.KeyboardDevice.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;
                if (MarkCommand != null)
                {
                    if (!Square.IsHidden)
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
                    if (Square.Score > 0)
                        Square.Score -= 1;
                }
                canChangeValue = false;
                _timer.Start();
                e.Handled = true;
            }
        }

        private void Border_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Square.IsHidden)
                return;

            if (CurrentPlayerColor == BingoColor.blank)
            {
                //tblScore.Foreground = App.Current.FindResource("AccentColorBrush") as SolidColorBrush;
            }

            if (canChangeValue)
            {
                if (e.Delta > 0)
                {
                    if (Square.Score > 0)
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
            e.Handled = true;
            if (Square.IsHidden)
                return;

            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (synergyBackgroundIndex == 4)
                    synergyBackgroundIndex = 0;
                else
                    synergyBackgroundIndex++;

                SynergyBackground = new SolidColorBrush(synergyBackgroundsList[synergyBackgroundIndex]);
            }
            else
            {
                Square.IsGoal = !Square.IsGoal;
            }
        }

        private void control_Initialized(object sender, EventArgs e)
        {
            if (CurrentPlayerColor == BingoColor.blank)
            {
                //tblScore.Foreground = App.Current.FindResource("AccentColorBrush") as SolidColorBrush;
            }
        }

        private void Border_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void ColorAnimationUsingKeyFrames_Completed(object sender, EventArgs e)
        {

        }
    }
}
