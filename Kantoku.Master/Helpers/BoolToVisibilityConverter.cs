using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Kantoku.Master.Helpers
{
    public class BoolToVisibilityConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var visibility = Visibility.Hidden;
            var flag = (bool)value;

            if (parameter is string b && b == "true")
                flag = !flag;

            // Set default visibility if parameter is set
            if (parameter is Visibility)
                visibility = (Visibility)parameter;

            return flag ? Visibility.Visible : visibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
