using System;
using System.Text.Json.Serialization;

namespace LibraryManagement.Models
{
    public class Magazine : Item
    {
        public Magazine(string name, double price, DateTime publishDate, string publisher, Genre genre, double discountPrice) : base(name, price, publishDate, publisher, genre, discountPrice)
        {
        }

        [JsonConstructor]
        public Magazine(double price) : base(price)
        {
        }

    }
}
