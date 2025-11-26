using HuntpointApp.Models;
using HuntpointApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace HuntpointApp.Classes;

public class NullToVisibility : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return Visibility.Visible;

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Visibility.Visible;
    }
}

public class NullToBool : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return false;

        return true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}

public class NotNullToBool : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return true;

        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}

public class NotNullToVisibility : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return Visibility.Collapsed;

        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Visibility.Collapsed;
    }
}

public class NotBoolToVisibility : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value != null)
        {
            if ((bool)value == true)
                return Visibility.Collapsed;
            else
                return Visibility.Visible;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Visibility.Collapsed;
    }
}

public class NotConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {

        return !((bool)value);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return !((bool)value);

    }
}

public class TooltipTimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var minutes = Math.Round((DateTime.Now - ((DateTime)value)).TotalMinutes, 0);
        if (minutes == 0)
            return "less than minute ago";
        else
        {
            return $"{minutes} minutes ago";
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;

    }
}

public class EqualVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {       
        if (value == parameter)
            return Visibility.Visible;
        else
        
            return Visibility.Collapsed;        
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;

    }
}

public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return Visibility.Collapsed;
        else

            return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;

    }
}

public class NotNullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return Visibility.Visible;
        else

            return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;

    }
}

public class BoolToHiddenConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {

        return ((bool)value) ? Visibility.Visible : Visibility.Hidden;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return !((bool)value);

    }
}

public class CountToMarginConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {

        switch ((int)value)
        {
            case 2:
                return new Thickness(-190, 0, 190, 0);
            case 3:
                return new Thickness(-190, 0, 190, 0);
            case 4:
                return new Thickness(-58, 0, 0, 0);
            case 5:
                return new Thickness(-63, 0, 0, 0);
        }

        return new Thickness();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();

    }
}

public class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {

        if ((int)value > 0)
        {
            return Visibility.Visible;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();

    }
}

public class NotCountToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {

        if ((int)value == 0)
        {
            return Visibility.Visible;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();

    }
}

public class CheckedCollorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (((HuntpointColor)value) == (HuntpointColor)Enum.Parse(typeof(HuntpointColor), parameter.ToString()))
            return true;
        else
            return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();

    }
}

public class StormStateToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        switch ((StormState)value)
        {
            case StormState.Day1Start:
                return Application.Current.FindResource("rp_day1").ToString();
            case StormState.Day1Shrink1:
                return Application.Current.FindResource("rp_day1shrink1").ToString();
            case StormState.Day1AfterShrink:
                return Application.Current.FindResource("rp_day1evening").ToString();
            case StormState.Day1Shrink2:
                return Application.Current.FindResource("rp_day1shrink2").ToString();
            case StormState.BossFight1:
                return Application.Current.FindResource("rp_day1nightboss").ToString();
            case StormState.Day2Start:
                return Application.Current.FindResource("rp_day2").ToString();
            case StormState.Day2Shrink1:
                return Application.Current.FindResource("rp_day2shrink1").ToString();
            case StormState.Day2AfterShrink:
                return Application.Current.FindResource("rp_day2evening").ToString();
            case StormState.Day2Shrink2:
                return Application.Current.FindResource("rp_day2shrink2").ToString();
            case StormState.BossFight2:
                return Application.Current.FindResource("rp_day2nightboss").ToString();
        }
        return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return "";
    }
}

public class HuntpointCollorBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {            
        switch (((HuntpointColor)value))
        {
            case HuntpointColor.orange:
                return Application.Current.FindResource("HuntpointColorOrrange");
            case HuntpointColor.red:
                return Application.Current.FindResource("HuntpointColorRed");
            case HuntpointColor.blue:
                return Application.Current.FindResource("HuntpointColorBlue");
            case HuntpointColor.green:
                return Application.Current.FindResource("HuntpointColorGreen");
            case HuntpointColor.purple:
                return Application.Current.FindResource("HuntpointColorPurple");
            case HuntpointColor.navy:
                return Application.Current.FindResource("HuntpointColorNavy");
            case HuntpointColor.teal:
                return Application.Current.FindResource("HuntpointColorTeal");
            case HuntpointColor.brown:
                return Application.Current.FindResource("HuntpointColorBrown");
            case HuntpointColor.pink:
                return Application.Current.FindResource("HuntpointColorPink");
            case HuntpointColor.yellow:
                return Application.Current.FindResource("HuntpointColorYellow");                
            case HuntpointColor.blank:
                return null;
            default:
                break;
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return !((bool)value);

    }
}

public class HuntpointCollorBackgroundTransparentConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        switch (((HuntpointColor)value))
        {
            case HuntpointColor.orange:
                return Application.Current.FindResource("HuntpointColorOrrangeAlpha20");
            case HuntpointColor.red:
                return Application.Current.FindResource("HuntpointColorRedAlpha20");
            case HuntpointColor.blue:
                return Application.Current.FindResource("HuntpointColorBlueAlpha20");
            case HuntpointColor.green:
                return Application.Current.FindResource("HuntpointColorGreenAlpha20");
            case HuntpointColor.purple:
                return Application.Current.FindResource("HuntpointColorPurpleAlpha20");
            case HuntpointColor.navy:
                return Application.Current.FindResource("HuntpointColorNavyAlpha20");
            case HuntpointColor.teal:
                return Application.Current.FindResource("HuntpointColorTealAlpha20");
            case HuntpointColor.brown:
                return Application.Current.FindResource("HuntpointColorBrownAlpha20");
            case HuntpointColor.pink:
                return Application.Current.FindResource("HuntpointColorPinkAlpha20");
            case HuntpointColor.yellow:
                return Application.Current.FindResource("HuntpointColorYellowAlpha20");
            case HuntpointColor.blank:
                return null;
            default:
                break;
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return !((bool)value);

    }
}


