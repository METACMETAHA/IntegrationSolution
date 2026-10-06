using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace IntegrationSolution.Localization.Markup
{
    /// <summary>
    /// values[0] is a composite format string, values[1..n] are its arguments.
    /// The <paramref name="culture"/> supplied by WPF is used for the arguments, i.e. the same
    /// culture <c>Binding.StringFormat</c> would use, so values look exactly as before localization.
    /// </summary>
    public sealed class LocalizedFormatConverter : IMultiValueConverter
    {
        public static readonly LocalizedFormatConverter Instance = new LocalizedFormatConverter();

        /// <param name="parameter"><c>true</c> to return <c>null</c> (and so the TargetNullValue) when any argument is <c>null</c>.</param>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length == 0)
                return DependencyProperty.UnsetValue;

            var format = values[0] as string;
            if (format == null)
                return DependencyProperty.UnsetValue;

            var args = values.Skip(1).ToArray();
            if (args.Any(x => x == DependencyProperty.UnsetValue || x == BindingOperations.DisconnectedSource))
                return DependencyProperty.UnsetValue;

            if (parameter is bool && (bool)parameter && args.Any(x => x == null))
                return null;

            try
            {
                return string.Format(culture ?? CultureInfo.CurrentCulture, format, args);
            }
            catch (FormatException ex)
            {
                Trace.TraceError($"Localization: invalid composite format '{format}'. {ex.Message}");
                return format;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
