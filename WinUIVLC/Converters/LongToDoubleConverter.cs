using Microsoft.UI.Xaml.Data;

namespace WinUIVLC.Converters;

public class LongToDoubleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is long longValue)
        {
            // Convert the long to a double
            var doubleValue = (double)longValue;
            return doubleValue;
        }

        // If the value is not a long, return null or some default value
        return 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is double doubleValue)
        {
            // Convert the double back to a long
            var longValue = (long)doubleValue;
            return longValue;
        }

        // If the value is not a double, return null or some default value
        return 0;
    }
}
