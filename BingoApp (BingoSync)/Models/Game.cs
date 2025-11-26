using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using BingoApp.Classes;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace BingoApp.Models
{
    public partial class Game: ObservableObject
    {
        [ObservableProperty]
        string name = default!;

        [ObservableProperty]
        ObservableCollection<BoardPreset> presets = [];
        
        public ImageSource ImageCover
        {
            get
            {
                var bi = new BitmapImage();
                var imgPath = System.IO.Path.Combine(App.Location, "GamesImages", $"{Name}.jpg");
                if (System.IO.File.Exists(imgPath))
                {
                    bi = ImageSourceHelpers.ByteArrayToImageSource(System.IO.File.ReadAllBytes(imgPath));
                }
                else
                {
                    bi = new BitmapImage(new Uri("pack://application:,,,/Images/presetcard.png"));
                }

                return bi;
            }
        }

        public string? ImageCoverPath
        {
            get
            {
                var imgPath = System.IO.Path.Combine(App.Location, "GamesImages", $"{Name}.jpg");
                if (System.IO.File.Exists(imgPath))
                {
                    return imgPath;
                }
                else
                {
                    return null;
                }
            }
        }

        [ObservableProperty]
        bool isWebLinkBad;

        [ObservableProperty]
        bool isDownloadError;

        [ObservableProperty]
        bool isGameNameError;

        [ObservableProperty]
        string? coverFilePath;

        [ObservableProperty]
        string? coverWebLink;

    }
}
