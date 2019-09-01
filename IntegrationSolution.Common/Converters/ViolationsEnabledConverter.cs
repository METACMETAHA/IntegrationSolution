using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace IntegrationSolution.Common.Converters
{
    public class ViolationsEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((int)value == 0) ? false : true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
