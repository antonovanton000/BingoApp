using System.IO;
using System.Windows.Media.Imaging;


namespace BingoApp.Classes
{
    public class ImageSourceHelpers
    {
        public static BitmapImage ByteArrayToImageSource(byte[] byteArray)
        {
            using (var ms = new System.IO.MemoryStream(byteArray))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // here
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }

        public static BitmapImage? ExtractImageResource(string filename)
        {
            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            using (Stream? resfilestream = a.GetManifestResourceStream(filename))
            {
                if (resfilestream == null) return null;
                byte[] ba = new byte[resfilestream.Length];
                resfilestream.ReadAsync(ba, 0, ba.Length);
                return ByteArrayToImageSource(ba);
            }
        }

    }
}
