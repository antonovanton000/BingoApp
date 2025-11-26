using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace WheelGame.Controls
{
    /// <summary>
    /// Interaction logic for InputFieldControl.xaml
    /// </summary>
    public partial class InputFieldControl : UserControl
    {
        public InputFieldControl()
        {
            InitializeComponent();
            Loaded += InputFieldControl_Loaded;
            
        }

        private void InputFieldControl_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        public bool IsPlaceholderUp
        {
            get { return (bool)GetValue(IsPlaceholderUpProperty); }
            set { SetValue(IsPlaceholderUpProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsPlaceholderUp.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsPlaceholderUpProperty =
            DependencyProperty.Register("IsPlaceholderUp", typeof(bool), typeof(InputFieldControl), new PropertyMetadata(false));



        public bool IsRequiered
        {
            get { return (bool)GetValue(IsRequieredProperty); }
            set { SetValue(IsRequieredProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsRequiered.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsRequieredProperty =
            DependencyProperty.Register("IsRequiered", typeof(bool), typeof(InputFieldControl), new PropertyMetadata(false));


        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(InputFieldControl), new PropertyMetadata(string.Empty, OnTextChangedCallBack));

        private static void OnTextChangedCallBack(
        DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            InputFieldControl c = sender as InputFieldControl;
            if (c != null)
            {
                c.OnTextChanged(c.Text);
            }
        }

        
        protected virtual void OnTextChanged(string text)
        {
            // Grab related data.
            // Raises INotifyPropertyChanged.PropertyChanged
            if(string.IsNullOrEmpty(text))
            {
                IsPlaceholderUp = false;
            }
            if (!string.IsNullOrEmpty(text) && !IsPlaceholderUp)
            {
                plc.Text = Label;
                var sb = FindResource("goUp") as Storyboard;
                sb.Begin();
                IsPlaceholderUp = true;
            }            
        }

        public string Password
        {
            get { return (string)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Password.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register("Password", typeof(string), typeof(InputFieldControl), new PropertyMetadata("", OnPasswordChangedCallBack));


        private static void OnPasswordChangedCallBack(
        DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            InputFieldControl c = sender as InputFieldControl;
            if (c != null)
            {
                c.OnPasswordChanged(c.Password);
            }
        }

        protected virtual void OnPasswordChanged(string text)
        {
            // Grab related data.
            // Raises INotifyPropertyChanged.PropertyChanged
            if (!string.IsNullOrEmpty(text) && !IsPlaceholderUp)
            {
                plc.Text = Label;
                var sb = FindResource("goUp") as Storyboard;
                sb.Begin();
                pbx.Password = text;
                IsPlaceholderUp = true;
            }
        }


        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Label.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(InputFieldControl), new PropertyMetadata(""));


        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Placeholder.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register("Placeholder", typeof(string), typeof(InputFieldControl), new PropertyMetadata(string.Empty));


        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsReadOnly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(InputFieldControl), new PropertyMetadata(false));



        public bool IsError
        {
            get { return (bool)GetValue(IsErrorProperty); }
            set { SetValue(IsErrorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsError.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsErrorProperty =
            DependencyProperty.Register("IsError", typeof(bool), typeof(InputFieldControl), new PropertyMetadata(false));




        public string ErrorMessage
        {
            get { return (string)GetValue(ErrorMessageProperty); }
            set { SetValue(ErrorMessageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ErrorMessage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ErrorMessageProperty =
            DependencyProperty.Register("ErrorMessage", typeof(string), typeof(InputFieldControl), new PropertyMetadata(string.Empty));



        public bool IsPassword
        {
            get { return (bool)GetValue(IsPasswordProperty); }
            set { SetValue(IsPasswordProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsPassword.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsPasswordProperty =
            DependencyProperty.Register("IsPassword", typeof(bool), typeof(InputFieldControl), new PropertyMetadata(false));




        public bool IsNumberOnly
        {
            get { return (bool)GetValue(IsNumberOnlyProperty); }
            set { SetValue(IsNumberOnlyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsNumberOnly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsNumberOnlyProperty =
            DependencyProperty.Register("IsNumberOnly", typeof(bool), typeof(InputFieldControl), new PropertyMetadata(false));




        public Color PlaceholderBackgroundColor
        {
            get { return (Color)GetValue(PlaceholderBackgroundColorProperty); }
            set { SetValue(PlaceholderBackgroundColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PlaceholderBackgroundColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PlaceholderBackgroundColorProperty =
            DependencyProperty.Register("PlaceholderBackgroundColor", typeof(Color), typeof(InputFieldControl), new PropertyMetadata(Color.FromRgb(0x1a,0x1b,0x1c)));




        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            plc.Text = Label;
            if (!IsPlaceholderUp)
            {
                var sb = FindResource("goUp") as Storyboard;
                sb.Begin();
                IsPlaceholderUp = true;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var sb = FindResource("goDown") as Storyboard;
            if ((sender as TextBox).Text == "")
            {
                if (IsPlaceholderUp)
                {
                    plc.Text = Placeholder;
                    sb.Begin();
                    IsPlaceholderUp = false;
                }
            }
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            plc.Text = Label;
            if (!IsPlaceholderUp)
            {
                var sb = FindResource("goUp") as Storyboard;
                sb.Begin();
                IsPlaceholderUp = true;
            }
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var sb = FindResource("goDown") as Storyboard;
            if ((sender as PasswordBox).Password == "")
            {
                if (IsPlaceholderUp)
                {
                    plc.Text = Placeholder;
                    sb.Begin();
                    IsPlaceholderUp = false;
                }

            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null && (sender as PasswordBox).IsFocused)
            {
                Password = ((PasswordBox)sender).Password;
                Text = ((PasswordBox)sender).Password;
            }
        }

        private void Button_MouseDown(object sender, MouseButtonEventArgs e)
        {
            psw_tbl.Text = pbx.Password;
            psw_tbl.Visibility = Visibility.Visible;
            pbx.Opacity = 0;
        }

        private void Button_MouseUp(object sender, MouseButtonEventArgs e)
        {
            psw_tbl.Text = "";
            psw_tbl.Visibility = Visibility.Collapsed;
            pbx.Opacity = 1;
        }

        private static readonly Regex _regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        private void tbx_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (IsNumberOnly)
            {
                e.Handled = !IsTextAllowed(e.Text);
            }
        }

        private void TextBoxPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (IsNumberOnly)
            {
                if (e.DataObject.GetDataPresent(typeof(String)))
                {
                    String text = (String)e.DataObject.GetData(typeof(String));
                    if (!IsTextAllowed(text))
                    {
                        e.CancelCommand();
                    }
                }
                else
                {
                    e.CancelCommand();
                }
            }
        }

    }
}
