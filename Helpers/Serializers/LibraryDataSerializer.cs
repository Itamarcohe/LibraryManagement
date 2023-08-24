using LibraryManagement.Manager;
using LibraryManagement.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;

namespace LibraryManagement.Helpers.Serializers
{
    internal static class LibraryDataSerializer
    {
        private const string FileName = "library_data.json";

        public static async Task SaveDataAsync(LibraryManager libraryManager)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Converters = { new UserConverter(), new ItemConverter(), new DateTimeConverter(),
                        new LibraryCashBoxConverter(), new UserRentsConverter()  }
                };
                var dataToSerialize = new Dictionary<string, object>

                {
                    { "_items", libraryManager.GetAllItems() },
                    { "_users", libraryManager._users },
                    { "LibraryCashBox", libraryManager.LibraryCashBox },
                    { "UserRents", libraryManager.userRents }
        };

                string jsonData = JsonSerializer.Serialize(dataToSerialize, options);
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                StorageFile file = await localFolder.CreateFileAsync(FileName, CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(file, jsonData);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }


        public static async Task LoadDataAsync(LibraryManager libraryManager)
        {
            try
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;

                StorageFile file = await localFolder.GetFileAsync(FileName);

                string jsonData = await FileIO.ReadTextAsync(file);

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Converters = { new UserConverter(), new DateTimeConverter(), new LibraryCashBoxConverter(), new UserRentsConverter(), new ItemConverter() }
                };

                var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonData, options);

                foreach (var kvp in data)
                {
                    switch (kvp.Key)
                    {
                        case "_items":
                            var itemsData = kvp.Value;

                            var items = new Dictionary<Guid, Item>();
                            var itemList = new List<Item>(); 

                            foreach (var itemJson in itemsData.EnumerateArray())
                            {
                                Item item = JsonSerializer.Deserialize<Item>(itemJson.GetRawText(), options);
                                items.Add(item.ID, item);
                                itemList.Add(item);
                            }

                            libraryManager.SetItems(items);
                            libraryManager.LibraryItems = new ObservableCollection<Item>(itemList); 

                            break;
                        case "_users":

                            List<User> users = JsonSerializer.Deserialize<List<User>>(kvp.Value.GetRawText(), options);

                            libraryManager._users = users;
                            break;
                        case "LibraryCashBox":
                            MyCashBox libraryCashBox = JsonSerializer.Deserialize<MyCashBox>(kvp.Value.GetRawText(), options);

                            libraryManager.LibraryCashBox = libraryCashBox;
                            break;
                        case "UserRents":

                            var userRentsData = kvp.Value;

                            var userRents = new Dictionary<string, List<Item>>();

                            foreach (var userRentEntry in userRentsData.EnumerateObject())
                            {
                                string userName = userRentEntry.Name;
                                var rentedItems = new List<Item>();

                                foreach (var itemJson in userRentEntry.Value.EnumerateArray())
                                {
                                    Item rentedItem = JsonSerializer.Deserialize<Item>(itemJson.GetRawText(), options);
                                    rentedItems.Add(rentedItem);
                                }

                                userRents.Add(userName, rentedItems);
                            }
                            libraryManager.userRents = userRents;
                            break;
                        default:
                            break;
                    }
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

    }
}
