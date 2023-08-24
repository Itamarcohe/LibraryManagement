using LibraryManagement.Helpers;
using LibraryManagement.Helpers.UIMessages;
using LibraryManagement.Manager;
using LibraryManagement.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using User = LibraryManagement.Models.User;
using UserType = LibraryManagement.Models.UserType;
using static LibraryManagement.Helpers.MyUtils;
using System.Linq;
using LibraryManagement.Helpers.Serializers;
using Windows.UI.Xaml.Documents;

namespace LibraryManagement
{

    public sealed partial class MainPage : Page

    {

        private readonly MessageDialog errorMessageDialog = new MessageDialog(string.Empty, "Error");
        private readonly MessageDialog successMessageDialog = new MessageDialog(string.Empty, "Success");

        public LibraryManager libraryManager;

        private Item editedItem;


        #region Initialize MainPage
        public MainPage() //First I enter here
        {
            this.InitializeComponent();
        }


        protected override void OnNavigatedTo(NavigationEventArgs e) // Then here
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is LibraryManager paramLibraryManager)
            {
                libraryManager = paramLibraryManager;

                DataContext = libraryManager;

                if (libraryManager._users.Count <= 0)
                {
                    libraryManager.DefaultUsers();
                }

                if (libraryManager.Count <= 0)
                {
                    libraryManager.DefaultBooks();
                }

                this.Loaded += MainPage_Loaded;

            }
        }


        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            DateTimeOffset currentDate = new DateTimeOffset(DateTime.Now.Year, 1, 1, 0, 0, 0, TimeSpan.Zero);
            AddItemPublishDate.MaxYear = currentDate;
            LibraryItemsControl.ItemsSource = libraryManager.LibraryItems;

            ComboBoxItem searchComboBoxItemDefault = new ComboBoxItem() { Content = "Genre" };
            SearchGenreComboBox.Items.Add(searchComboBoxItemDefault);
            SearchGenreComboBox.SelectedIndex = 0;


            foreach (Genre genre in Enum.GetValues(typeof(Genre)))
            {
                ComboBoxItem searchComboBoxItem = new ComboBoxItem();
                searchComboBoxItem.Content = genre.ToString();
                SearchGenreComboBox.Items.Add(searchComboBoxItem);

                ComboBoxItem addBookComboBoxItem = new ComboBoxItem();
                addBookComboBoxItem.Content = genre.ToString();
                AddBookGenreComboBox.Items.Add(addBookComboBoxItem);
            }
        }

        #endregion

        private async Task ShowErrorMessage(string message)
        {
            errorMessageDialog.Content = message;
            await errorMessageDialog.ShowAsync();
        }

        private async Task ShowSuccessMessage(string message)
        {
            successMessageDialog.Content = message;
            await successMessageDialog.ShowAsync();
        }

        internal async Task SaveDataBeforeAppClose()
        {
            await LibraryDataSerializer.SaveDataAsync(libraryManager);
        }


        #region Rents & Return item user/libarian
        private void ShowDialogButton_Click(object sender, RoutedEventArgs e)
        {
            ShowContentDialog(libraryManager.CurrentUserRents);
        }
        private void BookReturned(Item ReturnedItem, StackPanel stackPanel)
        {

            for (int i = stackPanel.Children.Count - 2; i >= 0; i -= 2)
            {
                if (stackPanel.Children[i] is TextBlock textBlock &&
                    textBlock.Text.EndsWith($", {ReturnedItem.RentDate},  {ReturnedItem.ReturnDate}"))
                {
                    stackPanel.Children.RemoveAt(i + 1); // Remove the Button 
                    stackPanel.Children.RemoveAt(i); // Remove the TextBlock To Update Remove in live
                }
            }
        }

        private async void ShowAllRents_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "All Rents",
                PrimaryButtonText = "Close"
            };

            StackPanel stackPanel = new StackPanel();


            if (libraryManager.userRents != null && libraryManager.userRents.Count > 0)
            {


                ObservableCollection<Item> allRents = new ObservableCollection<Item>(
                    libraryManager.userRents.Values.SelectMany(itemList => itemList)
                );

                foreach (Item item in allRents)
                {
                    TextBlock nameTextBlock = new TextBlock
                    {
                        Text = $"Renter: {item.Renter.Name} \n {item.Name}, {item.RentDate},  {item.ReturnDate}",
                        FontWeight = FontWeights.Bold,
                        Foreground = new SolidColorBrush(Windows.UI.Colors.Black),
                        Margin = new Thickness(10)
                    };

                    if (item.isLateOnReturn())
                    {
                        nameTextBlock.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);

                    }

                    stackPanel.Children.Add(nameTextBlock);

                    Button returnButton = new Button
                    {
                        Content = "Return",
                        Margin = new Thickness(10)
                    };

                    returnButton.Click += (btnSender, eventArgs) =>
                    {
                        string userName = item.Renter.Name;

                        BookReturned(item, stackPanel);
                        item.RentDate = DateTime.MinValue;
                        item.ReturnDate = DateTime.MinValue;
                        item.Renter = null;
                        item.IsAvailableToRent = true;

                        libraryManager.updateItem(item);
                        libraryManager.UpdateItemInUI(item);

                        libraryManager.userRents[userName].Remove(item);
                        allRents = new ObservableCollection<Item>(
                        libraryManager.userRents.Values.SelectMany(itemList => itemList)
     );
                        if (libraryManager.LoggedInUser.Name == userName)
                        {
                            libraryManager.RentsForLoggedInUser();
                        }


                    };

                    stackPanel.Children.Add(returnButton);

                }
            }
            else
            {
                TextBlock nameTextBlock = new TextBlock
                {
                    Text = $"There is no rents currently in the library",
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Windows.UI.Colors.Black),
                    Margin = new Thickness(10)
                };
                stackPanel.Children.Add(nameTextBlock);
            }

            ScrollViewer scrollViewer = new ScrollViewer
            {
                Content = stackPanel,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                MaxHeight = Window.Current.Bounds.Height * 0.6
            };
            dialog.Content = scrollViewer;
            await dialog.ShowAsync();
        }

        private async void ShowContentDialog(ObservableCollection<Item> currentUserRents)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "My Rents",
                PrimaryButtonText = "Close"
            };

            StackPanel stackPanel = new StackPanel();

            if (currentUserRents != null && currentUserRents.Count > 0)
            {

                foreach (Item item in currentUserRents)
                {
                    TextBlock nameTextBlock = new TextBlock
                    {
                        Text = $"{item.Name}, {item.RentDate},  {item.ReturnDate}",
                        FontWeight = FontWeights.Bold,
                        Foreground = new SolidColorBrush(Windows.UI.Colors.Black),
                        Margin = new Thickness(10)
                    };

                    if (item.isLateOnReturn())
                    {
                        nameTextBlock.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);

                    }

                    stackPanel.Children.Add(nameTextBlock);

                    Button returnButton = new Button
                    {
                        Content = "Return",
                        Margin = new Thickness(10)
                    };

                    returnButton.Click += (sender, e) =>
                    {

                        BookReturned(item, stackPanel);
                        item.RentDate = DateTime.MinValue;
                        item.ReturnDate = DateTime.MinValue;
                        item.Renter = null;
                        item.IsAvailableToRent = true;

                        libraryManager.updateItem(item);
                        libraryManager.UpdateItemInUI(item);

                        libraryManager.userRents[libraryManager.LoggedInUser.Name].Remove(item);
                        libraryManager.RentsForLoggedInUser();

                    };

                    stackPanel.Children.Add(returnButton);

                }
            }
            else
            {
                TextBlock nameTextBlock = new TextBlock
                {
                    Text = $"You currently dont have any rents in the library",
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Windows.UI.Colors.Black),
                    Margin = new Thickness(10)
                };
                stackPanel.Children.Add(nameTextBlock);
            }

            ScrollViewer scrollViewer = new ScrollViewer
            {
                Content = stackPanel,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                MaxHeight = Window.Current.Bounds.Height * 0.6
            };
            dialog.Content = scrollViewer;
            await dialog.ShowAsync();
        }
        #endregion

        #region Login
        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = NameTextBox.Text;
            string password = PasswordTextBox.Password;

            if (AreStringsNullOrEmpty(username, password))
            {
                await ShowErrorMessage(ErrorMessages.FillAllFields);
                return;

            }

            LoginResult result = libraryManager.ValidateUser(username, password);

            if (result == LoginResult.Success)
            {
                LoginGrid.Visibility = Visibility.Collapsed;
                LibraryData.Visibility = Visibility.Visible;
                SearchFormGrid.Visibility = Visibility.Visible;
                BackToLoginFromData.Visibility = Visibility.Visible;
                ShowMyRentsButton.Visibility = Visibility.Visible;
                LabelsGrid.Visibility = Visibility.Visible;

                libraryManager.RentsForLoggedInUser();

                if (libraryManager.ValidateLibarian())
                {
                    ShowLibarianUI();
                }
            }

            else if (result == LoginResult.UserNotFound)
            {
                await ShowErrorMessage(ErrorMessages.UserDoesNotExist);
                return;

            }
            else if (result == LoginResult.InvalidPassword)
            {
                await ShowErrorMessage(ErrorMessages.WrongPassword);
            }

        }
        #endregion

        #region Sign Up
        private async void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (SignUpGrid.Visibility == Visibility.Collapsed)
            {
                LoginGrid.Visibility = Visibility.Collapsed;
                SignUpGrid.Visibility = Visibility.Visible;
            }
            else
            {
                string name = NewNameTextBox.Text;
                string password = NewPasswordTextBox.Password;
                string confirmPassword = ConfirmPasswordTextBox.Password;


                if (MyUtils.AreStringsNullOrEmpty(name, password, confirmPassword))
                {
                    await ShowErrorMessage(ErrorMessages.FillAllFields);
                    return;
                }


                if (password != confirmPassword)
                {
                    await ShowErrorMessage(ErrorMessages.PasswordsDoNotMatch);
                    return;
                }


                if (libraryManager.GetUserByName(name) != null)
                {
                    await ShowErrorMessage(ErrorMessages.UserAlreadyExists);
                    return;
                }


                User signedUpUser = libraryManager.AddNewUser(name, password, UserType.User);
                libraryManager.LoggedInUser = signedUpUser;

                await ShowSuccessMessage(SuccessMessages.FormatMessage(SuccessMessages.SignedUp, name));

                LoginGrid.Visibility = Visibility.Collapsed;
                LibraryData.Visibility = Visibility.Visible;
                SearchFormGrid.Visibility = Visibility.Visible;
                BackToLoginFromData.Visibility = Visibility.Visible;
                ShowMyRentsButton.Visibility = Visibility.Visible;
                LabelsGrid.Visibility = Visibility.Visible;


                NewNameTextBox.Text = string.Empty;
                NewPasswordTextBox.Password = string.Empty;
                ConfirmPasswordTextBox.Password = string.Empty;

                SignUpGrid.Visibility = Visibility.Collapsed;

            }
        }
        #endregion

        #region Back To Login
        private void BackToLoginButton_Click(object sender, RoutedEventArgs e)
        {
            SignUpGrid.Visibility = Visibility.Collapsed;
            ShowMyRentsButton.Visibility = Visibility.Collapsed;
            CloseGuestUI();
            CloseLibarianUI();
            libraryManager.Logout();
            ClearUISearch();
            ResetSearchInputs();

        }
        #endregion

        #region Guest Login
        private void GuestLoginButton_Click(object sender, RoutedEventArgs e)
        {
            ShowGuestUI();
        }
        #endregion


        #region Search Item
        private void ResetSearchInputs()
        {
            AuthorTextBox.Text = string.Empty;         
            TitleTextBox.Text = string.Empty;
            SearchGenreComboBox.SelectedItem = SearchGenreComboBox.Items[0];
            MinPriceTextBox.Text = string.Empty;       
            MaxPriceTextBox.Text = string.Empty;      
        }

        public void ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            ResetSearchInputs();
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string author = AuthorTextBox.Text;

            string title = TitleTextBox.Text;


            bool hasMinPrice = double.TryParse(MinPriceTextBox.Text, out double minPrice);
            bool hasMaxPrice = double.TryParse(MaxPriceTextBox.Text, out double maxPrice);

            if (!hasMinPrice || !hasMaxPrice)
            {
                if (!hasMinPrice && !string.IsNullOrEmpty(MinPriceTextBox.Text))
                {

                    await ShowErrorMessage(ErrorMessages.MinMaxPrices);
                    return;

                }

                if (!hasMaxPrice && !string.IsNullOrEmpty(MaxPriceTextBox.Text))
                {
                    await ShowErrorMessage(ErrorMessages.MinMaxPrices);
                    return;

                }

            }

            if (hasMinPrice && hasMaxPrice && minPrice > maxPrice)
            {
                await ShowErrorMessage(ErrorMessages.MinPriceGreaterThanMaxPrice);
                return;
            }

            if (hasMinPrice && minPrice < 1)
            {
                await ShowErrorMessage(ErrorMessages.MinMaxPriceLessThanOne);
                return;
            }
            if (hasMaxPrice && maxPrice < 1)
            {
                await ShowErrorMessage(ErrorMessages.MinMaxPriceLessThanOne);
                return;
            }

            Genre? genre = null;

            if (SearchGenreComboBox.SelectedItem is ComboBoxItem selectedGenreItem && SearchGenreComboBox.SelectedIndex != 0)
            {
                if (Enum.TryParse(selectedGenreItem.Content.ToString(), out Genre parsedGenre))
                {
                    genre = parsedGenre;
                }
            }

            bool isAvailable = IsAvailableCB.IsChecked ?? false;

            ObservableCollection<Item> searchResults = libraryManager.SearchItems(title, author, genre, minPrice, maxPrice, isAvailable);
            LibraryItemsControl.ItemsSource = searchResults;
        }

        public void ClearUISearch()
        {
            LibraryItemsControl.ItemsSource = libraryManager.ClearSearch();
        }

        #endregion

        #region Add Item (Book / Magazine Handled)
        private async void AddItemButton_Clicked(object sender, RoutedEventArgs e)
        {
            string author = AddBookAuthor.Text;
            string name = AddItemName.Text;

            if (AreStringsNullOrEmpty(name, AddItemPrice.Text, AddItemPublisher.Text, AddItemDiscount.Text)
                || AreObjectsNull(AddBookGenreComboBox.SelectedItem))
            {
                await ShowErrorMessage(ErrorMessages.FillAllFields);
                return;
            }

            if (!double.TryParse(AddItemPrice.Text, out double price) || price <= 0)
            {
                await ShowErrorMessage(ErrorMessages.GreaterThanZero);
                return;
            }


            if (!double.TryParse(AddItemDiscount.Text, out double discountPrice))
            {
                await ShowErrorMessage(ErrorMessages.DiscountPriceOutOfRange);
                return;
            }


            if (discountPrice < 0 || discountPrice > 100)
            {
                await ShowErrorMessage(ErrorMessages.DiscountPriceOutOfRange);
                return;
            }

            DateTime publishDate = AddItemPublishDate.Date.DateTime;


            if (publishDate > DateTime.Now)
            {
                await ShowErrorMessage(ErrorMessages.FutureDateTime);
                return;
            }


            if (AddBookGenreComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                Genre selectedGenre;
                if (!Enum.TryParse(selectedItem.Content.ToString(), out selectedGenre))
                {
                    await ShowErrorMessage(ErrorMessages.GenreMissing);
                    return;
                }

                if (BookRadioButton.IsChecked == true)
                {
                    if (string.IsNullOrEmpty(author))
                    {
                        await ShowErrorMessage(ErrorMessages.AuthorMissing);
                        return;
                    }
                    libraryManager.AddBook(author, name, price, publishDate, AddItemPublisher.Text, selectedGenre, discountPrice);
                }
                else
                {
                    libraryManager.AddMagazine(name, price, publishDate, AddItemPublisher.Text, selectedGenre, discountPrice);

                }

                LibraryItemsControl.ItemsSource = libraryManager.LibraryItems;
                AddBookAuthor.Text = "";
                AddItemName.Text = "";
                AddItemPrice.Text = "";
                AddItemPublisher.Text = "";
                AddItemDiscount.Text = "";
                AddItemPublishDate.Date = DateTimeOffset.Now;
                AddBookGenreComboBox.SelectedIndex = -1;

            }
        }

        private void BookRadioButton_Click(object sender, RoutedEventArgs e)
        {
            AddBookAuthor.Visibility = Visibility.Visible;
        }
        private void MagazineRadioButton_Click(object sender, RoutedEventArgs e)
        {
            AddBookAuthor.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region Rent Item 
        private async void RentButton_Click(object sender, RoutedEventArgs e)
        {
            User currentUser = libraryManager.LoggedInUser;

            if (currentUser == null)
            {
                await ShowErrorMessage(ErrorMessages.LoginRequired);
                return;
            }

            var button = (Button)sender;
            var item = (Item)button.DataContext;

            if (item.IsAvailableToRent == false)
            {
                await ShowErrorMessage(ErrorMessages.ItemNotAvailableUntil(item.ReturnDate));
                return;
            }

            item.IsAvailableToRent = false;
            item.Renter = libraryManager.LoggedInUser;
            item.RentDate = DateTime.Now;
            item.ReturnDate = item.RentDate.AddDays(14);

            // Uncomment to create late on return rents 
            //item.ReturnDate = item.RentDate.AddDays(-25);




            if (libraryManager.userRents.ContainsKey(currentUser.Name))
            {
                libraryManager.userRents[currentUser.Name].Add(item);
            }
            else
            {
                libraryManager.userRents[currentUser.Name] = new List<Item> { item };
            }


            if (libraryManager.CurrentUserRents == null)
            {
                libraryManager.CurrentUserRents = new ObservableCollection<Item> { item };
            }
            else
            {
                libraryManager.CurrentUserRents.Add(item);
            }

            libraryManager.LibraryCashBox.AddCash(item.CurrentPrice);


            await ShowSuccessMessage(SuccessMessages.FormatMessage(SuccessMessages.ItemRented,
                currentUser.Name,
                item.Name,
                item.ReturnDate));


        }
        #endregion

        private async void DetailsButton_Clicked(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var item = (Item)button.DataContext;

            StringBuilder sb = new StringBuilder();

            if (item is Book book)
            {
                sb.Append($"Author: {book.Author} \n");
            }


            sb.Append(item.ToString());

            ContentDialog dialog = new ContentDialog
            {
                Title = "Details",
                PrimaryButtonText = "Close",
                Content = new TextBlock
                {
                    Text = sb.ToString(),
                    Foreground = new SolidColorBrush(Windows.UI.Colors.Black),
                    TextWrapping = TextWrapping.WrapWholeWords,
                    TextAlignment = TextAlignment.Justify,
                    Margin = new Thickness(10),
                    FontSize = 18
                },
            };

            await dialog.ShowAsync();
        }


        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {

            var button = (Button)sender;
            var item = (Item)button.DataContext;
            editedItem = item;


            EditPrice.PlaceholderText = item.Price.ToString();
            EditDiscount.PlaceholderText = item.DiscountPercentage.ToString();

            await EditItemDialog.ShowAsync();
        }

        private void CloseEdit_Click(object sender, RoutedEventArgs e)
        {
            EditDiscount.Text = string.Empty;
            EditPrice.Text = string.Empty;
            EditItemDialog.Hide();

        }

        private async void ConfirmEdit_Click(object sender, RoutedEventArgs e)
        {


            if (EditPrice.Text.Length > 0)
            {
                if (double.TryParse(EditPrice.Text, out double newPrice))
                {
                    if (newPrice < 0)
                    {
                        await ShowErrorMessage(ErrorMessages.GreaterThanZero);
                        return;
                    }
                    editedItem.UpdatePrice(newPrice);
                }
                else
                {
                    await ShowErrorMessage(ErrorMessages.DigitsOnly);
                    return;
                }
            }

            if (EditDiscount.Text.Length > 0)
            {
                if (double.TryParse(EditDiscount.Text, out double newDiscount))
                {
                    try
                    {
                        editedItem.UpdateDiscount(newDiscount);
                    }
                    catch
                    {
                        await ShowErrorMessage(ErrorMessages.DiscountPriceOutOfRange);
                        return;
                    }
                }
                else
                {
                    await ShowErrorMessage(ErrorMessages.DigitsOnly);
                    return;
                }

            }

            libraryManager.UpdateItemInUI(editedItem);
            await ShowSuccessMessage(SuccessMessages.FormatMessage(SuccessMessages.ItemEdited,
                libraryManager.LoggedInUser.Name, editedItem.Price, editedItem.CurrentPrice, editedItem.DiscountPercentage));

        }

        #region UI Visibility
        private void ShowLibarianUI()
        {
            libraryManager.IsLibrarianLoggedIn = true;
            LibraryData.Margin = new Thickness(239, 200, -60, 50);
            LibarianOptions.Visibility = Visibility.Visible;
            CashBox.Visibility = Visibility.Visible;
            ShowAllRentsButton.Visibility = Visibility.Visible;
        }

        private void CloseLibarianUI()
        {
            LibarianOptions.Visibility = Visibility.Collapsed;
            CashBox.Visibility = Visibility.Collapsed;
            LibraryData.Margin = new Thickness(239, 200, 50, 50);
            ShowAllRentsButton.Visibility = Visibility.Collapsed;
        }

        private void ShowGuestUI()
        {
            LoginGrid.Visibility = Visibility.Collapsed;
            LibraryData.Visibility = Visibility.Visible;
            SearchFormGrid.Visibility = Visibility.Visible;
            BackToLoginFromData.Visibility = Visibility.Visible;
            LabelsGrid.Visibility = Visibility.Visible;
        }

        private void CloseGuestUI()
        {
            LoginGrid.Visibility = Visibility.Visible;
            LibraryData.Visibility = Visibility.Collapsed;
            SearchFormGrid.Visibility = Visibility.Collapsed;
            BackToLoginFromData.Visibility = Visibility.Collapsed;
            LabelsGrid.Visibility = Visibility.Collapsed;
        }

        #endregion

    }
}
