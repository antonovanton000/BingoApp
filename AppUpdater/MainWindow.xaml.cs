using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
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
using System.Xml.Linq;

namespace AppUpdater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string ServerPath = "https://bingoapp.injusteam.kz";

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var bingoAppProcess = Process.GetProcessesByName("BingoApp");
                if (bingoAppProcess.Length > 0)
                {
                    foreach (var process in bingoAppProcess)
                    {
                        process.Kill();
                    }
                }
                await Task.Delay(1000);
                var httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(ServerPath);
                var json = await httpClient.GetStringAsync("/app/lastversion");

                var jobj = JObject.Parse(json);
                if (jobj != null)
                {
                    var link = jobj.Value<string>("updatelink");                    
                    var version = jobj.Value<string>("version");                    
                    using var s = await httpClient.GetStreamAsync(link);
                    using var fs = new FileStream($"{version}.zip", FileMode.Create);
                    await s.CopyToAsync(fs);
                    fs.Close();
                    s.Close();

                    using var archive = System.IO.Compression.ZipFile.Open($"{version}.zip", System.IO.Compression.ZipArchiveMode.Read);
                    foreach (var item in archive.Entries)
                    {
                        if (item.FullName.Contains(@"/"))
                        {
                            var folderName = System.IO.Path.GetDirectoryName(item.FullName);
                            if (!Directory.Exists(folderName))
                                Directory.CreateDirectory(folderName);
                        }
                        if (item.Length>0)
                            item.ExtractToFile(item.FullName, true);
                    }
                    archive.Dispose();

                    await Task.Delay(1000);
                    Process.Start("BingoApp.exe");
                    await Task.Delay(500);
                    File.Delete($"{version}.zip");
                    App.Current.Shutdown();
                }
                else
                {
                    App.Current.Shutdown();
                }



            }
            catch (Exception ex)
            {
                File.WriteAllText("update.log", ex.Message);
                App.Current.Shutdown();
            }

        }
    }
}