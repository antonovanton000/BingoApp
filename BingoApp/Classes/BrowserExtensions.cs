using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using Microsoft.Web.WebView2.Wpf;

namespace BingoApp.Classes
{
    public class BrowserBehavior
    {
        public static readonly DependencyProperty HtmlProperty = DependencyProperty.RegisterAttached(
                "Html",
                typeof(string),
                typeof(BrowserBehavior),
                new FrameworkPropertyMetadata(OnHtmlChanged));

        [AttachedPropertyBrowsableForType(typeof(WebView2))]
        public static string GetHtml(WebView2 d)
        {
            return (string)d.GetValue(HtmlProperty);
        }

        public static void SetHtml(WebView2 d, string value)
        {
            d.SetValue(HtmlProperty, value);
        }

        static async void OnHtmlChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            WebView2 webBrowser = dependencyObject as WebView2;
            if (webBrowser != null)
            {
                await webBrowser.EnsureCoreWebView2Async();
                webBrowser.NavigateToString(e.NewValue as string ?? "&nbsp;");
            }
        }
    }
}
