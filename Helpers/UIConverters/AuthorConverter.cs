using LibraryManagement.Models;
using System;
using Windows.UI.Xaml.Data;

namespace LibraryManagement.Helpers.UIConverters
{

    public class AuthorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Book book)
            {
                return book.Author;
            }
            else if (value is Magazine)
            {
                return "Magazine";
            }
            return null; 
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
