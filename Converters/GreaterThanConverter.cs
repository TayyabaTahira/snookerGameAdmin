using System;
using System.Globalization;
using System.Windows.Data;

namespace SnookerGameManagementSystem.Converters
{
    public class GreaterThanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            try
            {
                decimal numericValue = System.Convert.ToDecimal(value);
                decimal compareValue = System.Convert.ToDecimal(parameter);
                return numericValue > compareValue;
            }
            catch
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
