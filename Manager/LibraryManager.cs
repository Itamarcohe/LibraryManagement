using LibraryManagement.Helpers;
using LibraryManagement.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace LibraryManagement.Manager
{
    public class LibraryManager : INotifyPropertyChanged
    {


        private Dictionary<Guid, Item> _items;

        private ObservableCollection<Item> _currentUserRents = new ObservableCollection<Item>();

        private bool isLibrarianLoggedIn;

        private ObservableCollection<Item> _libraryItems = new ObservableCollection<Item>();

        private User loggedInUser;

        public bool IsLibrarianLoggedIn
        {
            get { return isLibrarianLoggedIn; }
            set
            {
                isLibrarianLoggedIn = value;
                OnPropertyChanged(nameof(IsLibrarianLoggedIn));
            }
        }

        public void Logout()
        {
            CurrentUserRents = new ObservableCollection<Item>();
            LoggedInUser = null;
            IsLibrarianLoggedIn = false;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Item> CurrentUserRents
        {
            get { return _currentUserRents; }
            set { _currentUserRents = value; }
        }

        public ObservableCollection<Item> LibraryItems
        {
            get { return _libraryItems; }
            set { _libraryItems = value; }
        }


        public Dictionary<string, List<Item>> userRents = new Dictionary<string, List<Item>>();

        internal List<User> _users = new List<User>();

        public int Count { get { return _items.Count; } }

        public MyCashBox LibraryCashBox;
        public User LoggedInUser
        {
            get { return loggedInUser; }
            set
            {
                loggedInUser = value;
                OnPropertyChanged(nameof(LoggedInUser));
                OnPropertyChanged(nameof(IsLibrarianLoggedIn));
            }
        }

        public LibraryManager()
        {
            _items = new Dictionary<Guid, Item>();
            _users = new List<User>();
            LibraryCashBox = new MyCashBox(1000.00);

        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void UpdateAvailability(Item item)
        {
            _items[item.ID] = item;
            UpdateUIChange();
        }

        public void UpdateUIChange()
        {
            LibraryItems.Clear();
            foreach (Item libraryItem in _items.Values)
            {
                LibraryItems.Add(libraryItem);
            }

        }

        public void UpdateItemInUI(Item updatedItem)
        {
            int index = LibraryItems.IndexOf(updatedItem);
            if (index >= 0)
            {
                LibraryItems[index] = updatedItem;
            }
        }


        public void updateItem(Item item)
        {
            _items[item.ID] = item;
        }


        public void RentsForLoggedInUser()
        {
            if (LoggedInUser != null)
            {
                if (userRents.TryGetValue(LoggedInUser.Name, out List<Item> rentedItems))
                {
                    CurrentUserRents = new ObservableCollection<Item>(rentedItems);

                }
            }

        }


        public void AddDiscount(string bookId, double discountPercentage)
        {
            if (IsLibrarianLoggedIn)
            {
                if (Guid.TryParse(bookId, out Guid guid))
                {
                    if (_items.ContainsKey(guid))
                    {
                        _items[guid].DiscountPercentage = discountPercentage;
                    }
                    else
                    {
                        Console.WriteLine("Not found");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid book ID format");
                }
            }
            else
            {
                Console.WriteLine("Only librarians can add discounts.");
            }
        }

        public User AddNewUser(string name, string password, UserType userType)
        {
            User user = new User(name, password, userType);
            _users.Add(user);
            return user;
        }


        public void AddNewUser(User user)
        {
            _users.Add(user);

        }

        public void GetAllItemsConsole()
        {
            foreach (Item item in _items.Values)
            {
                Console.WriteLine(item);
            }
        }

        public List<Item> GetAllItems()
        {
            return _items.Values.ToList();
        }

        public User GetUserByName(string name)
        {
            return _users.FirstOrDefault(user => user.Name == name);
        }


        public bool ValidateUser(string name, string password, UserType userType)
        {
            User user = GetUserByName(name);

            if (user != null && user.Password == password && user.UserType == userType)
            {
                LoggedInUser = user;
                return true;
            }
            return false;
        }



        public LoginResult ValidateUser(string name, string password)
        {
            User user = GetUserByName(name);

            if (user != null && user.Password == password)
            {
                LoggedInUser = user;
                return LoginResult.Success;
            }
            else if (user == null)
            {
                return LoginResult.UserNotFound;
            }
            else
            {
                return LoginResult.InvalidPassword;
            }
        }

        public bool ValidateLibarian() => LoggedInUser.UserType == UserType.Librarian;

        public bool UserExists(string name)
        {
            return _users.Any(user => user.Name == name);
        }

        public UserType GetUserType(string name)
        {
            User user = GetUserByName(name);
            return user?.UserType ?? UserType.User;
        }

        public void RemoveUser(string name)
        {
            User userToRemove = GetUserByName(name);
            if (userToRemove != null)
            {
                _users.Remove(userToRemove);
            }
        }


        public void AddBook(string author, string name, double price, DateTime publishDate, string publisher, Genre genre, double discountPrice)
        {
            Book book = new Book(author, name, price, publishDate, publisher, genre, discountPrice);
            _items[book.ID] = book;
            LibraryItems.Add(book);

        }


        public void AddMagazine(string name, double price, DateTime publishDate, string publisher, Genre genre, double discountPrice)
        {
            Magazine magazine = new Magazine(name, price, publishDate, publisher, genre, discountPrice);
            _items[magazine.ID] = magazine;
            LibraryItems.Add(magazine);
        }


        public ObservableCollection<Item> SearchItems(string title, string author, Genre? genre, double minPrice, double maxPrice, bool isAvailable)
        {

            var filteredItems = _items.Values.Where(item =>
            (string.IsNullOrEmpty(author) || (item is Book book && book.Author.Contains(author))) &&
            (string.IsNullOrEmpty(title) || (item.Name.Contains(title))) &&
            (!genre.HasValue || item.Genre == genre) &&
            (minPrice <= 0 || item.CurrentPrice >= minPrice) &&
            (maxPrice <= 0 || item.CurrentPrice <= maxPrice) &&
            (!isAvailable || (isAvailable && item.IsAvailableToRent))
            )
            .ToList();
            return new ObservableCollection<Item>(filteredItems);
        }


        public ObservableCollection<Item> ClearSearch()
        {
            return new ObservableCollection<Item>(_items.Values);
        }


        internal void SetItems(Dictionary<Guid, Item> dictionary)
        {
            _items = dictionary;
        }

        #region Create Defaults

        internal void DefaultUsers()
        {
            //Username //Password //Type
            User user = new User("Itamar", "Itamar", UserType.Librarian);
            AddNewUser(user);

            user = new User("Admin", "Admin", UserType.Librarian);
            AddNewUser(user);

            user = new User("RegUser", "RegUser", UserType.User);
            AddNewUser(user);

        }

        internal void DefaultBooks()
        {
            var random = new Random();

            for (int i = 0; i < 50; i++)
            {
                string[] bookNames = new string[]
                {
                    "Berlin Alexanderplatz", "Blindness", "The Book of Disquiet", "The Book of Job", "The Brothers Karamazov",
                    "Buddenbrooks", "Canterbury Tales", "The Castle", "Children of Gebelawi", "Collected Fictions",
                    "Complete Poems", "The Complete Stories", "The Complete Tales", "Confessions of Zeno", "Crime and Punishment",
                    "Dead Souls", "The Death of Ivan Ilyich and Other Stories", "Decameron", "The Devil to Pay in the Backlands",
                    "Diary of a Madman and Other Stories", "The Divine Comedy", "Don Quixote", "Essays", "Fairy Tales and Stories",
                    "Faust", "Gargantua and Pantagruel", "Gilgamesh", "The Golden Notebook", "Great Expectations", "Gulliver's Travels",
                    "Gypsy Ballads", "Hamlet", "History", "Hunger", "The Idiot", "The Iliad", "Independent People", "Invisible Man",
                    "Jacques the Fatalist and His Master", "Journey to the End of the Night", "King Lear", "Leaves of Grass",
                    "The Life and Opinions of Tristram Shandy", "Lolita", "Love in the Time of Cholera", "Madame Bovary",
                    "The Magic Mountain", "Mahabharata", "The Man Without Qualities", "The Mathnawi", "Medea", "Memoirs of Hadrian",
                    "Metamorphoses", "Middlemarch", "Midnight's Children", "Moby-Dick", "Mrs. Dalloway", "Njaals Saga",
                    "Nostromo", "The Odyssey", "Oedipus the King", "Old Goriot", "The Old Man and the Sea", "One Hundred Years of Solitude",
                    "The Orchard", "Othello", "Pedro Paramo", "Pippi Longstocking", "Poems", "The Possessed", "Pride and Prejudice",
                    "The Ramayana", "The Recognition of Sakuntala", "The Red and the Black", "Remembrance of Things Past",
                    "Season of Migration to the North", "Selected Stories", "Sons and Lovers", "The Sound and the Fury",
                    "The Sound of the Mountain", "The Stranger", "The Tale of Genji", "Things Fall Apart", "Thousand and One Nights",
                    "The Tin Drum"
                };

                string[] authors = new string[]
                {
                    "Alfred Doblin", "Jose Saramago", "Fernando Pessoa", "Unknown", "Fyodor M Dostoyevsky", "Thomas Mann",
                    "Geoffrey Chaucer", "Franz Kafka", "Naguib Mahfouz", "Jorge Luis Borges", "Giacomo Leopardi", "Edgar Allan Poe",
                    "Italo Svevo", "Nikolai Gogol", "Leo Tolstoy", "Giovanni Boccaccio", "Joao Guimaraes Rosa", "Lu Xun", "Dante Alighieri",
                    "Miguel de Cervantes Saavedra", "Michel de Montaigne", "Hans Christian Andersen", "Johann Wolfgang von Goethe",
                    "Francois Rabelais", "Unknown", "Doris Lessing", "Charles Dickens", "Jonathan Swift", "Federico Garcia Lorca",
                    "William Shakespeare", "Elsa Morante", "Knut Hamsun", "Fyodor M Dostoyevsky", "Homer", "Halldor K Laxness",
                    "Ralph Ellison", "Denis Diderot", "Louis-Ferdinand Celine", "William Shakespeare", "Walt Whitman", "Laurence Sterne",
                    "Vladimir Nabokov", "Gabriel Garcia Marquez", "Gustave Flaubert", "Thomas Mann", "Unknown", "Joseph Conrad",
                    "Homer", "Sophocles", "Honore de Balzac", "Ernest Hemingway", "Gabriel Garcia Marquez", "Sheikh Musharrif ud-din Sadi",

                };

                string name = bookNames[i];
                string author = authors[i];
                double price = random.Next(50, 201) + random.NextDouble();
                DateTime publishDate = new DateTime(random.Next(600, 2023), 1, 1);
                string publisher = $"Publisher {i}";


                Func<Genre> getRandomGenre = () =>
                {
                    Genre[] genres = (Genre[])Enum.GetValues(typeof(Genre));
                    return genres[random.Next(0, genres.Length)];
                };

                Genre genre = getRandomGenre();

                double discountPrice = random.Next(0, 30);

                AddBook(author, name, price, publishDate, publisher, genre, discountPrice);
            }

        }
        #endregion

    }
}


