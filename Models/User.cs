namespace LibraryManagement.Models
{
    public class User
    {
        public string Name { get; set; }
        public string Password { get; set; }

        public UserType UserType;

        public User(string name, string password, UserType userType)
        {
            Name = name;
            Password = password;
            UserType = userType;            
        }

        public User()
        {
        }

    }
}
