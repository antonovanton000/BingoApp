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

namespace BingoApp.Controls
{
    /// <summary>
    /// Interaction logic for InputFieldControl.xaml
    /// </summary>
    public partial class TextAreaFieldControl : UserControl
    {
        public TextAreaFieldControl()
        {
            InitializeComponent();
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TextAreaFieldControl), new PropertyMetadata(string.Empty, OnTextChangedCallBack));

        private static void OnTextChangedCallBack(
        DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            TextAreaFieldControl c = sender as TextAreaFieldControl;
            if (c != null)
            {
                c.OnTextChanged(c.Text);
            }
        }

        bool isPlaceholderUp = false;
        protected virtual void OnTextChanged(string text)
        {
            // Grab related data.
            // Raises INotifyPropertyChanged.PropertyChanged
            if (!string.IsNullOrEmpty(text) && !isPlaceholderUp)
            {
                plc.Text = Label;
                var sb = FindResource("goUp") as Storyboard;
                sb.Begin();
                isPlaceholderUp = true;
            }            
        }

        
        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Label.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(TextAreaFieldControl), new PropertyMetadata(""));


        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Placeholder.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register("Placeholder", typeof(string), typeof(TextAreaFieldControl), new PropertyMetadata(string.Empty));


        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsReadOnly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(TextAreaFieldControl), new PropertyMetadata(false));



        public bool IsError
        {
            get { return (bool)GetValue(IsErrorProperty); }
            set { SetValue(IsErrorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsError.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsErrorProperty =
            DependencyProperty.Register("IsError", typeof(bool), typeof(TextAreaFieldControl), new PropertyMetadata(false));




        public string ErrorMessage
        {
            get { return (string)GetValue(ErrorMessageProperty); }
            set { SetValue(ErrorMessageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ErrorMessage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ErrorMessageProperty =
            DependencyProperty.Register("ErrorMessage", typeof(string), typeof(TextAreaFieldControl), new PropertyMetadata(string.Empty));



        public bool IsPassword
        {
            get { return (bool)GetValue(IsPasswordProperty); }
            set { SetValue(IsPasswordProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsPassword.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsPasswordProperty =
            DependencyProperty.Register("IsPassword", typeof(bool), typeof(TextAreaFieldControl), new PropertyMetadata(false));




        public bool IsNumberOnly
        {
            get { return (bool)GetValue(IsNumberOnlyProperty); }
            set { SetValue(IsNumberOnlyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsNumberOnly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsNumberOnlyProperty =
            DependencyProperty.Register("IsNumberOnly", typeof(bool), typeof(TextAreaFieldControl), new PropertyMetadata(false));




        public Color PlaceholderBackgroundColor
        {
            get { return (Color)GetValue(PlaceholderBackgroundColorProperty); }
            set { SetValue(PlaceholderBackgroundColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PlaceholderBackgroundColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PlaceholderBackgroundColorProperty =
            DependencyProperty.Register("PlaceholderBackgroundColor", typeof(Color), typeof(TextAreaFieldControl), new PropertyMetadata(Color.FromRgb(0x1a,0x1b,0x1c)));




        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            plc.Text = Label;
            if (!isPlaceholderUp)
            {
                var sb = FindResource("goUp") as Storyboard;
                sb.Begin();
                isPlaceholderUp = true;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var sb = FindResource("goDown") as Storyboard;
            if ((sender as TextBox).Text == "")
            {
                if (isPlaceholderUp)
                {
                    plc.Text = Placeholder;
                    sb.Begin();
                    isPlaceholderUp = false;
                }
            }
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
