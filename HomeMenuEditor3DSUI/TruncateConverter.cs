using System;
using System.Globalization;
using System.Windows.Data;

namespace HomeMenuEditor3DSUI
{
    public class TruncateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int maxLength = 200; // Default max length
            if (parameter != null && int.TryParse(parameter.ToString(), out int paramLength))
            {
                maxLength = paramLength;
            }

            if (value is string text)
            {
                if (text.Length > maxLength)
                {
                    return text.Substring(0, maxLength) + "...";
                }
                else
                {
                    return text;
                }
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
