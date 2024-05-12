using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleApp.Models
{
    public class User<T>
    {
        public T Id { get; set; }
        public string Username { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public User(T id, string username, string email = "test@yandex.ru", string password = "123")
        {
            Id = id;
            Username = username;
            Email = email;
            Password = password;
        }
        public User(T id, string username, string firstName = "NoName", string lastName = "NoName", string email = "test@yandex.ru", string password = "123")
        {
            Id = id;
            Username = username;
            FirstName = firstName;
            LastName = lastName;  
            Email = email;
            Password = password;
        }
    }
}