public class HuntpointCollorNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        switch (((HuntpointColor)value))
        {
            case HuntpointColor.orange:
                return Application.Current.FindResource("bc_orange");
            case HuntpointColor.red:
                return Application.Current.FindResource("bc_red");
            case HuntpointColor.blue:
                return Application.Current.FindResource("bc_blue");
            case HuntpointColor.green:
                return Application.Current.FindResource("bc_green");
            case HuntpointColor.purple:
                return Application.Current.FindResource("bc_purple");
            case HuntpointColor.navy:
                return Application.Current.FindResource("bc_navy");
            case HuntpointColor.teal:
                return Application.Current.FindResource("bc_teal");
            case HuntpointColor.brown:
                return Application.Current.FindResource("bc_brown");
            case HuntpointColor.pink:
                return Application.Current.FindResource("bc_pink");
            case HuntpointColor.yellow:
                return Application.Current.FindResource("pc_yellow");
            case HuntpointColor.blank:
                return "";
            default:
                break;
        }
        return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return !((bool)value);

    }
}

public class HuntpointCollorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        switch (((HuntpointColor)value))
        {
            case HuntpointColor.orange:
                return Application.Current.FindResource("HuntpointColorOrrangeColor");
            case HuntpointColor.red:
                return Application.Current.FindResource("HuntpointColorRedColor");
            case HuntpointColor.blue:
                return Application.Current.FindResource("HuntpointColorBlueColor");
            case HuntpointColor.green:
                return Application.Current.FindResource("HuntpointColorGreenColor");
            case HuntpointColor.purple:
                return Application.Current.FindResource("HuntpointColorPurpleColor");
            case HuntpointColor.navy:
                return Application.Current.FindResource("HuntpointColorNavyColor");
            case HuntpointColor.teal:
                return Application.Current.FindResource("HuntpointColorTealColor");
            case HuntpointColor.brown:
                return Application.Current.FindResource("HuntpointColorBrownColor");
            case HuntpointColor.pink:
                return Application.Current.FindResource("HuntpointColorPinkColor");
            case HuntpointColor.yellow:
                return Application.Current.FindResource("HuntpointColorYellowColor");
            case HuntpointColor.blank:
                return null;
            default:
                break;
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return !((bool)value);

    }
}


//public class MinusConverter : IValueConverter
//{
//    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
//    {
//        var p = parameter as TemplateBindingExpression;
//        p.TemplateBindingExtension.Property.
//        var val = p.TemplateBindingExtension.ProvideValue(null);
//        var max = ((ProgressBar)parameter).Maximum;

//        return (max) - ((double)value);
//    }

//    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
//    {
//        return 0;

//    }
//}

public class TrackProgressConverter : IMultiValueConverter
{
    public object Convert(
        object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        return (double)((int)values[0] * 1000 / (long)values[1]);
    }

    public object[] ConvertBack(
        object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class MinusConverter : IMultiValueConverter
{
    public object Convert(
        object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        return ((double)values[0] - (double)values[1]);
    }

    public object[] ConvertBack(
        object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class IsEnumConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var t = value.GetType();
        var val = Enum.ToObject(t, byte.Parse(parameter.ToString()));
        var res = val.Equals(value);
        return res;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return false;

    }
}

public class IsEnumToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var t = value.GetType();
        var val = Enum.ToObject(t, byte.Parse(parameter.ToString()));
        var res = val.Equals(value);
        return res ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Visibility.Collapsed;

    }
}

public class EqualConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var t = value.GetType();
        var res = value.Equals(parameter);
        return res;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return false;

    }
}

public class MilisecondsToTimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var ms = (long)value;
        var str = TimeSpan.FromMilliseconds(ms).ToString(@"mm\:ss\.ff");

        return str;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var str = (string)value;
        var ms = TimeSpan.Parse(str).TotalMilliseconds;
        
        return ms;
    }
}

public class BooleanAndConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        foreach (object value in values)
        {
            if ((value is bool) && (bool)value == false)
            {
                return false;
            }
        }
        return true;
    }
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException("BooleanAndConverter is a OneWay converter.");
    }
}

public class BooleanOrConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        return (values.Any(i => ((i is bool) && ((bool)i == true))));
    }
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException("BooleanAndConverter is a OneWay converter.");
    }
}

public class InverseAndBooleansToBooleanConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (values.LongLength > 0)
        {
            foreach (var value in values)
            {
                if (value is bool && (bool)value)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class PercentageConverter : IValueConverter
{
    public object Convert(object value,
        Type targetType,
        object parameter,
        System.Globalization.CultureInfo culture)
    {
        return System.Convert.ToDouble(value) *
               System.Convert.ToDouble(parameter);
    }

    public object ConvertBack(object value,
        Type targetType,
        object parameter,
        System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
