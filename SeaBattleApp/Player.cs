using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleApp
{
    public class Player
    {
        public bool IsComputer { get; set; }
        public string Name { get; set; }

        public Player(string name, bool isComputer = false)
        {
            Name = name;
            IsComputer = isComputer;
        }
    }
}
