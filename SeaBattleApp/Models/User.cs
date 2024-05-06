using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleApp.Models
{
    internal class User<T>
    {
        public T Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public User(T id, string username, string email = "test@yandex.ru", string password = "123")
        {
            Id = id;
            Username = username;
            Email = email;
            Password = password;
        }
    }
}
