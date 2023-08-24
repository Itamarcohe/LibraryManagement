using System;
using System.Linq;

namespace LibraryManagement.Helpers
{
    public static class MyUtils
    {
        public static bool AreStringsNullOrEmpty(params string[] values)
        {
            return values.Any(string.IsNullOrEmpty);
        }

        public static bool AreObjectsNull(params object[] values)
        {
            return values.Any(value => value == null);
        }

        public static double RoundToTwoDecimalPlaces(double number)
        {
            return Math.Round(number, 2);
        }

    }
}



