using BingoApp.Classes;
using BingoApp.Models;
using BingoApp.Views;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
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

namespace BingoApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static MainWindow instance;


        public MainWindow()
        {
            InitializeComponent();
            instance = this;

        }

        bool needClean;
        public bool NeedClean { get { return needClean; } set { needClean = value; } }

        public static void ClearHistory()
        {
            if (!instance.frame.CanGoBack && !instance.frame.CanGoForward)
            {
                return;
            }

            var entry = instance.frame.RemoveBackEntry();
            while (entry != null)
            {
                entry = instance.frame.RemoveBackEntry();
            }

            instance.frame.Navigate(new PageFunction<string>() { RemoveFromJournal = true });
            instance.NeedClean = true;
        }

        public static void NavigateTo(Page page)
        {
            instance.frame.Navigate(page);
        }

        public static void GoBack()
        {
            instance.frame.GoBack();
        }

        public static void HideSettingsButton()
        {
            instance.btnSettingsGrid.Visibility = Visibility.Collapsed;
        }

        public static void ShowSettingsButton()
        {
            instance.btnSettingsGrid.Visibility = Visibility.Visible;
        }

        #region NotificationStuff
        private Action? _yesCallback;
        private Action? _noCallback;
        private Action? _okCallback;

        private Action<string> _promtCallback;
        public static void ShowMessage(string messageText, MessageNotificationType messageType, Action? yesCallback = null, Action? noCallback = null, Action? okCallback = null)
        {
            instance.tblNotificationMessage.Text = messageText;
            switch (messageType)
            {
                case MessageNotificationType.Ok:
                    instance.btnNotificationOk.Visibility = Visibility.Visible;
                    instance.btnNotificationYes.Visibility = Visibility.Collapsed;
                    instance.btnNotificationNo.Visibility = Visibility.Collapsed;
                    break;
                case MessageNotificationType.YesNo:
                    instance.btnNotificationOk.Visibility = Visibility.Collapsed;
                    instance.btnNotificationYes.Visibility = Visibility.Visible;
                    instance.btnNotificationNo.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
            instance._yesCallback = yesCallback;
            instance._noCallback = noCallback;
            instance._okCallback = okCallback;
            instance.NotificationGrid.Visibility = Visibility.Visible;
        }

        private void Notification_Yes_Click(object sender, RoutedEventArgs e)
        {
            NotificationGrid.Visibility = Visibility.Collapsed;
            _yesCallback?.Invoke();
        }

        private void Notification_No_Click(object sender, RoutedEventArgs e)
        {
            NotificationGrid.Visibility = Visibility.Collapsed;
            _noCallback?.Invoke();

        }

        private void Notification_Ok_Click(object sender, RoutedEventArgs e)
        {
            NotificationGrid.Visibility = Visibility.Collapsed;
            _okCallback?.Invoke();
        }

        public static void CloseMessage()
        {            
            instance.NotificationGrid.Visibility = Visibility.Collapsed;
        }

        public static void ShowErrorMessage(string errorMessage, string page, string procedure)
        {
            instance.tbxErrorMessage.Text = $"Страница: {page} \r\nПроцедура: {procedure}\r\nТекст ошибки: {errorMessage}";
            instance.ErrorMessageGrid.Visibility = Visibility.Visible;
        }

        public static void ShowToast(ToastInfo toastInfo)
        {
            instance.toastGrid.DataContext = toastInfo;
            Storyboard sbShow;
            Storyboard sbHide;
            Storyboard pbAnimation;

            var timer = new DispatcherTimer() { Interval = toastInfo.Duration };            
            var ticksCount = toastInfo.Duration.TotalMilliseconds / 10;
            var tickValue = toastInfo.Duration.TotalMilliseconds / ticksCount;
            switch (toastInfo.ToastType)
            {
                case ToastType.Success:                
                    sbShow = instance.FindResource("showSuccess") as Storyboard;
                    sbHide = instance.FindResource("hideSuccess") as Storyboard;
                    pbAnimation = instance.FindResource("successPBAnimation") as Storyboard;
                    (pbAnimation.Children[0] as DoubleAnimation).Duration = toastInfo.Duration;
                    ((instance.successToast.Child as Grid).Children[0] as Button).Click += (s, e) => {
                        if (timer != null)
                        {
                            instance.BeginStoryboard(sbHide);
                            timer?.Stop();                            
                        }
                    };
                    break;
                case ToastType.Warning:                    
                    sbShow = instance.FindResource("showWarning") as Storyboard;
                    sbHide = instance.FindResource("hideWarning") as Storyboard;
                    pbAnimation = instance.FindResource("warningPBAnimation") as Storyboard;                    
                    ((instance.warningToast.Child as Grid).Children[0] as Button).Click += (s, e) => {
                        if (timer != null)
                        {
                            instance.BeginStoryboard(sbHide);
                            timer.Stop();
                        }
                    };
                    break;
                case ToastType.Error:
                    sbShow = instance.FindResource("showError") as Storyboard;
                    sbHide = instance.FindResource("hideError") as Storyboard;
                    pbAnimation = instance.FindResource("dangerPBAnimation") as Storyboard;
                    ((instance.errorToast.Child as Grid).Children[0] as Button).Click += (s, e) => {
                        if (timer != null)
                        {
                            instance.BeginStoryboard(sbHide);
                            timer.Stop();
                        }
                    };
                    break;
                default:
                    sbShow = instance.FindResource("showSuccess") as Storyboard;
                    sbHide = instance.FindResource("hideSuccess") as Storyboard;
                    pbAnimation = instance.FindResource("successPBAnimation") as Storyboard;
                    break;
            }

            (pbAnimation.Children[0] as DoubleAnimation).Duration = toastInfo.Duration;
            timer.Tick += (s, e) => {
                instance.BeginStoryboard(sbHide);
                timer.Stop();
                timer = null;
            };

            instance.BeginStoryboard(sbShow);
            instance.BeginStoryboard(pbAnimation);
            timer.Start();
        }

        public static void InputDialog(string message, Action<string> okCallback, string inputValue = "", string placeholder = "")
        {
            instance.tblPromtMessage.Text = message;
            instance.tbxPromtBox.Text = inputValue;
            instance.tbxPromtPlaceholder.Text = placeholder;
            instance.PromtGrid.Visibility = Visibility.Visible;
            instance.tbxPromtBox.SelectAll();
            instance._promtCallback = okCallback;
        }

        private void Promt_Ok_Click(object sender, RoutedEventArgs e)
        {
            _promtCallback?.Invoke(tbxPromtBox.Text);
            PromtGrid.Visibility = Visibility.Collapsed;
        }

        private void Promt_Cancel_Click(object sender, RoutedEventArgs e)
        {
            PromtGrid.Visibility = Visibility.Collapsed;
            _promtCallback = null;
        }

        private void tbxPromtBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _promtCallback?.Invoke(tbxPromtBox.Text);
                PromtGrid.Visibility = Visibility.Collapsed;
            }
        }


        #endregion

        #region EvenHandlers        

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            if (frame.CanGoBack)
                frame.GoBack();
        }

        private void ErrorCopy_Click(object sender, RoutedEventArgs e)
        {
            tbxErrorMessage.Copy();
        }

        private void ErrorOk_Click(object sender, RoutedEventArgs e)
        {
            ErrorMessageGrid.Visibility = Visibility.Collapsed;
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    StartAuthenticate();
            //    db = new DbModels.VNSContext();
            //    var timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(200) };
            //    timer.Tick += async (s, e) =>
            //    {
            //        timer.Stop();
            //        await db.Products.FirstOrDefaultAsync();
            //        pbAwaiter.Visibility = Visibility.Collapsed;
            //        spButtons.Visibility = Visibility.Visible;
            //    };
            //    timer.Start();
            //}
            //catch (Exception ex)
            //{
            //    System.IO.File.WriteAllText("errors.txt", ex.Message);
            //}
        }
        private void frame_Navigated(object sender, NavigationEventArgs e)
        {
            if (NeedClean)
            {
                frame.JournalOwnership = JournalOwnership.OwnsJournal;
                frame.NavigationService.RemoveBackEntry();
            }
            NeedClean = false;
        }
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Expand_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
                this.WindowState = WindowState.Maximized;
            else
                this.WindowState = WindowState.Normal;
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var settingsPage = new SettingsPage();
            NavigateTo(settingsPage);
        }


        #endregion

    }

    public enum MessageNotificationType
    {
        Ok,
        YesNo
    }

    public enum ToastType
    {
        Success,
        Warning,
        Error
    }

    public class ToastInfo
    {
        public ToastInfo()
        {
            Title = "";
            Detail = "";
            ToastType = ToastType.Success;
            Duration = TimeSpan.FromMilliseconds(2000);
        }

        public ToastInfo(string title, string detail, ToastType toastType = ToastType.Success, int durationMs = 3000)
        {
            Title = title;
            Detail = detail;
            ToastType = toastType;
            Duration = TimeSpan.FromMilliseconds(durationMs);
        }
        public string Title { get; set; }
        public string Detail { get; set; }
        public ToastType ToastType { get; set; }
        public TimeSpan Duration { get; set; }
    }

}
