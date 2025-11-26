using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Point = System.Windows.Point;

namespace WheelTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<Sector> Sectors { get; set; } = new List<Sector>
        {
            new Sector("Убить падшую монахиню", "Red", 10),
            new Sector("Получить лотус", "Green", 20),
            new Sector("Полуить клинок Бессмертных", "Blue", 30),
            new Sector("Убить Генетиро", "GreenYellow", 40),
            new Sector("Убить 10 самураев", "Purple", 50),
            new Sector("Вытащить сороконожку 7 раз", "LightBlue", 60)
        };

        private Sector _currentSector;

        public MainWindow()
        {
            InitializeComponent();
            InitWheel();
        }

        private void StartRotation_Click(object sender, RoutedEventArgs e)
        {
            var sb = FindResource("RotateWheel") as Storyboard;
            if (sb.Children[0] is DoubleAnimation da)
            {
                da.To = 1080 + new Random().Next(0, 360); // Randomize the rotation
                var angle = 360 - (da.To - 1080);
                var sector = Sectors.FirstOrDefault(s => s.FromAngle <= angle && angle < s.ToAngle);
                if (sector != null)
                {
                    _currentSector = sector;
                    sb.Completed += (s, args) =>
                    {
                        tbxCurSector.Text = $"Текущий сектор: {sector.Name} ({sector.Value})";
                    };
                }
                else
                {
                    tbxCurSector.Text = "Текущий сектор: Неизвестно";
                }
                sb.Begin();
            }
        }

        public void InitWheel()
        {
            cnv.Children.Clear();
            var ellipse = new Ellipse
            {
                Width = 500,
                Height = 500,
                Stroke = Brushes.Black,
                Fill = Brushes.LightGray,
                StrokeThickness = 2
            };
            cnv.Children.Add(ellipse);
            if (Sectors.Count == 0)
            {
                MessageBox.Show("No sectors available to display.");
                return;
            }
            else if (Sectors.Count == 1)
            {
                var sector = Sectors[0];
                var ell = new Ellipse
                {
                    Width = 500,
                    Height = 500,
                    Fill = (SolidColorBrush)new BrushConverter().ConvertFromString(sector.Color),
                };
                var textBlock = new TextBlock
                {
                    Text = $"{sector.Name}",
                    Foreground = Brushes.White,
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
            else if (Sectors.Count >= 1)
            {
                double angle = 360.0 / Sectors.Count;
                for (int i = 0; i < Sectors.Count; i++)
                {
                    var sector = Sectors[i];
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
                        Fill = (SolidColorBrush)new BrushConverter().ConvertFromString(sector.Color),
                        Stroke = Brushes.Black,
                        StrokeThickness = 1
                    };
                    cnv.Children.Add(path);
                    var textBlock = new TextBlock
                    {
                        Text = $"{sector.Name}",
                        Foreground = Brushes.White,
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

        private void AddSector_Click(object sender, RoutedEventArgs e)
        {
            Sectors.Add(new Sector("Убить Генетиро (Замок Асина)", "Blue", 10));
            InitWheel();
        }

        private void RemoveSector_Click(object sender, RoutedEventArgs e)
        {
            if (_currentSector == null)
            {
                _currentSector = Sectors.FirstOrDefault();
            }            
            InitWheel();
            Sectors.Remove(_currentSector);
            _currentSector = null;
        }

        private void SetToZero_Click(object sender, RoutedEventArgs e)
        {
            var sb = new Storyboard();
            var da = new DoubleAnimation
            {               
                To = 0,
                Duration = TimeSpan.FromSeconds(0.1)
            };
            Storyboard.SetTarget(da, cnv);
            Storyboard.SetTargetProperty(da, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));
            sb.Children.Add(da);
            sb.Begin();
        }
    }

    public class Sector
    {
        public string Name { get; set; }
        public string Color { get; set; }
        public int Value { get; set; }
        public int FromAngle { get; set; }
        public int ToAngle { get; set; }
        public Sector(string name, string color, int value)
        {
            Name = name;
            Color = color;
            Value = value;
        }
    }
}