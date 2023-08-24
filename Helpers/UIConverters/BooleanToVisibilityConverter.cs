using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml;

namespace LibraryManagement.Helpers.UIConverters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool boolValue && targetType == typeof(Visibility))
            {

                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }

            throw new ArgumentException("Invalid conversion or target type.");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}


