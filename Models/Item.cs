using System;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;
using static LibraryManagement.Helpers.MyUtils;

namespace LibraryManagement.Models
{
    public abstract class Item : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Guid ID { get; set; }

        public double CurrentPrice { get; set; }


        private double discountPercentage;

        public double DiscountPercentage
        {
            get { return discountPercentage; }
            set
            {
                if (value >= 0 && value <= 100)
                {
                    discountPercentage = RoundToTwoDecimalPlaces(value);
                    CurrentPrice = GetCurrentPrice();
                    OnPropertyChanged(nameof(Price));

                }
                else
                {
                    throw new ArgumentException("Discount percentage must be between 0 and 100.");
                }
            }
        }
        public Genre Genre { get; set; }
        public string Name { get; set; }

        private double price;
        public double Price
        {
            get => price;
            set
            {
                if (price != value)
                {
                    price = RoundToTwoDecimalPlaces(value);
                    CurrentPrice = GetCurrentPrice();
                    OnPropertyChanged(nameof(Price));
                }
            }
        }

        public DateTime PublishDate { get; set; }
        public string Publisher { get; set; }
        public DateTime RentDate { get; set; }
        public User Renter { get; set; }
        public DateTime ReturnDate { get; set; }
        public bool IsAvailableToRent { get; set; } = true;

        [JsonConstructor]
        public Item(double price) 
        {
            ID = Guid.NewGuid();
            Price = price; 
        }

        public Item(string name, double price, DateTime publishDate, string publisher, Genre genre, double discountPercentage)
            : this(price) 
        {
            Price = price;
            PublishDate = publishDate;
            Publisher = publisher;
            Genre = genre;
            Name = name;
            DiscountPercentage = discountPercentage;
        }

        public double GetCurrentPrice()
        {
            return RoundToTwoDecimalPlaces(Price * (100 - DiscountPercentage) / 100);
        }

        public void UpdatePrice(double newPrice)
        {
            if (newPrice >= 0)
            {
                Price = newPrice;
            } 
           
        }
        public void UpdateDiscount(double newDiscount)
        {
            DiscountPercentage = newDiscount;
        }
        public bool isLateOnReturn()
        {
            return ReturnDate < DateTime.Now;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"Name: {Name}\n");
            sb.Append($"Genre: {Genre}\n");
            sb.Append($"Publisher: {Publisher}\n");
            sb.Append($"Publish Date: {PublishDate}\n");
            sb.Append($"Current Price: ${CurrentPrice}\n");
            sb.Append($"Full Price: ${Price}\n");
            sb.Append($"Discount: {DiscountPercentage}%\n");
            sb.Append($"Available To Rent: {IsAvailableToRent}");

            return sb.ToString();
        }

    }
}
