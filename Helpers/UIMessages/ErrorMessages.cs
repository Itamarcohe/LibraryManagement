using System;

namespace LibraryManagement.Helpers.UIMessages
{
    public static class ErrorMessages
    {

        
        public static string FillAllFields => "Please fill in all fields.";
        public static string WrongPassword => "Wrong password.";
        public static string UserDoesNotExist => "User does not exist.";
        public static string UserAlreadyExists => "A user with that username already exists.";
        public static string DiscountPriceOutOfRange => "Discount price must be between 0 and 100 (digits only).";
        public static string GreaterThanZero => "Please enter a valid discount/price greater than 0.";
        public static string InvalidInputDigitsOnly => "Invalid input. Digits only are allowed.";
        public static string PasswordsDoNotMatch => "Passwords do not match.";
        public static string DigitsOnly => "Prices must be digits only";
        public static string MinMaxPrices => "Min/Max Prices must be none negative digits only";
        public static string FutureDateTime => "Please enter a valid date that is not in the future.";
        public static string ItemNotAvailableUntil(DateTime date) => $"This item is not available until {date:d}.";
        public static string AuthorMissing => "Please enter Author name.";
        public static string GenreMissing => "Please choose Genre name.";
        public static string LoginRequired => "You Must login first.";
        public static string MinPriceGreaterThanMaxPrice => "Min Price cannot be greater than the max price.";
        public static string MinMaxPriceLessThanOne => "Min/Max Cannot be less than 1.";


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
