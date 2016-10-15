#undef UI_DESIGN

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TextEditV5
{
    /// <summary>
    /// 将Bool转化为Visibility（单向）
    /// Bool通常是CheckBox或RadioButton的IsChecked.Value
    /// True -> Visible
    /// False -> Collapsed
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
#if UI_DESIGN
            // Always return visible will make it easy to design UI
            return Visibility.Visible;
#else
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
#endif
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// 将Bool转化为Visibility（单向）
    /// Bool通常是CheckBox或RadioButton的IsChecked.Value
    /// True -> Collapsed
    /// False -> Visible
    /// </summary>
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
#if UI_DESIGN
            // Always return visible will make it easy to design UI
            return Visibility.Visible;
#else
            return (bool)value ? Visibility.Collapsed : Visibility.Visible;
#endif
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// 将表示插入位置的String转化为Visibility（单向）
    /// String通常是TextBox的Text，主要来源于Position
    /// String可转为 int -> Collapsed
    /// String不可转为 int -> Visiblt
    /// </summary>
    public class PositionNumberToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
#if UI_DESIGN
            // Always return visible will make it easy to design UI
            return Visibility.Visible;
#else
            int position;
            if (!int.TryParse((string)value, out position)) return Visibility.Visible;
            else return Visibility.Collapsed;
#endif
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// 将多个String所代表的数字转化为Visibility（单向）
    /// String通常是TextBox的Text，主要来源于隔行删除/插入功能的行数
    /// 全部String可转为正整数 -> Collapsed
    /// 任意String不可转为正整数 -> Visible
    /// </summary>
    public class AndMultiNumberToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
#if UI_DESIGN
            // Always return visible will make it easy to design UI
            return Visibility.Visible;
#else
            foreach(object value in values)
            {
                int n;
                if (!int.TryParse((string)value, out n)) return Visibility.Visible;
                if (n < 1) return Visibility.Visible;
            }
            return Visibility.Collapsed;
#endif
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// 将多个Bool转化为Visibility（单向）
    /// Bool通常是CheckBox或RadioButton的IsChecked.Value
    /// 全为  True -> Visible
    /// 任意 False -> Collapsed
    /// </summary>
    public class AndMultiBoolToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
#if UI_DESIGN
            // Always return visible will make it easy to design UI
            return Visibility.Visible;
#else
            foreach (object value in values)
            {
                if (!(bool)value) return Visibility.Collapsed;
            }
            return Visibility.Visible;
#endif
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// 将多个Bool转化为Visibility（单向）
    /// Bool通常是CheckBox或RadioButton的IsChecked.Value
    /// 任意  True -> Visible
    /// 全为 False -> Collapsed
    /// </summary>
    public class OrMultiBoolToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
#if UI_DESIGN
            // Always return visible will make it easy to design UI
            return Visibility.Visible;
#else
            foreach(object value in values)
            {
                if ((bool)value) return Visibility.Visible;
            }
            return Visibility.Collapsed;
#endif
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
