using System.Collections;
using System.Globalization;

namespace CodedByKay.PowerPatrol.Converters
{
    public class EmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Shows the control when the collection is empty
            if (value is null || value is ICollection collection && collection.Count == 0)
                return true; // Visible
            else
                return false; // Not visible (hide when there are items)
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
