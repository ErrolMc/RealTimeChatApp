using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ChatAppFrontEnd.Source.Converters  // Ensure this matches your project's namespace
{
    public class EmptyStringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Check if the value is a string and if it is empty or null
            if (value is string text)
            {
                // Return true if the string is empty, which means the watermark should be visible
                return string.IsNullOrEmpty(text);
            }

            // Return false for any other cases
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // ConvertBack is not implemented since we don't need it
            throw new NotImplementedException();
        }
    }
}