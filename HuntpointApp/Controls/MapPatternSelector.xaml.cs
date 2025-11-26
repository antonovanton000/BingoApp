using HuntpointApp.Classes;
using System;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HuntpointApp.Controls
{
    /// <summary>
    /// Interaction logic for MapPatternSelector.xaml
    /// </summary>
    public partial class MapPatternSelector : UserControl
    {
        public MapPatternSelector()
        {
            new NightreignMapPatternCollection();
            Bosses = new ObservableCollection<Boss>();
            InitControl();
            InitializeComponent();
        }

        public event EventHandler CloseClicked;

        public ObservableCollection<Boss> Bosses { get; set; }

        public void InitControl()
        {
            Bosses.Clear();
            NightreignMapPatternCollection.Instance.LoadPatterns();
            var allPaterns = NightreignMapPatternCollection.Instance.MapPatterns;
            var shiftingEarthes = NightreignMapPatternCollection.Mapping.GetAllShiftingEarthes();
            foreach (var item in NightreignMapPatternCollection.Mapping.GetAllBosses())
            {
                var boss = new Boss()
                {
                    Name = item.Value,
                    Id = item.Key,
                    ImagePath = new Uri(System.IO.Path.Combine(App.Location, "MapPatterns", "_mapping", "bosses", item.Key + ".png"))
                };
                foreach (var se in shiftingEarthes)
                {
                    var newSe = new ShiftingEarth() { Id = se.Key, Name = se.Value };
                    newSe.ImagePath = new Uri(System.IO.Path.Combine(App.Location, "MapPatterns", "_mapping", "earthes", se.Key + ".png"));
                    newSe.MapPath = System.IO.Path.Combine(App.Location, "MapPatterns", "_mapping", "maps", se.Key + ".jpg");

                    boss.ShiftingEarthes.Add(newSe);

                    var patterns = allPaterns.Where(i => i.BossName == item.Key && i.ShiftingEarh == se.Key).ToList();
                    foreach (var spawnPoints in patterns.GroupBy(i => i.SpawnPoint))
                    {
                        var spawnPoint = new SpawnPoint() { Id = spawnPoints.Key };
                        spawnPoint.Margin = NightreignMapPatternCollection.Mapping.GetSpawnPointMargins(spawnPoints.Key);
                        foreach (var pattern in spawnPoints)
                        {
                            spawnPoint.Patterns.Add(new Pattern() { ImagePath = new Uri(pattern.ImagePath), Thumbnail = new Uri(pattern.ThumbnailPath), Name = pattern.PatternName });
                        }
                        newSe.SpawnPoints.Add(spawnPoint);
                    }
                }
                Bosses.Add(boss);
            }
        }



        public bool IsBossSelectionStep
        {
            get { return (bool)GetValue(IsBossSelectionStepProperty); }
            set { SetValue(IsBossSelectionStepProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsBossSelectionStep.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsBossSelectionStepProperty =
            DependencyProperty.Register("IsBossSelectionStep", typeof(bool), typeof(MapPatternSelector), new PropertyMetadata(true));




        public bool IsShiftingEarthSelectionStep
        {
            get { return (bool)GetValue(IsShiftingEarthSelectionStepProperty); }
            set { SetValue(IsShiftingEarthSelectionStepProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsShiftingEarthSelectionStep.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsShiftingEarthSelectionStepProperty =
            DependencyProperty.Register("IsShiftingEarthSelectionStep", typeof(bool), typeof(MapPatternSelector), new PropertyMetadata(false));


        public bool IsSpawnPointSelectionStep
        {
            get { return (bool)GetValue(IsSpawnPointSelectionStepProperty); }
            set { SetValue(IsSpawnPointSelectionStepProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSpawnPointSelectionStep.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSpawnPointSelectionStepProperty =
            DependencyProperty.Register("IsSpawnPointSelectionStep", typeof(bool), typeof(MapPatternSelector), new PropertyMetadata(false));


        public bool IsPatternSelectionStep
        {
            get { return (bool)GetValue(IsPatternSelectionStepProperty); }
            set { SetValue(IsPatternSelectionStepProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsPatternSelectionStep.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsPatternSelectionStepProperty =
            DependencyProperty.Register("IsPatternSelectionStep", typeof(bool), typeof(MapPatternSelector), new PropertyMetadata(false));


        public bool IsMapViewStep
        {
            get { return (bool)GetValue(IsMapViewStepProperty); }
            set { SetValue(IsMapViewStepProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsMapViewStep.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsMapViewStepProperty =
            DependencyProperty.Register("IsMapViewStep", typeof(bool), typeof(MapPatternSelector), new PropertyMetadata(false));


        public Boss SelectedBoss
        {
            get { return (Boss)GetValue(SelectedBossProperty); }
            set { SetValue(SelectedBossProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedBoss.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedBossProperty =
            DependencyProperty.Register("SelectedBoss", typeof(Boss), typeof(MapPatternSelector), new PropertyMetadata(null));


        public ShiftingEarth SelectedShiftingEarth
        {
            get { return (ShiftingEarth)GetValue(SelectedShiftingEarthProperty); }
            set { SetValue(SelectedShiftingEarthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedShiftingEarth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedShiftingEarthProperty =
            DependencyProperty.Register("SelectedShiftingEarth", typeof(ShiftingEarth), typeof(MapPatternSelector), new PropertyMetadata(null));


        public SpawnPoint SelectedSpawnPoint
        {
            get { return (SpawnPoint)GetValue(SelectedSpawnPointProperty); }
            set { SetValue(SelectedSpawnPointProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedSpawnPoint.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedSpawnPointProperty =
            DependencyProperty.Register("SelectedSpawnPoint", typeof(SpawnPoint), typeof(MapPatternSelector), new PropertyMetadata(null));


        public Pattern SelectedPattern
        {
            get { return (Pattern)GetValue(SelectedPatternProperty); }
            set { SetValue(SelectedPatternProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedPattern.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedPatternProperty =
            DependencyProperty.Register("SelectedPattern", typeof(Pattern), typeof(MapPatternSelector), new PropertyMetadata(null));




        public ICommand CloseCommand
        {
            get { return (ICommand)GetValue(CloseCommandProperty); }
            set { SetValue(CloseCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CloseCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CloseCommandProperty =
            DependencyProperty.Register("CloseCommand", typeof(ICommand), typeof(MapPatternSelector), new PropertyMetadata(null));


        public void SetSpecificPattern(int patternId)
        {
            var allPaterns = NightreignMapPatternCollection.Instance.MapPatterns;
            var pattern = allPaterns.FirstOrDefault(i => i.PatternName == patternId.ToString("D3"));
            if (pattern != null)
            {
                var boss = Bosses.FirstOrDefault(i => i.Id == pattern.BossName);
                if (boss != null)
                {
                    SelectedBoss = boss;
                    var shiftingEarth = boss.ShiftingEarthes.FirstOrDefault(i => i.Id == pattern.ShiftingEarh);
                    if (shiftingEarth != null)
                    {
                        SelectedShiftingEarth = shiftingEarth;
                        var spawnPoint = shiftingEarth.SpawnPoints.FirstOrDefault(i => i.Id == pattern.SpawnPoint);
                        if (spawnPoint != null)
                        {
                            SelectedSpawnPoint = spawnPoint;
                            var spattern = spawnPoint.Patterns.FirstOrDefault(i => i.Name == pattern.PatternName);
                            if (spattern != null)
                            {
                                SelectedPattern = spattern;
                                IsBossSelectionStep = false;
                                IsShiftingEarthSelectionStep = false;
                                IsSpawnPointSelectionStep = false;
                                IsPatternSelectionStep = false;
                                backFromMapViewButton.Visibility = Visibility.Collapsed;
                                IsMapViewStep = true;
                            }
                        }                        
                    }
                }
            }
        }

        private void BossSelect_Clicked(object sender, MouseButtonEventArgs e)
        {
            SelectedBoss = (sender as FrameworkElement).DataContext as Boss;
            IsBossSelectionStep = false;
            IsShiftingEarthSelectionStep = true;
        }

        private void ShiftingEarthSelect_Clicked(object sender, MouseButtonEventArgs e)
        {
            SelectedShiftingEarth = (sender as FrameworkElement).DataContext as ShiftingEarth;
            IsShiftingEarthSelectionStep = false;
            IsSpawnPointSelectionStep = true;
        }

        private void SpawnPointSelect_Clicked(object sender, MouseButtonEventArgs e)
        {
            SelectedSpawnPoint = (sender as FrameworkElement).DataContext as SpawnPoint;
            SelectedPattern = SelectedSpawnPoint?.Patterns.FirstOrDefault();
            IsSpawnPointSelectionStep = false;
            IsPatternSelectionStep = true;
        }

        private void ShiftingEarthBack_Clicked(object sender, RoutedEventArgs e)
        {
            IsShiftingEarthSelectionStep = false;
            IsBossSelectionStep = true;
        }

        private void SpawnPointBack_Clicked(object sender, RoutedEventArgs e)
        {
            IsSpawnPointSelectionStep = false;
            IsShiftingEarthSelectionStep = true;
        }

        private void PatterBack_Clicked(object sender, RoutedEventArgs e)
        {
            IsPatternSelectionStep = false;
            IsSpawnPointSelectionStep = true;
        }

        private void SelectDifferentMap_Click(object sender, RoutedEventArgs e)
        {
            IsPatternSelectionStep = true;
            IsMapViewStep = false;
        }

        private void SelectMap_Clicked(object sender, RoutedEventArgs e)
        {
            IsPatternSelectionStep = false;
            IsMapViewStep = true;
        }

        private void Close_Clicked(object sender, RoutedEventArgs e)
        {
            CloseClicked?.Invoke(this, EventArgs.Empty);
            if (CloseCommand != null)
            {
                if (CloseCommand.CanExecute(null))
                    CloseCommand.Execute(null);
            }
        }
    }


    public class Boss()
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public Uri ImagePath { get; set; }

        public ObservableCollection<ShiftingEarth> ShiftingEarthes { get; set; } = new();
    }

    public class ShiftingEarth()
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public Uri ImagePath { get; set; }
        public string MapPath { get; set; }
        public ObservableCollection<SpawnPoint> SpawnPoints { get; set; } = new();
    }

    public class SpawnPoint()
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public Thickness Margin { get; set; } = new Thickness(0, 0, 0, 0);

        public ObservableCollection<Pattern> Patterns { get; set; } = new();
    }

    public class Pattern()
    {
        public string Name { get; set; }
        public Uri ImagePath { get; set; }
        public Uri Thumbnail { get; set; }
    }
}
