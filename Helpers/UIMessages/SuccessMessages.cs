using LibraryManagement.Models;
using System;
namespace LibraryManagement.Helpers.UIMessages
{
    public static class SuccessMessages
    {
        public static string SignedUp => "Greetings {0}, You have Successfully Signed Up.";
        public static string ItemReturn => "Thanks the item {0} Successfully Returned to the library.";
        public static string ItemRented => "Greetings {0} You have successfully rented: \n {1}, \n Return Date: {2}";
        public static string ItemEdited => "{0} You have successfully edited:\n" +
            "New Price:                {1},\n" +
            "Discounted Price:      {2},\n" +
            "Discount Percentage:  {3}";

        public static string FormatMessage(string message, params object[] args)
        {
            try
            {
                return string.Format(message, args);
            }
            catch (Exception)
            {
                return message;
            }

        }
    }
}
