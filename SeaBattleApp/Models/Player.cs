using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleApp.Models
{
    public class Player : User<int>
    {
        public int Score { get; set; }      // Наименьшее колличество выстрелов, за которое была унижтожена вражеская флотилия
        public int VictoryCounter { get; set; }
        public int DefeatCounter { get; set; }
        public int CurrentScoreCounter { get; set; }

        public Player(int id, string username, string email = "noname@yandex.ru", string password = "123") : base(id, username, email, password)
        {
            Score = 0;
            VictoryCounter = 0;
            DefeatCounter = 0;
            CurrentScoreCounter = 0;
        }
        
    }
 
}
