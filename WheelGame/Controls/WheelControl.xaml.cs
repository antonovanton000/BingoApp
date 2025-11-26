using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using WheelGame.Models;

namespace WheelGame.Controls
{
    /// <summary>
    /// Interaction logic for WheelControl.xaml
    /// </summary>
    public partial class WheelControl : UserControl
    {
        public ObservableCollection<Objective> Objectives
        {
            get { return (ObservableCollection<Objective>)GetValue(ObjectivesProperty); }
            set { SetValue(ObjectivesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ObjectivesProperty =
            DependencyProperty.Register("Objectives", typeof(ObservableCollection<Objective>), typeof(WheelControl),
                new PropertyMetadata(null, OnObjectivesChangedCallBack));



        private static void OnObjectivesChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            WheelControl c = sender as WheelControl;
            if (c != null)
            {
                c.OnObjectivesChanged(c.Objectives);
            }
        }


        protected virtual void OnObjectivesChanged(ObservableCollection<Objective> objectives)
        {
            InitWheel();
            objectives.CollectionChanged += Objectives_CollectionChanged;
        }

        private void Objectives_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            InitWheel();
        }

        public Brush SectorBackgroundBrush
        {
            get { return (Brush)GetValue(SectorBackgroundBrushProperty); }
            set { SetValue(SectorBackgroundBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SectorBackgroundBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SectorBackgroundBrushProperty =
            DependencyProperty.Register("SectorBackgroundBrush", typeof(Brush), typeof(WheelControl), new PropertyMetadata(Brushes.White));


        public Brush SectorForegroundBrush
        {
            get { return (Brush)GetValue(SectorForegroundBrushProperty); }
            set { SetValue(SectorForegroundBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SectorForegroundBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SectorForegroundBrushProperty =
            DependencyProperty.Register("SectorForegroundBrush", typeof(Brush), typeof(WheelControl), new PropertyMetadata(Brushes.Black));

        public Objective? SelectedObjective
        {
            get { return (Objective?)GetValue(SelectedObjectiveProperty); }
            set { SetValue(SelectedObjectiveProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedObjective.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedObjectiveProperty =
            DependencyProperty.Register("SelectedObjective", typeof(Objective), typeof(WheelControl), new PropertyMetadata(null));



        public Brush WheelBorderBrush
        {
            get { return (Brush)GetValue(WheelBorderBrushProperty); }
            set { SetValue(WheelBorderBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for WheelBorderBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WheelBorderBrushProperty =
            DependencyProperty.Register("WheelBorderBrush", typeof(Brush), typeof(WheelControl), new PropertyMetadata(Brushes.Black));


        public WheelControl()
        {
            InitializeComponent();
            Loaded += WheelControl_Loaded;
        }

        private void WheelControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitWheel();
        }

        private void InitWheel()
        {

            cnv.Children.Clear();
            var ellipse = new Ellipse
            {
                Width = 500,
                Height = 500,
                Stroke = WheelBorderBrush,
                Fill = SectorBackgroundBrush,
                StrokeThickness = 2
            };
            cnv.Children.Add(ellipse);

            if (Objectives == null) return;

            var notCompletedObjectives = Objectives.Where(i => !i.IsCompleted);
            if (notCompletedObjectives.Count() == 0)
            {
                MessageBox.Show("No sectors available to display.");
                return;
            }
            else if (notCompletedObjectives.Count() == 1)
            {
                var sector = notCompletedObjectives.First();
                var ell = new Ellipse
                {
                    Width = 500,
                    Height = 500,
                    Fill = SectorBackgroundBrush
                };
                var textBlock = new TextBlock
                {
                    Text = $"{sector.Name}",
                    Foreground = SectorForegroundBrush,
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    MaxWidth = 400
                };
                // Calculate the position for the text block

                textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                textBlock.Arrange(new Rect(0, 0, textBlock.DesiredSize.Width, textBlock.DesiredSize.Height));
                // Center the text block on the calculated position
                var textX = 250 - textBlock.ActualWidth / 2;
                var textY = 250 - textBlock.ActualHeight / 2;
                // Set the position of the text block
                Canvas.SetLeft(textBlock, textX);// - textBlock.ActualWidth / 2);
                Canvas.SetTop(textBlock, textY);// - textBlock.ActualHeight / 2);
                                                // Add the text block to the canvas
                cnv.Children.Add(ell);
                cnv.Children.Add(textBlock);
            }
            else if (notCompletedObjectives.Count() >= 1)
            {
                foreach (var item in Objectives)
                {
                    item.FromAngle = 0;
                    item.ToAngle = 0;
                }

                double angle = 360.0 / notCompletedObjectives.Count();
                for (int i = 0; i < notCompletedObjectives.Count(); i++)
                {
                    var sector = notCompletedObjectives.ElementAt(i);
                    var startAngle = angle * i;
                    var endAngle = startAngle + angle;
                    sector.FromAngle = (int)startAngle;
                    sector.ToAngle = (int)endAngle;
                    var pathFigure = new PathFigure
                    {
                        StartPoint = new Point(250, 250),
                        IsClosed = true
                    };
                    pathFigure.Segments.Add(new LineSegment(
                        new Point(250 + 250 * Math.Cos(startAngle * Math.PI / 180),
                                  250 + 250 * Math.Sin(startAngle * Math.PI / 180)), true));
                    pathFigure.Segments.Add(new ArcSegment(
                        new Point(250 + 250 * Math.Cos(endAngle * Math.PI / 180),
                                  250 + 250 * Math.Sin(endAngle * Math.PI / 180)),
                        new Size(250, 250), 0, false, SweepDirection.Clockwise, true));
                    var pathGeometry = new PathGeometry();
                    pathGeometry.Figures.Add(pathFigure);
                    var path = new Path
                    {
                        Data = pathGeometry,
                        Fill = SectorBackgroundBrush,
                        Stroke = WheelBorderBrush,
                        StrokeThickness = 1
                    };
                    cnv.Children.Add(path);
                    var textBlock = new TextBlock
                    {
                        Text = $"{sector.Name}",
                        Foreground = SectorForegroundBrush,
                        FontSize = 14,
                        FontWeight = FontWeights.Bold,
                        TextTrimming = TextTrimming.CharacterEllipsis,
                        MaxWidth = 200
                    };
                    // Adjust the angle to ensure text is upright
                    var textAngle = startAngle + angle / 2;
                    if (textAngle > 180)
                    {
                        textAngle -= 360; // Adjust for angles greater than 180 degrees
                    }
                    // Rotate the text block to match the sector angle
                    textBlock.RenderTransform = new RotateTransform(textAngle, 0, 0);
                    textBlock.RenderTransformOrigin = new Point(0.5, 0.5);
                    // Calculate the position for the text block
                    var textX = 250 + 150 * Math.Cos(textAngle * Math.PI / 180);
                    var textY = 250 + 150 * Math.Sin(textAngle * Math.PI / 180);
                    textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    textBlock.Arrange(new Rect(0, 0, textBlock.DesiredSize.Width, textBlock.DesiredSize.Height));
                    // Center the text block on the calculated position
                    textX = textX - textBlock.DesiredSize.Width / 2;
                    textY = textY - textBlock.DesiredSize.Height / 2;
                    // Set the position of the text block
                    Canvas.SetLeft(textBlock, textX);// - textBlock.ActualWidth / 2);
                    Canvas.SetTop(textBlock, textY);// - textBlock.ActualHeight / 2);
                    // Add the text block to the canvas
                    cnv.Children.Add(textBlock);
                }
            }

        }

        public void UpdateWheel()
        {
            InitWheel();
        }

        public void RotateWheel(int angle)
        {
            var sb = new Storyboard();
            var da = new DoubleAnimation();
            Storyboard.SetTarget(da, cnv);
            Storyboard.SetTargetProperty(da, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));
            da.Duration = TimeSpan.FromSeconds(5);
            da.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
            da.To = angle;
            da.From = 0;
            var angle360 = 360 - (da.To - 1080);
            var notCompletedObjectives = Objectives.Where(i => !i.IsCompleted);
            var sector = notCompletedObjectives.FirstOrDefault(s => s.FromAngle <= angle360 && angle360 < s.ToAngle);
            if (sector != null)
            {
                sb.Completed += (s, args) =>
                {
                    sb = null; // Clear the storyboard to prevent memory leaks                        
                    SelectedObjective = sector;
                };
            }
            else
            {
                SelectedObjective = null;            
            }
            sb.Children.Add(da);
            sb.Begin();

        }
    }
}
