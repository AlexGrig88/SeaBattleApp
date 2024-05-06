using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleApp.Models
{
    internal class Player : User<int>
    {
        public int Score { get; set; }      // за сколько ходов уничтожен корабль
        public int VictoryCounter { get; set; }
        public int DefeatCounter { get; set; }

        public Player(int id, string username, string email = "test@yandex.ru", string password = "123") : base(id, username, email, password)
        {
            Score = 0;
            VictoryCounter = 0;
            DefeatCounter = 0;
        }
    }
}
