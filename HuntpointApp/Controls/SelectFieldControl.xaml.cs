using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

namespace HuntpointApp.Controls
{
    /// <summary>
    /// Interaction logic for InputFieldControl.xaml
    /// </summary>
    public partial class SelectFieldControl : UserControl
    {
        public SelectFieldControl()
        {
            InitializeComponent();
        }

        public bool IsPlaceholderUp
        {
            get { return (bool)GetValue(IsPlaceholderUpProperty); }
            set { SetValue(IsPlaceholderUpProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsPlaceholderUp.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsPlaceholderUpProperty =
            DependencyProperty.Register("IsPlaceholderUp", typeof(bool), typeof(SelectFieldControl), new PropertyMetadata(false));



        public bool IsRequiered
        {
            get { return (bool)GetValue(IsRequieredProperty); }
            set { SetValue(IsRequieredProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsRequiered.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsRequieredProperty =
            DependencyProperty.Register("IsRequiered", typeof(bool), typeof(SelectFieldControl), new PropertyMetadata(false));



        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(SelectFieldControl), new PropertyMetadata(null));


        public object SelectedValue
        {
            get { return (object)GetValue(SelectedValueProperty); }
            set { SetValue(SelectedValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedValueProperty =
            DependencyProperty.Register("SelectedValue", typeof(object), typeof(SelectFieldControl), new PropertyMetadata(null));


        public object SelectedCurrentItem
        {
            get { return (object)GetValue(SelectedCurrentItemProperty); }
            set { SetValue(SelectedCurrentItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedCurrentItemProperty =
            DependencyProperty.Register("SelectedCurrentItem", typeof(object), typeof(SelectFieldControl), new PropertyMetadata(null, OnSelectedCurrentItemChangedCallBack));
        //

        private static void OnSelectedCurrentItemChangedCallBack(
        DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            SelectFieldControl c = sender as SelectFieldControl;
            if (c != null)
            {
                c.OnSelectedItemChanged(c.SelectedCurrentItem);
            }
        }
       

        protected virtual void OnSelectedItemChanged(object item)
        {
            // Grab related data.
            // Raises INotifyPropertyChanged.PropertyChanged
            if (item != null)
            {
                if (!IsPlaceholderUp)
                {                    
                    var sb = FindResource("goUp") as Storyboard;
                    sb.Begin();
                    IsPlaceholderUp = true;
                }
            }
            else
            {
                if (IsPlaceholderUp)
                {
                    var sb = FindResource("goDown") as Storyboard;
                    sb.Begin();
                    IsPlaceholderUp = false;
                }
            }
        }


        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Label.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(SelectFieldControl), new PropertyMetadata(""));


        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Placeholder.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register("Placeholder", typeof(string), typeof(SelectFieldControl), new PropertyMetadata(string.Empty));

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsReadOnly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(SelectFieldControl), new PropertyMetadata(false));


        public bool IsError
        {
            get { return (bool)GetValue(IsErrorProperty); }
            set { SetValue(IsErrorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsError.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsErrorProperty =
            DependencyProperty.Register("IsError", typeof(bool), typeof(SelectFieldControl), new PropertyMetadata(false));




        public string ErrorMessage
        {
            get { return (string)GetValue(ErrorMessageProperty); }
            set { SetValue(ErrorMessageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ErrorMessage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ErrorMessageProperty =
            DependencyProperty.Register("ErrorMessage", typeof(string), typeof(SelectFieldControl), new PropertyMetadata(string.Empty));



        public string DisplayProperty
        {
            get { return (string)GetValue(DisplayPropertyProperty); }
            set { SetValue(DisplayPropertyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisplayProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayPropertyProperty =
            DependencyProperty.Register("DisplayProperty", typeof(string), typeof(SelectFieldControl), new PropertyMetadata("Name"));




        public string ValueProperty
        {
            get { return (string)GetValue(ValuePropertyProperty); }
            set { SetValue(ValuePropertyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ValueProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValuePropertyProperty =
            DependencyProperty.Register("ValueProperty", typeof(string), typeof(SelectFieldControl), new PropertyMetadata("Value"));


        private void ComboBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!IsPlaceholderUp)
            {
                var sb = FindResource("goUp") as Storyboard;
                sb.Begin();
                IsPlaceholderUp = true;
            }
        }

        private void ComboBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if ((sender as ComboBox).SelectedItem == null)
            {
                if (IsPlaceholderUp)
                {
                    var sb = FindResource("goDown") as Storyboard;
                    plc.Text = Placeholder;
                    sb.Begin();
                    IsPlaceholderUp = false;
                }
            }
        }
    }
}
