using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BingoApp.Models
{
    public partial class BoardPreset : ObservableObject
    {
        [ObservableProperty]
        string presetName;

        [ObservableProperty]
        string filePath;


        [ObservableProperty]
        string json;

        [ObservableProperty]
        int squareCount;

        [ObservableProperty]
        ObservableCollection<PresetSquare> squares = new ObservableCollection<PresetSquare>();

        [JsonIgnore]
        [ObservableProperty] 
        string coverFilePath;

        [JsonIgnore]
        [ObservableProperty]
        string coverWebLink;

        [JsonIgnore]
        public ImageSource ImageCover
        {
            get
            {
                var bi = new BitmapImage();
                var imgPath = System.IO.Path.Combine(App.Location,"PresetImages", PresetName + ".jpg");
                if (System.IO.File.Exists(imgPath))
                {
                    bi = ToImage(System.IO.File.ReadAllBytes(imgPath));
                }
                else
                {
                    bi = new BitmapImage(new Uri("pack://application:,,,/Images/presetcard.png"));
                }

                return bi;
            }
        }

        private BitmapImage ToImage(byte[] array)
        {
            using (var ms = new System.IO.MemoryStream(array))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // here
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }

        public static byte[] ExtractResource(string filename)
        {
            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            using (Stream resfilestream = a.GetManifestResourceStream(filename))
            {
                if (resfilestream == null) return null;
                byte[] ba = new byte[resfilestream.Length];
                resfilestream.Read(ba, 0, ba.Length);
                return ba;
            }
        }

        public void RefreshImageCover()
        {
            OnPropertyChanged(nameof(ImageCover));
        }

        [JsonIgnore]
        [ObservableProperty]
        bool isPresetNameError;

        [JsonIgnore]
        [ObservableProperty]
        bool isJsonEmpty;

        [JsonIgnore]
        [ObservableProperty]
        bool isJsonError;

        [JsonIgnore]
        [ObservableProperty]
        bool isWebLinkBad;

        [JsonIgnore]
        [ObservableProperty]
        bool isDownloadError;
    }
}
