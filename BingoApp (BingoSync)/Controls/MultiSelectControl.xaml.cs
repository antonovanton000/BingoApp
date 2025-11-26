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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BingoApp.Controls
{
    /// <summary>
    /// Interaction logic for MultiSelectControl.xaml
    /// </summary>
    public partial class MultiSelectControl : UserControl
    {

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { 
                SetValue(ItemsSourceProperty, value);
                var type = ItemsSource.GetType().GetGenericArguments().Single();
                Type genericListType = typeof(List<>).MakeGenericType(type);
                SelectedItems = (IList)Activator.CreateInstance(genericListType);
                if (!string.IsNullOrEmpty(GroupingField))
                {
                    CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lv.ItemsSource);
                    PropertyGroupDescription groupDescription = new PropertyGroupDescription(GroupingField);
                    view.GroupDescriptions.Add(groupDescription);
                }
            }
        }

        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(MultiSelectControl), new PropertyMetadata(null));

      
        public IList SelectedItems
        {
            get { return (IList)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItems", typeof(IList), typeof(MultiSelectControl), new PropertyMetadata(null));



        public string GroupingField
        {
            get { return (string)GetValue(GroupingFieldProperty); }
            set { SetValue(GroupingFieldProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GroupingField.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GroupingFieldProperty =
            DependencyProperty.Register("GroupingField", typeof(string), typeof(MultiSelectControl), new PropertyMetadata(null));




        public MultiSelectControl()
        {            
            InitializeComponent();
            
        }


        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            popup.IsOpen = !popup.IsOpen;
            e.Handled = true;
        }
        

        private void Page_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            popup.IsOpen = false;
        }



        public static T FindAncestor<T>(DependencyObject obj)
    where T : DependencyObject
        {
            if (obj != null)
            {
                var dependObj = obj;
                do
                {
                    dependObj = GetParent(dependObj);
                    if (dependObj is T)
                        return dependObj as T;
                }
                while (dependObj != null);
            }

            return null;
        }

        public static DependencyObject GetParent(DependencyObject obj)
        {
            if (obj == null)
                return null;
            if (obj is ContentElement)
            {
                var parent = ContentOperations.GetParent(obj as ContentElement);
                if (parent != null)
                    return parent;
                if (obj is FrameworkContentElement)
                    return (obj as FrameworkContentElement).Parent;
                return null;
            }

            return VisualTreeHelper.GetParent(obj);
        }

        private void control_Loaded(object sender, RoutedEventArgs e)
        {
            var window = FindAncestor<Window>(this);
            if (window != null)
            {
                window.MouseLeftButtonDown += (s, e) => {
                    popup.IsOpen = false;
                };
                window.Deactivated += (s, e) =>
                {
                    popup.IsOpen = false;
                };
            }
        }

        private void lv_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            txt.Text = $"Выбранно элементов: {lv.SelectedItems.Count}";
        }

        private void UncheckGroup_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var cv = btn.DataContext as CollectionViewGroup;
            foreach (var item in cv.Items)
            {
                lv.SelectedItems.Remove(item);
            }
        }

        private void CheckGroup_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var cv = btn.DataContext as CollectionViewGroup;
            foreach (var item in cv.Items)
            {
                lv.SelectedItems.Add(item);
            }            
        }

        public IEnumerable GetSelectedItems()
        {
            return lv.SelectedItems;
        }
    }
}
