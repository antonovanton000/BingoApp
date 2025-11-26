using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

public static class VisualHelper
{
    public static void CopyElementToClipboard(UIElement element)
    {
        if (element == null) return;

        // Определяем размер элемента
        var bounds = VisualTreeHelper.GetDescendantBounds(element);
        var dpi = 96d;

        // Рендерим элемент в bitmap
        var rtb = new RenderTargetBitmap(
            (int)bounds.Width, (int)bounds.Height,
            dpi, dpi, PixelFormats.Pbgra32);

        var dv = new DrawingVisual();
        using (var ctx = dv.RenderOpen())
        {
            ctx.DrawRectangle(new VisualBrush(element), null, new Rect(new Point(), bounds.Size));
        }
        rtb.Render(dv);

        // Копируем в буфер обмена
        Clipboard.SetImage(rtb);
    }
}
