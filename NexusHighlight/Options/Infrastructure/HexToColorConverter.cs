using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace NexusHighlight.Options
{
    public class HexToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string hexColor && !string.IsNullOrEmpty(hexColor))
            {
                try
                {
                    return ColorConverter.ConvertFromString(hexColor) as Color? ?? Colors.Transparent;
                }
                catch
                {
                    return Colors.Transparent;
                }
            }
            return Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
            }
            return null;
        }
    }
}
