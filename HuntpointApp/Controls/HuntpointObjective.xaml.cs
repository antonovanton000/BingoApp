using HuntpointApp.Models;
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

namespace HuntpointApp.Controls
{
    /// <summary>
    /// Interaction logic for BingoSquare.xaml
    /// </summary>
    public partial class HuntpointObjective : UserControl, INotifyPropertyChanged
    {
        private DispatcherTimer _timer;

        private bool canChangeValue = true;

        public HuntpointObjective()
        {
            InitializeComponent();
            IsSynergyVisible = true;
            SynergyBackground = new SolidColorBrush(synergyBackgroundsList[synergyBackgroundIndex]);

            _timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(300) };
            _timer.Tick += _timer_Tick;
        }

        private void _timer_Tick(object? sender, EventArgs e)
        {
            _timer.Stop();
            canChangeValue = true;
        }

        public HuntpointColor CurrentPlayerColor
        {
            get { return (HuntpointColor)GetValue(CurrentPlayerColorProperty); }
            set { SetValue(CurrentPlayerColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentPlayerColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentPlayerColorProperty =
            DependencyProperty.Register("CurrentPlayerColor", typeof(HuntpointColor), typeof(HuntpointObjective), new PropertyMetadata(HuntpointColor.red));

        public Objective Objective
        {
            get { return (Objective)GetValue(ObjectiveProperty); }
            set { SetValue(ObjectiveProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Square.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ObjectiveProperty =
            DependencyProperty.Register("Objective", typeof(Objective), typeof(HuntpointObjective), new PropertyMetadata(null));

        public ICommand MarkCommand
        {
            get { return (ICommand)GetValue(MarkCommandProperty); }
            set { SetValue(MarkCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MarkCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MarkCommandProperty =
            DependencyProperty.Register("MarkCommand", typeof(ICommand), typeof(HuntpointObjective), new PropertyMetadata(null));



        public object MarkCommandParameter
        {
            get { return (object)GetValue(MarkCommandParameterProperty); }
            set { SetValue(MarkCommandParameterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MarkCommandParameter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MarkCommandParameterProperty =
            DependencyProperty.Register("MarkCommandParameter", typeof(object), typeof(HuntpointObjective), new PropertyMetadata(null));

        public bool IsTransparent
        {
            get { return (bool)GetValue(IsTransparentProperty); }
            set { SetValue(IsTransparentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsTransparent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsTransparentProperty =
            DependencyProperty.Register("IsTransparent", typeof(bool), typeof(HuntpointObjective), new PropertyMetadata(false));



        public bool IsPlayerScoreVisible => (Objective?.PlayerScore ?? 0) > 0;

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
            
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            (sender as Border).Focus();
        }

        private async void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Objective.IsHidden)
                return;

            if (MarkCommand != null)
            {
                if (MarkCommand.CanExecute(Objective))
                    MarkCommand.Execute(Objective);                
            }
        }

        private void Border_KeyDown(object sender, KeyEventArgs e)
        {
            if (Objective.IsHidden)
                return;

            if (CurrentPlayerColor == HuntpointColor.blank)
            {
                //tblScore.Foreground = App.Current.FindResource("AccentColorBrush") as SolidColorBrush;
            }

            if (e.Key == Key.Space && e.KeyboardDevice.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;
                if (MarkCommand != null)
                {
                    if (!Objective.IsHidden)
                        if (MarkCommand.CanExecute(Objective))
                            MarkCommand.Execute(Objective);
                }
            }
            if (e.Key == Key.Space && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                Objective.IsGoal = !Objective.IsGoal;
                e.Handled = true;

            }
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.Up)
            {
                if (canChangeValue)
                {
                    Objective.PlayerScore += 1;
                }
                canChangeValue = false;
                _timer.Start();
                e.Handled = true;
            }
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.Down)
            {
                if (canChangeValue)
                {
                    if (Objective.PlayerScore > 0)
                        Objective.PlayerScore -= 1;
                }
                canChangeValue = false;
                _timer.Start();
                e.Handled = true;
            }
        }

        private void Border_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Objective.IsHidden)
                return;

            if (CurrentPlayerColor == HuntpointColor.blank)
            {
                //tblScore.Foreground = App.Current.FindResource("AccentColorBrush") as SolidColorBrush;
            }

            if (canChangeValue)
            {
                if (e.Delta > 0)
                {
                    if (Objective.PlayerScore > 0)
                        Objective.PlayerScore -= 1;
                }
                else if (e.Delta < 0)
                {
                    Objective.PlayerScore += 1;
                }
            }
            canChangeValue = false;
            _timer.Start();
        }

        private void Border_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Objective.IsHidden)
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
                Objective.IsGoal = !Objective.IsGoal;
            }
        }

        private void control_Initialized(object sender, EventArgs e)
        {
            if (CurrentPlayerColor == HuntpointColor.blank)
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
