using System;
using System.Text.Json.Serialization; 

namespace LibraryManagement.Models
{
    public class Book : Item
    {
        public string Author { get; set; }

        public Book(string author, string name, double price, DateTime publishDate, string publisher, Genre genre, double discountPrice)
            : base(name, price, publishDate, publisher, genre, discountPrice)
        {
            this.Author = author;
        }
        [JsonConstructor] 
        public Book(double price) : base(price)
        {

        }
    }
}
